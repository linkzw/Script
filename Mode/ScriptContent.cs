using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script.Mode
{

	public enum CompilationStatus
	{
		NotCompiled,
		CompilationSucceeded,
		CompilationFailed
	}

	/// <summary>
	/// 脚本内容
	/// </summary>
	public class ScriptContent
	{
		/// <summary>脚本名</summary>
		public string Name { get; set; }

		/// <summary>源代码</summary>
		public string SourceCode { get; set; }

		/// <summary>编译状态</summary>
		public CompilationStatus CompilationStatus { get; set; }
	}
}
