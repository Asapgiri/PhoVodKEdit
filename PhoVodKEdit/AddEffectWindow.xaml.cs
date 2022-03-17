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
using PhoVodKEdit.Port.APS;

namespace PhoVodKEdit
{
	/// <summary>
	/// Interaction logic for AddEffectWindow.xaml
	/// </summary>
	public partial class AddEffectWindow : Window
	{
		private int selected = -1;
		private Action<int> selectedAction;

		public AppliedSettings Applied { get; private set; }
		public bool IsClosed { get; private set; } = false;

		public AddEffectWindow(AppliedSettings applied, List<Type> effects, Action<int> addEffect)
		{
			Applied = applied;
			DataContext = this;
			selectedAction = addEffect;
			InitializeComponent();

			foreach (Type item in effects) {
				Button btn = new Button() {
					Content = item.Name
				};
				btn.Click += (s, e) => {
					SelectEffect(AvailableEffects.Children.IndexOf(s as Button));
				};

				AvailableEffects.Children.Add(btn);
			}
		}

		private void SelectEffect(int index) {
			int i = 0;
			selected = index;
			foreach (Button button in AvailableEffects.Children) {
				if (i++ == index) {
					button.Background = Applied.Colors.SecondaryColor;
				}
				else {
					button.Background = Brushes.Transparent;
				}
			}
		}

		private void AddEffect(object sender, RoutedEventArgs e) {
			if (selected >= 0) {
				selectedAction(selected);
				this.Close();
			}
		}

		protected override void OnClosed(EventArgs e) {
			base.OnClosed(e);
			IsClosed = true;
		}
	}
}
