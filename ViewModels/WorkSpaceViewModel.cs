using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.CodeAnalysis;
using RoslynPad.Roslyn;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Script.Services;
using Script.Services.lmpl;
using Script.Mode;
using Script.Views.Dialogs;
using Script.Utils;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.Metadata;
using System.Collections;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Elfie.Model;

namespace Script.ViewModels
{
	public partial class WorkSpaceViewModel : ObservableObject
	{
		private readonly IScriptManager _scriptManageService = new ScriptManager();
		private readonly IScriptExecutor _scriptExecutor = new ScriptExecutor();
		//private readonly IAIAgentService aIAgentService = new AIAgentService();


		#region 事件
	
		public event EventHandler<ScriptViewModel> CurrentScriptChanged;
		public event EventHandler ScriptUpdated;

		#endregion

		#region 公共属性
		public RoslynHost Host { get; private set; }
		[ObservableProperty] string runStatus;
		[ObservableProperty] string runResult;
		[ObservableProperty] ScriptViewModel? currentScript;
		[ObservableProperty] ScriptViewModel? selectedScript;
		[ObservableProperty] FunctionViewModel currentFunction;
		[ObservableProperty] int addParameterTypeIndex;
		[ObservableProperty] string addParameterName;
		[ObservableProperty] string aIRequestText;
		[ObservableProperty] string aIResponseText;
		[ObservableProperty] bool testRunning;
		[ObservableProperty] bool aIRunning;
		[ObservableProperty] string aIResponseMarkdown;
		[ObservableProperty] bool functionEditEnable = false;

		public bool IsModified { get;  set; } = false;

		public ObservableCollection<ScriptViewModel> Scripts { get; private set; } = new ObservableCollection<ScriptViewModel>();

		private static PrintOptions PrintOptions { get; } =
	new PrintOptions { MemberDisplayFormat = MemberDisplayFormat.SeparateLines };

