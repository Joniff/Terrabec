using System;
using System.Linq;
using Newtonsoft.Json;

namespace Terrabec.Json
{
	public static class JsonHelper
	{
		public static string PropertyName<T>(string name)
		{
			var attr = typeof(T).GetProperty(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).
				GetCustomAttributes(typeof(JsonPropertyAttribute), true).First() as JsonPropertyAttribute;
			if (attr != null && !String.IsNullOrWhiteSpace(attr.PropertyName))
			{
				return attr.PropertyName;
			}
			return name.ToLowerInvariant();
		}
	}
}
