using Newtonsoft.Json;

namespace Terrabec.Module
{
	public interface IFrisk
	{
		[JsonProperty("id")]
		string Id { get; }
	}
}
