using Newtonsoft.Json;

namespace Terrabec.Connectors
{
	public interface IAttachment
	{
		[JsonProperty("filename")]
		string Filename { get; set; }

		[JsonProperty("content")]
		byte[] Content { get; set; }

		void Attach(string filepath);
	}
}
