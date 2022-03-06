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

namespace PhoVodKEdit
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private ResourceLoader resourceLoader;

		public AppliedSettings Applied { get; set; }
		public List<Type> Screens { get; private set; }

		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;
			Applied = new AppliedSettings(PropertyChanged);
			SetDarkColors();

			resourceLoader = new ResourceLoader(this, Applied);
			LoadScreens();
		}

		private void LoadScreens()
		{
			Screens = resourceLoader.LoadScreens();

			foreach (var screen in Screens)
			{
				AddScreenToTabControl(resourceLoader.CreateInstance(screen) as PortScreen);
			}

		}

		private void AddScreenToTabControl(PortScreen screen)
		{
			Border innerBorder = new Border()
			{
				BorderThickness = new Thickness(3, 3, 0, 0),
				BorderBrush = Applied.Colors.SecondaryColor,
				Child = screen.GetWindow()
			};

			BindingOperations.SetBinding(innerBorder, Border.BorderBrushProperty, new Binding("Applied.Colors.SecondaryColor")
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

			TabControl.Items.Add(item);
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
	}
}
