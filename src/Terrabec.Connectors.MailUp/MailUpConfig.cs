using Terrabec.Module;

namespace Terrabec.Connectors.MailUp
{
	public class MailUpConfig : BaseModuleConfig
	{
		public string ClientId { get; set; }

		public string ClientSecret { get; set; }

		public string Username { get; set; }

		public string Password { get; set; }

		public int SessionTimeout { get; set; } = 10;

		public string DefaultReplyAddress { get; set; }

		public string DefaultCompanyName { get; set; }

		public string DefaultContactName { get; set; }

		public string DefaultAddress { get; set; }

		public string DefaultCity { get; set; }

		public string DefaultCountry { get; set; }

		public string DefaultPermissionReminder { get; set; }

		public string DefaultWebsiteUrl { get; set; }

		public string SmtpUser { get; set; }

		public string SmtpPassword { get; set; }

	}
}
