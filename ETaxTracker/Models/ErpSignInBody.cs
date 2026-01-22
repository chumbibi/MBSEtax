using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ETaxTracker.Models
{
    public class ErpSignInBody
    {
        [JsonPropertyName("CompanyDB")]
        public string CompanyDB { get; set; }

        [JsonPropertyName("UserName")]
        public string UserName { get; set; }

        [JsonPropertyName("Password")]
        public string Password { get; set; }

    }
}
