using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScaleDotNet.Models;

namespace ScaleDotNet.Controllers
{
    public class HomeController : Controller
    {
        private static readonly Guid UniqueId = Guid.NewGuid();

        // Get environment variables and dump them    
        private IEnumerable<KeyValuePair<string, string>> GetVars(bool all)
        {
            return from DictionaryEntry entry in Environment.GetEnvironmentVariables()
                   where (all || entry.Key.ToString().StartsWith("VCAP"))
                   select new KeyValuePair<string, string>(entry.Key.ToString(), entry.Value.ToString()); ;
        }

        public ActionResult Index(string all)
        {
            return View(new EnvironmentModel { ApplicationId = UniqueId, Variables = GetVars(all != null) });
        }

        public ActionResult About()
        {
            ViewBag.Message = "Application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact page.";

            return View();
        }
    }
}

// Must deploy from bin PARENT
//cf push scale-dotnet -s windows2012R2 --no-start -b https://github.com/cloudfoundry/binary_buildpack -p ./ -m 512M