using System.Windows.Threading;
using ReactiveTables.Framework;

namespace ReactiveTables.Demo.Services
{
    public interface IBrokerFeedDataService
    {
        IReactiveTable Feeds { get; }
        void Start(Dispatcher dispatcher);
        void Stop(); 
    }
}
