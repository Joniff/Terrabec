using Terrabec.Modules;

namespace Terrabec.Connectors.ReadList
{
	public interface IReadListFeature : IFeature
	{
		IReadListResult ExecuteReadListFeature(string listId);
	}
}
