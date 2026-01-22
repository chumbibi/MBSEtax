using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETaxTracker.Models
{
    public class ItemLines
    {
        public int CompanyId { get; set; }

        public string CustomerCode { get; set; }

        /// <summary>
        /// SAP Invoice DocEntry
        /// </summary>
        public string DocEntry { get; set; }

        public int LineNum { get; set; }

        public string ItemCode { get; set; }

        public string ItemDescription { get; set; }

        public double Quantity { get; set; }

        public double Price { get; set; }

        public double PriceAfterVAT { get; set; }

        public string Currency { get; set; }

        public double LineTotal { get; set; }
    }

}
