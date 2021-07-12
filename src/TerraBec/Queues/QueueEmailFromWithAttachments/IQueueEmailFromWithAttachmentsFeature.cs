using Terrabec.Connectors;
using Terrabec.Modules;

namespace Terrabec.Queues.QueueEmailFromWithAttachments
{
	public interface IQueueEmailFromWithAttachmentsFeature : IFeature
	{
		IQueueEmailFromWithAttachmentsResult ExecuteQueueEmailFromWithAttachmentsFeature(string connectorId, string emailTemplateId, string fromName, string fromEmail, string toName, string toEmail, IContactFields fields, IAttachments attachments);
	}
}
