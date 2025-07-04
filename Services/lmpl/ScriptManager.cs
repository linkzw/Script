using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Script.Mode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Diagnostics;
using System.Collections;
using System.Windows.Media;
using Script.Utils;
using Microsoft.CodeAnalysis;

namespace Script.Services.lmpl
{
	public class ScriptManager : IScriptManager
	{
		#region 私有字段
		Dictionary<string , ScriptContent> _scriptContentsDic = new Dictionary<string, ScriptContent>();
		Dictionary<string, ScriptMetadata> _scriptMetadataDic = new Dictionary<string, ScriptMetadata>();
		string _loadPath;
		#endregion

		public ScriptManager()
		{
			//设置读取路径
			_loadPath = "D:\\Scripts";
			LoadScript();
		}


		public Ret CreateScript(out ScriptMetadata metadata, string name)
		{
			if(_scriptMetadataDic.ContainsKey(name))
			{
				metadata = null;
				//存在同名
				return new Ret();
			}

			ScriptMetadata tmpMetadata = new ScriptMetadata()
			{ Name = name };
			ScriptContent content = new ScriptContent()
			{ Name = tmpMetadata.Name};

			_scriptContentsDic.Add(tmpMetadata.Name, content);
			_scriptMetadataDic.Add(tmpMetadata.Name, tmpMetadata);
			SaveScript(tmpMetadata, content);
			metadata = tmpMetadata;

			return new Ret();
		}

		public Ret DeleteScript(string name)
		{
			_scriptMetadataDic.Remove(name);
			_scriptContentsDic.Remove(name);
			return new Ret();
		}

		public Ret GetAllScripts(out List<ScriptMetadata> metadatas)
		{
			List<ScriptMetadata> list = new List<ScriptMetadata>();

			foreach(var item in _scriptMetadataDic.Values)
			{
				list.Add(Utils.Utils.DeepCopy(item));
			}

			metadatas = list;
			return new Ret();
		}

		public Ret GetAllContents(out List<ScriptContent> contents)
		{
			List<ScriptContent> list = new List<ScriptContent>();

			foreach (var item in _scriptContentsDic.Values)
			{
				list.Add(Utils.Utils.DeepCopy(item));
			}

			contents = list;
			return new Ret();
		}

		public Ret GetAllContentsDic(out Dictionary<string, ScriptContent> contents)
		{
			Dictionary<string,ScriptContent> dic = new Dictionary<string,ScriptContent>();

			foreach (var item in _scriptContentsDic.Values)
			{
				dic.Add(item.Name ,Utils.Utils.DeepCopy(item));
			}

			contents = dic;
			return new Ret();
		}

		public Ret GetScriptContent(string name, ref ScriptContent content)
		{
			if(!_scriptContentsDic.ContainsKey(name))
			{
				//不存在的脚本

				content = null;
				return new Ret();
			}

			ScriptContent tmpContent = new ScriptContent()
			{ 
				Name = name,
				SourceCode = _scriptContentsDic[name].SourceCode,
				CompilationStatus = _scriptContentsDic[name].CompilationStatus
			};

			content = tmpContent;

			return new Ret(); 
		}

		public Ret GetScriptGroups(out List<string> groups)
		{
			throw new NotImplementedException();
		}

		public Ret GetScriptMetadata(string name, ref ScriptMetadata metadata)
		{
			if (!_scriptContentsDic.ContainsKey(name))
			{
				//不存在的脚本

				metadata = null;
				return new Ret();
			}

			metadata = Utils.Utils.DeepCopy(_scriptMetadataDic[name]);

			return new Ret();
		}

		public Ret GetScriptsByGroup(out Dictionary<string, List<ScriptMetadata>> scripts)
		{
			throw new NotImplementedException();
		}

		public Ret LoadScriptFromFile()
		{
			throw new NotImplementedException();
		}

		public Ret SaveScript(ScriptMetadata metadata, ScriptContent content)
		{
			if (metadata.Name != content.Name)
			{
				//返回错误
				return new Ret();
			}

			if (!_scriptMetadataDic.ContainsKey(metadata.Name))
			{
				_scriptMetadataDic.Add(metadata.Name, new ScriptMetadata());
			}
			if (!_scriptContentsDic.ContainsKey(metadata.Name))
			{
				_scriptContentsDic.Add(metadata.Name, new ScriptContent());
			}

			_scriptMetadataDic[metadata.Name] = metadata;
			_scriptContentsDic[metadata.Name] = content;

			Utils.Utils.SaveModel(_scriptMetadataDic, Path.Combine(_loadPath, "ScriptMetadata.json"));
			File.WriteAllText(Path.Combine(_loadPath, content.Name + ".csx"), content.SourceCode);
			//Utils.Utils.SaveModel(content.SourceCode, Path.Combine(_loadPath, content.Name + ".csx"));
			return new Ret();
		}

		#region 私有函数

		public Ret LoadScript()
		{
			if(!Directory.Exists(_loadPath)) return new Ret();
			_scriptMetadataDic = Utils.Utils.LoadModel<Dictionary<string, ScriptMetadata>>(Path.Combine(_loadPath, "ScriptMetadata.json"));

			if (_scriptMetadataDic != null)
			{
				foreach (var metadata in _scriptMetadataDic.Values)
				{
					string scriptFile = Path.Combine(_loadPath , metadata.Name + ".csx");
					string content;
					List<ScriptFunction> fncs = new List<ScriptFunction>();
					if (File.Exists(scriptFile))
					{
						content = File.ReadAllText(scriptFile);
						fncs = GetFunctionsWithParameters(content);
					}
					else
					{
						content = "";
					}

					_scriptContentsDic.Add(metadata.Name, new ScriptContent()
					{
						Name = metadata.Name,
						SourceCode = content,
						CompilationStatus = CompilationStatus.NotCompiled
					});

					var hashSetfnc = new HashSet<string>(fncs.Select(item => item.Sha256Hash));

					var keysToRemove = metadata.ScriptFunctions.Keys.Except(hashSetfnc).ToList();

					// 执行删除操作
					foreach (var key in keysToRemove)
					{
						metadata.ScriptFunctions.Remove(key);
					}
					//没有的就加
					foreach (var fnc in fncs)
					{
						if(!metadata.ScriptFunctions.ContainsKey(fnc.Sha256Hash))
						{
							metadata.ScriptFunctions.Add(fnc.Sha256Hash, fnc);
						}
					}

				}
			}
			else
			{
				_scriptMetadataDic = new Dictionary<string, ScriptMetadata>();
			}

			return new Ret();
		}

		public static List<ScriptFunction> GetFunctionsWithParameters(string scriptCode)
		{
			var functions = new List<ScriptFunction>();

			SyntaxTree tree = CSharpSyntaxTree.ParseText(scriptCode);
			CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

			var compilation = CSharpCompilation.Create("ScriptAssembly")
				.AddReferences(
					MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
					MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
				)
				.AddSyntaxTrees(tree);

			var semanticModel = compilation.GetSemanticModel(tree);
			var fullFormat = new SymbolDisplayFormat(
				typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
				genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
				miscellaneousOptions:
				SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
			// 注意：**不要添加 UseSpecialTypes**
			);

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
						DefaultValue = parameter.HasExplicitDefaultValue ? parameter.ExplicitDefaultValue : null

					});
				}
				functions.Add(fnc);
			}

			return functions;
		}

		#endregion
	}
}
