using BgTaxi.Models;
using BgTaxi.Models.Models;
using BgTaxi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Services
{
    public class DeviceService: IDeviceService
    {
        public readonly IDatabase data;
        public DeviceService(IDatabase data)
        {
            this.data = data;
        }
        public bool AddDrvice(Device device)
        {
            data.Devices.Add(device);
            data.SaveChanges();
             return true;
        }

        public IEnumerable<Device> GetAll()
        {
            return data.Devices.AsEnumerable();
        }
        public Device GetDeviceByAccessToken(string accessToken)
        {
            var accTok = data.AccessTokens.Where(x => x.UniqueAccesToken == accessToken).Include(x => x.Device).First();
            return accTok.Device;
        }

        public void SaveChanges()
        {
            data.SaveChanges();
        }
    }
}
