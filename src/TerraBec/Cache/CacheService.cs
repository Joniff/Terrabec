using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using Terrabec.Config.Models;
using Terrabec.Connectors;
using Terrabec.Connectors.CreateContact;
using Terrabec.Connectors.DeleteContact;
using Terrabec.Connectors.EnumerateEmailTemplates;
using Terrabec.Connectors.EnumerateLists;
using Terrabec.Connectors.FindContact;
using Terrabec.Connectors.ReadContact;
using Terrabec.Connectors.ReadEmailTemplate;
using Terrabec.Connectors.ReadList;
using Terrabec.Connectors.UpdateContact;
using Terrabec.Module;
using Terrabec.Modules.Info;
using Terrabec.Queues;
using Terrabec.Queues.Refresh;
using Umbraco.Core;
using static Terrabec.Config.Models.SourceSetting;

namespace Terrabec.Cache
{
	internal class CacheService
	{
		private static Lazy<CacheService> internalInstance = new Lazy<CacheService>(() => new CacheService());		
		public static CacheService Instance => internalInstance.Value;

		private static int connectorCounter = Constants.System.Root + 1;

		public static void RefreshConnector(int nodeId = -1)
		{
			internalInstance = new Lazy<CacheService>(() => new CacheService());
			connectorCounter = Constants.System.Root + 1;
		}

		public class ConnectorInfo
		{
			public int Index;
			public bool Enable;
			public IModule Connector;
			public IInfoResults Info;

			public int? ListIndex;
			public Dictionary<int, IEnumerateListsResult> Lists;
			public Dictionary<int, IReadListResult> ListItems;
		
			public int? EmailTemplateIndex;
			public Dictionary<int, IEnumerateEmailTemplatesResult> EmailTemplates;
			public Dictionary<int, IReadEmailTemplateResult> EmailTemplateItems;
		}

		public readonly IDictionary<int, ConnectorInfo> Connectors = null;
		public readonly IDictionary<int, int> NodeIdMap = null;

		public class QueueInfo
		{
			public int Index;
			public bool Enable;
			public IModule Queue;
			public IInfoResults Info;
		}

		public readonly IDictionary<int, QueueInfo> Queues = null;

		public class Subscription
		{
			public bool Enable;
			public ConnectorInfo Connector;
			public string ListId;
			public SourceType Type;
			public string Source;
		}

		public readonly IList<Subscription> Subscriptions;

		public CacheService()
		{
			Connectors = new Dictionary<int, ConnectorInfo>();
			NodeIdMap = new Dictionary<int, int> ();
			foreach (var connector in Frisk.Register<BaseConnector<IModuleConfig>>())
			{
				var instance = BaseConnector<IModuleConfig>.Create(connector.Key);
				if (instance != null && instance.Init())
				{
					var config = instance.ReadConfig();

					var connectorInfo = new ConnectorInfo
					{
						Index = connectorCounter,
						Enable = config.Enable,
						Connector = instance,
						Info = null,
						ListIndex = (instance is IEnumerateListsFeature) ? connectorCounter + 1 : (int ?) null,
						Lists = null,
						ListItems = null,
						EmailTemplateIndex = (instance is IEnumerateEmailTemplatesFeature) ? connectorCounter + 2 : (int ?) null,
						EmailTemplates = null,
						EmailTemplateItems = null
					};

					Connectors.Add(connectorCounter, connectorInfo);
					NodeIdMap.Add(connectorCounter, connectorCounter); 
					if (connectorInfo.ListIndex != null)
					{
						NodeIdMap.Add((int) connectorInfo.ListIndex, connectorCounter); 
					}
					if (connectorInfo.EmailTemplateIndex != null)
					{
						NodeIdMap.Add((int) connectorInfo.EmailTemplateIndex, connectorCounter);
					}
					connectorCounter += 10;
				}
			}

			Queues = new Dictionary<int, QueueInfo>();
			foreach (var queue in Frisk.Register<BaseQueue<IModuleConfig>>())
			{
				var instance = BaseQueue<IModuleConfig>.Create(queue.Key);
				if (instance != null && instance.Init())
				{
					var config = instance.ReadConfig();
					Queues.Add(connectorCounter, new QueueInfo
					{
						Index = connectorCounter,
						Enable = config.Enable,
						Queue = instance,
						Info = null,
					});
					NodeIdMap.Add(connectorCounter, connectorCounter); 

					connectorCounter += 10;
				}
			}

			var file = WebConfigurationManager.OpenWebConfiguration("~");
			var section = file.GetSection(Config.Models.Terrabec.Tag) as Config.Models.Terrabec;
			if (section == null)
			{
				return;
			}

			Subscriptions = new List<Subscription>();

			foreach (SubscriptionSetting sub in section.Subscriptions)
			{
				foreach (SourceSetting source in sub)
				{
					var connector = Connectors.FirstOrDefault(x => x.Value.Connector.Id == sub.Connector).Value;
					if (connector != null && connector.Enable && connector is ICreateContactFeature && connector is IReadContactFeature && 
						connector is IUpdateContactFeature && connector is IFindContactFeature && connector is IDeleteContactFeature)
					{
						Subscriptions.Add(new Subscription
						{
							Enable = sub.Enable,
							Connector = connector, 
							ListId = sub.List,
							Type = source.Type,
							Source = source.Source
						});
					}
				}
			}
		}

