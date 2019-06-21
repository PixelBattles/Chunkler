using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;
using Orleans.Streams;
using PixelBattles.API.Client;
using PixelBattles.Chunkler.Abstractions;
using PixelBattles.Chunkler.Grains.ImageProcessing;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Grains
{
    [StorageProvider(ProviderName = ChunklerConstants.MongoDBGrainStorage)]
    [ImplicitStreamSubscription(ChunklerConstants.ChunkIncomingAction)]
    public class ChunkGrain : Grain<ChunkGrainState>, IChunkGrain, IAsyncObserver<ChunkAction>
    {
        private readonly ILogger _logger;
        private readonly IApiClient _apiClient;
        private readonly IImageProcessor _imageProcessor;
        
        private int _chunkWidth;
        private int _chunkHeight;
        private Rgba32[] _pixelsCache;
        private Guid _chunkKey;
        private long _battleId;
        private int _xChunkIndex;
        private int _yChunkIndex;
        
        private IAsyncStream<ChunkUpdate> _chunkUpdateEventStream;
        private IAsyncStream<ChunkAction> _chunkActionEventStream;

        public ChunkGrain(
            IApiClient apiClient,
            IImageProcessor imageProcessor,
            ILogger<ChunkGrain> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _imageProcessor = imageProcessor ?? throw new ArgumentNullException(nameof(imageProcessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ReadStateAsync()
        {
            await base.ReadStateAsync();

            _chunkKey = this.GetPrimaryKey();
            (_battleId, _xChunkIndex, _yChunkIndex) = GuidExtensions.ToKeys(_chunkKey);

            var battle = await _apiClient.GetBattleAsync(_battleId);
            _chunkWidth = battle.Settings.ChunkWidth;
            _chunkHeight = battle.Settings.ChunkHeight;

            if (State.Image == null)
            {
                _pixelsCache = _imageProcessor.GetDefaultImage(_chunkHeight, _chunkWidth);

                State.ChangeIndex = 0;
                State.Image = _imageProcessor.GetBytesFromPixels(_pixelsCache, _chunkHeight, _chunkWidth);
            }
            else
            {
                _pixelsCache = _imageProcessor.GetPixelsFromBytes(State.Image, _chunkHeight, _chunkWidth);
            }
        }

        public override async Task OnActivateAsync()
        {
            var streamProvider = GetStreamProvider(ChunklerConstants.SimpleChunkStreamProvider);
            _chunkUpdateEventStream = streamProvider.GetStream<ChunkUpdate>(_chunkKey, ChunklerConstants.ChunkOutcomingUpdate);
            _chunkActionEventStream = streamProvider.GetStream<ChunkAction>(_chunkKey, ChunklerConstants.ChunkIncomingAction);
            await _chunkActionEventStream.SubscribeAsync(this);
            await base.OnActivateAsync();
        }
        
        protected override Task ClearStateAsync()
        {
            State.Image = null;
            State.ChangeIndex = 0;
            _pixelsCache = null;

            return base.ClearStateAsync();
        }
                
        public Task<ChunkState> GetStateAsync()
        {
            var chunkState = new ChunkState
            {
                ChangeIndex = State.ChangeIndex,
                Image = State.Image
            };
            return Task.FromResult(chunkState);
        }

        public async Task<int> ProcessActionAsync(ChunkAction action)
        {
            State.ChangeIndex++;
            _pixelsCache[action.XIndex + action.YIndex * _chunkWidth].Rgba = action.Color;
            State.Image = _imageProcessor.GetBytesFromPixels(_pixelsCache, _chunkHeight, _chunkWidth);

            await WriteStateAsync();

            await _chunkUpdateEventStream.OnNextAsync(new ChunkUpdate
            {
                ChangeIndex = State.ChangeIndex,
                Color = action.Color,
                XIndex = action.XIndex,
                YIndex = action.YIndex
            });

            return State.ChangeIndex;
        }
        
        public Task OnCompletedAsync()
        {
            _logger.LogInformation("Complete chunk update message was received.");
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            _logger.LogError(ex, "Error during getting chunk update.");
            return Task.CompletedTask;
        }

        public async Task OnNextAsync(ChunkAction action, StreamSequenceToken token = null)
        {
            State.ChangeIndex++;
            _pixelsCache[action.XIndex + action.YIndex * _chunkWidth].Rgba = action.Color;
            State.Image = _imageProcessor.GetBytesFromPixels(_pixelsCache, _chunkHeight, _chunkWidth);

            await WriteStateAsync();

            await _chunkUpdateEventStream.OnNextAsync(new ChunkUpdate
            {
                ChangeIndex = State.ChangeIndex,
                Color = action.Color,
                XIndex = action.XIndex,
                YIndex = action.YIndex
            });
        }
    }
}