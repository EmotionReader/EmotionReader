import os
import time
import cv2
import mediapipe as mp
from keras_preprocessing.image import img_to_array
import numpy as np
from config_data import EMOTIONS, server_ip, server_port
from threading import Thread
from live_socket import LiveSocket


def change_fps(file_path, fps):
    vid_capture = cv2.VideoCapture(file_path + r'\Live__.mp4')
    video_out = cv2.VideoWriter(file_path + r'\Live.mp4', cv2.VideoWriter_fourcc(*'FMP4'), fps, (640, 480))
    ret = True
    while ret:
        ret, frame = vid_capture.read()
        video_out.write(frame)
    vid_capture.release()
    video_out.release()
    os.remove(file_path + r'\Live__.mp4')
    print('video_write_end')


def emotion_analysis(emotion_model, my_socket, image,
                     frame_count, my_path, face_mesh, rect, file_flag, is_image_saved):
    img_rgb = image.copy()
    result = face_mesh.process(img_rgb)
    height, width, channels = img_rgb.shape

    if result is not None:
        if result.multi_face_landmarks is not None:
            rect[0] = x_start = int(result.multi_face_landmarks[0].landmark[234].x * width / 1)
            rect[1] = x_end = int(result.multi_face_landmarks[0].landmark[356].x * width / 1)
            rect[2] = y_start = int(result.multi_face_landmarks[0].landmark[10].y * height / 1)
            rect[3] = y_end = int(result.multi_face_landmarks[0].landmark[152].y * height / 1)

            if x_start < 0:  x_start = 0; 
            if x_end   < 0:  x_end = 0; 
            if y_start < 0:  y_start = 0; 
            if y_end   < 0:  y_end = 0; 
            

            img = img_rgb[y_start:y_end, x_start:x_end]
            img_resize = cv2.resize(img, (48, 48))
            image_to_define = img_to_array(img_resize)
            image_to_define = cv2.cvtColor(image_to_define, cv2.COLOR_RGB2GRAY)
            image_to_define = image_to_define.reshape((1, 48, 48))
            image_to_define = image_to_define.astype('float32') / 255.
            prediction_ = emotion_model.predict(image_to_define)
            prediction = prediction_[0]

            if (not is_image_saved[0]) & (file_flag[0] == 'file_write_ing'):
                cv2.imwrite(my_path + r'\Live.mp4_0.png',
                            img_rgb[int(y_start * 0.8):int(y_end * 1.2), int(x_start * 0.8):int(x_end * 1.2)])
                is_image_saved[0] = True

            if file_flag[0] == 'file_write_ing':
                file_emotion = open(my_path + r'\Live.mp4.txt', 'a')
                file_emotion.write(f'@FRAMEDATA@{frame_count}$0^{prediction[0]}#{prediction[1]}#{prediction[2]}#' +
                                   f'{prediction[3]}#{prediction[4]}#{prediction[5]}#{prediction[6]}' +
                                   f'^{x_start}#{x_end}#{y_start}#{y_end}\n')
                file_emotion.close()

            my_socket.send(bytes(f'@LIVEDATA@{prediction[0]}#{prediction[1]}#{prediction[2]}#{prediction[3]}' +
                                 f'#{prediction[4]}#{prediction[5]}#{prediction[6]}' +
                                 f'${x_start}#{x_end}#{y_start}#{y_end}\n', 'utf8'))
            return prediction


        else:

            if file_flag[0] == 'file_write_ing':
                file_emotion = open(my_path + r'\Live.mp4.txt', 'a')
                file_emotion.write(f'@FRAMEDATA@{frame_count}\n')
                file_emotion.close()

            my_socket.send(bytes(f'@LIVEDATA@\n', 'utf8'))
            return [0, 0, 0, 0, 0, 0, 0]


def send_to_server_image(image, prediction, s_server):
    img_to_server = image.copy()
    img_to_server = cv2.resize(img_to_server, (640, 480))
    for index, emotion in enumerate(EMOTIONS):
        cv2.putText(img_to_server, emotion, (10, index * 30 + 30),
                    cv2.FONT_HERSHEY_DUPLEX, 1, (0, 255, 0), 1)
        cv2.rectangle(img_to_server, (180, index * 30 + 10), (180 + int(prediction[index] * 250), (index + 1) * 30 + 4),
                      (255, 0, 0), -1)
    # cv2.imshow('test',img_to_server)
    # cv2.waitKey(1)
    s_server.l_send(img_to_server)


