using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace BgTaxi.Controllers
{
    public class DownloadController : Controller
    {

        /// <summary>
        /// Download path for downloading the driver application
        /// </summary>
        /// <param name="fileType"></param>
        /// <returns></returns>
        public ActionResult Driver(string fileType)
        {
            if (fileType == "apk")
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Content/appFiles/BgTaxiDriver.apk"));
                string fileName = "BgTaxiDriver.apk";
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            else if (fileType == "xap")
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Content/appFiles/BgTaxiDriver.xap"));
                string fileName = "BgTaxiDriver.xap";
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            else if (fileType == "xap")
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Content/appFiles/BgTaxiDriver.xap"));
                string fileName = "BgTaxiDriver.xap";
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            else
            {
                return Redirect("http://bgtaxi.net");
            }

        }
    }
}