using Newtonsoft.Json;

namespace Terrabec.Connectors.ReadList
{
	public class ReadListResult : EnumerateLists.EnumerateListsResult, IReadListResult
	{
		[JsonProperty("subscribers")]
		public int? Subscribers { get; set; }

		[JsonProperty("unsubscribers")]
		public int? Unsubscribers { get; set; }
	}
}
