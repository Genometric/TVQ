﻿using Genometric.TVQ.WebService.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genometric.TVQ.WebService.Infrastructure.BackgroundTasks.JobRunners
{
    public abstract class BaseService<T>
        where T : BaseJob
    {
        protected TVQContext Context { get; }
        protected ILogger<BaseService<T>> Logger { get; }

        protected BaseService(
            TVQContext context,
            ILogger<BaseService<T>> logger)
        {
            Context = context;
            Logger = logger;
        }

        protected abstract Task ExecuteAsync(T job, CancellationToken cancellationToken);

        public async Task StartAsync(T job, CancellationToken cancellationToken)
        {
            var dbSet = Context.Set<T>();
            if (!dbSet.Local.Any(e => e.ID == job.ID))
                Context.Attach(job);

            job.Status = State.Running;
            await Context.SaveChangesAsync().ConfigureAwait(false);

            try
            {
                await ExecuteAsync(job, cancellationToken).ConfigureAwait(false);
                job.Status = State.Completed;
            }
            catch (Exception e)
            {
                if (e is DbUpdateException || e is DbUpdateConcurrencyException)
                { }

                Logger.LogError($"Error occurred executing job `{job.ID}`: {e.Message}");
                job.Status = State.Failed;
                job.Message = e.Message;
                throw;
            }
            finally
            {
                // TODO: catch exception that may happen here (e.g., Microsoft.Data.SqlClient.SqlException).
                await Context.SaveChangesAsync().ConfigureAwait(false);
                Logger.LogInformation($"Job `{job.ID}` executed successfully.");
            }
        }
    }
}
