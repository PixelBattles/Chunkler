using System;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Client
{
    public interface IChunklerClient : IDisposable
    {
        Task<int> ProcessActionAsync(ChunkKey key, ChunkAction action);
        Task EnqueueActionAsync(ChunkKey key, ChunkAction action);
        Task<ChunkState> GetChunkStateAsync(ChunkKey key);
        Task SubscribeOnUpdateAsync(ChunkKey key, Action<ChunkUpdate> onUpdate);
        Task UnsubscribeOnUpdateAsync(ChunkKey key);
    }
}