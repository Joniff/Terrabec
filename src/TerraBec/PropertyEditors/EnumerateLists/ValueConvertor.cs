using System;
using Newtonsoft.Json.Linq;
using Terrabec.Json;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Terrabec.PropertyEditors.EnumerateLists
{
	[PropertyValueType(typeof(PropertyModel))]
	[PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
	public class ValueConverter : IPropertyValueConverter
	{
		public bool IsConverter(PublishedPropertyType propertyType)
		{
			return propertyType.PropertyEditorAlias == TerrabecEnumerateListsPropertyEditor.PropertyEditorAlias;
		}

		private void MergeJson(JObject data, JObject config, string fieldName) => 
			data.Merge(new JObject(new JProperty(JsonHelper.PropertyName<PropertyModel>(fieldName), config.GetValue(JsonHelper.PropertyName<PropertyModel>(fieldName), StringComparison.InvariantCultureIgnoreCase))));

		public object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
		{
			return new PropertyModel(JObject.Parse((string) source));
		}

		public object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
		{
			return source;
		}

		public object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview)
		{
			return null;
		}

	}
}
