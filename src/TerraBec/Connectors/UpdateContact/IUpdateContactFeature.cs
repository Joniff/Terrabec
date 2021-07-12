using Terrabec.Connectors.ReadContact;
using Terrabec.Modules;

namespace Terrabec.Connectors.UpdateContact
{
	public interface IUpdateContactFeature : IFeature
	{
		IUpdateContactResult ExecuteUpdateContactFeature(string listId, string contactId, bool enabled, string email, IContactFields fields);
		IUpdateContactResult ExecuteUpdateContactFeature(string listId, IReadContactResult contact);
	}
}
