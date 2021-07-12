using System.Collections.Generic;
using Newtonsoft.Json;

namespace Terrabec.Connectors.ReadContact
{
	public class ReadContactResult : IReadContactResult
	{
		[JsonProperty("id")]
		public string ContactId { get; set; }

		[JsonProperty("enabled")]
		public bool Enabled { get; set; }

		[JsonProperty("email")]
		public string Email { get; set; }
		
		[JsonProperty("fields")]
		public IContactFields Fields { get; set; }
	}
}
