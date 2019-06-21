using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace PixelBattles.Chunkler.Grains.ImageProcessing
{
    public interface IImageProcessor
    {
        Rgba32[] GetPixelsFromBytes(byte[] imageArray, int height, int width);
        byte[] GetBytesFromPixels(Rgba32[] pixelArray, int height, int width);
        Rgba32[] GetDefaultImage(int height, int width, uint color = 16777215);
        byte[] GenerateImageFromChunks(IEnumerable<(int x, int y, byte[] image)> chunks, int chunkHeight, int chunkWidth, int previewHeight, int previewWidth, int centerX, int centerY, uint color = 16777215);
    }
}