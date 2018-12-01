using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using System;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Client
{
    public class ChunklerClient : IChunklerClient
    {
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
            //var observerReference = await _clusterClient.CreateObjectReference<IChunkObserver>(observer);
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

        //private static async Task StaySubscribed(IChunkGrain grain, IChunkObserver observer, CancellationToken token)
        //{
        //    while (!token.IsCancellationRequested)
        //    {
        //        try
        //        {
        //            await Task.Delay(TimeSpan.FromSeconds(5), token);
        //            await grain.Subscribe(_chunkObserver);
        //        }
        //        catch (Exception exception)
        //        {
        //            Console.WriteLine($"Exception while trying to subscribe for updates: {exception}");
        //        }
        //    }
        //}
    }
}
