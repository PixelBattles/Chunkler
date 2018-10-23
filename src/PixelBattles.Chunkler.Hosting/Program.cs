using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers.MongoDB.Configuration;
using PixelBattles.Chunkler.Grains;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Hosting
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var host = await StartSilo();
                Console.WriteLine("Press Enter to terminate...");
                Console.ReadLine();

                await host.StopAsync();

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }
        }

        private static async Task<ISiloHost> StartSilo()
        {
            var builder = new SiloHostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("hostsettings.json", optional: true);
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true)
                        .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                })
                .ConfigureServices((context, services) => 
                {
                    services.Configure<MongoDBMembershipTableOptions>(context.Configuration.GetSection(nameof(MongoDBMembershipTableOptions)));
                    services.Configure<MongoDBGrainStorageOptions>("MongoDBGrainStorage", context.Configuration.GetSection(nameof(MongoDBGrainStorageOptions)));
                    services.Configure<ClusterOptions>(context.Configuration.GetSection(nameof(ClusterOptions)));
                })
                .UseMongoDBClustering()
                .AddMongoDBGrainStorage("MongoDBGrainStorage")
                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                .ConfigureLogging(logging => logging.AddConsole())
                //.ConfigureServices(c => c.AddApiClient(opt => opt.BaseUrl = "http://localhost:5000"))
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ChunkGrain).Assembly).WithReferences());

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}
