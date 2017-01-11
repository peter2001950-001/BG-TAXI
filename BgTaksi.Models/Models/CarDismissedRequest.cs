using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Models.Models
{
    public class CarDismissedRequest
    {
        public int Id { get; set; }
        public RequestInfo Request { get; set; }
        public Car Car { get; set; }
    }
}
