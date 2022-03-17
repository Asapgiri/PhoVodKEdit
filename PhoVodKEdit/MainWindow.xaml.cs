using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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

			/*Label label = new Label
			{
				Content = Screens.Count,
				Visibility = Visibility.Hidden
			};*/

			TabControl.Items.Add(item);
			Screens.Add(screen);
		}

		private void SelectScreen(int index) {
			LayersPanel.Children.Clear();
			foreach (Layer layer in Screens[index].GetAllLayers()) {
				Button btn = new Button() {
					Content = layer.Name
				};
				btn.Click += (s, e) => {
					MessageBox.Show("You selected a layer.\nSadly it is not implemented yet...");
				};

				LayersPanel.Children.Add(btn);
			}

			EffectsPanel.Children.Clear();
			foreach (PortEffect effect in Screens[index].GetEffects()) {
				Button btn = new Button() {
					Content = effect.Name
				};
				btn.Click -= (s, e) => {
					MessageBox.Show("You selected an effect.\nSadly it is not implemented yet...");
				};

				EffectsPanel.Children.Add(btn);
			}
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
			if (e.LeftButton == MouseButtonState.Pressed) {
				SelectScreen(TabControl.Items.IndexOf(sender));
			}
			else if (e.MiddleButton == MouseButtonState.Pressed)
			{
				TabControl.Items.Remove(sender as TabItem);
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
					SelectScreen(index);
				});
				addEffectWindow.Show();
			}
			else {
				addEffectWindow.Focus();
			}
		}
	}
}
