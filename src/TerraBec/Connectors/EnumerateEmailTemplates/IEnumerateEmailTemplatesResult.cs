using Newtonsoft.Json;
using Terrabec.Modules;

namespace Terrabec.Connectors.EnumerateEmailTemplates
{
	public interface IEnumerateEmailTemplatesResult : IFeatureResult
	{
		[JsonProperty("id")]
		string Id { get; }

		[JsonProperty("name")]
		string Name { get; }
	}
}
