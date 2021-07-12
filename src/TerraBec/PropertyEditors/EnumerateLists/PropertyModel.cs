using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terrabec.Connectors;
using Terrabec.Connectors.CreateContact;
using Terrabec.Connectors.DeleteContact;
using Terrabec.Connectors.EnumerateContactFields;
using Terrabec.Connectors.FindContact;
using Terrabec.Connectors.ReadContact;
using Terrabec.Connectors.ReadList;
using Terrabec.Connectors.UpdateContact;
using Terrabec.Module;
using uSync.MemberEdition.Security;

namespace Terrabec.PropertyEditors.EnumerateLists
{
	[JsonObject(MemberSerialization.OptIn)]
	[JsonConverter(typeof(PropertyModelJsonConvertor))]
	[DebuggerDisplay("{connectorId + \".\" + listId}")]
	public class PropertyModel
	{
		private static Cryptography cryptography = new Cryptography(nameof(EnumerateLists));

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
		
		[JsonProperty(PropertyName = "list")]
		internal string listId { get; set; }

		private IReadListResult list;
		public IReadListResult List 
		{ 
			get
			{
				if (list != null)
				{
					return list;
				}
				if (string.IsNullOrWhiteSpace(listId) || Connector == null || (Connector as IReadListFeature) == null)
				{
					return null;
				}
				return (list = (Connector as IReadListFeature).ExecuteReadListFeature(listId));
			}
			set
			{
				if (value == null)
				{
					list = null;
					listId = null;
				}
				else
				{
					list = value;
					listId = list.Id;
				}
			}
		}

		public PropertyModel()
		{
		}

		public PropertyModel(PropertyModel other)
		{
			Connector = other.Connector;
			List = other.List;
		}

		public PropertyModel(string json) : this(string.IsNullOrWhiteSpace(json) ? null : JsonConvert.DeserializeObject<PropertyModel>(json[0] == '{' ? json : cryptography.Decrypt(json)))
		{ 
		}

		public PropertyModel(JObject data) : this(data.ToObject<PropertyModel>())
		{
		}

		public bool IsValid() => Connector != null && List != null;

		public IFindContactResult FindContact(string email) => (IsValid() && (Connector as IFindContactFeature) != null) ? (Connector as IFindContactFeature).ExecuteFindContactFeature(listId, email) : null;

		public ICreateContactResult CreateContact(bool enabled, string email, IContactFields fields) => 
			(IsValid() && (Connector as ICreateContactFeature) != null) ? (Connector as ICreateContactFeature).ExecuteCreateContactFeature(listId, enabled, email, fields) : null;
		public IReadContactResult ReadContact(string contactId) => (IsValid() && (Connector as IReadContactFeature) != null) ? (Connector as IReadContactFeature).ExecuteReadContactFeature(listId, contactId) : null;
		public IUpdateContactResult UpdateContact(string contactId, bool enabled, string email, IContactFields fields) => 
			(IsValid() && (Connector as IUpdateContactFeature) != null) ? (Connector as IUpdateContactFeature).ExecuteUpdateContactFeature(listId, contactId, enabled, email, fields) : null;

		public bool DeleteContact(string contactId) => (IsValid() && (Connector as IDeleteContactFeature) != null) ? (Connector as IDeleteContactFeature).ExecuteDeleteContactFeature(listId, contactId).Success : false;

		public IEnumerateContactFieldsResults EnumerateContactFields() => (IsValid() && (Connector as IEnumerateContactFieldsFeature) != null) ? (Connector as IEnumerateContactFieldsFeature).ExecuteEnumerateContactFieldsFeature(listId) : null;

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
