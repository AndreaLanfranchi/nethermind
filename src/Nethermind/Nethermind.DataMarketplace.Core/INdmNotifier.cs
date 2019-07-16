using System.Threading.Tasks;
using Nethermind.DataMarketplace.Core.Domain;

namespace Nethermind.DataMarketplace.Core
{
    public interface INdmNotifier
    {
        Task NotifyAsync(Notification notification);
    }
}