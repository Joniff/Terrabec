using System;
using System.Configuration;

namespace Terrabec.Config.Models
{
	public class PropertySetting : ConfigurationElement
	{
		public const string KeyTag = "key";
		public const string ValueTag = "value";

		[ConfigurationProperty(KeyTag, IsRequired = true, IsKey = true)]
		public string Key => base.Properties.Contains(KeyTag) ? base[KeyTag] as string : null;

		[ConfigurationProperty(ValueTag)]
		public string Value => base.Properties.Contains(ValueTag) ? base[ValueTag] as string : null;

		public T ValueType<T>() => (Value != null) ? (T)Convert.ChangeType(Value, typeof(T)) : default(T);

		public object ValueType(Type type) => (Value != null) ? Convert.ChangeType(Value, type) : ((type.IsValueType) ? Activator.CreateInstance(type) : null);

		protected override bool OnDeserializeUnrecognizedAttribute(string attribute, string value)
		{
			return base.OnDeserializeUnrecognizedAttribute(attribute, value);
		}
	}
}
