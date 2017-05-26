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
        void AddDispatcher(Dispatcher dispatcher);
        void RemoveDispatcher(Dispatcher dispatcher);

    }
}
