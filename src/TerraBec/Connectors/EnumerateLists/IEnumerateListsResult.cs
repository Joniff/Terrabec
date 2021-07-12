using Newtonsoft.Json;
using Terrabec.Modules;

namespace Terrabec.Connectors.EnumerateLists
{
	public interface IEnumerateListsResult : IFeatureResult
	{
		[JsonProperty("id")]
		string Id { get; }

		[JsonProperty("name")]
		string Name { get; }
	}
}
