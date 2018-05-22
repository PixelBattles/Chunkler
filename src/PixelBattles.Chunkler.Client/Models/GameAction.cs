using System;

namespace PixelBattles.Chunkler.Client
{
    public class BattleAction
    {
        public Guid BattleId { get; set; }

        public int ChunkXIndex { get; set; }

        public int ChunkYIndex { get; set; }

        public int XIndex { get; set; }

        public int YIndex { get; set; }

        public uint Color { get; set; }
    }
}
