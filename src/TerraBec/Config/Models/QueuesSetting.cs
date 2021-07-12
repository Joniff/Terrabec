using System;
using System.Collections.Generic;
using System.Configuration;

namespace Terrabec.Config.Models
{
	[ConfigurationCollection(typeof(QueueSetting), AddItemName = QueueSetting.Tag, CollectionType = ConfigurationElementCollectionType.BasicMap)]
	public class QueuesSetting : ConfigurationElementCollection, IEnumerable<QueueSetting>
	{
		public const string Tag = "queues";

		protected override ConfigurationElement CreateNewElement()
		{
			return new QueueSetting();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}
			return ((QueueSetting)element).Name;
		}

		public QueueSetting this[int index]
		{
			get
			{
				return BaseGet(index) as QueueSetting;
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

		new public QueueSetting this[string name]
		{
			get
			{
				return BaseGet(name) as QueueSetting;
			}
		}

		public new IEnumerator<QueueSetting> GetEnumerator()
		{
			int count = this.Count;

			for (int i = 0; i < count; i++)
			{
				yield return this.BaseGet(i) as QueueSetting;
			}
		}

		protected override bool OnDeserializeUnrecognizedAttribute(string attribute, string value)
		{
			return base.OnDeserializeUnrecognizedAttribute(attribute, value);
		}
	}
}
