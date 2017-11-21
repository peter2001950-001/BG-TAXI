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
    public class AccessTokenService : IAccessTokenService
    {
        private readonly IDatabase data;

        public AccessTokenService(IDatabase data)
        {
            this.data = data;
        }

        /// <summary>
        /// Returns the userId of the logged in user or null if nobody has been logged in
        /// </summary>
        /// <param name="accessToken"></param>
        public string GetUserId(string accessToken)
        {
            var accessTokObj = this.data.AccessTokens.Where(x => x.UniqueAccesToken == accessToken).Include(x => x.Device).FirstOrDefault();
            if (accessTokObj == null)
            {
                return null;
            }
            return accessTokObj.Device.UserId;

        }
        /// <summary>
        /// Adds a new accessToken 
        /// </summary>
        /// <param name="accessToken"></param>
        public void AddAccessToken(AccessToken accessToken)
        {
            this.data.AccessTokens.Add(accessToken);
            this.data.SaveChanges();
        }

        /// <summary>
        /// Returns All AccessTokens
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AccessToken> GetAll()
        {
            return data.AccessTokens.AsEnumerable();
        }
        /// <summary>
        /// Checks if a user has been logged in
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public bool IsUserLoggedIn(string accessToken)
        {
            var accTok = data.AccessTokens.Where(x => x.UniqueAccesToken == accessToken).Include(x=>x.Device).FirstOrDefault();
            if (accTok != null)
            {
                var device = data.Devices.FirstOrDefault(x => x.Id == accTok.Device.Id);
                if (device?.UserId != null)
                {
                    return true;
                }
            }
            return false;
            }

        public void LogoutUser(string accessToken)
        {
            var accTok = data.AccessTokens.Where(x => x.UniqueAccesToken == accessToken).Include(x => x.Device).FirstOrDefault();
            if (accTok != null)
            {
                var device = data.Devices.FirstOrDefault(x => x.Id == accTok.Device.Id);
                if (device?.UserId != null)
                {
                    device.UserId = null;
                    data.SaveChanges();
                }
            }
        }
        /// <summary>
        /// Add userId to the device of the accessToken
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="userId"></param>
        public bool AddDeviceUserId(string accessToken, string userId)
        {
            
            var accTok = data.AccessTokens.Where(x => x.UniqueAccesToken == accessToken).Include(x => x.Device).First();
            if (accTok != null)
            {
                accTok.Device.UserId = userId;
                accTok.Device.LastRequestDateTime = DateTime.Now;
                data.SaveChanges();
                return true;
            }
            return false;
        }
        /// <summary>
        /// Generates a new accessToken
        /// </summary>
        /// <param name="oldAccessToken"></param>
        /// <returns></returns>
        public string GenerateAccessToken(string oldAccessToken)
        {
            var oldAccTok = data.AccessTokens.Where(x => x.UniqueAccesToken == oldAccessToken).Include(x => x.Device).FirstOrDefault();
            if (oldAccTok == null)
            {
                var previousAccTok = data.AccessTokens.Where(x => x.PreviousUniqueAccessToken == oldAccessToken).Include(x => x.Device).FirstOrDefault();
                if (previousAccTok == null)
                {
                    return null;
                }
                else
                {

                    previousAccTok.Device.LastRequestDateTime = DateTime.Now;
                    data.SaveChanges();
                    return previousAccTok.UniqueAccesToken;
                }
            }

            var newAccessTokenString = Guid.NewGuid().ToString("D");
            oldAccTok.PreviousUniqueAccessToken = oldAccTok.UniqueAccesToken;
            oldAccTok.UniqueAccesToken = newAccessTokenString;
            oldAccTok.CreatedDateTime = DateTime.Now;
            oldAccTok.Device.LastRequestDateTime = DateTime.Now;
            data.SaveChanges();
            return newAccessTokenString;
        }

        public void SaveChanges()
        {
            data.SaveChanges();
        }
    }
}
