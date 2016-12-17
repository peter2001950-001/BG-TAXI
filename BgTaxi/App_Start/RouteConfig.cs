using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BgTaxi.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Download",
                url: "download/{action}/{fileType}",
                defaults: new { controller = "Download", action = "Client", filetype = UrlParameter.Optional }
                );
            routes.MapRoute(
                name: "Profile",
                url: "Profile/{action}",
                defaults: new { controller = "Manage", action = "Index" }
                );
            routes.MapRoute(
               name: "AddCompanyCode",
               url: "externalManage/AddCompanyCode",
               defaults: new { controller = "Request", action = "AddCompanyCode" }
               );
            //routes.MapRoute(
            //    name: "CreateRequest",
            //    url: "request/createRequest",
            //    defaults: new { controller = "Request", action = "CreateRequest" }
            //    );
            routes.MapRoute(
               name: "RequestInfo",
               url: "request/requestInfo",
               defaults: new { controller = "Request", action = "GetInfoForRequest" }
               );
            routes.MapRoute(
                name: "GetAppropriateRequest",
                url: "request/approRequest",
                defaults: new { controller = "Request", action = "GetAppropriateRequest" }
                );
           // routes.MapRoute(
           //name: "RequestStatus",
           //url: "request/requestStatus",
           //defaults: new { controller = "Request", action = "RequestStatus" }
           //);
           // routes.MapRoute(
           //name: "CreateNewRequest",
           //url: "request/createNewRequest",
           //defaults: new { controller = "Request", action = "CreateNewRequest" }
           //);
           // routes.MapRoute(
           //name: "TakeRequest",
           //url: "request/takeRequest",
           //defaults: new { controller = "Request", action = "TakeRequest" }
           //);
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
