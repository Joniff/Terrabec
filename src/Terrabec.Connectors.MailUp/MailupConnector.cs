using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Terrabec.Connectors.CreateContact;
using Terrabec.Connectors.CreateList;
using Terrabec.Connectors.DeleteContact;
using Terrabec.Connectors.EnumerateContactFields;
using Terrabec.Connectors.EnumerateEmailTemplates;
using Terrabec.Connectors.EnumerateLists;
using Terrabec.Connectors.FindContact;
using Terrabec.Connectors.MailUp.Client;
using Terrabec.Connectors.ReadContact;
using Terrabec.Connectors.ReadEmailTemplate;
using Terrabec.Connectors.ReadList;
using Terrabec.Connectors.SendEmail;
using Terrabec.Connectors.SendEmailFrom;
using Terrabec.Connectors.SendEmailFromWithAttachments;
using Terrabec.Connectors.SendEmailWithAttachments;
using Terrabec.Connectors.UpdateContact;
using Terrabec.Loggers;
using Terrabec.Module;
using Terrabec.Modules.Info;

namespace Terrabec.Connectors.MailUp
{
	public class MailupConnector : BaseConnector<MailUpConfig>, 
		IInfoFeature, 
		IEnumerateListsFeature, IReadListFeature, ICreateListFeature, 
		IEnumerateEmailTemplatesFeature, IReadEmailTemplateFeature, 
		ICreateContactFeature, IReadContactFeature, IUpdateContactFeature, IFindContactFeature,
		ISendEmailFeature, ISendEmailFromFeature, 
		ISendEmailWithAttachmentsFeature, ISendEmailFromWithAttachmentsFeature,
		IEnumerateContactFieldsFeature
	{
		public override string Id => "MailUp";

		public override string Name => "MailUp";

		public override string Description => "Italian based email marketing platform with 10,000 customers sending over 21 billion emails per year";

		public override string Icon => "icon-navigational-arrow color-brown";

		public override string Image => "/App_Plugins/Terrabec.Connectors.MailUp/images/MailupLogo.svg";

		public override string Url => "https://www.mailup.com/";

		public override IModuleConfig DefaultConfig => new MailUpConfig();

		MailUpClient client = null;
		DateTime valid = DateTime.MinValue;
		ILogger Logger;

		private const string EnableGroupName = "Enabled";
		private const string EnableGroupNotes = "All the contacts that can be emailed";

		public override bool Init(ILogger logger)
		{
			var config = ReadConfig() as MailUpConfig;
			Logger = logger;
			return (client = new MailUpClient(config.ClientId, config.ClientSecret, config.SmtpUser, config.SmtpPassword)) != null;
		}

		private void Login()
		{
			if (DateTime.UtcNow > valid)
			{
				var config = ReadConfig() as MailUpConfig;
				client.Login(config.Username, config.Password);
				valid = DateTime.UtcNow.AddMinutes(config.SessionTimeout);
			}
		}

		public IInfoResults ExecuteInfoFeature()
		{
			Login();
			var info = client.GetInfo();

			var results = new InfoResults();

			foreach (var property in info.GetType().GetProperties())
			{
				foreach (var attribute in property.GetCustomAttributes(true))
				{
					if (attribute is DescriptionAttribute)
					{
						results.Add(new InfoResult
						{
							Name = (attribute as DescriptionAttribute).Description,
							Value = property.GetValue(info).ToString()
						});
					}
				}
			}

			return results;
		}

		public IEnumerateListsResults ExecuteEnumerateListsFeature()
		{
			Login();
			var list = client.EnumerateLists();

			var results = new EnumerateListsResults();
			foreach (var item in list)
			{
				results.Add(new EnumerateListsResult
				{
					Id = item.Id,
					Name = item.Name
				});
			}

			return results;
		}

		public IReadListResult ExecuteReadListFeature(string listId)
		{
			Login();
			var list = client.GetList(listId);
			return new ReadListResult
			{
				Id = list.Id,
				Name = list.Name,
				Subscribers = client.CountSubscribers(listId),
				Unsubscribers = client.CountUnsubscribers(listId)
			};
		}

		public IEnumerateEmailTemplatesResults ExecuteEnumerateEmailTemplatesFeature()
		{
			Login();
			var results = new EnumerateEmailTemplatesResults();

			foreach (var item in client.GetEmails())
			{
				results.Add(new EnumerateEmailTemplatesResult
				{
					Id = item.Id,
					Name = item.Name
				});
			}

			return results;
		}

		public IReadEmailTemplateResult ExecuteReadEmailTemplateFeature(string emailTemplateId)
		{
			Login();
			var email = client.GetEmail(emailTemplateId);
			if (email == null)
			{
				return null;
			}
			return new ReadEmailTemplateResult
			{
				Id = email.Id,
				Name = email.Name,
				Created = email.Created,
				LastBroadcast = email.LastBroadcast,
				Subject = email.Subject,
				Content = email.Body
			};
		}

		private MailUpClient.ReadGroupDto CreateEnableGroup(string listId) => client.CreateGroup(listId, new MailUpClient.CreateGroupDto
		{
			Name = EnableGroupName,
			Notes = EnableGroupNotes
		});

		//	We don't require fields when creating lists
		public ICreateListResult ExecuteCreateListFeature(string name)
		{
			Login();
			var config = ReadConfig() as MailUpConfig;
			var result = client.CreateList(new MailUpClient.CreateListDto
			{
				Name = name,
				ReplyTo = config.DefaultReplyAddress,
				OwnerEmail = config.DefaultReplyAddress,
				CompanyName = config.DefaultCompanyName,
				ContactName = config.DefaultContactName,
				Address = config.DefaultAddress,
				City = config.DefaultCity,
				CountryCode = config.DefaultCountry,
				PermissionReminder = config.DefaultPermissionReminder,
				WebSiteUrl = config.DefaultWebsiteUrl
			});

			if (result != null && string.IsNullOrWhiteSpace(result.Id))
			{
				var group = CreateEnableGroup(result.Id);
				if (group == null || string.IsNullOrEmpty(group.Id))
				{
					client.DeleteList(result.Id, result.Guid);
					return null;
				}
			}

			return new CreateListResult
			{
				Id = result.Id,
				Name = result.Name,
				Subscribers = 0,
				Unsubscribers = 0
			};
		}

		private string ToName(string email) => email.Replace("@", "_");

		private IEnumerable<MailUpClient.CreateContactFieldDto> ConvertFields(IContactFields source)
		{
			var fields = new List<MailUpClient.CreateContactFieldDto>();
			foreach (var field in source)
			{
				int id;
				if (int.TryParse(field.Id, out id))
				{
					fields.Add(new MailUpClient.CreateContactFieldDto
					{
						Id = id,
						Description = field.Name,
						Value = field.Value
					});
				}
			}
			return fields;
		}

		private IContactFields ConvertFields(IEnumerable<MailUpClient.CreateContactFieldDto> source)
		{
			var fields = new ContactFields();
			foreach (var field in source)
			{
				fields.Add(new ContactField
				{
					Id = field.Id.ToString(),
					Name = field.Description,
					Value = field.Value
				});
			}
			return fields;
		}
		
		private string EnabledGroupId(string listId)
		{
			var groups = client.EnumerateGroups(listId);
			var enabledGroup = groups.FirstOrDefault(x => string.Compare(x.Name, EnableGroupName, true) == 0);
			if (enabledGroup == null)
			{
				enabledGroup = CreateEnableGroup(listId);
			}
			return enabledGroup != null ? enabledGroup.Id : null;
		}

		private void AddContactToEnabledGroup(string listId, int contactId)
		{
			var groupId = EnabledGroupId(listId);
			if (groupId != null)
			{
				client.AddGroupToContact(groupId, contactId);
			}
		}

		private void RemoveContactFromEnabledGroup(string listId, int contactId)
		{
			var groupId = EnabledGroupId(listId);
			if (groupId != null)
			{
				client.RemoveGroupFromContact(groupId, contactId);
			}
		}

		public ICreateContactResult ExecuteCreateContactFeature(string listId, bool enabled, string email, IContactFields fields)
		{
			Login();
			var infields = new ContactFields();
			var outfields = new List<MailUpClient.CreateContactFieldDto>();
			IEnumerable<MailUpClient.ContactFieldDto> matchedFields = null;
			foreach (var field in fields)
			{
				int id;
				var name = field.Name;
				if (field.Id == null || !int.TryParse(field.Id, out id))
				{
					if (string.IsNullOrWhiteSpace(name))
					{
						throw new ArgumentNullException("Fields have to contain either an id or name or both");
					}
					if (matchedFields == null)
					{
						matchedFields = client.EnumerateDynamicFields();
					}
					var match = matchedFields.FirstOrDefault(x => string.Compare(x.Description, field.Name, true) == 0);
					if (match == null)
					{
						throw new ArgumentNullException($"Field \'{field.Name}\' doesn\'t exist");
					}
					id = match.Id;
				}
				else if (string.IsNullOrWhiteSpace(name))
				{
					if (matchedFields == null)
					{
						matchedFields = client.EnumerateDynamicFields();
					}
					var match = matchedFields.FirstOrDefault(x => x.Id == id);
					if (match == null)
					{
						throw new ArgumentNullException($"Field id of {field.Id} doesn\'t exist");
					}
					name = match.Description;
				}

				infields.Add(new ContactField
				{
					Id = id.ToString(),
					Name = name,
					Value = field.Value ?? ""
				});
				outfields.Add(new MailUpClient.CreateContactFieldDto
				{
					Id = id,
					Description = name,
					Value = field.Value ?? ""
				});
			}

			var result = client.CreateContact(listId, new MailUpClient.CreateContactDto
			{
				Email = email,
				Name = ToName(email),
				Fields = outfields
			});
			if (result == null)
			{
				return null;
			}

			if (enabled)
			{
				AddContactToEnabledGroup(listId, (int) result);
			}

			return new CreateContactResult
			{
				ContactId = result.ToString(),
				Enabled = enabled,
				Email = email,
				Fields = infields
			};
		}

		public IReadContactResult ExecuteReadContactFeature(string listId, string contactId)
		{
			Login();
			int id = 0;
			if (!int.TryParse(contactId, out id))
			{
				return null;
			}

			var enabled = true;
			var result = client.ReadSubscribedContact(listId, id);
			if (result != null)
			{
				var groupId = EnabledGroupId(listId);
				enabled = (groupId != null) ? client.IsContactInGroup(listId, groupId, result.Id) : false;
			}
			else
			{
				result = client.ReadUnsubscribedContact(listId, id);
				if (result == null)
				{
					return null;
				}
				enabled = false;
			}

			return new ReadContactResult
			{
				Enabled = enabled,
				ContactId = result.Id.ToString(),
				Email = result.Email,
				Fields = ConvertFields(result.Fields)
			};
		}

		public IUpdateContactResult ExecuteUpdateContactFeature(string listId, IReadContactResult contact)
		{
			Login();
			int id = 0;
			if (!int.TryParse(contact.ContactId, out id))
			{
				return null;
			}

			var match = client.ReadSubscribedContact(listId, id);
			if (match == null)
			{
				match = client.ReadUnsubscribedContact(listId, id);
				if (match == null)
				{
					//	contact not found in either subscribed or unsubscribed list
					return null;
				}
				if (contact.Enabled)
				{
					//	This will send an email to the contact, to say are they sure
					client.ResubscribeContact(listId, match);
					AddContactToEnabledGroup(listId, id);
				}
			}
			else
			{
				var groupId = EnabledGroupId(listId);
				var enabled = (groupId == null) ? client.IsContactInGroup(listId, groupId, id) : false;
				if (enabled != contact.Enabled)
				{
					if (contact.Enabled)
					{
						client.AddGroupToContact(groupId, id);
					}
					else
					{
						client.RemoveGroupFromContact(groupId, id);
					}
				}
			}


			//	Create and matchup the fields so they share ids
			var fields = new List<MailUpClient.CreateContactFieldDto>();
			foreach (var field in contact.Fields.Where(x => x.Value != null))
			{
				int fieldId = 0;
				string fieldName = field.Name;
				if (field.Id == null || int.TryParse(field.Id, out id))
				{
					var fieldMatch = match.Fields.FirstOrDefault(x => string.Compare(x.Description, field.Name, true) == 0);
					if (fieldMatch != null)
					{
						fieldId = fieldMatch.Id;
						fieldName = fieldMatch.Description;
					}
				}
				else
				{
					var fieldMatch = match.Fields.FirstOrDefault(x => x.Id == fieldId);
					if (fieldMatch != null)
					{
						fieldId = fieldMatch.Id;
						fieldName = fieldMatch.Description;
					}
				}

				fields.Add(new MailUpClient.CreateContactFieldDto
				{
					Id = fieldId,
					Description = fieldName,
					Value = field.Value ?? ""
				});
			}

			var result = client.UpdateContact(listId, new MailUpClient.ReadContactDto
			{
				Id = id,
				Email = contact.Email,
				Name = ToName(contact.Email),
				Fields = fields
			});

			if (result == null)
			{
				return null;
			}

			return new UpdateContactResult
			{
				ContactId = result.Id.ToString(),
				Email = result.Email,
				Enabled = contact.Enabled,
				Fields = ConvertFields(result.Fields)
			};
		}

		public IUpdateContactResult ExecuteUpdateContactFeature(string listId, string contactId, bool enabled, string email, IContactFields fields) =>
			ExecuteUpdateContactFeature(listId, new ReadContactResult
			{
				ContactId = contactId,
				Enabled = enabled,
				Email = email,
				Fields = fields
			});

		public IFindContactResult ExecuteFindContactFeature(string listId, string email)
		{
			Login();
			var enabled = true;
			var result = client.FindSubscribedContact(listId, email).FirstOrDefault();
			if (result != null)
			{
				var groupId = EnabledGroupId(listId);
				enabled = (groupId != null) ? client.IsContactInGroup(listId, groupId, result.Id) : false;
			}
			else
			{
				result = client.FindUnsubscribedContact(listId, email).FirstOrDefault();
				if (result == null)
				{
					return null;
				}
				enabled = false;
			}

			return new FindContactResult
			{
				ContactId = result.Id.ToString(),
				Enabled = enabled,
				Email = result.Email,
				Fields = ConvertFields(result.Fields)
			};
		}

		public IDeleteContactResult ExecuteDeleteContactFeature(string listId, string contactId)
		{
			Login();
			int id = 0;
			if (!int.TryParse(contactId, out id))
			{
				return new DeleteContactResult
				{
					Success = false
				};
			}
			var result = client.ReadSubscribedContact(listId, id);
			if (result != null)
			{
				return new DeleteContactResult
				{
					Success = client.UnsubscribeContact(listId, id)
				};
			}
			return new DeleteContactResult
			{
				Success = true
			};
		}

		public ISendEmailResult ExecuteSendEmailFeature(string emailTemplateId, string email, IContactFields fields)
		{
			Login();
			return new SendEmailResult
			{
				Success = client.SendEmail(
					emailTemplateId, 
					null, 
					new MailUpClient.SendMailDto.EmailAddressDto
					{
						Name = email,
						Email = email
					}, 
					fields.Select(x => new MailUpClient.SendMailDto.FieldDto 
					{
						Name = x.Name,
						Value = x.Value
					})
				)
			};
		}

		public ISendEmailFromResult ExecuteSendEmailFromFeature(string emailTemplateId, string fromName, string fromEmail, string toName, string toEmail, IContactFields fields)
		{
			Login();
			return new SendEmailFromResult
			{
				Success = client.SendEmail(
					emailTemplateId, 
					new MailUpClient.SendMailDto.EmailAddressDto
					{
						Name = fromName,
						Email = fromEmail
					}, new MailUpClient.SendMailDto.EmailAddressDto
					{
						Name = toName,
						Email = toEmail
					}, 
					fields.Select(x => new MailUpClient.SendMailDto.FieldDto 
					{
						Name = x.Name,
						Value = x.Value
					})
				)
			};
		}

		public ISendEmailWithAttachmentsResult ExecuteSendEmailWithAttachmentsFeature(string emailTemplateId, string email, IContactFields fields, IAttachments attachments)
		{
			Login();
			return new SendEmailWithAttachmentsResult
			{
				Success = client.SendEmail(
					emailTemplateId, 
					null, 
					new MailUpClient.SendMailDto.EmailAddressDto
					{
						Name = email,
						Email = email
					}, 
					fields.Select(x => new MailUpClient.SendMailDto.FieldDto 
					{
						Name = x.Name,
						Value = x.Value
					}),
					attachments.Select(x => new MailUpClient.SendMailDto.MessagePartDto
					{
						Filename = x.Filename,
						Body = x.Content
					})
				)
			};
		}

		public ISendEmailFromWithAttachmentsResult ExecuteSendEmailFromWithAttachmentsFeature(string emailTemplateId, string fromName, string fromEmail, string toName, string toEmail, IContactFields fields, IAttachments attachments)
		{
			Login();
			return new SendEmailFromWithAttachmentsResult
			{
				Success = client.SendEmail(
					emailTemplateId, 
					new MailUpClient.SendMailDto.EmailAddressDto
					{
						Name = fromName,
						Email = fromEmail
					}, new MailUpClient.SendMailDto.EmailAddressDto
					{
						Name = toName,
						Email = toEmail
					}, 
					fields.Select(x => new MailUpClient.SendMailDto.FieldDto 
					{
						Name = x.Name,
						Value = x.Value
					}),
					attachments.Select(x => new MailUpClient.SendMailDto.MessagePartDto
					{
						Filename = x.Filename,
						Body = x.Content
					})
				)
			};
		}

		public IEnumerateContactFieldsResults ExecuteEnumerateContactFieldsFeature(string listId)
		{
			Login();
			var results = new List<IContactField>();

			//	All lists in Mailup share the same contact fields
			foreach (var field in client.EnumerateDynamicFields())
			{
				results.Add(new ContactField
				{
					Id = field.Id.ToString(),
					Name = field.Description
				} as IContactField);
			}
			return (IEnumerateContactFieldsResults) results;
		}

	}
}
