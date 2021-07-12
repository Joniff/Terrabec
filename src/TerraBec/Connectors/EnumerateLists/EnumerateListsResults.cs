using System.Collections.Generic;
using Terrabec.Modules;

namespace Terrabec.Connectors.EnumerateLists
{
	public class EnumerateListsResults : List<EnumerateListsResult>, IEnumerateListsResults
	{
		IEnumerator<IEnumerateListsResult> IEnumerable<IEnumerateListsResult>.GetEnumerator() => this.GetEnumerator();
		IEnumerator<IFeatureResult> IEnumerable<IFeatureResult>.GetEnumerator() => this.GetEnumerator();
	}
}
