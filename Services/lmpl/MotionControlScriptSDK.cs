using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script.Services.lmpl
{
	public class MotionControlScriptSDK : IMotionControlScriptSDK
	{


		//public IFunctionRegister FunctionRegister ;

		//public IVariableAccessor VariableAccessor;

		public MotionControlScriptSDK()
		{

		}

		public IScriptManager ScriptManager => new ScriptManager();

		public IScriptExecutor ScriptExecutor => new ScriptExecutor();

		public void Initialize()
		{
			throw new NotImplementedException();
		}

		public void Shutdown()
		{
			throw new NotImplementedException();
		}
	}
}
