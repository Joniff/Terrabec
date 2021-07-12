using Terrabec.Modules;

namespace Terrabec.Connectors.DeleteList
{
	public interface IDeleteListFeature : IFeature
	{
		IDeleteListResult ExecuteDeleteListFeature(string listId);
	}
}
