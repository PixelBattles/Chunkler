using Orleans;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler
{
    public interface IChunkGrain : IGrainWithGuidCompoundKey
    {
        Task<int> ProcessActionAsync(ChunkAction action);

        Task<ChunkState> GetStateAsync();

        Task Subscribe(IChunkObserver observer);
    }
}
