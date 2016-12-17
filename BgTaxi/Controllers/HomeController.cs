using BgTaxi.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace BgTaxi.Controllers
{


    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToRoute("Profile");
            }
            return View();
        }
        
        public ActionResult Drivers()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View("../ Manage / Index");
            }
            return View();
        }

        public ActionResult Companies()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View("../ Manage / Index");
            }
            return View();
        }


    }
}
