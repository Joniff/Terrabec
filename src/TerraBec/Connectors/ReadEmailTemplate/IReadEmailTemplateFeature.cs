using Terrabec.Modules;

namespace Terrabec.Connectors.ReadEmailTemplate
{
	public interface IReadEmailTemplateFeature : IFeature
	{
		IReadEmailTemplateResult ExecuteReadEmailTemplateFeature(string emailTemplateId);
	}
}
