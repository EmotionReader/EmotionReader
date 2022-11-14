using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CentralServer
{
    public class Control : PbservableObject
    {
        public MainWindow main;

        #region 싱글톤
        public static Control Instance { get; private set; }
        static Control()
        {
            Instance = new Control();
        }
        private Control() { }
        #endregion

        #region 프로퍼티
        private Tuple<Socket, ImageSource> remoteFrame;
        public Tuple<Socket, ImageSource> RemoteFrame
        {
            get => remoteFrame;
            set {
                remoteFrame = value;
                if (main != null)
                    main.MySource_image = remoteFrame;
                OnPropertyChanged("RemoteFrame");
                //await main.image_test.SetSourceAsync(stream);
            }
        }
        public bool isRecoding = false;
        public string recodeSavingPath = null;
        #endregion

        #region 네트웤 사용 필드
        private const int SERVER_PORT = 6556;
        private WbServer server = null;
        #endregion

        #region 시작과 종료시점(데이터 베이스 연결/소켓 및 종료 처리)
        public bool Init()
        {
            try
            {
                server = new WbServer(SERVER_PORT); //소켓생성--> listen
                server.Start(LogMessage, RecvMessage, CreateImage, CreateText); //ListenThread
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void Exit()
        {
            try
            {
                if (server != null)
                {
                    server.Dispose(); //ListenThread를 종료, 대기소켓close
                    server = null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region 네트웤 콜백 메서드
        #region LogMessage
        private void LogMessage(LogFlag flag, string msg)
        {
            Console.WriteLine("[{0}] : {1} ({2})", flag, msg, DateTime.Now.ToString());
            string logmsg = string.Format("[{0}] : {1} ({2})", flag, msg, DateTime.Now.ToString());
        }
        #endregion

        #region RecvMessgae
        private ImageSource BitmapToImageSource(Bitmap bitmap)
        {
            IntPtr handle = bitmap.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        public static extern bool DeleteObject([In] IntPtr hObject);
        private void RecvMessage(Socket client, byte[] msg)
        {
            try
            {
                if (msg.Length > 200)
                {
                    var bytes = msg.Select(x=> Convert.ToByte(x)).ToArray();  //<=========================================
                                                                              //video_Data.Datas = bytes;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        using (MemoryStream stream = new MemoryStream(bytes))
                        {
                            using (Bitmap bitmap = new Bitmap(stream))
                            {
                                RemoteFrame = new Tuple<Socket, ImageSource> (client, BitmapToImageSource(bitmap));
                            }
                        }
                    });
                }
                else
                {
                    string recv = Encoding.Default.GetString(msg).Trim('\0');
                    string[] sp = recv.Split('@');

                    for (int i = 1; i < sp.Length; i+=2)
                    {
                        switch (sp[i])
                        {
                            case "DATA":  OnFaceData(client, sp[i + 1]); break;
                            case "CLOSE": OnClientClose(client); break;
                        }
                    }
                }
                #region 일단 킵
                //var a = LoadImage(bytes);
                //main.main.LoadImage_D(bytes);
                //MemoryStream mem = new MemoryStream();
                //mem.Write(bytes, 0, (int)bytes.Length);
                ////mem = new MemoryStream(imageData);
                //mem.Position = 0;

                //var image = new BitmapImage();
                //image.BeginInit();
                //image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                //image.CacheOption = BitmapCacheOption.OnLoad;
                //image.UriSource = null;
                //image.StreamSource = mem;
                //image.EndInit();
                //video_Data.RemoteFrame = image;
                //main.CamImage = a;
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void OnFaceData(Socket client, string msg)
        {
            // 0 - anger, 1 - , 2- , 3- , 4- , 5 - , 6 -  
            if (isRecoding == true)
            {
                string[] facedata = msg.Split('#');
                double[] arr = new double[facedata.Length];
                for (int i = 0; i < facedata.Length; i++)
                {
                    arr[i] = double.Parse(facedata[i]);
                }
                int idx = Array.IndexOf(arr, arr.Max());
                object a = main.EmotionCount.Clone();
                int[] b = (int[])a;
                b[idx]++;
                b[7]++;
                main.EmotionCount = b;
                IPEndPoint ip = (IPEndPoint)client.RemoteEndPoint;
                string path = this.recodeSavingPath + "\\" + ip.Address.ToString() + ".txt";
                if (!File.Exists(path))
                {
                    using (StreamWriter sw = File.CreateText(path)) { }
                }
                
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.Write("["+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]@");
                    sw.WriteLine(facedata[0] + "#" + facedata[1] + "#" + facedata[2] + "#" + facedata[3] + "#" + facedata[4] + "#" + facedata[5] + "#" + facedata[6]);
                }
                
            }

        }

        private void OnClientClose(Socket client)
        {
            main.ImageConDel(client);
        }
        #endregion

        #region CreateImage
        private void CreateImage(Socket client, string ipaddress)
        {
            try
            {
                main.ImageConInit(client, ipaddress);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region CreateText
        private void CreateText(string ipaddress)
        {
          
        }
        #endregion
        #endregion
    }
}
