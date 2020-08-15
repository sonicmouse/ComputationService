using Newtonsoft.Json;

namespace ComputationService.Models
{
	public sealed class JobParametersModel
	{
		[JsonProperty("iterations")]
		public uint Iterations { get; set; }

		[JsonProperty("seedData")]
		public uint SeedData { get; set; }

		// TODO: Add any other parameters for work
	}
}
