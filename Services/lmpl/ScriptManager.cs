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
			LoadScript();
		}


		public Ret CreateScript(ref ScriptMetadata metadata, string name)
		{
			if(!_scriptMetadataDic.ContainsKey(name))
			{
				//存在同名
				return new Ret();
			}

			ScriptMetadata tmpMetadata = new ScriptMetadata()
			{ Name = name };
			ScriptContent content = new ScriptContent()
			{ Name = tmpMetadata.Name};

			_scriptContentsDic.Add(tmpMetadata.Name, content);
			_scriptMetadataDic.Add(tmpMetadata.Name, tmpMetadata);

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

		public Ret SaveScript(ScriptMetadata metadata, ref ScriptContent content)
		{
			if(metadata.Name!=content.Name)
			{
				//返回错误
				return new Ret();
			}

			if(!_scriptMetadataDic.ContainsKey(metadata.Name))
			{
				_scriptMetadataDic.Add(metadata.Name, new ScriptMetadata());
			}
			if(!_scriptContentsDic.ContainsKey(metadata.Name))
			{
				_scriptContentsDic.Add(metadata.Name, new ScriptContent());
			}

			_scriptMetadataDic[metadata.Name] = metadata;
			_scriptContentsDic[metadata.Name] = content;

			return new Ret();                                                                                   
		}

		#region 私有函数

		public Ret LoadScript()
		{
			_scriptMetadataDic =  Utils.Utils.LoadModel<Dictionary<string, ScriptMetadata>>(_loadPath+"ScriptMetadata.json");
			
			foreach(var metadata in _scriptMetadataDic.Values)
			{
				string scriptFile = _loadPath+metadata.Name+".csx";
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

				var hashSetfnc = new HashSet<string>(fncs.Select(item=>item.Sha256Hash));

				for (int i = metadata.ScriptFunctions.Count - 1; i >= 0; i--)
				{
					if (!hashSetfnc.Contains(metadata.ScriptFunctions[i].Sha256Hash))
					{
						metadata.ScriptFunctions.RemoveAt(i);
					}
				}


				return new Ret();
			}

			string[] csFiles = Directory.GetFiles(_loadPath, "*.csx");

			// 遍历文件列表
			foreach (string filePath in csFiles)
			{
				string fileContent = File.ReadAllText(filePath);
				string name = Path.GetFileName(filePath);
				_scriptContentsDic.Add(name ,new ScriptContent() { 
					Name = name,
					SourceCode  = fileContent,
					CompilationStatus = CompilationStatus.NotCompiled
				});

				

			}

			return new Ret();
		}

		public static List<ScriptFunction> GetFunctionsWithParameters(string scriptCode)
		{
			//var scriptCode = File.ReadAllText(scriptPath);
			var syntaxTree = CSharpSyntaxTree.ParseText(scriptCode);
			var root = syntaxTree.GetRoot();

			var functions = new List<ScriptFunction>();

			// 获取所有方法声明
			foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
			{
				var funcInfo = new ScriptFunction
				{
					Name = method.Identifier.Text,
					ReturnType = method.ReturnType.ToString(),
				};

				// 获取参数列表
				foreach (var param in method.ParameterList.Parameters)
				{
					funcInfo.Parameters.Add(new FunctionParameter
					{
						Name = param.Identifier.Text,
						TypeName = param.Type?.ToString() ?? "var"
					});
				}

				functions.Add(funcInfo);
			}

			return functions;
		}

		#endregion
	}
}
