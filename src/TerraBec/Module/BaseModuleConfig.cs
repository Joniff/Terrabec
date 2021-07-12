using Newtonsoft.Json;

namespace Terrabec.Module
{
	[JsonObject(MemberSerialization.OptIn)]
	public abstract class BaseModuleConfig : IModuleConfig
	{
		[JsonProperty("enable")]
		public bool Enable { get; set; } = true;
	}
}
