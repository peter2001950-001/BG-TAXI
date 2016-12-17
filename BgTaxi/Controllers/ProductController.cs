using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BgTaxi.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult DriverApp()
        {
            return View();
        }
        public ActionResult CompanyApp()
        {
            return View();
        }

    }
}