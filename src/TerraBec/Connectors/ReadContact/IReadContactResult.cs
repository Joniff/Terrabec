using System.ComponentModel;
using Newtonsoft.Json;
using Terrabec.Modules;

namespace Terrabec.Connectors.ReadContact
{
	public interface IReadContactResult : IFeatureResult
	{
		[Description("Unique identifier for this contact")]
		[JsonProperty("id")]
		string ContactId { get; }

		[Description("Whether this contact can be sent emails, true = subscribed, false = unsubscribed")]
		[JsonProperty("enabled")]
		bool Enabled { get; }

		[Description("Unique and valid email for this contact")]
		[JsonProperty("email")]
		string Email { get; }
		
		[Description("Various values attached to this contact, can be mapped when sending emails")]
		[JsonProperty("fields")]
		IContactFields Fields { get; }
	}
}
