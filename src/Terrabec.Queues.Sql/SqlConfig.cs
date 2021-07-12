using Terrabec.Module;

namespace Terrabec.Queues.Sql
{
	public class SqlConfig : BaseModuleConfig
	{
		public string DataSourceName { get; set; }

		public string DatabaseTable { get; set; }

		public int Retry { get; set; }

	}
}
