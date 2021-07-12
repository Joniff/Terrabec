using System.Collections.Generic;
using Terrabec.Modules;

namespace Terrabec.Queues.EnumerateEmailsSent
{
	public class EnumerateEmailsSentResults : List<EnumerateEmailsSentResult>, IEnumerateEmailsSentResults
	{
		IEnumerator<IEnumerateEmailsSentResult> IEnumerable<IEnumerateEmailsSentResult>.GetEnumerator() => this.GetEnumerator();
		IEnumerator<IFeatureResult> IEnumerable<IFeatureResult>.GetEnumerator() => this.GetEnumerator();
	}
}
