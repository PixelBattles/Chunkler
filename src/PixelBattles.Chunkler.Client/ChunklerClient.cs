using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
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

        public Task<int> ProcessAction(ChunkKey key, ChunkAction action)
        {
            var chunk = clusterClient.GetGrain<IChunkGrain>(key.BattleId, FormatClusterKeyExtension(key), null);
            return chunk.ProcessActionAsync(action);
        }

        public Task<ChunkState> GetChunkState(ChunkKey key)
        {
            var chunk = clusterClient.GetGrain<IChunkGrain>(key.BattleId, FormatClusterKeyExtension(key), null);
            return chunk.GetStateAsync();
        }

        private string FormatClusterKeyExtension(ChunkKey key)
        {
            return $"{key.ChunkXIndex}:{key.ChunkYIndex}";
        }
        
        public Task Subscribe(ChunkKey key, Action<ChunkUpdate> onUpdate)
        {
            throw new NotImplementedException();
        }

        public Task Unsubscribe(ChunkKey key)
        {
            throw new NotImplementedException();
        }
    }
}
