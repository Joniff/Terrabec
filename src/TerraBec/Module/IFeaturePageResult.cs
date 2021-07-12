using System.Collections.Generic;
using Newtonsoft.Json;

namespace Terrabec.Modules
{
	public interface IFeaturePageResult
	{
		[JsonProperty("page")]
		int Page { get; }

		[JsonProperty("pageSize")]
		int PageSize { get; }	

		[JsonProperty("results")]
		IFeatureResults Results { get; }	
	}
}
