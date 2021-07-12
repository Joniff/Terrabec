using System.Collections.Generic;
using System.Configuration;

namespace Terrabec.Config.Models
{
	public class Terrabec : ConfigurationSection
	{
		public const string Tag = "terrabec";

		[ConfigurationProperty(ConnectorsSetting.Tag, IsRequired = false)]
		public ConnectorsSetting Connectors => this.Properties.Contains(ConnectorsSetting.Tag) ? this[ConnectorsSetting.Tag] as ConnectorsSetting : null;

		[ConfigurationProperty(SubscriptionsSetting.Tag, IsRequired = false)]
		public SubscriptionsSetting Subscriptions => this.Properties.Contains(SubscriptionsSetting.Tag) ? this[SubscriptionsSetting.Tag] as SubscriptionsSetting : null;

		[ConfigurationProperty(QueuesSetting.Tag, IsRequired = false)]
		public QueuesSetting Queues => this.Properties.Contains(QueuesSetting.Tag) ? this[QueuesSetting.Tag] as QueuesSetting : null;

		protected override bool OnDeserializeUnrecognizedAttribute(string attribute, string value)
		{
			return base.OnDeserializeUnrecognizedAttribute(attribute, value);
		}
	}
}
