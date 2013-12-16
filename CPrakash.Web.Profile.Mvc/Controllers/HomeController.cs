using CPrakash.Web.Profile.Mvc.Models;
using CPrakash.Web.Profile.Mvc.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CPrakash.Web.Profile.Mvc.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            Profile profile = new Profile(new ProfileService());

            var firstName = profile.Properties.FirstName;
            var lastName = profile.Properties.LastName;
            var name = profile.Properties.Name;
            if (string.IsNullOrWhiteSpace(name))
                profile.Properties.Name = string.Format("Hello Mr.!!! {0} {1}", firstName, lastName);

            var address = profile.Properties.Address;
            if (address == null)
                address = new Address { Street = "Jefferson", Unit = "Line1", City = "Louisville", State = "KY", ZipCode = "40220" };
            profile.Properties.Address = address;
            profile.Save();
            
            return View(profile);
        }
    }
}
