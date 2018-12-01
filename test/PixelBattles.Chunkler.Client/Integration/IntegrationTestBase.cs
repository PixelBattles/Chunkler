using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using PixelBattles.API.Client;
using PixelBattles.Chunkler.Hosting;
using System;

namespace PixelBattles.Chunkler.Client.Tests
{
    public abstract class IntegrationTestBase : IDisposable
    {
        protected ISiloHost Host { get; private set; }
        protected IChunklerClient ChunklerClient { get; private set; }
        protected IApiClient ApiClient { get; private set; }

        public IntegrationTestBase()
        {
            var hostBuilder = new SiloHostBuilder().ConfigureDefaultHost();
            ConfigureCustomServices(hostBuilder);
            Host = hostBuilder.Build();

            ChunklerClient = new ChunklerClient(new ChunklerClientOptions
            {
                ClusterOptions = Host.Services.GetService<IOptions<ClusterOptions>>().Value
            });

            ApiClient = Host.Services.GetService<IApiClient>();

            Host.StartAsync().Wait();
            ChunklerClient.Connect().Wait();
        }
        
        protected virtual ISiloHostBuilder ConfigureCustomServices(ISiloHostBuilder hostBuilder)
        {
            return hostBuilder;
        }

        public void Dispose()
        {
            ChunklerClient.Close().Wait();
            Host.StopAsync().Wait();
            Host.Dispose();
        }
    }
}
