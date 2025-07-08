using CommunityToolkit.Mvvm.ComponentModel;
using Script.Mode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script.ViewModels
{
	public partial class FunctionViewModel : ObservableObject
	{
		#region 公共属性

		[ObservableProperty] string name;

		public string DisplayName
		{
			get
			{
				var sb = new StringBuilder();
				sb.Append(Name).Append('(');

				// 遍历所有参数
				for (int i = 0; i < Parameters.Count; i++)
				{
					var param = Parameters[i];

					// 添加参数类型和名称
					sb.Append(param.TypeName).Append(' ').Append(param.Name);

					// 如果不是最后一个参数，添加逗号分隔
					if (i < Parameters.Count - 1)
					{
						sb.Append(", ");
					}
				}

				sb.Append(')');
				return sb.ToString();
			}
		}

		[ObservableProperty] string description;

		[ObservableProperty] string returnType = "void";

		public ObservableCollection<ParameterViewModel> Parameters { get; set; } = new ObservableCollection<ParameterViewModel>();
		public string ID { get; set; }

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

		[ObservableProperty] ScriptExecutionResult runResult;

		#endregion

		#region 初始化

		public FunctionViewModel()
		{
			ID = Guid.NewGuid().ToString();
		}

		public FunctionViewModel(ScriptFunction function)
		{
			ID = Guid.NewGuid().ToString();
			Name = function.Name;
			Description = function.Description;
			ReturnType = function.ReturnType;
			foreach(var paramter in function.Parameters)
			{
				Parameters.Add(new ParameterViewModel(paramter));
			}
		}


		#endregion

	}
}
