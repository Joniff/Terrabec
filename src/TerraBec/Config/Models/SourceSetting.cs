using System;
using System.Configuration;

namespace Terrabec.Config.Models
{
	public class SourceSetting : ConfigurationElement
	{
		public const string KeyTag = "key";
		public const string SourceTypeTag = "type";
		public const string SourceTag = "source";

		[ConfigurationProperty(KeyTag, IsRequired = true, IsKey = true)]
		public string Key => base.Properties.Contains(KeyTag) ? base[KeyTag] as string : null;

		public enum SourceType
		{
			Member
		}

		[ConfigurationProperty(SourceTypeTag, IsRequired = true)]
		public SourceType Type
		{
			get
			{
				if (base.Properties.Contains(SourceTypeTag))
				{
					string value = (string) base[SourceTypeTag];
					return (SourceType) Enum.Parse(typeof(SourceType), (string) base[SourceTypeTag]);
				}
				throw new ArgumentOutOfRangeException(SourceTag + " is missing from config");
			}
			set
			{
				base[SourceTypeTag] = value.ToString();
			}
		}

		[ConfigurationProperty(SourceTag, IsRequired = false)]
		public string Source => base.Properties.Contains(SourceTag) ? base[SourceTag] as string : null;

		protected override bool OnDeserializeUnrecognizedAttribute(string attribute, string value)
		{
			return base.OnDeserializeUnrecognizedAttribute(attribute, value);
		}
	}
}
