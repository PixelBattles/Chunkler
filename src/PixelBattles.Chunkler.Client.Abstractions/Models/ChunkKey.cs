using System;

namespace PixelBattles.Chunkler.Client
{
    public class ChunkKey
    {
        public Guid BattleId { get; set; }

        public int ChunkXIndex { get; set; }

        public int ChunkYIndex { get; set; }
    }
}