def send_to_server_prediction(prediction, s_server):
    prediction_string = f'{prediction[0]}#{prediction[1]}#{prediction[2]}#{prediction[3]}#{prediction[4]}' \
                        f'#{prediction[5]}#{prediction[6]}'
    s_server.s_send('@DATA@' + prediction_string)


def live_analysis(live_flag, recode_flag, emotion_model, my_socket, my_path):  # flag  go/stop
    s_server = LiveSocket()
    s_server.connect(server_ip, server_port)

    cap = cv2.VideoCapture(0)
    mp_face_mesh = mp.solutions.face_mesh
    face_mesh = mp_face_mesh.FaceMesh(max_num_faces=1)
    start_time = 0
    frame_count = 0
    frame_gap = 12
    rect = [1, 1, 2, 2]
    fps = 30
    video_out = None
    file_write_flag = ['file_write_none']
    is_image_saved = [False]
    prediction = [0, 0, 0, 0, 0, 0, 0, 0]

    while live_flag[0] == 'live_start':
        res, image = cap.read()

        if recode_flag[0] == 'video_write_start':
            if my_path[0] == '':
                continue
            fourcc = cv2.VideoWriter_fourcc(*'FMP4')
            video_out = cv2.VideoWriter(my_path[0] + r'\Live__.mp4', fourcc, 60, (640, 480))
            file_emotion = open(my_path[0] + r'\Live.mp4.txt', 'w')
            file_emotion.close()
            start_time = time.time()
            frame_count = 0
            video_out.write(image)
            recode_flag[0] = 'video_write_ing'
            file_write_flag[0] = 'file_write_ing'

        elif recode_flag[0] == 'video_write_ing':
            video_out.write(image)
            cv2.putText(image, "rec", (0, 50), cv2.FONT_HERSHEY_COMPLEX, 1, (0, 150, 0), 2)

        elif recode_flag[0] == 'video_write_end':
            file_write_flag[0] = 'file_write_end'
            video_out.release()
            video_time = time.time() - start_time
            fps = frame_count / video_time
            change_fps(my_path[0], fps)
            recode_flag[0] = 'write_none'

        if file_write_flag[0] == 'file_write_end':
            with open(my_path[0] + r'\Live.mp4.txt', 'r+') as file:
                file_data = file.read()
                file.seek(0, 0)
                file.write(f'@START@{fps}#{frame_count}#{frame_gap}\n' + file_data)
            with open(my_path[0] + r'\Live.mp4.txt', 'a') as file2:
                file2.write(f'@END@{my_path[0]}' + r'\Live.mp4')
            file_write_flag[0] = 'file_write_none'

        # th1 = Thread(target=send_to_server, args=(image, prediction, s_server))
        # th1.start()

        if (frame_count % frame_gap) == 0:
            prediction = emotion_analysis(emotion_model, my_socket, image,
                                          frame_count, my_path[0], face_mesh, rect, file_write_flag, is_image_saved)

            send_to_server_prediction(prediction, s_server)
        send_to_server_image(image,prediction, s_server)
        # #################
        #         img_to_server = image.copy()
        #         img_to_server = cv2.resize(img_to_server,(640,480))
        #         for index, emotion in enumerate(EMOTIONS):
        #             cv2.putText(img_to_server, emotion, (10, index * 30 + 30),
        #                         cv2.FONT_HERSHEY_DUPLEX, 1, (0, 255, 0), 1)
        #             cv2.rectangle(img_to_server, (180, index * 30 + 10), (180 + int(prediction[index] * 250), (index + 1) * 30 + 4),
        #                           (255, 0, 0), -1)
        #
        #         cv2.imshow('test', img_to_server)
        #         cv2.waitKey(1)
        #
        # #################

        cv2.rectangle(image, (rect[0], rect[2]), (rect[1], rect[3]), (0, 0, 0), 3)
        cv2.namedWindow("live", cv2.WINDOW_NORMAL)
        cv2.imshow('live', image)
        cv2.waitKey(1)

        frame_count += 1
        # th1.join()

    cv2.destroyWindow('live')

    s_server.close()
