using ComputationService.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace ComputationService.Services.Data
{
	public sealed class RedisCacheJobStorageService : IJobStorageService
	{
		private readonly IDatabaseAsync _redis;

		public RedisCacheJobStorageService(IConnectionMultiplexer redis)
		{
			_redis = redis.GetDatabase();
		}

		public async Task ClearAllAsync(string databaseKey)
		{
			var keys = await _redis.HashKeysAsync(databaseKey).ConfigureAwait(false);
			await Task.WhenAll(keys.Select(x =>
				_redis.HashDeleteAsync(databaseKey, x))).ConfigureAwait(false);
		}

		public async Task WriteAsync(string databaseKey, JobModel job)
		{
			var data = await Task.Run(() =>
				JsonConvert.SerializeObject(job, Formatting.None)).ConfigureAwait(false);
			await _redis.HashSetAsync(databaseKey, job.JobId, data).ConfigureAwait(false);
		}

		public async Task<JobModel> ReadAsync(string databaseKey, string jobId)
		{
			var data = await _redis.HashGetAsync(databaseKey, jobId).ConfigureAwait(false);
			return await Task.Run(() =>
				JsonConvert.DeserializeObject<JobModel>(data)).ConfigureAwait(false);
		}

		public Task<bool> ExistsAsync(string databaseKey, string jobId)
		{
			return _redis.HashExistsAsync(databaseKey, jobId);
		}

		public async Task<IReadOnlyDictionary<string, JobModel>> GetAllAsync(string databaseKey)
		{
			var allKeys = await _redis.HashGetAllAsync(databaseKey).ConfigureAwait(false);
			return (await Task.WhenAll(
				allKeys.Select(x =>
					Task.Run(() =>
						JsonConvert.DeserializeObject<JobModel>(x.Value.ToString())))).
							ConfigureAwait(false)).ToImmutableDictionary(y => y.JobId, y => y);
		}
	}
}
