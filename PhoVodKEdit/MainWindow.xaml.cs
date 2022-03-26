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
		public List<Type> ScreenTypes { get; private set; }
		public List<Type> EffectTypes { get; private set; }

		public List<PortScreen> Screens { get; private set; }

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

		private void LoadScreens()
		{
			ScreenTypes = resourceLoader.LoadScreens(out List<Type> effects);
			EffectTypes = effects;

			foreach (var screenTyle in ScreenTypes)
			{
				PortScreen newScreen = resourceLoader.CreateInstance(screenTyle) as PortScreen;
				AddScreenToTabControl(newScreen);
			}

			if (ScreenTypes.Count > 0) {
				SelectScreen(0);
			}
		}

		private void AddScreenToTabControl(PortScreen screen)
		{
			Border innerBorder = new Border()
			{
				BorderThickness = new Thickness(3, 3, 0, 0),
				BorderBrush = Applied.Colors.SecondaryColor,
				Background = Applied.Colors.BackgroundColor,
				Child = screen.GetWindow()
			};

			BindingOperations.SetBinding(innerBorder, Border.BorderBrushProperty, new Binding("Applied.Colors.SecondaryColor")
			{
				Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
			});
			BindingOperations.SetBinding(innerBorder, Border.BackgroundProperty, new Binding("Applied.Colors.BackgroundColor")
			{
				Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
			});

			Border outerBorder = new Border()
			{
				BorderThickness = new Thickness(0, 0, 1, 1),
				Margin = new Thickness(0, -2, -2, -2),
				BorderBrush = Applied.Colors.BorderColor,
				Child = innerBorder,
			};

			BindingOperations.SetBinding(outerBorder, Border.BorderBrushProperty, new Binding("Applied.Colors.BorderColor")
			{
				Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
			});

			TabItem item = new TabItem()
			{
				Header = screen.GetType().Name,
				Content = outerBorder
			};
			item.MouseDown += TabItem_MouseDown;
			item.MouseLeftButtonUp += TabItem_MouseLeftButtonUp;

			/*Label label = new Label
			{
				Content = Screens.Count,
				Visibility = Visibility.Hidden
			};*/

			TabControl.Items.Insert(TabControl.Items.Count == 1 ? 0 : TabControl.Items.Count - 2,
									item);
			Screens.Add(screen);
		}

		public void SelectScreen(int index) {
			LayersPanel.Children.Clear();
			EffectsPanel.Children.Clear();

			SelectedScreen = index;

			if (index >= Screens.Count) {
				return;
			}

            StatusBar.Items.Clear();
			StatusBar.Items.Add(Screens[index].GetStatusbarContent());

			//Task.Factory.StartNew(() => {
				foreach (Layer layer in Screens[index].GetAllLayers()) {
					Grid grid = new Grid();
					grid.ColumnDefinitions.Add(new ColumnDefinition());
					grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(35) });

					#region Buttons
					Button btn = new Button() {
						Content = layer.Name
					};
					btn.Click += (s, e) => {
						int i = LayersPanel.Children.IndexOf((s as Button).Parent as Grid);
						Screens[SelectedScreen].SelectLayer(i);
						SelectScreen(SelectedScreen);
						//ApplyEffects();
					};
					grid.Children.Add(btn);
					Grid.SetColumn(btn, 0);

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
					Grid.SetColumn(closebtn, 1);
					#endregion Buttons

					LayersPanel.Children.Add(grid);
				}
			//});

			//Task.Factory.StartNew(() => {
				foreach (PortEffect effect in Screens[index].GetEffects()) {
				Grid grid = new Grid();
				grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(35) });
				grid.ColumnDefinitions.Add(new ColumnDefinition());
				//grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto)});
				grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(35) });

				grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength() });
				grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength() });

				#region Buttons
				Button renderredbtn = new Button() {
					Content = effect.Rendered ? new Image { Source = BitmapFrame.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("PhoVodKEdit.Pics.eye.png"), BitmapCreateOptions.None, BitmapCacheOption.OnLoad) } : new Image { Source = BitmapFrame.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("PhoVodKEdit.Pics.ceye.png"), BitmapCreateOptions.None, BitmapCacheOption.OnLoad) }
				};
				renderredbtn.MaxHeight = 35;
				renderredbtn.Click += (s, e) => {
					PortEffect eff = Screens[SelectedScreen].GetEffects()[EffectsPanel.Children.IndexOf((s as Button).Parent as Grid)];
					eff.Rendered = !eff.Rendered;
					if (eff.Rendered) {
						(s as Button).Content = new Image { Source = BitmapFrame.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("PhoVodKEdit.Pics.eye.png"), BitmapCreateOptions.None, BitmapCacheOption.OnLoad) };
					}
					else {
						(s as Button).Content = new Image { Source = BitmapFrame.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("PhoVodKEdit.Pics.ceye.png"), BitmapCreateOptions.None, BitmapCacheOption.OnLoad) };
					}
					ApplyEffects();
				};
				grid.Children.Add(renderredbtn);
				Grid.SetColumn(renderredbtn, 0);


				Button btn = new Button() {
					Content = effect.Name
				};
				btn.Click += (s, e) => {
					MessageBox.Show("You selected an effect.\nSadly it is not implemented yet...");
				};
				grid.Children.Add(btn);
				Grid.SetColumn(btn, 1);

				Button closebtn = new Button() {
					Content = "X"
				};
				closebtn.MaxHeight = 35;
				closebtn.Click += (s, e) => {
					int i = EffectsPanel.Children.IndexOf((s as Button).Parent as Grid);
					Screens[SelectedScreen].RemoveEffect(i);
					SelectScreen(SelectedScreen);
				};
				grid.Children.Add(closebtn);
				Grid.SetColumn(closebtn, 2);
				#endregion Buttons

				#region GetEffectView
				var view = effect.GetView();
				if (view != null) {
					Grid.SetRow(view, 1);
					Grid.SetColumnSpan(view, 3);
					grid.Children.Add(view);
				}
				#endregion GetEffectView

				#region Diagnostics
				Label lbl = new Label() {
					Content = effect.Rendered ? string.Format("{0:0.00}ms", effect.GetProcessTinmeMs()) : "- ms",
					FontSize = Applied.Font.Size,
					Foreground = Applied.Colors.ForegroundColor,
					HorizontalAlignment = HorizontalAlignment.Right,
					VerticalAlignment = VerticalAlignment.Center,
				};
				grid.Children.Add(lbl);
				Grid.SetColumn(lbl, 1);
				#endregion Diagnostics

				EffectsPanel.Children.Add(grid);
			}
			//});
		}

		private void ApplyEffects() {
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
			Applied.Colors.MainColor = new SolidColorBrush(Settings.Colors.Dark.Main);
			Applied.Colors.SecondaryColor = new SolidColorBrush(Settings.Colors.Dark.Secondary);
			Applied.Colors.ForegroundColor = new SolidColorBrush(Settings.Colors.Dark.Foreground);
			Applied.Colors.BackgroundColor = new SolidColorBrush(Settings.Colors.Dark.Background);
			Applied.Colors.BorderColor = new SolidColorBrush(Settings.Colors.Dark.Border);

			MenuItem item = ((Toolbar.Children[0] as Menu).Items[0] as MenuItem);
		}

		private void SetLightColors()
		{
			Applied.Colors.MainColor = new SolidColorBrush(Settings.Colors.Light.Main);
			Applied.Colors.SecondaryColor = new SolidColorBrush(Settings.Colors.Light.Secondary);
			Applied.Colors.ForegroundColor = new SolidColorBrush(Settings.Colors.Light.Foreground);
			Applied.Colors.BackgroundColor = new SolidColorBrush(Settings.Colors.Light.Background);
			Applied.Colors.BorderColor = new SolidColorBrush(Settings.Colors.Light.Border);
		}

		#endregion Theme setters

		private void TabItem_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.MiddleButton == MouseButtonState.Pressed)
			{
				if (TabControl.Items.IndexOf(sender as TabItem) < TabControl.Items.Count - 1) {
					Screens.RemoveAt(TabControl.Items.IndexOf(sender));
					TabControl.Items.Remove(sender as TabItem);
				}
				if (TabControl.Items.Count > 1) {
					SelectedScreen = TabControl.Items.IndexOf(TabControl.SelectedItem);
					SelectScreen(SelectedScreen);
				}
				else {
					SelectedScreen = -1;
					EffectsPanel.Children.Clear();
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

		private void LayersGrid_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			SetMaxSizes(LayersGrid);
		}

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
			PortScreen screen = Screens[SelectedScreen];
			ContentFilter cf = screen.ContentFilter;

			var dialog = new OpenFileDialog() {
				InitialDirectory = cf.InitDirectory,
				Filter = cf.Filter,
				RestoreDirectory = cf.RestoreDirectory,
				DereferenceLinks = false
			};
			dialog.FileOk += (s, err) => screen.SetContent(dialog.FileName);
 
			dialog.ShowDialog();
		}

		private void MenuItem_CloseClick(object sender, RoutedEventArgs e) {
			Screens.RemoveAt(TabControl.Items.IndexOf(TabControl.SelectedItem));
			TabControl.Items.Remove(TabControl.SelectedItem);
		}

		private void MenuItem_NewClick(object sender, RoutedEventArgs e) {
			Window cncw = Screens[SelectedScreen].CreateNewContent();
			cncw.Show();
		}
	}
}
