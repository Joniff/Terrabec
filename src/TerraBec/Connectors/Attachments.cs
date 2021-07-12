using System.Collections.Generic;

namespace Terrabec.Connectors
{
	public class Attachments : List<Attachment>, IAttachments
	{
		IEnumerator<IAttachment> IEnumerable<IAttachment>.GetEnumerator() => this.GetEnumerator();

		public void Attach(params string[] filepaths)
		{
			foreach (var filepath in filepaths)
			{
				this.Add(new Attachment(filepath));
			}
		}

		public Attachments()
		{
		}

		public Attachments(params string[] filepaths)
		{
			Attach(filepaths);
		}
	}
}
