using Orleans;
using System.Threading.Tasks;

namespace PixelBattles.Server.Grains
{
    public class ChunkGrain : Grain<ChunkGrainState>, IChunkGrain
    {
        public Task<ChunkState> GetStateAsync()
        {
            var chunkState = new ChunkState
            {
                ChangeIndex = State.ChangeIndex
            };

            return Task.FromResult(chunkState);
        }

        public async Task<bool> ProcessActionAsync(ChunkAction action)
        {
            State.ChangeIndex++;
            await WriteStateAsync();
            return true;
        }
    }
}
