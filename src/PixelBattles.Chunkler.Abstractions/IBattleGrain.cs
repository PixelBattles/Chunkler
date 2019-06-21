using Orleans;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler
{
    public interface IBattleGrain : IGrainWithIntegerKey, IRemindable
    {
        Task<BattleState> GetStateAsync();
    }
}