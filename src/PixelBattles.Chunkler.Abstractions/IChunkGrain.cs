using Orleans;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler
{
    public interface IChunkGrain : IGrainWithGuidKey
    {
        Task<int> ProcessActionAsync(ChunkAction action);
        Task<ChunkState> GetStateAsync();
    }
}