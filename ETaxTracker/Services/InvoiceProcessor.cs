using ETaxTracker.Models;
using log4net;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using MSMQ.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ETaxTracker.Services
{
    public class InvoiceProcessor
    {
        private int threadCount;
        public bool isApplicationProcessing;

        //MethodBase.GetCurrentMethod().DeclaringType
        private static readonly ILog _log4net = LogManager.GetLogger(typeof(InvoiceProcessor));

        public InvoiceProcessor(bool _isApplicationProcessing)
        {
            isApplicationProcessing = _isApplicationProcessing;
        }

        private void InvoiceHandler(object? source)
        {
            Companies company = (Companies)source;
            _log4net.Info("Invoice Handler started for Company: " + company.CompanyName);

            string newBeginTime = DateTime.Parse("2026-01-01T00:00:00Z")
                .ToString("yyyy-MM-ddTHH:mm:ssZ");

            string QPLastInvoiceDate = @".\Private$\" + $"{company.CompanyId}LastInvoiceSyncTime";
            string QPInvoice = @".\Private$\" + $"{company.CompanyId}Invoices";
            string QDocumentLine = @".\Private$\" + $"{company.CompanyId}InvoiceDocumentLines";
            MessageQueue queue;

            if (!MessageQueue.Exists(QPLastInvoiceDate))
            {
                queue = MessageQueue.Create(QPLastInvoiceDate);
                queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                queue.Send(newBeginTime);
            }
            else
            {
                queue = new MessageQueue(QPLastInvoiceDate)
                {
                    Formatter = new XmlMessageFormatter(new Type[] { typeof(string) })
                };
            }




            if (!MessageQueue.Exists(QPInvoice))
            {
                queue = MessageQueue.Create(QPInvoice);
                queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(InvoiceTransactions) });
            }
            else
            {
                queue = new MessageQueue(QPInvoice)
                {
                    Formatter = new XmlMessageFormatter(new Type[] { typeof(InvoiceTransactions) })
                };
            }



            if (!MessageQueue.Exists(QDocumentLine))
            {
                queue = MessageQueue.Create(QDocumentLine);
                queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(DocumentLine) });
            }
            else
            {
                queue = new MessageQueue(QDocumentLine)
                {
                    Formatter = new XmlMessageFormatter(new Type[] { typeof(DocumentLine) })
                };
            }

            try
            {
                using var mq = new MessageQueue(QPLastInvoiceDate)
                {
                    Formatter = new XmlMessageFormatter(new Type[] { typeof(string) })
                };

                var msg = mq.Receive(TimeSpan.FromSeconds(10));
                newBeginTime = msg.Body as string ?? newBeginTime;
            }
            catch
            {
                newBeginTime = DateTime.Parse("2026-01-01T00:55:00Z")
                    .ToString("yyyy-MM-ddTHH:mm:ssZ");
            }

            //The nexe tread will read invoices from the queue and persist it in the database

            Thread invoiceQueueThread =
                new Thread(new ParameterizedThreadStart(InvoiceQueueHandler));
            invoiceQueueThread.Start(company);
            Interlocked.Increment(ref threadCount);

            // Every invoice has at heast 1 document line. The document line is written into another que
            //This thread will read from it and persist to the database

            Thread documentlineQueueThread =
               new Thread(new ParameterizedThreadStart(DocumentLineQueueHandler));
            documentlineQueueThread.Start(company);
            Interlocked.Increment(ref threadCount);



            while (isApplicationProcessing)
            {
                DateTime beginTime = DateTime.SpecifyKind(
                    DateTime.Parse(newBeginTime), DateTimeKind.Utc);

                try
                {
                    var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };

                    using var client = new HttpClient(handler)
                    {
                        BaseAddress = new Uri(company.AuthUrl)
                    };

                    client.DefaultRequestHeaders.Accept
                        .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var loginResponse = client
                        .PostAsJsonAsync("Login", new ErpSignInBody
                        {
                            CompanyDB = company.CompanyCode.Trim(),
                            UserName = company.ERPUserId.Trim(),
                            Password = company.ERPPassword.Trim()
                        }).Result;

                    loginResponse.EnsureSuccessStatusCode();

                    var login = loginResponse.Content
                        .ReadFromJsonAsync<LoginResponse>().Result;

                    client.DefaultRequestHeaders.Add(
                        "Cookie", $"B1SESSION={login.SessionId}; ROUTEID=.node4");

                    DateTime endDate = DateTime.UtcNow;

                    for (DateTime day = beginTime.Date; day <= endDate.Date; day = day.AddDays(1))
                    {
                        DateTime nextDay = day.AddDays(1);

                        try
                        {
                            for (DateTime lower = day; lower < nextDay; lower = lower.AddMinutes(20))
                            {
                                DateTime upper = lower.AddMinutes(15);
                                if (upper > nextDay) upper = nextDay;

                                string from = lower.ToString("yyyy-MM-ddTHH:mm:ssZ");
                                string to = upper.ToString("yyyy-MM-ddTHH:mm:ssZ");

                                string requestUrl =
                                    $"Invoices?$select=DocEntry,DocDate,CardCode,CardName,Address," +
                                    $"DocTotal,Comments,Address,VatSum,DocumentLines" +
                                    $"&$filter=DocDate ge {from} and DocDate le {to} " +
                                    $"&$top=1500";



                                while (!string.IsNullOrWhiteSpace(requestUrl))
                                {
                                    _log4net.Info($"Query: {requestUrl}");
                                    using var request =
                                        new HttpRequestMessage(HttpMethod.Get, requestUrl);

                                    var response = client.SendAsync(request).Result;
                                    response.EnsureSuccessStatusCode();

                                    var json = response.Content.ReadAsStringAsync().Result;
                                    _log4net.Info(json.ToString());
                                    var page =
                                        JsonSerializer.Deserialize<InvoiceTransactionResponse>(
                                            json,
                                            new JsonSerializerOptions
                                            {
                                                PropertyNameCaseInsensitive = true
                                            });

                                    if (page?.Value != null)
                                    {
                                        MessageQueue msgQ = new MessageQueue(QPInvoice);

                                        foreach (var inv in page.Value)
                                        {
                                            InvoiceTransactions itr = new InvoiceTransactions();
                                            itr.InvoiceNumber = inv.DocEntry.ToString();
                                            itr.InvoiceDate = inv.DocDate;
                                            itr.CustomerCode = inv.CardCode;
                                            itr.CustomerName = inv.CardName;
                                            itr.TotalAmount = inv.DocTotal;
                                            itr.VatSum = inv.VatSum;
                                            itr.CurrencyCode = inv.CurrencyCode;
                                            inv.Address = inv.Address ?? string.Empty;
                                            inv.Comments = inv.Comments ?? string.Empty;
                                            itr.CompanyId = company.Id.ToString();
                                            itr.IRN = string.Empty;                                           
                                            itr.IRNDate = new DateTime(1900, 1, 1);
                                            itr.QRCode = string.Empty;
                                            itr.FirsInvoiceNumber = string.Empty;
                                            itr.TransmitStatus = 0;
                                            itr.EmailNotificationStatus = 0;

                                            msgQ.Send(itr);

                                            // Iterate through DocumentLines and send the content to queue




                                            foreach (DocumentLine ln in inv.DocumentLines)
                                            {                                     //  _log4net.Info($"-- Line {ln.LineNum}: {ln.ItemCode} - {ln.ItemDescription}, Qty: {ln.Quantity}, Price: {ln.Price}, Line Total: {ln.LineTotal}");
                                                ItemLines itemLine = new ItemLines();
                                                itemLine.CompanyId = company.Id;
                                                itemLine.CustomerCode = inv.CardCode;
                                                itemLine.DocEntry = inv.DocEntry.ToString();
                                                itemLine.LineNum = ln.LineNum;
                                                itemLine.ItemCode = ln.ItemCode;
                                                itemLine.ItemDescription = ln.ItemDescription;
                                                itemLine.Quantity = ln.Quantity;
                                                itemLine.Price = ln.Price;
                                                itemLine.PriceAfterVAT = ln.PriceAfterVAT;
                                                itemLine.Currency = ln.Currency;
                                                itemLine.LineTotal = ln.LineTotal;
                                                MessageQueue lineQ = new MessageQueue(QDocumentLine);
                                                lineQ.Send(itemLine);

                                            }


                                        }
                                    }

                                    // 🔴 paging advancement (correct)
                                    requestUrl = page?.NextLink;
                                }

                                // 🔴 advance cursor only AFTER window completes
                                newBeginTime = upper.ToString("yyyy-MM-ddTHH:mm:ssZ");
                            }
                        }
                        catch (Exception ex)
                        {

                            _log4net.Error($"Invoice Processing for {company.CompanyName}: {ex.Message}");

                        }
                    }

                    // 🔴 commit sync cursor
                    using var cursorQueue = new MessageQueue(QPLastInvoiceDate);
                    cursorQueue.Send(newBeginTime);
                }
                catch (Exception ex)
                {
                    _log4net.Error($"Invoice Processing for {company.CompanyName}: {ex.Message}");
                }

                Thread.Sleep(1000);
            }
        }

        private void DocumentLineQueueHandler(object? source)
        {
            Companies company = (Companies)source;

            string QDocumentLine = @".\Private$\" + $"{company.CompanyId}InvoiceDocumentLines";

            MessageQueue _documentqueue = new MessageQueue(QDocumentLine);

            // Explicitly set formatter for Customer class: ItemLines is a compact version of DocumentLines
            _documentqueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(ItemLines) });

            _documentqueue.BeginPeek();

            _documentqueue.PeekCompleted += _documentqueue_PeekCompleted;
        }

        private void _documentqueue_PeekCompleted(object sender, PeekCompletedEventArgs e)
        {
            MessageQueue mq = (MessageQueue)sender;

            try
            {

                // End the peek operation
                Message Msg = mq.EndPeek(e.AsyncResult);


                ItemLines itemline = (ItemLines)Msg.Body;

                // Log it to DB
                

                int ret = LogMessageToDB(itemline);
                switch (ret)
                {

                    case 1:
                        _log4net.Info($"DocumentLine logged to DB: {itemline.DocEntry.ToString()}-{itemline.LineNum.ToString()}");
                        break;
                    case 2:
                        _log4net.Info($"DocumentLine already exists in DB: {itemline.DocEntry.ToString()}-{itemline.LineNum.ToString()}");
                        break;
                    default:
                        _log4net.Error($"Failed to log Customer to DB: {itemline.DocEntry.ToString()}-{itemline.LineNum.ToString()}");
                        MessageQueue dlq = new MessageQueue(@".\Private$\DeadLetterQueue");
                        dlq.Label = $"FailedCustomer_{itemline.DocEntry.ToString()}-{itemline.LineNum.ToString()}";
                        dlq.Send(itemline);
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

        private int LogMessageToDB(ItemLines itemLine)
        {
            // 1 = Success, 2 = Already Exists, 0 = Failed

            const string sql = @"
                        INSERT INTO dbo.ItemLines
                        (
                            CompanyId,
                            CustomerCode,
                            DocEntry,
                            LineNum,
                            ItemCode,
                            ItemDescription,
                            Quantity,
                            Price,
                            PriceAfterVAT,
                            Currency,
                            LineTotal
                        )
                        SELECT
                            @CompanyId,
                            @CustomerCode,
                            @DocEntry,
                            @LineNum,
                            @ItemCode,
                            @ItemDescription,
                            @Quantity,
                            @Price,
                            @PriceAfterVAT,
                            @Currency,
                            @LineTotal
                        WHERE NOT EXISTS
                        (
                            SELECT 1
                            FROM dbo.ItemLines
                            WHERE CompanyId = @CompanyId
                              AND DocEntry = @DocEntry
                              AND LineNum = @LineNum
                        );";
            string connectionstring = ConfigurationManager.ConnectionStrings["connectionstring"].ConnectionString;

            try
            {
                using (SqlConnection cnn = new SqlConnection(connectionstring))
                using (SqlCommand cmd = new SqlCommand(sql, cnn))
                {
                    cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = itemLine.CompanyId;
                    cmd.Parameters.Add("@CustomerCode", SqlDbType.NVarChar, 50).Value = itemLine.CustomerCode ?? string.Empty;
                    cmd.Parameters.Add("@DocEntry", SqlDbType.NVarChar, 50).Value = itemLine.DocEntry ?? string.Empty;
                    cmd.Parameters.Add("@LineNum", SqlDbType.Int).Value = itemLine.LineNum;
                    cmd.Parameters.Add("@ItemCode", SqlDbType.NVarChar, 50).Value = itemLine.ItemCode ?? string.Empty;
                    cmd.Parameters.Add("@ItemDescription", SqlDbType.NVarChar, 200).Value = itemLine.ItemDescription ?? string.Empty;
                    cmd.Parameters.Add("@Quantity", SqlDbType.Decimal).Value = itemLine.Quantity;
                    cmd.Parameters.Add("@Price", SqlDbType.Decimal).Value = itemLine.Price;
                    cmd.Parameters.Add("@PriceAfterVAT", SqlDbType.Decimal).Value = itemLine.PriceAfterVAT;
                    cmd.Parameters.Add("@Currency", SqlDbType.NVarChar, 10).Value = itemLine.Currency ?? string.Empty;
                    cmd.Parameters.Add("@LineTotal", SqlDbType.Decimal).Value = itemLine.LineTotal;

                    cnn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    return rowsAffected == 1 ? 1 : 2;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error("Error logging ItemLines to DB", ex);
                return 0;
            }
        }




        /// <summary>
        /// Helper: Ensure MSMQ queue exists
        /// </summary>
        /// 

        private void CreateQueueIfNotExists(string queuePath, Type formatterType)
        {
            if (!MessageQueue.Exists(queuePath))
            {
                var q = MessageQueue.Create(queuePath);
                q.Formatter = new XmlMessageFormatter(new Type[] { formatterType });
            }
        }

        private void InvoiceQueueHandler(object? source)
        {

            Companies company = (Companies)source;

            string QPCustomer = @".\Private$\" + $"{company.CompanyId}Invoices";

            MessageQueue _queue = new MessageQueue(QPCustomer);

            // Explicitly set formatter for Customer class
            _queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(InvoiceTransactions) });

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


                InvoiceTransactions invoice = (InvoiceTransactions)Msg.Body;

                // Log it to DB


                int ret = LogMessageToDB(invoice);
                switch (ret)
                {

                    case 1:
                        _log4net.Info("Invoice logged to DB: " + invoice.CustomerName);
                        break;
                    case 2:
                        _log4net.Info("Invoice already exists in DB: " + invoice.CustomerName);
                        break;
                    default:
                        _log4net.Error("Failed to log Customer to DB: " + invoice.CustomerName);
                        MessageQueue dlq = new MessageQueue(@".\Private$\DeadLetterQueue");
                        dlq.Label = $"FailedCustomer_{invoice.CustomerCode}";
                        dlq.Send(invoice);
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

        private int LogMessageToDB(InvoiceTransactions invoice)
        {
            // 1 = Success, 2 = Already Exists, 0 = Failed
             
            const string sql = @"
        INSERT INTO dbo.InvoiceTransactions
        (
            CompanyId,
            InvoiceNumber,
            InvoiceDate,
            CustomerCode,
            CustomerName,
            CustomerAddress,
            TotalAmount,
            VatSum,
            Comments,
            TransmitStatus,
            IRN,
            IRNDate,
            EmailNotificationStatus
        )
        SELECT
            @CompanyId,
            @InvoiceNumber,
            @InvoiceDate,
            @CustomerCode,
            @CustomerName,
            @CustomerAddress,
            @TotalAmount,
            @VatSum,
            @Comments,
            @TransmitStatus,
            @IRN,
            @IRNDate,
            @EmailNotificationStatus
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM dbo.InvoiceTransactions
            WHERE CompanyId = @CompanyId
              AND InvoiceNumber = @InvoiceNumber
        );";
            string connectionstring = ConfigurationManager.ConnectionStrings["connectionstring"].ConnectionString;

            try
            {
                using (SqlConnection cnn = new SqlConnection(connectionstring))
                using (SqlCommand cmd = new SqlCommand(sql, cnn))
                {
                    cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = invoice.CompanyId;
                    cmd.Parameters.Add("@InvoiceNumber", SqlDbType.NVarChar, 50).Value = invoice.InvoiceNumber ?? string.Empty;
                    cmd.Parameters.Add("@InvoiceDate", SqlDbType.DateTime).Value = (object?)invoice.InvoiceDate ?? DBNull.Value;
                    cmd.Parameters.Add("@CustomerCode", SqlDbType.NVarChar, 50).Value = invoice.CustomerCode ?? string.Empty;
                    cmd.Parameters.Add("@CustomerName", SqlDbType.NVarChar, 200).Value = invoice.CustomerName ?? string.Empty;
                    cmd.Parameters.Add("@CustomerAddress", SqlDbType.NVarChar, -1).Value = invoice.CustomerAddress ?? string.Empty;
                    cmd.Parameters.Add("@TotalAmount", SqlDbType.Decimal).Value = invoice.TotalAmount;
                    cmd.Parameters.Add("@VatSum", SqlDbType.Decimal).Value = invoice.VatSum;
                    cmd.Parameters.Add("@Comments", SqlDbType.NVarChar, -1).Value = invoice.Comments ?? string.Empty;
                    cmd.Parameters.Add("@TransmitStatus", SqlDbType.Int).Value = 0; // Not transmitted to FIRS
                    cmd.Parameters.Add("@IRN", SqlDbType.NVarChar, 100).Value = string.Empty; // IRN not generated yet
                    cmd.Parameters.Add("@IRNDate", SqlDbType.DateTime).Value = new DateTime(1900, 1, 1); // Sentinel date (far past)
                    cmd.Parameters.Add("@EmailNotificationStatus", SqlDbType.Int).Value = 0; // Email not sent
                    cnn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    return rowsAffected == 1 ? 1 : 2;
                }
            }
            catch (Exception ex)
            {
                _log4net.Error("Error logging InvoiceTransaction to DB", ex);
                return 0;
            }
        }


        public void StartProcess(Companies company)
        {

            // Customer Thread
            Thread invoiceThread = new Thread(new ParameterizedThreadStart(InvoiceHandler));

            invoiceThread.Start(company);
            Interlocked.Increment(ref threadCount);

            while (isApplicationProcessing != false)
            {
                Thread.Sleep(1000);
                _log4net.Info($"Invoice probing in progress...");
            }
        }
        public void StopProcess(Companies company)
        {

            _log4net.Info("Stopping processing invoice for Company: " + company.CompanyName);
            isApplicationProcessing = false;
            Interlocked.Decrement(ref threadCount);


        }
    }
}
