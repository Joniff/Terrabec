using System;
using System.Threading;

namespace Terrbec.Cache
{
	public class Locker
	{
#if TRACE2
		private const int LockWait = 1000000;
#else
		private const int LockWait = 1000;
#endif
		private readonly ReaderWriterLockSlim _slim = new ReaderWriterLockSlim();

		public bool Read(Action execute
#if TRACE2
			,[CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0
#endif
			)

		{
#if TRACE2
			var callingMethodDebug = sourceFilePath + ":" + sourceLineNumber + " Locker.Read() = ";
#endif
			var hasLocked = false;
			try
			{
				if (!_slim.TryEnterReadLock(LockWait))
				{
#if TRACE2
					Debug.WriteLine(callingMethodDebug + "failed to require lock");
#endif
					return false;
				}
#if TRACE2
				Debug.WriteLine(callingMethodDebug + "required lock");
#endif
				hasLocked = true;
				execute();
				return true;
			}
			finally
			{
				if (hasLocked)
				{
#if TRACE2
					Debug.WriteLine(callingMethodDebug + "released lock");
#endif
					_slim.ExitReadLock();
				}
			}
		}

		public T Read<T>(Func<T> execute
#if TRACE2
		, [CallerMemberName] string memberName = "", 
			[CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0
#endif			
			)
		{
#if TRACE2
			var callingMethodDebug = sourceFilePath + ":" + sourceLineNumber + " Locker.Read<T>() = ";
#endif
			var hasLocked = false;
			try
			{
				if (!_slim.TryEnterReadLock(LockWait))
				{
#if TRACE2
					Debug.WriteLine(callingMethodDebug + "failed to require lock");
#endif
					return default(T);
				}
#if TRACE2
				Debug.WriteLine(callingMethodDebug + "required lock");
#endif
				hasLocked = true;
				return execute();
			}
			finally
			{
				if (hasLocked)
				{
#if TRACE2
					Debug.WriteLine(callingMethodDebug + "released lock");
#endif
					_slim.ExitReadLock();
				}
			}
		}

		public bool Write(Action execute
#if TRACE2
		, [CallerMemberName] string memberName = "", 
			[CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0
#endif
			)
		{
#if TRACE2
			var callingMethodDebug = sourceFilePath + ":" + sourceLineNumber + " Locker.Write() = ";
#endif
			var hasLocked = false;
			try
			{
				if (!_slim.TryEnterWriteLock(LockWait))
				{
#if TRACE2
					Debug.WriteLine(callingMethodDebug + "failed to require lock");
#endif
					return false;
				}
#if TRACE2
				Debug.WriteLine(callingMethodDebug + "required lock");
#endif
				hasLocked = true;
				execute();
				return true;
			}
			finally
			{
				if (hasLocked)
				{
#if TRACE2
					Debug.WriteLine(callingMethodDebug + "released lock");
#endif
					_slim.ExitWriteLock();
				}
			}
		}

		public T Write<T>(Func<T> execute
#if TRACE2
		, [CallerMemberName] string memberName = "", 
			[CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0
#endif			
			)
		{
#if TRACE2
			var callingMethodDebug = sourceFilePath + ":" + sourceLineNumber + " Locker.Write<T>() = ";
#endif
			var hasLocked = false;
			try
			{
				if (!_slim.TryEnterWriteLock(LockWait))
				{
#if TRACE2
					Debug.WriteLine(callingMethodDebug + "failed to require lock");
#endif
					return default(T);
				}
#if TRACE2
				Debug.WriteLine(callingMethodDebug + "required lock");
#endif
				hasLocked = true;
				return execute();
			}
			finally
			{
				if (hasLocked)
				{
#if TRACE2
					Debug.WriteLine(callingMethodDebug + "released lock");
#endif
					_slim.ExitWriteLock();
				}
			}
		}

		bool Upgradable(Func<bool> executeReadBefore, Action executeWrite, Action<bool> executeReadAfter)
		{
			var hasReadLocked = false;
			try
			{
				if (!_slim.TryEnterUpgradeableReadLock(LockWait))
				{
					return false;
				}
				hasReadLocked = true;
				var doRunWrite = executeReadBefore();
				if (doRunWrite)
				{
					var hasWriteLock = false;
					try
					{
						if (!_slim.TryEnterWriteLock(LockWait))
						{
							return false;
						}
						hasWriteLock = true;
						executeWrite();
					}
					finally
					{
						if (hasWriteLock)
						{
							_slim.ExitWriteLock();
						}
					}
				}
				executeReadAfter?.Invoke(doRunWrite);
				return true;
			}
			finally
			{
				if (hasReadLocked)
				{
					_slim.ExitUpgradeableReadLock();
				}
			}
		}
	}
}
