using BgTaxi.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BgTaxi.Services.Contracts
{
   public interface IRequestService: IService
    {
        IEnumerable<ActiveRequest> GetActiveRequests();

        IEnumerable<TakenRequest> GetTakenRequests();
        IEnumerable<RequestHistory> GetRequestHistories();

        IEnumerable<RequestInfo> GetRequestInfos();
        ActiveRequest GetActiveRequest(int id);
        void RemoveActiveRequest(ActiveRequest request);
        TakenRequest GetTakenRequest(int id);
        RequestHistory GetRequestHistory(int id);
        RequestInfo GetRequestInfo(int id);
        void ModifyRequestInfo(RequestInfo newRequestInfo);
        void ModifyActiveRequest(ActiveRequest newActiveRequest);
        void AddRequestInfo(RequestInfo request);
        void AddActiveRequest(ActiveRequest request);
        object AppropriateRequest(Car car);
        bool UpdateAnswer(bool answer, int requestId, Driver driver);
        object CatchUpRequest(Car car);
        bool FinishRequest(int requestId, string userId);
    }
}
