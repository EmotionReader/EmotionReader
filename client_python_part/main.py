from sys import exit
import socket
import os.path as path
from threading import Thread
from keras.models import load_model
import video_analysis as fa
import video_cut
import live_analysis as la
from config_data import *
from live_socket import LiveSocket
import os

if __name__ == '__main__':
    # ctypes.windll.user32.ShowWindow(ctypes.windll.kernel32.GetConsoleWindow(), 0)
    basePath = (os.path.abspath(os.path.join(os.path.abspath(__file__), os.pardir)))
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)


    loaded_model = load_model(basePath+'\hand_recog_model_0528.h5')
    print('load_model_success')
   
    live_flag = ['live_start']
    recode_flag = ['write_none']
    my_path = ['']
    try:
        s.connect((cs_application_ip, cs_application_port))
        print('server_connected')

        while True:
            data_recv = s.recv(1024).decode("utf-8")
            print(data_recv)
            data_recv_split = data_recv.split('@')

            if data_recv_split[1] == 'RECORDSTART':
                recode_flag[0] = 'video_write_start'
                my_path[0] = data_recv_split[2]

            elif data_recv_split[1] == 'RECORDEND':
                recode_flag[0] = 'video_write_end'
            elif data_recv_split[1] == 'LIVESTART':
                live_flag[0] = 'live_start'

                th1 = Thread(target=la.live_analysis,
                             args=(live_flag, recode_flag, loaded_model, s, my_path,))
                th1.start()
            elif data_recv_split[1] == 'LIVEEND':
                live_flag[0] = 'live_end'
            elif data_recv_split[1] == 'START':
                video_path = data_recv_split[2]
                if not path.exists(video_path):
                    s.send(bytes(f"@ERROR@PATHERROR", "utf8"))
                else:
                    fa.video_analysis(video_path, 60, loaded_model, s)
            elif data_recv_split[1] == 'VIDEOCUT':
                video_cut.video_cutting(data_recv_split[2])

    except Exception as e:
        print(e)
        s.close()
        exit(0)
