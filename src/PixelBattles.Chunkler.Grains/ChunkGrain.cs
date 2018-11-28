using Orleans;
using Orleans.Providers;
using PixelBattles.API.Client;
using PixelBattles.Chunkler.Grains.ImageProcessing;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Grains
{
    [StorageProvider(ProviderName = "MongoDBGrainStorage")]
    public class ChunkGrain : Grain<ChunkGrainState>, IChunkGrain
    {
        private readonly IApiClient _apiClient;
        private readonly IImageProcessor _imageProcessor;
        private readonly GrainObserverManager<IChunkObserver> observers = new GrainObserverManager<IChunkObserver>();

        private Guid _battleId;
        private int _chunkWidth;
        private int _chunkHeight;
        private Rgba32[] _pixelsCache;

        public ChunkGrain(
            IApiClient apiClient,
            IImageProcessor imageProcessor)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _imageProcessor = imageProcessor ?? throw new ArgumentNullException(nameof(imageProcessor));
        }
        
        protected override async Task ReadStateAsync()
        {
            _battleId = this.GetPrimaryKey(out string postfix);
            var battle = await _apiClient.GetBattleAsync(_battleId);
            _chunkWidth = battle.Settings.ChunkWidth;
            _chunkHeight = battle.Settings.ChunkHeight;

            await base.ReadStateAsync();
        }

        public override async Task OnActivateAsync()
        {
            if (State.Image == null)
            {
                _pixelsCache = _imageProcessor.GetDefaultImage(_chunkHeight, _chunkWidth);

                State = new ChunkGrainState
                {
                    ChangeIndex = 0,
                    Image = _imageProcessor.GetBytesFromPixels(_pixelsCache, _chunkHeight, _chunkWidth)
                };
                await WriteStateAsync();
            }
            else
            {
                _pixelsCache = _imageProcessor.GetPixelsFromBytes(State.Image, _chunkHeight, _chunkWidth);
            }
            await base.OnActivateAsync();
        }
        
        public Task Subscribe(IChunkObserver observer)
        {
            observers.Subscribe(observer);
            return Task.FromResult(0);
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
            return State.ChangeIndex;
        }
    }
}
