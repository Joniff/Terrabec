using Terrabec.Modules;

namespace Terrabec.Connectors.SendEmailWithAttachments
{
	public interface ISendEmailWithAttachmentsFeature : IFeature
	{
		ISendEmailWithAttachmentsResult ExecuteSendEmailWithAttachmentsFeature(string emailTemplateId, string email, IContactFields fields, IAttachments attachments);
	}
}
