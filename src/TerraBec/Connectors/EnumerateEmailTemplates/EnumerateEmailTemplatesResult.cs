using Newtonsoft.Json;

namespace Terrabec.Connectors.EnumerateEmailTemplates
{
	public class EnumerateEmailTemplatesResult : IEnumerateEmailTemplatesResult
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }
	}
}
