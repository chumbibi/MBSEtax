
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class FirsInvoiceRequest
{
    [JsonPropertyName("note")]
    public string Note { get; set; }

    [JsonPropertyName("due_date")]
    public string DueDate { get; set; }

    [JsonPropertyName("tax_total")]
    public List<TaxTotal> TaxTotal { get; set; }

    [JsonPropertyName("issue_date")]
    public string IssueDate { get; set; }

    [JsonPropertyName("business_id")]
    public string BusinessId { get; set; }

    [JsonPropertyName("invoice_line")]
    public List<InvoiceLine> InvoiceLine { get; set; }

    [JsonPropertyName("payment_means")]
    public List<PaymentMean> PaymentMeans { get; set; }

    [JsonPropertyName("payment_status")]
    public string? PaymentStatus { get; set; }

    [JsonPropertyName("invoice_number")]
    public string InvoiceNumber { get; set; }

    [JsonPropertyName("tax_point_date")]
    public string TaxPointDate { get; set; }

    [JsonPropertyName("accounting_cost")]
    public string AccountingCost { get; set; }

    [JsonPropertyName("buyer_reference")]
    public string BuyerReference { get; set; }

    [JsonPropertyName("order_reference")]
    public string OrderReference { get; set; }

    [JsonPropertyName("allowance_charge")]
    public List<AllowanceCharge> AllowanceCharge { get; set; }

    [JsonPropertyName("invoice_type_code")]
    public string InvoiceTypeCode { get; set; }

    [JsonPropertyName("tax_currency_code")]
    public string TaxCurrencyCode { get; set; }

    [JsonPropertyName("payment_terms_note")]
    public string PaymentTermsNote { get; set; }

    [JsonPropertyName("actual_delivery_date")]
    public string ActualDeliveryDate { get; set; }

    [JsonPropertyName("legal_monetary_total")]
    public LegalMonetaryTotal LegalMonetaryTotal { get; set; }

    [JsonPropertyName("document_currency_code")]
    public string DocumentCurrencyCode { get; set; }

    [JsonPropertyName("invoice_delivery_period")]
    public InvoiceDeliveryPeriod InvoiceDeliveryPeriod { get; set; }

    [JsonPropertyName("accounting_customer_party")]
    public AccountingCustomerParty AccountingCustomerParty { get; set; }

    [JsonPropertyName("accounting_supplier_party")]
    public AccountingSupplierParty AccountingSupplierParty { get; set; }
}

public class AccountingCustomerParty
{
    [JsonPropertyName("tin")]
    public string Tin { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("telephone")]
    public string Telephone { get; set; }

    [JsonPropertyName("party_name")]
    public string PartyName { get; set; }

    [JsonPropertyName("postal_address")]
    public PostalAddress PostalAddress { get; set; }

    [JsonPropertyName("business_description")]
    public string BusinessDescription { get; set; }
}

public class AccountingSupplierParty
{
    [JsonPropertyName("tin")]
    public string Tin { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("telephone")]
    public string Telephone { get; set; }

    [JsonPropertyName("party_name")]
    public string PartyName { get; set; }

    [JsonPropertyName("postal_address")]
    public PostalAddress PostalAddress { get; set; }

    [JsonPropertyName("business_description")]
    public string BusinessDescription { get; set; }
}

public class PostalAddress
{
    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; }     

    [JsonPropertyName("city_name")]
    public string CityName { get; set; }

    [JsonPropertyName("postal_zone")]
    public string PostalZone { get; set; }

    [JsonPropertyName("street_name")]
    public string StreetName { get; set; }
}

public class InvoiceDeliveryPeriod
{
    [JsonPropertyName("start_date")]
    public string StartDate { get; set; }

    [JsonPropertyName("end_date")]
    public string EndDate { get; set; }
}

public class InvoiceLine
{
    [JsonPropertyName("item")]
    public Item Item { get; set; }

    [JsonPropertyName("price")]
    public Price Price { get; set; }

    [JsonPropertyName("fee_rate")]
    public double FeeRate { get; set; }

    [JsonPropertyName("hsn_code")]
    public string HsnCode { get; set; }

