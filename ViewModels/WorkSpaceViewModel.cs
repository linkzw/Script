using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Script.ViewModels
{
	public partial class WorkSpaceViewModel : ObservableObject
	{
		#region private fields
		private bool mDisposed = false;

		private bool _isInitialized = false;       // application should be initialized through one method ONLY!
		private object _lockObject = new object(); // thread lock semaphore

		private ICommand _OpenFileCommand;
		private IHighlightingDefinition _HighlightingDefinition;
		private ICommand _HighlightingChangeCommand;
		[ObservableProperty]
		TextDocument document = new TextDocument()
		{
			Text = @"using System;

					public class Script
					{
						public static void Main()
						{
							Console.WriteLine(""Hello, World!"");
						}
					}"
		};
		public ObservableCollection<TextDocument> Documents { get; set; } = new ObservableCollection<TextDocument>();
		private readonly DocumentRootViewModel _demo;
		#endregion private fields

		#region constructors
		public WorkSpaceViewModel()
		{
			_demo = new DocumentRootViewModel();
			Documents.Add(new TextDocument()
			{
				Text = @"using System;

					public class Script
					{
						public static void Main()
						{
							Console.WriteLine(""Hello, World!"");
						}
					}"
			});
		}
		#endregion constructors

		#region properties

		#region Highlighting Definition
		/// <summary>
		/// AvalonEdit exposes a Highlighting property that controls whether keywords,
		/// comments and other interesting text parts are colored or highlighted in any
		/// other visual way. This property exposes the highlighting information for the
		/// text file managed in this viewmodel class.
		/// </summary>
		[ObservableProperty] public IHighlightingDefinition highlightingDefinition;


		/// <summary>
		/// Gets a command that changes the currently selected syntax highlighting in the editor.
		/// </summary>
		public ICommand HighlightingChangeCommand
		{
			get
			{
				if (_HighlightingChangeCommand == null)
				{
					_HighlightingChangeCommand = new RelayCommand<object>((p) =>
					{
						var parames = p as object[];

						if (parames == null)
							return;

						if (parames.Length != 1)
							return;

						var param = parames[0] as IHighlightingDefinition;
						if (param == null)
							return;

						HighlightingDefinition = param;
					});
				}

				return _HighlightingChangeCommand;
			}
		}
		#endregion Highlighting Definition

		#endregion properties


		#region methods
		/// <summary>
		/// Call this method if you want to initialize a headless
		/// (command line) application. This method will initialize only
		/// Non-WPF related items.
		/// 
		/// Method should not be called after <seealso cref="InitForMainWindow"/>
		/// </summary>
		public void InitWithoutMainWindow()
		{
			lock (_lockObject)
			{
				if (_isInitialized == true)
					throw new Exception("AppViewModel initizialized twice.");

				_isInitialized = true;
			}
		}

		#endregion methods

	}
}
