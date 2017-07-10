using System;
using System.Collections.Generic;
using System.Linq;
using FluentScheduler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Services
{
    /// <summary>
    /// Sheluder that checks source feeds for new articles.
    /// </summary>
    public class Sheluder
    {
        private readonly Registry _registry = new Registry();
        private readonly ApplicationDbContext _dbContext;
        private readonly ISourceSaver _sourceSaver;
        private readonly JobFactory jf;
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="sourceSaver"></param>
        public Sheluder(ApplicationDbContext dbContext, ISourceSaver sourceSaver, IServiceProvider services)
        {
            _sourceSaver = sourceSaver;
            _dbContext = dbContext;

            jf = new JobFactory(services);
            JobManager.JobFactory = jf;
        }
        /// <summary>
        /// Resets sheluder interval by specified timeout.
        /// </summary>
        /// <param name="minutes"></param>
        public void ChangeTimeoutByMinutes(int minutes)
        {
            //            JobManager.AddJob(() => UpdateAllSources(sources), s => s
            //                .ToRunNow()
            //                .AndEvery(minutes)
            //                .Minutes());
        }
        /// <summary>
        /// Job for updating articles.
        /// </summary>
        private async void UpdateAllSources(IEnumerable<Source> sources)
        {
            foreach (var source in sources)
            {
                await _sourceSaver.Save(source);
            }
        }
    }

    public class Job : IJob
    {
        private readonly object _lock = new object();

        private readonly ApplicationDbContext _dbContext;
        private readonly ISourceSaver _sourceSaver;

        public Job(ApplicationDbContext dbContext, ISourceSaver sourceSaver)
        {
            _sourceSaver = sourceSaver;
            _dbContext = dbContext;
        }

        public void Execute()
        {
            lock (_lock)
            {
                var sources = _dbContext.Sources.ToArray();

                foreach (var source in sources)
                {
                    _sourceSaver.Save(source);
                }
            }
        }
    }

    public class JobFactory : IJobFactory
    {
        private readonly IServiceProvider _services;

        public JobFactory(IServiceProvider services)
        {
            _services = services;
        }

        public IJob GetJobInstance<T>() where T : IJob
        {
            return _services.GetService<T>();
        }
    }

    public class MyRegistry : Registry
    {
        public MyRegistry()
        {
            JobManager.AddJob<Job>(schedule => schedule.ToRunNow().AndEvery(5).Minutes());
//            JobManager.AddJob<Job>(schedule => schedule.ToRunNow().AndEvery(20).Seconds());
        }
    }
}