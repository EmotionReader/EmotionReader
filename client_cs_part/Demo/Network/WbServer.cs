using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EmotionReader
{

    enum LogFlag { NONE, START, STOP, CONNECT, DISCONNECT, ERROR };
    delegate void LogMessage(LogFlag flag, string msg);
    delegate void RecvMessage(Socket s, string msg);


    class WbServer : IDisposable
    {
        private const int BUFF_SIZE = 1024;
        private LogMessage LogMessage = null;
        private RecvMessage RecvMessage = null;

        private Socket server = null;
        public Socket client = null;
        private Thread tr = null;

        public WbServer(int port)
        {
            CreatListenSocket(port);
        }

        public void Start(LogMessage log, RecvMessage rmsg)
        {
            LogMessage = log;
            RecvMessage = rmsg;
            tr = new Thread(ListenThread);
            //tr.IsBackground = true;
            tr.Start();
        }

        private void ListenThread()
        {
            LogMessage(LogFlag.START, "서버 동작.....");
            try
            {
                while (true)
                {
                    client = server.Accept();
                    IPEndPoint ip = (IPEndPoint)client.RemoteEndPoint;
                    string temp = string.Format("client 연결: {0}:{1}", ip.Address, ip.Port);
                    LogMessage(LogFlag.CONNECT, temp);
                    Thread t = new Thread(WorkThread);
                    t.IsBackground = true;
                    t.Start(client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void WorkThread(object obj) //recice
        {
            Socket client = (Socket)obj;
            byte[] data = new byte[BUFF_SIZE];
            try
            {

                while (true)
                {
                    Array.Clear(data, 0, data.Length);
                    client.Receive(data, data.Length, SocketFlags.None);
                    RecvMessage(client, Encoding.Default.GetString(data).Trim('\0'));
                    //Console.WriteLine("수신 메시지: " + Encoding.Default.GetString(data));

                }
            }
            catch (Exception)
            {
                IPEndPoint ip = (IPEndPoint)client.RemoteEndPoint;
                string temp = string.Format("client disconnect: {0}:{1}", ip.Address, ip.Port);
                LogMessage(LogFlag.DISCONNECT, temp);
                client.Close();
            }
        }

        private void CreatListenSocket(int port)
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
            server.Bind(ipep);
            server.Listen(20);
        }

        public void Dispose()
        {
            LogMessage(LogFlag.STOP, "서버 종료.....");           
            tr.Interrupt();
            server.Close();
        }

        public int SendData(Socket sock, string msg, int size)
        {
            return sock.Send(Encoding.Default.GetBytes(msg));
        }
    }
}
