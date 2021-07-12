using Terrabec.Connectors;
using Terrabec.Modules;

namespace Terrabec.Queues.QueueEmail
{
	public interface IQueueEmailFeature : IFeature
	{
		IQueueEmailResult ExecuteQueueEmailFeature(string connectorId, string emailTemplateId, string email, IContactFields fields);
	}
}
