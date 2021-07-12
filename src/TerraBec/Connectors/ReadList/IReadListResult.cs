using Newtonsoft.Json;
using Terrabec.Modules;

namespace Terrabec.Connectors.ReadList
{
	public interface IReadListResult : EnumerateLists.IEnumerateListsResult, IFeatureResult
	{
		[JsonProperty("subscribers")]
		int? Subscribers { get; }

		[JsonProperty("unsubscribers")]
		int? Unsubscribers { get; }
	}
}
