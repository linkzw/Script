using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace Script.Utils
{
	public class CSharpCompletionProvider : IDisposable
	{
		private readonly TextEditor _editor;
		private CompletionWindow _completionWindow;
		private readonly AdhocWorkspace _workspace;
		private Project _project;
		private Document _document;

		public CSharpCompletionProvider(TextEditor editor)
		{
			_editor = editor;

			// 初始化Roslyn工作区
			_workspace = new AdhocWorkspace();

			/*
			// 创建项目并添加必要的程序集引用
			var projectInfo = ProjectInfo.Create(
				ProjectId.CreateNewId(),
				VersionStamp.Create(),
				"ScriptProject",
				"ScriptAssembly",
				LanguageNames.CSharp,
				metadataReferences: GetDefaultMetadataReferences());

			_project = _workspace.AddProject(projectInfo);
			*/

			// 加上引用程序集，防止找不到引用
			var referenceAssemblyPaths = new[]
			{
				typeof(object).Assembly.Location,
				typeof(Console).Assembly.Location,
			};
			var csharpCompilationOptions = new CSharpCompilationOptions
			(
				OutputKind.DynamicallyLinkedLibrary, // 输出类型 dll 类型
				usings: new[] { "System" }, // 引用的命名空间
				allowUnsafe: true, // 允许不安全代码
				sourceReferenceResolver: new SourceFileResolver
				(
					searchPaths: new[] { Environment.CurrentDirectory },
					baseDirectory: Environment.CurrentDirectory
				)
			);

			var project = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(),
				name: "Lindexi",
				assemblyName: "Lindexi",
				language: csharpCompilationOptions.Language,
				metadataReferences: referenceAssemblyPaths.Select(t => MetadataReference.CreateFromFile(t)));

			_project = _workspace.AddProject(project);

			UpdateDocument();

			_editor.TextChanged += OnTextChanged;
			_editor.TextArea.TextEntering += OnTextEntering;
			_editor.TextArea.TextEntered += OnTextEntered;
		}



		private void UpdateDocument()
		{
			_document = _workspace.AddDocument(_project.Id, "Script.cs", SourceText.From(_editor.Text));
			_project = _document.Project;
		}

		private static IEnumerable<MetadataReference> GetDefaultMetadataReferences()
		{
			var assemblies = new[]
			{
				typeof(object).Assembly,                // mscorlib
				typeof(Enumerable).Assembly,            // System.Core
				typeof(System.ComponentModel.EditorBrowsableAttribute).Assembly,  // System
				typeof(System.Data.DataTable).Assembly,  // System.Data
				typeof(System.Xml.XmlDocument).Assembly, // System.Xml
				typeof(Window).Assembly                 // PresentationFramework (WPF)
			};

			return assemblies.Select(a => MetadataReference.CreateFromFile(a.Location));
		}

		private async void OnTextChanged(object sender, EventArgs e)
		{
			UpdateDocument();
		}

		private async void OnTextEntered(object sender, TextCompositionEventArgs e)
		{
			// 触发补全的字符
			if (e.Text == "." || e.Text == " " || e.Text == "(" || char.IsLetter(e.Text[0]))
			{
				await ShowCompletion();
			}
		}

		private void OnTextEntering(object sender, TextCompositionEventArgs e)
		{
			if (_completionWindow != null)
			{
				if (!char.IsLetterOrDigit(e.Text[0]))
				{
					_completionWindow.CompletionList.RequestInsertion(e);
				}
			}
		}

		private async Task ShowCompletion()
		{
			try
			{
				if (_completionWindow != null)
					return;

				var position = _editor.CaretOffset;
				var currentWord = GetCurrentWord(); // 获取当前正在输入的词
				var completionService = CompletionService.GetService(_document);
				var results = await completionService.GetCompletionsAsync(_document, position);

				if (results == null || !results.Items.Any())
					return;

				_completionWindow = new CompletionWindow(_editor.TextArea);

				// 按推荐度排序
				var sortedItems = results.Items
					.OrderByDescending(i => i.Rules.MatchPriority)
					.ThenBy(i => i.DisplayText);

				foreach (var completionItem in results.ItemsList
			 .OrderBy(item => item.DisplayText.StartsWith(currentWord) ? 0 : 1)
			 .ThenByDescending(item => item.Rules.MatchPriority)
			 .ThenBy(item => item.SortText))
				{
					_completionWindow.CompletionList.CompletionData.Add(
						new CompletionData(
							completionItem.DisplayText,
							completionItem.FilterText,
							completionItem.InlineDescription));
				}

				_completionWindow.Closed += (s, e) => _completionWindow = null;
				_completionWindow.Show();
			}
			catch (Exception ex)
			{
				// 调试时可以取消下面注释
				// MessageBox.Show($"补全错误: {ex.Message}");
			}
		}

		private string GetCurrentWord()
		{
			var document = _editor.Document;
			var offset = _editor.CaretOffset;

			// 查找单词起始位置
			int start = FindWordStart(document, offset);
			// 查找单词结束位置
			int end = FindWordEnd(document, offset);

			return document.GetText(start, end - start);
		}

		private int FindWordStart(ICSharpCode.AvalonEdit.Document.TextDocument document, int offset)
		{
			// 从光标位置向前查找单词起始
			while (offset > 0)
			{
				char c = document.GetCharAt(offset - 1);
				if (!IsWordPart(c))
					break;
				offset--;
			}
			return offset;
		}

		private int FindWordEnd(ICSharpCode.AvalonEdit.Document.TextDocument document, int offset)
		{
			// 从光标位置向后查找单词结束
			while (offset < document.TextLength)
			{
				char c = document.GetCharAt(offset);
				if (!IsWordPart(c))
					break;
				offset++;
			}
			return offset;
		}

		private bool IsWordPart(char c)
		{
			// 定义什么字符属于单词部分
			return char.IsLetterOrDigit(c) || c == '_';
		}

		public void Dispose()
		{
			_editor.TextChanged -= OnTextChanged;
			_editor.TextArea.TextEntering -= OnTextEntering;
			_editor.TextArea.TextEntered -= OnTextEntered;
		}
	}

	public class CompletionData : ICompletionData
	{
		public CompletionData(string text, string content, string description)
		{
			Text = text;
			Content = content;
			Description = description;
		}

		public string Text { get; }
		public object Content { get; }
		public object Description { get; }
		public double Priority => 0;
		public System.Windows.Media.ImageSource Image => null;

		public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
		{
			// 1. 获取当前文档和光标位置
			var document = textArea.Document;
			var caretOffset = textArea.Caret.Offset;

			// 2. 计算要替换的文本范围
			var replaceStart = Math.Min(completionSegment.Offset, caretOffset);
			var replaceLength = Math.Max(0, caretOffset - replaceStart);
			//int start = FindActualWordStart(textArea, completionSegment.Offset);
			// 3. 执行替换操作
			// 使用正确的参数顺序（而不是命名参数）
			document.Replace(completionSegment.Offset, completionSegment.Length, this.Text);


		}
	}
}