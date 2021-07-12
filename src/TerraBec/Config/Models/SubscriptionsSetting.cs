using System;
using System.Collections.Generic;
using System.Configuration;

namespace Terrabec.Config.Models
{
	[ConfigurationCollection(typeof(SubscriptionSetting), AddItemName = SubscriptionSetting.Tag, CollectionType = ConfigurationElementCollectionType.BasicMap)]
	public class SubscriptionsSetting : ConfigurationElementCollection, IEnumerable<SubscriptionSetting>
	{
		public const string Tag = "subscriptions";

		protected override ConfigurationElement CreateNewElement()
		{
			return new SubscriptionSetting();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}
			return ((SubscriptionSetting)element).Name;
		}

		public SubscriptionSetting this[int index]
		{
			get
			{
				return BaseGet(index) as SubscriptionSetting;
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

		new public SubscriptionSetting this[string name]
		{
			get
			{
				return BaseGet(name) as SubscriptionSetting;
			}
		}

		public new IEnumerator<SubscriptionSetting> GetEnumerator()
		{
			int count = this.Count;

			for (int i = 0; i < count; i++)
			{
				yield return this.BaseGet(i) as SubscriptionSetting;
			}
		}

		protected override bool OnDeserializeUnrecognizedAttribute(string attribute, string value)
		{
			return base.OnDeserializeUnrecognizedAttribute(attribute, value);
		}
	}
}
