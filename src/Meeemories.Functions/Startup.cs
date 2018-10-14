using System;
using Meeemories.Core;
using Meeemories.Functions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(Startup))]
namespace Meeemories.Functions
{
    internal class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder) =>
            builder.AddDependencyInjection<ServiceProviderBuilder>();
    }
    internal class ServiceProviderBuilder : IServiceProviderBuilder
    {
        private readonly IConfiguration _configuration;

        public ServiceProviderBuilder(IConfiguration configuration) =>
            _configuration = configuration;

        public IServiceProvider Build()
        {
            var services = new ServiceCollection();

            services.AddMediaServices(_configuration.GetSection("Core"));

            return services.BuildServiceProvider();
        }
    }
}
