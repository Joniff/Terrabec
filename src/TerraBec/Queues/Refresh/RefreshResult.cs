using Newtonsoft.Json;

namespace Terrabec.Queues.Refresh
{
	public class RefreshResult : IRefreshResult
	{
		[JsonProperty("refreshed")]
		public bool Refreshed { get; set; }
	}
}
