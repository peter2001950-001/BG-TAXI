using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Models.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; }
        public string RegisterNumber { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string InternalNumber { get; set; }
        public Location Location { get; set; }
        public CarStatus CarStatus { get; set; }

        public virtual Company Company { get; set; }
        public DateTime LastActiveDateTime { get; set; }
    }

    public enum CarStatus
    {
        Free,
        Busy,
        Absent,
        OffDuty,
        Offline
        
    }
}
