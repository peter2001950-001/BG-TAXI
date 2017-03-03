using BgTaxi.Models;
using BgTaxi.Models.Models;
using BgTaxi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Services
{
    public class DispatcherService: IDispatcherService
    {
        private readonly IDatabase data;
        public  DispatcherService(IDatabase database)
        {
            this.data = database; 
        }

        public void AddDispatcher(Dispatcher dispatcher)
        {
            data.Dispatchers.Add(dispatcher);
            data.SaveChanges();
        }

        public IEnumerable<Dispatcher> GetAll()
        {
            return data.Dispatchers.AsEnumerable();
        }

        public void SaveChanges()
        {
            data.SaveChanges();
        }
    }
}
