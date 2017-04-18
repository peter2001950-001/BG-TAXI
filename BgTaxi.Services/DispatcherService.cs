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

        public void AddDispatcher(Dispatcher dispatcher)
        {
            _data.Dispatchers.Add(dispatcher);
            _data.SaveChanges();
        }

        public IEnumerable<Dispatcher> GetAll()
        {
            return _data.Dispatchers.Include(x=>x.Company).AsEnumerable();
        }
        public IEnumerable<DispatcherDashboard> GetAllDashboards()
        {
            return _data.DispatchersDashboard.AsEnumerable();
        }

        public void SaveChanges()
        {
            _data.SaveChanges();
        }
    }
}
