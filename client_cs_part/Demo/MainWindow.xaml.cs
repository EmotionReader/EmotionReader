using System;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System.Windows.Media.Animation;
using WinForms = System.Windows.Forms;

namespace EmotionReader
{
	public partial class MainWindow : Window
	{
		#region Member Variables
		private Control con = Control.Instance;
		public double CurrentProgress
		{
			get { return progressbar.Value; }
			set { Dispatcher.Invoke(new Action(() => { progressbar.Value = value; })); }
		}
		public Visibility CurrentProgress_Visibility
		{
			get { return progressbar.Visibility; }
			set { Dispatcher.Invoke(new Action(() => { progressbar.Visibility = value; })); }
		}
		private List<int> list_facecodes = new List<int>(); // Merge & Delete 				
		private bool sldrDragStart = false;
		private double Keep_sound;
		private bool play = true;
		private bool sound = true;

		#region Progress Bar (Live - Model2)
		public double LiveBar_Anger
		{
			get { return live_anger.Value; }
			set { Dispatcher.Invoke(new Action(() => { UpdateProgressBarSmooth(live_anger, value); })); }
		}
		public double LiveBar_Disgust
		{
			get { return live_disgust.Value; }
			set { Dispatcher.Invoke(new Action(() => { UpdateProgressBarSmooth(live_disgust, value); })); }
		}
		public double LiveBar_Fear
		{
			get { return live_fear.Value; }
			set { Dispatcher.Invoke(new Action(() => { UpdateProgressBarSmooth(live_fear, value); })); }
		}
		public double LiveBar_Happy
		{
			get { return live_happy.Value; }
			set { Dispatcher.Invoke(new Action(() => { UpdateProgressBarSmooth(live_happy, value); })); }
		}
		public double LiveBar_Neutral
		{
			get { return live_neutral.Value; }
			set { Dispatcher.Invoke(new Action(() => { UpdateProgressBarSmooth(live_neutral, value); })); }
		}
		public double LiveBar_Sad
		{
			get { return live_sad.Value; }
			set { Dispatcher.Invoke(new Action(() => { UpdateProgressBarSmooth(live_sad, value); })); }
		}
		public double LiveBar_Surprise
		{
			get { return live_surprise.Value; }
			set { Dispatcher.Invoke(new Action(() => { UpdateProgressBarSmooth(live_surprise, value); })); }
		}
		#endregion

		#region Pie Chart
		public SeriesCollection Live_SeriesCollection { get; set; }
		public PieSeries Live_Anger { get; set; }
		public PieSeries Live_Disgust { get; set; }
		public PieSeries Live_Fear { get; set; }
		public PieSeries Live_Happy { get; set; }
		public PieSeries Live_Neutral { get; set; }
		public PieSeries Live_Sad { get; set; }
		public PieSeries Live_Surprise { get; set; }
		#endregion

		#endregion

		public MainWindow()
		{
			con.PathSetting();		//control에서 사용 되는 주소 가져오기
			InitializeComponent();	//mainwindow initialize
			con.mainwindow = this;	//control에서 mainwindow UI에 접근 하기위함
			progressbar.Value = 0;  //progressbar Value 초기화 			
			PieChartInitiailze();   //PieChart Initiailze
			DataContext = this;		//piechart 위해 binding 진행
		}

		#region Live Pie Chart 

