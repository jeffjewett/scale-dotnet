using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScaleDotNet.Models;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace ScaleDotNet.Controllers
{
    public class HomeController : Controller
    {
        private static readonly Guid UniqueId = Guid.NewGuid();
        private static String VcapServices = String.Empty;
        private static String ActualConnection = String.Empty;

        // Get environment variables and dump them    
        private IEnumerable<KeyValuePair<string, string>> GetVars(bool all)
        {
            var variables = from DictionaryEntry entry in Environment.GetEnvironmentVariables()
            where (all || entry.Key.ToString().StartsWith("VCAP"))
            select new KeyValuePair<string, string>(entry.Key.ToString(), entry.Value.ToString());

            bool found = false;

            foreach (KeyValuePair<string, string> kvp in variables)
            {
                Console.WriteLine(String.Format("Key: {0}, Value: {1}", 
                    kvp.Key.ToString(), kvp.Value.ToString()));

                if (kvp.Key == "VCAP_SERVICES")
                {
                    VcapServices = kvp.Value;

                    Debug.WriteLine(String.Format("GetVars.VCAP_SERVICES: {0}", VcapServices));
                    Console.WriteLine(String.Format("GetVars.VCAP_SERVICES: {0}", VcapServices));

                    found = true;
                    break;
                }

                // ref:
                //OUT Key: VCAP_SERVICES, Value: { "user-provided":[{"name":"pcf-sql-connection","label":"user-provided","tags":[],"credentials":{"connectionString":"Server=tcp:trkzkp608j.database.windows.net,1433;Database=ContosoUniversity2;User ID=pivotal@trkzkp608j;Password=paasword1!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;"},"syslog_drain_url":""}]}
            }

            if (!found)
            {
                Debug.WriteLine(String.Format("{0}", "VCAP_SRVICES not found"));
                Console.WriteLine(String.Format("{0}", "VCAP_SRVICES not found"));
                VcapServices = @"{'p-redis':[{'name':'pcf-redis','label':'p-redis','tags':['pivotal','redis'],'plan':'shared-vm','credentials':{'host':'10.68.44.113','password':'5cf28159-3df8-4f7a-a141-5ee93bbc99b1','port':41165}}],'user-provided':[{'name':'pcf-sql-connection','label':'user-provided','tags':[],'credentials':{'connectionString':'Server=tcp:trkzkp608j.database.windows.net,1433;Database=ContosoUniversity2;User ID=pivotal@trkzkp608j;Password=paasword1!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;'},'syslog_drain_url':''}]}";

            }
            return variables;
        }

        List<string> DoQuery(string vcapServices, string queryString)
        {
            Console.WriteLine(String.Format("DoQuery.queryString: {0}", queryString));

            List<string> results = new List<string>();

            bool found = false;

            if (!String.IsNullOrEmpty(VcapServices))
            {
                string queryResult = ParseJson(vcapServices, queryString);
                if (!String.IsNullOrEmpty(queryResult))
                    results.Add(queryResult);
                else
                    results.Add("Not Found");

                found = true;
            }

            if (!found)
            {
                results.Add("Not Found");
            }

            return results;
        }

        private string ParseJson(string vcapServices, string queryString)
        {
            char[] delims = { ',', ' ', ';' };
            string[] parameters = queryString.Split(delims);
            
            JObject o = JObject.Parse(vcapServices);
            try {
                string qs = null;
                switch (parameters.Length)
                {
                    case (1):
                        qs = String.Format(@"{0}{1}",
                              "$.",
                              parameters[0]);
                        break;
                    case (2):
                        qs = String.Format(@"{0}{1}{2}{3}{4}",
                              "$.",
                              parameters[0],
                              "[?(@name == '",
                              parameters[1],
                              "')].credentials");
                        break;
                    case (3):
                        qs = String.Format(@"{0}{1}{2}{3}{4}{5}",
                            "$.",
                            parameters[0],
                            "[?(@name == '",
                            parameters[1],
                            "')].credentials.",
                            parameters[2]);
                        break;
                    default:
                        return null;
                }

                JToken sqls = o.SelectToken(qs);
                return sqls.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Console.WriteLine(ex.Message);
                return "Not found";
            }

            // ref:
            //JObject o = JObject.Parse(@"{'user-provided':[{'name':'pcf-sql-connection','label':'user-provided','tags':[],'credentials':{'connectionString':'Server=tcp:trkzkp608j.database.windows.net,1433;Database=ContosoUniversity2;User ID=pivotal@trkzkp608j;Password=paasword1!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;'},'syslog_drain_url':''}]}");
            //JObject o = JObject.Parse(@"{'p-redis':[{'name':'pcf-redis','label':'p-redis','tags':['pivotal','redis'],'plan':'shared-vm','credentials':{'host':'10.68.44.113','password':'5cf28159-3df8-4f7a-a141-5ee93bbc99b1','port':41165}}],'user-provided':[{'name':'pcf-sql-connection','label':'user-provided','tags':[],'credentials':{'connectionString':'Server=tcp:trkzkp608j.database.windows.net,1433;Database=ContosoUniversity2;User ID=pivotal@trkzkp608j;Password=paasword1!;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;'},'syslog_drain_url':''}]}");
            //JToken sqls = o.SelectToken("$.user-provided[?(@name == 'pcf-sql-connection')].credentials.connectionString");
            //JToken redi = o.SelectToken("$.p-redis[?(@name == 'pcf-redis')].credentials");
        }

        public ActionResult Index(string all)
        {
            return View(new EnvironmentModel { ApplicationId = UniqueId, Variables = GetVars(all != null)});
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

        public ActionResult Students(string all)
        {
            return View(new EnvironmentModel { ApplicationId = UniqueId, Variables = GetVars(all != null) });
        }
        [HttpGet]
        public ViewResult QueryForm()
        {
            return View();
        }

        [HttpPost]
        public ViewResult QueryForm(QueryModel results)
        {
            results.Connection = ActualConnection;
            results.Results = DoQuery(VcapServices, results.Query);
            return View("Results", results);
        }
    }
}

// ref:
// Must deploy from bin PARENT
//cf push scale-dotnet -s windows2012R2 --no-start -b https://github.com/cloudfoundry/binary_buildpack -p ./ -m 512M