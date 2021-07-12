using Newtonsoft.Json;
using Terrabec.Modules;

namespace Terrabec.Queues.QueueEmail
{
	public interface IQueueEmailResult : IFeatureResult
	{
		[JsonProperty("queued")]
		bool Queued { get; }
	}
}
