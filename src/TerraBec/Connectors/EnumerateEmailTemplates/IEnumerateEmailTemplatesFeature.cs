using Terrabec.Modules;

namespace Terrabec.Connectors.EnumerateEmailTemplates
{
	public interface IEnumerateEmailTemplatesFeature : IFeature
	{
		IEnumerateEmailTemplatesResults ExecuteEnumerateEmailTemplatesFeature();
	}

}
