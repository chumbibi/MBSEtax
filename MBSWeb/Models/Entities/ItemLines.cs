using System.ComponentModel.DataAnnotations;

namespace MBSWeb.Models.Entities
{
    public class ItemLines
    {
        [Key]
        public int Id { get; set; }
        public int CompanyId { get; set; }

        public string CustomerCode { get; set; }

        /// <summary>
        /// SAP Invoice DocEntry
        /// </summary>
        public string DocEntry { get; set; }

        public int LineNum { get; set; }

        public string ItemCode { get; set; }

        public string ItemDescription { get; set; }

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal PriceAfterVAT { get; set; }

        public string Currency { get; set; }

        public decimal LineTotal { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
