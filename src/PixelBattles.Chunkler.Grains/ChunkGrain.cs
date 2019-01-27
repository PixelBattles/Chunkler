using Orleans;
using Orleans.Providers;
using PixelBattles.API.Client;
using PixelBattles.API.DataTransfer.Battle;
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
        private readonly GrainObserverManager<IChunkObserver> _chunkObserver;

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
            _chunkObserver = new GrainObserverManager<IChunkObserver>();
        }

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
        }

        public override Task OnDeactivateAsync()
        {
            return base.OnDeactivateAsync();
        }

        protected override Task ClearStateAsync()
        {
            State.Image = null;
            State.ChangeIndex = 0;
            _pixelsCache = null;

            return base.ClearStateAsync();
        }

        protected override Task WriteStateAsync()
        {
            return base.WriteStateAsync();
        }

        protected override async Task ReadStateAsync()
        {
            await base.ReadStateAsync();

            _chunkKey = new ChunkKey
            {
                BattleId = this.GetPrimaryKey(out string postfix)
            };
            var chunkIndexes = postfix.Split(':').Select(t => int.Parse(t)).ToArray();
            _chunkKey.ChunkXIndex = chunkIndexes[0];
            _chunkKey.ChunkYIndex = chunkIndexes[1];

            BattleDTO battle = null;
            try
            {
                battle = await _apiClient.GetBattleAsync(_chunkKey.BattleId);
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to get battle by id: {_chunkKey.BattleId}", exception);
            }

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

        public Task Subscribe(IChunkObserver observer)
        {
            _chunkObserver.Subscribe(observer);
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

            _chunkObserver.Notify(observer => observer.ChunkUpdated(_chunkKey, new ChunkUpdate
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