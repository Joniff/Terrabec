using Newtonsoft.Json;
using Terrabec.Modules;

namespace Terrabec.Modules.Info
{
	public class InfoResult : IFeatureResult
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("value")]
		public string Value { get; set; }
	}
}
