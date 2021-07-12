using Terrabec.Modules;

namespace Terrabec.Connectors.EnumerateContactFields
{
	public interface IEnumerateContactFieldsFeature : IFeature
	{
		IEnumerateContactFieldsResults ExecuteEnumerateContactFieldsFeature(string listId);
	}
}
