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
        /// <summary>
        /// Adds new Device
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public bool AddDrvice(Device device)
        {
            Data.Devices.Add(device);
            Data.SaveChanges();
             return true;
        }

        /// <summary>
        /// Returs all Devices
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Device> GetAll()
        {
            return Data.Devices.AsEnumerable();
        }
        /// <summary>
        /// Returns Device by its accessToken
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
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
