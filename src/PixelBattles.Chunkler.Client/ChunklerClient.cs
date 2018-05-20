using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using PixelBattles.Chunkler.Grains;
using System;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Client
{
    public class ChunklerClient : IChunklerClient
    {
        private IClusterClient clusterClient;

        public ChunklerClient(Action<ILoggingBuilder> configureLogging)
        {
            clusterClient = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "MyAwesomeService";
                })
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IChunkGrain).Assembly))
                .ConfigureLogging(configureLogging)
                .Build();
        }

        public async Task Connect()
        {
            await clusterClient.Connect();
        }

        public async Task Close()
        {
            await clusterClient.Close();
        }

        public Task<bool> ProcessAction(GameAction gameAction)
        {
            var chunk = clusterClient.GetGrain<IChunkGrain>(gameAction.GameId, FormatClusterKeyExtension(gameAction), null);
            var chunkAction = new ChunkAction
            {
                Color = gameAction.Color,
                X = gameAction.XIndex,
                Y = gameAction.YIndex
            };
            return chunk.ProcessActionAsync(chunkAction);
        }

        private string FormatClusterKeyExtension(GameAction gameAction)
        {
            return $"{gameAction.ChunkXIndex}:{gameAction.ChunkYIndex}";
        }
        
        public void Dispose()
        {
            clusterClient.Dispose();
        }
    }
}
