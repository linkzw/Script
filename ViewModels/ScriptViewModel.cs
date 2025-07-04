using CommunityToolkit.Mvvm.ComponentModel;
using CSScriptLib;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using Script.Mode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace Script.ViewModels
{
	public partial class ScriptViewModel:ObservableObject
	{

		#region 私有字段

		private AssemblyLoadContext? innerLoader;

		#endregion

		#region 公共属性

		public string ScriptId { get; set; }

		[ObservableProperty]
		private DocumentId id = DocumentId.CreateNewId(ProjectId.CreateNewId());

		[ObservableProperty]
		private string name = string.Empty;

		[ObservableProperty]
		private string description;

		[ObservableProperty]
		private bool hasResult;

		[ObservableProperty]
		private bool hadError;

		[ObservableProperty]
		private string funcName = string.Empty;

		[ObservableProperty]
		private int returnTypeIndex;

		[ObservableProperty]
		private string text = string.Empty;

		[ObservableProperty]
		private string parameterName = string.Empty;

		[ObservableProperty]
		private int parameterTypeIndex;

		[ObservableProperty]
		private FunctionViewModel? currentParameter;

		[ObservableProperty]
		private ReferenceViewModel? currentReference;

		public ObservableCollection<FunctionViewModel> Functions { get; set; } = new();
		public ObservableCollection<ReferenceViewModel> References { get; set; } = new();

		#endregion

		#region 初始化
		public ScriptViewModel(string name)
		{
			innerLoader = new AssemblyLoadContext(UUID.New().ToString(), true);
			this.Name = name;
		}

		public ScriptViewModel(ScriptMetadata metadata, string code)
		{
			innerLoader = new AssemblyLoadContext(Guid.NewGuid().ToString(), true);
			Name = metadata.Name;
			Description = metadata.Description;
			Text = code;
			
			//加载函数列表
			foreach(var func in metadata.ScriptFunctions.Values)
			{
				Functions.Add(new FunctionViewModel(func));
			}
			//加载外部引用包
			foreach (var reference in metadata.Imports)
			{
				if (File.Exists(reference))
				{
					var assembly = innerLoader.LoadFromAssemblyPath(reference);
					
				}
			}
		}

		#endregion


		public void Dispose()
		{
			if (innerLoader != null)
			{
				innerLoader.Unload();
				innerLoader = null;
			}
		}

	}
}
