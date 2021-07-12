using System;
using System.Collections.Generic;
using System.Threading;

namespace Terrabec.Connectors.MailUp.Client
{
	internal class CacheService
	{
		private IDictionary<string, KeyValuePair<DateTime, object>> Store = null;
		private ReaderWriterLockSlim Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		private Dictionary<string, ReaderWriterLockSlim> StoreLocker = new Dictionary<string, ReaderWriterLockSlim>();
		private DateTime Purge = DateTime.MinValue;
		private int CacheLengthInSeconds = 60;
		private int LockerTimeout = 10000;	//	1 second

		public CacheService()
		{
			ClearCache();
		}

		public CacheService(int cacheLength)
		{
			ClearCache();
			CacheLengthInSeconds = cacheLength;
		}

		public T GetCache<T>(string key, Func<T> action) where T : new()
		{
			ReaderWriterLockSlim storeLocker = null;

			if (Locker.TryEnterUpgradeableReadLock(LockerTimeout))
			{
				try
				{
					if (!StoreLocker.TryGetValue(key, out storeLocker))
					{
						storeLocker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
						Locker.EnterWriteLock();
						try
						{
							StoreLocker.Add(key, storeLocker);
						}
						finally
						{
							Locker.ExitWriteLock();
						}
					}
				}
				finally
				{
					Locker.ExitUpgradeableReadLock();
				}
			}

			if (storeLocker == null)
			{
				throw new ApplicationException();
			}

			if (storeLocker.TryEnterUpgradeableReadLock(LockerTimeout))
			{
				try
				{
					KeyValuePair<DateTime, object> found;
					var now = DateTime.UtcNow;

					if (Store.TryGetValue(key, out found))
					{
						if (found.Key < now)
						{
							return (T)found.Value;
						}
						Store.Remove(key);
					}

					storeLocker.EnterWriteLock();
					try
					{
						var results = action();
						Store.Add(key, new KeyValuePair<DateTime, object>(now.AddSeconds(CacheLengthInSeconds), results));
						return results;
					}
					finally
					{
						storeLocker.ExitWriteLock();
					}
				}
				finally
				{
					storeLocker.ExitUpgradeableReadLock();
				}
			}

			throw new ApplicationException();
		}

		public void ClearCache()
		{
			if (Locker.TryEnterWriteLock(LockerTimeout))
			{
				try
				{
					Store = new Dictionary<string, KeyValuePair<DateTime, object>>();
					StoreLocker = new Dictionary<string, ReaderWriterLockSlim>();
				}
				finally
				{
					Locker.ExitWriteLock();
				}
			}
		}
	}
}
