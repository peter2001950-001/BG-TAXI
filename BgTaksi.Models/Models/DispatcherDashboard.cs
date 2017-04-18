using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Models.Models
{
    public class DispatcherDashboard
    {
        [Key]
        public int Id { get; set; }
        public string DispatcherUserId { get; set; }
        public virtual RequestInfo Request { get; set; }
        public DateTime LastSeen { get; set; }
        public RequestStatusEnum LastSeenStatus { get; set; }
    }
}
