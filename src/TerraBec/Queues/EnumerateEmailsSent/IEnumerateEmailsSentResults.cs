using System.Collections.Generic;
using Terrabec.Modules;

namespace Terrabec.Queues.EnumerateEmailsSent
{
	public interface IEnumerateEmailsSentResults : IEnumerable<IEnumerateEmailsSentResult>, IFeatureResults
	{
	}
}
