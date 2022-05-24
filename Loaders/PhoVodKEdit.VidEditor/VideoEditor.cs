using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using OpenCvSharp;
using PhoVodKEdit.Port;
using PhoVodKEdit.Port.APS;
using PhoVodKEdit.Port.Utilities;

namespace PhoVodKEdit.VidEditor {
	[FileTypes("MP4")]
	public class VideoEditor : PortScreen, INotifyPropertyChanged {
		#region Variables
		private string videoFile;
		private int framesCount;
		private uint maxBufferSize = 0x2FFFFFFF;
		private int maxFrameCount;
		private int actualBufferStart = 0;
		private int actualBufferEnd = 0;
		private BitmapImage actualFrame;

		private int actualFrameNum;
		private int vWidth;
		private int vHeight;
		private int vCapIndex;
		private VideoCapture vCapture;
		private Mat[] matBuffer;
		System.Media.SoundPlayer audio;

		private Timer frameTimer;
		private Timer lineTimer;
		private double fps;
		double preferredBordersOnScreen = 4;
		double secondsPerBorder = 2;
		double defBorderWidth = 300;
		double framesPerBorder;
		double pixelsPerFrame;

		private int selectedLayer;
		private bool underFrameCreation = false;
		private bool underBufferRefresh = false;
		#endregion

		internal List<VLayer> VideoLayers { get; private set; }

		#region Properties
		public bool Playing { get; private set; } = false;
		public BitmapImage ActualFrame {
			get {
				return actualFrame;
			}
			private set {
				actualFrame = value;
				OnPropertyChanged("ActualFrame");
			}
		}
		#endregion

		#region Public members
		public VideoEditor(AppliedSettings _applied) : base(_applied) {
			//ActualFrame = new BitmapImage(new Uri("C:\\Users\\Mika\\Pictures\\1ca2048715d76a26ac0040264ce3e24c.jpg"));
			this.OwnWindow = new VideoEditorWindow(this);
			TabName = videoFile;
		}

		public override System.Windows.Window CreateNewContent() {
			return new CreateNew();
		}

		public override void Refresh() {
			//TODO: throw new NotImplementedException();
		}

		public override bool Dispose() {
			return base.Dispose();
		}

		public override void SetContent(string contentPath) {
			videoFile = contentPath;
			
			vCapture = new VideoCapture(videoFile);
			framesCount = vCapture.FrameCount;
			vWidth = vCapture.FrameWidth;
			vHeight = vCapture.FrameHeight;
			fps = vCapture.Fps;
			secondsPerBorder = framesCount / preferredBordersOnScreen / fps;
			maxFrameCount = (int)(maxBufferSize / (vCapture.FrameWidth * vCapture.FrameHeight * 4));
			framesPerBorder = secondsPerBorder * fps;
			pixelsPerFrame = defBorderWidth / framesPerBorder;
			actualBufferEnd = maxFrameCount < framesCount ? maxFrameCount : framesCount;
			matBuffer = new Mat[framesCount];

			for (int i = 0; i < framesCount && i < maxFrameCount; i++) {
				matBuffer[i] = vCapture.RetrieveMat();
				vCapIndex = i;
			}

			//vCapture.Release();
			//vCapture.Dispose();
			//GC.Collect();

			string spFile = GrabSoundFile(contentPath);
			if (spFile != null) {
				audio = new System.Media.SoundPlayer(spFile);
			}

			GC.KeepAlive(vCapture);
			SetActurlFrame();
			InitializeBar();
			SetupTimers();
		}

		public void PlayVideo() {
			frameTimer.Start();
			lineTimer.Start();
			if (audio != null) audio.Play();
			Playing = true;
		}

		public void PauseVideo() {
			frameTimer.Stop();
			lineTimer.Stop();
			if (audio != null) audio.Stop();
			Playing = false;
		}
		#endregion
		#region Private members
		private void SetupTimers() {
			frameTimer = new Timer(1000 / fps);
			lineTimer = new Timer(1000 / fps);

			frameTimer.Elapsed += (s, e) => {
				actualFrameNum++;
				SetActurlFrame(actualFrameNum);
			};
			lineTimer.Elapsed += (s, e) => {
				(OwnWindow as VideoEditorWindow).SetPlayLine(actualFrameNum * pixelsPerFrame);
			};
		}

