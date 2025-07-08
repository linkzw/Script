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
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.CodeAnalysis;
using RoslynPad.Editor;
using System.IO;
using System.Globalization;
using Microsoft.CodeAnalysis.CSharp;
using System.Windows.Forms;

namespace Script.Views
{
	/// <summary>
	/// WorkSpaceView.xaml 的交互逻辑
	/// </summary>
	public partial class WorkSpaceView : Window
	{
		WorkSpaceViewModel viewModel;
		public WorkSpaceView(WorkSpaceViewModel vm)
		{
			InitializeComponent();
			viewModel = vm;
			this.DataContext = vm;
			vm.CurrentScriptChanged += ViewModel_CurrentScriptChanged;



		}

		private async void ViewModel_ScriptUpdated(object? sender, EventArgs e)
		{
			if (viewModel.CurrentScript != null)
			{
				var workingDirectory = Directory.GetCurrentDirectory();
				var documentId = await Editor.InitializeAsync(viewModel.Host, new ClassificationHighlightColors(),
				workingDirectory, string.Empty, SourceCodeKind.Script).ConfigureAwait(true);
				viewModel.CurrentScript.Id = documentId;
				Editor.Text = viewModel.CurrentScript.Text;
			}
		}

		private async void ViewModel_CurrentScriptChanged(object? sender, ScriptViewModel? e)
		{

			if (viewModel.CurrentScript != null)
			{
				
				Editor.Text = viewModel.CurrentScript.Text;
			}
		}

		private void Editor_KeyDown(object sender, KeyEventArgs e)
		{
			if (viewModel.CurrentScript != null)
			{
				viewModel.CurrentScript.Text = Editor.Text;
				if(!viewModel.IsModified)
				{
					viewModel.IsModified = true;
				}
			}
		}

		private async void Editor_Loaded(object sender, RoutedEventArgs e)
		{
			if (!(sender is RoslynCodeEditor editor)) return;

			editor.Loaded -= Editor_Loaded;
			editor.Focus();

			var workingDirectory = Directory.GetCurrentDirectory();
			var documentId = await editor.InitializeAsync(viewModel.Host, new ClassificationHighlightColors(),
			workingDirectory, string.Empty, SourceCodeKind.Script).ConfigureAwait(true);
			//viewModel.CurrentScript.Id = documentId;

		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			var editor = Editor; // 你的 RoslynCodeEditor
			int offset = editor.CaretOffset;

			string codeToInsert = "LogMain.Print(\"Log\");";

			editor.Document.Insert(offset, codeToInsert);
			editor.Text = AddUsingDirective(editor.Text, "Script.Utils");
		}

		#region 普通函数
		// 给文本添加using
		public static string AddUsingDirective(string code, string namespaceToAdd)
		{
			var tree = CSharpSyntaxTree.ParseText(code);
			var root = tree.GetCompilationUnitRoot();

			// 检查是否已经有这个 using
			if (root.Usings.Any(u => u.Name.ToString() == namespaceToAdd))
				return code;

			var newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(namespaceToAdd))
										.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

			var newRoot = root.AddUsings(newUsing);
			return newRoot.NormalizeWhitespace().ToFullString();
		}

		#endregion

		private void NugetAddButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				// 设置过滤器，例如只显示CSV文件
				Filter = "Files (*.nupkg)|*.nupkg"
			};
			if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				// 获取选中文件的完整路径
				string filePath = openFileDialog.FileName;
				// 4. 更新编辑器的补全服务

			}
		}
	}

	public class LogLevelToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value switch
			{
				LogLevel.INFO => Brushes.Black,
				LogLevel.WARN => Brushes.Orange,
				LogLevel.ERROR => Brushes.Red,
				_ => Brushes.Black
			};
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}

}
