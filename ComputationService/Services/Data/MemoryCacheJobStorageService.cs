using ComputationService.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace ComputationService.Services.Data
{
	public sealed class MemoryCacheJobStorageService : IJobStorageService
	{
		private static readonly ConcurrentDictionary<string, JobModel> _jobs = new ConcurrentDictionary<string, JobModel>();

		private static string Key(string databaseKey, string jobId) => $"{databaseKey}:{jobId}";

		public Task ClearAllAsync(string databaseKey)
		{
			var keys = _jobs.Where(x => x.Key.StartsWith(databaseKey)).Select(x => x.Key);
			foreach(var k in keys)
			{
				_jobs.Remove(k, out _);
			}
			return Task.CompletedTask;
		}

		public Task<bool> ExistsAsync(string databaseKey, string jobId)
		{
			return Task.FromResult(_jobs.ContainsKey(Key(databaseKey, jobId)));
		}

		public Task<IReadOnlyDictionary<string, JobModel>> GetAllAsync(string databaseKey)
		{
			var d = _jobs.Where(
				x => x.Key.StartsWith(databaseKey)).ToDictionary(
					y => y.Value.JobId, y => y.Value).ToImmutableDictionary();

			return Task.FromResult<IReadOnlyDictionary<string, JobModel>>(d);
		}

		public Task<JobModel> ReadAsync(string databaseKey, string jobId)
		{
			var key = Key(databaseKey, jobId);
			return _jobs.ContainsKey(key) ? Task.FromResult(_jobs[key]) : null;
		}

		public Task WriteAsync(string databaseKey, JobModel job)
		{
			_jobs[Key(databaseKey, job.JobId)] = job;
			return Task.CompletedTask;
		}
	}
}
