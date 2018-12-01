using Orleans;

namespace PixelBattles.Chunkler
{
    public interface IChunkObserver : IGrainObserver
    {
        void ChunkUpdated(ChunkKey chunkKey, ChunkUpdate update);
    }
}