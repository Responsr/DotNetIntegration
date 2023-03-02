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
 *  - Fetch only for clients already activly using Responsr
 *  - Fetch all active customer relations for clients
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

            // Create a runner to send data to Responsr, this part is standard
            var runner = new RelationsUpdateRunner(new RelationsUpdateRetriever(), new Guid(ConfigurationManager.AppSettings["PartnerKey"]), ConfigurationManager.AppSettings["LogFolderPath"], ResponsrEnvironment.Production);
            runner.Run();

            Debug("Application finished");
        }

        private class RelationsUpdateRetriever : IRelationsUpdateRetriever
        {
            public RelationsUpdate Retrieve(List<ClientInfo> clients, List<VariableInfo> variables)
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
                var responsrClients = new List<ClientRelations>();
                foreach (var client in clients.Where(x => !x.IsInDemoMode)) {
                    var responsrClient = new ClientRelations { ClientId = client.ClientId, Persons = new List<PersonRelation>() };

                    _logger.LogInformation("Found client: " + client.ClientId);

                    // ----------------------------------
                    // client.ClientId
                    // ----------------------------------
                    //      The clientId of the client in Responsr. This is likely an integration key or similar provided by you
                    //      Make sure these are NOT easily guessed or incorrectly entered such as number series (1,2,3,4 .. ) 
                    //      The clientId is entered by the customer into Responsr and it is likely provided by your organisation through support or a UI

                    // ----------------------------------
                    // YOUR CODE GOES HERE : 
                    // ----------------------------------
                    /*
                    var relations = connection.Query<dynamic>(
                        @"
                            select FirstName, LastName, Email, MobilePhone, Company
                            from Relations where ClientId = @clientId
                        ",
                        new { clientId = client.ClientId }
                    );

                    foreach (var relation in relations) {
                        // Add a person to be sent to responsr
                        responsrClient.Persons.Add(new PersonRelation {
                            Variables = new Dictionary<string, string> {
                                { "firstname", relation.FirstName },
                                { "lastname", relation.LastName },
                                { "email", relation.Email },
                                { "mobilephone", relation.MobilePhone },
                                { "company", relation.Company },
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

                // Send the retrieved data to Responsr
                var result = new RelationsUpdate(responsrClients);
                return result;
            }
        }
    }
}
