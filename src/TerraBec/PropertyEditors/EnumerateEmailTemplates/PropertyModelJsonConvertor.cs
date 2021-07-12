using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terrabec.Json;
using Umbraco.Core;

namespace Terrabec.PropertyEditors.EnumerateEmailTemplates
{
	public class PropertyModelJsonConvertor : JsonConverter
	{
		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(PropertyModel).IsAssignableFrom(objectType);
		}

		private IEnumerable<string> GetPreValuesByDataTypeId(Guid guid)
		{
			var dataService = ApplicationContext.Current.Services.DataTypeService;
			var guidMethod = dataService.GetType().GetMethod(
				nameof(dataService.GetPreValuesByDataTypeId), 
				new Type[] {typeof(Guid) });
			if (guidMethod == null)
			{
				return null;
			}
			return (guidMethod.Invoke(dataService, new object[] { guid }) as IEnumerable<string>);
		}

		private JObject Definition(JObject obj)
		{
			var token = obj.GetValue("datatypeId", StringComparison.InvariantCultureIgnoreCase);
			if (token == null)
			{
				return null;
			}

			int id = 0;
			Guid guid = Guid.Empty;
			switch (token.Type)
			{
				case JTokenType.Guid:
					guid = token.Value<Guid>();
					break;

				case JTokenType.Integer:
					id = token.Value<int>();
					break;

				case JTokenType.String:
					var text = token.Value<string>();
					if (!int.TryParse(text, out id))
					{
						Guid.TryParse(text, out guid);
					}
					break;
			}

			string json = null;
			if (id != 0)
			{
				json = ApplicationContext.Current.Services.DataTypeService.GetPreValuesByDataTypeId(token.Value<int>()).FirstOrDefault();
			}
			else if (guid != Guid.Empty)
			{
				json = GetPreValuesByDataTypeId(token.Value<Guid>()).FirstOrDefault();
			}
			else
			{
				return null;
			}
			return JObject.Parse(json);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var model = new PropertyModel();

			JObject item = JObject.Load(reader);
			model.connectorId = item.GetValue(JsonHelper.PropertyName<PropertyModel>(nameof(PropertyModel.connectorId)), StringComparison.InvariantCultureIgnoreCase)?.Value<string>();
			model.emailTemplateId = item.GetValue(JsonHelper.PropertyName<PropertyModel>(nameof(PropertyModel.emailTemplateId)), StringComparison.InvariantCultureIgnoreCase)?.Value<string>();

			return model;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var model = value as PropertyModel;
			writer.WriteStartObject();

			writer.WritePropertyName(JsonHelper.PropertyName<PropertyModel>(nameof(PropertyModel.connectorId)));
			writer.WriteValue(model.connectorId);

			writer.WritePropertyName(JsonHelper.PropertyName<PropertyModel>(nameof(PropertyModel.emailTemplateId)));
			writer.WriteValue(model.emailTemplateId);

			writer.WriteEndObject();
		}
	}
}
