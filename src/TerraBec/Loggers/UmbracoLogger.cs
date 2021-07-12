using System;
using System.Diagnostics;
using Umbraco.Core;

namespace Terrabec.Loggers
{
	public class UmbracoLogger : ILogger
	{
		public void Debug(string msg) => ApplicationContext.Current.ProfilingLogger.Logger.Debug(new StackTrace().GetFrame(1).GetMethod().ReflectedType, msg);
		public void Error(string msg, Exception ex) => ApplicationContext.Current.ProfilingLogger.Logger.Error(new StackTrace().GetFrame(1).GetMethod().ReflectedType, msg, ex);
		public void Fatal(string msg, Exception ex) => ApplicationContext.Current.ProfilingLogger.Logger.Error(new StackTrace().GetFrame(1).GetMethod().ReflectedType, msg, ex);
		public void Info(string msg) => ApplicationContext.Current.ProfilingLogger.Logger.Info(new StackTrace().GetFrame(1).GetMethod().ReflectedType, msg);
		public void Warn(string msg) => ApplicationContext.Current.ProfilingLogger.Logger.Warn(new StackTrace().GetFrame(1).GetMethod().ReflectedType, msg);


	}
}
