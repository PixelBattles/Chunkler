using Orleans;

namespace PixelBattles.Chunkler.Grains
{
    public interface IChunkObserver : IGrainObserver
    {
        void ChunkUpdated(int updateIndex);
    }
}
