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

        public Task<bool> ProcessAction(BattleAction battleAction)
        {
            var chunk = clusterClient.GetGrain<IChunkGrain>(battleAction.BattleId, FormatClusterKeyExtension(battleAction), null);
            var chunkAction = new ChunkAction
            {
                Color = battleAction.Color,
                XIndex = battleAction.XIndex,
                YIndex = battleAction.YIndex
            };
            return chunk.ProcessActionAsync(chunkAction);
        }

        private string FormatClusterKeyExtension(BattleAction battleAction)
        {
            return $"{battleAction.ChunkXIndex}:{battleAction.ChunkYIndex}";
        }
        
        public void Dispose()
        {
            clusterClient.Dispose();
        }
    }
}
