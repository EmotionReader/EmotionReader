using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace EmotionReader
{
    /// <summary>
    /// option.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class option : Window
    {
        public Control con = Control.Instance;
        public Dictionary<string, string> config_data = new Dictionary<string, string>();
            
        public string cs_application_ip{
            get { return Client_Ip.Text; }
            set { Client_Ip.Text = value;}
        }
        public string cs_application_port
        {
            get { return Client_Port.Text; }
            set { Client_Port.Text = value; }
        }
        public string server_ip
        {
            get { return Server_Ip.Text; }
            set { Server_Ip.Text = value; }
        }
        public string server_port
        {
            get { return Server_Port.Text; }
            set { Server_Port.Text = value; }
        }
        public string radian_th
        {
            get { return Radian.Text; }
            set { Radian.Text = value; }
        }
        public string blur_th
        {
            get { return Blur.Text; }
            set { Blur.Text = value; }
        }

        public option()
        {            
            InitializeComponent();
            show_config();
        }   
        private void Option_OK_Click(object sender, RoutedEventArgs e)
        {
            config_data["cs_application_ip"]    = cs_application_ip;
            config_data["cs_application_port"]  = cs_application_port;
            config_data["server_ip"]            = server_ip;
            config_data["server_port"]          = server_port;
            config_data["radian_th"]            = radian_th;
            config_data["blur_th"]              = blur_th;

            con.SetConfigpy(config_data);
            this.Close();
        }
        public void show_config()
        {
            string[] str = File.ReadAllLines(con.ConfigPath);
            foreach(string s in str)
            {
                if (s != "")
                {
                    string[] temp = s.Split('=');
                    config_data.Add(temp[0], temp[1]);
                }
            }
            cs_application_ip   = config_data["cs_application_ip"];
            cs_application_port = config_data["cs_application_port"];
            server_ip           = config_data["server_ip"];
            server_port         = config_data["server_port"];
            radian_th           = config_data["radian_th"];
            blur_th             = config_data["blur_th"];
        }
    }
}