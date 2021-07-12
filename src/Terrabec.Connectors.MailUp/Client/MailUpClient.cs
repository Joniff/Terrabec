using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Terrabec.Connectors.MailUp.Client
{
	internal class MailUpClient
	{
		private const int MaxRetries = 50;					//	Maximum retries, when receiving unauthorized issues
		private const int MaxSpeedInMilliseconds = 500;		//	Stop any requests quicker than this
		private const int PageSize = 20;					//	Page size

		private DateTime GateKeep = DateTime.MinValue;
		private void GateKeeper()
		{
			if (GateKeep > DateTime.Now)
			{
				System.Threading.Thread.Sleep(MaxSpeedInMilliseconds);
			}
			GateKeep = DateTime.Now.AddMilliseconds(MaxSpeedInMilliseconds);
		}

		public enum ContentType
		{
			Json,
			Xml
		}

		private class EndPoints
		{
			private const string RootUrl = "https://services.mailup.com";

			public const string Login = RootUrl + "/Authorization/OAuth/LogOn";

			public const string Authorization = RootUrl + "/Authorization/OAuth/Authorization";

			public const string Token = RootUrl + "/Authorization/OAuth/Token";

			public const string Console = RootUrl + "/API/v1.1/Rest/ConsoleService.svc/Console";

			public const string MailStatistics = RootUrl + "/API/v1.1/Rest/MailStatisticsService.svc";

			public const string AuthenticationInfo = Console  + "/Authentication/Info";

			public const string List = Console + "/List";

			public const string RecipientDetail = Console + "/Recipient/Detail";

			public const string Group = Console + "/Group";

			public const string SendEmail = "https://send.mailup.com/API/v2.0/messages/sendmessage";
		}

		private string ClientId { get;set; }

		private string ClientSecret { get; set; }

		private string SmtpUser { get;set; }

		private string SmtpPassword { get; set; }

		private string AccessToken { get; set; }

		private string RefreshToken { get; set; }
		
		private CacheService cache;

		public MailUpClient(string clientId, string clientSecret, string smtpUser, string smtpPassword)
		{
			ClientId = clientId;
			ClientSecret = clientSecret;
			SmtpUser = smtpUser;
			SmtpPassword = smtpPassword;
			cache = new CacheService();
		}

		public string GetLoginUri(string returnUri) => EndPoints.Login + "?client_id=" + ClientId + "&client_secret=" + ClientSecret + "&response_type=code&redirect_uri=" + returnUri;

		private string GetResponse(HttpWebRequest request) => GetResponse((HttpWebResponse)request.GetResponse());

		private string GetResponse(HttpWebResponse retreiveResponse)
		{
			//statusCode = (int)retreiveResponse.StatusCode;
			Stream objStream = retreiveResponse.GetResponseStream();
			StreamReader objReader = new StreamReader(objStream);
			string json = objReader.ReadToEnd();
			retreiveResponse.Close();
			return json;
		}
		
		private void SetRequest(HttpWebRequest request, string body)
		{
			byte[] byteArray = Encoding.UTF8.GetBytes(body);
			request.ContentLength = byteArray.Length;
			Stream dataStream = request.GetRequestStream();
			dataStream.Write(byteArray, 0, byteArray.Length);
			dataStream.Close();
		}

		public string Login(string code)
		{
			GateKeeper();
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(EndPoints.Token + "?code=" + code + "&grant_type=authorization_code");
			request.AllowAutoRedirect = false;
			request.KeepAlive = true;
			try
			{
				var json = GetResponse(request);
				AccessToken = ExtractJsonValue(json, "access_token");
				RefreshToken = ExtractJsonValue(json, "refresh_token");

				return AccessToken;
				//SaveToken();
			}
			catch (WebException wex)
			{
				HttpWebResponse wrs = (HttpWebResponse)wex.Response;
				throw new MailUpException((int)wrs.StatusCode, wex.Message);
			}
			catch (Exception ex)
			{
				throw new MailUpException(0, ex.Message);
			}
		}

		public string Login(string login, string password)
		{
			for (int retry = MaxRetries; retry != 0; retry--)
			{
				GateKeeper();
				CookieContainer cookies = new CookieContainer();
				HttpWebRequest wrLogon = (HttpWebRequest)WebRequest.Create(EndPoints.Token);
				wrLogon.CookieContainer = cookies;
				wrLogon.AllowAutoRedirect = false;
				wrLogon.KeepAlive = true;
				wrLogon.Method = WebRequestMethods.Http.Post;
				wrLogon.ContentType = "application/x-www-form-urlencoded";

				string auth = string.Format("{0}:{1}", ClientId, ClientSecret);
				wrLogon.Headers["Authorization"] = "Basic " + Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(auth));
				try
				{

					SetRequest(wrLogon, "client_id=" + ClientId + "&client_secret=" + ClientSecret + "&grant_type=password&username=" + login + "&password=" + password);
					string json = GetResponse(wrLogon);

					AccessToken = ExtractJsonValue(json, "access_token");
					RefreshToken = ExtractJsonValue(json, "refresh_token");

					return AccessToken;
				}
				catch (WebException wex)
				{
					if (retry != 0)
					{
						continue;
					}

					HttpWebResponse wrs = (HttpWebResponse)wex.Response;
					throw new MailUpException((int)wrs.StatusCode, wex.Message);
				}
				catch (Exception ex)
				{
					if (retry != 0)
					{
						continue;
					}
					throw new MailUpException(0, ex.Message);
				}
			}
			throw new MailUpException(0, "Unreachable code detected");
		}

		private string RefreshAccessToken()
		{
			GateKeeper();
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(EndPoints.Token);
			request.AllowAutoRedirect = false;
			request.KeepAlive = true;
			request.Method = WebRequestMethods.Http.Post;
			request.ContentType = "application/x-www-form-urlencoded";
			try
			{
				SetRequest(request, "client_id=" + ClientId + "&client_secret=" + ClientSecret + "&refresh_token=" + RefreshToken + "&grant_type=refresh_token");
				var json = GetResponse(request);

				AccessToken = ExtractJsonValue(json, "access_token");
				RefreshToken = ExtractJsonValue(json, "refresh_token");
			}
			catch (WebException wex)
			{
				HttpWebResponse wrs = (HttpWebResponse)wex.Response;
				throw new MailUpException((int)wrs.StatusCode, wex.Message);
			}
			catch (Exception ex)
			{
				throw new MailUpException(0, ex.Message);
			}
			return AccessToken;
		}

		private string CallMethod(string url, System.Net.Http.HttpMethod verb, string body = null, ContentType contentType = ContentType.Json, IEnumerable<KeyValuePair<string, string>> headers = null, int refresh = MaxRetries)
		{
			GateKeeper();
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.AllowAutoRedirect = false;
			request.KeepAlive = true;
			request.Method = verb.ToString();
			request.ContentType = GetContentTypestring(contentType);
			request.ContentLength = 0;
			request.Accept = GetContentTypestring(contentType);
			request.Headers.Add("Authorization", "Bearer " + AccessToken);
			if (headers != null)
			{
				foreach (var header in headers)
				{
					request.Headers.Add(header.Key, header.Value);
				}
			}
			try
			{
				if (!string.IsNullOrWhiteSpace(body))
				{
					SetRequest(request, body);
				}

				return GetResponse(request);
			}
			catch (WebException wex)
			{
				try
				{
					HttpWebResponse errorResponse = (HttpWebResponse)wex.Response;
					if ((errorResponse.StatusCode == HttpStatusCode.Unauthorized || ((int) errorResponse.StatusCode) == 429) && refresh != 0)
					{
						System.Threading.Thread.Sleep(MaxSpeedInMilliseconds * 2);
						RefreshAccessToken();
						return CallMethod(url, verb, body, contentType, headers, refresh - 1);
					}
					var errorJson = GetResponse(errorResponse);
					if (string.IsNullOrWhiteSpace(errorJson))
					{
						throw new MailUpException((int)errorResponse.StatusCode, wex.Message);
					}
					var errorDescription = ExtractJsonValue(errorJson, "ErrorDescription");
					if (string.IsNullOrWhiteSpace(errorDescription))
					{
						throw new MailUpException((int)errorResponse.StatusCode, wex.Message);
					}
					throw new MailUpException((int)errorResponse.StatusCode, errorDescription);

				}
				catch (Exception ex)
				{
					throw new MailUpException(0, ex.Message);
				}
			}
			catch (IOException ex)
			{
				if (refresh != 0)
				{
					System.Threading.Thread.Sleep(MaxSpeedInMilliseconds * 2);
					RefreshAccessToken();
					return CallMethod(url, verb, body, contentType, headers, refresh - 1);
				}
				throw new MailUpException(0, ex.Message);
			}				
			catch (Exception ex)
			{
				throw new MailUpException(0, ex.Message);
			}
		}

		private string ExtractJsonValue(string json, string name)
		{
			string delim = "\"" + name + "\":\"";
			int start = json.IndexOf(delim) + delim.Length;
			int end = json.IndexOf("\"", start + 1);
			return (end > start && start > -1 && end > -1) ? json.Substring(start, end - start) : "";
		}

		private string GetContentTypestring(ContentType cType) => (cType == ContentType.Json) ? "application/json" : "application/xml";


		private class DateFormatConverter : IsoDateTimeConverter
		{
			public DateFormatConverter(string format, DateTimeStyles style)
			{
				DateTimeFormat = format;
				DateTimeStyles = style;
			}
		}

		public class InfoDto
		{
			[Description("Id")]
			[JsonProperty(PropertyName = "UID")]
			public string Id { get; set; }

			[Description("Company")]
			[JsonProperty(PropertyName = "Company")]
			public string Company { get; set; }

			[Description("Expiry")]
			[JsonProperty(PropertyName = "ExpiryDate")]
			[JsonConverter(typeof(DateFormatConverter), new object[] {"yyyy-MM-dd HH:mm:ss.fff", DateTimeStyles.AssumeUniversal})]
			public DateTime Expiry { get; set; }

			[Description("IsTrial")]
			[JsonProperty(PropertyName = "IsTrial")]
			public bool IsTrial { get; set; }

			[Description("Url")]
			[JsonProperty(PropertyName = "Url")]
			public string Url { get; set; }

			[Description("Username")]
			[JsonProperty(PropertyName = "Username")]
			public string Username { get; set; }

			[Description("Version")]
			[JsonProperty(PropertyName = "Version")]
			public string Version { get; set; }
		}

		public InfoDto GetInfo() => cache.GetCache<InfoDto>("I", () => JsonConvert.DeserializeObject<InfoDto>(CallMethod(EndPoints.AuthenticationInfo, System.Net.Http.HttpMethod.Get)));

		public interface IItem
		{
		}

		private class ListDto<T> where T : IItem
		{
			[JsonProperty(PropertyName = "IsPaginated")]
			public bool IsPaginated { get; set; }

			[JsonProperty(PropertyName = "Items")]
			public List<T> Items { get; set; }

			[JsonProperty(PropertyName = "PageNumber")]
			public int PageNumber { get; set; }

			[JsonProperty(PropertyName = "PageSize")]
			public int PageSize { get; set; }

			[JsonProperty(PropertyName = "Skipped")]
			public int Skipped { get; set; }

			[JsonProperty(PropertyName = "TotalElementsCount")]
			public int TotalCount { get; set; }
		}

		public class ListItemDto : IItem
		{
			[JsonProperty(PropertyName = "IdList")]
			public string Id { get; set; }

			[JsonProperty(PropertyName = "ListGuid")]
			public Guid Guid { get; set; }

			[JsonProperty(PropertyName = "Name")]
			public string Name { get; set; }

			[JsonProperty(PropertyName = "Company")]
			public string Company { get;set; }

			[JsonProperty(PropertyName = "Description")]
			public string Description { get;set; }

			[JsonProperty(PropertyName = "MultiOptoutList")]
			public int? MultiOptoutList { get;set; }

			[JsonProperty(PropertyName = "OwnerEmail")]
			public string FromEmail { get;set; }

			[JsonProperty(PropertyName = "ReplyTo")]
			public string ReplyTo { get;set; }
		}

		public IEnumerable<ListItemDto> EnumerateLists() => cache.GetCache<List<ListItemDto>>("L", () => JsonConvert.DeserializeObject<ListDto<ListItemDto>>(CallMethod(EndPoints.List, System.Net.Http.HttpMethod.Get)).Items);

		public ListItemDto GetList(string id) => cache.GetCache<ListItemDto>("L#" + id, () => JsonConvert.DeserializeObject<ListItemDto>(CallMethod(EndPoints.List + "/" + id, System.Net.Http.HttpMethod.Get)));

		public class EmailItemDto : IItem
		{
			[JsonProperty(PropertyName = "idMessage")]
			public string Id { get; set; }

			[JsonProperty(PropertyName = "idList")]
			public string ListId { get; set; }

			[JsonProperty(PropertyName = "Notes")]
			public string Name { get; set; }

			[JsonProperty(PropertyName = "Subject")]
			public string Subject { get;set; }

			[JsonProperty(PropertyName = "CreationDate")]
			[JsonConverter(typeof(DateFormatConverter), new object[] {"yyyy-MM-dd HH:mm:ssZ", DateTimeStyles.AssumeUniversal})]
			public DateTime? Created { get; set; }

			[JsonProperty(PropertyName = "LastSendDate")]
			[JsonConverter(typeof(DateFormatConverter), new object[] {"yyyy-MM-dd HH:mm:ssZ", DateTimeStyles.AssumeUniversal})]
			public DateTime? LastBroadcast { get; set; }

			[JsonProperty(PropertyName = "Url")]
			public string Url { get; set; }

			[JsonProperty(PropertyName = "Content")]
			public string Body { get; set; }
		}
	
		private List<T> Paging<T>(string url, System.Net.Http.HttpMethod verb, string body = null, ContentType contentType = ContentType.Json, IEnumerable<KeyValuePair<string, string>> headers = null, int refresh = MaxRetries) where T : IItem
		{
			int page = 0;
			var results = new List<T>();
			ListDto<T> resultPage = null;
			do
			{
				resultPage = JsonConvert.DeserializeObject<ListDto<T>>(CallMethod(url + "?PageNumber=" + page.ToString() + "&PageSize=" + PageSize, verb, body, contentType, headers, refresh));
				if (resultPage.Items == null || resultPage.Items.Count == 0)
				{
					break;
				}

				results.AddRange(resultPage.Items);
				page++;
			}
			while(resultPage.TotalCount > results.Count);

			return results;
		}

		private IEnumerable<EmailItemDto> GetEmailsForList(string id) => cache.GetCache<List<EmailItemDto>>("LE#" + id, () => 
			Paging<EmailItemDto>(EndPoints.List + "/" + id + "/Emails", System.Net.Http.HttpMethod.Get));

		public IEnumerable<EmailItemDto> GetEmails() => cache.GetCache<List<EmailItemDto>>("E", () => 
		{ 
			var results = new List<EmailItemDto>();
			var lists = EnumerateLists();
			foreach (var list in lists)
			{
				var emails = GetEmailsForList(list.Id);
				foreach (var email in emails)
				{
					if (string.IsNullOrWhiteSpace(email.Name))
					{
						email.Name = email.Subject;
					}
					results.Add(email);
				}
			}

			foreach (var email in results)
			{
				if (results.Any(x => x.Id != email.Id && string.Compare(x.Name, email.Name, true) == 0))
				{
					foreach (var match in results.Where(x => string.Compare(x.Name, email.Name, true) == 0))
					{
						match.Name = lists.FirstOrDefault(x => x.Id == match.ListId).Name + " - " + match.Name;
					}
				}
			}

			return results;
		});

		public EmailItemDto GetEmail(string listId, string id) => cache.GetCache<EmailItemDto>("L#" + listId + "#E#" + id, () => 
		{
			return JsonConvert.DeserializeObject<EmailItemDto>(CallMethod(EndPoints.List + "/" + listId + "/Email/" + id, System.Net.Http.HttpMethod.Get));
		});

		public EmailItemDto GetEmail(string id) => cache.GetCache<EmailItemDto>("E#" + id, () => 
		{
			foreach (var list in EnumerateLists())
			{
				var emails = GetEmailsForList(list.Id);
				foreach (var email in emails)
				{
					if (email.Id == id)
					{
						return GetEmail(list.Id, id);
					}
				}
			}

			return null;	//	No found email
		});

		public class CreateListDto
		{
			public string Name;
			public bool Business = false;
			public bool Customer = true;
			public string OwnerEmail; 
			public string ReplyTo;
			public string NLSenderName => ContactName;
			public string CompanyName;
			public string ContactName;
			public string Address;
			public string City;
			public string CountryCode;
			public string PermissionReminder;
			public string WebSiteUrl;
			public bool UseDefaultSettings = true;
		}

		public ListItemDto CreateList(CreateListDto info)
		{
			cache.ClearCache();
			return JsonConvert.DeserializeObject<ListItemDto>(CallMethod(EndPoints.List, System.Net.Http.HttpMethod.Post, JsonConvert.SerializeObject(info)));
		}

		public bool DeleteList(string listId, Guid listGuid)
		{
			cache.ClearCache();

			var headers = new KeyValuePair<string, string>[] 
			{ 
				new KeyValuePair<string, string>("if-match", listGuid.ToString())
			};

			CallMethod(EndPoints.List + "/" + listId, System.Net.Http.HttpMethod.Delete, null, ContentType.Json, headers);
			return true;
		}

		public class ContactFieldDto : IItem
		{
			public string Description;
			public int Id;
		}

		public class CreateContactFieldDto : ContactFieldDto
		{
			public string Value;
		}

		public class CreateContactDto : IItem
		{
			public string Name;
			public string Email;

			public List<CreateContactFieldDto> Fields;
		}

		public int? CreateContact(string listId, CreateContactDto info)
		{
			return JsonConvert.DeserializeObject<int>(CallMethod(EndPoints.List + "/" + listId + "/Recipient", System.Net.Http.HttpMethod.Post, JsonConvert.SerializeObject(info)));
		}
			
		public class ReadContactDto : CreateContactDto
		{
			[JsonProperty(PropertyName = "idRecipient")]
			public int Id;
		}

		public int CountSubscribers(string listId) => cache.GetCache<int>("L#" + listId + "#CNS", () => 
			JsonConvert.DeserializeObject<ListDto<ReadContactDto>>(CallMethod(EndPoints.List + "/" + listId + "/Recipients/Subscribed", System.Net.Http.HttpMethod.Get)).TotalCount);

		public int CountUnsubscribers(string listId) => cache.GetCache<int>("L#" + listId + "#CNU", () => 
			JsonConvert.DeserializeObject<ListDto<ReadContactDto>>(CallMethod(EndPoints.List + "/" + listId + "/Recipients/Unsubscribed", System.Net.Http.HttpMethod.Get)).TotalCount);


		public IEnumerable<ReadContactDto> FindSubscribedContact(string listId, string email) => cache.GetCache<List<ReadContactDto>>("L#" + listId + "#CFS#" + email, () => 
			GetListDto<ReadContactDto>(CallMethod(EndPoints.List + "/" + listId + "/Recipients/Subscribed?filterby=\"Email=='" + email + "'\"", System.Net.Http.HttpMethod.Get)));

		public IEnumerable<ReadContactDto> FindUnsubscribedContact(string listId, string email) => cache.GetCache<List<ReadContactDto>>("L#" + listId + "#CFU#" + email, () => 
			GetListDto<ReadContactDto>(CallMethod(EndPoints.List + "/" + listId + "/Recipients/Unsubscribed?filterby=\"Email=='" + email + "'\"", System.Net.Http.HttpMethod.Get)));

		public ReadContactDto ReadSubscribedContact(string listId, int contactId) => cache.GetCache<ReadContactDto>("L#" + listId + "#CRS#" + contactId, () => 
			GetListDto<ReadContactDto>(CallMethod(EndPoints.List + "/" + listId + "/Recipients/Subscribed?filterby=\"idRecipient==" + contactId.ToString() + "\"", System.Net.Http.HttpMethod.Get)).FirstOrDefault());

		public ReadContactDto ReadUnsubscribedContact(string listId, int contactId) => cache.GetCache<ReadContactDto>("L#" + listId + "#CRU#" + contactId, () => 
			GetListDto<ReadContactDto>(CallMethod(EndPoints.List + "/" + listId + "/Recipients/Unsubscribed?filterby=\"idRecipient==" + contactId.ToString() + "\"", System.Net.Http.HttpMethod.Get)).FirstOrDefault());
		
		public bool UnsubscribeContact(string listId, int contactId)
		{
			CallMethod(EndPoints.List + "/" + listId + "/Unsubscribed/" + contactId, System.Net.Http.HttpMethod.Delete);
			return true;
		}

		public bool SubscribeContact(string listId, int contactId)
		{
			var contact = ReadUnsubscribedContact(listId, contactId);
			return (contact != null) ? ResubscribeContact(listId, contact) : SubscribeContact(listId, contact);
		}

		public bool SubscribeContact(string listId, CreateContactDto contact)
		{
			JsonConvert.DeserializeObject<int>(CallMethod(EndPoints.List + "/" + listId + "/Recipient?ConfirmEmail=true", System.Net.Http.HttpMethod.Post, JsonConvert.SerializeObject(contact)));
			return true;
		}

		private class RecipientItemDto
		{
			public string Email;
		}

		public bool ResubscribeContact(string listId, CreateContactDto contact)
		{
			JsonConvert.DeserializeObject<int>(CallMethod(EndPoints.List + "/" + listId + "/Recipients?importType=asOptin", System.Net.Http.HttpMethod.Post, JsonConvert.SerializeObject(
			new List<RecipientItemDto>
			{
				new RecipientItemDto { Email = contact.Email }
			})));
			return true;
		}

		public ReadContactDto UpdateContact(string listId, ReadContactDto contact) =>
			JsonConvert.DeserializeObject<ReadContactDto>(CallMethod(EndPoints.RecipientDetail, System.Net.Http.HttpMethod.Put, JsonConvert.SerializeObject(contact)));

		public class SendMailDto
		{
			public class FieldDto
			{
				[JsonProperty(PropertyName = "N")]
				public string Name { get;set; }
					
				[JsonProperty(PropertyName = "V")]
				public string Value { get;set; }
			}

			public class HtmlDto
			{
				[JsonProperty(PropertyName = "DocType")]
				public string DocType { get;set; }
				
				[JsonProperty(PropertyName = "Head")]
				public string Head { get;set; }
				
				[JsonProperty(PropertyName = "Body")]
				public string Body { get;set; }

				[JsonProperty(PropertyName = "BodyTag")]
				public string BodyTag { get;set; } = "<body>";
			}

			[JsonProperty(PropertyName = "Html")]
			public HtmlDto Html { get;set; }

			[JsonProperty(PropertyName = "Text")]
			public string Text { get;set; }

			[JsonProperty(PropertyName = "Subject")]
			public string Subject { get;set; }

			public class EmailAddressDto
			{
				[JsonProperty(PropertyName = "Name")]
				public string Name { get;set; }

				[JsonProperty(PropertyName = "Email")]
				public string Email { get;set; }
			}
				
			[JsonProperty(PropertyName = "From")]
			public EmailAddressDto From { get;set; }

			[JsonProperty(PropertyName = "To")]
			public IEnumerable<EmailAddressDto> To { get;set; }

			[JsonProperty(PropertyName = "Cc")]
			public IEnumerable<EmailAddressDto> CarbonCopy { get;set; }

			[JsonProperty(PropertyName = "Bcc")]
			public IEnumerable<EmailAddressDto> BlindCarbonCopy { get;set; }

			[JsonProperty(PropertyName = "ReplyTo")]
			public string ReplyTo { get;set; }

			[JsonProperty(PropertyName = "CharSet")]
			public string CharSet { get;set; } = "utf-8";

			[JsonProperty(PropertyName = "ExtendedHeaders")]
			public IEnumerable<FieldDto> ExtendedHeaders { get;set; }

			public class MessagePartDto
			{
				[JsonProperty(PropertyName = "Filename")]
				public string Filename { get;set; }
				
				[JsonProperty(PropertyName = "ContentId")]
				public string ContentId { get;set; }
				
				[JsonProperty(PropertyName = "Body")]
				public byte[] Body { get;set; }

			}

			[JsonProperty(PropertyName = "Attachments")]
			public IEnumerable<MessagePartDto> Attachments { get;set; }


			[JsonProperty(PropertyName = "EmbeddedImages")]
			public IEnumerable<MessagePartDto> Images { get;set; }

			public class SmtpDto
			{
				[JsonProperty(PropertyName = "CampaignName")]
				public string CampaignName { get;set; }
				
				[JsonProperty(PropertyName = "CampaignCode")]
				public string CampaignCode { get;set; }

				[JsonProperty(PropertyName = "Header")]
				public bool Header { get;set; }

				[JsonProperty(PropertyName = "Footer")]
				public bool Footer { get;set; }

				[JsonProperty(PropertyName = "ClickTracking")]
				public string ClickTracking { get;set; }

				[JsonProperty(PropertyName = "ViewTracking")]
				public string ViewTracking { get;set; }

				[JsonProperty(PropertyName = "Priority")]
				public string Priority { get;set; }

				[JsonProperty(PropertyName = "Schedule")]
				public string Schedule { get;set; }

				[JsonProperty(PropertyName = "DynamicFields")]
				public IEnumerable<FieldDto> Fields { get;set; }

				[JsonProperty(PropertyName = "CampaignReport")]
				public string CampaignReport { get;set; }

				[JsonProperty(PropertyName = "SkipDynamicFields")]
				public string SkipDynamicFields { get;set; }
			}

			[JsonProperty(PropertyName = "XSmtpAPI")]
			public SmtpDto Smtp { get;set; }

			public class CredentialsDto
			{
				[JsonProperty(PropertyName = "Username")]
				public string User { get;set; }
				
				[JsonProperty(PropertyName = "Secret")]
				public string Password { get;set; }
			}

			[JsonProperty(PropertyName = "User")]
			public CredentialsDto Credentials { get;set; }
		}

		public class SendMailResponseDto
		{
			[JsonProperty(PropertyName = "Status")]
			public string Status { get;set; }

			[JsonProperty(PropertyName = "Code")]
			public string Code { get;set; }

			[JsonProperty(PropertyName = "Message")]
			public string Message { get;set; }
		}

		public bool SendEmail(string emailTemplateId, SendMailDto.EmailAddressDto fromEmail, SendMailDto.EmailAddressDto toEmail, IEnumerable<SendMailDto.FieldDto> fields, IEnumerable<SendMailDto.MessagePartDto> attachments = null)
		{
			foreach (var list in EnumerateLists())
			{
				var emailTemplates = GetEmailsForList(list.Id).FirstOrDefault(x => x.Id == emailTemplateId);
				if (emailTemplates == null)
				{
					continue;
				}

				//	Have found the email template
				var listInfo = GetList(list.Id);
				var emailTemplate = GetEmail(list.Id, emailTemplateId);

				if (fromEmail == null)
				{
					fromEmail = new SendMailDto.EmailAddressDto
					{
						Name = listInfo.Name,
						Email = listInfo.FromEmail
					};
				}

				return SendEmail(
					fromEmail, 
					toEmail, 
					emailTemplate.Subject, 
					emailTemplate.Body, 
					fields,
					attachments);
			}

			return false;
		}

		private string ReplaceFields(string text, IEnumerable<SendMailDto.FieldDto> fields)
		{
			var process = new StringBuilder();
			StringBuilder fieldBuilder = null;

			foreach (var ch in text)
			{
				if (ch == '[')
				{
					fieldBuilder = new StringBuilder();
				}
				else if (fieldBuilder != null)
				{
					if (ch == ']')
					{
						var field = fieldBuilder.ToString();
						var match = fields.Where(x => string.Compare(x.Name, field, true) == 0).FirstOrDefault();
						if (match != null)
						{
							process.Append(match.Value);
						}
						else
						{
							process.Append('[');
							process.Append(field);
							process.Append(']');
						}
						fieldBuilder = null;
					}
					else
					{
						fieldBuilder.Append(ch);
					}
				}
				else
				{
					process.Append(ch);
				}
			}
			return process.ToString();
		}

		public bool SendEmail(SendMailDto.EmailAddressDto fromEmail, SendMailDto.EmailAddressDto toEmail, string subject, string body, IEnumerable<SendMailDto.FieldDto> fields, IEnumerable<SendMailDto.MessagePartDto> attachments = null)
		{
			GateKeeper();
			SendMailDto info = new SendMailDto
			{
				Html = new SendMailDto.HtmlDto
				{
					Body = ReplaceFields(body, fields)
				},
				Text = subject,
				Subject = subject,
				From = fromEmail,
				CarbonCopy = Enumerable.Empty<SendMailDto.EmailAddressDto>(),
				BlindCarbonCopy = Enumerable.Empty<SendMailDto.EmailAddressDto>(),
				To = new SendMailDto.EmailAddressDto[] 
				{
					toEmail
				},
				Smtp = new SendMailDto.SmtpDto
				{
					Fields = null
				},
				Credentials = new SendMailDto.CredentialsDto
				{
					User = SmtpUser,
					Password = SmtpPassword
				},
				Attachments = attachments
			};


			SendMailResponseDto response = null;
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(EndPoints.SendEmail);
			request.AllowAutoRedirect = false;
			request.KeepAlive = true;
			request.Method = System.Net.Http.HttpMethod.Post.ToString();
			request.ContentType = GetContentTypestring(ContentType.Json);
			request.Accept = GetContentTypestring(ContentType.Json);
			try
			{
				SetRequest(request, JsonConvert.SerializeObject(info));
				response = JsonConvert.DeserializeObject<SendMailResponseDto>(GetResponse(request));
			}
			catch (WebException wex)
			{
				var errorResponse = (HttpWebResponse)wex.Response;
				if (errorResponse == null)
				{
					//	No connection
					throw new MailUpException(503, "Service Unavailable");
				}

				var mailResponse = JsonConvert.DeserializeObject<SendMailResponseDto>(GetResponse(errorResponse));
				int errorCode = (int) errorResponse.StatusCode;
				int.TryParse(mailResponse.Code, out errorCode);
				throw new MailUpException(errorCode, mailResponse.Message);
			}
			catch (Exception ex)
			{
				throw new MailUpException(0, ex.Message);
			}

			int code;
			if (!int.TryParse(response.Code, out code))
			{
				throw new MailUpException(-1, $"Unknown code {response.Code} returned from SendMail");
			}

			if (code == 0)
			{
				return true;
			}

			throw new MailUpException(code, response.Message);
		}

		public class CreateGroupDto : IItem
		{
			[JsonProperty(PropertyName = "Name")]
			public string Name { get;set; }

			[JsonProperty(PropertyName = "Notes")]
			public string Notes { get;set; }
		}

		public class ReadGroupDto : CreateGroupDto
		{
			[JsonProperty(PropertyName = "Deletable")]
			public bool Deletable { get;set; }

			[JsonProperty(PropertyName = "idGroup")]
			public string Id { get;set; }

			[JsonProperty(PropertyName = "idList")]
			public string ListId { get;set; }
		}

		private List<T> GetListDto<T>(string json) where T : IItem
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				return new List<T>();
			}

			var items = JsonConvert.DeserializeObject<ListDto<T>>(json);
			if (items == null || items.Items == null || !items.Items.Any())
			{
				return new List<T>();
			}

			return items.Items;
		}

		public ReadGroupDto CreateGroup(string listId, CreateGroupDto group) => 
			JsonConvert.DeserializeObject<ReadGroupDto>(CallMethod(EndPoints.List + "/" + listId + "/Group", System.Net.Http.HttpMethod.Post, JsonConvert.SerializeObject(group)));

		public ReadGroupDto UpdateGroup(string listId, string groupId, CreateGroupDto group) => 
			JsonConvert.DeserializeObject<ReadGroupDto>(CallMethod(EndPoints.List + "/" + listId + "/Group/" + groupId, System.Net.Http.HttpMethod.Put, JsonConvert.SerializeObject(group)));

		public IEnumerable<ReadGroupDto> EnumerateGroups(string listId) => cache.GetCache<List<ReadGroupDto>>("L#" + listId + "#GL", () => 
			GetListDto<ReadGroupDto>(CallMethod(EndPoints.List + "/" + listId + "/Groups", System.Net.Http.HttpMethod.Get)));

		public void DeleteGroup(string listId, string groupId) => CallMethod(EndPoints.List + "/" + listId + "/Group/" + groupId, System.Net.Http.HttpMethod.Delete);

		public void AddGroupToContact(string groupId, int contactId) => CallMethod(EndPoints.Group + "/" + groupId + "/Subscribe/" + contactId.ToString() + "?confirmSubscription=false", System.Net.Http.HttpMethod.Post);

		public void RemoveGroupFromContact(string groupId, int contactId) => CallMethod(EndPoints.Group + "/" + groupId + "/Unsubscribe/" + contactId.ToString(), System.Net.Http.HttpMethod.Delete);

		public IEnumerable<ReadContactDto> EnumerateContactsInGroup(string listId, string groupId) => cache.GetCache<List<ReadContactDto>>("L#" + listId + "#GC#" + groupId, () => 
			GetListDto<ReadContactDto>(CallMethod(EndPoints.Group + "/" + groupId + "/Recipients", System.Net.Http.HttpMethod.Get)));

		public ReadContactDto EnumerateContactInGroup(string listId, string groupId, int contactId) => cache.GetCache<ReadContactDto>("L#" + listId + "#GC#" + groupId + "#C" + contactId.ToString(), () => 
			GetListDto<ReadContactDto>(CallMethod(EndPoints.Group + "/" + groupId + "/Recipients?filterby=\"idRecipient==" + contactId.ToString() + "\"", System.Net.Http.HttpMethod.Get)).FirstOrDefault());

		public IEnumerable<ReadGroupDto> EnumerateContactGroups(string listId, int contactId)
		{
			var found = new List<ReadGroupDto>();
			var groups = EnumerateGroups(listId);
			if (groups != null)
			{
				foreach (var group in groups)
				{
					if (IsContactInGroup(listId, group.Id, contactId))
					{
						found.Add(group);
					}

				}
			}
			return found;
		}

		public bool IsContactInGroup(string listId, string groupId, int contactId) => EnumerateContactInGroup(listId, groupId, contactId) != null;

		public IEnumerable<ContactFieldDto> EnumerateDynamicFields() => cache.GetCache<List<ContactFieldDto>>("D", () => 
			GetListDto<ContactFieldDto>(CallMethod(EndPoints.Console + "/Recipient/DynamicFields?pageSize=100", System.Net.Http.HttpMethod.Get)));

	}
}
