using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using System;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Client
{
    public class ChunklerClient : IChunklerClient
    {
        private IChunkObserver _clusterChunkObserver;
        private readonly IClusterClient _clusterClient;
        private readonly ILogger _logger;
        private readonly ChunklerClientOptions _options;
        private readonly IChunkObserver _chunkObserver;

        public ChunklerClient(ChunklerClientOptions options, ILogger logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _clusterClient = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(cfg =>
                {
                    cfg.ClusterId = options.ClusterOptions.ClusterId;
                    cfg.ServiceId = options.ClusterOptions.ServiceId;
                })
                .Build();

            _chunkObserver = new ChunkObserver(_logger);
        }

        public async Task Connect()
        {
            await _clusterClient.Connect();
            _clusterChunkObserver = await _clusterClient.CreateObjectReference<IChunkObserver>(_chunkObserver);
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
        
        public async Task SubscribeAsync(ChunkKey key, Action<ChunkUpdate> onUpdate)
        {
            var chunk = _clusterClient.GetGrain<IChunkGrain>(key.BattleId, FormatClusterKeyExtension(key), null);
            await chunk.Subscribe(_clusterChunkObserver);
        }

        public Task Unsubscribe(ChunkKey key)
        {
            throw new NotImplementedException();
        }
    }
}
