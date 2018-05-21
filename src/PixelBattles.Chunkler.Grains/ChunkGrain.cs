using Orleans;
using Orleans.Providers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Grains
{
    [StorageProvider(ProviderName = "MemoryStore")]
    public class ChunkGrain : Grain<ChunkGrainState>, IChunkGrain
    {
        private int width;

        private int height;

        private Rgba32[] pixels;

        private byte[] image;

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
