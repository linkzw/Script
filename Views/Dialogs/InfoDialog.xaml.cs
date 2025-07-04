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


namespace Script.Views.Dialogs
{
	/// <summary>
	/// InfoDialog.xaml 的交互逻辑
	/// </summary>
	public partial class InfoDialog : Window
	{
		public InfoDialog(string message)
		{
			InitializeComponent();
			MessageText.Text = message;
		}
		public InfoDialog(string message, bool cancelEnable)
		{
			InitializeComponent();
			MessageText.Text = message;
			if(cancelEnable == false)
			{
				Cacel_Button.Visibility = Visibility.Collapsed;
			}
		}
		private void Confirm_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}
	}
}
