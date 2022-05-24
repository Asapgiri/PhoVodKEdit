using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PhoVodKEdit.Loader;
using PhoVodKEdit.Port;
using PhoVodKEdit.Port.APS;
using PhoVodKEdit.Port.Utilities;
using PhoVodKEdit.VidEditor;

namespace PhoVodKEdit
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private readonly ResourceLoader resourceLoader;
		private AddEffectWindow addEffectWindow;
		private Task applierTask;

		public int SelectedScreen { get; set; } = -1;

		public AppliedSettings Applied { get; set; }
		public Dictionary<string, List<Type>> ScreenTypes { get; private set; }
		public List<Type> EffectTypes { get; private set; }

		public List<PortScreen> Screens { get; private set; }
		public ContentFilter ContentFilter { get; private set; }

		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;
			Applied = new AppliedSettings(PropertyChanged);
			SetDarkColors();

			Screens = new List<PortScreen>();
			resourceLoader = new ResourceLoader(this, Applied, PropertyChanged);
			LoadScreens();
		}

		public PortingUtility RequestPortedTypeGeneration(string typeName) {
			Type selectedType = null;
			bool found = false;
			foreach (var st in ScreenTypes) {
				foreach (var item in st.Value) {
					if (item.Name == typeName) {
						selectedType = item;
						found = true;
						break;
					}
				}
				if (found) break;
			}
			if (!found) {
				foreach (var item in EffectTypes) {
					if (item.Name == typeName) {
						selectedType = item;
						found = true;
						break;
					}
				}
			}
			if (found) {
				return resourceLoader.CreateInstance(selectedType);
			}
			MessageBox.Show($"Cannot load requested resource '{typeName}'.");
			return null;
		}

		private void LoadScreens()
		{
				ScreenTypes = resourceLoader.LoadScreens(out List<Type> effects);
				EffectTypes = effects;

				string extensinFilers = string.Empty;
				string extensinFilersAfter = string.Empty;
				int i = 0;
				foreach (var screen in ScreenTypes) {
					extensinFilersAfter += "|" + screen.Key + " |*." + screen.Key;
					extensinFilers += "*." + screen.Key;
					if (i < ScreenTypes.Count - 1) {
						extensinFilers += ";";
					}
					i++;
				}

				ContentFilter = new ContentFilter() {
					InitDirectory = string.Empty,
					Filter = "Loadable |" + extensinFilers + extensinFilersAfter + "|All files |*.*",
					RestoreDirectory = false
				};

				//foreach (var screenTyle in ScreenTypes)
				//{
				//	PortScreen newScreen = resourceLoader.CreateInstance(screenTyle) as PortScreen;
				//	AddScreenToTabControl(newScreen);
				//}

				if (ScreenTypes.Count > 0) {
					SelectScreen(0);
				}
			
		}

		private void AddScreenToTabControl(PortScreen screen)
		{
			screen.SetEventHandler(this.PropertyChanged);
			Border innerBorder = new Border()
			{
				BorderThickness = new Thickness(3, 3, 0, 0),
				BorderBrush = Applied.Colors.Secondary,
				Background = Applied.Colors.Background,
				Child = screen.GetWindow()
			};

			BindingOperations.SetBinding(innerBorder, Border.BorderBrushProperty, new Binding("Applied.Colors.Secondary")
			{
				Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
			});
			BindingOperations.SetBinding(innerBorder, Border.BackgroundProperty, new Binding("Applied.Colors.Background")
			{
				Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
			});

			Border outerBorder = new Border()
			{
				BorderThickness = new Thickness(0, 0, 1, 1),
				Margin = new Thickness(0, -2, -2, -2),
				BorderBrush = Applied.Colors.Border,
				Child = innerBorder,
			};

			BindingOperations.SetBinding(outerBorder, Border.BorderBrushProperty, new Binding("Applied.Colors.Border")
			{
				Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
			});

			TabItem item = new TabItem()
			{
				Header = screen.TabName,
				Content = outerBorder
			};
			BindingOperations.SetBinding(item, TabItem.HeaderProperty, new Binding("TabName") {
				Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
			});
			item.MouseDown += TabItem_MouseDown;
			item.MouseLeftButtonUp += TabItem_MouseLeftButtonUp;

			/*Label label = new Label
			{
				Content = Screens.Count,
				Visibility = Visibility.Hidden
			};*/

			SelectedScreen = TabControl.Items.Count == 1 ? 0 : TabControl.Items.Count - 1;
			TabControl.Items.Insert(SelectedScreen, item);
			Screens.Add(screen);
			TabControl.SelectedIndex = SelectedScreen;
		}

		public void SelectScreen(int index) {
			LayersPanel.Children.Clear();
			//EffectsPanel.Children.Clear();

			SelectedScreen = index;

			if (index >= Screens.Count) {
				return;
			}

            StatusBar.Items.Clear();
			StatusBar.Items.Add(Screens[index].GetStatusbarContent());

			//Task.Factory.StartNew(() => {
			foreach (ILayer layer in Screens[index].GetAllLayers()) {
				Grid grid = new Grid();
				grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(35) });
				grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(35) });
				grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(15) });
				grid.ColumnDefinitions.Add(new ColumnDefinition());
				grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(35) });
				grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
				grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

				ScrollViewer scrollViewer = new ScrollViewer() {
					Visibility = layer.Collapsed ? Visibility.Collapsed : Visibility.Visible,
					VerticalScrollBarVisibility = ScrollBarVisibility.Auto
				};
				Border effectAddBorder = new Border() {
					Visibility = layer.Collapsed ? Visibility.Collapsed : Visibility.Visible,
					BorderBrush = Applied.Colors.Border,
					//BorderThickness = new Thickness(0, 1, 0, 0)
				};
				Grid.SetColumnSpan(effectAddBorder, 2);
				Button addEffectBtn = new Button() {
					Content = "Add effect", //TODO: Should be set from Applied.Language...
					HorizontalAlignment = HorizontalAlignment.Center
				};
				addEffectBtn.Click += AddEffect;
				effectAddBorder.Child = addEffectBtn;
				grid.Children.Add(effectAddBorder);

				#region Buttons
				Button btn = new Button() {
					Content = layer.Name
				};
				btn.Click += (s, e) => {
					int i = LayersPanel.Children.IndexOf((s as Button).Parent as Grid);
					Screens[SelectedScreen].SelectLayer(i);
					//SelectScreen(SelectedScreen);
					//ApplyEffects();

					//TODO: Implement open effects here...
					if (scrollViewer.Visibility == Visibility.Visible) {
						scrollViewer.Visibility = Visibility.Collapsed;
						effectAddBorder.Visibility = Visibility.Collapsed;
						layer.Collapsed = true;
					}
					else {
						scrollViewer.Visibility = Visibility.Visible;
						effectAddBorder.Visibility= Visibility.Visible;
						layer.Collapsed = false;
					}
				};
				grid.Children.Add(btn);
				Grid.SetColumn(btn, 2);
				Grid.SetColumnSpan(btn, 2);

				Button closebtn = new Button() {
					Content = "X"
				};
				closebtn.MaxHeight = 35;
				closebtn.Click += (s, e) => {
					int i = LayersPanel.Children.IndexOf((s as Button).Parent as Grid);
					//Screens[SelectedScreen].RemoveLayer(i);
					ApplyEffects();
				};
				grid.Children.Add(closebtn);
				Grid.SetColumn(closebtn, 4);
				#endregion Buttons


				StackPanel stackPanel = new StackPanel() {
					Margin = new Thickness(5),
					VerticalAlignment = VerticalAlignment.Top
				};

				foreach (PortEffect effect in layer.Effects) {
					Grid efcgrid = new Grid();
					efcgrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(35) });
					efcgrid.ColumnDefinitions.Add(new ColumnDefinition());
					//efcgrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto)});
					efcgrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(35) });

					efcgrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength() });
					efcgrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength() });

					#region Buttons
					Button renderredbtn = new Button() {
						Content = effect.Rendered ? new Image { Source = BitmapFrame.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("PhoVodKEdit.Pics.eye.png"), BitmapCreateOptions.None, BitmapCacheOption.OnLoad) } : new Image { Source = BitmapFrame.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("PhoVodKEdit.Pics.ceye.png"), BitmapCreateOptions.None, BitmapCacheOption.OnLoad) }
					};
					renderredbtn.MaxHeight = 35;
					renderredbtn.Click += (s, e) => {
						PortEffect eff = Screens[SelectedScreen].GetEffects()[stackPanel.Children.IndexOf((s as Button).Parent as Grid)];
						eff.Rendered = !eff.Rendered;
						if (eff.Rendered) {
							(s as Button).Content = new Image { Source = BitmapFrame.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("PhoVodKEdit.Pics.eye.png"), BitmapCreateOptions.None, BitmapCacheOption.OnLoad) };
						}
						else {
							(s as Button).Content = new Image { Source = BitmapFrame.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("PhoVodKEdit.Pics.ceye.png"), BitmapCreateOptions.None, BitmapCacheOption.OnLoad) };
						}
						ApplyEffects();
					};
					efcgrid.Children.Add(renderredbtn);
					Grid.SetColumn(renderredbtn, 0);


					Button effbtn = new Button() {
						Content = effect.Name
					};
					effbtn.Click += (s, e) => {
						MessageBox.Show("You selected an effect.\nSadly it is not implemented yet...");
					};
					efcgrid.Children.Add(effbtn);
					Grid.SetColumn(effbtn, 1);

					Button effclosebtn = new Button() {
						Content = "X"
					};
					effclosebtn.MaxHeight = 35;
					effclosebtn.Click += (s, e) => {
						int i = stackPanel.Children.IndexOf((s as Button).Parent as Grid);
						Screens[SelectedScreen].RemoveEffect(i);
						SelectScreen(SelectedScreen);
					};
					efcgrid.Children.Add(effclosebtn);
					Grid.SetColumn(effclosebtn, 2);
					#endregion Buttons

					#region GetEffectView
					var view = effect.GetPublicView();
					if (view != null) {
						Grid.SetRow(view, 1);
						Grid.SetColumnSpan(view, 3);
						efcgrid.Children.Add(view);
					}
					#endregion GetEffectView

					#region Diagnostics
					Label lbl = new Label() {
						Content = effect.Rendered ? string.Format("{0:0.00}ms", effect.GetProcessTinmeMs()) : "- ms",
						FontSize = Applied.Font.Size,
						Foreground = Applied.Colors.Foreground,
						HorizontalAlignment = HorizontalAlignment.Right,
						VerticalAlignment = VerticalAlignment.Center,
					};
					efcgrid.Children.Add(lbl);
					Grid.SetColumn(lbl, 1);
					#endregion Diagnostics

					stackPanel.Children.Add(efcgrid);
				}

				scrollViewer.Content = stackPanel;
				grid.Children.Add(scrollViewer);
				Grid.SetRow(scrollViewer, 1);
				Grid.SetColumn(scrollViewer, 1);
				Grid.SetColumnSpan(scrollViewer, 4);


				LayersPanel.Children.Add(grid);
			}
		}

		public void ApplyEffects() {
			//if (applierTask == null)

			Task.Factory.StartNew(() => {
				Screens[SelectedScreen].Apply();
			}).ContinueWith(r => {
				Screens[SelectedScreen].Refresh();
				TotalEffectTimerLabel.Content = string.Format("{0:0.00}ms", Screens[SelectedScreen].GetProcessTinmeMs());
				SelectScreen(SelectedScreen);
			}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		#region Theme setters

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			SetDarkColors();
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			SetLightColors();
		}

		private void SetDarkColors()
		{
			Applied.Colors.Main = new SolidColorBrush(Settings.Colors.Dark.Main);
			Applied.Colors.Secondary = new SolidColorBrush(Settings.Colors.Dark.Secondary);
			Applied.Colors.Foreground = new SolidColorBrush(Settings.Colors.Dark.Foreground);
			Applied.Colors.Background = new SolidColorBrush(Settings.Colors.Dark.Background);
			Applied.Colors.Border = new SolidColorBrush(Settings.Colors.Dark.Border);

			MenuItem item = ((Toolbar.Children[0] as Menu).Items[0] as MenuItem);
		}

		private void SetLightColors()
		{
			Applied.Colors.Main = new SolidColorBrush(Settings.Colors.Light.Main);
			Applied.Colors.Secondary = new SolidColorBrush(Settings.Colors.Light.Secondary);
			Applied.Colors.Foreground = new SolidColorBrush(Settings.Colors.Light.Foreground);
			Applied.Colors.Background = new SolidColorBrush(Settings.Colors.Light.Background);
			Applied.Colors.Border = new SolidColorBrush(Settings.Colors.Light.Border);
		}

		#endregion Theme setters

		private void TabItem_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.MiddleButton == MouseButtonState.Pressed)
			{
				if (TabControl.Items.IndexOf(sender as TabItem) < TabControl.Items.Count - 1) {
					if (!Screens[TabControl.Items.IndexOf(sender)].Dispose()) return;
					Screens.RemoveAt(TabControl.Items.IndexOf(sender));
					TabControl.Items.Remove(sender as TabItem);
					GC.Collect();
				}
				if (TabControl.Items.Count > 1) {
					SelectedScreen = TabControl.Items.IndexOf(TabControl.SelectedItem);
					SelectScreen(SelectedScreen);
				}
				else {
					SelectedScreen = -1;
					LayersPanel.Children.Clear();
				}
			}
		}

		private void SetMaxSizes(Grid titleGrid)
		{
			FrameworkElement title = titleGrid.Children[0] as FrameworkElement;
			FrameworkElement addLayer = titleGrid.Children[2] as FrameworkElement;
			FrameworkElement parent = titleGrid.Parent as FrameworkElement;

			(titleGrid.Children[1] as FrameworkElement).MaxHeight = parent.ActualHeight -
																	 (title.ActualHeight + addLayer.MinHeight + 10);
		}

		//private void LayersGrid_SizeChanged(object sender, SizeChangedEventArgs e)
		//{
		//	  SetMaxSizes(LayersGrid);
		//}

		private void EffectsGrid_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			SetMaxSizes(EffectsGrid);
		}

		private void NewLayer(object sender, RoutedEventArgs e) {
			int index = TabControl.Items.IndexOf(TabControl.SelectedItem);
			Screens[index].AddLayer();
			SelectScreen(index);
		}

		private void AddEffect(object sender, RoutedEventArgs e) {
			int index = TabControl.Items.IndexOf(TabControl.SelectedItem);
			if (addEffectWindow == null || addEffectWindow.IsClosed) {
				addEffectWindow = new AddEffectWindow(Applied, EffectTypes, (int i) => {
					Screens[index].AddEffect(resourceLoader.CreateInstance(EffectTypes[i]) as PortEffect);
					ApplyEffects();
				});
				addEffectWindow.Show();
			}
			else {
				addEffectWindow.Focus();
			}
		}

		private void TabItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			SelectScreen(TabControl.Items.IndexOf(sender));
		}

		private void MenuItem_OpenClick(object sender, RoutedEventArgs e) {
			var dialog = new OpenFileDialog() {
				InitialDirectory = ContentFilter.InitDirectory,
				Filter = ContentFilter.Filter,
				RestoreDirectory = ContentFilter.RestoreDirectory,
				DereferenceLinks = false
			};
			dialog.FileOk += (s, cea) => {
				string[] splits = dialog.FileName.Split('.');
				string typeEXT = splits[splits.Length - 1].ToUpper();

				List<Type> availableTypes = ScreenTypes[typeEXT];
				int selected = 0;

				if (availableTypes.Count > 1) {
					var message = new AddEffectWindow(Applied, availableTypes, (int i) => {
						selected = i;
					});
					message.ShowDialog();
				}

				PortScreen newScreen = resourceLoader.CreateInstance(availableTypes[selected]) as PortScreen;
				AddScreenToTabControl(newScreen);

				//if (newScreen.GetType() == typeof(VideoEditor.VideoEditor)) {
				//	(newScreen as VideoEditor.VideoEditor).SetContent(dialog.FileName);
				//}

				RoutedEventHandler handler = null;
				handler = (c, ero) => {
					Screens[SelectedScreen].SetContent(dialog.FileName);
					Screens[SelectedScreen].OwnWindow.Loaded -= handler;
				};
				newScreen.OwnWindow.Loaded += handler;
			};

			dialog.ShowDialog();
		}

		private void MenuItem_CloseClick(object sender, RoutedEventArgs e) {
			Screens.RemoveAt(TabControl.Items.IndexOf(TabControl.SelectedItem));
			TabControl.Items.Remove(TabControl.SelectedItem);
		}

		private void MenuItem_NewClick(object sender, RoutedEventArgs e) {
			var availableTypes = new List<Type>();
			int selected = 0;

			foreach (var type in ScreenTypes) {
				foreach (var item in type.Value) {
					if (!availableTypes.Contains(item)) {
						availableTypes.Add(item);
					}
				}
			}
			var message = new AddEffectWindow(Applied, availableTypes, (int i) => {
				selected = i;
			});
			message.ShowDialog();

			PortScreen newScreen = resourceLoader.CreateInstance(availableTypes[selected]) as PortScreen;
			AddScreenToTabControl(newScreen);

			Window cncw = newScreen.CreateNewContent();
			cncw.Show();
		}

		private void MenuItem_SaveClick(object sender, RoutedEventArgs e) {
			var exts = (Screens[SelectedScreen].GetType().GetCustomAttribute(typeof(FileTypesAttribute)) as FileTypesAttribute).exts;

			string extensinFilers = string.Empty;
			string extensinFilersAfter = string.Empty;
			int i = 0;
			foreach (var ext in exts) {
				extensinFilersAfter += "|" + ext + " |*." + ext;
				extensinFilers += "*." + ext;
				if (i < ScreenTypes.Count - 1) {
					extensinFilers += ";";
				}
				i++;
			}

			var dialog = new SaveFileDialog() {
				InitialDirectory = ContentFilter.InitDirectory,
				Filter = "SAVE |" + extensinFilers + extensinFilersAfter + "|All files |*.*",
				RestoreDirectory = ContentFilter.RestoreDirectory,
				DereferenceLinks = false
			};

			dialog.FileOk += (s, cea) => {
				var parts = dialog.FileName.Split('.');
				string saveFile = dialog.FileName;
				if (parts.Length > 0) {
					if (parts[parts.Length - 1].ToUpper() != exts[0].ToUpper()) {
						parts[parts.Length - 1] = exts[0].ToLower();
						saveFile = parts[0];
						for (i = 1; i < parts.Length; i++) {
							saveFile += "." + parts[i];
						}
					}
				}
				else {
					saveFile += "." + exts[0].ToLower();
				}

				Screens[SelectedScreen].SaveToFile(saveFile);
			};

			dialog.ShowDialog();
		}
	}
}