    [JsonPropertyName("fee_amount")]
    public double FeeAmount { get; set; }

    [JsonPropertyName("discount_rate")]
    public double DiscountRate { get; set; }

    [JsonPropertyName("discount_amount")]
    public double DiscountAmount { get; set; }

    [JsonPropertyName("product_category")]
    public string ProductCategory { get; set; }

    [JsonPropertyName("invoiced_quantity")]
    public int InvoicedQuantity { get; set; }

    [JsonPropertyName("line_extension_amount")]
    public double LineExtensionAmount { get; set; }
}

public class Item
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("sellers_item_identification")]
    public string SellersItemIdentification { get; set; }
}

public class Price
{
    [JsonPropertyName("price_unit")]
    public string PriceUnit { get; set; }

    [JsonPropertyName("price_amount")]
    public double PriceAmount { get; set; }

    [JsonPropertyName("base_quantity")]
    public int BaseQuantity { get; set; }
}

public class LegalMonetaryTotal
{
    [JsonPropertyName("payable_amount")]
    public double PayableAmount { get; set; }

    [JsonPropertyName("tax_exclusive_amount")]
    public double TaxExclusiveAmount { get; set; }

    [JsonPropertyName("tax_inclusive_amount")]
    public double TaxInclusiveAmount { get; set; }

    [JsonPropertyName("line_extension_amount")]
    public double LineExtensionAmount { get; set; }
}

 

public class PaymentMean
{
    [JsonPropertyName("payment_due_date")]
    public string PaymentDueDate { get; set; }

    [JsonPropertyName("payment_means_code")]
    public string PaymentMeansCode { get; set; }
}



public class AllowanceCharge
{
    [JsonPropertyName("amount")]
    public double Amount { get; set; }

    [JsonPropertyName("charge_indicator")]
    public bool ChargeIndicator { get; set; }
}

public class TaxTotal
{
    [JsonPropertyName("tax_amount")]
    public double TaxAmount { get; set; }

    [JsonPropertyName("tax_subtotal")]
    public List<TaxSubtotal> TaxSubtotal { get; set; }
}

public class TaxSubtotal
{
    [JsonPropertyName("tax_amount")]
    public double TaxAmount { get; set; }

    [JsonPropertyName("tax_category")]
    public TaxCategory TaxCategory { get; set; }

    [JsonPropertyName("taxable_amount")]
    public double TaxableAmount { get; set; }
}

public class TaxCategory
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("percent")]
    public double Percent { get; set; }
}



//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Text.Json.Serialization;
//using System.Threading.Tasks;


//namespace ETaxTracker.Models.Dtos
//{
//    public class FirsInvoiceRequest
//    {
//        [JsonPropertyName("_document_reference")]
//        public List<DocumentReference>? DocumentReference { get; set; }

//        [JsonPropertyName("accounting_cost")]
//        public string? AccountingCost { get; set; }

//        [JsonPropertyName("accounting_customer_party")]
//        public Party? AccountingCustomerParty { get; set; }

//        [JsonPropertyName("accounting_supplier_party")]
//        public Party? AccountingSupplierParty { get; set; }

//        [JsonPropertyName("actual_delivery_date")]
//        public string? ActualDeliveryDate { get; set; }

//        [JsonPropertyName("allowance_charge")]
//        public List<AllowanceCharge>? AllowanceCharge { get; set; }

//        [JsonPropertyName("billing_reference")]
//        public List<DocumentReference>? BillingReference { get; set; }

//        [JsonPropertyName("business_id")]
//        public string? BusinessId { get; set; }

//        [JsonPropertyName("buyer_reference")]
//        public string? BuyerReference { get; set; }

//        [JsonPropertyName("contract_document_reference")]
//        public DocumentReference? ContractDocumentReference { get; set; }

//        [JsonPropertyName("dispatch_document_reference")]
//        public DocumentReference? DispatchDocumentReference { get; set; }

//        [JsonPropertyName("document_currency_code")]
//        public string? DocumentCurrencyCode { get; set; }

//        [JsonPropertyName("due_date")]
//        public string? DueDate { get; set; }

//        [JsonPropertyName("invoice_delivery_period")]
//        public DeliveryPeriod? InvoiceDeliveryPeriod { get; set; }

