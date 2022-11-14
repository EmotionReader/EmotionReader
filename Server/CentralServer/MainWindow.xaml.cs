using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CentralServer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public Control con = Control.Instance;
		public MainWindow main;
		public BitmapImage image_ = new BitmapImage();
		public MemoryStream mem = new MemoryStream();

		Dictionary<Socket, System.Windows.Controls.StackPanel> ImageInfo = new Dictionary<Socket, System.Windows.Controls.StackPanel>();

		internal Tuple<Socket, ImageSource> MySource_image
		{
			get { return MySource_image; }
			set { Dispatcher.Invoke(new Action(() => {
				var image = (System.Windows.Controls.Image)ImageInfo[value.Item1].Children[1];
				image.Source = value.Item2;
			})); }
		}
		private int[] emotionCount;
		public int[] EmotionCount
        {
			get { return emotionCount; }
			set {
				emotionCount = value;
				if (emotionCount[7] == 0)
					return;
				Dispatcher.Invoke(new Action(() => {

					//CultureInfo culture = CultureInfo.CreateSpecificCulture("fr-FR");
					angry.Width		= (double)emotionCount[0] / emotionCount[7] * 500; angry.ToolTip = "angry"			+ Math.Round(((double)emotionCount[0] / emotionCount[7] * 100), 2).ToString() + "%";
					disgusted.Width = (double)emotionCount[1] / emotionCount[7] * 500; disgusted.ToolTip = "disgusted"	+ Math.Round(((double)emotionCount[1] / emotionCount[7] * 100), 2).ToString() + "%";
					fearful.Width	= (double)emotionCount[2] / emotionCount[7] * 500; fearful.ToolTip = "fearful"		+ Math.Round(((double)emotionCount[2] / emotionCount[7] * 100), 2).ToString() + "%";
					happy.Width		= (double)emotionCount[3] / emotionCount[7] * 500; happy.ToolTip = "happy"			+ Math.Round(((double)emotionCount[3] / emotionCount[7] * 100), 2).ToString() + "%";
					neutral.Width	= (double)emotionCount[4] / emotionCount[7] * 500; neutral.ToolTip = "neutral"		+ Math.Round(((double)emotionCount[4] / emotionCount[7] * 100), 2).ToString() + "%";
					sad.Width		= (double)emotionCount[5] / emotionCount[7] * 500; sad.ToolTip = "sad"				+ Math.Round(((double)emotionCount[5] / emotionCount[7] * 100), 2).ToString() + "%";
					surprised.Width = (double)emotionCount[6] / emotionCount[7] * 500; surprised.ToolTip = "surprised"	+ Math.Round(((double)emotionCount[6] / emotionCount[7] * 100), 2).ToString() + "%";
				}));
			}
        }
		

		public MainWindow()
		{
			InitializeComponent();
			con.main = this;
			EmotionCount = new int[8];
		}
		
		#region Window Load & Close
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (con.Init() == true)
			{

			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Environment.Exit(0);
			System.Diagnostics.Process.GetCurrentProcess().Kill();

			this.Close();
		}
		#endregion

		#region Image Control Init / Delete
		public void ImageConInit(Socket socket, string ipadd)
		{
			Dispatcher.Invoke(new Action(() =>
			{
				StackPanel stack = new StackPanel();
				System.Windows.Controls.Image image = new System.Windows.Controls.Image();
				System.Windows.Controls.TextBlock text = new System.Windows.Controls.TextBlock();

				text.Text = ipadd;
				text.FontSize = 12;
				text.Width = 170;
				text.Height = 20;
				text.Margin = new Thickness(0, 0, 0, 0);
				text.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
				text.HorizontalAlignment = HorizontalAlignment.Left;

				image.Width = subGrid.ActualWidth;
				image.Height = subGrid.ActualHeight / 4;
				image.Stretch = Stretch.UniformToFill;
				image.Margin = new Thickness(2, 2, 2, 2);
				
				stack.MouseLeftButtonDown += new MouseButtonEventHandler(Imagestack_MouseLeftButtonDown);
				stack.Children.Add(text);
				stack.Children.Add(image);
			
				sub_image.Background = new SolidColorBrush(Color.FromRgb(200, 200, 200));
				sub_image.Children.Add(stack);

				ImageInfo.Add(socket, stack);
			}));
		}

		private void Imagestack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
			{
				StackPanel stack = (StackPanel)sender;
				StackPanel temp = new StackPanel();

				// main_Image에 있는 이미지 클릭 시
				if (main_Image.Children.Contains(stack))
				{
					var text = (TextBlock)stack.Children[0];
					var img = (System.Windows.Controls.Image)stack.Children[1];
					#region size
					text.FontSize = 12;
					text.Width = 170;
					text.Height = 20;

					img.Width = 200;
					img.Height = 130;

					stack.Width = img.Width + 4;
					stack.Height = text.Height + img.Height + 4;
					#endregion
					main_Image.Children.Clear();
					sub_image.Children.Add(stack);
					return;
				}

				// sub_Image에 있는 이미지 클릭 시 (main_Image에 이미지가 존재)
				if (main_Image.Children.Count != 0)
				{
					temp = (StackPanel)main_Image.Children[0];
					var text = (TextBlock)temp.Children[0];
					var img = (System.Windows.Controls.Image)temp.Children[1];

					#region Resize (main -> sub)
					text.FontSize = 12;
					text.Width    = 170;
					text.Height   = 20;

					img.Width  = 200;
					img.Height = 130;

					main_Image.Children.Clear();
					sub_image.Children.Add(temp);
					#endregion

					#region Resize (sub -> main)
					text = (TextBlock)stack.Children[0];
					img = (System.Windows.Controls.Image)stack.Children[1];

					sub_image.Children.Remove(stack);
					main_Image.Children.Add(stack);

					text.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
					text.FontSize = 14;
					text.Width = 170;
					text.Height = 20;

					img.Width = 400 * 4 / 5;
					img.Height = 260 * 4 / 5;
					#endregion
				}
				else // sub_Image에 있는 이미지 클릭 시 (main_Image에 이미지가 없음)
				{
					var text = (TextBlock)stack.Children[0];
					var img = (System.Windows.Controls.Image)stack.Children[1];

					main_Image.Children.Clear();
					sub_image.Children.Remove(stack);
					main_Image.Children.Add(stack);

					stack.Width = 550;
					stack.Height = 400;

					text.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
					text.FontSize = 14;
					text.Width = 170;
					text.Height = 20;

					img.Width = 450;
					img.Height = 300;
				}
			}
		}

		public void ImageConDel(Socket socket)
		{
			Dispatcher.Invoke(new Action(() =>
			{
				if (main_Image.Children.Contains(ImageInfo[socket]))
					main_Image.Children.Remove(ImageInfo[socket]);
				else
					sub_image.Children.Remove((ImageInfo[socket]));

				ImageInfo.Remove(socket);
			}));
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show(String.Format("{0}\n{1}", mainGrid.ActualHeight, main_Image.ActualHeight));
		}
        #endregion


        private void recode_Click(object sender, RoutedEventArgs e)
        {
			if((string)recode.Content == "Stop Record")
            {
				recode.Content = "Start Record";
				con.isRecoding = false;
				string path = con.recodeSavingPath + "\\" + "server.txt";
				if (!File.Exists(path))
				{
					using (StreamWriter sw = File.CreateText(path)) { }
				}
				using (StreamWriter sw = File.AppendText(path))
				{
					sw.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]\n");
					sw.WriteLine("[total_anger]: "		+ EmotionCount[0]);
					sw.WriteLine("[total_disgusted]: "	+ EmotionCount[1]);
					sw.WriteLine("[total_fearful]: "	+ EmotionCount[2]);
					sw.WriteLine("[total_happy]: "		+ EmotionCount[3]);
					sw.WriteLine("[total_neutral]: "	+ EmotionCount[4]);
					sw.WriteLine("[total_sad]: "		+ EmotionCount[5]);
					sw.WriteLine("[total_surprised]: "	+ EmotionCount[6]);
					sw.WriteLine("[total_count]: "		+ EmotionCount[7]);
				}
				data_p.Children.Clear();
				EmotionCount = new int[8];
				con.recodeSavingPath = null;
				
			}
			else if((string)recode.Content == "Start Record")
            {
				recode.Content = "Stop Record";
				con.isRecoding = true;
				
				DateTime ct = DateTime.Now;
				string time = string.Format("{0:0000}{1:00}{2:00}_{3:00}{4:00}{5:00}", ct.Year, ct.Month, ct.Day, ct.Hour, ct.Minute, ct.Second);
 				string path = Environment.CurrentDirectory + @"\"+ time;
				
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				con.recodeSavingPath = path;
			}
		}
    }

    public class BorderGrid : Grid
	{
		protected override void OnRender(DrawingContext dc)
		{
			double leftOffset = 0;
			double topOffset = 0;
			Pen pen = new Pen(Brushes.White, 0.4);
			pen.Freeze();

			foreach (RowDefinition row in this.RowDefinitions)
			{
				dc.DrawLine(pen, new Point(0, topOffset), new Point(this.ActualWidth, topOffset));
				topOffset += row.ActualHeight;
			}
			// draw last line at the bottom
			dc.DrawLine(pen, new Point(0, topOffset), new Point(this.ActualWidth, topOffset));

			foreach (ColumnDefinition column in this.ColumnDefinitions)
			{
				dc.DrawLine(pen, new Point(leftOffset, 0), new Point(leftOffset, this.ActualHeight));
				leftOffset += column.ActualWidth;
			}
			//draw last line on the right

			dc.DrawLine(pen, new Point(leftOffset, 0), new Point(leftOffset, this.ActualHeight));

			base.OnRender(dc);
		}
	}
}
