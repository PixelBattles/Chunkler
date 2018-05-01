using Orleans;
using System.Threading.Tasks;

namespace PixelBattles.Server.Grains
{
    public interface IChunkGrain : IGrainWithGuidCompoundKey
    {
        Task<bool> ProcessActionAsync(ChunkAction action);

        Task<ChunkState> GetStateAsync();
    }
}
