using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Script.Mode
{
	public enum LogLevel
	{
		INFO,
		WARN,
		ERROR
	}


	public class LogEntry
	{
		public string Message { get; set; }
		public LogLevel Level { get; set; }
		public DateTime Timestamp { get; set; }
	}
}
