using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ETaxTracker.Models
{
    public class InvoiceTransactionResponse
    {
        [JsonPropertyName("@odata.context")]
        public string ODataContext { get; set; }

        [JsonPropertyName("value")]
        public List<Invoice> Value { get; set; }

        [JsonPropertyName("@odata.nextLink")]
        public string NextLink { get; set; }
    }

    public class Invoice
    {
       [JsonPropertyName("@odata.etag")]
        public string ODataEtag { get; set; }

        public int DocEntry { get; set; }

        public DateTime DocDate { get; set; }

        public string CardCode { get; set; }

        public string CardName { get; set; }

        public string Address { get; set; }

        public double DocTotal { get; set; } // was decimal

        public string Comments { get; set; }

        public double VatSum { get; set; } // was decimal

        public string CurrencyCode { get; set; }

        // Navigation Property
        public List<DocumentLine> DocumentLines { get; set; }
    }

    public class DocumentLine
    {
        public int LineNum { get; set; }
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public double Quantity { get; set; }
        public object ShipDate { get; set; }
        public double Price { get; set; }
        public double PriceAfterVAT { get; set; }
        public string Currency { get; set; }
        public double Rate { get; set; }
        public double DiscountPercent { get; set; }
        public string VendorNum { get; set; }
        public object SerialNum { get; set; }
        public string WarehouseCode { get; set; }
        public int SalesPersonCode { get; set; }
        public double CommisionPercent { get; set; }
        public string TreeType { get; set; }
        public string AccountCode { get; set; }
        public string UseBaseUnits { get; set; }
        public string SupplierCatNum { get; set; }
        public string CostingCode { get; set; }
        public string ProjectCode { get; set; }
        public object BarCode { get; set; }
        public string VatGroup { get; set; }
        public double Height1 { get; set; }
        public object Hight1Unit { get; set; }
        public double Height2 { get; set; }
        public object Height2Unit { get; set; }
        public double Lengh1 { get; set; }
        public object Lengh1Unit { get; set; }
        public double Lengh2 { get; set; }
        public object Lengh2Unit { get; set; }
        public double Weight1 { get; set; }
        public object Weight1Unit { get; set; }
        public double Weight2 { get; set; }
        public object Weight2Unit { get; set; }
        public double Factor1 { get; set; }
        public double Factor2 { get; set; }
        public double Factor3 { get; set; }
        public double Factor4 { get; set; }
        public int BaseType { get; set; }
        public object BaseEntry { get; set; }
        public object BaseLine { get; set; }
        public double Volume { get; set; }
        public int VolumeUnit { get; set; }
        public double Width1 { get; set; }
        public object Width1Unit { get; set; }
        public double Width2 { get; set; }
        public object Width2Unit { get; set; }
        public string Address { get; set; }
        public object TaxCode { get; set; }
        public string TaxType { get; set; }
        public string TaxLiable { get; set; }
        public string PickStatus { get; set; }
        public double PickQuantity { get; set; }
        public object PickListIdNumber { get; set; }
        public string OriginalItem { get; set; }
        public object BackOrder { get; set; }
        public string FreeText { get; set; }
        public int ShippingMethod { get; set; }
        public object POTargetNum { get; set; }
        public string POTargetEntry { get; set; }
        public object POTargetRowNum { get; set; }
        public string CorrectionInvoiceItem { get; set; }
        public double CorrInvAmountToStock { get; set; }
        public double CorrInvAmountToDiffAcct { get; set; }
        public double AppliedTax { get; set; }
        public double AppliedTaxFC { get; set; }
        public double AppliedTaxSC { get; set; }
        public string WTLiable { get; set; }
        public string DeferredTax { get; set; }
        public double EqualizationTaxPercent { get; set; }
        public double TotalEqualizationTax { get; set; }
        public double TotalEqualizationTaxFC { get; set; }
        public double TotalEqualizationTaxSC { get; set; }
        public double NetTaxAmount { get; set; }
        public double NetTaxAmountFC { get; set; }
        public double NetTaxAmountSC { get; set; }
        public string MeasureUnit { get; set; }
        public double UnitsOfMeasurment { get; set; }
        public double LineTotal { get; set; }
        public double TaxPercentagePerRow { get; set; }
        public double TaxTotal { get; set; }
        public string ConsumerSalesForecast { get; set; }
        public double ExciseAmount { get; set; }
        public double TaxPerUnit { get; set; }
        public double TotalInclTax { get; set; }
        public object CountryOrg { get; set; }
        public string SWW { get; set; }
        public object TransactionType { get; set; }
        public string DistributeExpense { get; set; }
        public double RowTotalFC { get; set; }
        public double RowTotalSC { get; set; }
        public double LastBuyInmPrice { get; set; }
        public double LastBuyDistributeSumFc { get; set; }
        public double LastBuyDistributeSumSc { get; set; }
        public double LastBuyDistributeSum { get; set; }
        public double StockDistributesumForeign { get; set; }
        public double StockDistributesumSystem { get; set; }
        public double StockDistributesum { get; set; }
        public double StockInmPrice { get; set; }
        public string PickStatusEx { get; set; }
        public double TaxBeforeDPM { get; set; }
        public double TaxBeforeDPMFC { get; set; }
        public double TaxBeforeDPMSC { get; set; }
        public object CFOPCode { get; set; }
        public object CSTCode { get; set; }
        public object Usage { get; set; }
        public string TaxOnly { get; set; }
        public int VisualOrder { get; set; }
        public double BaseOpenQuantity { get; set; }
        public double UnitPrice { get; set; }
        public string LineStatus { get; set; }
        public double PackageQuantity { get; set; }
        public string Text { get; set; }
        public string LineType { get; set; }
        public string COGSCostingCode { get; set; }
        public string COGSAccountCode { get; set; }
        public string ChangeAssemlyBoMWarehouse { get; set; }
        public double GrossBuyPrice { get; set; }
        public int GrossBase { get; set; }
        public double GrossProfitTotalBasePrice { get; set; }
        public object CostingCode2 { get; set; }
        public object CostingCode3 { get; set; }
        public object CostingCode4 { get; set; }
        public object CostingCode5 { get; set; }
        public object ItemDetails { get; set; }
        public object LocationCode { get; set; }
        public DateTime ActualDeliveryDate { get; set; }
        public double RemainingOpenQuantity { get; set; }
        public double OpenAmount { get; set; }
        public double OpenAmountFC { get; set; }
        public double OpenAmountSC { get; set; }
        public object ExLineNo { get; set; }
        public object RequiredDate { get; set; }
        public double RequiredQuantity { get; set; }
        public object COGSCostingCode2 { get; set; }
        public object COGSCostingCode3 { get; set; }
        public object COGSCostingCode4 { get; set; }
        public object COGSCostingCode5 { get; set; }
        public object CSTforIPI { get; set; }
        public object CSTforPIS { get; set; }
        public object CSTforCOFINS { get; set; }
        public object CreditOriginCode { get; set; }
        public string WithoutInventoryMovement { get; set; }
        public object AgreementNo { get; set; }
        public object AgreementRowNumber { get; set; }
        public object ActualBaseEntry { get; set; }
        public object ActualBaseLine { get; set; }
        public int DocEntry { get; set; }
        public double Surpluses { get; set; }
        public double DefectAndBreakup { get; set; }
        public double Shortages { get; set; }
        public string ConsiderQuantity { get; set; }
        public string PartialRetirement { get; set; }
        public double RetirementQuantity { get; set; }
        public double RetirementAPC { get; set; }
        public string ThirdParty { get; set; }
        public object PoNum { get; set; }
        public object PoItmNum { get; set; }
        public object ExpenseType { get; set; }
        public object ReceiptNumber { get; set; }
        public object ExpenseOperationType { get; set; }
        public object FederalTaxID { get; set; }
        public double GrossProfit { get; set; }
        public double GrossProfitFC { get; set; }
        public double GrossProfitSC { get; set; }
        public string PriceSource { get; set; }
        public object StgSeqNum { get; set; }
        public object StgEntry { get; set; }
        public object StgDesc { get; set; }
        public int UoMEntry { get; set; }
        public string UoMCode { get; set; }
        public double InventoryQuantity { get; set; }
        public double RemainingOpenInventoryQuantity { get; set; }
        public object ParentLineNum { get; set; }
        public int Incoterms { get; set; }
        public int TransportMode { get; set; }
        public object NatureOfTransaction { get; set; }
        public object DestinationCountryForImport { get; set; }
        public object DestinationRegionForImport { get; set; }
        public object OriginCountryForExport { get; set; }
        public object OriginRegionForExport { get; set; }
        public string ItemType { get; set; }
        public string ChangeInventoryQuantityIndependently { get; set; }
        public string FreeOfChargeBP { get; set; }
        public object SACEntry { get; set; }
        public object HSNEntry { get; set; }
        public double GrossPrice { get; set; }
        public double GrossTotal { get; set; }
        public double GrossTotalFC { get; set; }
        public double GrossTotalSC { get; set; }
        public int NCMCode { get; set; }
        public object NVECode { get; set; }
        public string IndEscala { get; set; }
        public object CtrSealQty { get; set; }
        public object CNJPMan { get; set; }
        public object CESTCode { get; set; }
        public object UFFiscalBenefitCode { get; set; }
        public string ReverseCharge { get; set; }
        public object ShipToCode { get; set; }
        public string ShipToDescription { get; set; }
        public object OwnerCode { get; set; }
        public double ExternalCalcTaxRate { get; set; }
        public double ExternalCalcTaxAmount { get; set; }
        public double ExternalCalcTaxAmountFC { get; set; }
        public double ExternalCalcTaxAmountSC { get; set; }
        public int StandardItemIdentification { get; set; }
        public int CommodityClassification { get; set; }
        public object WeightOfRecycledPlastic { get; set; }
        public object PlasticPackageExemptionReason { get; set; }
        public object LegalText { get; set; }
        public object Cig { get; set; }
        public object Cup { get; set; }
        public object OperatingProfit { get; set; }
        public object OperatingProfitFC { get; set; }
        public object OperatingProfitSC { get; set; }
        public object NetIncome { get; set; }
        public object NetIncomeFC { get; set; }
        public object NetIncomeSC { get; set; }
        public object CSTforIBS { get; set; }
        public object CSTforCBS { get; set; }
        public object CSTforIS { get; set; }
        public object UnencumberedReason { get; set; }
        public string CUSplit { get; set; }
        public object ListNum { get; set; }
        public object RecognizedTaxCode { get; set; }
        public object U_VHC2 { get; set; }
        public object U_Uom2 { get; set; }
        public object U_LIssDat { get; set; }
        public object U_CustName { get; set; }
        public object U_PurPose { get; set; }
        public object U_BPLoc { get; set; }
        public object U_Reib { get; set; }
        public List<object> LineTaxJurisdictions { get; set; }
        public List<object> ExportProcesses { get; set; }
        public List<object> EBooksDetails { get; set; }
        public List<object> DocLinePickLists { get; set; }
        public List<object> DocumentLineAdditionalExpenses { get; set; }
        public List<object> WithholdingTaxLines { get; set; }
        public List<object> SerialNumbers { get; set; }
        public List<object> BatchNumbers { get; set; }
        public List<object> CCDNumbers { get; set; }
        public List<object> DocumentLinesBinAllocations { get; set; }
    }



}



