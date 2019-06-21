using HashDepot;
using PixelBattles.Chunkler.Grains.ImageProcessing;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace PixelBattles.Chunkler.Grains.Tests
{
    public class ImageProcessorTests
    {
        private IImageProcessor _imageProcessor;
        public ImageProcessorTests()
        {
            _imageProcessor = new ImageProcessor();
        }

        [Theory]
        [InlineData(10, 10)]
        [InlineData(1, 10)]
        [InlineData(10, 1)]
        [InlineData(1, 1)]
        public void ImageProcessor_Can_GetDefaultImage(int height, int width)
        {
            var defaultImage = _imageProcessor.GetDefaultImage(height, width);
            
            Assert.NotNull(defaultImage);
            Assert.Equal(height * width, defaultImage.Length);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(-1, 1)]
        [InlineData(1, -1)]
        [InlineData(-1, -1)]
        public void ImageProcessor_ThrowsException_GetDefaultImageCalled_WithWrongInput(int height, int width)
        {
            Assert.ThrowsAny<Exception>(() =>
            {
                var defaultImage = _imageProcessor.GetDefaultImage(height, width);
            });
        }

        [Fact]
        public void ImageProcessor_Can_ConvertBytesFromPixels()
        {
            var image = _imageProcessor.GetBytesFromPixels(new[] { Rgba32.OldLace }, 1, 1);

            Assert.NotNull(image);
        }

        [Fact]
        public void ImageProcessor_ConvertImages_To_Original()
        {
            var originalImage = new[]
            {
                Rgba32.Red,
                Rgba32.Green,
                Rgba32.Blue,
                Rgba32.Black,
                Rgba32.White,
                Rgba32.Yellow
            };
            var serializedImage = _imageProcessor.GetBytesFromPixels(originalImage, 2, 3);
            var resultImage = _imageProcessor.GetPixelsFromBytes(serializedImage, 2, 3);

            Assert.True(AreSame(originalImage, resultImage));
        }

        [Fact]
        public void ImageProcessor_Can_GenerateImageFromChunks()
        {
            var image = _imageProcessor.GetBytesFromPixels(_imageProcessor.GetDefaultImage(100, 100, 4278190080), 100, 100);
            var chunks = new List<(int x, int y, byte[] image)>
            {
                (0, 0, image),
                (1, 0, image),
                (2, 0, image)
            };
            var previewImage = _imageProcessor.GenerateImageFromChunks(chunks, 100, 100, 100, 150, 100, 50);
            Assert.Equal(45613623U, XXHash.Hash32(previewImage));
        }

        private bool AreSame(Rgba32[] expected, Rgba32[] actual)
        {
            if (expected.Length != actual.Length)
            {
                return false;
            }
            
            for (int i = 0; i < expected.Length; i++)
            {
                if (expected[i] != actual[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}