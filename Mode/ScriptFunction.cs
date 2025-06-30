using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script.Mode
{
	/// <summary>
	/// 脚本函数定义
	/// </summary>
	public class ScriptFunction
	{
		/// <summary>函数名称</summary>
		public string Name { get; set; }

		/// <summary>函数描述</summary>
		public string Description { get; set; }

		/// <summary>参数列表</summary>
		public List<FunctionParameter> Parameters { get; set; } = new();

		/// <summary>返回值类型</summary>
		public string ReturnType { get; set; } = "void";

		public string Sha256Hash
		{
			get
			{
				string str = ReturnType + Name;
				foreach (var parameter in Parameters)
				{
					str += parameter.TypeName;
					str += parameter.Name;
				}
				return Utils.Utils.ComputeSHA256Hash(str);
			}
		}

	}
}
