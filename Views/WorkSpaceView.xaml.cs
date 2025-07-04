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
	}
}
