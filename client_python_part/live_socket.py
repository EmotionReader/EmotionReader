import socket
import cv2
import io
from PIL import Image


class LiveSocket:
    """A special type of socket to handle the sending and receiving of fixed
       size frame strings over usual sockets
       Size of a packet or whatever is assumed to be less than 100MB
    """

    def __init__(self, sock=None):
        if sock is None:
            self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        else:
            self.sock = sock

    def connect(self, host, port):
        self.sock.connect((host, port))

    def l_send(self, data):

        total_sent = 0
        meta_sent = 0

        cv2_im = cv2.cvtColor(data, cv2.COLOR_BGR2RGB)
        pil_im = Image.fromarray(cv2_im)
        b = io.BytesIO()
        pil_im.save(b, 'jpeg')
        frame_string = b.getvalue()

        length = len(frame_string)
        length_str = str(length).zfill(8)

        while meta_sent < 8:
            b = length_str[meta_sent:].encode()
            sent = self.sock.send(b)
            if sent == 0:
                raise RuntimeError("Socket connection broken")
            meta_sent += sent

        while total_sent < length:
            # a = frame_string[total_sent:]
            sent = self.sock.send(frame_string[total_sent:])
            if sent == 0:
                raise RuntimeError("Socket connection broken")
            total_sent += sent

    def s_send(self, data):

        total_sent = 0
        meta_sent = 0
        data_b = bytes(data, 'utf-8')

        length = len(data_b)
        length_str = str(length).zfill(8)
        while meta_sent < 8:
            b = length_str[meta_sent:].encode()
            sent = self.sock.send(b)
            if sent == 0:
                raise RuntimeError("Socket connection broken")
            meta_sent += sent

        while total_sent < length:
            # a = data_b[total_sent:]
            sent = self.sock.send(data_b[total_sent:])
            if sent == 0:
                raise RuntimeError("Socket connection broken")
            total_sent += sent

    def close(self):
        self.s_send("@CLOSE@")
        self.sock.close()

    def l_receive(self):
        tot_rec = 0
        meta_rec = 0
        msg_array = []
        meta_array = []
        while meta_rec < 8:
            a = self.sock.recv(8 - meta_rec)
            chunk = a.decode()
            if chunk == '':
                raise RuntimeError("Socket connection broken")
            meta_array.append(chunk)
            meta_rec += len(chunk)
        length_str = ''.join(meta_array)
        length = int(length_str)

        while tot_rec < length:
            chunk = self.sock.recv(length - tot_rec)
            if chunk == '':
                raise RuntimeError("Socket connection broken")
            msg_array.append(chunk)
            tot_rec += len(chunk)
        return b''.join(msg_array)
