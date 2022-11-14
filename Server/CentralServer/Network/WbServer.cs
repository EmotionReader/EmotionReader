using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace CentralServer
{
    enum LogFlag { NONE, START, STOP, CONNECT, DISCONNECT, ERROR };

    delegate void LogMessage(LogFlag flag, string msg);
    delegate void RecvMessage(Socket s, byte[] msg);
    delegate void CreateImage(Socket s, string ipaddress);
    delegate void CreateText(string ipaddress);

    internal class WbServer : IDisposable
    {
        private LogMessage  LogMessage  = null;
        private RecvMessage RecvMessage = null;
        private CreateImage createImage = null;
        private CreateText  createText  = null;

        private Socket server = null;
        List<Socket> sockets = new List<Socket>();

        private Thread tr = null;

        private const int BUFF_SIZE = 1024;

        public WbServer(int port)
        {
            CreateListenSocket(port);
        }

        #region 대기소켓 생성 및 클라이언트 접속 대기
        private void CreateListenSocket(int port)
        {
            //1. 소켓 생성
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //2. 주소 할당
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
            server.Bind(ipep);

            //3. 망 연결
            server.Listen(20);
        }

        public void Start(LogMessage log, RecvMessage rmsg, CreateImage crimg, CreateText crtxt)
        {
            LogMessage  = log;
            RecvMessage = rmsg;
            createImage = crimg;
            createText = crtxt;

            tr = new Thread(ListenThread);
            //tr.IsBackground = true;
            tr.Start();
        }

        private void ListenThread()
        {
            LogMessage(LogFlag.START, "서버 동작......");

            try
            {
                while (true)
                {
                    //4. 접속 대기
                    Socket client = server.Accept();

                    IPEndPoint ip = (IPEndPoint)client.RemoteEndPoint;
                    string temp = string.Format("클라이언트 연결 - {0}:{1}", ip.Address, ip.Port);
                    LogMessage(LogFlag.CONNECT, temp);

                    sockets.Add(client);
                    createImage(client, ip.Address.ToString());
                    createText(ip.Address.ToString());

                    Thread t = new Thread(WorkThread);
                    t.IsBackground = true;
                    t.Start(client);
                }
            }
            catch (Exception)
            {
                //deleteImage(client);    // 연결 해제 시 ImageControl 삭제
            }
        }
        #endregion

        #region 통신 관련 코드
        //Recv만 처리
        private void WorkThread(object obj)
        {
            Socket client = (Socket)obj;

            byte[] data;

            try
            {
                while (true)
                {
                    data = null;
                    if (ReceiveData(client, ref data) == 0)
                        throw new Exception("수신 오류");
                    //client.Receive(data, data.Length, SocketFlags.None);
                    RecvMessage(client, data);
                }
            }
            catch (Exception)
            {
                IPEndPoint ip = (IPEndPoint)client.RemoteEndPoint;
                string temp = string.Format("클라이언트 연결 해제 - {0}:{1}", ip.Address, ip.Port);
                LogMessage(LogFlag.DISCONNECT, temp);
                sockets.Remove(client);
                client.Close();
            }
        }

        public int SendData(Socket sock, string msg)
        {
            byte[] data = Encoding.Default.GetBytes(msg);
            int ret = SendData(sock, data, data.Length);
            return ret;
        }

        //전체 전송
        public void SendAllData(Socket sock, string msg, bool b)
        {
            if (b == true)
            {
                foreach (Socket socket in sockets) //자신을 포함
                {
                    SendData(socket, msg);
                }
            }
            else
            {
                foreach (Socket socket in sockets)
                {
                    if (socket != sock)  //자신을 제외
                        SendData(socket, msg);
                }
            }
        }

        private int SendData(Socket client, byte[] data, int length)
        {
            try
            {
                int total = 0;
                int size = length; //보낼크기
                int left_data = size;
                int send_data = 0;

                // 전송할 데이터의 크기 전달
                byte[] data_size = new byte[4];
                data_size = BitConverter.GetBytes(size);
                send_data = client.Send(data_size);

                // 실제 데이터 전송
                while (total < size)
                {
                    send_data = client.Send(data, total, left_data, SocketFlags.None);
                    total += send_data;
                    left_data -= send_data;
                }
                return total;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        private int ReceiveData(Socket client, ref byte[] data)
        {
            try
            {
                int total = 0;  //실제 받은 크기
                int size = 0;   //수신할 크기
                int left_data = 0;
                var recv_data = 0;

                // 수신할 데이터 크기 알아내기 
                byte[] data_size_1 = new byte[8];

                while(left_data < 8)
                {
                    recv_data = client.Receive(data_size_1, data_size_1.Length, SocketFlags.None);
                    left_data += recv_data;
                }

                string a = Encoding.Default.GetString(data_size_1);
                size = int.Parse(a);

                data = new byte[size];
                recv_data = 0;
                left_data = size;
                // 실제 데이터 수신
                while (total < size)
                {
                    recv_data = client.Receive(data, total, left_data, 0);
                    if (recv_data == 0) break;
                    total += recv_data;
                    left_data -= recv_data;
                }
                return total;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }
        #endregion

        public void Dispose()
        {
            LogMessage(LogFlag.STOP, "서버 종료......");
            //tr.Abort();
            tr.Interrupt();
            server.Close();
        }
    }
}
