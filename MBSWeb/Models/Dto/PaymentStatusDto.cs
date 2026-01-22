using System.Text.Json.Serialization;

namespace MBSWeb.Models.Dto
{
    public class PaymentStatusDto
    {
         
        [JsonPropertyName("payment_Status")]  
        public PaymentStatus PaymentStatus { get; set; } //0=PENDING,1=PAID,2=REJECTED

        [JsonPropertyName("reference")]
        public string? Reference { get; set; }

    }

    public enum PaymentStatus
    {
        Pending = 0,
        Paid = 1,
        Rejected = 2
    }

}
