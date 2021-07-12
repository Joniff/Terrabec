using System.Collections.Generic;

namespace Terrabec.Modules.Info
{
	public class InfoResults : List<InfoResult>, IInfoResults
	{
		IEnumerator<IFeatureResult> IEnumerable<IFeatureResult>.GetEnumerator() => this.GetEnumerator();
	}
}
