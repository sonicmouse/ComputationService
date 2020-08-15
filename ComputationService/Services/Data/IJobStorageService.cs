using ComputationService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ComputationService.Services.Data
{
	public interface IJobStorageService
	{
		Task<JobModel> ReadAsync(string databaseKey, string jobId);
		Task WriteAsync(string databaseKey, JobModel job);
		Task ClearAllAsync(string databaseKey);
		Task<bool> ExistsAsync(string databaseKey, string jobId);
		Task<IReadOnlyDictionary<string, JobModel>> GetAllAsync(string databaseKey);
	}
}