//        [JsonPropertyName("invoice_line")]
//        public List<InvoiceLine>? InvoiceLine { get; set; }

//        [JsonPropertyName("invoice_number")]
//        public string? InvoiceNumber { get; set; }

//        [JsonPropertyName("invoice_type_code")]
//        public string? InvoiceTypeCode { get; set; }

//        [JsonPropertyName("irn")]
//        public string? Irn { get; set; }

//        [JsonPropertyName("issue_date")]
//        public string? IssueDate { get; set; }

//        [JsonPropertyName("issue_time")]
//        public string? IssueTime { get; set; }

//        [JsonPropertyName("legal_monetary_total")]
//        public LegalMonetaryTotal? LegalMonetaryTotal { get; set; }

//        [JsonPropertyName("note")]
//        public string? Note { get; set; }

//        [JsonPropertyName("order_reference")]
//        public string? OrderReference { get; set; }

//        [JsonPropertyName("originator_document_reference")]
//        public DocumentReference? OriginatorDocumentReference { get; set; }

//        [JsonPropertyName("payee_party")]
//        public Party? PayeeParty { get; set; }

//        [JsonPropertyName("payment_means")]
//        public List<PaymentMeans>? PaymentMeans { get; set; }

//        [JsonPropertyName("payment_status")]
//        public string? PaymentStatus { get; set; }

//        [JsonPropertyName("payment_terms_note")]
//        public string? PaymentTermsNote { get; set; }

//        [JsonPropertyName("receipt_document_reference")]
//        public DocumentReference? ReceiptDocumentReference { get; set; }

//        [JsonPropertyName("tax_currency_code")]
//        public string? TaxCurrencyCode { get; set; }

//        [JsonPropertyName("tax_point_date")]
//        public string? TaxPointDate { get; set; }

//        [JsonPropertyName("tax_representative_party")]
//        public Party? TaxRepresentativeParty { get; set; }

//        [JsonPropertyName("tax_total")]
//        public List<TaxTotal>? TaxTotal { get; set; }
//    }

//    public class DocumentReference
//    {
//        [JsonPropertyName("irn")]
//        public string? Irn { get; set; }

//        [JsonPropertyName("issue_date")]
//        public string? IssueDate { get; set; }
//    }

//    public class Party
//    {
//        [JsonPropertyName("business_description")]
//        public string? BusinessDescription { get; set; }

//        [JsonPropertyName("email")]
//        public string? Email { get; set; }

//        [JsonPropertyName("party_name")]
//        public string? PartyName { get; set; }

//        [JsonPropertyName("postal_address")]
//        public PostalAddress? PostalAddress { get; set; }

//        [JsonPropertyName("telephone")]
//        public string? Telephone { get; set; }

//        [JsonPropertyName("tin")]
//        public string? Tin { get; set; }
//    }

//    public class PostalAddress
//    {
//        [JsonPropertyName("city_name")]
//        public string? CityName { get; set; }

//        [JsonPropertyName("country")]
//        public string? Country { get; set; }

//        [JsonPropertyName("country_code")]
//        public string? CountryCode { get; set; }

//        [JsonPropertyName("postal_zone")]
//        public string? PostalZone { get; set; }

//        [JsonPropertyName("street_name")]
//        public string? StreetName { get; set; }
//    }

//    public class InvoiceLine
//    {
//        [JsonPropertyName("discount_amount")]
//        public decimal DiscountAmount { get; set; }

//        [JsonPropertyName("discount_rate")]
//        public decimal DiscountRate { get; set; }

//        [JsonPropertyName("fee_amount")]
//        public decimal FeeAmount { get; set; }

//        [JsonPropertyName("fee_rate")]
//        public decimal FeeRate { get; set; }

//        [JsonPropertyName("hsn_code")]
//        public string? HsnCode { get; set; }

//        [JsonPropertyName("invoiced_quantity")]
//        public int InvoicedQuantity { get; set; }

//        [JsonPropertyName("item")]
//        public InvoiceItem? Item { get; set; }

//        [JsonPropertyName("line_extension_amount")]
//        public decimal LineExtensionAmount { get; set; }

