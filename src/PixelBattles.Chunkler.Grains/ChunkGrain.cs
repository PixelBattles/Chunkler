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
        private readonly IApiClient apiClient;
        private Guid battleId;
        private int width;
        private int height;
        private Rgba32[] pixels;

        public ChunkGrain(IApiClient apiClient)
        {
            this.apiClient = apiClient;
        }

        protected override async Task ReadStateAsync()
        {
            battleId = this.GetPrimaryKey(out string postfix);
            var battle = await apiClient.GetBattleAsync(battleId);
            this.width = battle.Settings.ChunkWidth;
            this.height = battle.Settings.ChunkHeight;

            await base.ReadStateAsync();
        }

        public override Task OnActivateAsync()
        {
            if (State == null)
            {
                pixels = Enumerable
                    .Range(0, width * height)
                    .Select(t => Rgba32.White)
                    .ToArray();
                State = new ChunkGrainState
                {
                    ChangeIndex = 0,
                    Image = GetBytesFromPixels(pixels)
                };
            }
            else
            {
                pixels = GetPixelsFromBytes(State.Image);
            }
            return base.OnActivateAsync();
        }

        private readonly GrainObserverManager<IChunkObserver> observers = new GrainObserverManager<IChunkObserver>();
        
        public Task Subscribe(IChunkObserver observer)
        {
            observers.Subscribe(observer);
            return Task.FromResult(0);
        }
        
        public Task<ChunkState> GetStateAsync()
        {
            var chunkState = new ChunkState
            {
                ChangeIndex = State.ChangeIndex
            };

            return Task.FromResult(chunkState);
        }

        public async Task<bool> ProcessActionAsync(ChunkAction action)
        {
            State.ChangeIndex++;
            pixels[action.XIndex + action.YIndex * width].Rgba = action.Color;
            State.Image = GetBytesFromPixels(pixels);
            await WriteStateAsync();
            return true;
        }

        private Rgba32[] GetPixelsFromBytes(byte[] imageArray)
        {
            Rgba32[] tempPixels = new Rgba32[height * width];
            IImageDecoder imageDecoder = new PngDecoder()
            {
                IgnoreMetadata = true
            };

            var image = Image.Load(imageArray, imageDecoder);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tempPixels[x * height + y] = image[x, y];
                }
            }
            return tempPixels;
        }

        private byte[] GetBytesFromPixels(Rgba32[] pixelArray)
        {
            byte[] byteArray;
            using (MemoryStream stream = new MemoryStream())
            {
                var image = Image.LoadPixelData(pixelArray, width, height);
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
