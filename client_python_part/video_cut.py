from moviepy.video.io.VideoFileClip import VideoFileClip
from os.path import splitext


def video_cutting(data):
    datas = data.split('#')

    file_path_no_extend = splitext(datas[2])[0]
    start_time = float(datas[0])
    end_time = float(datas[1])

    input_video_path = datas[2]
    output_video_path = file_path_no_extend + f"_{int(start_time)}S-{int(end_time)}S.mp4"

    with VideoFileClip(input_video_path) as video:
        new = video.subclip(start_time, end_time)
        new.write_videofile(output_video_path, audio_codec='aac')






