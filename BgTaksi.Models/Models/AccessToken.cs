using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Models.Models
{
    public class AccessToken
    {
        [Key]
        public int Id { get; set; }
        public string UniqueAccesToken { get; set; }
        public Device Device { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
