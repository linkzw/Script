using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script.Services
{
	/// <summary>
	/// SDK 主入口
	/// </summary>
	public interface IMotionControlScriptSDK
	{
		/// <summary>脚本管理器</summary>
		IScriptManager ScriptManager { get; }

		/// <summary>脚本执行器</summary>
		IScriptExecutor ScriptExecutor { get; }

		/// <summary>函数注册器</summary>
		//IFunctionRegister FunctionRegister { get; }

		/// <summary>变量访问器</summary>
		//IVariableAccessor VariableAccessor { get; }

		/// <summary>
		/// 初始化SDK
		/// </summary>
		void Initialize();

		/// <summary>
		/// 关闭SDK
		/// </summary>
		void Shutdown();
	}
}
