using BgTaxi.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Models
{
   public static class AccessTokenStaticClass
    {
     
        public static string GenerateAccessToken(string oldAccessToken)
        {
            Database db = new Database();
            var newAccessTokenString = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var oldAccTok = db.AccessTokens.Where(x => x.UniqueAccesToken == oldAccessToken).FirstOrDefault();
            if (oldAccTok == null)
            {
                return null;
            }
            oldAccTok.UniqueAccesToken = newAccessTokenString;
            oldAccTok.CreatedDateTime = DateTime.Now;
            db.SaveChanges();
            return newAccessTokenString;
        }
    }
}
