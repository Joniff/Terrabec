using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Terrabec.Cache;
using Terrabec.Connectors.CreateList;
using Terrabec.Connectors.SendEmail;
using Terrabec.Modules.Info;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Terrabec.Api
{
	[PluginController(TreeController.Register.SectionAlias)]
	public class TerrabecApiController : UmbracoAuthorizedJsonController
	{
		[System.Web.Http.HttpGet]
		public bool RefreshConnector(int nodeId)
		{
			CacheService.RefreshConnector(nodeId);
			return true;
		}
			
		public class ConnectorDto
		{
			[JsonProperty("nodeId")]
			public int NodeId { get; set; }

			[JsonProperty("listsNodeId")]
			public int? ListsNodeId { get; set; }

			[JsonProperty("emailTemplatesNodeId")]
			public int? EmailTemplatesNodeId { get; set; }

			[JsonProperty("id")]
			public string Id { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("description")]
			public string Description { get; set; }

			[JsonProperty("icon")]
			public string Icon { get; set; }

			[JsonProperty("image")]
			public string Image { get; set; }

			[JsonProperty("url")]
			public string Url { get; set; }
		}

		[System.Web.Http.HttpGet]
		public IEnumerable<ConnectorDto> Connectors()
		{
			return CacheService.Instance.Connectors.Select(x => new ConnectorDto
			{
				NodeId = x.Key,
				ListsNodeId = x.Value.ListIndex,
				EmailTemplatesNodeId = x.Value.EmailTemplateIndex,
				Id = x.Value.Connector.Id,
				Name = x.Value.Connector.Name,
				Description = x.Value.Connector.Description,
				Icon = x.Value.Connector.Icon,
				Image = x.Value.Connector.Image,
				Url = x.Value.Connector.Url
			});
		}

		[System.Web.Http.HttpGet]
		public ConnectorDto Connector(int nodeId)
		{
			var connector = CacheService.Instance.Connectors[CacheService.Instance.NodeIdMap[nodeId]];
			return new ConnectorDto
			{
				NodeId = connector.Index,
				ListsNodeId = connector.ListIndex,
				EmailTemplatesNodeId = connector.EmailTemplateIndex,
				Id = connector.Connector.Id,
				Name = connector.Connector.Name,
				Description = connector.Connector.Description,
				Icon = connector.Connector.Icon,
				Image = connector.Connector.Image,
				Url = connector.Connector.Url
			};
		}

		[System.Web.Http.HttpGet]
		public IInfoResults ConnectorInfo(int nodeId)
		{
			return CacheService.Instance.InfoConnector(CacheService.Instance.NodeIdMap[nodeId]).Info;
		}

		public class ListDto
		{
			[JsonProperty("nodeId")]
			public int NodeId { get; set; }

			[JsonProperty("id")]
			public string Id { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("subscribers", NullValueHandling = NullValueHandling.Ignore)]
			public int? Subscribers { get; set; }

			[JsonProperty("unsubscribers", NullValueHandling = NullValueHandling.Ignore)]
			public int? Unsubscribers { get; set; }
		}			


		[System.Web.Http.HttpGet]
		public IEnumerable<ListDto> ConnectorEnumerateLists(int nodeId)
		{
			var results = new List<ListDto>();
			foreach (var item in CacheService.Instance.EnumerateLists(CacheService.Instance.Connectors[CacheService.Instance.NodeIdMap[nodeId]]).Lists.OrderBy(x => x.Value.Name))
			{
				results.Add(new ListDto
				{
					NodeId = item.Key,
					Id = item.Value.Id,
					Name = item.Value.Name
				});
			}
			return results;
		}

		[System.Web.Http.HttpGet]
		public ListDto ConnectorReadList(int nodeId)
		{
			var list = CacheService.Instance.ReadList(CacheService.Instance.Connectors[CacheService.Instance.NodeIdMap[nodeId]], nodeId).ListItems[nodeId];
			return new ListDto
			{
				NodeId = nodeId,
				Id = list.Id,
				Name = list.Name,
				Subscribers = list.Subscribers,
				Unsubscribers = list.Unsubscribers
			};
		}

		public class EmailDto
		{
			[JsonProperty("nodeId")]
			public int NodeId { get; set; }

			[JsonProperty("id")]
			public string Id { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("created", NullValueHandling = NullValueHandling.Ignore)]
			public DateTime? Created { get; set; }

			[JsonProperty("lastBroadcast", NullValueHandling = NullValueHandling.Ignore)]
			public DateTime? LastBroadcast { get; set; }

			[JsonProperty("subject", NullValueHandling = NullValueHandling.Ignore)]
			public string Subject { get; set; }

			[JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
			public string Content { get; set; }
		}			


		[System.Web.Http.HttpGet]
		public IEnumerable<EmailDto> ConnectorEnumerateEmailTemplates(int nodeId)
		{
			var results = new List<EmailDto>();
			foreach (var item in CacheService.Instance.EnumerateEmailTemplates(CacheService.Instance.Connectors[CacheService.Instance.NodeIdMap[nodeId]]).EmailTemplates.OrderBy(x => x.Value.Name))
			{
				results.Add(new EmailDto
				{
					NodeId = item.Key,
					Id = item.Value.Id,
					Name = item.Value.Name,
				});
			}
			return results;
		}

		[System.Web.Http.HttpGet]
		public EmailDto ConnectorReadEmailTemplate(int nodeId)
		{
			var email = CacheService.Instance.ReadEmailTemplate(CacheService.Instance.Connectors[CacheService.Instance.NodeIdMap[nodeId]], nodeId).EmailTemplateItems[nodeId];
			return new EmailDto
			{
				NodeId = nodeId,
				Id = email.Id,
				Name = email.Name,
				Created = email.Created,
				LastBroadcast = email.LastBroadcast,
				Subject = email.Subject,
				Content = HttpUtility.JavaScriptStringEncode(email.Content)
			};
		}

		public class SubscriptionDto
		{
			[JsonProperty("enable")]
			public bool Enable { get; set; }

			[JsonProperty("connectorId")]
			public string ConnectorId;

			[JsonProperty("listId")]
			public string ListId;

			[JsonProperty("sourceType")]
			public Config.Models.SourceSetting.SourceType SourceType;

			[JsonProperty("source")]
			public string Source;

			internal SubscriptionDto(CacheService.Subscription sub)
			{
				Enable = sub.Enable;
				ConnectorId = sub.Connector.Connector.Id;
				ListId = sub.ListId;
				SourceType = sub.Type;
				Source = sub.Source;
			}
		}

		[System.Web.Http.HttpGet]
		public IEnumerable<SubscriptionDto> SubscriptionEnumerateSubscriptions(int nodeId)
		{
			if (nodeId == global::Umbraco.Core.Constants.System.Root)
			{
				return CacheService.Instance.Subscriptions.Select(x => new SubscriptionDto(x));
			}
			var connector = CacheService.Instance.Connectors[CacheService.Instance.NodeIdMap[nodeId]];
			if (nodeId == connector.Index || nodeId == connector.ListIndex)
			{
				return CacheService.Instance.Subscriptions.Where(x => x.Connector.Connector.Id == connector.Connector.Id).Select(x => new SubscriptionDto(x));
			}
			if (connector.ListItems.ContainsKey(nodeId))
			{
				return CacheService.Instance.Subscriptions.Where(x => x.Connector.Connector.Id == connector.Connector.Id && x.ListId == connector.ListItems[nodeId].Id).Select(x => new SubscriptionDto(x));
			}

			//	Not sure what this nodeId refers too
			return null;
		}

		[System.Web.Http.HttpGet]
		public ListDto ConnectorCreateList(int nodeId, string name)
		{
			var connector = CacheService.Instance.Connectors[CacheService.Instance.NodeIdMap[nodeId]];
			CacheService.Instance.EnumerateLists(connector);
			if (connector.Lists.Any(x => x.Value.Name == name))
			{
				//	Duplicate name
				return null;
			}
			var list = ((ICreateListFeature) connector.Connector).ExecuteCreateListFeature(name);

			//	Clear out cache
			connector.Lists = null;
			connector.ListItems = null;

			return new ListDto
			{
				NodeId = nodeId,
				Id = list.Id,
				Name = list.Name,
				Subscribers = list.Subscribers,
				Unsubscribers = list.Unsubscribers
			};
		}

		[System.Web.Http.HttpGet]
		public bool SendEmail(int nodeId, string emailAddress)
		{
			var connector = CacheService.Instance.Connectors[CacheService.Instance.NodeIdMap[nodeId]];
			var emailTemplate = connector.EmailTemplateItems[nodeId];
			return ((ISendEmailFeature) connector.Connector).ExecuteSendEmailFeature(emailTemplate.Id, emailAddress, null).Success;
		}

		public class QueueDto
		{
			[JsonProperty("nodeId")]
			public int NodeId { get; set; }

			[JsonProperty("id")]
			public string Id { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("description")]
			public string Description { get; set; }

			[JsonProperty("icon")]
			public string Icon { get; set; }

			[JsonProperty("image")]
			public string Image { get; set; }

			[JsonProperty("url")]
			public string Url { get; set; }


			[JsonProperty("emailsNotSent", NullValueHandling = NullValueHandling.Ignore)]
			public int? EmailsNotSent { get; set; }

			[JsonProperty("emailsSent", NullValueHandling = NullValueHandling.Ignore)]
			public int? EmailsSent { get; set; }
		}


		[System.Web.Http.HttpGet]
		public IEnumerable<QueueDto> Queues()
		{
			return CacheService.Instance.Queues.Select(x => new QueueDto
			{
				NodeId = x.Key,
				Id = x.Value.Queue.Id,
				Name = x.Value.Queue.Name,
				Description = x.Value.Queue.Description,
				Icon = x.Value.Queue.Icon,
				Image = x.Value.Queue.Image,
				Url = x.Value.Queue.Url
			});
		}

		[System.Web.Http.HttpGet]
		public QueueDto Queue(int nodeId)
		{
			var queue = CacheService.Instance.Queues[CacheService.Instance.NodeIdMap[nodeId]];
			return new QueueDto
			{
				NodeId = nodeId,
				Id = queue.Queue.Id,
				Name = queue.Queue.Name,
				Description = queue.Queue.Description,
				Icon = queue.Queue.Icon,
				Image = queue.Queue.Image,
				Url = queue.Queue.Url
			};
		}

		[System.Web.Http.HttpGet]
		public IInfoResults QueueInfo(int nodeId)
		{
			return CacheService.Instance.InfoQueue(CacheService.Instance.NodeIdMap[nodeId]).Info;
		}

		[System.Web.Http.HttpGet]
		public bool RefreshQueue(int nodeId)
		{
			return CacheService.Instance.RefreshQueue(nodeId).Refreshed;
		}

	}
}
