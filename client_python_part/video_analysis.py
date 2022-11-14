from face_recognition import face_encodings, compare_faces
from keras_preprocessing.image import img_to_array
import numpy as np
import cv2
import dlib
from imutils import face_utils
import math
from config_data import *
import os

basePath = (os.path.abspath(os.path.join(os.path.abspath(__file__), os.pardir)))
predictor = dlib.shape_predictor(basePath+'\shape_predictor_68_face_landmarks.dat')
detector = dlib.get_frontal_face_detector()


########################################################################################################################
def video_analysis(fa_path, frame_gap, emotion_model, s):
    face_encode_dict = {}
    video = cv2.VideoCapture(fa_path)
    fps = video.get(cv2.CAP_PROP_FPS)
    frame_count = int(video.get(cv2.CAP_PROP_FRAME_COUNT))

    f = open(fa_path + '.txt', 'w')
    s.send(bytes(f"@START@", "utf8"))
    f.write(f'@START@{fps}#{frame_count}#{frame_gap}\n')
    f.close()

    my_frame_number = 0
    my_frame_number_max = frame_count // frame_gap
    count = 0
    while my_frame_number < frame_count:
        if video.set(cv2.CAP_PROP_POS_FRAMES, my_frame_number):
            ret, frame = video.read()

            f = open(fa_path + '.txt', 'a')
            face_emotion_detection_dlib(frame, my_frame_number, emotion_model, face_encode_dict, f, fa_path)
            f.close()
        progress = count / my_frame_number_max
        count += 1
        s.send(bytes(f"@PROGRESS@{progress}", "utf8"))
        my_frame_number += frame_gap

    s.send(bytes(f"@END@{fa_path + '.txt'}", "utf8"))
    f = open(fa_path + '.txt', 'a')
    f.write(f"@END@\n")
    f.close()


########################################################################################################################


def image_is_blur(image):
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    fm = cv2.Laplacian(gray, cv2.CV_64F).var()
    blur = False

    if float(fm) < blur_th:
        blur = True

    return blur


########################################################################################################################
def face_emotion_detection_dlib(video_cap, my_frame_number, emotion_model, face_encode_dict, f, fa_path_split):
    images, images_recognition, positions = detection_dlib(video_cap)

    frame_data = f"@FRAMEDATA@{my_frame_number}"
    for i in range(0, len(images)):
        if images[i].size / 3 < 10000:
            continue
        # if detection_radian(images[i]):
        #     continue
        # if image_is_blur(images[i]):
        #     continue

        face_definer = FaceDefine(face_encode_dict, emotion_model, fa_path_split)
        face_code = face_definer.face_recognition(images_recognition[i])
        if face_code == -1:
            continue
        emotion_code, prediction = face_definer.emotion_detection(images[i])
        position = positions[i]
        x1 = position[0][0]
        y1 = position[0][1]
        x2 = position[1][0]
        y2 = position[1][1]
        frame_data += f'${face_code}^{prediction[0]}#{prediction[1]}#{prediction[2]}#{prediction[3]}#{prediction[4]}#' \
                      f'{prediction[5]}#{prediction[6]}^{x1}#{y1}#{x2}#{y2}'
# $face_code ^ anger # disgust #  fear # happy # neutral # sad # surprise ^ x1 # y1 # x2 # y2
    f.write(frame_data + '\n')


########################################################################################################################
class FaceDefine:

    def __init__(self, face_encode_dict, loaded_model, fa_path_split):
        self.face_encode_dict = face_encode_dict
        self.loaded_model = loaded_model
        self.fa_path_split = fa_path_split

    def face_recognition(self, image):  # original image
        try:
            image_to_define = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
        except Exception as e:
            print(e)
            image_to_define = image.copy()
        encode_list = face_encodings(image_to_define)
        if encode_list == []:
            return -1
        image_encode = encode_list[0]

        for face_code, encode in self.face_encode_dict.items():
            if_same_face = compare_faces([encode], image_encode, 0.6)
            if if_same_face[0]:
                return face_code
        new_face_code = len(self.face_encode_dict)
        self.face_encode_dict[new_face_code] = image_encode
        cv2.imwrite(rf"{self.fa_path_split}_{new_face_code}.png", image)
        return new_face_code

    def emotion_detection(self, image):
        image = cv2.resize(image, (48, 48))
        image_to_define = img_to_array(image)
        image_to_define = cv2.cvtColor(image_to_define, cv2.COLOR_RGB2GRAY)
        image_to_define = image_to_define.reshape((1, 48, 48))
        image_to_define = image_to_define.astype('float32') / 255.
        prediction = self.loaded_model.predict(image_to_define)[0]

        max_index = np.argmax(prediction[0])
        return max_index, prediction


########################################################################################################################
def detection_dlib(frame):
    try:
        frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    except Exception as e:
        print(e)
        frame_rgb = frame.copy()
    gray = cv2.cvtColor(frame_rgb, cv2.COLOR_RGB2GRAY)
    face_detect = dlib.get_frontal_face_detector()
    rectangles = face_detect(gray, 1)
    face_positions = []
    images_recognition = []
    images = []
    for i, rect in enumerate(rectangles):
        (x, y, w, h) = face_utils.rect_to_bb(rect)
        face_image = frame[y:(y + h), x:(x + w)]
        face_image_re = frame[int(y - 0.2 * h):int(y + 1.2 * h), int(x - 0.2 * w):int(x + 1.2 * w)]
        images.append(face_image)
        face_positions.append([[x, y], [x + w, y + h]])
        images_recognition.append(face_image_re)

    return images, images_recognition, face_positions


##################################### 이미지 각도 계산 함수 ###############################################################
def detection_radian(image_radian):
    try:
        dets = detector(image_radian, 1)
    except:
        return True

    for k, d in enumerate(dets):
        shape = predictor(image_radian, d)

        num_of_points_out = 17
        num_of_points_in = shape.num_parts - num_of_points_out
        gx_out = 0
        gy_out = 0
        gx_in = 0
        gy_in = 0

        for i in range(shape.num_parts):
            shape_point = shape.part(i)

            if i < num_of_points_out:
                gx_out = gx_out + shape_point.x / num_of_points_out
                gy_out = gy_out + shape_point.y / num_of_points_out
            else:
                gx_in = gx_in + shape_point.x / num_of_points_in
                gy_in = gy_in + shape_point.y / num_of_points_in
        theta = math.asin(2 * (gx_in - gx_out) / (d.right() - d.left()))
        radian = theta * 180 / math.pi
        if abs(radian) < radian_th:
            return False
        else:
            return True

########################################################################################################################
