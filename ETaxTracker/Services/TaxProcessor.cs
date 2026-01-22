
using Azure.Core;
using ETaxTracker.Models;
using ETaxTracker.Models.Dtos;
using log4net;
using Microsoft.Data.SqlClient;
using MSMQ.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
//using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ETaxTracker.Services
{
    public class TaxProcessor
    {
        private int threadCount;
        public bool isApplicationProcessing;

        //MethodBase.GetCurrentMethod().DeclaringType
        private static readonly ILog _log4net = LogManager.GetLogger(typeof(CustomerProcessor));

        public TaxProcessor(bool _isApplicationProcessing)
        {
            isApplicationProcessing = _isApplicationProcessing;
        }

        private void CustomerHandler(object? source)
        {
            Companies company = (Companies)source;

            _log4net.Info("Customer Handler started for Company: " + company.CompanyName);



            while (isApplicationProcessing)
            {
                string NumberToProcess = ConfigurationManager.AppSettings["NumberOfInvoicesToProcess"];

                try
                {

                    //Authentication on APP FIRS Portal // 07056970-8540
                    var credentials = new AuthCredentials
                    {
                        Email = "thenew@info.com",
                        Password = "password123"
                    };

                    var loginUrl = "https://einvoice.gention.tech/api/v1/auth/login";

                    using var client = new HttpClient();

                    var jsonPayload = JsonSerializer.Serialize(credentials);
                    using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    using var request = new HttpRequestMessage(HttpMethod.Post, loginUrl)
                    {
                        Content = content
                    };

                    using var response = client.SendAsync(request).Result;
                    response.EnsureSuccessStatusCode();

                    var json = response.Content.ReadAsStringAsync().Result; _log4net.Info(json.ToString());
                    if (json.ToString().ToLower().Contains("error"))
                    {
                        var err = JsonSerializer.Deserialize<APPErrorResponse>(
                                    json,
                                    new JsonSerializerOptions
                                    {
                                        PropertyNameCaseInsensitive = true
                                    });

                        _log4net.Error($"{company.CompanyName}: Authentication failed.{err}");


                        Thread.Sleep(300000);
                        continue;
                    }
                    else
                    {
                        //Login was successful, deserialize the response and extract the token
                        var page = JsonSerializer.Deserialize<APPLoginResponse>(
                                    json,
                                    new JsonSerializerOptions
                                    {
                                        PropertyNameCaseInsensitive = true
                                    });



                        _log4net.Info($"{company.CompanyName}: Authentication successful. Token: {page.Data.AccessToken}");

                        string accessToken = page.Data.AccessToken;
                        // Use the token for subsequent requests

                        // The supplier entity

                        company.TIN = "07056970-8540";
                        company.Street = "33 Saka Tinubu Street";
                        company.PostalZone = "101233";
                        company.CompanyFIRSServiceNumber = page.Data.User.ServiceId;
                        company.CompanyAddress = "33 Saka Tinubu Street, Victoria Island, Lagos";
                        company.ActiveStatus = 1;


                        string connectionstring = ConfigurationManager.ConnectionStrings["connectionstring"].ConnectionString;

                        SqlConnection con = new SqlConnection(connectionstring);
                        con.Open();
                        SqlCommand cmd = null;
                        List<InvoiceTransactions> invTransactions = new List<InvoiceTransactions>();
                        if (con.State == System.Data.ConnectionState.Open)
                        {
                            // One invice that is not yet transmitted 
                            cmd = new SqlCommand($"SELECT top {NumberToProcess} [Id],[CompanyId],[InvoiceNumber],[InvoiceDate],[CustomerCode],[CustomerName],[CustomerAddress],[TotalAmount],[VatSum],[CurrencyCode],[Comments],[TransmitStatus],[IRN],[QRCode],[FirsInvoiceNumber],[IRNDate],[ValidatedInvoice],[EmailNotificationStatus] FROM InvoiceTransactions WHERE [TransmitStatus]=0 ORDER BY [InvoiceDate],[InvoiceNumber]", con);
                            SqlDataReader reader = cmd.ExecuteReader();
                            if (reader.HasRows == true)
                            {
                                while (reader.Read() == true)
                                {
                                    InvoiceTransactions invoice = new InvoiceTransactions();
                                    invoice.Id = int.Parse(reader.GetValue(0).ToString());
                                    invoice.CompanyId = reader.GetString(1);
                                    invoice.InvoiceNumber = reader.GetString(2);
                                    invoice.InvoiceDate = reader.GetDateTime(3);
                                    invoice.CustomerCode = reader.GetString(4);
                                    invoice.CustomerName = reader.GetString(5);
                                    invoice.CustomerAddress = reader.GetString(6);
                                    invoice.TotalAmount = double.Parse(reader.GetValue(7).ToString());
                                    invoice.VatSum = double.Parse(reader.GetValue(8).ToString());
                                    invoice.CurrencyCode = reader.IsDBNull(9) ? "NGN" : reader.GetString(9);
                                    invoice.Comments = reader.GetString(10);
                                    invoice.TransmitStatus = int.Parse(reader.GetValue(11).ToString());
                                    invoice.IRN = reader.IsDBNull(12) ? "" : reader.GetString(12);
                                    invoice.QRCode = reader.IsDBNull(13) ? "" : reader.GetString(13);

                                    invoice.FirsInvoiceNumber = reader.IsDBNull(14) ? $"INV{reader.GetString(14).PadLeft(6, '0')}" : reader.GetString(14);

                                    invoice.IRNDate = reader.IsDBNull(15) ? DateTime.Parse("1900-01-01T00:00:00Z") : reader.GetDateTime(15);
                                    invoice.ValidatedInvoice = reader.IsDBNull(16) ? 0 : reader.GetInt32(16);
                                    invoice.EmailNotificationStatus = reader.IsDBNull(17) ? 0 : int.Parse(reader.GetValue(17).ToString());
                                    invTransactions.Add(invoice);

                                }

                            }

                            reader.Close();
                            reader.DisposeAsync();
                            cmd.Dispose();
                        }
                        else
                        {
                            _log4net.Error($"DB Connection failed for Company: {company.CompanyName}");

                            continue;

                        }

                        HybridDictionary invoiceResults = new HybridDictionary();

                        foreach (var invoice in invTransactions)
                        {
                            _log4net.Info($"Begin processing Invoice: {invoice.InvoiceNumber} for Company: {company.CompanyName}");



                            // ====================================================
                            // 1. Who is the customer?
                            // ====================================================
                            cmd = new SqlCommand("SELECT top 1 [CompanyId],[CustomerCode],[CustomerName],[BusinessDescription],[Email],[CustomerAddress],[City],[Country],[PostalZone],[Street],[Telephone],[TIN],[ActiveStatus],[CountryCode] FROM [Customers] WHERE [CustomerCode]=@CustomerCode", con);
                            cmd.Parameters.AddWithValue("@CustomerCode", invoice.CustomerCode);
                            SqlDataReader reader = cmd.ExecuteReader();

                            Customers customer = new Customers();

                            string invNo = $"INV{DateTime.Now.ToString("yyMMddhhmmss")}";
                            // ---- Customer
                            if (reader.HasRows == true)
                            {
                                if (reader.Read() == true) // This is just one customer, so no need for while loop
                                {
                                    customer.CompanyId = reader.IsDBNull(0) ? null : reader.GetString(0);
                                    customer.CustomerCode = reader.IsDBNull(1) ? null : reader.GetString(1);
                                    customer.CustomerName = reader.IsDBNull(2) ? null : reader.GetString(2);
                                    customer.BusinessDescription = reader.IsDBNull(3) ? null : reader.GetString(3);
                                    customer.Email = reader.IsDBNull(4) ? null : reader.GetString(4);
                                    customer.CustomerAddress = reader.IsDBNull(5) ? null : reader.GetString(5);
                                    customer.City = reader.IsDBNull(6) ? null : reader.GetString(6);
                                    customer.Country = reader.IsDBNull(7) ? null : reader.GetString(7);                     
                             
                                    customer.PostalZone = reader.IsDBNull(8) ? null : reader.GetString(8);
                                    customer.Street = reader.IsDBNull(9) ? null : reader.GetString(9);
                                    customer.Telephone = reader.IsDBNull(10) ? null : reader.GetString(10);

                                    switch (customer.Country.ToUpper())
                                    {
                                        case "NG":
                                        case "NIGERIA":
                                            customer.Telephone = NormalizeToE164(customer.Telephone, "234");
                                            break;

                                        default:
                                            customer.Telephone = NormalizeToE164(customer.Telephone);
                                            break;
                                    }

                                    customer.CountryCode = reader.IsDBNull(13) ? null : reader.GetString(13);
                                    // Business rule: always use Company TIN
                                    customer.TIN = company.TIN;
                                    // ActiveStatus is non-nullable int
                                    customer.ActiveStatus = reader.IsDBNull(12) ? 0 : reader.GetInt32(12);

                                }
                            }
                            reader.Close();
                            cmd.Dispose();

                            // Select the items the customer purchased from ItemLines table
                            List<ItemLines> itemLines = new List<ItemLines>();
                            if (customer.CountryCode.Length > 0)
                            {
                                // There an invoice that is not yet transmitted
                                //There is a customer for this invoice
                                //Then there should be at lease one item line for this invoice

                                cmd = new SqlCommand("SELECT [CompanyId],[CustomerCode],[DocEntry],[LineNum],[ItemCode],[ItemDescription],[Quantity],[Price],[PriceAfterVAT],[Currency],[LineTotal] FROM [ItemLines] WHERE [CompanyId] = @CompanyId and [CustomerCode]= @CustomerCode and [DocEntry]=@DocEntry ORDER BY [LineNum]", con);

                                cmd.Parameters.AddWithValue("@CompanyId", invoice.CompanyId);
                                cmd.Parameters.AddWithValue("@CustomerCode", invoice.CustomerCode);
                                cmd.Parameters.AddWithValue("@DocEntry", invoice.InvoiceNumber);

                                reader = cmd.ExecuteReader();
                                if (reader.HasRows == true)
                                {
                                    while (reader.Read() == true)
                                    {
                                        ItemLines itemLine = new ItemLines();

                                        itemLine.CompanyId = reader.IsDBNull(0) ? 0 : int.Parse(reader.GetValue(0).ToString());

                                        itemLine.CustomerCode = reader.IsDBNull(1)
                                            ? string.Empty
                                            : reader.GetString(1);

                                        itemLine.DocEntry = reader.IsDBNull(2)
                                            ? string.Empty
                                            : reader.GetString(2);

                                        itemLine.LineNum = reader.IsDBNull(3)
                                            ? 0
                                            : int.Parse(reader.GetValue(3).ToString());

                                        itemLine.ItemCode = reader.IsDBNull(4)
                                            ? string.Empty
                                            : reader.GetString(4);

                                        itemLine.ItemDescription = reader.IsDBNull(5)
                                            ? string.Empty
                                            : reader.GetString(5);

                                        itemLine.Quantity = reader.IsDBNull(6)
                                            ? 0
                                            : double.Parse(reader.GetValue(6).ToString());

                                        itemLine.Price = reader.IsDBNull(7)
                                            ? 0
                                            : double.Parse(reader.GetValue(7).ToString());

                                        itemLine.PriceAfterVAT = reader.IsDBNull(8)
                                            ? 0
                                            : double.Parse(reader.GetValue(8).ToString());

                                        itemLine.Currency = reader.IsDBNull(9)
                                            ? string.Empty
                                            : reader.GetString(9);

                                        itemLine.LineTotal = reader.IsDBNull(10)
                                            ? 0
                                            : double.Parse(reader.GetValue(10).ToString());

                                        itemLines.Add(itemLine);

                                    }
                                }

                                reader.Close();
                                cmd.Dispose();

                            }


                            // ====================================================
                            // 2. FIRS INVOICE REQUEST
                            // ====================================================

                            FirsInvoiceRequest firsInv = new FirsInvoiceRequest();

                            //--------------------------------
                            // Supplier Party (Accounting Supplier)
                            // ----------------------------------------------------
                            AccountingSupplierParty supplierParty = new AccountingSupplierParty();
                            supplierParty.PartyName = company.CompanyName;
                            supplierParty.BusinessDescription = company.BusinessDescription;
                            
                            
                            supplierParty.Email = company.Email;
                            supplierParty.Telephone = company.Telephone;
                           
                            supplierParty.Tin = company.TIN;

                            PostalAddress supplierAddress = new PostalAddress();
                            supplierAddress.StreetName = company.Street;
                            supplierAddress.CityName = company.City;
                            supplierAddress.Country = company.Country;
                            supplierAddress.CountryCode = company.Country;

                            switch (supplierAddress.Country.ToUpper())
                            {       
                                case "NG":
                                  supplierParty.Telephone = NormalizeToE164(supplierParty.Telephone,"234");
                                break;

                                default:
                                   supplierParty.Telephone = NormalizeToE164(supplierParty.Telephone);
                                break;
                            }
                            

                            supplierAddress.PostalZone = company.PostalZone;

                            supplierParty.PostalAddress = supplierAddress;
                            firsInv.AccountingSupplierParty = supplierParty;

                            // ----------------------------------------------------
                            // Customer Party (Accounting Customer + Payee)
                            // ----------------------------------------------------
                            AccountingCustomerParty customerParty = new AccountingCustomerParty();
                            customerParty.PartyName = customer.CustomerName;
                            customerParty.BusinessDescription = customer.BusinessDescription;
                            customerParty.Email = customer.Email;
                            customerParty.Telephone = customer.Telephone;
                            customerParty.Telephone = NormalizeToNgE164(customerParty.Telephone);
                            customerParty.Tin = customer.TIN;

                            PostalAddress customerAddress = new PostalAddress();
                            customerAddress.StreetName = customer.Street;
                            customerAddress.CityName = customer.City;
                            customerAddress.Country = customer.Country;
                            customerAddress.CountryCode = customer.Country;
                            customerAddress.PostalZone = customer.PostalZone;

                            customerParty.PostalAddress = customerAddress;
                            firsInv.AccountingCustomerParty = customerParty;


                            // ----------------------------------------------------
                            // Invoice Line - this is based on each item purchased
                            // loop through itemLines
                            // ----------------------------------------------------

                            //Begin loop through itemLines
                            firsInv.InvoiceLine = new List<InvoiceLine>();
                            firsInv.TaxTotal = new List<TaxTotal>();
                            TaxSubtotal subtot = new TaxSubtotal();

                            List<TaxSubtotal> subtots = new List<TaxSubtotal>();

                            foreach (var itemLine in itemLines)
                            {
                                InvoiceLine firsLine = new InvoiceLine();
                                firsLine.InvoicedQuantity = int.Parse(itemLine.Quantity.ToString()); // MUST be int
                                firsLine.LineExtensionAmount = itemLine.LineTotal;
                                firsLine.ProductCategory = itemLine.ItemDescription;
                                firsLine.DiscountAmount = 0;
                                firsLine.DiscountRate = 0;
                                firsLine.FeeAmount = 1;
                                firsLine.FeeRate = 1;


                                var description = itemLine.ItemDescription.ToLower();

                                if (description.Contains("advertising") || description.Contains("ad service"))
                                {
                                    firsLine.HsnCode = "9997"; // Advertising services
                                }
                                else if (description.Contains("ai") || description.Contains("robotics"))
                                {
                                    firsLine.HsnCode = "9989"; // AI and robotics services
                                }
                                else if (description.Contains("blockchain") || description.Contains("cryptocurrency"))
                                {
                                    firsLine.HsnCode = "9990"; // Blockchain and crypto services
                                }
                                else if (description.Contains("cable") || description.Contains("fibre") || description.Contains("fiber"))
                                {
                                    firsLine.HsnCode = "9984"; // Internet / cable services
                                }
                                else if ((description.Contains("cctv") || description.Contains("surveillance")) &&
                                         (description.Contains("monitoring") || description.Contains("managed")))
                                {
                                    firsLine.HsnCode = "9979"; // CCTV / surveillance monitoring services
                                }
                                else if (description.Contains("cctv") ||
                                         description.Contains("surveillance") ||
                                         description.Contains("security camera") ||
                                         description.Contains("dvr") ||
                                         description.Contains("nvr"))
                                {
                                    firsLine.HsnCode = "8525"; // CCTV / surveillance hardware
                                }
                                else if (description.Contains("cloud") || description.Contains("cloud service"))
                                {
                                    firsLine.HsnCode = "9986"; // Cloud services
                                }
                                else if (description.Contains("colocation") || description.Contains("platform"))
                                {
                                    firsLine.HsnCode = "9988"; // Colocation / platform services
                                }
                                else if (description.Contains("consultancy") || description.Contains("advisory"))
                                {
                                    firsLine.HsnCode = "9998"; // Consultancy services
                                }
                                else if (description.Contains("consulting") || description.Contains("service"))
                                {
                                    firsLine.HsnCode = "9993"; // General services
                                }
                                else if (description.Contains("course") || description.Contains("training"))
                                {
                                    firsLine.HsnCode = "9994"; // Training services
                                }
                                else if (description.Contains("cybersecurity"))
                                {
                                    firsLine.HsnCode = "9979"; // Cybersecurity services
                                }
                                else if (description.Contains("data") || description.Contains("cyberdata"))
                                {
                                    firsLine.HsnCode = "9980"; // Data services
                                }
                                else if (description.Contains("digital content") ||
                                         description.Contains("ebook") ||
                                         description.Contains("audiobook"))
                                {
                                    firsLine.HsnCode = "9987"; // Digital content
                                }
                                else if (description.Contains("electronic gadget") || description.Contains("gadget"))
                                {
                                    firsLine.HsnCode = "9977"; // Electronic gadgets
                                }
                                else if (description.Contains("electronic"))
                                {
                                    firsLine.HsnCode = "9991"; // Electronic devices
                                }
                                else if (description.Contains("energy") ||
                                         description.Contains("solar") ||
                                         description.Contains("power"))
                                {
                                    firsLine.HsnCode = "9982"; // Energy services
                                }
                                else if (description.Contains("hardware") ||
                                         description.Contains("computer") ||
                                         description.Contains("laptop") ||
                                         description.Contains("tablet"))
                                {
                                    firsLine.HsnCode = "8471"; // Computers and hardware
                                }
                                else if (description.Contains("information technology") || description.Contains("technology"))
                                {
                                    firsLine.HsnCode = "9978"; // IT services
                                }
                                else if (description.Contains("internet") || description.Contains("internet access"))
                                {
                                    firsLine.HsnCode = "9984"; // Internet services
                                }
                                else if (description.Contains("license") ||
                                     description.Contains("software") ||
                                     description.Contains("domain") ||
                                     description.Contains("domain name") ||
                                     description.Contains("dns"))
                                {
                                    firsLine.HsnCode = "9983"; // Software, licenses, and domain services
                                }

                                else if (description.Contains("maintenance") || description.Contains("support"))
                                {
                                    firsLine.HsnCode = "9995"; // Maintenance and support
                                }
                                else if (description.Contains("mobile") || description.Contains("phone"))
                                {
                                    firsLine.HsnCode = "8517"; // Mobile devices
                                }
                                else if (description.Contains("network") ||
                                         description.Contains("router") ||
                                         description.Contains("switch"))
                                {
                                    firsLine.HsnCode = "8517"; // Network equipment
                                }
                                else if (description.Contains("payment") ||
                                         description.Contains("gateway") ||
                                         description.Contains("wallet") ||
                                         description.Contains("fintech") ||
                                         description.Contains("switching") ||
                                         description.Contains("settlement"))
                                {
                                    firsLine.HsnCode = "9986"; // FinTech / payment services
                                }
                                else if (description.Contains("pos"))
                                {
                                    firsLine.HsnCode = "8470"; // POS systems
                                }
                                else if (description.Contains("printer") || description.Contains("scanner"))
                                {
                                    firsLine.HsnCode = "8443"; // Printers and scanners
                                }
                                else if (description.Contains("research"))
                                {
                                    firsLine.HsnCode = "9999"; // R&D services
                                }
                                else if (description.Contains("hosting") ||
                                      description.Contains("web") ||
                                      description.Contains("server hosting") ||
                                      description.Contains("email hosting"))
                                {
                                    firsLine.HsnCode = "9986"; // Hosting / cloud infrastructure services
                                }
                                else if (description.Contains("saas") ||
                                         description.Contains("software as a service"))
                                {
                                    firsLine.HsnCode = "9985"; // SaaS
                                }

                                else if (description.Contains("subscription"))
                                {
                                    firsLine.HsnCode = "9996"; // Subscription services
                                }
                                else if (description.Contains("voice") || description.Contains("cybervoice"))
                                {
                                    firsLine.HsnCode = "9981"; // Voice services
                                }
                                else if ((description.Contains("supplies") || description.Contains("consumables")) &&
                                     (description.Contains("it") ||
                                      description.Contains("computer") ||
                                      description.Contains("cable") ||
                                      description.Contains("network") ||
                                      description.Contains("accessory")))
                                {
                                    firsLine.HsnCode = "8473"; // IT supplies / consumables / accessories
                                }
                                else if (description.Contains("supplies") || description.Contains("consumables"))
                                {
                                    firsLine.HsnCode = "9992"; // General / miscellaneous supplies
                                }

                                else
                                {
                                    firsLine.HsnCode = "9992"; // General / miscellaneous items
                                }


                                Item invoiceItem = new Item();
                                invoiceItem.Name = itemLine.ItemCode;
                                invoiceItem.Description = itemLine.ItemDescription;
                                invoiceItem.SellersItemIdentification = itemLine.ItemCode;

                                Price price = new Price();
                                price.PriceAmount = itemLine.Price;
                                price.BaseQuantity = int.Parse(itemLine.Quantity.ToString()); // MUST be int
                                price.PriceUnit = $"{itemLine.Currency} per 1";

                                firsLine.Item = invoiceItem;
                                firsLine.Price = price;


                                subtot.TaxableAmount = price.PriceAmount * price.BaseQuantity;
                                subtot.TaxAmount = 0.01 * (7.5 * subtot.TaxableAmount);
                                subtot.TaxCategory = new TaxCategory();
                                TaxCategory cat = new TaxCategory();
                                cat.Percent = 7.5;
                                cat.Id = "STANDARD_VAT";

                                subtot.TaxCategory = cat;


                                subtots.Add(subtot);



                                TaxTotal tot = new TaxTotal();
                                tot.TaxAmount = subtots.Sum(a => a.TaxAmount);
                                tot.TaxSubtotal = subtots;


                                firsInv.TaxTotal.Add(tot);


                                firsInv.InvoiceLine.Add(firsLine);
                            }







                            // End of loop through itemLines


                            //firsInv.Irn = dreference.Irn;
                            // ----------------------------------------------------
                            // Invoice Metadata
                            // ----------------------------------------------------
                            firsInv.PaymentStatus = "PENDING";// PENDING, PAID, REJECTED
                            firsInv.InvoiceNumber = invoice.FirsInvoiceNumber;
                            firsInv.InvoiceTypeCode = "381"; // 381 is Commercial Invoice
                            firsInv.IssueDate = DateTime.UtcNow.ToString("yyyy-MM-dd"); // invoice.InvoiceDate.ToString("yyyy-MM-dd");
                            firsInv.DueDate = DateTime.Parse(firsInv.IssueDate).AddDays(30).ToString("yyyy-MM-dd"); // DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd");
                            firsInv.DocumentCurrencyCode = invoice.CurrencyCode;
                            firsInv.TaxCurrencyCode = invoice.CurrencyCode;
                            firsInv.Note = "Test invoice submission to FIRS";
                            firsInv.BusinessId = page.Data.User.BusinessId; // company.CompanyFIRSServiceNumber;

                            // ----------------------------------------------------
                            // Monetary Totals
                            // ----------------------------------------------------
                            LegalMonetaryTotal totals = new LegalMonetaryTotal();

                            totals.TaxInclusiveAmount = double.Parse(invoice.TotalAmount.ToString());// itemLine.PriceAfterVAT * itemLine.Quantity;
                            totals.PayableAmount = double.Parse(invoice.TotalAmount.ToString());

                            totals.LineExtensionAmount = double.Parse(invoice.TotalAmount.ToString()) - double.Parse(invoice.VatSum.ToString());
                            totals.TaxExclusiveAmount = double.Parse(invoice.TotalAmount.ToString()) - double.Parse(invoice.VatSum.ToString());


                            firsInv.LegalMonetaryTotal = totals;


                            // ----------------------------------------------------
                            // Payment Means
                            // ----------------------------------------------------
                            PaymentMean means = new PaymentMean();
                            means.PaymentMeansCode = "42"; // Bank Transfer                         
                            means.PaymentDueDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd");
                            firsInv.PaymentMeans = new List<PaymentMean>();
                            firsInv.PaymentMeans.Add(means); // = new List<Means>() { means };

                            firsInv.BuyerReference = "PO12345"; //  
                            firsInv.OrderReference = "ON12346"; //order_reference": null,


                            firsInv.PaymentTermsNote = "This invoice has no discount";
                            firsInv.TaxPointDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd");


                            firsInv.ActualDeliveryDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd");
                            firsInv.InvoiceDeliveryPeriod = new InvoiceDeliveryPeriod();
                            firsInv.InvoiceDeliveryPeriod.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
                            firsInv.InvoiceDeliveryPeriod.EndDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd");

                            AllowanceCharge cg = new AllowanceCharge();
                            cg.ChargeIndicator = false;
                            cg.Amount = 10;
                            firsInv.AllowanceCharge = new List<AllowanceCharge> { cg };
                            firsInv.DueDate = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd");
                            firsInv.AccountingCost = $"{(0.68 * totals.PayableAmount).ToString()} NGN";

                            // firsinvoice collection is ready



                            invoiceResults.Add(invoice.Id, firsInv);

                        }

                        foreach (DictionaryEntry entry in invoiceResults)
                        {

                            var invoiceUrl = "https://einvoice.gention.tech/api/v1/invoice/upload";
                            var bearerToken = accessToken; // token obtained from login response

                            using var invoiceClient = new HttpClient();

                            // Serialize payload
                            var invoicePayload = JsonSerializer.Serialize(entry.Value);
                            _log4net.Info($"Invoice Payload: {invoicePayload}");
                            using var invoiceContent = new StringContent(invoicePayload, Encoding.UTF8, "application/json");

                            // Build request
                            using var invoiceRequest = new HttpRequestMessage(HttpMethod.Post, invoiceUrl)
                            {
                                Content = invoiceContent
                            };

                            // ✅ Correct place to attach the token
                            invoiceRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                            //      invoiceRequest.Headers.Add( "Authorization", "Bearer " + accessToken);


                            // Send request
                            using var invoiceResponse = invoiceClient.SendAsync(invoiceRequest).Result;
                            var invoiceJson = invoiceResponse.Content.ReadAsStringAsync().Result;
                            // Ensure success
                            invoiceResponse.EnsureSuccessStatusCode();

                            // Read response
                            invoiceJson = invoiceResponse.Content.ReadAsStringAsync().Result;

                            // Log response
                            _log4net.Info(invoiceJson);

                            var finalresponse = JsonSerializer.Deserialize<FirsInvoiceResponse>(invoiceJson);

                            InvoiceMetadata metadata = null; // new InvoiceMetadata();

                            switch (finalresponse.StatusCode)
                            {
                                case 200:
                                case 201:

                                case 400:
                                    metadata = finalresponse.Data.Metadata
                                    .FirstOrDefault(m =>
                                                    m.Step == "validated_invoice" &&
                                                    m.Status == "success");

                                    if (metadata != null)
                                    {
                                        string base64QrCode = finalresponse?.Data?.Data?.QrCodeBase64;
                                        string irn = finalresponse?.Data?.Data?.Irn;
                                        string invoiceNumber = finalresponse?.Data?.Data.InvoiceNumber;
                                        string timeStamp = metadata.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                        byte[] img = Convert.FromBase64String(base64QrCode);

                                        if (con.State == System.Data.ConnectionState.Open)
                                        {
                                            _log4net.Info($"DB Connection was Successful");

                                            cmd = new SqlCommand("UPDATE [InvoiceTransactions] set [IRN]=@IRN, [IRNDate]=@IRNDate, [InvoiceNumber]=@InvoiceNumber,[QRCode]=@QRCode,[ValidatedInvoice]=1, [TransmitStatus]=1, [InvoiceStatus]=0 WHERE [Id]=@Id", con);

                                            cmd.Parameters.AddWithValue("@IRN", irn);
                                            cmd.Parameters.AddWithValue("@IRNDate", timeStamp);
                                            cmd.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);
                                            cmd.Parameters.AddWithValue("@QRCode", base64QrCode);
                                            cmd.Parameters.AddWithValue("@Id", int.Parse(entry.Key.ToString()));

                                            cmd.ExecuteNonQuery();
                                            cmd.Dispose();
                                        }


                                    }
                                    break;
                                default:
                                    _log4net.Error($"End processing Invoice: {invoiceJson} for Company: {company.CompanyName}");


                                    break;
                            }

                            var invoiceNumberLog = ((entry.Value as FirsInvoiceRequest).InvoiceNumber);

                            _log4net.Info($"End processing Invoice: {invoiceNumberLog} for Company: {company.CompanyName}");


                        }

                        //  Get top 1 from Invoice from InvoiceTransactions where TransmitStatus = 0
                        Thread.Sleep(new TimeSpan(0, 10, 0));
                    }


                }
                catch (Exception ex)
                {
                    _log4net.Error(ex.Message);

                }


                Thread.Sleep(new TimeSpan(0, 20, 0));
                _log4net.Info("Customer Handler heartbeat for Company: " + company.CompanyName);
            }
        }

        private void CustomerQueueHandler(object? source)
        {

            Companies company = (Companies)source;

            string QPCustomer = @".\Private$\" + $"{company.CompanyId}Customers";

            MessageQueue _queue = new MessageQueue(QPCustomer);

            // Explicitly set formatter for Customer class
            _queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(Customers) });

            _queue.BeginPeek();

            _queue.PeekCompleted += Queue_PeekCompleted;


        }

        private void Queue_PeekCompleted(object sender, PeekCompletedEventArgs e)
        {
            MessageQueue mq = (MessageQueue)sender;

            try
            {

                // End the peek operation
                Message Msg = mq.EndPeek(e.AsyncResult);


                Customers cust = (Customers)Msg.Body;

                // Log it to DB


                int ret = LogMessageToDB(cust);
                switch (ret)
                {

                    case 1:
                        _log4net.Info("Customer logged to DB: " + cust.CustomerName);
                        break;
                    case 2:
                        _log4net.Info("Customer already exists in DB: " + cust.CustomerName);
                        break;
                    default:
                        _log4net.Error("Failed to log Customer to DB: " + cust.CustomerName);
                        MessageQueue dlq = new MessageQueue(@".\Private$\DeadLetterQueue");
                        dlq.Label = $"FailedCustomer_{cust.CustomerCode}";
                        dlq.Send(cust);
                        dlq.Dispose();
                        // Send to Dead Letter Queue
                        break;

                }


                //Remove from Queue
                mq.ReceiveById(Msg.Id);
            }
            catch (Exception ex)
            {
                _log4net.Error("Error in PeekCompleted: " + ex.Message);
            }
            finally
            {
                mq.BeginPeek();
            }

        }

        private int LogMessageToDB(Customers cust)
        {
            // 1 = Success, 2 = Already Exists, 0 = Failed

            const string sql = @"
                    INSERT INTO Customers
                    (
                        CompanyId,
                        CustomerCode,
                        CustomerName,
                        BusinessDescription,
                        Email,
                        CustomerAddress,
                        City,
                        Country,
                        Telephone,
                        TIN,
                        ActiveStatus
                    )
                    SELECT
                        @CompanyId,
                        @CustomerCode,
                        @CustomerName,
                        @BusinessDescription,
                        @Email,
                        @CustomerAddress,
                        @City,
                        @Country,
                        @Telephone,
                        @TIN,
                        @ActiveStatus
                    WHERE NOT EXISTS
                    (
                        SELECT 1
                        FROM Customers
                        WHERE CompanyId = @CompanyId
                          AND CustomerCode = @CustomerCode
                    );";

            using (SqlConnection cnn = new SqlConnection(
                "Server=localhost;user id=sa;password=Test_test1;Database=CybMBSDb;TrustServerCertificate=True"))
            using (SqlCommand cmd = new SqlCommand(sql, cnn))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@CompanyId", cust.CompanyId);
                    cmd.Parameters.AddWithValue("@CustomerCode", cust.CustomerCode);
                    cmd.Parameters.AddWithValue("@CustomerName", cust.CustomerName);
                    cmd.Parameters.AddWithValue("@BusinessDescription", cust.BusinessDescription);
                    cmd.Parameters.AddWithValue("@Email", cust.Email);
                    cmd.Parameters.AddWithValue("@CustomerAddress", cust.CustomerAddress);
                    cmd.Parameters.AddWithValue("@City", cust.City);
                    cmd.Parameters.AddWithValue("@Country", cust.Country);
                    cmd.Parameters.AddWithValue("@Telephone", cust.Telephone);
                    cmd.Parameters.AddWithValue("@TIN", cust.TIN);
                    cmd.Parameters.AddWithValue("@ActiveStatus", cust.ActiveStatus);

                    cnn.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    cnn.Close();

                    return rowsAffected == 1 ? 1 : 2;
                }
                catch (Exception ex)
                {
                    _log4net.Error("Error logging Customer to DB", ex);
                    return 0;
                }
            }
        }

        public void StartProcess(Companies company)
        {

            // Customer Thread
            Thread customerThread = new Thread(new ParameterizedThreadStart(CustomerHandler));

            customerThread.Start(company);
            Interlocked.Increment(ref threadCount);

            while (isApplicationProcessing != false)
            {
                Thread.Sleep(1000);
                _log4net.Info($"Customer probing in progress...");
            }
        }
        public void StopProcess(Companies company)
        {

            _log4net.Info("Stopping processing for Company: " + company.CompanyName);
            isApplicationProcessing = false;
            Interlocked.Decrement(ref threadCount);


        }

        public string NormalizeToNgE164(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            // Remove spaces, dashes, brackets, etc.
            string digits = Regex.Replace(phone, @"\D", "");

            // Normalize Nigerian formats
            if (digits.StartsWith("0") && digits.Length == 11)
            {
                // 09070033961 → 9070033961
                digits = digits.Substring(1);
            }
            else if (digits.StartsWith("234") && digits.Length == 13)
            {
                // already normalized without +
                digits = digits.Substring(3);
            }

            // Final validation
            if (digits.Length != 10)
                throw new FormatException("Invalid Nigerian phone number format.");

            return $"+234{digits}";
        }

        public static string NormalizeToE164(string phone, string defaultCountryCode = "234")
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            // Remove everything except digits
            string digits = Regex.Replace(phone, @"\D", "");

            // Handle international prefix 00 → +
            if (digits.StartsWith("00"))
                digits = digits.Substring(2);

            // Nigerian local format (0XXXXXXXXXX)
            if (defaultCountryCode == "234" &&
                digits.StartsWith("0") &&
                digits.Length == 11)
            {
                digits = digits.Substring(1);
                return $"+234{digits}";
            }

            // Nigerian without + (234XXXXXXXXXX)
            if (defaultCountryCode == "234" &&
                digits.StartsWith("234") &&
                digits.Length == 13)
            {
                return $"+{digits}";
            }

            // Already contains country code (generic international)
            if (digits.Length >= 11 && digits.Length <= 15)
            {
                return $"+{digits}";
            }

            // Assume local number → prepend default country code
            if (digits.Length >= 7 && digits.Length <= 10)
            {
                return $"+{defaultCountryCode}{digits}";
            }

            throw new FormatException("Invalid phone number format.");
        }
    }
}


