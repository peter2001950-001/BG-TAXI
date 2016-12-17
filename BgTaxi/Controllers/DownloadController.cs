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
        // GET: Download
        public ActionResult Client(string fileType)
        {
            if (fileType == "apk")
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Content/appFiles/BgTaxiClient.apk"));
                string fileName = "BgTaxiClient.apk";
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            else if (fileType == "hap")
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Content/appFiles/BgTaxiClient.xap"));
                string fileName = "BgTaxiClient.xap";
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }else if (fileType == "xap"){
                byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Content/appFiles/BgTaxiClient.xap"));
                string fileName = "BgTaxiClient.xap";
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }else {
                return Redirect("http://bgtaxi.net");
            }
            
        }

        public ActionResult Sitemap()
        {
            return this.Content("~/Content/images/sitemap.xml", "text/xml");
        }

        public ActionResult Driver(string fileType)
        {
            if (fileType == "apk")
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Content/appFiles/BgTaxiDriver.apk"));
                string fileName = "BgTaxiDriver.apk";
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            else if (fileType == "hap")
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