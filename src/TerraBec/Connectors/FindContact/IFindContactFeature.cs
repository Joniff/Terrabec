using Terrabec.Modules;

namespace Terrabec.Connectors.FindContact
{
	public interface IFindContactFeature : IFeature
	{
		IFindContactResult ExecuteFindContactFeature(string listId, string email);
	}
}
