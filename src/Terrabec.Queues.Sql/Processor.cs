using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using Terrabec.Connectors;
using Terrabec.Connectors.SendEmail;
using Terrabec.Connectors.SendEmailFrom;
using Terrabec.Connectors.SendEmailFromWithAttachments;
using Terrabec.Connectors.SendEmailWithAttachments;
using Terrabec.Loggers;
using Terrabec.Module;
using Terrabec.Queues.QueueEmail;
using Terrabec.Queues.QueueEmailFrom;
using Terrabec.Queues.QueueEmailFromWithAttachments;
using Terrabec.Queues.QueueEmailWithAttachments;
using Terrabec.Queues.Refresh;
using Terrabec.Queues.Sql.Persistance;

namespace Terrabec.Queues.Sql
{
	internal class Processor
	{
		private static Lazy<Processor> instance = new Lazy<Processor>(() => new Processor
		{
			Worker = new Thread(Work),
			Errors = 0,
			Locker = new ReaderWriterLockSlim()
		});

		public static Processor Instance => instance.Value;
			
		public SqlConfig Config;
		public ILogger Logger;

		private bool DoWork;
		private bool RereadEmailTracking;
		private bool Sleep;
		private Dictionary<int, EmailTracking> Process;
		private Thread Worker;
		private ReaderWriterLockSlim Locker;
		private const int LockWait = 5000;						//	5 second
		private DateTime? LastTimeTriedToConnect = null;		//	This connection may have been a success or failure
		private bool? LastTimeSuccess = null;					//	Was last connection a success or failure

		public int Errors { get; private set; }

		public bool Init(SqlConfig config, ILogger logger)
		{
			Config = config;
			Logger = logger;

			try
			{
				Start();
				return true;
			}
			catch(Exception ex)
			{
				Logger.Error(ex.Message, ex);
				Errors++;
				return false;
			}
		}

		public void Start()
		{
			DoWork = true;
			Sleep = false;
			if (!Worker.IsAlive)
			{
				RereadEmailTracking = true;
				Worker.Start(this);
			}
		}

		public void Add(EmailTracking emailTrack)
		{
			Task.Run(() => {
				try
				{
					try
					{
						using (var db = new Storage(Config))
						{
							db.EmailTracking.Insert(emailTrack);
							db.Close();
						}
					}
					catch (Exception ex)
					{
						Logger.Error($"Can\'t save new emails to {Config.DatabaseTable}", ex);
						Errors++;
						return;
					}

					if (!Locker.TryEnterWriteLock(LockWait))
					{
						RereadEmailTracking = true;
						Logger.Info($"Can\'t lock EmailTracking");
						Errors++;
						return;
					}

					try
					{
						Process.Add(emailTrack.Id, emailTrack);
						Start();
					}
					finally
					{
						Locker.ExitWriteLock();
					}
				}
				catch (Exception ex)
				{
					Logger.Error($"Can\'t save new emails", ex);
				}
			});
		}

		public IEnumerable<EmailTracking> List(bool? sent, TimeSpan? submittedInLast)
		{
			using (var db = new Storage(Config))
			{
				var query = db.EmailTracking.Query();
				if (submittedInLast != null)
				{
					DateTime from = DateTime.UtcNow.Subtract((TimeSpan) submittedInLast);
					query = query.Where(r => r.Submitted >= from);
				}
				if (sent == true)
				{
					query = query.Where(r => r.Completed != null);
				}
				else if (sent == false)
				{
					query = query.Where(r => r.Completed == null);
				}

				var results = query.ToList();
				db.Close();
				return results;
			}
		}

		public DateTime? LastSentEmail()
		{
			using (var db = new Storage(Config))
			{
				var query = db.EmailTracking.Query().Where(r => r.Completed != null).OrderByDescending(r => r.Completed).FirstOrDefault();
				if (query == null)
				{
					return null;
				}
				var results = query.Completed;
				db.Close();
				return results;
			}
		}

		public (bool?, DateTime?) LastAttempt() => (LastTimeSuccess, LastTimeTriedToConnect);

		public int EmailSentBetween(DateTime? startDate, DateTime? endDate)
		{
			using (var db = new Storage(Config))
			{
				var query = db.EmailTracking.Query();
				if (startDate != null)
				{
					query = query.Where(r => r.Completed >= startDate);
				}
				if (endDate != null)
				{
					query = query.Where(r => r.Completed <= endDate);
				}
				var results = query.Count();
				db.Close();
				return results;
			}
		}

		private IList<EmailTracking>  ReadEmailTracking()
		{
			using (var db = new Storage(Config))
			{
				var results = db.EmailTracking.Query().Where(r => r.Completed == null).ToList();
				db.Close();
				return results;
			}
		}

