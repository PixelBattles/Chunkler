using System;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Client
{
    public interface IChunklerClient
    {
        Task Connect();
        Task Close();
        Task<int> ProcessAction(ChunkKey key, ChunkAction action);
        Task<ChunkState> GetChunkState(ChunkKey key);
        Task SubscribeAsync(ChunkKey key, Action<ChunkUpdate> onUpdate);
        Task Unsubscribe(ChunkKey key);
    }
}