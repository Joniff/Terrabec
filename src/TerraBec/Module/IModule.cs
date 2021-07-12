using Newtonsoft.Json;

namespace Terrabec.Module
{
	public interface IModule : IFrisk
	{
		[JsonProperty("name")]
		string Name { get; }

		[JsonProperty("description")]
		string Description { get; }

		[JsonProperty("icon")]
		string Icon { get; }

		[JsonProperty("image")]
		string Image { get; }

		[JsonProperty("url")]
		string Url { get; }

		bool Init();
		bool Init(Loggers.ILogger logger);

		IModuleConfig DefaultConfig{ get; }

		IModuleConfig ReadConfig();

		bool WriteConfig(IModuleConfig config);
	}
}
