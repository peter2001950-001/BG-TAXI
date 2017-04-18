using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Models.Models
{
    public class ActiveRequest
    {
        [Key]
        public int Id { get; set; }
        public RequestInfo Request { get; set; }
        public virtual Car AppropriateCar { get; set; }

        public DateTime DateTimeChosenCar { get; set; }
    }
}
