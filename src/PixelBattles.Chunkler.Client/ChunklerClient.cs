﻿using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Streams;
using PixelBattles.Chunkler.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Client
{
    public class ChunklerClient : IChunklerClient
    {
        private readonly ILogger _logger;
        private readonly ChunklerClientOptions _options;
        private readonly IClusterClient _clusterClient;
        private readonly IStreamProvider _streamProvider;
        private readonly ConcurrentDictionary<ChunkKey, StreamSubscriptionHandle<ChunkUpdate>> _streamSubscriptionHandles;

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
                .AddSimpleMessageStreamProvider(ChunklerConstants.SimpleChunkStreamProvider)
                .Build();

            _clusterClient.Connect().Wait();
            _streamProvider = _clusterClient.GetStreamProvider(ChunklerConstants.SimpleChunkStreamProvider);
        }
        
        public Task<ChunkState> GetChunkStateAsync(ChunkKey key)
        {
            var chunk = _clusterClient.GetGrain<IChunkGrain>(FormatChunkKey(key));
            return chunk.GetStateAsync();
        }
        
        public async Task<IChunkSubscription> SubscribeOnChunkUpdateAsync(ChunkKey key, Func<ChunkUpdate, Task> onUpdate)
        {
            var stream = _streamProvider.GetStream<ChunkUpdate>(FormatChunkKey(key), ChunklerConstants.ChunkOutcomingUpdate);
            var handler = await stream.SubscribeAsync(new ChunkObserver(_logger, onUpdate));
            return new ChunkSubscription(handler);
        }
        
        public Task<int> ProcessChunkActionAsync(ChunkKey key, ChunkAction action)
        {
            var chunk = _clusterClient.GetGrain<IChunkGrain>(FormatChunkKey(key));
            return chunk.ProcessActionAsync(action);
        }

        public async Task EnqueueChunkActionAsync(ChunkKey key, ChunkAction action)
        {
            var stream =_streamProvider.GetStream<ChunkAction>(FormatChunkKey(key), ChunklerConstants.ChunkIncomingAction);
            await stream.OnNextAsync(action);
        }

        private Guid FormatChunkKey(ChunkKey key)
        {
            return GuidExtensions.ToGuid(key.BattleId, key.ChunkXIndex, key.ChunkYIndex);
        }

        public void Dispose()
        {
            _clusterClient.Close().Wait();
        }

        public Task ActivateBattleReminder(long battleId, TimeSpan refreshInterval)
        {
            var chunk = _clusterClient.GetGrain<IBattleGrain>(battleId);
            return chunk.ActivateBattleReminderAsync(refreshInterval);
        }
    }
}
