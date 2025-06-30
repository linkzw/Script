using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script.Mode
{
	/// <summary>
	/// 脚本执行结果
	/// </summary>
	public class ScriptExecutionResult
	{
		/// <summary>是否成功</summary>
		public bool IsSuccess { get; set; }

		/// <summary>返回值</summary>
		public object ReturnValue { get; set; }

		/// <summary>错误信息</summary>
		public string Error { get; set; }

		/// <summary>执行耗时</summary>
		public TimeSpan ExecutionDuration { get; set; }

		/// <summary>日志记录</summary>
		public List<string> Logs { get; set; } = new();
	}
}
