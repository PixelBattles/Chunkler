using System;

namespace PixelBattles.Chunkler
{
    public class ChunkKey
    {
        public Guid BattleId { get; set; }
        public int ChunkXIndex { get; set; }
        public int ChunkYIndex { get; set; }
    }
}