using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EmotionReader
{
    public class Control
    {
        public VideoInfo Vinfo;         //video 분석 Data 저장
        public MainWindow mainwindow;   //MainWindow 객체 저장           
        public Thread t;                //python code 실행 하는 thread
        public string Pythonexe;        //python.exe 위치
        public string Pythonsource;     //main.py 위치
        public string ProjectPath;      //EmotionReader.exe 위치
        public string ConfigPath;       //pytho의 configfile.py 위치
        public string Allpath;          //main.py 상위 폴드

        #region 네트웤 사용 필드
        private const int SERVER_PORT = 9001;
        private WbServer server = null;
        #endregion

        #region 싱글톤
        public static Control Instance { get; private set; }
        static Control() { Instance = new Control(); }
        private Control() { }
        #endregion        

        #region 네트워크 초기화
        public bool Init()
        {
            try
            {
                //소켓생성 -> listen
                server = new WbServer(SERVER_PORT);
                //ListenThread 에서 사용되는 delegate 전달
                server.Start(LogMessage, RecvMessage); 
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region 네트웤 콜백 메서드
        private void LogMessage(LogFlag flag, string msg)
        {
            Console.WriteLine("[{0}] : {1} ({2})", flag, msg, DateTime.Now.ToString());
            string logmsg = string.Format("[{0}] : {1} ({2})", flag, msg, DateTime.Now.ToString());
            int flag1 = (int)flag;
            mainwindow.UpdateLogMsg_D(msg, flag1);
        }
        private void RecvMessage(Socket client, string msg)
        {
            string[] sp = msg.Split('@');         
            Console.WriteLine(msg);

            for (int i = 1; i < sp.Length; i += 2)
            {
                switch (sp[i])
                {
                    //UI에 처리 진도 나타낸다
                    case "PROGRESS": OnGetInfo(sp[i + 1]); break;
                    //표정 분석이 끝남고, Data를 읽어온다
                    case "END": OnEnd(sp[i + 1]); break;
                    //python 코드에서 error 발생시 해당 함수로 error 내용들어온다
                    case "ERROR": OnError(sp[i + 1]); break;        
                    //live 표정 분석 시 처리한 Data 들어온다
                    case "LIVEDATA": OnLiveData(sp[i + 1]); break;                    
                }
            }
        }
        #endregion

        #region 네트웤 응답 처리 메서드

        private void OnLiveData(string msg)
        {
            if (msg == "\n")  // 얼굴 못찾으면 value 안옴
            {
                mainwindow.LiveBar_Anger = 0;
                mainwindow.LiveBar_Disgust = 0;
                mainwindow.LiveBar_Fear = 0;
                mainwindow.LiveBar_Happy = 0;
                mainwindow.LiveBar_Neutral = 0;
                mainwindow.LiveBar_Sad = 0;
                mainwindow.LiveBar_Surprise = 0;
                return;
            }
            string[] sp = msg.Split('$');
            string[] face_ex = sp[0].Split('#');
            string[] points = sp[1].Split('#');

            mainwindow.LiveBar_Anger = double.Parse(face_ex[0]) * 100;
            mainwindow.LiveBar_Disgust = double.Parse(face_ex[1]) * 100;
            mainwindow.LiveBar_Fear = double.Parse(face_ex[2]) * 100;
            mainwindow.LiveBar_Happy = double.Parse(face_ex[3]) * 100;
            mainwindow.LiveBar_Neutral = double.Parse(face_ex[4]) * 100;
            mainwindow.LiveBar_Sad = double.Parse(face_ex[5]) * 100;
            mainwindow.LiveBar_Surprise = double.Parse(face_ex[6]) * 100;

            DrawPieChart(face_ex);
        }

        private void DrawPieChart(string[] facial_ex)
        {
            List<double> valuelist = new List<double>();
            for (int i = 0; i < facial_ex.Length - 1; i++)
            {
                valuelist.Add(double.Parse(facial_ex[i]));
            }

            for (int i = 0; i < valuelist.Count - 1; i++)
            {
                if (valuelist[i] == valuelist.Max())
                {
                    mainwindow.ChangePieChart(i);
                }
            }
        }

        private void OnGetInfo(string msg)
        {
            try
            {
                //프로그래스바 정보 업데이트
                //File Read
                msg = msg.Trim();
                Double currentprogress = Double.Parse(msg);

                mainwindow.CurrentProgress = (Double)(currentprogress * 100);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void OnEnd(string msg)
        {
            try
            {
                msg = msg.Trim();
                string dir = Path.GetFullPath(msg);

                string[] sp = dir.Split('\\');


                FileRead(dir);

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void OnError(string msg)
        {
            try
            {
                throw new Exception(string.Format("!!{0}!! \n잘못된 파일 위치입니다.", msg));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion

        #region FileIO
        public void FileRead(string dir)
        {
            try
            {
                
                string text = File.ReadAllText(dir);
                string[] sp = text.Split('@');

                for (int i = 1; i < sp.Length; i += 2)
                {
                    switch (sp[i])
                    {
                        case "START": FileStart(sp[i + 1]); break;
                        case "FRAMEDATA": FileGetInfo(sp[i + 1]); break;
                        case "END": FileEnd(dir.Substring(0,dir.Length-4)); break;                        
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void SaveFile()
        {
            var dir = Vinfo.VideoPath + ".txt";

            using (var writer = new StreamWriter(dir))
            {
                if (Vinfo != null)
                {
                    // START Write
                    string start = string.Format("@START@{0}#{1}#{2}", Vinfo.Fps, Vinfo.Frame_max_count, Vinfo.Frame_Gap);
                    writer.WriteLine(start);

                    string framedata = string.Empty;

                    foreach (Face face in Vinfo.Faces)
                    {
                        int facecode = face.Face_code;
                        foreach (FaceDetail fd in face.Face_details)
                        {
                            framedata += string.Format("@FRAMEDATA@");
                            framedata += string.Format("{0}${1}^{2}#{3}#{4}#{5}#{6}#{7}#{8}^{9}#{10}#{11}#{12}", fd.Frame_no, facecode,
                                fd.Current_Anger / 100, fd.Current_Disgust / 100, fd.Current_Fear / 100, fd.Current_Happy / 100, fd.Current_Neutral / 100, fd.Current_Sad / 100, fd.Current_Surprise / 100,
                                fd.X1, fd.Y1, fd.X2, fd.Y2);
                            //// FRAMEDATA write
                            writer.WriteLine(framedata);
                            framedata = string.Empty;
                        }
                    }
                    writer.WriteLine("@END@");
                }
            }
            MessageBox.Show("Successfully Saved");
        }
        private void FileStart(string msg)
        {
            try
            {
                msg = msg.Trim();
                string[] f1 = msg.Split('#');

                double fps = double.Parse(f1[0]);
                int fmc = int.Parse(f1[1]);
                double tps = double.Parse(f1[2]);

                Vinfo = new VideoInfo(fps, fmc, tps);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void FileGetInfo(string msg)
        {
            //   frame_no  $   face_code   ^   anger # disgust #  fear # happy # neutral # sad # surprise   ^   x1 # y1 # x2 # y2 
            //             $   face_code   ^   anger # disgust #  fear # happy # neutral # sad # surprise   ^   x1 # y1 # x2 # y2     (video_analysis)
            try
            {
                msg = msg.Trim();
                string[] f1 = msg.Split('$');

                if (f1.Length == 1)
                    return;

                int frame_no = int.Parse(f1[0]);

                for (int i = 1; i <= f1.Length - 1; i++)
                {
                    int emotion_code, x1, y1, x2, y2;
                    List<double> face_valuelist;

                    string[] f2 = f1[i].Split('^');
                    int face_code = int.Parse(f2[0]);
                    string facial_ex_str = f2[1];
                    string points_str = f2[2];

                    //  i)  가장 높은 확률의 표정을 face_valuelist 에 저장
                    // ii)  Pie Chart Value 수정
                    GetMaxFaceValue(facial_ex_str, out emotion_code, out face_valuelist);

                    // 얼굴 좌표 저장
                    GetPointsValue(points_str, out x1, out y1, out x2, out y2);

                    FaceDetail facedetail = new FaceDetail(frame_no, emotion_code, x1, y1, x2, y2,
                        face_valuelist[0], face_valuelist[1], face_valuelist[2], face_valuelist[3], face_valuelist[4], face_valuelist[5], face_valuelist[6]);

                    int idx = Vinfo.Get_Face_code_idx(face_code);
                    if (idx != -1)                    
                        Vinfo.Faces[idx].Face_details.Add(facedetail);
                    else // 인식한 face_code의 기존 데이터가 리스트에 없는 경우
                    {
                        Face face = new Face(face_code, facedetail);
                        Vinfo.Faces.Add(face);                                 
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #region FileGetInfo에서 사용

        private void GetMaxFaceValue(string msg, out int emotion_code, out List<double> valuelist)
        {
            string[] facial_ex = msg.Split('#');
            emotion_code = -1;

            valuelist = new List<double>();
            for (int i = 0; i < facial_ex.Length; i++)
            {
                valuelist.Add(double.Parse(facial_ex[i]));
            }

            for (int i = 0; i < valuelist.Count; i++)
            {
                if (valuelist[i] == valuelist.Max())
                {
                    emotion_code = i;
                }
            }
        }
        private void GetPointsValue(string msg, out int x1, out int y1, out int x2, out int y2)
        {
            string[] sp = msg.Split('#');

            x1 = int.Parse(sp[0]);
            y1 = int.Parse(sp[1]);
            x2 = int.Parse(sp[2]);
            y2 = int.Parse(sp[3]);
        }
        #endregion

        private void FileEnd(string msg)
        {
            try
            {
                msg = msg.Trim();
                Vinfo.Set_VideoPath(msg);               
                mainwindow.MakeSlide_D(Vinfo);
                mainwindow.CurrentProgress_Visibility = System.Windows.Visibility.Hidden;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region 전송 메서드
        public void SendCutVid(string msg)
        {
            string packet = Packet.SendCutVid(msg);
            server.SendData(server.client, packet, packet.Length);
        }
        public void SendDirectory(string msg)
        {
            string packet = Packet.SendDirectory(msg);
            server.SendData(server.client, packet, packet.Length);
        }
        public void SendLiveStart()
        {
            string packet = Packet.SendLiveStart();
            server.SendData(server.client, packet, packet.Length);
        }
        public void SendLiveEnd()
        {
            string packet = Packet.SendLiveEnd();
            server.SendData(server.client, packet, packet.Length);
        }
        public void SendRecordStart(string msg)
        {
            string packet = Packet.SendRecordStart(msg);
            server.SendData(server.client, packet, packet.Length);
        }
        public void SendRecordEnd()
        {
            string packet = Packet.SendRecordEnd();
            server.SendData(server.client, packet, packet.Length);
        }
        #endregion

        #region Call Python
        public void Call_Python()
        {
            try
            {
                t = new Thread(Python_execProcess);
                t.IsBackground = true;
                t.Start();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void Python_execProcess()
        {
            try
            {
                var psi = new ProcessStartInfo();               
                psi.FileName = Pythonexe;
                var script = Pythonsource;
                psi.Arguments = $"\"{script}\"";
                psi.UseShellExecute = false;
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = false;
                var errors = "";
                using (var process = Process.Start(psi))
                {
                    errors = process.StandardError.ReadToEnd();
                }
                Console.WriteLine(errors);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region Read File Paths

        
        public void PathSetting()
        {
            ProjectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\"));
            Allpath = Path.GetFullPath(Path.Combine(ProjectPath, @"..\..\client_python_part"));
            Pythonexe = Path.GetFullPath(Path.Combine(ProjectPath, @"..\..\Pythonexe\python.exe"));
            Pythonsource = Allpath + "\\main.py";
            ConfigPath = Allpath + "\\config_data.py";  
        }
        public void SetConfigpy(Dictionary<string,string> getmap)
        {
            string path = ConfigPath;
            string text = File.ReadAllText(path);
            string[] split2 = text.Split('\n');            
           
            File.WriteAllText(path, "");
            foreach (var a in getmap)
            {
                using (StreamWriter w = File.AppendText(path))
                {
                    w.WriteLine(a.Key + "=" + a.Value);
                }
            }
        }


        #endregion
    }
}
