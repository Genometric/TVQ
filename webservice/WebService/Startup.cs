using Genometric.TVQ.WebService.Analysis;
using Genometric.TVQ.WebService.Crawlers;
using Genometric.TVQ.WebService.Infrastructure.BackgroundTasks;
using Genometric.TVQ.WebService.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Genometric.TVQ.WebService
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
        public void ConfigureServices(IServiceCollection services)
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
            services.AddHostedService<BaseScheduler<LiteratureCrawlingService, LiteratureCrawlingJob>>();
            services.AddHostedService<BaseScheduler<RepoCrawlingService, RepoCrawlingJob>>();
            services.AddHostedService<BaseScheduler<AnalysisService, AnalysisJob>>();
            services.AddSingleton<IBaseBackgroundTaskQueue<AnalysisJob>, BaseBackgroundTaskQueue<AnalysisJob>>();
            services.AddSingleton<IBaseBackgroundTaskQueue<RepoCrawlingJob>, BaseBackgroundTaskQueue<RepoCrawlingJob>>();
            services.AddSingleton<IBaseBackgroundTaskQueue<LiteratureCrawlingJob>, BaseBackgroundTaskQueue<LiteratureCrawlingJob>>();
            services.AddScoped<RepoCrawlingService>();
            services.AddScoped<LiteratureCrawlingService>();
            services.AddScoped<AnalysisService>();
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
}
