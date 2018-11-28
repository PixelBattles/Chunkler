using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Linq;

namespace PixelBattles.Chunkler.Grains.ImageProcessing
{
    public class ImageProcessor : IImageProcessor
    {
        public ImageProcessor()
        {

        }

        public Rgba32[] GetPixelsFromBytes(byte[] imageArray, int height, int width)
        {
            Rgba32[] tempPixels = new Rgba32[height * width];
            IImageDecoder imageDecoder = new PngDecoder()
            {
                IgnoreMetadata = true
            };

            var image = Image.Load(imageArray, imageDecoder);
            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    tempPixels[x * width + y] = image[y, x];
                }
            }
            return tempPixels;
        }
        public byte[] GetBytesFromPixels(Rgba32[] pixelArray, int height, int width)
        {
            byte[] byteArray;
            using (MemoryStream stream = new MemoryStream())
            {
                var image = Image.LoadPixelData(pixelArray, width, height);
                PngEncoder pngEncoder = new PngEncoder
                {
                    CompressionLevel = 9,
                    ColorType = PngColorType.RgbWithAlpha
                };
                image.SaveAsPng(stream, pngEncoder);
                byteArray = stream.ToArray();
            }
            return byteArray;
        }
        public Rgba32[] GetDefaultImage(int height, int width)
        {
            if (height <= 0 || width <= 0)
            {
                throw new ArgumentException();
            }

            return Enumerable
                    .Range(0, height * width)
                    .Select(t => Rgba32.White)
                    .ToArray();
        }
    }
}