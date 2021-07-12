using Newtonsoft.Json;
using Terrabec.Modules;

namespace Terrabec.Connectors.ReadContacts
{
	public class ReadContactsPageResult : IReadContactsPageResult
	{
		[JsonProperty("page")]
		public int Page { get; set; }

		[JsonProperty("pageSize")]
		public int PageSize { get; set; }

		[JsonProperty("results")]
		public IFeatureResults Results { get; set; }
	}
}
