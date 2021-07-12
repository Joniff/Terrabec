using Terrabec.Modules;

namespace Terrabec.Connectors.ReadContact
{
	public interface IReadContactFeature : IFeature
	{
		IReadContactResult ExecuteReadContactFeature(string listId, string contactId);
	}
}
