using Orleans;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Grains
{
    public interface IChunkGrain : IGrainWithGuidCompoundKey
    {
        Task<bool> ProcessActionAsync(ChunkAction action);

        Task<ChunkState> GetStateAsync();

        Task Subscribe(IChunkObserver observer);
    }
}
