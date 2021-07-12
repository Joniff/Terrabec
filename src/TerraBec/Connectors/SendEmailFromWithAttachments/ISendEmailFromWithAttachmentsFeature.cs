using Terrabec.Modules;

namespace Terrabec.Connectors.SendEmailFromWithAttachments
{
	public interface ISendEmailFromWithAttachmentsFeature : IFeature
	{
		ISendEmailFromWithAttachmentsResult ExecuteSendEmailFromWithAttachmentsFeature(string emailTemplateId, string fromName, string fromEmail, string toName, string toEmail, IContactFields fields, IAttachments attachments);
	}
}
