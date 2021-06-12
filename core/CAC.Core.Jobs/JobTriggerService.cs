using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CAC.Core.Jobs
{
    internal sealed class JobTriggerService : IHostedService
    {
        private readonly IHostApplicationLifetime appLifetime;
        private readonly IReadOnlyDictionary<string, IJob> jobsByName;
        private readonly ILogger<JobTriggerService> logger;
        private readonly IOptions<JobTriggerOptions> options;

        private int exitCode = -1;

        public JobTriggerService(IHostApplicationLifetime appLifetime,
                                 IEnumerable<IJob> jobs,
                                 IOptions<JobTriggerOptions> options,
                                 ILogger<JobTriggerService> logger)
        {
            jobsByName = jobs.ToDictionary(j => j.GetType().Name);
            this.appLifetime = appLifetime;
            this.options = options;
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = appLifetime.ApplicationStarted.Register(() => Task.Run(RunJob, cancellationToken));
            return Task.CompletedTask;

            async Task RunJob()
            {
                try
                {
                    if (!jobsByName.TryGetValue(options.Value.JobName, out var job))
                    {
                        logger.LogError("Job '{JobName}' does not exist!", options.Value.JobName);
                        exitCode = 1;
                        return;
                    }

                    await job.RunAsync();
                    exitCode = 0;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unhandled exception!");
                    exitCode = 1;
                }
                finally
                {
                    appLifetime.StopApplication();
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogDebug("Exiting with return code: {ExitCode}", exitCode);
            Environment.ExitCode = exitCode;
            return Task.CompletedTask;
        }
    }
}
