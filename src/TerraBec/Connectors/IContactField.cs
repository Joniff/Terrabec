using Newtonsoft.Json;

namespace Terrabec.Connectors
{
	public interface IContactField
	{
		[JsonProperty("id")]
		string Id { get; }

		[JsonProperty("name")]
		string Name { get; }

		[JsonProperty("value")]
		string Value { get; }
	}
}
