using AvalonDock.Layout;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using Script.Utils;
using Microsoft.CodeAnalysis.Differencing;
using RoslynPad.Editor;
using RoslynPad.Roslyn;
using System.IO;
using Script.ViewModels.Dialogs;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Script.Views.Dialogs
{
	/// <summary>
	/// UserControl1.xaml 的交互逻辑
	/// </summary>
	public partial class CsharpEditor : UserControl
	{
		private CSharpEditorViewModel viewModel;
		private CSharpCompletionProvider cSharpCompletionProvider;
		private readonly RoslynHost _host;
		public CsharpEditor(CSharpEditorViewModel viewModel)
		{
			InitializeComponent();
			this.viewModel = viewModel;
			//cSharpCompletionProvider = new CSharpCompletionProvider(avEditor);
			_host = new CustomRoslynHost(additionalAssemblies:
				[
							Assembly.Load("RoslynPad.Roslyn.Windows"),
							Assembly.Load("RoslynPad.Editor.Windows")
						], RoslynHostReferences.NamespaceDefault.With(assemblyReferences:
				[
					typeof(object).Assembly,
					typeof(System.Text.RegularExpressions.Regex).Assembly,
					typeof(Enumerable).Assembly,
				]));
		}

		private async void OnItemLoaded(object sender, EventArgs e)
		{
			if (!(sender is RoslynCodeEditor editor)) return;

			editor.Loaded -= OnItemLoaded;
			editor.Focus();

			var workingDirectory = Directory.GetCurrentDirectory();

			var documentId = await editor.InitializeAsync(_host, new ClassificationHighlightColors(),
				workingDirectory, string.Empty, SourceCodeKind.Script).ConfigureAwait(true);

			//viewModel.Initialize(documentId);
		}
		private class CustomRoslynHost : RoslynHost
		{
			private bool _addedAnalyzers;

			public CustomRoslynHost(IEnumerable<Assembly>? additionalAssemblies = null, RoslynHostReferences? references = null, ImmutableHashSet<string>? disabledDiagnostics = null) : base(additionalAssemblies, references, disabledDiagnostics)
			{
			}

			protected override IEnumerable<AnalyzerReference> GetSolutionAnalyzerReferences()
			{
				if (!_addedAnalyzers)
				{
					_addedAnalyzers = true;
					return base.GetSolutionAnalyzerReferences();
				}

				return Enumerable.Empty<AnalyzerReference>();
			}
		}
	}

}
