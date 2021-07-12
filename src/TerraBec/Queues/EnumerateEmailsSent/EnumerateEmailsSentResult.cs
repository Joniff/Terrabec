using System;
using Newtonsoft.Json;

namespace Terrabec.Queues.EnumerateEmailsSent
{
	public class EnumerateEmailsSentResult : IEnumerateEmailsSentResult
	{
		[JsonProperty("submitted")]
		public DateTime Submitted { get; set; }

		[JsonProperty("completed")]
		public DateTime? Completed { get; set; }

		[JsonProperty("connectorId")]
		public string ConnectorId { get; set; }

		[JsonProperty("emailTemplateId")]
		public string EmailTemplateId { get; set; }
		
		[JsonProperty("fromName")]
		public string FromName { get; set; }

		[JsonProperty("fromEmail")]
		public string FromEmail { get; set; }

		[JsonProperty("toName")]
		public string ToName { get; set; }

		[JsonProperty("toEmail")]
		public string ToEmail { get; set; }
	}
}
