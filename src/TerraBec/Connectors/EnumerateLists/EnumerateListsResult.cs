using Newtonsoft.Json;

namespace Terrabec.Connectors.EnumerateLists
{
	public class EnumerateListsResult : IEnumerateListsResult
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }
	}
}
