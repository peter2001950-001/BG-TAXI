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
        /// <summary>
        /// Returns all Dispatchers
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Dispatcher> GetAll()
        {
            return _data.Dispatchers.Include(x=>x.Company).AsEnumerable();
        }
        /// <summary>
        /// Remove a Dispatcher
        /// </summary>
        /// <param name="disparcher"></param>
        public void RemoveDispatcher(Dispatcher disparcher)
        {
            _data.Dispatchers.Remove(disparcher);
        }
        public void SaveChanges()
        {
            _data.SaveChanges();
        }
    }
}
