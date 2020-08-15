using Newtonsoft.Json;

namespace ComputationService.Models
{
	public sealed class JobResultModel
	{
		[JsonProperty("calculatedResult",
			NullValueHandling = NullValueHandling.Ignore)]
		public uint? CalculatedResult { get; set; }

		[JsonProperty("exception",
			NullValueHandling = NullValueHandling.Ignore)]
		public JobExceptionModel Exception { get; set; }
	}
}
