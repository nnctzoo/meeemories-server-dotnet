using Meeemories.Core.Models;
using Meeemories.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Meeemories.Core
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMediaServices(this IServiceCollection services, IConfiguration section)
        {
            services.Configure<Settings>(section);
            services.AddSingleton<IStorageAccessor, StorageAccessor>();
            services.AddTransient<IMediaRepository, MediaRepository>();
        }
    }
}