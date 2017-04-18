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
        public readonly IDatabase Data;
        public DeviceService(IDatabase data)
        {
            this.Data = data;
        }
        public bool AddDrvice(Device device)
        {
            Data.Devices.Add(device);
            Data.SaveChanges();
             return true;
        }

        public IEnumerable<Device> GetAll()
        {
            return Data.Devices.AsEnumerable();
        }
        public Device GetDeviceByAccessToken(string accessToken)
        {
            var accTok = Data.AccessTokens.Where(x => x.UniqueAccesToken == accessToken).Include(x => x.Device).First();
            return accTok.Device;
        }

        public void SaveChanges()
        {
            Data.SaveChanges();
        }
    }
}
