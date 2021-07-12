using Terrabec.Modules;

namespace Terrabec.Connectors.CreateContact
{
	public interface ICreateContactFeature : IFeature
	{
		ICreateContactResult ExecuteCreateContactFeature(string listId, bool enabled, string email, IContactFields fields);
	}
}
