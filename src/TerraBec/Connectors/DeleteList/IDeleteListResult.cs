using Newtonsoft.Json;
using Terrabec.Modules;

namespace Terrabec.Connectors.DeleteList
{
	public interface IDeleteListResult : IFeatureResult
	{
		[JsonProperty("success")]
		bool Success { get; }
	}
}
