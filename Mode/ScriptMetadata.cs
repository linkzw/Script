using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script.Mode
{
	/// <summary>
	/// 脚本元数据
	/// </summary>
	public class ScriptMetadata
	{
		/// <summary>脚本唯一标识</summary>
		public string ScriptId { get; set; } = Guid.NewGuid().ToString();

		/// <summary>脚本名称</summary>
		public string Name { get; set; } = "New Script";

		/// <summary>所属分组</summary>
		public string Group { get; set; } = "Default";

		/// <summary>脚本描述</summary>
		public string Description { get; set; } = "";

		/// <summary>入口函数名</summary>
		//public string EntryFunction { get; set; }

		/// <summary>函数列表</summary>
		public List<ScriptFunction> ScriptFunctions { get; set; } = new();

		/// <summary>引用的第三方库</summary>
		public List<string> References { get; set; } = new();

		/// <summary>导入的命名空间</summary>
		public List<string> Imports { get; set; } = new();

		/// <summary>创建时间</summary>
		public DateTime CreatedTime { get; set; } = DateTime.Now;

		/// <summary>修改时间</summary>
		public DateTime ModifiedTime { get; set; } = DateTime.Now;
	}
}