		private void PieChartInitiailze()
		{
			Live_SeriesCollection = new SeriesCollection();

			Live_Anger = new PieSeries()
			{
				Title = "Anger",
				Values = new ChartValues<ObservableValue> { new ObservableValue(1) },
				DataLabels = true
			};
			Live_Disgust = new PieSeries()
			{
				Title = "Disgust",
				Values = new ChartValues<ObservableValue> { new ObservableValue(0) },
				DataLabels = true
			};
			Live_Fear = new PieSeries()
			{
				Title = "Fear",
				Values = new ChartValues<ObservableValue> { new ObservableValue(0) },
				DataLabels = true
			};
			Live_Happy = new PieSeries()
			{
				Title = "Happy",
				Values = new ChartValues<ObservableValue> { new ObservableValue(0) },
				DataLabels = true
			};
			Live_Neutral = new PieSeries()
			{
				Title = "Neutral",
				Values = new ChartValues<ObservableValue> { new ObservableValue(0) },
				DataLabels = true
			};
			Live_Sad = new PieSeries()
			{
				Title = "Sad",
				Values = new ChartValues<ObservableValue> { new ObservableValue(0) },
				DataLabels = true
			};
			Live_Surprise = new PieSeries()
			{
				Title = "Surprise",
				Values = new ChartValues<ObservableValue> { new ObservableValue(0) },
				DataLabels = true
			};

			Live_SeriesCollection.Add(Live_Anger);
			Live_SeriesCollection.Add(Live_Disgust);
			Live_SeriesCollection.Add(Live_Fear);
			Live_SeriesCollection.Add(Live_Happy);
			Live_SeriesCollection.Add(Live_Neutral);
			Live_SeriesCollection.Add(Live_Sad);
			Live_SeriesCollection.Add(Live_Surprise);
		}
		public void ChangePieChart(int idx)
		{
			Dispatcher.Invoke(new Action(() =>
			{
				switch (idx)
				{
					case 0: Live_Anger.Values.Cast<ObservableValue>().First().Value += 1; break;
					case 1: Live_Disgust.Values.Cast<ObservableValue>().First().Value += 1; break;
					case 2: Live_Fear.Values.Cast<ObservableValue>().First().Value += 1; break;
					case 3: Live_Happy.Values.Cast<ObservableValue>().First().Value += 1; break;
					case 4: Live_Neutral.Values.Cast<ObservableValue>().First().Value += 1; break;
					case 5: Live_Sad.Values.Cast<ObservableValue>().First().Value += 1; break;
					case 6: Live_Surprise.Values.Cast<ObservableValue>().First().Value += 1; break;
					case 7: 
						Live_Anger.Values.Cast<ObservableValue>().First().Value = 0;
						Live_Disgust.Values.Cast<ObservableValue>().First().Value = 0;
						Live_Fear.Values.Cast<ObservableValue>().First().Value = 0;
						Live_Happy.Values.Cast<ObservableValue>().First().Value = 0;
						Live_Neutral.Values.Cast<ObservableValue>().First().Value = 0;
						Live_Sad.Values.Cast<ObservableValue>().First().Value = 0;
						Live_Surprise.Values.Cast<ObservableValue>().First().Value = 0;
						break;
				}
			}));
		}

		#endregion

		#region Progress Bar Smoother
		private void UpdateProgressBarSmooth(ProgressBar progressbar, double value)
		{
			Duration duration = new Duration(TimeSpan.FromMilliseconds(150));
			DoubleAnimation doubleanimation = new DoubleAnimation(value, duration);
			progressbar.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);
		}
		#endregion

		#region video player
		private void MediaMain_MediaOpened(object sender, RoutedEventArgs e)
		{
			// 미디어 파일이 열리면, 플레이타임 슬라이더의 값을 초기화 한다.
			sldrPlayTime.Minimum = 0;
			sldrPlayTime.Maximum = mediaMain.NaturalDuration.TimeSpan.TotalSeconds;
			slider_editor.Minimum = 0;
			slider_editor.Maximum = mediaMain.NaturalDuration.TimeSpan.TotalSeconds; 
		}
		private void MediaMain_MediaEnded(object sender, RoutedEventArgs e)
		{
			// 미디어 중
			mediaMain.Stop();
		}
		private void MediaMain_MediaFailed(object sender, ExceptionRoutedEventArgs e)
		{
			// 미디어 파일 실행 오류시
			MessageBox.Show("동영상 재생 실패 : " + e.ErrorException.Message.ToString());
		}
		private void Media_Play_Pause(string play_or_pause)
		{
			if (play_or_pause == "play")
			{
				mediaMain.Play();
				var brush = new ImageBrush();
				string path = con.ProjectPath + @"Resources\Pause.png";
				var uri = new System.Uri(path);
				var converted = uri.AbsoluteUri;
				brush.ImageSource = new BitmapImage(new Uri(converted));
				btnStart.Background = brush;
				play = true;
			}
			else if(play_or_pause == "pause")
			{
				mediaMain.Pause();
				var brush = new ImageBrush();
				string path = con.ProjectPath + @"Resources\Play.png";
				var uri = new System.Uri(path);
				var converted = uri.AbsoluteUri;
				brush.ImageSource = new BitmapImage(new Uri(converted));
				btnStart.Background = brush;
				play = false;
			}
		}
		// 미디어파일 타임 핸들러
		// 미디어파일의 실행시간이 변경되면 호출된다.
		private void TimerTickHandler(object sender, EventArgs e)
		{
			// 미디어파일 실행시간이 변경되었을 때 사용자가 임의로 변경하는 중인지를 체크한다.
			if (sldrDragStart)
				return;
			if (mediaMain.Source == null || !mediaMain.NaturalDuration.HasTimeSpan)
			{
				lblPlayTime.Content = "No file selected...";
				return;
			}
			// 미디어 파일 총 시간을 슬라이더와 동기화한다.
			sldrPlayTime.Value = mediaMain.Position.TotalSeconds;
		}

