using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETaxTracker.Models.Dtos
{

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    public class InvoiceResponseError
    {
        public string status { get; set; }
        public int status_code { get; set; }
        public string message { get; set; }
        public Error error { get; set; }
    }

    public class Metadata
    {
        public string step { get; set; }
        public string status { get; set; }
        public string timestamp { get; set; }
    }
    public class Error
    {
        public List<Metadata> metadata { get; set; }
    }
    

}
