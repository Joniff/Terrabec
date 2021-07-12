using System;
using System.Collections.Generic;
using System.Configuration;

namespace Terrabec.Config.Models
{
	[ConfigurationCollection(typeof(SourceSetting), AddItemName = "add", RemoveItemName = "remove", ClearItemsName = "clear")]
	public class SubscriptionSetting : ConfigurationElementCollection, IEnumerable<SourceSetting>
	{
		public const string Tag = "subscription";

		public const string NameTag = "name";
		public const string EnableTag = "enable";
		public const string ListTag = "list";
		public const string ConnectorTag = "connector";


		[ConfigurationProperty(NameTag, IsRequired = true, IsKey = true)]
		public string Name => base.Properties.Contains(NameTag) ? base[NameTag] as string : null;

		[ConfigurationProperty(EnableTag, DefaultValue = true, IsRequired = false)]
		public bool Enable => base.Properties.Contains(EnableTag) ? (bool) base[EnableTag] : true;

		[ConfigurationProperty(EnableTag, DefaultValue = true, IsRequired = false)]
		public string List => base.Properties.Contains(ListTag) ? base[ListTag] as string : null;

		[ConfigurationProperty(EnableTag, DefaultValue = true, IsRequired = false)]
		public string Connector => base.Properties.Contains(ConnectorTag) ? base[ConnectorTag] as string : null;

		protected override ConfigurationElement CreateNewElement()
		{
			return new SourceSetting();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}
			return ((SourceSetting)element).Key;
		}

		public SourceSetting this[int index]
		{
			get
			{
				return BaseGet(index) as SourceSetting;
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

		new public SourceSetting this[string name]
		{
			get
			{
				return BaseGet(name) as SourceSetting;
			}
		}

		public new IEnumerator<SourceSetting> GetEnumerator()
		{
			int count = this.Count;

			for (int i = 0; i < count; i++)
			{
				yield return this.BaseGet(i) as SourceSetting;
			}
		}

		protected override bool OnDeserializeUnrecognizedAttribute(string attribute, string value)
		{
			return base.OnDeserializeUnrecognizedAttribute(attribute, value);
		}

	}
}
