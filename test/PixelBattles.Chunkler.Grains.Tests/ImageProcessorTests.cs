using PixelBattles.Chunkler.Grains.ImageProcessing;
using SixLabors.ImageSharp.PixelFormats;
using System;
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