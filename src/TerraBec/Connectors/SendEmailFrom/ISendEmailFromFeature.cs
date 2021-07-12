using Terrabec.Modules;

namespace Terrabec.Connectors.SendEmailFrom
{
	public interface ISendEmailFromFeature : IFeature
	{
		ISendEmailFromResult ExecuteSendEmailFromFeature(string emailTemplateId, string fromName, string fromEmail, string toName, string toEmail, IContactFields fields);
	}
}