		private void BtnStart_Click(object sender, RoutedEventArgs e)
		{
			if (mediaMain.Source == null)
				return;
			if (play == false)
				Media_Play_Pause("play");
			else
				Media_Play_Pause("pause");
		}

		private void BtnStop_Click(object sender, RoutedEventArgs e)
		{
			if (mediaMain.Source == null)
				return;
			Media_Play_Pause("pause");
		}

		private void BtnPause_Click(object sender, RoutedEventArgs e)
		{
			if (mediaMain.Source == null)
				return;
			mediaMain.Pause();
		}

		private void SldrVolume_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
		{
			// 사용자가 변경한 볼륨값으로 미디어 볼륨값을 변경한다.
			mediaMain.Volume = sldrVolume.Value;
		}
		private void sldrVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			mediaMain.Volume = sldrVolume.Value;
			if (Keep_sound != 0)
			{
				sound = true;
			}
			if (sldrVolume.Value == 0)
			{
				var brush = new ImageBrush();
				string path = con.ProjectPath + @"Resources\SoundOff.png";
				var uri = new System.Uri(path);
				var converted = uri.AbsoluteUri;
				brush.ImageSource = new BitmapImage(new Uri(converted));
				soundimage.Background = brush;
			}
			else
			{
				var brush = new ImageBrush();
				string path = con.ProjectPath + @"Resources\SoundOn.png";
				var uri = new System.Uri(path);
				var converted = uri.AbsoluteUri;
				brush.ImageSource = new BitmapImage(new Uri(converted));
				soundimage.Background = brush;
			}
		}
		private void Label_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (sound == true)
			{
				Keep_sound = sldrVolume.Value;
				mediaMain.Volume = 0;
				sldrVolume.Value = 0;
				sound = false;
			}
			else if (sound == false)
			{
				sound = true;
				sldrVolume.Value = Keep_sound;
				Keep_sound = 0;
			}
		}

		private void mediaMain_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (play == false)
				Media_Play_Pause("play");
			else
				Media_Play_Pause("pause");
		}

		private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Space)
			{
				if (play == false)				
					Media_Play_Pause("play");				
				else				
					Media_Play_Pause("pause");				
			}
			if (e.Key == System.Windows.Input.Key.OemPeriod)			
				mediaMain.Position = TimeSpan.FromSeconds(sldrPlayTime.Value + 10);	

			if (e.Key == System.Windows.Input.Key.OemComma)			
				mediaMain.Position = TimeSpan.FromSeconds(sldrPlayTime.Value - 10);

			if (e.Key == System.Windows.Input.Key.Up)			
				mediaMain.Volume = (double)(sldrVolume.Value + 0.1);	

			if (e.Key == System.Windows.Input.Key.Down)			
				mediaMain.Volume = (double)(sldrVolume.Value - 0.1);
		}

		#region Slider Control
		private void SldrPlayTime_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
		{
			// 사용자가 시간대를 변경하면, 잠시 미디어 재생을 멈춘다.
			sldrDragStart = true;
			mediaMain.Pause();		  
		}

		private void SldrPlayTime_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
		{
			// 사용자가 지정한 시간대로 이동하면, 이동한 시간대로 값을 지정한다.
			mediaMain.Position = TimeSpan.FromSeconds(sldrPlayTime.Value);
			Media_Play_Pause("play");// 멈췄던 미디어를 재실행한다.			
			sldrDragStart = false;
		}

		private void SldrPlayTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (sldrPlayTime == null)
				return;
			//SliderBar와 timelinebar 동기화
			double ins = ((sldrPlayTime.ActualWidth - 18) * sldrPlayTime.Value) / sldrPlayTime.Maximum;
			Canvas.SetLeft(timelinebar, ins);
			int face_idx;

			#region Bar Chart와 동기화
			// Face 선택 (Default : 0 번째 얼굴)
			if (con.Vinfo != null && con.Vinfo.Faces.Count != 0)
			{
				if (list_facecodes.Count > 0)
					face_idx = con.Vinfo.Get_Face_code_idx(list_facecodes.Last());
				else
					face_idx = 0;

				// 현재 슬라이더가 있는 시간의 프레임 넘버
				int current_frame = Convert.ToInt32(sldrPlayTime.Value / 100 * con.Vinfo.Frame_max_count);

				// 선택된 얼굴의 현재 프레임의 FaceDetail index 가져오기
				int frameidx = con.Vinfo.Faces[face_idx].Get_Frame_No_idx(current_frame);

				// 만약 그 프레임의 데이터가 있다면 Bar Chart 수정
				if (frameidx != -1)
				{
					FaceDetail facedetail = con.Vinfo.Faces[face_idx].Face_details[frameidx];
					UpdateProgressBarSmooth(facedetail);
				}
			}
			#endregion
			if (mediaMain.Source == null)
				return;

			// 플레이시간이 변경되면, 표시영역을 업데이트한다.
			lblPlayTime.Content = String.Format("{0} / {1}", mediaMain.Position.ToString(@"mm\:ss"), mediaMain.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
 
			if((mediaMain.Position > TimeSpan.FromSeconds(slider_editor.ValueEnd)) && (bool)edit_checkbox.IsChecked)
			{
				BtnPause_Click(null, null);
				sldrPlayTime.Value = slider_editor.ValueEnd;
				SldrPlayTime_DragCompleted(sender, null);
				Media_Play_Pause("pause");
			}
			if ((mediaMain.Position < TimeSpan.FromSeconds(slider_editor.ValueStart)) && (bool)edit_checkbox.IsChecked)
			{
				BtnPause_Click(null, null);
				sldrPlayTime.Value = slider_editor.ValueStart;
				SldrPlayTime_DragCompleted(sender, null);
			}
		}

		private void UpdateProgressBarSmooth(FaceDetail facedetail)
		{
			Duration duration = new Duration(TimeSpan.FromMilliseconds(150));

			DoubleAnimation doubleanimation = new DoubleAnimation(facedetail.Current_Anger, duration);
			anger.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);

			doubleanimation = new DoubleAnimation(facedetail.Current_Disgust, duration);
			disgust.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);

			doubleanimation = new DoubleAnimation(facedetail.Current_Fear, duration);
			fear.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);

			doubleanimation = new DoubleAnimation(facedetail.Current_Happy, duration);
			happy.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);

			doubleanimation = new DoubleAnimation(facedetail.Current_Neutral, duration);
			neutral.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);

			doubleanimation = new DoubleAnimation(facedetail.Current_Sad, duration);
			sad.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);

			doubleanimation = new DoubleAnimation(facedetail.Current_Surprise, duration);
			surprise.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);
		}
		#endregion

		

		private void btnM10sec_Click(object sender, RoutedEventArgs e)
		{
			mediaMain.Position = TimeSpan.FromSeconds(sldrPlayTime.Value - 10);
		}

		private void btnP10sec_Click(object sender, RoutedEventArgs e)
		{

			mediaMain.Position = TimeSpan.FromSeconds(sldrPlayTime.Value + 10);
		}

		#endregion

		#region Window Load & Close

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (con.Init() == true)			
				con.Call_Python();	//python 코드 실행
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Environment.Exit(0);
			System.Diagnostics.Process.GetCurrentProcess().Kill();
			con.t.Abort();

			this.Close();   // python thread 종료				
		}

		#endregion

		#region slider bar

		#region ScrollViewer (Make Slides)
		private void MakeSlider(VideoInfo vinfo)
		{
			if (vinfo == null)
			{
				MessageBox.Show("load video data file error");
				return;
			}
			double fps = vinfo.Fps;
			int frame_gap = (int)(vinfo.Frame_Gap);
			int max_count = vinfo.Frame_max_count;

			slide_area.Children.Clear();
			double stackpanelmargin = 0;
			foreach (var face in vinfo.Faces)
			{
				StackPanel stackPanel = new StackPanel();
				stackPanel.Height = 50;
				stackPanel.Orientation = Orientation.Horizontal;
				
				stackpanelmargin += 50;

				Button headimage = new Button();
				var brush = new ImageBrush();
				string path = vinfo.VideoPath;
				path = path + "_" + face.Face_code.ToString() + ".png";
				var uri = new System.Uri(path);
				var converted = uri.AbsoluteUri;
				brush.ImageSource = new BitmapImage(new Uri(converted));
				headimage.Background = brush;
				headimage.Height = 50;
				headimage.Width = 50;
				headimage.HorizontalAlignment = HorizontalAlignment.Left;

				headimage.Content = string.Format(string.Format("                   face_{0}_false", face.Face_code));
				headimage.Name = string.Format("face_{0}", face.Face_code);
				headimage.Click += new RoutedEventHandler(ButtonCreatedFace_Click);
				stackPanel.Children.Add(headimage);

				Button current_button = null;
				double sliderlenth = sldrPlayTime.ActualWidth - 18;
				for (int i = 0; i < face.Face_details.Count; i++)
				{
					var face_detail = face.Face_details[i];
					if (i != 0 &&
					   (face.Face_details[i - 1].Frame_no == (face_detail.Frame_no - frame_gap)) &&
					   (face.Face_details[i - 1].Emotion_code == face_detail.Emotion_code))
					{
						current_button.Width += (double)frame_gap / max_count * sliderlenth;
						current_button.Content += string.Format("^{0}", face_detail.Frame_no);
					}
					else
					{
						current_button = new Button();						
						HandyControl.Controls.BorderElement.SetCornerRadius(current_button, new CornerRadius(0));
						current_button.BorderThickness = new Thickness(0);
						current_button.Height = 50;
						current_button.Width = (double)frame_gap / max_count * sliderlenth;
						double margin_left = i == 0 ? (double)face_detail.Frame_no / max_count * sliderlenth : (double)(face_detail.Frame_no - face.Face_details[i - 1].Frame_no - frame_gap) / max_count * sliderlenth;
						current_button.Margin = new Thickness(margin_left, 0, 0, 0);
						current_button.HorizontalAlignment = HorizontalAlignment.Left;
						switch (face_detail.Emotion_code)
						{
							case 0: current_button.Foreground = current_button.Background = 
									(SolidColorBrush)new BrushConverter().ConvertFrom("#FF6666"); break;
							case 1: current_button.Foreground = current_button.Background = 
									(SolidColorBrush)new BrushConverter().ConvertFrom("#33FF33"); break;
							case 2: current_button.Foreground = current_button.Background = 
									(SolidColorBrush)new BrushConverter().ConvertFrom("#3333FF"); break;
							case 3: current_button.Foreground = current_button.Background = 
									(SolidColorBrush)new BrushConverter().ConvertFrom("#FFFF66"); break;
							case 4: current_button.Foreground = current_button.Background = 
									(SolidColorBrush)new BrushConverter().ConvertFrom("#FFFF00"); break;
							case 5: current_button.Foreground = current_button.Background = 
									(SolidColorBrush)new BrushConverter().ConvertFrom("#9999FF"); break;
							case 6: current_button.Foreground = current_button.Background = 
									(SolidColorBrush)new BrushConverter().ConvertFrom("#FF9999"); break;

							default: current_button.Background = Brushes.Black; break;


						}
						current_button.Content = string.Format("^{0}^{1}", face_detail.Frame_no, face_detail.Frame_no);
						//current_button.
						current_button.Click += new RoutedEventHandler(ButtonCreatedByCode_Click);
						stackPanel.Children.Add(current_button);
					}
				}
				slide_area.Children.Add(stackPanel);
			}
		}
		public void MakeSlide_D(VideoInfo vinfo)
		{
			Dispatcher.Invoke(new Action(() => MakeSlider(vinfo)));
		}
		#endregion

		#region Made Button, CheckBox Events
		private void ButtonCreatedByCode_Click(object sender, RoutedEventArgs e)
		{
			var keyword = (e.Source as Button).Content.ToString();
			string[] sp = keyword.Split('^');

			int fn = int.Parse(sp[1]);
			int en = int.Parse(sp[sp.Length - 1]);

			int fmc = con.Vinfo.Frame_max_count;

			mediaMain.Position = TimeSpan.FromSeconds(fn / con.Vinfo.Fps);

			if(edit_checkbox.IsChecked == true)
			{
				slider_editor.ValueStart = (double)fn / con.Vinfo.Fps;
				slider_editor.ValueEnd = (double)(en + con.Vinfo.Frame_Gap) / con.Vinfo.Fps;
			}
			//MessageBox.Show(keyword);
		}
		// Button - Face_code
		private void ButtonCreatedFace_Click(object sender, RoutedEventArgs e)
		{
			//Button facebutton = (e.Source as Button);
			Button facebutton = sender as Button;
			var truefalse1 = facebutton.Content.ToString().Trim();

			//face_{0}_false
			string[] sp = truefalse1.Split('_');
			int facecode = int.Parse(sp[1]);
			string truefalse = sp[2];

			if (truefalse == "false")
			{
				facebutton.Content = string.Format(string.Format("                   face_{0}_true", facecode));
				facebutton.Opacity = 0.3;
				list_facecodes.Add(facecode);
				// Sort 안하면 오류 생김 (list_facecodes.Sort()는 Merge 하기 직전에 함)
			}
			else if (truefalse == "true")
			{
				facebutton.Content = string.Format(string.Format("                   face_{0}_false", facecode));
				facebutton.Opacity = 1;
				for (int i = 0; i < list_facecodes.Count; i++)
				{
					if (list_facecodes[i] == facecode)
					{
						list_facecodes.RemoveAt(i);
						break;
					}
				}
			}
			Media_Play_Pause("play");
		}

		#endregion
		#endregion

		#region Slider Editor Bar

		private void Slider_editor_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<HandyControl.Data.DoubleRange> e)
		{
			double temp = slider_editor.ValueStart / slider_editor.Maximum * (slider_editor.ActualWidth - 18) + 1;
			double edit_timelinebar_width = (slider_editor.ValueEnd - slider_editor.ValueStart) / slider_editor.Maximum * (slider_editor.ActualWidth - 18);
			if (edit_timelinebar == null)
				return;
			Canvas.SetLeft(edit_timelinebar, temp);
			edit_timelinebar.Width = Math.Abs(edit_timelinebar_width);

			SldrPlayTime_ValueChanged(null, null);
		}
		#endregion

		#region MenuItem
		private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
		{
			// Win32 DLL 을 사용하여 선택할 파일 다이얼로그를 실행한다.
			OpenFileDialog dlg = new OpenFileDialog()
			{
				DefaultExt = ".avi",
				Filter = "All files (*.*)|*.*",
				Multiselect = false
			};

			if (dlg.ShowDialog() == true)
			{
				con.SendDirectory(dlg.FileName);

				// 선택한 파일을 Media Element에 지정하고 초기화한다.
				mediaMain.Source = new Uri(dlg.FileName);
				mediaMain.Volume = 0.5;
				mediaMain.SpeedRatio = 1;

				// 동영상 파일의 Timespan 제어를 위해 초기화와 이벤트처리기를 추가한다.
				DispatcherTimer timer = new DispatcherTimer()
				{
					Interval = TimeSpan.FromSeconds(1)
				};
				timer.Tick += TimerTickHandler;
				timer.Start();

				// 선택한 파일을 실행

				progressbar.Visibility = Visibility.Visible;

				// Btn Start Setting
				Media_Play_Pause("play");
			}
		}
		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog()
			{				
				Filter = "All files (*.*)|*.*",
				Multiselect = false
			};
			if (dlg.ShowDialog() == true)
			{
				con.FileRead(dlg.FileName);
				//MakeSlider(con.Vinfo);
				string videoPath = con.Vinfo.VideoPath;
				mediaMain.Source = new Uri(videoPath);
				mediaMain.Volume = 0.5;
				mediaMain.SpeedRatio = 1;
				// 동영상 파일의 Timespan 제어를 위해 초기화와 이벤트처리기를 추가한다.
				DispatcherTimer timer = new DispatcherTimer()
				{
					Interval = TimeSpan.FromSeconds(1)
				};
				timer.Tick += TimerTickHandler;
				timer.Start();
				// 선택한 파일을 실행
				Media_Play_Pause("play");
				//MediaMain_MediaOpened(null, null);
			}
		}
		private void MenuItem_Click_1(object sender, RoutedEventArgs e)
		{
			//옵션창 띄우는거
			EmotionReader.option tw = new EmotionReader.option();
			tw.ShowDialog();
		}
		private void ClostItem_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		#endregion

		#region Update LogMessage
		private void UpdateLogMsg(string msg, int flag)
		{
			logmsg.Text = msg;

			switch (flag)
			{
				case 0: logrect.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF"); break;// None    - 빨강색
				case 1: logrect.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#6B6B6B"); break;// START   -
				case 2: logrect.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#FC0000"); break;// STOP    - 
				case 3: logrect.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#00CC00");
					menuitem_Video_Analysis.IsEnabled = true;
					break;// CONNECT - 녹색
				case 4: logrect.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#FC0000");
					menuitem_Video_Analysis.IsEnabled = false;
					break;// DISCONN - 
				case 5: logrect.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFB441"); break;// ERROR   - 

				default: logrect.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#696969"); break; // 회색
			}
		}

		public void UpdateLogMsg_D(string msg, int flag)
		{
			Dispatcher.Invoke(new Action(() => UpdateLogMsg(msg, flag)));
		}
		#endregion

		#region edit function
		private void CheckBox_Checked(object sender, RoutedEventArgs e)		//show edit buttons
		{
			button_save_selecte_dvideo.Visibility = Visibility.Visible;
			slider_editor.Visibility = Visibility.Visible;
			edit_timelinebar.Visibility = Visibility.Visible;
			btn_delete.Visibility = Visibility.Visible;
			btn_merge.Visibility = Visibility.Visible;
			btn_save.Visibility = Visibility.Visible;
		}
		private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			button_save_selecte_dvideo.Visibility = Visibility.Hidden;
			slider_editor.Visibility = Visibility.Hidden;
			edit_timelinebar.Visibility= Visibility.Hidden;
			btn_delete.Visibility = Visibility.Hidden;
			btn_merge.Visibility = Visibility.Hidden;
			btn_save.Visibility = Visibility.Hidden;
		}
		private void button_save_selecte_dvideo_Click(object sender, RoutedEventArgs e)
		{
			double start = slider_editor.ValueStart;
			double end = slider_editor.ValueEnd;
			con.SendCutVid(start.ToString() + "#" + end.ToString() + "#" + con.Vinfo.VideoPath);
		}
		#endregion


		private void Slider_editor_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
		{
			if (sldrPlayTime == null)
			{
				return;
			}
			if (sldrPlayTime.Value > slider_editor.ValueStart && sldrPlayTime.Value > slider_editor.ValueEnd)
			{
				sldrPlayTime.Value = slider_editor.ValueStart;
				SldrPlayTime_DragCompleted(sender, null);
			}
		}

		private void btn_merge_Click(object sender, RoutedEventArgs e)
		{
			if (list_facecodes.Count <= 1)
				return;

			list_facecodes.Sort();

			int first_face_code = con.Vinfo.Get_Face_code_idx(list_facecodes[0]);

			for (int i = 1; i < list_facecodes.Count; i++)
			{
				int following_face_code = con.Vinfo.Get_Face_code_idx(list_facecodes[i]);


				List<FaceDetail> list_followingface = con.Vinfo.Faces[following_face_code].Face_details;

				for (int j = 0; j < list_followingface.Count; j++)
				{
					con.Vinfo.Faces[first_face_code].Face_details.Add(list_followingface[j]);
				}

				//Delete Face
				con.Vinfo.Faces.RemoveAt(following_face_code);
			}

			// Vinfo.Face[first_face_code].Face_details 오름차순으로 정렬
			con.Vinfo.Faces[first_face_code].Sort_Face_details();

			list_facecodes.Clear();

			MessageBox.Show("Successfully Merged");

			MakeSlider(con.Vinfo);
		}

		private void btn_delete_Click(object sender, RoutedEventArgs e)
		{
			for (int i = 0; i < list_facecodes.Count; i++)
			{
				int idx = con.Vinfo.Get_Face_code_idx(list_facecodes[i]);
				con.Vinfo.Faces.RemoveAt(idx);
			}

			list_facecodes.Clear();

			MessageBox.Show("Successfully Deleted");

			MakeSlider(con.Vinfo);
		}

		private void btn_save_Click(object sender, RoutedEventArgs e)
		{
			con.SaveFile();
		}

		bool Record_State = false;
		bool Live_State = false;

		private void btn_record_Click(object sender, RoutedEventArgs e)
		{
			if (Record_State == false)
			{
				if (Live_State == false)
				{
					MessageBox.Show("실시간 분석을 먼저 실행시켜주세요.");
					return;
				}

				MessageBox.Show("저장할 폴더 경로를 선택하세요");

				WinForms.FolderBrowserDialog folderDialog = new WinForms.FolderBrowserDialog();
				folderDialog.ShowNewFolderButton = false;
				folderDialog.SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory;
				WinForms.DialogResult result = folderDialog.ShowDialog();
				if (result == WinForms.DialogResult.OK)
				{					
					string sPath = folderDialog.SelectedPath;

					DirectoryInfo folder = new DirectoryInfo(sPath);
					if (folder.Exists)
					{
						con.SendRecordStart(sPath);
					}

					var brush = new ImageBrush();
					string path = con.ProjectPath + @"Resources\RecordOn.png";
					var uri = new System.Uri(path);
					var converted = uri.AbsoluteUri;
					brush.ImageSource = new BitmapImage(new Uri(converted));
					On_Off_Record.Background = brush;
					Record_State = true;

					ChangePieChart(7);
				}
			}
			else if (Record_State == true)
			{
				con.SendRecordEnd();

				var brush = new ImageBrush();
				string path = con.ProjectPath + @"Resources\RecordStop.png";
				var uri = new System.Uri(path);
				var converted = uri.AbsoluteUri;
				brush.ImageSource = new BitmapImage(new Uri(converted));
				On_Off_Record.Background = brush;

				Record_State = false;
			}
		}

	
		private void btn_live_on_Click(object sender, RoutedEventArgs e)
		{
			
			con.SendLiveStart(); 
			Live_State = true;
			btn_live_on.IsEnabled = false;
			btn_live_off.IsEnabled = true;



		}
		private void btn_live_off_Click(object sender, RoutedEventArgs e)
		{
			
				if (Record_State == true)
				{
					MessageBox.Show("녹화 중입니다.\n녹화 정지를 먼저 눌러주세요.");
					return;
				}
			con.SendLiveEnd();
			Live_State = false;
			btn_live_on.IsEnabled = true;
			btn_live_off.IsEnabled = false;
			ChangePieChart(7);
		}


		private void mode1_Checked(object sender, RoutedEventArgs e)
		{
			if (mode_1 == null)
				return;
			MainWin.Height = 975;
			MainWin.Width = 1390;
			mode_1.Visibility = Visibility.Visible;
			mode_2.Visibility = Visibility.Hidden;
			MainWin.Title = "EmotionReader-video";
		}

		private void mode2_Checked(object sender, RoutedEventArgs e)
		{
			if (mode_1 == null)
				return;
			MainWin.Height = 500;
			MainWin.Width = 800;
			mode_1.Visibility = Visibility.Hidden;
			mode_2.Visibility = Visibility.Visible;
			MainWin.Title = "EmotionReader-live";
		}

	  
	}
}
