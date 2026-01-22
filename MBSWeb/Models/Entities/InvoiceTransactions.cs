using System.ComponentModel.DataAnnotations;

namespace MBSWeb.Models.Entities
{
    public class InvoiceTransactions
    {
        [Key]
        public int Id { get; set; }

        public int CompanyId { get; set; }

        public string? InvoiceNumber { get; set; }

        public DateTime? InvoiceDate { get; set; }

        public string? CustomerCode { get; set; }

        public string? CustomerName { get; set; }

        public string? CustomerAddress { get; set; }

        public decimal TotalAmount { get; set; } // was decimal

        public decimal VatSum { get; set; } // was decimal

        public string? CurrencyCode { get; set; }

        public string? Comments { get; set; }

        public int TransmitStatus { get; set; } //1 if yes, 0 if no, default 0

        public string? IRN { get; set; } // Invoice Reference Number is empty by default until transmitted successfully

        public string? QRCode { get; set; } // QR Code is a 64bitstring and is also empty by default until transmitted successfully

        public string? FirsInvoiceNumber { get; set; } // FIRS Invoice Number is empty by default until transmitted successfully
        public DateTime? IRNDate { get; set; } // Date and Time of IRN Generation

        public int ValidatedInvoice { get; set; }// 0 = Not Validated, 1 = Validated

        public int InvoicePaymentStatus { get; set; } // 0=PENDING, 1 = PAID, 2=REJECTED

        public int PaymentStatusTransmited { get; set; } //0 = not transmitted, 1 = transmitted 

        public int EmailNotificationStatus { get; set; } // 1 if yes, 0 if no, default 0
    }
}
