using System;
using System.Linq;
using System.Text;
using Terrabec.Connectors;
using Terrabec.Loggers;
using Terrabec.Module;
using Terrabec.Modules.Info;
using Terrabec.Queues.EnumerateEmailsSent;
using Terrabec.Queues.QueueEmail;
using Terrabec.Queues.QueueEmailFrom;
using Terrabec.Queues.QueueEmailFromWithAttachments;
using Terrabec.Queues.QueueEmailWithAttachments;
using Terrabec.Queues.Refresh;
using Terrabec.Queues.Sql.Persistance;

namespace Terrabec.Queues.Sql
{
	public class SqlQueue : BaseQueue<SqlConfig>, 
		IInfoFeature,
		IEnumerateEmailsSentFeature, 
		IQueueEmailFeature,
		IQueueEmailFromFeature,
		IQueueEmailWithAttachmentsFeature,
		IQueueEmailFromWithAttachmentsFeature,
		IRefreshFeature
	{
		public override string Id => "Sql";

		public override string Name => "Sql";

		public override string Description => "Use a database to store emails before they being sent";

		public override string Icon => "icon-server-alt color-green";

		public override string Image => "/App_Plugins/Terrabec.Connectors.MailUp/images/SqlLogo.svg";

		public override string Url => "https://www.microsoft.com/en-gb/sql-server/sql-server-downloads";

		public override IModuleConfig DefaultConfig => new SqlConfig();

		public override bool Init(ILogger logger) => Processor.Instance.Init(ReadConfig() as SqlConfig, logger);

		public IInfoResults ExecuteInfoFeature()
		{
			var results = new InfoResults();

			results.Add(new InfoResult
			{
				Name = "Emails queued",
				Value = Processor.Instance.List(false, null).Count().ToString()
			});

			var lastSentEmail = Processor.Instance.LastSentEmail();
			StringBuilder lastSentEmailValue = new StringBuilder();
			if (lastSentEmail == null)
			{
				lastSentEmailValue.Append("No emails sucessfully sent");
			}
			else
			{
				var timeElapsed = new TimeSpan(DateTime.UtcNow.Ticks - ((DateTime) lastSentEmail).Ticks);
				var days = Convert.ToInt32(Math.Floor(timeElapsed.TotalDays));

				if (days > 0)
				{
					lastSentEmailValue.Append(days);
					lastSentEmailValue.Append(" day");
					if (days != 1)
					{
						lastSentEmailValue.Append('s');
					}
				}
				if (timeElapsed.Hours > 0)
				{
					if (lastSentEmailValue.Length != 0)
					{
						lastSentEmailValue.Append(" and ");
					}
					lastSentEmailValue.Append(timeElapsed.Hours);
					lastSentEmailValue.Append(" hour");
					if (timeElapsed.Hours != 1)
					{
						lastSentEmailValue.Append('s');
					}
				}
				if (timeElapsed.Minutes > 0)
				{
					if (lastSentEmailValue.Length != 0)
					{
						lastSentEmailValue.Append(" and ");
					}
					lastSentEmailValue.Append(timeElapsed.Minutes);
					lastSentEmailValue.Append(" minute");
					if (timeElapsed.Minutes != 1)
					{
						lastSentEmailValue.Append('s');
					}
				}

				if (lastSentEmailValue.Length == 0)
				{
					lastSentEmailValue.Append("Just now");
				}
				else
				{
					lastSentEmailValue.Append(" ago");
				}
				lastSentEmailValue.Append(" (");
				lastSentEmailValue.Append(((DateTime) lastSentEmail).ToString("dd MMM yyyy HH:mm:ss"));
				lastSentEmailValue.Append(" UTC)");
			}
			results.Add(new InfoResult
			{
				Name = "Last successfully sent email",
				Value = lastSentEmailValue.ToString()
			});

			results.Add(new InfoResult
			{
				Name = "Errors",
				Value = Processor.Instance.Errors.ToString()
			});

			results.Add(new InfoResult
			{
				Name = "Emails completed in last 24 hours",
				Value = Processor.Instance.EmailSentBetween(DateTime.UtcNow.AddDays(-1), null).ToString()
			});

			var last = Processor.Instance.LastAttempt();
			if (last.Item1 == null)
			{
				results.Add(new InfoResult
				{
					Name = "No communication yet",
					Value = ""
				});
			}
			else if (last.Item1 == true)
			{
				results.Add(new InfoResult
				{
					Name = "Last communication was successful at",
					Value = last.Item2.Value.ToString()
				});
			}
			else
			{
				results.Add(new InfoResult
				{
					Name = "Last communication was a failure at",
					Value = last.Item2.Value.ToString()
				});
			}
			return results;
		}

		public IEnumerateEmailsSentResults ExecuteEnumerateEmailsSentFeature(bool? sent, TimeSpan? submittedInLast)
		{
			var results = new EnumerateEmailsSentResults();
			foreach (var record in Processor.Instance.List(sent, submittedInLast))
			{
				results.Add(new EnumerateEmailsSentResult
				{
					Submitted = record.Submitted,
					Completed = record.Completed,
					ConnectorId = record.ConnectionId,
					EmailTemplateId = record.EmailTemplateId,
					FromName = record.FromName,
					FromEmail = record.FromEmail,
					ToName = record.ToName,
					ToEmail = record.ToEmail,
				});
			}
			return results;
		}

		public IQueueEmailResult ExecuteQueueEmailFeature(string connectorId, string emailTemplateId, string email, IContactFields fields)
		{
			Processor.Instance.Add(new EmailTracking
			{
				Submitted = DateTime.UtcNow,
				Completed = null,
				ConnectionId = connectorId,
				Feature = nameof(IQueueEmailFeature),
				EmailTemplateId = emailTemplateId,
				FromName = null,
				FromEmail = null,
				ToName = null,
				ToEmail = email,
				Fields = fields,
				Attachments = null,
			});

			return new QueueEmailResult
			{
				Queued = true
			};
		}

		public IQueueEmailFromResult ExecuteQueueEmailFromFeature(string connectorId, string emailTemplateId, string fromName, string fromEmail, string toName, string toEmail, IContactFields fields)
		{
			Processor.Instance.Add(new EmailTracking
			{
				Submitted = DateTime.UtcNow,
				Completed = null,
				ConnectionId = connectorId,
				Feature = nameof(IQueueEmailFromFeature),
				EmailTemplateId = emailTemplateId,
				FromName = fromName,
				FromEmail = fromEmail,
				ToName = toName,
				ToEmail = toEmail,
				Fields = fields,
				Attachments = null,
			});

			return new QueueEmailFromResult
			{
				Queued = true
			};
		}

		public IQueueEmailFromWithAttachmentsResult ExecuteQueueEmailFromWithAttachmentsFeature(string connectorId, string emailTemplateId, string fromName, string fromEmail, string toName, string toEmail, IContactFields fields, IAttachments attachments)
		{
			Processor.Instance.Add(new EmailTracking
			{
				Submitted = DateTime.UtcNow,
				Completed = null,
				ConnectionId = connectorId,
				Feature = nameof(IQueueEmailFromWithAttachmentsFeature),
				EmailTemplateId = emailTemplateId,
				FromName = fromName,
				FromEmail = fromEmail,
				ToName = toName,
				ToEmail = toEmail,
				Fields = fields,
				Attachments = attachments
			});
			return new QueueEmailFromWithAttachmentsResult
			{
				Queued = true
			};
		}

		public IQueueEmailWithAttachmentsResult ExecuteQueueEmailWithAttachmentsFeature(string connectorId, string emailTemplateId, string toEmail, IContactFields fields, IAttachments attachments)
		{
			Processor.Instance.Add(new EmailTracking
			{
				Submitted = DateTime.UtcNow,
				Completed = null,
				ConnectionId = connectorId,
				Feature = nameof(IQueueEmailWithAttachmentsFeature),
				EmailTemplateId = emailTemplateId,
				FromName = null,
				FromEmail = null,
				ToName = null,
				ToEmail = toEmail,
				Fields = fields,
				Attachments = attachments
			});

			return new QueueEmailWithAttachmentsResult
			{
				Queued = true
			};
		}

		public IRefreshResult ExecuteRefreshFeature()
		{
			return Processor.Instance.Refresh();
		}
	}
}
