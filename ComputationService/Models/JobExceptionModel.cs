using Newtonsoft.Json;
using System;

namespace ComputationService.Models
{
	public sealed class JobExceptionModel
	{
		public JobExceptionModel() { }

		public JobExceptionModel(Exception ex)
		{
			if(ex == null) { return; }

			Type = ex.GetType().Name;
			StackTrace = ex.StackTrace;
			Source = ex.Source;
			Message = ex.Message;
			InnerJobException =
				ex.InnerException != null ? new JobExceptionModel(ex.InnerException) : null;
		}

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("stackTrace")]
		public string StackTrace { get; set; }

		[JsonProperty("source")]
		public string Source { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("innerJobException",
			NullValueHandling = NullValueHandling.Ignore)]
		public JobExceptionModel InnerJobException { get; set; }
	}
}
