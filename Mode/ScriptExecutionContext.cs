using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script.Mode
{
	/// <summary>
	/// 脚本执行上下文
	/// </summary>
	public class ScriptExecutionContext
	{
		/// <summary>脚本ID</summary>
		public string ScriptName { get; set; }

		/// <summary>函数名称</summary>
		public string FunctionName { get; set; }

		/// <summary>参数值</summary>
		public List<object> Parameters { get; set; } = new();

		public List<Type> ParametersTypes { get; set; } = new();

		/// <summary>执行超时时间</summary>
		public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
	}
}
