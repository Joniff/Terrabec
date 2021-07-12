using System;
using Newtonsoft.Json;
using Terrabec.Modules;

namespace Terrabec.Queues.EnumerateEmailsSent
{
	public interface IEnumerateEmailsSentResult : IFeatureResult
	{
		[JsonProperty("submitted")]
		DateTime Submitted { get; }

		[JsonProperty("completed")]
		DateTime? Completed { get; }

		[JsonProperty("connectorId")]
		string ConnectorId { get; }

		[JsonProperty("emailTemplateId")]
		string EmailTemplateId { get; }
		
		[JsonProperty("fromName")]
		string FromName { get; }

		[JsonProperty("fromEmail")]
		string FromEmail { get; }

		[JsonProperty("toName")]
		string ToName { get; }

		[JsonProperty("toEmail")]
		string ToEmail { get; }
	}
}
