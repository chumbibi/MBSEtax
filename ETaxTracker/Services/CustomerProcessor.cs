using ETaxTracker.Models;
using log4net;
using Microsoft.Data.SqlClient;
using MSMQ.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ETaxTracker.Services
{
    public   class CustomerProcessor
    {
        private   int threadCount;
        public   bool isApplicationProcessing;

        //MethodBase.GetCurrentMethod().DeclaringType
        private static readonly ILog _log4net = LogManager.GetLogger(typeof(CustomerProcessor));

        public CustomerProcessor(bool _isApplicationProcessing)
        {
            isApplicationProcessing = _isApplicationProcessing;
        }
        
        private void CustomerHandler(object? source)
        {
            Companies company = (Companies)source;
            _log4net.Info("Customer Handler started for Company: " + company.CompanyName);

            string newBeginTime = DateTime.Parse("2020-12-01T00:00:00Z")
                .ToString("yyyy-MM-ddTHH:mm:ssZ");

            string QPLastCustomerDate = @".\Private$\" + $"{company.CompanyId}LastCustomerSyncTime";
            string QPCustomer = @".\Private$\" + $"{company.CompanyId}Customers";

            MessageQueue queue;

            if (!MessageQueue.Exists(QPLastCustomerDate))
            {
                queue = MessageQueue.Create(QPLastCustomerDate);
                queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                queue.Send(newBeginTime);
            }
            else
            {
                queue = new MessageQueue(QPLastCustomerDate)
                {
                    Formatter = new XmlMessageFormatter(new Type[] { typeof(string) })
                };
            }

            if (!MessageQueue.Exists(QPCustomer))
            {
                queue = MessageQueue.Create(QPCustomer);
                queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(Customers) });
            }

            try
            {
                using var mq = new MessageQueue(QPLastCustomerDate)
                {
                    Formatter = new XmlMessageFormatter(new Type[] { typeof(string) })
                };

                var msg = mq.Receive(TimeSpan.FromSeconds(10));
                newBeginTime = msg.Body as string ?? newBeginTime;
            }
            catch
            {
                newBeginTime = DateTime.Parse("2020-12-01T00:00:00Z")
                    .ToString("yyyy-MM-ddTHH:mm:ssZ");
            }

            Thread customerQueueThread =
                new Thread(new ParameterizedThreadStart(CustomerQueueHandler));
            customerQueueThread.Start(company);
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
                        "Cookie", $"B1SESSION={login.SessionId}; ROUTED=.node4");

                    DateTime endDate = DateTime.UtcNow;

                    for (DateTime day = beginTime.Date; day <= endDate.Date; day = day.AddDays(1))
                    {
                        DateTime nextDay = day.AddDays(1);

                        for (DateTime lower = day; lower < nextDay; lower = lower.AddMinutes(20))
                        {
                            DateTime upper = lower.AddMinutes(15);
                            if (upper > nextDay) upper = nextDay;

                            string from = lower.ToString("yyyy-MM-ddTHH:mm:ssZ");
                            string to = upper.ToString("yyyy-MM-ddTHH:mm:ssZ");

                            string requestUrl =
                                $"BusinessPartners?$select=CardCode,CardName,EmailAddress," +
                                $"BusinessType,Address,Country,City,Phone1,FederalTaxID" +
                                $"&$filter=CreateDate ge {from} and CreateDate le {to} " +
                                $"and CardType eq 'cCustomer'&$top=2000";

                            while (!string.IsNullOrWhiteSpace(requestUrl))
                            {
                                _log4net.Info($"Query: {requestUrl}");
                                using var request =
                                    new HttpRequestMessage(HttpMethod.Get, requestUrl);

                                var response = client.SendAsync(request).Result;
                                response.EnsureSuccessStatusCode();

                                var json = response.Content.ReadAsStringAsync().Result;

                                var page =
                                    JsonSerializer.Deserialize<BusinessPartnersResponse>(
                                        json,
                                        new JsonSerializerOptions
                                        {
                                            PropertyNameCaseInsensitive = true
                                        });

                                if (page?.Value != null)
                                {
                                    using var customerQueue =
                                        new MessageQueue(QPCustomer);

                                    foreach (var cust in page.Value)
                                    {
                                        customerQueue.Send(new Customers
                                        {
                                            CompanyId = company.CompanyId,
                                            CustomerCode = cust.CardCode ?? string.Empty,
                                            CustomerName = cust.CardName ?? string.Empty,
                                            BusinessDescription = cust.BusinessType ?? string.Empty,
                                            Email = cust.EmailAddress ?? string.Empty,
                                            CustomerAddress = cust.Address ?? string.Empty,
                                            City = cust.City ?? string.Empty,
                                            Country = cust.Country ?? string.Empty,
                                            Telephone = cust.Phone1 ?? string.Empty,
                                            TIN = cust.FederalTaxID ?? string.Empty,
                                            ActiveStatus = 1
                                        });
                                    }
                                }

                                // 🔴 paging advancement (correct)
                                requestUrl = page?.NextLink;
                            }

                            // 🔴 advance cursor only AFTER window completes
                            newBeginTime = upper.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        }
                    }

                    // 🔴 commit sync cursor
                    using var cursorQueue = new MessageQueue(QPLastCustomerDate);
                    cursorQueue.Send(newBeginTime);
                }
                catch (Exception ex)
                {
                    _log4net.Error($"{company.CompanyName}: {ex.Message}");
                }

                Thread.Sleep(300000);
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

        private   void Queue_PeekCompleted(object sender, PeekCompletedEventArgs e)
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

        private   int LogMessageToDB(Customers cust)
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
    }
}
