using Meeemories.Functions.Models;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

[assembly: FunctionsStartup(typeof(Meeemories.Startup))]

namespace Meeemories
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();
        }
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<Settings>() .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("Meeemories").Bind(settings);
                settings.ConnectionString = configuration.GetValue<string>("AzureWebJobsStorage");
            });
            builder.Services.AddSingleton<MediaService>();
        }
    }
}