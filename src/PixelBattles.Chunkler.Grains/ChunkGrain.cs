using Orleans;
using Orleans.Providers;
using PixelBattles.Server.Client;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Grains
{
    [StorageProvider(ProviderName = "MemoryStore")]
    public class ChunkGrain : Grain<ChunkGrainState>, IChunkGrain
    {
        private readonly IApiClient _apiClient;
        private readonly GrainObserverManager<IChunkObserver> observers = new GrainObserverManager<IChunkObserver>();

        private Guid _battleId;
        private int _chunkWidth;
        private int _chunkHeight;
        private Rgba32[] _pixelsCache;

        public ChunkGrain(IApiClient apiClient)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
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
                _pixelsCache = Enumerable
                    .Range(0, _chunkWidth * _chunkHeight)
                    .Select(t => Rgba32.White)
                    .ToArray();

                State = new ChunkGrainState
                {
                    ChangeIndex = 0,
                    Image = GetBytesFromPixels(_pixelsCache)
                };
                await WriteStateAsync();
            }
            else
            {
                _pixelsCache = GetPixelsFromBytes(State.Image);
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
            State.Image = GetBytesFromPixels(_pixelsCache);
            await WriteStateAsync();
            return State.ChangeIndex;
        }

        private Rgba32[] GetPixelsFromBytes(byte[] imageArray)
        {
            Rgba32[] tempPixels = new Rgba32[_chunkHeight * _chunkWidth];
            IImageDecoder imageDecoder = new PngDecoder()
            {
                IgnoreMetadata = true
            };

            var image = Image.Load(imageArray, imageDecoder);
            for (int x = 0; x < _chunkWidth; x++)
            {
                for (int y = 0; y < _chunkHeight; y++)
                {
                    tempPixels[x * _chunkHeight + y] = image[x, y];
                }
            }
            return tempPixels;
        }

        private byte[] GetBytesFromPixels(Rgba32[] pixelArray)
        {
            byte[] byteArray;
            using (MemoryStream stream = new MemoryStream())
            {
                var image = Image.LoadPixelData(pixelArray, _chunkWidth, _chunkHeight);
                PngEncoder pngEncoder = new PngEncoder
                {
                    CompressionLevel = 9,
                    PngColorType = PngColorType.RgbWithAlpha
                };
                image.SaveAsPng(stream, pngEncoder);
                byteArray = stream.ToArray();
            }
            return byteArray;
        }
    }
}
