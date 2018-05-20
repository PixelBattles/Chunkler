using Orleans;
using Orleans.Providers;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Grains
{
    [StorageProvider(ProviderName = "MemoryStore")]
    public class ChunkGrain : Grain<ChunkGrainState>, IChunkGrain
    {
        private readonly GrainObserverManager<IChunkObserver> observers = new GrainObserverManager<IChunkObserver>();

        public Task Subscribe(IChunkObserver observer)
        {
            observers.Subscribe(observer);
            return Task.FromResult(0);
        }

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
