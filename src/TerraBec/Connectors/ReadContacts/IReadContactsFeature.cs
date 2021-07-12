using Terrabec.Modules;

namespace Terrabec.Connectors.ReadContacts
{
	public interface IReadContactsFeature : IFeature
	{
		IReadContactsPageResult ExecuteReadContactsFeature(string listId, int page, int pageSize = 100);
	}
}
