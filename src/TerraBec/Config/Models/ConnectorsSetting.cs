using System;
using System.Collections.Generic;
using System.Configuration;

namespace Terrabec.Config.Models
{
	[ConfigurationCollection(typeof(ConnectorSetting), AddItemName = ConnectorSetting.Tag, CollectionType = ConfigurationElementCollectionType.BasicMap)]
	public class ConnectorsSetting : ConfigurationElementCollection, IEnumerable<ConnectorSetting>
	{
		public const string Tag = "connectors";

		protected override ConfigurationElement CreateNewElement()
		{
			return new ConnectorSetting();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}
			return ((ConnectorSetting)element).Name;
		}

		public ConnectorSetting this[int index]
		{
			get
			{
				return BaseGet(index) as ConnectorSetting;
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

		new public ConnectorSetting this[string name]
		{
			get
			{
				return BaseGet(name) as ConnectorSetting;
			}
		}

		public new IEnumerator<ConnectorSetting> GetEnumerator()
		{
			int count = this.Count;

			for (int i = 0; i < count; i++)
			{
				yield return this.BaseGet(i) as ConnectorSetting;
			}
		}

		protected override bool OnDeserializeUnrecognizedAttribute(string attribute, string value)
		{
			return base.OnDeserializeUnrecognizedAttribute(attribute, value);
		}
	}
}
