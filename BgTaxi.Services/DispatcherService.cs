using BgTaxi.Models;
using BgTaxi.Models.Models;
using BgTaxi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace BgTaxi.Services
{
    public class DispatcherService: IDispatcherService
    {
        private readonly IDatabase _data;
        public  DispatcherService(IDatabase database)
        {
            this._data = database; 
        }

        /// <summary>
        /// Adds a Dispatcher
        /// </summary>
        /// <param name="dispatcher"></param>
        public void AddDispatcher(Dispatcher dispatcher)
        {
            _data.Dispatchers.Add(dispatcher);
            _data.SaveChanges();
        }

        public void AddOnlineDispatcher(string userId, string connectionId)
        {

            var dispatcher = _data.Dispatchers.Where(x => x.UserId == userId).FirstOrDefault();
            if (dispatcher != null) {
                _data.OnlineDispatchers.Add(new OnlineDispatcher()
                {
                    ConnectionId = connectionId,
                    Dispatcher = dispatcher
                });
                _data.SaveChanges();
            }
        }

        /// <summary>
        /// Returns all Dispatchers
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Dispatcher> GetAll()
        {
            return _data.Dispatchers.Include(x=>x.Company).AsEnumerable();
        }

        public IEnumerable<OnlineDispatcher> GetAllOnlineDispatchers()
        {
            return _data.OnlineDispatchers.Include(x => x.Dispatcher).AsEnumerable();
        }

        /// <summary>
        /// Remove a Dispatcher
        /// </summary>
        /// <param name="disparcher"></param>
        public void RemoveDispatcher(Dispatcher disparcher)
        {
            _data.Dispatchers.Remove(disparcher);
        }

        public void RemoveOnlineDispatcher(string connectionId)
        {
           var dispatcher =  _data.OnlineDispatchers.Where(x => x.ConnectionId == connectionId).FirstOrDefault();
            if(dispatcher != null)
            {
                _data.OnlineDispatchers.Remove(dispatcher);
                _data.SaveChanges();
            }
        }

        public void SaveChanges()
        {
            _data.SaveChanges();
        }
    }
}
