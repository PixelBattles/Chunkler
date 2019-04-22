using System;

namespace PixelBattles.Chunkler
{
    public class ChunkKey
    {
        public long BattleId { get; set; }
        public int ChunkXIndex { get; set; }
        public int ChunkYIndex { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is ChunkKey key)
            {
                return key.BattleId == BattleId && key.ChunkXIndex == ChunkXIndex && key.ChunkYIndex == ChunkYIndex;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return BattleId.GetHashCode() ^ ChunkXIndex ^ ChunkYIndex;
        }
    }
}