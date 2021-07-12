using Terrabec.Modules;

namespace Terrabec.Connectors.SendEmail
{
	public interface ISendEmailFeature : IFeature
	{
		ISendEmailResult ExecuteSendEmailFeature(string emailTemplateId, string email, IContactFields fields);
	}
}
