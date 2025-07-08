using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script.Utils
{
	public class LogMessage : ValueChangedMessage<string>
	{
		public LogMessage(string value) : base(value) { }
	}

	public class LogMain
	{
		public static void Print(string args)
		{
			WeakReferenceMessenger.Default.Send(new LogMessage(args));
		}

		public static void Logrint(string args)
		{
			WeakReferenceMessenger.Default.Send(new LogMessage(args));
		}
	}
}
