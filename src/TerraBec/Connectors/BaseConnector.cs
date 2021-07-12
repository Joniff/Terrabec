using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Terrabec.Module;

namespace Terrabec.Connectors
{
	[JsonObject(MemberSerialization.OptIn)]
	public abstract class BaseConnector<C> : BaseModule<C> where C : IModuleConfig 
	{
		public new static IDictionary<string, Type> Register => Frisk.Register<BaseConnector<C>>();

		public new static IModule Create(string id)
		{
			if (Register.TryGetValue(id, out var derivedType))
			{
				return Activator.CreateInstance(derivedType) as IModule;
			}
			return null;
		}

		public new IModuleConfig ReadConfig() => ReadConfig((Config.Models.Terrabec setting, string id) => { return setting.Connectors[id]; });

	}
}
