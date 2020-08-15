using ComputationService.Models;
using ComputationService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ComputationService.Controllers
{
	[ApiController, Route("api/[controller]")]
	public class ComputationController : ControllerBase
	{
		private readonly IQueuedBackgroundService _queuedBackgroundService;
		private readonly IComputationJobStatusService _computationJobStatusService;

		public ComputationController(
			IQueuedBackgroundService queuedBackgroundService,
			IComputationJobStatusService computationJobStatusService)
		{
			_queuedBackgroundService = queuedBackgroundService;
			_computationJobStatusService = computationJobStatusService;
		}

		[HttpPost, Route("beginComputation")]
		[ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(JobCreatedModel))]
		public async Task<IActionResult> BeginComputation([FromBody] JobParametersModel obj)
		{
			return Accepted(
				await _queuedBackgroundService.PostWorkItemAsync(obj).ConfigureAwait(false));
		}

		[HttpGet, Route("computationStatus/{jobId}")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JobModel))]
		[ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
		public async Task<IActionResult> GetComputationResultAsync(string jobId)
		{
			var job = await _computationJobStatusService.GetJobAsync(jobId).ConfigureAwait(false);
			if(job != null)
			{
				return Ok(job);
			}
			return NotFound($"Job with ID `{jobId}` not found");
		}

		[HttpGet, Route("getAllJobs")]
		[ProducesResponseType(StatusCodes.Status200OK,
			Type = typeof(IReadOnlyDictionary<string, JobModel>))]
		public async Task<IActionResult> GetAllJobsAsync()
		{
			return Ok(await _computationJobStatusService.GetAllJobsAsync().ConfigureAwait(false));
		}

		[HttpDelete, Route("clearAllJobs")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> ClearAllJobsAsync([FromQuery] string permission)
		{
			if(permission == "this is flakey security so this can be run as a public demo")
			{
				await _computationJobStatusService.ClearAllJobsAsync().ConfigureAwait(false);
				return Ok();
			}
			return Unauthorized();
		}
	}
}