		public ConnectorInfo InfoConnector(int nodeId) => Info(CacheService.Instance.Connectors[nodeId]);

		public ConnectorInfo Info(ConnectorInfo connector)
		{
			if (connector == null || !(connector.Connector is IInfoFeature))
			{
				return null;
			}
			
			if (connector.Info != null)
			{
				return connector;
			}

			return (connector.Info = ((IInfoFeature) connector.Connector).ExecuteInfoFeature()) != null ? connector : null;
		}

		public ConnectorInfo EnumerateLists(int nodeId)
		{
			foreach (var connector in CacheService.Instance.Connectors)
			{
				if (nodeId == connector.Value.ListIndex)
				{
					return EnumerateLists(connector.Value);
				}
			}
			return null;
		}

		public ConnectorInfo EnumerateLists(ConnectorInfo connector)
		{
			if (connector == null || !(connector.Connector is IEnumerateListsFeature))
			{
				return null;
			}
			
			if (connector.Lists != null)
			{
				return connector;
			}

			connector.Lists = new Dictionary<int, IEnumerateListsResult>();
			foreach (var item in ((IEnumerateListsFeature) connector.Connector).ExecuteEnumerateListsFeature() as IEnumerable<IEnumerateListsResult>)
			{
				NodeIdMap.Add(connectorCounter, connector.Index);
				connector.Lists.Add(connectorCounter++, item);
			}
			return connector;
		}

		public ConnectorInfo ReadList(int nodeId)
		{
			foreach (var connector in CacheService.Instance.Connectors)
			{
				if (connector.Value.Lists.ContainsKey(nodeId) || connector.Value.ListItems.ContainsKey(nodeId))
				{
					return ReadList(connector.Value, nodeId);
				}
			}

			return null;
		}

		public ConnectorInfo ReadList(ConnectorInfo connector, int nodeId)
		{
			if (connector == null || !(connector.Connector is IReadListFeature))
			{
				return null;
			}
			
			if (connector.ListItems == null)
			{
				connector.ListItems = new Dictionary<int, IReadListResult>();
			}
			else if (connector.ListItems.ContainsKey(nodeId))
			{
				return connector;
			}

			connector.ListItems.Add(nodeId, ((IReadListFeature) connector.Connector).ExecuteReadListFeature(connector.Lists[nodeId].Id));
			return connector;
		}

		public ConnectorInfo EnumerateEmailTemplates(int nodeId)
		{
			foreach (var connector in CacheService.Instance.Connectors)
			{
				if (nodeId == connector.Value.EmailTemplateIndex)
				{
					return EnumerateEmailTemplates(connector.Value);
				}
			}
			return null;
		}

		public ConnectorInfo EnumerateEmailTemplates(ConnectorInfo connector)
		{
			if (connector == null || !(connector.Connector is IEnumerateEmailTemplatesFeature))
			{
				return null;
			}
			
			if (connector.EmailTemplates != null)
			{
				return connector;
			}


			connector.EmailTemplates = new Dictionary<int, IEnumerateEmailTemplatesResult>();
			foreach (var item in ((IEnumerateEmailTemplatesFeature) connector.Connector).ExecuteEnumerateEmailTemplatesFeature() as IEnumerable<IEnumerateEmailTemplatesResult>)
			{
				NodeIdMap.Add(connectorCounter, connector.Index);
				connector.EmailTemplates.Add(connectorCounter++, item);
			}
			return connector;
		}

		public ConnectorInfo ReadEmailTemplate(int nodeId)
		{
			foreach (var connector in CacheService.Instance.Connectors)
			{
				if (connector.Value.EmailTemplates.ContainsKey(nodeId) || connector.Value.EmailTemplateItems.ContainsKey(nodeId))
				{
					return ReadEmailTemplate(connector.Value, nodeId);
				}
			}

			return null;
		}

		public ConnectorInfo ReadEmailTemplate(ConnectorInfo connector, int nodeId)
		{
			if (connector == null || !(connector.Connector is IReadEmailTemplateFeature))
			{
				return null;
			}
			
			if (connector.EmailTemplateItems == null)
			{
				connector.EmailTemplateItems = new Dictionary<int, IReadEmailTemplateResult>();
			}
			else if (connector.EmailTemplateItems.ContainsKey(nodeId))
			{
				return connector;
			}

			connector.EmailTemplateItems.Add(nodeId, ((IReadEmailTemplateFeature) connector.Connector).ExecuteReadEmailTemplateFeature(connector.EmailTemplates[nodeId].Id));
			return connector;
		}

		public QueueInfo InfoQueue(int nodeId) => Info(CacheService.Instance.Queues[nodeId]);

		public QueueInfo Info(QueueInfo queue)
		{
			if (queue == null || !(queue.Queue is IInfoFeature))
			{
				return null;
			}
			
			if (queue.Info != null)
			{
				return queue;
			}

			return (queue.Info = ((IInfoFeature) queue.Queue).ExecuteInfoFeature()) != null ? queue : null;
		}

		public IRefreshResult RefreshQueue(int nodeId) => RefreshQueue(CacheService.Instance.Queues[nodeId]);

		public IRefreshResult RefreshQueue(QueueInfo queue)
		{
			if (queue == null || !(queue.Queue is IRefreshFeature))
			{
				return null;
			}
			return ((IRefreshFeature) queue.Queue).ExecuteRefreshFeature();
		}
	}
}
