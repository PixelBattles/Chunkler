using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PixelBattles.Chunkler.Grains.ImageProcessing
{
    public class ImageProcessor : IImageProcessor
    {
        public Rgba32[] GetPixelsFromBytes(byte[] imageArray, int height, int width)
        {
            Rgba32[] tempPixels = new Rgba32[height * width];
            IImageDecoder imageDecoder = new PngDecoder()
            {
                IgnoreMetadata = true
            };

            using (var image = Image.Load(imageArray, imageDecoder))
            {
                for (int x = 0; x < height; x++)
                {
                    for (int y = 0; y < width; y++)
                    {
                        tempPixels[x * width + y] = image[y, x];
                    }
                }
                return tempPixels;
            }
        }
        public byte[] GetBytesFromPixels(Rgba32[] pixelArray, int height, int width)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (var image = Image.LoadPixelData(pixelArray, width, height))
                {
                    PngEncoder pngEncoder = new PngEncoder
                    {
                        CompressionLevel = 9,
                        ColorType = PngColorType.RgbWithAlpha
                    };
                    image.SaveAsPng(stream, pngEncoder);
                    return stream.ToArray();
                }
            }
        }
        public Rgba32[] GetDefaultImage(int height, int width, uint color = uint.MaxValue)
        {
            if (height <= 0 || width <= 0)
            {
                throw new ArgumentException();
            }

            var pixel = new Rgba32(color);

            return Enumerable
                    .Range(0, height * width)
                    .Select(t => pixel)
                    .ToArray();
        }
        public byte[] GenerateImageFromChunks(IEnumerable<(int x, int y, byte[] image)> chunks, int chunkHeight, int chunkWidth, int previewHalfHeight, int previewHalfWidth, int centerX, int centerY, uint color = uint.MaxValue)
        {
            int previewHeight = previewHalfHeight * 2;
            int previewWidth = previewHalfWidth * 2;
            var pixel = new Rgba32(color);
            var pixelArray = Enumerable
                            .Range(0, previewWidth * previewHeight)
                            .Select(t => pixel)
                            .ToArray();

            using (var previewImage = Image.LoadPixelData(pixelArray, previewWidth, previewHeight))
            {

                previewImage.Mutate(ctx =>
                {
                    IImageDecoder imageDecoder = new PngDecoder()
                    {
                        IgnoreMetadata = true
                    };

                    foreach (var (x, y, image) in chunks)
                    {
                        using (var chunkImage = Image.Load(image, imageDecoder))
                        {
                            ctx.DrawImage(chunkImage, new Point(previewHalfWidth + x * chunkWidth - centerX, previewHalfHeight + y * chunkHeight - centerY), 1f);
                        }
                    }
                });

                using (MemoryStream stream = new MemoryStream())
                {
                    PngEncoder pngEncoder = new PngEncoder
                    {
                        CompressionLevel = 9,
                        ColorType = PngColorType.RgbWithAlpha
                    };
                    previewImage.SaveAsPng(stream, pngEncoder);
                    return stream.ToArray();
                }
            }
        }
    }
}