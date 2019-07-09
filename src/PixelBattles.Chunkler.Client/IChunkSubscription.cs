using System.Threading.Tasks;

namespace PixelBattles.Chunkler.Client
{
    public interface IChunkSubscription
    {
        Task CloseAsync();
    }
}