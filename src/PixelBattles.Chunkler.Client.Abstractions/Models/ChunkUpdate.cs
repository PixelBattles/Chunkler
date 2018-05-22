namespace PixelBattles.Chunkler.Client
{
    public class ChunkUpdate
    {
        public ChunkKey Key { get; set; }

        public int ChangeIndex { get; set; }

        public int WidthIndex { get; set; }

        public int HeightIndex { get; set; }

        public uint Color { get; set; } 
    }
}
