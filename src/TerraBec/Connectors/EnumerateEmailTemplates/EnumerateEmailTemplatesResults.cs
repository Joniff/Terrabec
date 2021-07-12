using System.Collections.Generic;
using Terrabec.Modules;

namespace Terrabec.Connectors.EnumerateEmailTemplates
{
	public class EnumerateEmailTemplatesResults : List<EnumerateEmailTemplatesResult>, IEnumerateEmailTemplatesResults
	{
		IEnumerator<IEnumerateEmailTemplatesResult> IEnumerable<IEnumerateEmailTemplatesResult>.GetEnumerator() => this.GetEnumerator();
		IEnumerator<IFeatureResult> IEnumerable<IFeatureResult>.GetEnumerator() => this.GetEnumerator();
	}
}
