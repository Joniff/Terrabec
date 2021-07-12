using Terrabec.Modules;

namespace Terrabec.Connectors.CreateList
{
	public interface ICreateListFeature : IFeature
	{
		ICreateListResult ExecuteCreateListFeature(string name);
	}
}
