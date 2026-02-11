
using ETaxTracker.Models;
using ETaxTracker.Services;
using log4net;
using log4net.Config;
using Microsoft.Data.SqlClient;
using MSMQ.Messaging;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using RestSharp; // Nugget Version = 105.2.3 - This is what worked
using System;
//using RestSharp.Authenticators;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;


namespace ETaxTracker
{
    public class Program
    {
        public static int threadCount;
        private static bool isApplicationProcessing;

        //MethodBase.GetCurrentMethod().DeclaringType
        private static readonly ILog _log4net = LogManager.GetLogger(typeof(Program));
        static void Main(string[] args)
        {
            isApplicationProcessing = true;
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("Log4net.config"));

            List<Companies> Companies = new List<Companies>();
            string connectionstring = ConfigurationManager.ConnectionStrings["connectionstring"].ConnectionString;

            SqlConnection cn = new SqlConnection(connectionstring);
            cn.Open();
            if (cn.State == System.Data.ConnectionState.Open)
            {

                _log4net.Info($"DB Connection was Successful");

                SqlCommand cmd = new SqlCommand(@"SELECT [Id],[CompanyId],[ERPUserId],[ERPPassword],
                                               [CompanyFIRSReferenceNumber],[CompanyFIRSServiceNumber],
                                               [CompanyCode],[CompanyName],[CompanyAddress],[BusinessDescription],
                                               [Email],[City],[LgaCode],[StateCode],[Country],[CountryCode],[PostalZone],
                                               [Street],[Telephone],[TIN],[AuthURL],[ActiveStatus] 
                                               FROM Companies WHERE [ActiveStatus]=1", cn);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows == true)
                {

                    while (dr.Read())
                    {
                        Companies company = new Companies();
                        company.Id = dr.IsDBNull(0) ? 0 : dr.GetInt32(0);
                        company.CompanyId = dr.IsDBNull(1) ? string.Empty : dr.GetString(1);
                        company.ERPUserId = dr.IsDBNull(2) ? string.Empty : dr.GetString(2);
                        company.ERPPassword = dr.IsDBNull(3) ? string.Empty : dr.GetString(3);
                        company.CompanyFIRSReferenceNumber = dr.IsDBNull(4) ? string.Empty : dr.GetString(4);
                        company.CompanyFIRSServiceNumber = dr.IsDBNull(5) ? string.Empty : dr.GetString(5);
                        company.CompanyCode = dr.IsDBNull(6) ? string.Empty : dr.GetString(6);
                        company.CompanyName = dr.IsDBNull(7) ? string.Empty : dr.GetString(7);
                        company.CompanyAddress = dr.IsDBNull(8) ? string.Empty : dr.GetString(8);
                        company.BusinessDescription = dr.IsDBNull(9) ? string.Empty : dr.GetString(9);
                        company.Email = dr.IsDBNull(10) ? string.Empty : dr.GetString(10);
                        company.City = dr.IsDBNull(11) ? string.Empty : dr.GetString(11);
                        company.LgaCode= dr.IsDBNull(12) ? string.Empty : dr.GetString(12);
                        company.StateCode = dr.IsDBNull(13) ? string.Empty : dr.GetString(13);
                        company.Country = dr.IsDBNull(14) ? string.Empty : dr.GetString(14);
                        company.CountryCode = dr.IsDBNull(15) ? string.Empty : dr.GetString(15);
                        company.PostalZone = dr.IsDBNull(16) ? string.Empty : dr.GetString(16);
                        company.Street = dr.IsDBNull(17) ? string.Empty : dr.GetString(17);
                        company.Telephone = dr.IsDBNull(18) ? string.Empty : dr.GetString(18);
                        company.TIN = dr.IsDBNull(19) ? string.Empty : dr.GetString(19);
                        company.AuthUrl = dr.IsDBNull(20) ? string.Empty : dr.GetString(20);
                        company.ActiveStatus = dr.IsDBNull(21) ? 0 : int.Parse(dr.GetValue(21).ToString());
                        Companies.Add(company);
                    }

                }
                else
                {
                    _log4net.Error("No Company Data Found");

                }


            }


            //CustomerProcessor
            Thread customerThread = null; // Create threads based on number of companies
            CustomerProcessor customerProcessor;

            //InvoiceProcessor
            Thread invoiceThread = null; // Create threads based on number of companies
            InvoiceProcessor invoiceProcessor;

            //TaxProcessor
            Thread taxThread = null;
            TaxProcessor taxProcessor;

            foreach (Companies company in Companies)
            {

                //customerThread = new Thread(new ParameterizedThreadStart(CustomerProcessHandler)); // Create threads based on number of companies
                //customerThread.Start(company);


                //invoiceThread = new Thread(new ParameterizedThreadStart(InvoiceProcessHandler)); // Create threads based on number of companies
                //invoiceThread.Start(company);

                //Tax Thread


                taxThread = new Thread(new ParameterizedThreadStart(TaxProcessHandler)); // Create threads based on number of companies
                taxThread.Start(company);

            }

            while (isApplicationProcessing != false)
            {

                _log4net.Info("Main Thread is alive. Current Thread Count: " + threadCount);

                Thread.Sleep(10000);
            }

        }

        private static void TaxProcessHandler(object? source)
        {
            Companies company = (Companies)source;
            TaxProcessor taxProcessor = new TaxProcessor(isApplicationProcessing);
            taxProcessor.StartProcess(company);
        }

        private static void InvoiceProcessHandler(object? source)
        {
            
            Companies company = (Companies)source;
            InvoiceProcessor invoiceProcessor = new InvoiceProcessor(isApplicationProcessing);
            invoiceProcessor.StartProcess(company);
 

        }

        private static void CustomerProcessHandler(object? source)
        {
            Companies company = (Companies)source;
            CustomerProcessor customerProcessor = new CustomerProcessor(isApplicationProcessing);
             customerProcessor.StartProcess(company);
            

        }
    }
}