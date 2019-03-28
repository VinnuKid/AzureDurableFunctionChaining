//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Microsoft.Azure.Cosmos;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
//using Microsoft.Azure.WebJobs.Host;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;

//namespace CosmosDB
//{

   

//    public static class O_Function
//    {

//        private static readonly string EndpointUri = Environment.GetEnvironmentVariable("EndpointUri");
//        // The primary key for the Azure Cosmos account.
//        private static readonly string PrimaryKey = Environment.GetEnvironmentVariable("PrimaryKey");

//        // The Cosmos client instance
//        private static CosmosClient cosmosClient;

//        private static string partitionKey;



//        [FunctionName("O_Function")]
//        public static async Task<List<string>> RunOrchestrator(
//            [OrchestrationTrigger] DurableOrchestrationContext context, ILogger log)
//        {
//            var outputs = new List<string>();

//            var family = context.GetInput<Family>();
//            outputs.Add(await context.CallActivityAsync<string>("CreateDocuments", family));
//            outputs.Add(await context.CallActivityAsync<string>("ListDocument", family));

            
//            return outputs;
//        }





//        [FunctionName("CreateDocuments")]
//        public static async Task CreateDocuments([ActivityTrigger] DurableActivityContext context, ILogger log)
//        {
//            var family = context.GetInput<string>();
//            var familyObject = JsonConvert.DeserializeObject<Family>(family);

//            log.LogInformation(familyObject.Address.County);


//           var container= await GetContainer();
//           var item= await container.Container.Items.ReadItemAsync<Family>(familyObject.LastName, familyObject.Id);

//            if (item.StatusCode == HttpStatusCode.NotFound)
//            {
//               await  container.Container.Items.CreateItemAsync(familyObject.LastName, familyObject);
//            }
            
//        }



//        [FunctionName("ListDocument")]
//        public static async Task ListDocument([ActivityTrigger] DurableActivityContext context, ILogger log)
//        {
//            var sqlQueryText = "SELECT * FROM c";
//            var partitionKey = "Andersen";
//            Console.WriteLine("Running query: {0}\n", sqlQueryText);
//            var container=await GetContainer();
           
//            CosmosSqlQueryDefinition queryDefinition = new CosmosSqlQueryDefinition(sqlQueryText);
//            CosmosResultSetIterator<Family> queryResultSetIterator = container.Container.Items.CreateItemQuery<Family>(queryDefinition, partitionKey);

//            List<Family> families = new List<Family>();

//            while (queryResultSetIterator.HasMoreResults)
//            {
//                CosmosQueryResponse<Family> currentResultSet = await queryResultSetIterator.FetchNextSetAsync();
//                foreach (Family family in currentResultSet)
//                {
//                    families.Add(family);
//                    log.LogInformation("\tRead {0}\n", family);
//                }
//            }
//        }





//        [FunctionName("O_Function_HttpStart")]
//        public static async Task<HttpResponseMessage> HttpStart(
//            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
//            [OrchestrationClient]DurableOrchestrationClient starter,
//            ILogger log)
//        {
//            // Function input comes from the request content.

//            var family = await req.Content.ReadAsAsync<Family>();

//            string instanceId = await starter.StartNewAsync("O_Function", family);
            
//            return starter.CreateCheckStatusResponse(req, instanceId);
//        }

//        private static async Task<CosmosContainerResponse> GetContainer()
//        {

//            string databaseId = Environment.GetEnvironmentVariable("databaseId");
//            string containerId = Environment.GetEnvironmentVariable("containerId");
//            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
//            var database = await cosmosClient.Databases.CreateDatabaseIfNotExistsAsync(databaseId);
//            Console.WriteLine("Created Database: {0}\n", database.Database.Id);
//            var container = await database.Database.Containers.CreateContainerIfNotExistsAsync(containerId, "/LastName");
//            return container;
//        }
//    }
//}