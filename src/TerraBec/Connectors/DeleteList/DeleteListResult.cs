using Newtonsoft.Json;

namespace Terrabec.Connectors.DeleteList
{
	public class DeleteListResult : IDeleteListResult
	{
		[JsonProperty("success")]
		public bool Success { get; set; }
	}
}
