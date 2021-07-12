using System;
using System.Linq;
using System.Net.Http.Formatting;
using Terrabec.Cache;
using Terrabec.Connectors.CreateList;
using Terrabec.Connectors.ReadEmailTemplate;
using Terrabec.Connectors.ReadList;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;

namespace Terrabec.TreeController
{
	[PluginController(Register.SectionName)]
	[Tree(Register.SectionAlias, Register.SectionAlias, Register.SectionName, iconClosed: "icon-doc")]
	public class TerrabecTreeController : Umbraco.Web.Trees.TreeController
	{
		public const int RootId = global::Umbraco.Core.Constants.System.Root;

		//	List
		public string ListsName =>  Services.TextService.Localize("TerrabecTree/ListName");
		public const string ListsIcon = "icon-users-alt color-indigo";
		public const string ListIcon = "icon-umb-users color-blue";

		//	Email Templates
		public string EmailTemplatesName =>  Services.TextService.Localize("TerrabecTree/EmailTemplateName");
		public const string EmailTemplatesIcon = "icon-layers-alt color-indigo";
		public const string EmailTemplateIcon = "icon-message color-blue";

		public override string RootNodeDisplayName => Services.TextService.Localize("TerrabecTree/Heading");

		protected override TreeNodeCollection GetTreeNodes(string idText, FormDataCollection queryStrings)
		{
			var id = int.Parse(idText);
			var tree = new TreeNodeCollection();

			if (id == RootId)
			{
				CacheService.RefreshConnector();

				foreach(var connector in CacheService.Instance.Connectors)
				{
					var node = CreateTreeNode(
						connector.Key.ToString(),
						$"{RootId}",
						queryStrings,
						connector.Value.Connector.Name,
						connector.Value.Connector.Icon,
						connector.Value.Enable && (connector.Value.ListIndex != null || connector.Value.EmailTemplateIndex != null),
						Register.SectionAlias + "/" + Register.SectionAlias + "/Connector/" + connector.Key.ToString());

					if (!connector.Value.Enable)
					{
						node.SetNotPublishedStyle();
					}

					tree.Add(node);
				}

				foreach(var queue in CacheService.Instance.Queues)
				{
					var node = CreateTreeNode(
						queue.Key.ToString(),
						$"{RootId}",
						queryStrings,
						queue.Value.Queue.Name,
						queue.Value.Queue.Icon,
						false,
						Register.SectionAlias + "/" + Register.SectionAlias + "/Queue/" + queue.Key.ToString());

					if (!queue.Value.Enable)
					{
						node.SetNotPublishedStyle();
					}

					tree.Add(node);
				}

				return tree;
			}

			int connectorId;

			if(CacheService.Instance.NodeIdMap.TryGetValue(id, out connectorId))
			{
				var connector = CacheService.Instance.Connectors[connectorId];

				if (id == connectorId)
				{
					if (connector.ListIndex != null)
					{
						CacheService.Instance.EnumerateLists(connector);

						if (connector.Lists.Count != 0)
						{
							var node = CreateTreeNode(
								(connector.ListIndex).ToString(),
								id.ToString(),
								queryStrings,
								ListsName,
								ListsIcon,
								connector.Connector is IReadListFeature,
								Register.SectionAlias + "/" + Register.SectionAlias + "/Lists/" + (connector.ListIndex).ToString());
							
							tree.Add(node);
						}
					}

					if (connector.EmailTemplateIndex != null)
					{
						CacheService.Instance.EnumerateEmailTemplates(connector);

						if (connector.EmailTemplates.Count != 0)
						{
							var node = CreateTreeNode(
								(connector.EmailTemplateIndex).ToString(),
								id.ToString(),
								queryStrings,
								EmailTemplatesName,
								EmailTemplatesIcon,
								connector.Connector is IReadEmailTemplateFeature,
								Register.SectionAlias + "/" + Register.SectionAlias + "/EmailTemplates/" + (connector.EmailTemplateIndex).ToString());
						
							tree.Add(node);
						}
					}

					return tree;
				}

				if (id == connector.ListIndex)
				{
					foreach (var list in connector.Lists.OrderBy(x => x.Value.Name))
					{
						var node = CreateTreeNode(
							(list.Key).ToString(),
							id.ToString(),
							queryStrings,
							list.Value.Name,
							ListIcon,
							false,
							Register.SectionAlias + "/" + Register.SectionAlias + "/List/" + (list.Key).ToString());
						tree.Add(node);
					}
					return tree;
				}

				if (id == connector.EmailTemplateIndex)
				{
					foreach (var emailTemplate in connector.EmailTemplates.OrderBy(x => x.Value.Name))
					{
						var node = CreateTreeNode(
							(emailTemplate.Key).ToString(),
							id.ToString(),
							queryStrings,
							emailTemplate.Value.Name,
							EmailTemplateIcon,
							false,
							Register.SectionAlias + "/" + Register.SectionAlias + "/EmailTemplate/" + (emailTemplate.Key).ToString());
						tree.Add(node);
					}
					return tree;
				}

			}

			throw new ArgumentException("Invalid tree node");
		}
 
		protected override MenuItemCollection GetMenuForNode(string idText, FormDataCollection queryStrings)
		{
			var menu = new MenuItemCollection();
			var id = int.Parse(idText);

			if (id == RootId)
			{
				menu.Items.Add<RefreshNode, umbraco.BusinessLogic.Actions.ActionRefresh>(Services.TextService.Localize("TerrabecTree/Refresh"));
			}
			else
			{
				foreach (var connector in CacheService.Instance.Connectors)
				{
					if (id == connector.Value.ListIndex && connector.Value.Connector is ICreateListFeature)
					{
						menu.Items.Add<umbraco.BusinessLogic.Actions.ActionNew>(Services.TextService.Localize("TerrabecTree/CreateList"));
					}
					//else if (connector.Value.EmailTemplateItems != null && connector.Value.EmailTemplateItems.ContainsKey(id) && connector.Value.Connector is ISendEmailFeature)
					//{
					//	menu.Items.Add<ActionPublish>(Services.TextService.Localize("TerrabecTree/SendEmail"));
					//}

				}
			}
			return menu;
		}
	}
}
