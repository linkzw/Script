using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script.Mode
{
	/// <summary>
	/// 函数参数定义
	/// </summary>
	public class FunctionParameter
	{
		/// <summary>参数名称</summary>
		public string Name { get; set; }

		/// <summary>参数类型</summary>
		public string TypeName { get; set; }

		public string TypeFullName { get; set; }

		/// <summary>参数描述</summary>
		public string Description { get; set; }

		/// <summary>默认值</summary>
		public Object? DefaultValue { get; set; }
	}
}
