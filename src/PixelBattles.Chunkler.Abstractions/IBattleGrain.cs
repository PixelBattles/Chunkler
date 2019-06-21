using Orleans;
using System;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler
{
    public interface IBattleGrain : IGrainWithIntegerKey, IRemindable
    {
        Task<BattleState> GetStateAsync();
        Task ActivateBattleReminderAsync(TimeSpan refreshInterval);
    }
}