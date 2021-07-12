using System.Diagnostics;
using Newtonsoft.Json;
using Terrabec.Connectors;
using Terrabec.Module;
using Terrabec.Queues;
using Terrabec.Queues.QueueEmail;
using Terrabec.Queues.QueueEmailFrom;
using Terrabec.Queues.QueueEmailFromWithAttachments;
using Terrabec.Queues.QueueEmailWithAttachments;
using uSync.MemberEdition.Security;

namespace Terrabec.PropertyEditors.EnumerateQueues
{
	[JsonObject(MemberSerialization.OptIn)]
	[JsonConverter(typeof(PropertyModelJsonConvertor))]
	[DebuggerDisplay("{queueId}")]
	public class PropertyModel
	{
		private static Cryptography cryptography = new Cryptography(nameof(EnumerateQueues));

		[JsonProperty(PropertyName = "queue")]
		internal string queueId { get; set; }

		private IModule queue;
		public IModule Queue 
		{ 
			get
			{
				if (queue != null)
				{
					return queue;
				}
				if (string.IsNullOrWhiteSpace(queueId) || !BaseQueue<IModuleConfig>.Register.ContainsKey(queueId))
				{
					return null;
				}
				queue = BaseQueue<IModuleConfig>.Create(queueId);
				queue.Init();
				return queue;
			}
			set
			{
				if (value == null)
				{
					queueId = null;
					queue = null;
				}
				else
				{
					queue = value;
					queueId = queue.Id;
				}
			}
		}

		public PropertyModel()
		{
		}

		public PropertyModel(PropertyModel other)
		{
			Queue = other.Queue;
		}

		public PropertyModel(string id)
		{ 
			queueId = id;
		}

		public bool IsValid() => Queue != null;

		public bool QueueEmail(string connectorId, string emailTemplateId, string toAddress, IContactFields fields) => 
			(IsValid() && (Queue as IQueueEmailFeature) != null) ? (Queue as IQueueEmailFeature).ExecuteQueueEmailFeature(connectorId, emailTemplateId, toAddress, fields).Queued : false;

		public bool QueueEmail(EnumerateEmailTemplates.PropertyModel model, string toAddress, IContactFields fields) => QueueEmail(model.connectorId, model.emailTemplateId, toAddress, fields);

		public bool QueueEmailFrom(string connectorId, string emailTemplateId, string fromName, string fromAddress, string toName, string toAddress, IContactFields fields) => 
			(IsValid() && (Queue as IQueueEmailFromFeature) != null) ? (Queue as IQueueEmailFromFeature).ExecuteQueueEmailFromFeature(connectorId, emailTemplateId, fromName, fromAddress, toName, toAddress, fields).Queued : false;

		public bool QueueEmailFrom(EnumerateEmailTemplates.PropertyModel model, string fromName, string fromAddress, string toName, string toAddress, IContactFields fields) => 
			QueueEmailFrom(model.connectorId, model.emailTemplateId, fromName, fromAddress, toName, toAddress, fields);

		public bool QueueEmailWithAttachment(string connectorId, string emailTemplateId, string toAddress, IContactFields fields, IAttachments attachments) => 
			(IsValid() && (Queue as IQueueEmailWithAttachmentsFeature) != null) ? (Queue as IQueueEmailWithAttachmentsFeature).ExecuteQueueEmailWithAttachmentsFeature(connectorId, emailTemplateId, toAddress, fields, attachments).Queued : false;

		public bool QueueEmailWithAttachment(EnumerateEmailTemplates.PropertyModel model, string toAddress, IContactFields fields, IAttachments attachments) => 
			QueueEmailWithAttachment(model.connectorId, model.emailTemplateId, toAddress, fields, attachments);

		public bool QueueEmailFromWithAttachment(string connectorId, string emailTemplateId, string fromName, string fromAddress, string toName, string toAddress, IContactFields fields, IAttachments attachments) => 
			(IsValid() && (Queue as IQueueEmailFromWithAttachmentsFeature) != null) ? (Queue as IQueueEmailFromWithAttachmentsFeature).ExecuteQueueEmailFromWithAttachmentsFeature(connectorId, emailTemplateId, fromName, fromAddress, toName, toAddress, fields, attachments).Queued : false;

		public bool QueueEmailFromWithAttachment(EnumerateEmailTemplates.PropertyModel model, string fromName, string fromAddress, string toName, string toAddress, IContactFields fields, IAttachments attachments) => 
			QueueEmailFromWithAttachment(model.connectorId, model.emailTemplateId, fromName, fromAddress, toName, toAddress, fields, attachments);

		public override string ToString()                                                                                                                                        
		{
			return cryptography.Encrypt(JsonConvert.SerializeObject(this));
		}

		public static implicit operator string(PropertyModel model)
		{
			return cryptography.Encrypt(JsonConvert.SerializeObject(model));
		}

		public static implicit operator PropertyModel(string text)
		{
			return string.IsNullOrWhiteSpace(text) ? new PropertyModel() : new PropertyModel(text[0] == '{' ? text : cryptography.Decrypt(text));
		}
	}
}
