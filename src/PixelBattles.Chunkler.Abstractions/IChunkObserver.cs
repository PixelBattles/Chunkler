using Orleans;

namespace PixelBattles.Chunkler
{
    public interface IChunkObserver : IGrainObserver
    {
        void ChunkUpdated(int updateIndex);
    }
}