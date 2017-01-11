using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Models.Models
{
    public class TakenRequests
    {
        [Key]
        public int Id { get; set; }
        public RequestInfo Request { get; set; }
        public DateTime DateTimeTaken { get; set; }
        public Car Car { get; set; }
        public string DriverUserId { get; set; }
        public Location DriverLocation{ get; set; }
        public string DuractionText { get; set; }
        public int DuractionValue { get; set; }
        public string DistanceText { get; set; }
        public int DistanceValue { get; set; }
        public Company Company { get; set; }
        public bool UserInformed { get; set; }
        public bool OnAddress { get; set; }
    }
}
