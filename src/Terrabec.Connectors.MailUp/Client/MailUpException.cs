using System;

namespace Terrabec.Connectors.MailUp.Client
{
	internal class MailUpException : Exception
	{
		public int StatusCode { get; set; }

		public MailUpException(int statusCode, string message) : base(message)
		{
			StatusCode = statusCode;
		}
	}
}
