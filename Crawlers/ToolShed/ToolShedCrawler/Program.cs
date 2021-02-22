using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ToolShedCrawler
{
    public class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var crawler = serviceProvider.GetService<Crawler>();
            crawler.Crawl();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole()).AddTransient<Crawler>();
        }
    }
}
