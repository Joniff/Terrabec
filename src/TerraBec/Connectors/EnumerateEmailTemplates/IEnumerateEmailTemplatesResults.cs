using System.Collections.Generic;
using Terrabec.Modules;

namespace Terrabec.Connectors.EnumerateEmailTemplates
{
	public interface IEnumerateEmailTemplatesResults : IEnumerable<IEnumerateEmailTemplatesResult>, IFeatureResults
	{
	}	
}
