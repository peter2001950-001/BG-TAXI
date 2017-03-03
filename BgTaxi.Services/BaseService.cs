using BgTaxi.Models;
using BgTaxi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Services
{
    public class BaseService : IService
    {
        private readonly IDatabase data;
        public BaseService(IDatabase data)
        {
            this.data = data;
        }
        public void SaveChanges()
        {
            data.SaveChanges();
        }
    }
}
