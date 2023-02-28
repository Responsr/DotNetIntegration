using Responsr.Partner.Integration;
using Responsr.Partner.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

/* 
 *  -------------------------------------------------------- 
 *  Responsr Integration template project V1.02
 *  --------------------------------------------------------
 *  
 *  TL;DR:
 *  
 *  - Fetch data from your system and send it to Responsr
 *  - Fetch only new posts that have not already been sent
 *  - Fetch only for clients already activly using Responsr 
 *  
 *  Add NUGET Source: 
 *  https://qsdev.pkgs.visualstudio.com/ResponsrPublic/_packaging/ResponsrPartner/nuget/v3/index.json
 *  
 *  Add Nuget packages:
 *  Responsr.Partner.Integration
 *  Microsoft.Extensions.Logging.Abstractions
 *  Newtonsoft.Json
 *  
 *  - Sample code below
 *  
 *  Additional information in README.txt
 *  
 */

namespace SendToResponsr
{
    class Program
    {
        private static Logger _logger = null;
        private static bool _debug = false;

        public static void Debug(string text)
        {
            if (_debug) { Console.WriteLine(text); }
        }

        static void Main(string[] args)
        {
            // Create somewhere to log and check for tries to debug the application
            _logger = new Logger(ConfigurationManager.AppSettings["LogFolderPath"]);
            _debug = (args.Contains("help") || args.Contains("debug") || args.Contains("verbose") || ConfigurationManager.AppSettings["Debug"].ToLower() == "true") ? true : false;

            // Create a batchrunner to send data to Responsr, this part is standard
            var batchRunner = new TransactionBatchRunner(new BatchRetriever(), new Guid(ConfigurationManager.AppSettings["PartnerKey"]), ConfigurationManager.AppSettings["LogFolderPath"], ResponsrEnvironment.Production);
            batchRunner.Run();

            Debug("Application finished");
        }

        private class BatchRetriever : ITransactionBatchRetriever
        {
            public TransactionBatch Retrieve(DateTime recommendedBatchRetrievalTimestamp, List<ClientInfo> clients, List<VariableInfo> variables)
            {
                /*          -----------------------------------------------------------------------------------------
                 *          THIS IS A SAMPLE IMPLEMENTATION OF INTEGRATION TO YOUR SYSTEM
                 *          Your code goes here, this is fetching the data that should be sent to Responsr.
                 *          In this sample we have made a mock up SQL query, your application might be using an API. 
                 *          -----------------------------------------------------------------------------------------
                 */

                // Connect to mockup database

                using var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Database"].ConnectionString);
                connection.Open();

                // Gets a list of all clients currently using Responsr, this is populated by responsr
                // We will not send data to clients who are using Responsr in demo mode only.
                var responsrClients = new List<ClientTransactions>();
                foreach (var client in clients.Where(x => !x.IsInDemoMode)) {
                    var responsrClient = new ClientTransactions { ClientId = client.ClientId, Persons = new List<PersonTransaction>() };

                    _logger.LogInformation("Found client: " + client.ClientId);

                    // ----------------------------------
                    // client.ClientId
                    // ----------------------------------
                    //      The clientId of the client in Responsr. This is likely an integration key or similar provided by you
                    //      Make sure these are NOT easily guessed or incorrectly entered such as number series (1,2,3,4 .. ) 
                    //      The clientId is entered by the customer into Responsr and it is likely provided by your organisation through support or a UI

                    // ----------------------------------
                    // recommendedBatchRetrievalTimestamp
                    // ----------------------------------
                    //      Responsr will keep track of the last time you sent data and provide you with a suggested time to fetch changes from based on it.
                    //      You can use this to fetch only new posts. this value is then set further down in the code to be used on your next execution of this
                    //      transfer. The reccomended time will at most be a few days back when you get started. We don't want to send surveys to end customers
                    //      that have not recently been in contact with your client.
                    //
                    //      Stored in UTC time within Responsr

                    // Responsr will make sure that each end customer will only recieve a survey at most once every 30 days. That way we do not spam them.
                    // please send ALL transactions to responsr since they will be used to keep track of how active the end customer is.

                    // ----------------------------------
                    // YOUR CODE GOES HERE : 
                    // ----------------------------------
                    /*
                    var bookings = connection.Query<dynamic>(
                        @"
                            select FirstName, LastName, Email, Price, priceplan, CampaignName, Room, CheckinDate, CheckoutDate, BookingTime, BookingReference
                            from Bookings where CustomerId = @clientId and CheckoutDate > @recommendedBatchRetrievalTimestamp
                        ",
                        new { clientId = client.ClientId, recommendedBatchRetrievalTimestamp }
                    );

                    foreach (var booking in bookings) {

                        _logger.LogInformation("Found booking: " + booking.BookingReference);

                        // Add a person to be sent to responsr
                        responsrClient.Persons.Add(new PersonTransaction {
                            CreatedAt = booking.CheckoutDate,
                            Variables = new Dictionary<string, string> {
                                { "firstname", booking.FirstName },
                                { "lastname", booking.LastName },
                                { "email", booking.Email },
                                { "price", booking.Price },
                                { "pricePlan", booking.Priceplan },
                                { "campaignname", booking.CampaignName },
                                { "room", booking.Room },
                                { "checkin", booking.CheckinDate.ToString() },
                                { "checkout", booking.CheckoutDate.ToString() },
                                { "bookingTime", booking.BookingTime.ToString() },
                                { "bookingreference", booking.BookingReference },
                            }
                        });
                    }
                    */
                    // ----------------------------------
                    // END OF YOUR CODE
                    // ----------------------------------

                    // Add to the list of data to send to responsr
                    responsrClients.Add(responsrClient);
                }

                // ----------------------------------------
                // SUGGESTED NEXT START TIME TO FETCH DATA
                // ----------------------------------------
                //      Set a new suggested datetime for the next call. You will recieve this back from Responsr on the next time you 
                //      Initiate your program, hence no need for you to keep track of this locally

                //      This is what you will recieve as recommendedBatchRetrievalTimestamp 
                //      the next time you run your application. In this case we use the checkoutDate of the last post we retrieved
                var latestRetrievalTimestamp = responsrClients.SelectMany(x => x.Persons).Max(x => DateTime.Parse(x.Variables["CheckoutDate"]));

                //      You should avoid using DateTime.Now as latestRetrievalTimestamp since new posts to be sent to Responsr might be created
                //      during the time this application is running, after having retrieved data. In that case that data might be missed.              
                //
                // var latestRetrievalTimestamp = DateTime.Now;

                // Send the retrieved data to Responsr
                var result = new TransactionBatch(latestRetrievalTimestamp, responsrClients);
                return result;
            }
        }
    }
}
