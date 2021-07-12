using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Configuration;
using Newtonsoft.Json;
using Terrabec.Config.Models;
using Terrabec.Loggers;

namespace Terrabec.Module
{
	[JsonObject(MemberSerialization.OptIn)]
	public abstract class BaseModule<C> : IModule where C : IModuleConfig 
	{
		[JsonProperty("id")]
		public abstract string Id { get; }

		[JsonProperty("name")]
		public abstract string Name { get; }

		[JsonProperty("description")]
		public abstract string Description { get; }

		[JsonProperty("icon")]
		public abstract string Icon { get; }

		[JsonProperty("image")]
		public abstract string Image { get; }

		[JsonProperty("url")]
		public abstract string Url { get; }

		public virtual bool Init() => Init(new UmbracoLogger());
		public virtual bool Init(Loggers.ILogger logger) => true;

		public virtual IModuleConfig DefaultConfig => default(BaseModuleConfig);

		private IModuleConfig currentConfig = null;

		protected IModuleConfig ReadConfig(Func<Config.Models.Terrabec, string, IEnumerable<PropertySetting>> mapConfig)
		{
			if (currentConfig != null)
			{
				return currentConfig;
			}

			currentConfig = DefaultConfig;

			var file = WebConfigurationManager.OpenWebConfiguration("~");
			var section = file.GetSection(Config.Models.Terrabec.Tag) as Config.Models.Terrabec;
			if (section == null)
			{
				return currentConfig;
			}

			var config = mapConfig(section, Id);
			if (config == null)
			{
				return currentConfig;
			}

			foreach (var prop in currentConfig.GetType().GetProperties())
			{
				var element = config.FirstOrDefault(x => string.Compare(x.Key, prop.Name, true) == 0);
				if (element != null)
				{
					prop.SetValue(currentConfig, element.ValueType(prop.PropertyType));
				}
			}

			return currentConfig;
		}

		public virtual bool WriteConfig(IModuleConfig config)
		{
			currentConfig = config;
			return false;
		}

		public static IDictionary<string, Type> Register => Frisk.Register<BaseModule<C>>();

		public static IModule Create(string id)
		{
			if (Register.TryGetValue(id, out var derivedType))
			{
				return Activator.CreateInstance(derivedType) as IModule;
			}
			return null;
		}

		public virtual IModuleConfig ReadConfig()
		{
			return currentConfig;
		}
	}
}
