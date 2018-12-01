using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using System;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Client
{
    public class ChunklerClient : IChunklerClient
    {
        private IClusterClient _clusterClient;
        private readonly ChunklerClientOptions _options;

        public ChunklerClient(ChunklerClientOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _clusterClient = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(cfg =>
                {
                    cfg.ClusterId = options.ClusterOptions.ClusterId;
                    cfg.ServiceId = options.ClusterOptions.ServiceId;
                })
                .Build();
        }

        public async Task Connect()
        {
            await _clusterClient.Connect();
        }

        public async Task Close()
        {
            await _clusterClient.Close();
        }

        public Task<int> ProcessAction(ChunkKey key, ChunkAction action)
        {
            var chunk = _clusterClient.GetGrain<IChunkGrain>(key.BattleId, FormatClusterKeyExtension(key), null);
            return chunk.ProcessActionAsync(action);
        }

        public Task<ChunkState> GetChunkState(ChunkKey key)
        {
            var chunk = _clusterClient.GetGrain<IChunkGrain>(key.BattleId, FormatClusterKeyExtension(key), null);
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