		private bool ProcessEmail(EmailTracking email)
		{
			if (string.IsNullOrEmpty(email.ConnectionId) || !BaseConnector<IModuleConfig>.Register.ContainsKey(email.ConnectionId))
			{
				Logger.Warn($"No connector called '{email.ConnectionId}'");
				return true;
			}

			var connector = BaseConnector<IModuleConfig>.Create(email.ConnectionId);
			connector.Init();

			try
			{
				switch (email.Feature)
				{
					case nameof(IQueueEmailFeature):
						if (!(connector is ISendEmailFeature))
						{
							Logger.Warn($"Connector '{email.ConnectionId}' has no SendEmailFeature");
							return true;
						}
				
						return (connector as ISendEmailFeature).ExecuteSendEmailFeature(email.EmailTemplateId, email.ToEmail, email.Fields).Success;

					case nameof(IQueueEmailFromFeature):
						if (!(connector is ISendEmailFromFeature))
						{
							Logger.Warn($"Connector '{email.ConnectionId}' has no SendEmailFromFeature");
							return true;
						}
				
						return (connector as ISendEmailFromFeature).ExecuteSendEmailFromFeature(email.EmailTemplateId, email.FromName, email.FromEmail, email.ToName, email.ToEmail, email.Fields).Success;

					case nameof(IQueueEmailWithAttachmentsFeature):

						if (!(connector is ISendEmailWithAttachmentsFeature))
						{
							Logger.Warn($"Connector '{email.ConnectionId}' has no SendEmailWithAttachmentsFeature");
							return true;
						}
				
						return (connector as ISendEmailWithAttachmentsFeature).ExecuteSendEmailWithAttachmentsFeature(email.EmailTemplateId, email.ToEmail, email.Fields, email.Attachments).Success;

					case nameof(IQueueEmailFromWithAttachmentsFeature):

						if (!(connector is ISendEmailFromWithAttachmentsFeature))
						{
							Logger.Warn($"Connector '{email.ConnectionId}' has no SendEmailFromWithAttachmentsFeature");
							return true;
						}
				
						return (connector as ISendEmailFromWithAttachmentsFeature).ExecuteSendEmailFromWithAttachmentsFeature(email.EmailTemplateId, email.FromName, email.FromEmail, email.ToName, email.ToEmail, email.Fields, email.Attachments).Success;
				}
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		public IRefreshResult Refresh()
		{
			RereadEmailTracking = true;
			return new RefreshResult
			{
				Refreshed = true
			};
		}

		private Stack<int> SendMail()
		{
			DateTime end = DateTime.UtcNow.AddMilliseconds(LockWait);

			var toDelete = new Stack<int>();
			foreach (var email in Process.Where(a => a.Value.Completed == null).OrderBy(b => Guid.NewGuid()))			//	We want them in random order, so we don't get stuck on one entry
			{
				if (ProcessEmail(email.Value))
				{
					email.Value.Completed = DateTime.UtcNow;
					toDelete.Push(email.Key);
					LastTimeSuccess = true;
				}
				else
				{
					LastTimeSuccess = false;
				}
				LastTimeTriedToConnect = DateTime.UtcNow;
				//	Only run for maximum of 5 seconds
				if (DateTime.UtcNow > end)
				{
					break;
				}
				Thread.Sleep(1);
			}
			return toDelete;
		}

		private void CompleteMail(Stack<int> toDelete)
		{
			if (toDelete.Any())
			{
				var now = DateTime.UtcNow;
				using (var db = new Storage(Config))
				{
					while (toDelete.Any())
					{
						int id = toDelete.Pop();
						try
						{
							db.EmailTracking.Update(Process[id], nameof(EmailTracking.Completed));
						}
						catch (Exception ex)
						{
							Logger.Error($"Unable to set completed value for {id}", ex);
							Errors++;
						}
						Process.Remove(id);
					}
					db.Close();
				}
			}
		}

		private static void Work(object thisObject)
		{
			var processor = thisObject as Processor;

			while (true)
			{
				if ((!processor.DoWork && !processor.RereadEmailTracking) || processor.Sleep)
				{
					Thread.Sleep(processor.Config.Retry);
					processor.Sleep = false;
					continue;
				}
					
				if (!processor.Locker.TryEnterUpgradeableReadLock(LockWait))
				{
					Thread.Sleep(processor.Config.Retry);
					processor.RereadEmailTracking = true;
					processor.Errors++;
					continue;
				}
				try
				{
					if (processor.RereadEmailTracking)
					{
						try
						{
							processor.Process = processor.ReadEmailTracking().ToDictionary(k => k.Id, v => v);
							processor.RereadEmailTracking = false;
						}
						catch (Exception)
						{
							processor.RereadEmailTracking = true;
							processor.Sleep = true;
							continue;
						}
					}

					if (!processor.Process.Any())
					{
						processor.DoWork = false;
						continue;
					}

					if (!processor.Locker.TryEnterWriteLock(LockWait))
					{
						
						processor.Sleep = true;
						processor.RereadEmailTracking = true;
						continue;
					}

					try
					{
						try
						{
							var toDelete = processor.SendMail();
							processor.CompleteMail(toDelete);
						}
						catch (Exception ex)
						{
							processor.Logger.Error($"Unable to process mail", ex);
							processor.Errors++;
						}

						if (processor.Process.Any())
						{
							processor.Sleep = true;
						}
						else
						{
							processor.DoWork = false;
						}
					}
					finally
					{
						processor.Locker.ExitWriteLock();
					}
				}
				finally
				{
					processor.Locker.ExitUpgradeableReadLock();
				}
			}
		}
	}
}

