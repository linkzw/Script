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
	/// AddItemDialog.xaml 的交互逻辑
	/// </summary>
	public partial class AddItemDialog : Window
	{
		public string Input { get; private set; }

		public AddItemDialog()
		{
			InitializeComponent();
		}

		private void AcceptButton_Click(object sender, RoutedEventArgs e)
		{
			Input = InputText.Text.Trim();
			this.DialogResult = true;
		}
	}
}
