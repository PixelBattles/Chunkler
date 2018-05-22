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

        public Task<bool> ProcessAction(BattleAction action)
        {
            var chunk = clusterClient.GetGrain<IChunkGrain>(action.Key.BattleId, FormatClusterKeyExtension(action), null);
            var chunkAction = new ChunkAction
            {
                Color = action.Color,
                XIndex = action.WidthIndex,
                YIndex = action.HeightIndex
            };
            return chunk.ProcessActionAsync(chunkAction);
        }

        private string FormatClusterKeyExtension(BattleAction battleAction)
        {
            return $"{battleAction.Key.ChunkXIndex}:{battleAction.Key.ChunkYIndex}";
        }
        
        public Task Subscribe(ChunkKey key, Action<ChunkUpdate> onUpdate)
        {
            throw new NotImplementedException();
        }

        public Task Unsubscribe(ChunkKey key)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            clusterClient.Dispose();
        }
    }
}