		private string GrabSoundFile(string contentPath) {
			if (!Directory.Exists("temp")) {
				Directory.CreateDirectory("temp");
			}

			string outFile = string.Concat("temp\\", (new Random()).Next(0, 99999999).ToString(), ".wav");
			string mp3out = string.Empty;
			string standardout = string.Empty;

			#region Check Audio
			var ffmpegProcess = new Process();
			ffmpegProcess.StartInfo.UseShellExecute = false;
			ffmpegProcess.StartInfo.RedirectStandardInput = true;
			ffmpegProcess.StartInfo.RedirectStandardOutput = true;
			ffmpegProcess.StartInfo.RedirectStandardError = true;
			ffmpegProcess.StartInfo.CreateNoWindow = true;
			ffmpegProcess.StartInfo.FileName = "C:\\ffmpeg\\bin\\ffprobe.exe";
			ffmpegProcess.StartInfo.Arguments = " -i " + contentPath + " -show_streams -select_streams a -loglevel error";
			ffmpegProcess.Start();
			standardout = ffmpegProcess.StandardOutput.ReadToEnd();
			mp3out = ffmpegProcess.StandardError.ReadToEnd();
			ffmpegProcess.WaitForExit();
			if (!ffmpegProcess.HasExited) {
				ffmpegProcess.Kill();
			}
			#endregion

			if (!standardout.Contains("index=1")) {
				return null;
			}

			#region Check Audio
			ffmpegProcess = new Process();
			ffmpegProcess.StartInfo.UseShellExecute = false;
			ffmpegProcess.StartInfo.RedirectStandardInput = true;
			ffmpegProcess.StartInfo.RedirectStandardOutput = true;
			ffmpegProcess.StartInfo.RedirectStandardError = true;
			ffmpegProcess.StartInfo.CreateNoWindow = true;
			ffmpegProcess.StartInfo.FileName = "C:\\ffmpeg\\bin\\ffmpeg.exe";
			ffmpegProcess.StartInfo.Arguments = " -i " + contentPath + " -vn -acodec pcm_s16le -ar 44100 -ac 2 " + outFile;
			ffmpegProcess.Start();
			standardout = ffmpegProcess.StandardOutput.ReadToEnd();
			mp3out = ffmpegProcess.StandardError.ReadToEnd();
			ffmpegProcess.WaitForExit();
			if (!ffmpegProcess.HasExited) {
				ffmpegProcess.Kill();
			}
			#endregion

			return outFile;
		}

		private void InitializeBar() {
			VideoLayers = new List<VLayer>();
			selectedLayer = 0;
			AddPanel(LayerType.Video);
			AddPanel(LayerType.Audio);
		}

		private void AddPanel(LayerType _lt, bool audioLenFromVideo = true) {
			VLayer vl = new VLayer(_lt);

			#region Init Layer
			Grid c_grid = new Grid();
			c_grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
			c_grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
			c_grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
			c_grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });

			// TODO: Define Controls ...

			vl.Control = new Border() {
				//Background = Brushes.Black,
				BorderThickness = new System.Windows.Thickness(0, 0, 0, 1),
				BorderBrush = Applied.Colors.Border,
				Child = c_grid
			};


			Grid p_grid = new Grid();

			StackPanel dock = new StackPanel() {
				Height = 80,
				Margin = new System.Windows.Thickness(0),
				Orientation = Orientation.Horizontal,
			};

			if (LayerType.Video == _lt) {
				for (double i = 0; i < framesCount; i += framesPerBorder) {
					var img = new System.Windows.Controls.Image() {
						HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
						Source = GetNthFrameUI((int)i)
					};

					dock.Children.Add(new Border() {
						BorderThickness = new System.Windows.Thickness(1, 0, 0, 0),
						BorderBrush = Applied.Colors.Border,
						Background = System.Windows.Media.Brushes.DarkGray,
						Width = framesPerBorder + i < framesCount ? defBorderWidth : 
							defBorderWidth * ((framesCount - i) / framesPerBorder),
						Child = img,
					});
				}
			}
			else if (LayerType.Audio == _lt) {
				if (audio != null) {
					if (audioLenFromVideo) {
						dock.Children.Add(new Border() {
							BorderThickness = new System.Windows.Thickness(1, 0, 0, 0),
							BorderBrush = Applied.Colors.Border,
							Background = System.Windows.Media.Brushes.DarkCyan,
							Width = framesCount / framesPerBorder * defBorderWidth
						});
					}
					else {
						double newWidth = audio.Stream.Length;
						throw new NotImplementedException();
					}
				}
				else {
					return;
				}
			}
			else {
				throw new NotImplementedException();
			}
			GridSplitter gs = new GridSplitter() {
				Background = Applied.Colors.Secondary,
				Visibility = Visibility.Visible,
				Margin = new System.Windows.Thickness(0),
				Height = 3,
				VerticalAlignment = System.Windows.VerticalAlignment.Center,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
			};
			Grid.SetRow(gs, 1);
			p_grid.Children.Add(dock);
			p_grid.Children.Add(new Grid() {
				RowDefinitions = {
					new RowDefinition() { Height = new GridLength(0), MaxHeight = 80 - gs.Height },
					new RowDefinition() { Height = GridLength.Auto },
					new RowDefinition(),
				},
				Children = {
					gs
				},
				MaxHeight = 80,
			});

			vl.Panel = new Border() {
				//Background = Brushes.Black,
				BorderThickness = new System.Windows.Thickness(0, 0, 0, 1),
				//Background	= Applied.Colors.Main,
				BorderBrush = Applied.Colors.Border,
				Child = p_grid
			};
			#endregion

