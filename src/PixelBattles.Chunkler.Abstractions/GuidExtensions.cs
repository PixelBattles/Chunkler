using System;

namespace PixelBattles.Chunkler
{
    public static class GuidExtensions
    {
        public static (long battleId, int xChunk, int yChunk) ToKeys(Guid guid)
        {
            var bytes = guid.ToByteArray();
            var battleId = BitConverter.ToInt64(bytes, 0);
            var xChunk = BitConverter.ToInt32(bytes, 8);
            var yChunk = BitConverter.ToInt32(bytes, 12);
            return (battleId, xChunk, yChunk);
        }

        public static Guid ToGuid(long battleId, int xChunk, int yChunk)
        {
            byte[] guidData = new byte[16];
            Array.Copy(BitConverter.GetBytes(battleId), guidData, 8);
            Array.Copy(BitConverter.GetBytes(xChunk), 0, guidData, 8, 4);
            Array.Copy(BitConverter.GetBytes(yChunk), 0, guidData, 12, 4);
            return new Guid(guidData);
        }
    }
}