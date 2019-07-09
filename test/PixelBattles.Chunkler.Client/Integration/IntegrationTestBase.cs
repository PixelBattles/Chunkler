using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PixelBattles.API.Client;
using PixelBattles.Chunkler.Hosting;
using System;
using NSubstitute;
using Microsoft.Extensions.Logging;

namespace PixelBattles.Chunkler.Client.Tests
{
    public abstract class IntegrationTestBase : IDisposable
    {
        protected ISiloHost Host { get; private set; }
        protected IChunklerClient ChunklerClient { get; private set; }
        protected IApiClient ApiClient { get; private set; }
        protected long ActiveBattleId => 1L;
        protected ChunkKey ActiveBattleChunkKey => new ChunkKey() { BattleId = ActiveBattleId, ChunkXIndex = 0, ChunkYIndex = 0 };

        public IntegrationTestBase()
        {
            var hostBuilder = new SiloHostBuilder()
                .ConfigureDefaultHost();

            ConfigureCustomServices(hostBuilder);
            Host = hostBuilder.Build();

            ApiClient = Host.Services.GetService<IApiClient>();

            Host.StartAsync().Wait();

            ChunklerClient = new ChunklerClient(new ChunklerClientOptions
            {
                ClusterOptions = Host.Services.GetService<IOptions<ClusterOptions>>().Value
            },
            Substitute.For<ILogger>());
        }

        protected virtual ISiloHostBuilder ConfigureCustomServices(ISiloHostBuilder hostBuilder)
        {
            return hostBuilder;
        }

        public void Dispose()
        {
            ChunklerClient.Dispose();
            Host.StopAsync().Wait();
            Host.Dispose();
        }
    }
}
