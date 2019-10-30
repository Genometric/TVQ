using Autofac;
using Autofac.Extensions.DependencyInjection;
using Genometric.TVQ.API.Crawlers;
using Genometric.TVQ.API.Infrastructure;
using Genometric.TVQ.API.Infrastructure.BackgroundTasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;

namespace Genometric.TVQ.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            WebHostEnvironment = env;
        }

        public IWebHostEnvironment WebHostEnvironment { get; private set; }
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add 
        /// services to the container.
        /// </summary>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(
                options => options.EnableEndpointRouting = false
                ).SetCompatibilityVersion(
                CompatibilityVersion.Version_3_0);


            // TODO: can this be merged with the previous one?!
            services.AddMvc().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddCustomDbContext(Configuration);
            services.AddHostedService<QueuedHostedService>();
            services.AddHostedService<QueueLiteratureCrawling>();
            services.AddHostedService<QueueToolRepoCrawling>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddSingleton<IBackgroundToolRepoCrawlingQueue, BackgroundToolRepoCrawlingQueue>();
            services.AddSingleton<IBackgroundLiteratureCrawlingQueue, BackgroundLiteratureCrawlingQueue>();
            services.AddScoped<CrawlerService>();

            var container = new ContainerBuilder();
            container.Populate(services);
            return new AutofacServiceProvider(container.Build());
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to 
        /// configure the HTTP request pipeline.
        /// </summary>
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }

    public static class CustomExtensionMethods
    {
        public static IServiceCollection AddCustomDbContext(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContextPool<TVQContext>(
                options =>
                {
                    options
                    //.UseLazyLoadingProxies(true)
                    .UseSqlServer(
                        configuration.GetConnectionString("DefaultConnection"),
                        sqlServerOptionsAction: sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                            /// Configuring Connection Resiliency: 
                            /// https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                            sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 10,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorNumbersToAdd: null);
                        });

                    /// Changing default behavior when client evaluation occurs to throw. 
                    /// Default in EF Core would be to log a warning when client evaluation is performed.
                    options.ConfigureWarnings(
                        warnings => warnings.Throw(
                            RelationalEventId.QueryClientEvaluationWarning));
                    /// Check Client vs. Server evaluation: 
                    /// https://docs.microsoft.com/en-us/ef/core/querying/client-eval
                    /// 
                });

            return services;
        }
    }
}
