using ComputationService.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ComputationService.Services
{
	public interface IComputationWorkService
	{
		Task<JobResultModel> DoWorkAsync(string JobId, JobParametersModel work,
			CancellationToken cancellationToken);
	}

	public sealed class ComputationWorkService : IComputationWorkService
	{
		private readonly IComputationJobStatusService _computationJobStatus;
		private readonly IHttpClientFactory _httpClientFactory;

		public ComputationWorkService(
			IComputationJobStatusService computationJobStatus,
			IHttpClientFactory httpClientFactory)
		{
			_computationJobStatus = computationJobStatus;
			_httpClientFactory = httpClientFactory;
		}

		/// <summary>
		/// Do some work. This is where the meat of the processing is done
		/// </summary>
		public async Task<JobResultModel> DoWorkAsync(string jobId, JobParametersModel work,
			CancellationToken cancellationToken)
		{
			// here's an HttpClient if you need one
			var httpClient = _httpClientFactory.CreateClient();

			var next = work.SeedData;
			var result = new JobResultModel();

			var sw = new Stopwatch();
			sw.Start();
			for (ulong i = 0; i < work.Iterations; ++i)
			{
				next = unchecked(next * 1103515245 + 12345);
				result.CalculatedResult = next / 65536 % 32768;

				await Task.Delay(1000,
					cancellationToken).ConfigureAwait(false); // simulate long-running task.

				// make sure we only update status once a second
				if(sw.ElapsedMilliseconds >= 1000)
				{
					sw.Restart();
					await _computationJobStatus.UpdateJobProgressInformationAsync(
						jobId, $"Current result: {result.CalculatedResult}",
						i / (double)work.Iterations).ConfigureAwait(false);
				}
			}

			await _computationJobStatus.UpdateJobProgressInformationAsync(
				jobId, $"Done", 1.0).ConfigureAwait(false);

			return result;
		}
	}
}
