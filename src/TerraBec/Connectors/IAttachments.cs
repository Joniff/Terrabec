using System.Collections.Generic;

namespace Terrabec.Connectors
{
	public interface IAttachments : IEnumerable<IAttachment>
	{
		void Attach(params string[] filepaths);
	}
}
