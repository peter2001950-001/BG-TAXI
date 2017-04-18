using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Models.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EIK { get; set; }
        public string DDS { get; set; }
        public string City { get; set; }
        public string MOL { get; set; }
        public string UserId { get; set; }
        public string UniqueNumber { get; set; }
        public  Location CityLocation { get; set; }


    }
}
