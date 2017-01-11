using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Models.Models
{
   public class RequestInfo
    {
        [Key]
        public int Id { get; set; }
        public Location StartingLocation { get; set; }
        public string StartingAddress { get; set; }
        public Location FinishLocation { get; set; }
        public string FinishAddress { get; set; }
        public CreatedBy CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string CreatorUserId { get; set; }
        public RequestStatusEnum RequestStatus { get; set; }
        public DateTime LastModificationDateTime { get; set; }
    }

    public enum RequestStatusEnum
    {
        NotTaken,
        Taken,
        Dismissed,
        NoCarChosen,
        OnAddress,
        ClientConnected,
        MissingClient,
        Finishted,

    }

    public enum CreatedBy
    {
        Dispatcher,
        Client
    }
}
