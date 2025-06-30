using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script.Mode
{
	/// <summary>
	/// 编译结果
	/// </summary>
	public class CompilationResult
	{
		public bool Success { get; set; }
		public List<string> Errors { get; set; } = new();
	}
}
