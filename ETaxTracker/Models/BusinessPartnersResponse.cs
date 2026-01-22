using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETaxTracker.Models
{
    using System.Text.Json.Serialization;

    //public class BusinessPartnersResponse
    //{
    //    [JsonPropertyName("@odata.context")]
    //    public string ODataContext { get; set; }

    //    [JsonPropertyName("value")]
    //    public List<BusinessPartner> Value { get; set; }
    //}

    public class BusinessPartnersResponse
    {
        public List<BusinessPartner> Value { get; set; }

        [JsonPropertyName("@odata.nextLink")]
        public string NextLink { get; set; }
    }

    public class BusinessPartner
    {
        [JsonPropertyName("@odata.etag")]
        public string ODataEtag { get; set; }

        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string Address { get; set; }
        public string Phone1 { get; set; }
        public string FederalTaxID { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string EmailAddress { get; set; }
        public string BusinessType { get; set; }
    }

}

