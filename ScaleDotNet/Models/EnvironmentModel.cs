using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScaleDotNet.Models
{ 
    public class EnvironmentModel
    {
        public Guid ApplicationId { get; set; }

        public IEnumerable<KeyValuePair<string, string>> Variables { get; set; }
        public List<string> Students { get; set; }
    }
}