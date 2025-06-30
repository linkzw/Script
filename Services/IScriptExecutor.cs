using Script.Mode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script.Services
{
	/// <summary>
	/// 脚本执行器接口
	/// </summary>
	public interface IScriptExecutor
	{
		/// <summary>
		/// 加载脚本
		/// </summary>
		 CompilationResult LoadScriptAsync(ScriptContent content);

		/// <summary>
		/// 卸载脚本
		/// </summary>
		Ret UnloadScript(string scriptId);

		/// <summary>
		/// 执行脚本函数
		/// </summary>
		ScriptExecutionResult ExecuteFunctionAsync(ScriptExecutionContext context);

		/// <summary>
		/// 获取脚本中的函数列表
		/// </summary>
		List<ScriptFunction> GetScriptFunctions(string scriptName);
	}
}
