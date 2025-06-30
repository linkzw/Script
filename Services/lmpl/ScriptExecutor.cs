using CSScripting;
using CSScriptLib;
using Microsoft.CodeAnalysis.Scripting;
using Script.Mode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Script.Services.lmpl
{
	public class ScriptExecutor : IScriptExecutor
	{
		Dictionary<string, dynamic> _scriptObject   = new Dictionary<string, dynamic>();


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
			_scriptObject[context.ScriptName].ProcessValues(context.Parameters.ToArray());
			var paramTypes = context.ParametersTypes.ToArray();
			var method = _scriptObject[context.ScriptName].GetMethod(context.FunctionName, paramTypes);
			if(method != null)
			{
				try
				{
					var stopwatch = Stopwatch.StartNew();
					var result = method.Invoke(context.Parameters.ToArray());
					stopwatch.Stop();
					scriptExecutionResult.ReturnValue = result;
					scriptExecutionResult.ExecutionDuration = stopwatch.Elapsed;
				}
				catch (Exception ex)
				{
					scriptExecutionResult.Error = ex.Message;
					scriptExecutionResult.IsSuccess = false;

				}
			}
			else
			{
				//不存在函数
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
				dynamic scripyobject = CSScript.Evaluator.LoadCode(content.SourceCode);
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
