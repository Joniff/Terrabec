using System.Reflection;
using System.Web;
using Newtonsoft.Json;

namespace Terrabec.Connectors
{
	[JsonConverter(typeof(AttachmentJsonConvertor))]
	public class Attachment : IAttachment
	{
		[JsonProperty("filename")]
		public string Filename { get; set; }

		[JsonProperty("content")]
		public byte[] Content { get; set; }

		public void Attach(string filepath)
		{
			Filename = System.IO.Path.GetFileName(filepath);
			Content = System.IO.File.ReadAllBytes(System.IO.Path.IsPathRooted(filepath) ? 
				filepath : 
				((HttpContext.Current != null) ? HttpContext.Current.Server.MapPath(filepath) :
				System.IO.Path.Combine(Assembly.GetExecutingAssembly().Location, filepath)));
		}

		public Attachment()
		{
		}

		public Attachment(string filepath)
		{
			Attach(filepath);
		}
	}
}
