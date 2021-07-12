using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terrabec.Json;

namespace Terrabec.Connectors
{
	public class AttachmentJsonConvertor : JsonConverter
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
			return typeof(Attachment).IsAssignableFrom(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var attachment = new Attachment();

			JObject item = JObject.Load(reader);
			attachment.Filename = item.GetValue(JsonHelper.PropertyName<Attachment>(nameof(Attachment.Filename)), StringComparison.InvariantCultureIgnoreCase)?.Value<string>();
			attachment.Content = Convert.FromBase64String(item.GetValue(JsonHelper.PropertyName<Attachment>(nameof(Attachment.Content)), StringComparison.InvariantCultureIgnoreCase)?.Value<string>());

			return attachment;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var attachment = value as Attachment;
			writer.WriteStartObject();

			writer.WritePropertyName(JsonHelper.PropertyName<Attachment>(nameof(Attachment.Filename)));
			writer.WriteValue(attachment.Filename);

			writer.WritePropertyName(JsonHelper.PropertyName<Attachment>(nameof(Attachment.Content)));
			writer.WriteValue(Convert.ToBase64String(attachment.Content));

			writer.WriteEndObject();
		}
	}
}
