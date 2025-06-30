using CommunityToolkit.Mvvm.ComponentModel;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script.ViewModels.Dialogs
{
	public partial class CSharpEditorViewModel:ObservableObject
	{
		[ObservableProperty] string title;

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


	}


}
