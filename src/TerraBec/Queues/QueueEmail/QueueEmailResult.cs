using Newtonsoft.Json;

namespace Terrabec.Queues.QueueEmail
{
	public class QueueEmailResult : IQueueEmailResult
	{
		[JsonProperty("queued")]
		public bool Queued { get; set; }
	}
}
