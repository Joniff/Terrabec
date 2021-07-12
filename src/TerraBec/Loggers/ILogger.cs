using System;

namespace Terrabec.Loggers
{
	public interface ILogger
	{
		void Debug(string msg);
		void Error(string msg, Exception ex);
		void Fatal(string msg, Exception ex);
		void Info(string msg);
		void Warn(string msg);
	}
}