			VideoLayers.Add(vl);
			(OwnWindow as VideoEditorWindow).ReloedLayers();
		}

		private void SetActurlFrame(int i = 0) {
			if (!underFrameCreation) {
				underFrameCreation = true;
				if (i >= 0 && i < framesCount) {
					actualFrameNum = i;
					Task.Factory.StartNew(() => RefreshBuffer());
					actualFrame = GetNthFrameUI(i);
				}
				else {
					ActualFrame = null;
				};
				(OwnWindow as VideoEditorWindow).UpdateFrame();
				underFrameCreation = false;
			}
		}

		private MemoryStream GetBufferMemoryStreamAt(int i, bool reallyNeedIt = false) {
			if (reallyNeedIt) {
				RefreshBuffer(i);
			}
			else {
				int cnt = 0;
				while (!(i >= actualBufferStart && i < actualBufferEnd)) {
					if (cnt > 500) {
						return GetBufferMemoryStreamAt(i, true);
					}
					//if (!underBufferRefresh) {
						RefreshBuffer();
						cnt++;
					//}
				}
			}

			return matBuffer[i].ToMemoryStream();
		}

		private void RefreshBuffer(int index = -1) {
			if (index >= 0 && index < framesCount) {
				while (underBufferRefresh) { }
				underBufferRefresh = true;
				vCapture = new VideoCapture(videoFile);
				for (int i = 0; i < index; i++) {
					vCapture.RetrieveMat();
					vCapIndex = i;
					GC.Collect();
				}
				matBuffer[index] = vCapture.RetrieveMat();
				vCapIndex++;
				underBufferRefresh = false;
				GC.Collect();
				return;
			}
			if (actualBufferStart == actualFrameNum) return;
			if (!underBufferRefresh) {
				underBufferRefresh = true;
				if (actualBufferStart < actualFrameNum) {
					int bufDif = actualFrameNum - actualBufferStart;
					int newBufEnd = actualFrameNum + maxFrameCount;
					if (newBufEnd > framesCount) {
						bufDif -= newBufEnd - framesCount;
						newBufEnd = framesCount;
					}
					if (bufDif <= 0) {
						underBufferRefresh=false;
						return;
					}

					for (int i = 0; i < actualFrameNum; i++) {
						matBuffer[i] = null;
					}
					GC.Collect();
					//for (int i = 0; i < actualFrameNum; i++) {
					//	vCapture.RetrieveMat();
					//	GC.Collect();
					//}

					for (int i = actualBufferEnd; i < newBufEnd; i++) {
						matBuffer[i] = vCapture.RetrieveMat();
						vCapIndex = i;
					}

					actualBufferStart = actualFrameNum;
					actualBufferEnd = newBufEnd;
				}
				else {
					vCapture = new VideoCapture(videoFile);
					GC.KeepAlive(vCapture);
					matBuffer = new Mat[framesCount];
					GC.Collect();
					int newBufEnd = actualFrameNum + maxFrameCount;
					if (newBufEnd > framesCount) {
						newBufEnd = framesCount;
					}

					for (int i = 0; i < actualFrameNum; i++) {
						vCapture.RetrieveMat();
						vCapIndex = i;
						GC.Collect();
					}

					for (int i = actualFrameNum; i < newBufEnd; i++) {
						matBuffer[i] = vCapture.RetrieveMat();
						vCapIndex = i;
					}

					actualBufferStart = actualFrameNum;
					actualBufferEnd = newBufEnd;
				}
				GC.Collect();
				underBufferRefresh=false;
			}
		}

		private BitmapImage GetNthFrameUI(int i) {
			if (i >= 0 && i < framesCount) {
				MemoryStream ms;
				if (this.Layers.Count > 0) {
					Bitmap bmp = new Bitmap(GetBufferMemoryStreamAt(i));

					foreach (Layer layer in Layers) {
						if (layer.Rendered) {
							foreach (PortEffect effect in layer.Effects) {
								try {
									if (effect.Rendered) effect.Apply(bmp, bmp.PixelFormat);//PictureExt == "png" ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);
								}
								catch (Exception ex) { effect.CatchedException = ex; }
							}
						}
					}

					ms = new MemoryStream();
					bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
				}
				else {
					ms = GetBufferMemoryStreamAt(actualFrameNum);
				}

				var frame = new BitmapImage();
				frame.BeginInit();
				frame.StreamSource = ms;
				frame.CacheOption = BitmapCacheOption.OnLoad;
				frame.EndInit();
				frame.Freeze();
				return frame;
			}
			return null;
		}
		#endregion

		protected override void ApplyEffects() {
			SetActurlFrame(actualFrameNum);
		}

		internal void SetTimespanFromControlGrid(double gs) {
			actualFrameNum = (int)(gs / pixelsPerFrame);
			Task.Factory.StartNew(() => SetActurlFrame(actualFrameNum));
		}

		#region INotifiedProperty Block
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null) {
			if (!Equals(field, newValue)) {
				field = newValue;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
				return true;
			}

			return false;
		}

		public override void SaveToFile(string filePath) {
			throw new NotImplementedException();
		}
		#endregion
	}
}
