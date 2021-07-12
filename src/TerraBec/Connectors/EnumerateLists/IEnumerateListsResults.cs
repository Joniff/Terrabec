using System.Collections.Generic;
using Terrabec.Modules;

namespace Terrabec.Connectors.EnumerateLists
{
	public interface IEnumerateListsResults : IEnumerable<IEnumerateListsResult>, IFeatureResults
	{
	}	
}
