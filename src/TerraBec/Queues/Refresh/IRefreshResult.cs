using Newtonsoft.Json;
using Terrabec.Modules;

namespace Terrabec.Queues.Refresh
{
	public interface IRefreshResult : IFeatureResult
	{
		[JsonProperty("refreshed")]
		bool Refreshed { get; }
	}
}