		private static MethodInfo HasSubmissionResult { get; } =
			typeof(Compilation).GetMethod(nameof(HasSubmissionResult), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new MissingMemberException(nameof(HasSubmissionResult));


		#endregion

		#region 初始化
		public WorkSpaceViewModel()
		{
			Init();
		}



		public void Init()
		{
			List<ScriptMetadata> metadatas;
			Dictionary<string, ScriptContent> contentsDic;
			_scriptManageService.GetAllScripts(out metadatas);
			_scriptManageService.GetAllContentsDic(out contentsDic);
			foreach (var metadata in metadatas)
			{
				Scripts.Add(new ScriptViewModel(metadata, contentsDic[metadata.Name].SourceCode));
			}
			Host = new CustomRoslynHost(additionalAssemblies: new[]
			{
					Assembly.Load("RoslynPad.Roslyn.Windows"),
					Assembly.Load("RoslynPad.Editor.Windows")
				}, RoslynHostReferences.NamespaceDefault.With(assemblyReferences: new[]
				{
					typeof(object).Assembly,
					typeof(System.Text.RegularExpressions.Regex).Assembly,
					typeof(Enumerable).Assembly,
				}));

		}
		#endregion

		#region 命令

		[RelayCommand]
		public void AddScript()
		{
			AddItemDialog dialog = new AddItemDialog();
			if (dialog.ShowDialog() == true)
			{
				if (!string.IsNullOrEmpty(dialog.Input) && !Scripts.Any(item => item.Name == dialog.Input))
				{
					ScriptMetadata metadata;
					_scriptManageService.CreateScript(out metadata, dialog.Input);
					Scripts.Add(new ScriptViewModel(metadata, ""));
				}
			}
		}

		[RelayCommand] 
		public void RemoveScript() 
		{ 
		
		}

		[RelayCommand]
		public void  OpenScript(ScriptViewModel selectScript)
		{
			if (CurrentScript != null && selectedScript != null && selectedScript.Name == CurrentScript.Name) { return; }

			if(FunctionEditEnable)
			{
				InfoDialog dialog = new InfoDialog("函数描述正在修改中，请先退出编辑状态",false);
				dialog.ShowDialog();
				return;
			}

			var modifiyFlag =  QueryCurrMetadataModifiedState();
			if ((modifiyFlag || IsModified) && CurrentScript != null)
			{
				InfoDialog dialog = new InfoDialog("脚本已被修改，是否丢失已修改的内容？");
				if (dialog.ShowDialog() == true)
				{
					var metadata = new ScriptMetadata();
					var content = new ScriptContent();
					_scriptManageService.GetScriptMetadata(CurrentScript.Name, ref metadata);
					_scriptManageService.GetScriptContent(CurrentScript.Name, ref content);
					var scriptvm = new ScriptViewModel(metadata, content.SourceCode);
					int index = 0;
					for (int i = 0; i < Scripts.Count; i++)
					{
						if (Scripts[i].Name == CurrentScript.Name)
						{
							index = i;
							break;
						}
					}
					Scripts[index] = scriptvm;
					//return;
				}
				else
				{
					return;
				}
			}
			CurrentScript = selectScript;
			CurrentScriptChanged?.Invoke(this, selectScript);
			IsModified = false;

		}


		[RelayCommand]
		public void CompileAndSave()
		{
			if(CurrentScript == null) return;
			if (FunctionEditEnable)
			{
				InfoDialog dialog = new InfoDialog("函数描述正在修改中，请先退出编辑状态", false);
				dialog.ShowDialog();
				return;
			}

			ScriptMetadata scriptMetadata = new();
			ScriptContent scriptContent = new ScriptContent();
			_scriptManageService.GetScriptMetadata(CurrentScript.Name, ref scriptMetadata);
			_scriptManageService.GetScriptContent(CurrentScript.Name, ref scriptContent);

			SyntaxTree tree = CSharpSyntaxTree.ParseText(CurrentScript.Text);
			CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

			// 2. 创建编译对象（需要添加基础引用）
			var compilation = CSharpCompilation.Create("ScriptAssembly")
				.AddReferences(
					MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
					MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
				)
				.AddSyntaxTrees(tree);

			// 3. 获取语义模型
			var semanticModel = compilation.GetSemanticModel(tree);

			var fullFormat = new SymbolDisplayFormat(
				typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
				genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
				miscellaneousOptions:
				SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
			// 注意：**不要添加 UseSpecialTypes**
			);

			// 4. 提取所有方法声明
			var methodDeclarations = root.DescendantNodes()
				.OfType<MethodDeclarationSyntax>();
			var hashSetfnc = new HashSet<string>();
			foreach (var method in methodDeclarations)
			{
				var methodSymbol = semanticModel.GetDeclaredSymbol(method) as IMethodSymbol;

				var fnc = new ScriptFunction
				{
					Name = method.Identifier.Text,
					ReturnType = methodSymbol?.ReturnType.ToString() ?? "void"
				};

				foreach (var parameter in methodSymbol.Parameters)
				{
					var type = parameter.Type;
					string fullTypeName = type.ToDisplayString(fullFormat);
					fnc.Parameters.Add(new FunctionParameter()
					{
						Name = parameter.Name,
						TypeName = parameter.Type.Name,
						TypeFullName = fullTypeName,
						DefaultValue = parameter.HasExplicitDefaultValue  ? parameter.ExplicitDefaultValue : null

					});
				}

				/*

				// 获取参数信息
				foreach (var parameter in method.ParameterList.Parameters)
				{
					var paramSymbol = semanticModel.GetDeclaredSymbol(parameter) as IParameterSymbol;
					EqualsValueClauseSyntax defaultValue = parameter.Default;
					fnc.Parameters.Add(new FunctionParameter()
					{
						Name = parameter.Identifier.Text,
						TypeName = paramSymbol?.Type.ToString() ?? "object", 
						TypeFullName = paramSymbol?.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "Object",
						DefaultValue = defaultValue == null?null: semanticModel.GetConstantValue(defaultValue.Value).Value

					});
				}
				*/
				hashSetfnc.Add(fnc.Sha256Hash);
				if (!scriptMetadata.ScriptFunctions.ContainsKey(fnc.Sha256Hash))
				{
					scriptMetadata.ScriptFunctions.Add(fnc.Sha256Hash, fnc);
				}
				else
				{
					for(int i=0;i< scriptMetadata.ScriptFunctions[fnc.Sha256Hash].Parameters.Count; i++)
					{
						scriptMetadata.ScriptFunctions[fnc.Sha256Hash].Parameters[i].DefaultValue = fnc.Parameters[i].DefaultValue;
					}
				}
			}
			// 执行删除操作
			// 获取所有需要删除的键
			var keysToRemove = scriptMetadata.ScriptFunctions.Keys.Where(k => !hashSetfnc.Contains(k)).ToList();

			// 删除这些键
			foreach (var key in keysToRemove)
			{
				scriptMetadata.ScriptFunctions.Remove(key);
			}
			scriptContent.SourceCode = CurrentScript.Text;
			_scriptManageService.SaveScript(scriptMetadata, scriptContent);
		}

		[RelayCommand]
		private void ModifyFuncDescription()
		{

			FunctionEditEnable = true;
		}

		[RelayCommand]
		private void CancelFuncDescription()
		{
			if (CurrentScript == null) return ;
			var script = new ScriptMetadata();
			_scriptManageService.GetScriptMetadata(CurrentScript.Name, ref script);
			for(int i=0;i<CurrentScript.Functions.Count;i++)
			{
				if (CurrentScript.Functions[i].Sha256Hash == CurrentFunction.Sha256Hash)
				{
					CurrentScript.Functions[i] = new FunctionViewModel(script.ScriptFunctions[CurrentFunction.Sha256Hash]);
					CurrentFunction = CurrentScript.Functions[i];
					break;
				}
			}

			FunctionEditEnable = false;
		}

		[RelayCommand]
		private void SaveFuncDescription()
		{
			var script = new ScriptMetadata();
			ScriptContent scriptContent = new ScriptContent();
			_scriptManageService.GetScriptMetadata(CurrentScript.Name, ref script);
			_scriptManageService.GetScriptContent(CurrentScript.Name, ref scriptContent);


			script.ScriptFunctions[CurrentFunction.Sha256Hash].Description = CurrentFunction.Description;
			for (int i = 0; i < script.ScriptFunctions[CurrentFunction.Sha256Hash].Parameters.Count; i++)
			{
				script.ScriptFunctions[CurrentFunction.Sha256Hash].Parameters[i].Description = CurrentFunction.Parameters[i].Description;
			}
			_scriptManageService.SaveScript(script, scriptContent);
			FunctionEditEnable = false;
		}

		[RelayCommand]
		private void RunScriptFunc()
		{
			var scriptExecutionContext = new ScriptExecutionContext() 
			{ 
				ScriptName = CurrentScript.Name,
				FunctionName = CurrentFunction.Name,
				Parameters = CurrentFunction.Parameters.Select(a => a.DefaultValue).ToList(),
				ParametersTypes = CurrentFunction.Parameters.Select(a => Utils.Utils.GetTypeforName(a.TypeFullName)).ToList(),
			};

			var script = new ScriptContent();
			_scriptManageService.GetScriptContent(CurrentScript.Name, ref script);


			_scriptExecutor.LoadScriptAsync(script);

			var result = _scriptExecutor.ExecuteFunctionAsync(scriptExecutionContext);
		}
		#endregion


		#region  普通函数

		/// <summary>
		/// 查询当前脚本的metadata是否被修改(看各个Description)
		/// </summary>
		/// <returns>修改返回true，未修改返回false</returns>
		private bool QueryCurrMetadataModifiedState()
		{
			if (CurrentScript == null) return false;
			var script = new ScriptMetadata();
			_scriptManageService.GetScriptMetadata(CurrentScript.Name, ref script);
			if (script == null) return true;
			if(script.Description!=CurrentScript.Description) { return true; }

			// 创建参数级的字典，使用(函数名, 参数名)作为复合键
			var viewModelParamDict = CurrentScript.Functions
				.SelectMany(f => f.Parameters
					.Select(p => new
					{
						Key = (FuncName: f.Sha256Hash, ParamName: p.Name),
						Param = p
					}))
				.ToDictionary(x => x.Key, x => x.Param);

			// 检查所有元素
			foreach (var scriptFunc in script.ScriptFunctions.Values)
			{
				// 检查函数描述
				var viewModelFunc = CurrentScript.Functions
					.FirstOrDefault(f => f.Name == scriptFunc.Name);

				if (viewModelFunc != null && scriptFunc.Description != viewModelFunc.Description)
				{
					return true;
				}

				// 检查参数描述
				foreach (var scriptParam in scriptFunc.Parameters)
				{
					var key = (scriptFunc.Name, scriptParam.Name);
					if (viewModelParamDict.TryGetValue(key, out var viewModelParam))
					{
						if (scriptParam.Description != viewModelParam.Description)
						{
							return true;
						}
					}
				}
			}

			return false;
		}


		#endregion

	}

}
