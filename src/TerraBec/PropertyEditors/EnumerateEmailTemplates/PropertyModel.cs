using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terrabec.Connectors;
using Terrabec.Connectors.ReadEmailTemplate;
using Terrabec.Connectors.SendEmail;
using Terrabec.Connectors.SendEmailFrom;
using Terrabec.Connectors.SendEmailFromWithAttachments;
using Terrabec.Connectors.SendEmailWithAttachments;
using Terrabec.Module;
using uSync.MemberEdition.Security;

namespace Terrabec.PropertyEditors.EnumerateEmailTemplates
{
	[JsonObject(MemberSerialization.OptIn)]
	[JsonConverter(typeof(PropertyModelJsonConvertor))]
	[DebuggerDisplay("{connectorId + \".\" + emailTemplateId}")]
	public class PropertyModel
	{
		private static Cryptography cryptography = new Cryptography(nameof(EnumerateEmailTemplates));

		[JsonProperty(PropertyName = "connector")]
		internal string connectorId { get; set; }

		private IModule connector;
		public IModule Connector 
		{ 
			get
			{
				if (connector != null)
				{
					return connector;
				}
				if (string.IsNullOrWhiteSpace(connectorId) || !BaseConnector<IModuleConfig>.Register.ContainsKey(connectorId))
				{
					return null;
				}
				connector = BaseConnector<IModuleConfig>.Create(connectorId);
				connector.Init();
				return connector;
			}
			set
			{
				if (value == null)
				{
					connectorId = null;
					connector = null;
				}
				else
				{
					connector = value;
					connectorId = connector.Id;
				}
			}
		}
		
		[JsonProperty(PropertyName = "emailTemplate")]
		internal string emailTemplateId { get; set; }

		private IReadEmailTemplateResult emailTemplate;
		public IReadEmailTemplateResult EmailTemplate
		{ 
			get
			{
				if (emailTemplate != null)
				{
					return emailTemplate;
				}
				if (string.IsNullOrWhiteSpace(emailTemplateId) || Connector == null || (Connector as IReadEmailTemplateFeature) == null)
				{
					return null;
				}
				return (emailTemplate = (Connector as IReadEmailTemplateFeature).ExecuteReadEmailTemplateFeature(emailTemplateId));
			}
			set
			{
				if (value == null)
				{
					emailTemplate = null;
					emailTemplateId = null;
				}
				else
				{
					emailTemplate = value;
					emailTemplateId = emailTemplate.Id;
				}
			}
		}

		public PropertyModel()
		{
		}

		public PropertyModel(PropertyModel other)
		{
			if (other != null)
			{
				Connector = other.Connector;
				emailTemplateId = other.emailTemplateId;
			}
		}

		public PropertyModel(string json) : this((PropertyModel) (string.IsNullOrWhiteSpace(json) ? null : JsonConvert.DeserializeObject<PropertyModel>(json[0] == '{' ? json : cryptography.Decrypt(json))))
		{ 
		}

		public PropertyModel(JObject data) : this(data.ToObject<PropertyModel>())
		{
		}

		public bool IsValid() => Connector != null && EmailTemplate != null;

		public bool SendEmail(string toAddress, IContactFields fields) => 
			(IsValid() && (Connector as ISendEmailFeature) != null) ? (Connector as ISendEmailFeature).ExecuteSendEmailFeature(emailTemplateId, toAddress, fields).Success : false;

		public bool SendEmail(string fromName, string fromAddress, string toName, string toAddress, IContactFields fields) => 
			(IsValid() && (Connector as ISendEmailFromFeature) != null) ? (Connector as ISendEmailFromFeature).ExecuteSendEmailFromFeature(emailTemplateId, fromName, fromAddress, toName, toAddress, fields).Success : false;

		public bool SendEmail(string toAddress, IContactFields fields, IAttachments attachments) => 
			(IsValid() && (Connector as ISendEmailWithAttachmentsFeature) != null) ? (Connector as ISendEmailWithAttachmentsFeature).ExecuteSendEmailWithAttachmentsFeature(emailTemplateId, toAddress, fields, attachments).Success : false;

		public bool SendEmail(string fromName, string fromAddress, string toName, string toAddress, IContactFields fields, IAttachments attachments) => 
			(IsValid() && (Connector as ISendEmailFromWithAttachmentsFeature) != null) ? (Connector as ISendEmailFromWithAttachmentsFeature).ExecuteSendEmailFromWithAttachmentsFeature(emailTemplateId, fromName, fromAddress, toName, toAddress, fields, attachments).Success : false;

		public override string ToString()
		{
			return cryptography.Encrypt(JsonConvert.SerializeObject(this));
		}

		public static implicit operator string(PropertyModel model)
		{
			return cryptography.Encrypt(JsonConvert.SerializeObject(model));
		}

		public static implicit operator PropertyModel(string text)
		{
			return string.IsNullOrWhiteSpace(text) ? new PropertyModel() : new PropertyModel(text[0] == '{' ? text : cryptography.Decrypt(text));
		}
	}
}
