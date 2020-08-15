using ComputationService.Services;
using ComputationService.Services.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace ComputationService
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			var useRedisCache = Configuration.GetValue<bool>(
				"UnsecureApplicationSettings:UseRedisCache");
			var redisCacheConnectionString = Configuration.GetValue<string>(
				"UnsecureApplicationSettings:RedisCacheConnectionString");

			// work object, where the computations are done.
			services.AddTransient<IComputationWorkService, ComputationWorkService>();

			// QueuedBackgroundService is a dual-purpose service
			services.AddHostedService<QueuedBackgroundService>();
			services.AddTransient<IQueuedBackgroundService, QueuedBackgroundService>();

			// Manages jobs
			services.AddTransient<IComputationJobStatusService, ComputationJobStatusService>();

			if (useRedisCache && !string.IsNullOrWhiteSpace(redisCacheConnectionString))
			{
				// setup redis cache for horizontally scaled services
				services.AddSingleton<IConnectionMultiplexer>(
					ConnectionMultiplexer.Connect(redisCacheConnectionString));
				// job status service, CRUD operations on jobs stored in redis cache.
				services.AddTransient<IJobStorageService, RedisCacheJobStorageService>();
			}
			else
			{
				// strictly for testing purposes
				services.AddTransient<IJobStorageService, MemoryCacheJobStorageService>();
			}

			services.AddSwaggerGen();
			services.AddHttpClient();

			services.AddControllers().AddNewtonsoftJson();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseSwagger();
			app.UseSwaggerUI(c =>
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "Background Computation Queue Example"));

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
