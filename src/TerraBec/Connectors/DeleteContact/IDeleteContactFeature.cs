using Terrabec.Modules;

namespace Terrabec.Connectors.DeleteContact
{
	public interface IDeleteContactFeature : IFeature
	{
		IDeleteContactResult ExecuteDeleteContactFeature(string listId, string contactId);
	}
}
