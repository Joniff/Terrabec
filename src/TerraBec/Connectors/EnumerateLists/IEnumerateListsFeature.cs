using Terrabec.Modules;

namespace Terrabec.Connectors.EnumerateLists
{
	public interface IEnumerateListsFeature : IFeature
	{
		IEnumerateListsResults ExecuteEnumerateListsFeature();
	}
}