//        [JsonPropertyName("price")]
//        public Price? Price { get; set; }

//        [JsonPropertyName("product_category")]
//        public string? ProductCategory { get; set; }
//    }

//    public class InvoiceItem
//    {
//        [JsonPropertyName("description")]
//        public string? Description { get; set; }

//        [JsonPropertyName("name")]
//        public string? Name { get; set; }

//        [JsonPropertyName("sellers_item_identification")]
//        public string? SellersItemIdentification { get; set; }
//    }

//    public class Price
//    {
//        [JsonPropertyName("base_quantity")]
//        public int BaseQuantity { get; set; }

//        [JsonPropertyName("price_amount")]
//        public decimal PriceAmount { get; set; }

//        [JsonPropertyName("price_unit")]
//        public string? PriceUnit { get; set; }
//    }
//    public class LegalMonetaryTotal
//    {
//        [JsonPropertyName("line_extension_amount")]
//        public decimal LineExtensionAmount { get; set; }

//        [JsonPropertyName("payable_amount")]
//        public decimal PayableAmount { get; set; }

//        [JsonPropertyName("tax_exclusive_amount")]
//        public decimal TaxExclusiveAmount { get; set; }

//        [JsonPropertyName("tax_inclusive_amount")]
//        public decimal TaxInclusiveAmount { get; set; }
//    }

//    public class PaymentMeans
//    {
//        [JsonPropertyName("payment_due_date")]
//        public string? PaymentDueDate { get; set; }

//        [JsonPropertyName("payment_means_code")]
//        public string? PaymentMeansCode { get; set; }
//    }

//    public class TaxTotal
//    {
//        [JsonPropertyName("tax_amount")]
//        public decimal TaxAmount { get; set; }

//        [JsonPropertyName("tax_subtotal")]
//        public List<TaxSubtotal>? TaxSubtotal { get; set; }
//    }

//    public class TaxSubtotal
//    {
//        [JsonPropertyName("taxable_amount")]
//        public decimal TaxableAmount { get; set; }
//        [JsonPropertyName("tax_amount")]
//        public decimal TaxAmount { get; set; }       

//        [JsonPropertyName("tax_category")]
//        public TaxCategory? TaxCategory { get; set; }
//    }

//    public class TaxCategory
//    {
//        [JsonPropertyName("id")]
//        public string? Id { get; set; }

//        [JsonPropertyName("percent")]
//        public decimal Percent { get; set; }
//    }

//    public class AllowanceCharge
//    {
//        [JsonPropertyName("amount")]
//        public decimal Amount { get; set; }

//        [JsonPropertyName("charge_indicator")]
//        public bool ChargeIndicator { get; set; }
//    }

//    public class DeliveryPeriod
//    {
//        [JsonPropertyName("start_date")]
//        public string? StartDate { get; set; }

//        [JsonPropertyName("end_date")]
//        public string? EndDate { get; set; }
//    }

//    //public sealed class FirsInvoiceRequest
//    //{
//    //    [JsonPropertyName("_document_reference")]
//    //    public List<DocumentReference>? DocumentReference { get; set; }

//    //    [JsonPropertyName("accounting_cost")]
//    //    public string? AccountingCost { get; set; }

//    //    [JsonPropertyName("accounting_customer_party")]
//    //    public Party? AccountingCustomerParty { get; set; }

//    //    [JsonPropertyName("accounting_supplier_party")]
//    //    public Party? AccountingSupplierParty { get; set; }

//    //    [JsonPropertyName("actual_delivery_date")]
//    //    public string? ActualDeliveryDate { get; set; }

//    //    [JsonPropertyName("allowance_charge")]
//    //    public List<AllowanceCharge>? AllowanceCharge { get; set; }

//    //    [JsonPropertyName("billing_reference")]
//    //    public List<DocumentReference>? BillingReference { get; set; }

//    //    [JsonPropertyName("business_id")]
//    //    public string? BusinessId { get; set; }

//    //    [JsonPropertyName("buyer_reference")]
//    //    public string? BuyerReference { get; set; }

//    //    [JsonPropertyName("contract_document_reference")]
//    //    public DocumentReference? ContractDocumentReference { get; set; }

