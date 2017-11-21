using BgTaxi.Models.Models;
using BgTaxi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Services.Contracts
{
    public interface IDispatcherService : IService
    {
        IEnumerable<Dispatcher> GetAll();
        IEnumerable<OnlineDispatcher> GetAllOnlineDispatchers();
        void AddOnlineDispatcher(string userId, string connectionId);
        void RemoveOnlineDispatcher(string connectionId);
        void AddDispatcher(Dispatcher dispatcher);
        void RemoveDispatcher(Dispatcher dispatcher);

    }
}
