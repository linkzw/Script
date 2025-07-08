using CSScripting;
using CSScriptLib;
using Microsoft.CodeAnalysis.Scripting;
using RoslynPad.Editor;
using Script.Mode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Script.Services.lmpl
{
	public class ScriptExecutor : IScriptExecutor
	{
		Dictionary<string, Object> _scriptObject   = new Dictionary<string, Object>();
		Dictionary<string, Type> _scriptType = new Dictionary<string, Type>();

		public ScriptExecutionResult ExecuteFunctionAsync(ScriptExecutionContext context)
		{
			if(!_scriptObject.ContainsKey(context.ScriptName))
			{
				//不存在脚本
			}
			ScriptExecutionResult scriptExecutionResult  = new()
			{ 
				IsSuccess = true
			};
			//_scriptObject[context.ScriptName].ProcessValues(context.Parameters.ToArray());
			var paramTypes = context.ParametersTypes.ToArray();
			var method = _scriptType[context.ScriptName].GetMethod(context.FunctionName, paramTypes);
			if(method != null)
			{
				try
				{
					var stopwatch = Stopwatch.StartNew();
					var result = method.Invoke(_scriptObject[context.ScriptName],context.Parameters.ToArray());
					stopwatch.Stop();
					scriptExecutionResult.ReturnValue = result;
					scriptExecutionResult.ExecutionDuration = stopwatch.Elapsed;
				}
				catch (Exception ex)
				{
					scriptExecutionResult.Error = ex.Message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
					scriptExecutionResult.IsSuccess = false;

				}
			}
			else
			{
				//不存在函数
				var sb = new StringBuilder();
				sb.Append(context.FunctionName).Append('(');

				// 遍历所有参数
				for (int i = 0; i < context.ParametersTypes.Count; i++)
				{
					var param = context.ParametersTypes[i];

					// 添加参数类型和名称
					sb.Append(param.FullName);

					// 如果不是最后一个参数，添加逗号分隔
					if (i < context.ParametersTypes.Count - 1)
					{
						sb.Append(", ");
					}
				}

				sb.Append(')');
				
				scriptExecutionResult.IsSuccess = false;
				scriptExecutionResult.Error.Add($"脚本{context.ScriptName}不存在函数{sb.ToString()}");
			}
			return scriptExecutionResult;
		}

		public List<ScriptFunction> GetScriptFunctions(string scriptName)
		{
			List<ScriptFunction> fncList = new List<ScriptFunction>();
			// 获取类型信息
			Type scriptType = (Type)_scriptObject[scriptName].GetType();

			// 获取所有公共方法
			var publicMethods = scriptType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
				.Where(m => !m.IsSpecialName);

			foreach (var method in publicMethods)
			{
				ScriptFunction tmp = new ScriptFunction()
				{
					Name = method.Name,
					ReturnType = method.ReturnType.Name,
					Parameters = method.GetParameters().Select(b => new FunctionParameter
					{
						Name = b.Name,
						TypeName = b.ParameterType.Name
					}).ToList()
				};

			}

			return fncList;
		}

		public CompilationResult LoadScriptAsync(ScriptContent content)
		{
			CompilationResult compilationResult = new()
			{
				Success = true,
				Errors = new List<string>()
			};

			try
			{
				dynamic scripyobject = CSScript.Evaluator
					.ReferenceAssembly(typeof(object).Assembly)        // mscorlib.dll / System.Runtime.dll
					.ReferenceAssembly(typeof(Encoding).Assembly)      // System.Text.Encoding (System.Text.Encoding.dll)
					.ReferenceAssembly(typeof(Enumerable).Assembly)    // System.Linq (System.Linq.dll)
					.LoadCode(content.SourceCode);
				//dynamic scripyobject = CSScript.Evaluator.LoadCode(content.SourceCode);
				Type scriptType = scripyobject.GetType();
				if (!_scriptObject.ContainsKey(content.Name))
				{
					_scriptObject.Add(content.Name, scripyobject);
					_scriptType.Add(content.Name, scriptType);
				}
				else
				{
					_scriptObject[content.Name] = scripyobject;
					_scriptType[content.Name] = scriptType;
				}
				
			}
			catch (CSScriptLib.CompilerException ex)
			{

				compilationResult.Errors = ex.Message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
				compilationResult.Success = false;
				return compilationResult;
			}
			return compilationResult;
		}

		public Ret UnloadScript(string scriptName)
		{
			var scriptAssemblyType = (Type)_scriptObject[scriptName].GetType();

			scriptAssemblyType.Assembly.Unload();
			_scriptObject.Remove(scriptName);

			return new Ret();
		}
	}
}
