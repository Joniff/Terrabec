using System;
using Newtonsoft.Json;

namespace Terrabec.Connectors.ReadEmailTemplate
{
	public class ReadEmailTemplateResult : EnumerateEmailTemplates.EnumerateEmailTemplatesResult, IReadEmailTemplateResult
	{
		[JsonProperty("Created")]
		public DateTime? Created { get; set; }

		[JsonProperty("LastBroadcast")]
		public DateTime? LastBroadcast { get; set; }

		[JsonProperty("subject")]
		public string Subject { get; set; }

		[JsonProperty("content")]
		public string Content { get; set; }
	}
}
