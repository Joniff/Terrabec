using Terrabec.Connectors;
using Terrabec.Modules;

namespace Terrabec.Queues.Refresh
{
	public interface IRefreshFeature : IFeature
	{
		IRefreshResult ExecuteRefreshFeature();
	}
}
