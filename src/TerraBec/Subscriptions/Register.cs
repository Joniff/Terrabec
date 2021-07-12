using System.Linq;
using System.Web.Security;
using Terrabec.Cache;
using Terrabec.Connectors;
using Terrabec.Connectors.CreateContact;
using Terrabec.Connectors.DeleteContact;
using Terrabec.Connectors.FindContact;
using Terrabec.Connectors.UpdateContact;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using static Terrabec.Config.Models.SourceSetting;

namespace Terrabec.Subscriptions
{
	public class Register : ApplicationEventHandler 
	{
		protected override void ApplicationStarted(UmbracoApplicationBase umbraco, ApplicationContext context) 
		{
			if (CacheService.Instance.Subscriptions.Any(x => x.Enable && x.Type == SourceType.Member))
			{
				MemberService.Saved += MemberService_Saved;
				MemberService.Deleted += MemberService_Deleted;
			}
			base.ApplicationStarted(umbraco, context);
		}

		private void MemberService_Deleted(IMemberService sender, Umbraco.Core.Events.DeleteEventArgs<IMember> e)
		{
			foreach (var sub in CacheService.Instance.Subscriptions.Where(x => x.Enable && x.Type == SourceType.Member))
			{
				foreach (IMember member in e.DeletedEntities)
				{
					if (!string.IsNullOrWhiteSpace(sub.Source) && !Roles.GetRolesForUser(member.Username).Contains(sub.Source))
					{
						//	This member is not part of the source group
						continue;
					}

					var match = (sub.Connector.Connector as IFindContactFeature).ExecuteFindContactFeature(sub.ListId, member.Email);
		
					if (match != null && !(sub.Connector.Connector as IDeleteContactFeature).ExecuteDeleteContactFeature(sub.ListId, match.ContactId).Success)
					{
						LogHelper.Info(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, $"Unable to delete {member.Email} into list {sub.ListId}");
						return;
					}
				}
				return;
			}
		}

		private void MemberService_Saved(IMemberService sender, Umbraco.Core.Events.SaveEventArgs<IMember> e)
		{
			foreach (var sub in CacheService.Instance.Subscriptions.Where(x => x.Enable && x.Type == SourceType.Member))
			{
				foreach (IMember member in e.SavedEntities)
				{
					if (!string.IsNullOrWhiteSpace(sub.Source) && !Roles.GetRolesForUser(member.Username).Contains(sub.Source))
					{
						//	This member is not part of the source group
						continue;
					}

					var match = (sub.Connector.Connector as IFindContactFeature).ExecuteFindContactFeature(sub.ListId, member.Email);

					var fields = member.Properties.Select(x => new ContactField
					{
						Id = x.Alias,
						Name = x.Alias,
						Value = x.ToString()
					} as IContactField) as IContactFields;

					if (match == null)
					{
						var created = (sub.Connector.Connector as ICreateContactFeature).ExecuteCreateContactFeature(sub.ListId, true, member.Email, fields as IContactFields);
						if (created == null || string.IsNullOrWhiteSpace(created.ContactId))
						{
							LogHelper.Info(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, $"Unable to create {member.Email} into list {sub.ListId}");
							return;
						}
						continue;
					}

					foreach (var field in fields)
					{
						if (!match.Fields.Any(x => x.Id == field.Id && x.Value == field.Value))
						{
							var update = (sub.Connector.Connector as IUpdateContactFeature).ExecuteUpdateContactFeature(sub.ListId, match.ContactId, true, match.Email, fields);
							if (update == null || string.IsNullOrWhiteSpace(update.ContactId))
							{
								LogHelper.Info(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, $"Unable to update {member.Email} into list {sub.ListId}");
								return;
							}

							break;
						}
					}
				}
				return;
			}
		}
	}
}
