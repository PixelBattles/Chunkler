using SixLabors.ImageSharp.PixelFormats;

namespace PixelBattles.Chunkler.Grains.ImageProcessing
{
    public interface IImageProcessor
    {
        Rgba32[] GetPixelsFromBytes(byte[] imageArray, int height, int width);
        byte[] GetBytesFromPixels(Rgba32[] pixelArray, int height, int width);
        Rgba32[] GetDefaultImage(int height, int width);
    }
}