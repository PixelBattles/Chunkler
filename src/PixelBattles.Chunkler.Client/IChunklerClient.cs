using System;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Client
{
    public interface IChunklerClient : IDisposable
    {
        Task<int> ProcessChunkActionAsync(ChunkKey key, ChunkAction action);
        Task EnqueueChunkActionAsync(ChunkKey key, ChunkAction action);
        Task<ChunkState> GetChunkStateAsync(ChunkKey key);
        Task<IChunkSubscription> SubscribeOnChunkUpdateAsync(ChunkKey key, Func<ChunkUpdate, Task> onUpdate);
        Task ActivateBattleReminder(long battleId, TimeSpan refreshInterval);
    }
}