using Script.ViewModels;
using Script.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Script
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		WorkSpaceViewModel viewModel = new WorkSpaceViewModel();
		public MainWindow()
		{
			
			InitializeComponent();

		}

		private void Button_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			WorkSpaceView view = new WorkSpaceView(viewModel);
			view.Show();
		}
	}
}