using System;

namespace PixelBattles.Chunkler.Client
{
    public class GameAction
    {
        public Guid GameId { get; set; }

        public int ChunkXIndex { get; set; }

        public int ChunkYIndex { get; set; }

        public int XIndex { get; set; }

        public int YIndex { get; set; }

        public int Color { get; set; }
    }
}
