using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit;
using Script.ViewModels;
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
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using AvalonDock.Layout;
using Script.Views.Dialogs;
using Script.ViewModels.Dialogs;

namespace Script.Views
{
	/// <summary>
	/// WorkSpaceView.xaml 的交互逻辑
	/// </summary>
	public partial class WorkSpaceView : Window
	{
		public WorkSpaceView(WorkSpaceViewModel vm)
		{
			this.DataContext = vm;
			InitializeComponent();

			CSharpEditorViewModel csharpEditorVM = new()
			{
				Title = "test"
			};


			LayoutDocument document = new LayoutDocument
			{
				Content = new CsharpEditor(csharpEditorVM)
			};

			dockPanelDocument.Children.Add(document);
		}

	}
}
