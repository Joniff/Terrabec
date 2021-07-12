using Terrabec.Connectors;
using Terrabec.Modules;

namespace Terrabec.Queues.QueueEmailFrom
{
	public interface IQueueEmailFromFeature : IFeature
	{
		IQueueEmailFromResult ExecuteQueueEmailFromFeature(string connectorId, string emailTemplateId, string fromName, string fromEmail, string toName, string toEmail, IContactFields fields);
	}
}
