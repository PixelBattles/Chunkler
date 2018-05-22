using System;
using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Client
{
    public interface IChunklerClient : IDisposable
    {
        Task Connect();
        Task Close();
        Task<bool> ProcessAction(BattleAction gameAction);
    }
}