//    //    [JsonPropertyName("dispatch_document_reference")]
//    //    public DocumentReference? DispatchDocumentReference { get; set; }

//    //    [JsonPropertyName("document_currency_code")]
//    //    public string? DocumentCurrencyCode { get; set; }

//    //    [JsonPropertyName("due_date")]
//    //    public string? DueDate { get; set; }

//    //    [JsonPropertyName("invoice_delivery_period")]
//    //    public DeliveryPeriod? InvoiceDeliveryPeriod { get; set; }

//    //    [JsonPropertyName("invoice_line")]
//    //    public List<InvoiceLine>? InvoiceLine { get; set; }

//    //    [JsonPropertyName("invoice_number")]
//    //    public string? InvoiceNumber { get; set; }

//    //    [JsonPropertyName("invoice_type_code")]
//    //    public string? InvoiceTypeCode { get; set; }

//    //    [JsonPropertyName("irn")]
//    //    public string? Irn { get; set; }

//    //    [JsonPropertyName("issue_date")]
//    //    public string? IssueDate { get; set; }

//    //    [JsonPropertyName("issue_time")]
//    //    public string? IssueTime { get; set; }

//    //    [JsonPropertyName("legal_monetary_total")]
//    //    public LegalMonetaryTotal? LegalMonetaryTotal { get; set; }

//    //    [JsonPropertyName("note")]
//    //    public string? Note { get; set; }

//    //    [JsonPropertyName("order_reference")]
//    //    public string? OrderReference { get; set; }

//    //    [JsonPropertyName("originator_document_reference")]
//    //    public DocumentReference? OriginatorDocumentReference { get; set; }

//    //    [JsonPropertyName("payee_party")]
//    //    public Party? PayeeParty { get; set; }

//    //    [JsonPropertyName("payment_means")]
//    //    public List<PaymentMeans>? PaymentMeans { get; set; }

//    //    [JsonPropertyName("payment_status")]
//    //    public string? PaymentStatus { get; set; }

//    //    [JsonPropertyName("payment_terms_note")]
//    //    public string? PaymentTermsNote { get; set; }

//    //    [JsonPropertyName("receipt_document_reference")]
//    //    public DocumentReference? ReceiptDocumentReference { get; set; }

//    //    [JsonPropertyName("tax_currency_code")]
//    //    public string? TaxCurrencyCode { get; set; }

//    //    [JsonPropertyName("tax_point_date")]
//    //    public string? TaxPointDate { get; set; }

//    //    [JsonPropertyName("tax_representative_party")]
//    //    public Party? TaxRepresentativeParty { get; set; }

//    //    [JsonPropertyName("tax_total")]
//    //    public List<TaxTotal>? TaxTotal { get; set; }
//    //}

//    //public class DocumentReference
//    //{
//    //    [JsonPropertyName("irn")]
//    //    public string? Irn { get; set; }

//    //    [JsonPropertyName("issue_date")]
//    //    public string? IssueDate { get; set; }
//    //}

//    //public class Party
//    //{
//    //    [JsonPropertyName("business_description")]
//    //    public string? BusinessDescription { get; set; }

//    //    [JsonPropertyName("email")]
//    //    public string? Email { get; set; }

//    //    [JsonPropertyName("party_name")]
//    //    public string? PartyName { get; set; }

//    //    [JsonPropertyName("postal_address")]
//    //    public PostalAddress? PostalAddress { get; set; }

//    //    [JsonPropertyName("telephone")]
//    //    public string? Telephone { get; set; }

//    //    [JsonPropertyName("tin")]
//    //    public string? Tin { get; set; }
//    //}
//    //public class PostalAddress
//    //{
//    //    [JsonPropertyName("city_name")]
//    //    public string? CityName { get; set; }

//    //    [JsonPropertyName("country")]
//    //    public string? Country { get; set; }

//    //    [JsonPropertyName("country_code")]
//    //    public string? CountryCode { get; set; }

//    //    [JsonPropertyName("postal_zone")]
//    //    public string? PostalZone { get; set; }

//    //    [JsonPropertyName("street_name")]
//    //    public string? StreetName { get; set; }
//    //}

