using Newtonsoft.Json;

namespace Terrabec.Connectors
{
	public class ContactField : IContactField
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("value")]
		public string Value { get; set; }
	}
}
