//using System.IO;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Azure.WebJobs.Host;
//using Newtonsoft.Json;
//using System;
//using Microsoft.Azure.Cosmos.Table;
//using System.Collections.Generic;
//using System.Net.Http;
//using System.Linq;
//using System.Net;
//using System.Text;

//using Microsoft.Extensions.Logging;
//using System.Threading.Tasks;


//namespace TableStorage
//{

//    public class StorageTableReader
//    {
//        private readonly ILogger _log;

//        public StorageTableReader(ILoggerFactory loggerFactory)
//        {
//            _log = loggerFactory.CreateLogger<StorageTableReader>();
//        }



//        //var tableQuery = new TableQuery() .where('Name == ? or Name <= ?', 'Person1', 'Person2'); .or('Age >= ?', 18);
//        //https://azure.github.io/azure-storage-node/TableQuery.html

//        [FunctionName("TableReader")]
//        public async Task<HttpResponseMessage> TableReader([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
//        [Table("test")] CloudTable tab)
//        {
//            _log.LogInformation("starting...");

//            string json = await req.ReadAsStringAsync();
//            var requestObject = JsonConvert.DeserializeObject<B2CAttributes>(json);

//            _log.LogInformation("Email: " + requestObject.Email);
//            _log.LogInformation("MemNo: " + requestObject.MembershipNumber);

//            // Load data from TableStorage using CloudTable
//            var emailFilter = TableQuery.GenerateFilterCondition("email", QueryComparisons.Equal, requestObject.Email);
//            var membershipNumberFilter = TableQuery.GenerateFilterCondition("membership_number", QueryComparisons.Equal, requestObject.MembershipNumber);
//            var querySegment = await tab.ExecuteQuerySegmentedAsync(new TableQuery<TabEntity>().Where(TableQuery.CombineFilters(emailFilter, TableOperators.And, membershipNumberFilter)), null);

//            StringContent responseContent = default!;
//            foreach (TabEntity item in querySegment)
//            {
//                //log.Info($"Data loaded: '{item.PartitionKey}' | '{item.RowKey}' | '{item.rowX}' | '{item.rowY}'");
//                responseContent = new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json");
//            }

//            //log.Info("Done.");
//            return new HttpResponseMessage(HttpStatusCode.OK)
//            {
//                Content = responseContent
//            };
//        }

//        [FunctionName("ValidateMemebershipNumber")]
//        public static async Task<HttpResponseMessage> ValidateMemebershipNumber([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, 
//        ILogger log, 
//        [Table("test")] CloudTable tab)
//        {
//            // read in the request body (POST only)
//            string requestbody = await new StreamReader(req.Body).ReadToEndAsync();
//            var b2CAttributes = JsonConvert.DeserializeObject<B2CAttributes>(requestbody);

//            // no point continuing to check the table if the input data is not valid
//            if (!String.IsNullOrWhiteSpace(b2CAttributes.MembershipNumber) && !String.IsNullOrWhiteSpace(b2CAttributes.Email))
//            {

//                // Create query filters to only return where email and membership number matches
//                var emailFilter = TableQuery.GenerateFilterCondition("email", QueryComparisons.Equal, b2CAttributes.Email);
//                var membershipNumberFilter = TableQuery.GenerateFilterCondition("membership_number", QueryComparisons.Equal, b2CAttributes.MembershipNumber);
//                var combinedFilters = TableQuery.CombineFilters(emailFilter, TableOperators.And, membershipNumberFilter);

//                // Load data from TableStorage using CloudTable
//                var querySegment = tab.ExecuteQuery(new TableQuery<TabEntity>().Where(combinedFilters), null).ToList(); // todo; this is potentially unsafe and needs try/catch.

//                // Only return happy if record exists in table
//                if (querySegment.Any())
//                {
//                    // delete matched record so user cannot sign up again? Also prunes table. If you delete you can check the table for how many are still to sign up and target them with comms.
//                    querySegment.ForEach(_ =>
//                    {
//                        try
//                        {
//                            tab.Execute(TableOperation.Delete(_));
//                        }
//                        catch (System.Exception e) // do not stop execution flow. This is not the users fault and they should not be punished. Log error and move on.
//                        {
//                            // log error with membership number so we can find it. Note, don't think we want to log email as it can be classified as PII when combined with other data.
//                            log.LogError(e, $"User with membership number '{_.membership_number}' not deleted from data table.");
//                        }
//                    });

//                    return new HttpResponseMessage(HttpStatusCode.OK)
//                    {
//                        Content = new StringContent(JsonConvert.SerializeObject(new ContinueResponse()), Encoding.UTF8, "application/json")
//                    };
//                }
//            }

//            // If the code got to here, there was no matching record in the data table.
//            return new HttpResponseMessage(HttpStatusCode.BadRequest)
//            {
//                Content = new StringContent(JsonConvert.SerializeObject(new ErrorResponse()), Encoding.UTF8, "application/json")
//            };

//        }

//        public sealed class ContinueResponse
//        {
//            public string Version => "1.0.0";
//            public string Action => "Continue";
//            public string UserMessage => "";
//        }

//        public sealed class ErrorResponse
//        {
//            public string Version => "1.0.0";
//            public string Action => "ValidationError";
//            public string UserMessage => "We could not validate your membership number. Please call xxxxxx";
//            public int Status => 400;
//        }

//        public sealed class B2CAttributes
//        {
//            [JsonProperty("email")]
//            public string? Email { get; set; }
//            [JsonProperty("extension_membership_number")]
//            public string? MembershipNumber { get; set; }
//        }

//        public sealed class TabEntity : TableEntity
//        {
//            public string? email { get; set; }
//            public string? membership_number { get; set; }
//        }
//    }
//}