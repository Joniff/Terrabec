using System;
using System.Collections.Generic;
using System.Configuration;

namespace Terrabec.Config.Models
{
	[ConfigurationCollection(typeof(PropertySetting), AddItemName = "add", RemoveItemName = "remove", ClearItemsName = "clear")]
	public class ConnectorSetting : ConfigurationElementCollection, IEnumerable<PropertySetting>
	{
		public const string Tag = "connector";

		public const string NameTag = "name";
		public const string EnableTag = "enable";

		[ConfigurationProperty(NameTag, IsRequired = true, IsKey = true)]
		public string Name => base.Properties.Contains(NameTag) ? base[NameTag] as string : null;

		[ConfigurationProperty(EnableTag, DefaultValue = true, IsRequired = false)]
		public bool Enable => base.Properties.Contains(EnableTag) ? (bool) base[EnableTag] : true;

		protected override ConfigurationElement CreateNewElement()
		{
			return new PropertySetting();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}
			return ((PropertySetting)element).Key;
		}

		public PropertySetting this[int index]
		{
			get
			{
				return BaseGet(index) as PropertySetting;
			}
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}
				BaseAdd(index, value);
			}
		}

		new public PropertySetting this[string name]
		{
			get
			{
				return BaseGet(name) as PropertySetting;
			}
		}

		public new IEnumerator<PropertySetting> GetEnumerator()
		{
			int count = this.Count;

			for (int i = 0; i < count; i++)
			{
				yield return this.BaseGet(i) as PropertySetting;
			}
		}

		protected override bool OnDeserializeUnrecognizedAttribute(string attribute, string value)
		{
			return base.OnDeserializeUnrecognizedAttribute(attribute, value);
		}

	}
}
