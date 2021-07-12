using System;
using Newtonsoft.Json;
using Terrabec.Modules;

namespace Terrabec.Connectors.ReadEmailTemplate
{
	public interface IReadEmailTemplateResult : EnumerateEmailTemplates.IEnumerateEmailTemplatesResult, IFeatureResult
	{
		[JsonProperty("Created")]
		DateTime? Created { get; }

		[JsonProperty("LastBroadcast")]
		DateTime? LastBroadcast { get; }

		[JsonProperty("subject")]
		string Subject { get; }

		[JsonProperty("content")]
		string Content { get; }
	}
}
