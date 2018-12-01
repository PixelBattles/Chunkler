using Orleans;
using Orleans.Providers;
using PixelBattles.API.Client;
using PixelBattles.Chunkler.Grains.ImageProcessing;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Grains
{
    [StorageProvider(ProviderName = "MongoDBGrainStorage")]
    public class ChunkGrain : Grain<ChunkGrainState>, IChunkGrain
    {
        private readonly IApiClient _apiClient;
        private readonly IImageProcessor _imageProcessor;
        private readonly GrainObserverManager<IChunkObserver> _observers;

        private ChunkKey _chunkKey;

        private int _chunkWidth;
        private int _chunkHeight;
        private Rgba32[] _pixelsCache;

        public ChunkGrain(
            IApiClient apiClient,
            IImageProcessor imageProcessor)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _imageProcessor = imageProcessor ?? throw new ArgumentNullException(nameof(imageProcessor));
            _observers = new GrainObserverManager<IChunkObserver>();
        }
        
        protected override async Task ReadStateAsync()
        {
            _chunkKey = new ChunkKey();
            _chunkKey.BattleId = this.GetPrimaryKey(out string postfix);
            var chunkIndexes = postfix.Split(':').Select(t => int.Parse(t)).ToArray();
            _chunkKey.ChunkXIndex = chunkIndexes[0];
            _chunkKey.ChunkYIndex = chunkIndexes[1];

            var battle = await _apiClient.GetBattleAsync(_chunkKey.BattleId);
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
            _observers.Subscribe(observer);
            return Task.CompletedTask;
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

            _observers.Notify(observer => observer.ChunkUpdated(_chunkKey, new ChunkUpdate
            {
                ChangeIndex = State.ChangeIndex,
                Color = action.Color,
                XIndex = action.XIndex,
                YIndex = action.YIndex
            }));

            return State.ChangeIndex;
        }
    }
}
