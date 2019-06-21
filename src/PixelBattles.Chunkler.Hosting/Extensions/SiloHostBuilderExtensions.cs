using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers.MongoDB.Configuration;
using PixelBattles.API.Client;
using PixelBattles.Chunkler.Abstractions;
using PixelBattles.Chunkler.Grains;
using PixelBattles.Chunkler.Grains.ImageProcessing;
using System.IO;
using System.Net;

namespace PixelBattles.Chunkler.Hosting
{
    public static class SiloHostBuilderExtensions
    {
        public static ISiloHostBuilder ConfigureDefaultHost(this ISiloHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureHostConfiguration(configHost =>
                 {
                     configHost
                         .SetBasePath(Directory.GetCurrentDirectory())
                         .AddEnvironmentVariables()
                         .AddJsonFile("hostsettings.json", optional: true);
                 })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddEnvironmentVariables()
                        .AddJsonFile("appsettings.json", optional: true)
                        .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.Configure<MongoDBMembershipTableOptions>(context.Configuration.GetSection(nameof(MongoDBMembershipTableOptions)));
                    services.Configure<MongoDBGrainStorageOptions>(ChunklerConstants.MongoDBGrainStorage, context.Configuration.GetSection(nameof(MongoDBGrainStorageOptions)));
                    services.Configure<ClusterOptions>(context.Configuration.GetSection(nameof(ClusterOptions)));
                    services.Configure<ApiClientOptions>(context.Configuration.GetSection(nameof(ApiClientOptions)));
                    services.Configure<MongoDBRemindersOptions>(context.Configuration.GetSection(nameof(MongoDBRemindersOptions)));
                })
                .UseMongoDBClustering()
                .UseMongoDBReminders()
                .AddMongoDBGrainStorage(ChunklerConstants.MongoDBGrainStorage)
                .AddMemoryGrainStorage(ChunklerConstants.PubSubStore)
                .AddSimpleMessageStreamProvider(ChunklerConstants.SimpleChunkStreamProvider)
                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                .ConfigureLogging(logging => logging.AddConsole())
                .ConfigureChunklerServices();
        }

        public static ISiloHostBuilder ConfigureChunklerServices(this ISiloHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureServices(c => c.AddSingleton<IApiClient, ApiClient>())
                .ConfigureServices(c => c.AddSingleton<IImageProcessor, ImageProcessor>())
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ChunkGrain).Assembly).WithReferences());
        }
    }
}
