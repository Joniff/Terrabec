using Terrabec.Connectors;
using Terrabec.Modules;

namespace Terrabec.Queues.QueueEmailWithAttachments
{
	public interface IQueueEmailWithAttachmentsFeature : IFeature
	{
		IQueueEmailWithAttachmentsResult ExecuteQueueEmailWithAttachmentsFeature(string connectorId, string emailTemplateId, string email, IContactFields fields, IAttachments attachments);
	}
}