//    //public class AllowanceCharge
//    //{
//    //    [JsonPropertyName("amount")]
//    //    public decimal Amount { get; set; }

//    //    [JsonPropertyName("charge_indicator")]
//    //    public bool ChargeIndicator { get; set; }
//    //}
//    //public class DeliveryPeriod
//    //{
//    //    [JsonPropertyName("start_date")]
//    //    public string? StartDate { get; set; }

//    //    [JsonPropertyName("end_date")]
//    //    public string? EndDate { get; set; }
//    //}

//    //public class InvoiceLine
//    //{
//    //    [JsonPropertyName("discount_amount")]
//    //    public decimal DiscountAmount { get; set; }

//    //    [JsonPropertyName("discount_rate")]
//    //    public decimal DiscountRate { get; set; }

//    //    [JsonPropertyName("fee_amount")]
//    //    public decimal FeeAmount { get; set; }

//    //    [JsonPropertyName("fee_rate")]
//    //    public decimal FeeRate { get; set; }

//    //    [JsonPropertyName("hsn_code")]
//    //    public string? HsnCode { get; set; }

//    //    [JsonPropertyName("invoiced_quantity")]
//    //    public decimal InvoicedQuantity { get; set; }

//    //    [JsonPropertyName("item")]
//    //    public InvoiceItem? Item { get; set; }

//    //    [JsonPropertyName("line_extension_amount")]
//    //    public decimal LineExtensionAmount { get; set; }

//    //    [JsonPropertyName("price")]
//    //    public Price? Price { get; set; }

//    //    [JsonPropertyName("product_category")]
//    //    public string? ProductCategory { get; set; }
//    //}

//    //public class InvoiceItem
//    //{
//    //    [JsonPropertyName("description")]
//    //    public string? Description { get; set; }

//    //    [JsonPropertyName("name")]
//    //    public string? Name { get; set; }

//    //    [JsonPropertyName("sellers_item_identification")]
//    //    public string? SellersItemIdentification { get; set; }
//    //}
//    //public class Price
//    //{
//    //    [JsonPropertyName("base_quantity")]
//    //    public decimal BaseQuantity { get; set; }

//    //    [JsonPropertyName("price_amount")]
//    //    public decimal PriceAmount { get; set; }

//    //    [JsonPropertyName("price_unit")]
//    //    public string? PriceUnit { get; set; }
//    //}

//    //public class LegalMonetaryTotal
//    //{
//    //    [JsonPropertyName("line_extension_amount")]
//    //    public decimal LineExtensionAmount { get; set; }

//    //    [JsonPropertyName("payable_amount")]
//    //    public decimal PayableAmount { get; set; }

//    //    [JsonPropertyName("tax_exclusive_amount")]
//    //    public decimal TaxExclusiveAmount { get; set; }

//    //    [JsonPropertyName("tax_inclusive_amount")]
//    //    public decimal TaxInclusiveAmount { get; set; }
//    //}
//    //public class PaymentMeans
//    //{
//    //    [JsonPropertyName("payment_due_date")]
//    //    public string? PaymentDueDate { get; set; }

//    //    [JsonPropertyName("payment_means_code")]
//    //    public string? PaymentMeansCode { get; set; }
//    //}
//    //public class TaxSubtotal
//    //{
//    //    [JsonPropertyName("tax_amount")]
//    //    public decimal TaxAmount { get; set; }

//    //    [JsonPropertyName("tax_category")]
//    //    public TaxCategory? TaxCategory { get; set; }

//    //    [JsonPropertyName("taxable_amount")]
//    //    public decimal TaxableAmount { get; set; }
//    //}
//    //public class TaxCategory
//    //{
//    //    [JsonPropertyName("id")]
//    //    public string? Id { get; set; }

//    //    [JsonPropertyName("percent")]
//    //    public decimal Percent { get; set; }
//    //}

//    //public class TaxTotal
//    //{
//    //    [JsonPropertyName("tax_amount")]
//    //    public decimal TaxAmount { get; set; }

//    //    [JsonPropertyName("tax_subtotal")]
//    //    public List<TaxSubtotal>? TaxSubtotal { get; set; }
//    //}

//}
