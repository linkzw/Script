using CommunityToolkit.Mvvm.ComponentModel;
using Script.Mode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script.ViewModels
{
	public partial class ParameterViewModel:ObservableObject
	{
		#region 公共属性

		[ObservableProperty] string name;
		[ObservableProperty] string description;
		[ObservableProperty] string typeName;
		[ObservableProperty] string typeFullName;
		[ObservableProperty] Object defaultValue;
		#endregion

		#region 初始化
		public ParameterViewModel() { }

		public ParameterViewModel(FunctionParameter parameter)
		{
			Name = parameter.Name;
			Description = parameter.Description;
			TypeName = parameter.TypeName;
			TypeFullName = parameter.TypeFullName;
			DefaultValue = parameter.DefaultValue;
		}

		#endregion

	}
}
