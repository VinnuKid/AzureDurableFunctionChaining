using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System.Threading;

namespace FunctionApp1
{
    public static class Function1
    {

        

        private static string key = TelemetryConfiguration.Active.InstrumentationKey = System.Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process);
        private static TelemetryClient telemetry = new TelemetryClient()
        {
            InstrumentationKey = "b6838dcc-e0b3-4a39-bf7d-f2261178d69a"

        };

        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(

            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {

            string name;
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                name = req.Query["name"];

                log.LogInformation(null);
                
                dynamic data = JsonConvert.DeserializeObject(null);
                name = name ?? data?.name;


                return name != null
                    ? (ActionResult)new OkObjectResult($"Hello, {name}")
                    : new BadRequestObjectResult("Please pass a name on the query string or in the request body");


            }

            catch (Exception ex)
            {
                telemetry.TrackException(ex);
                log.LogInformation(ex.Message);
                return null;
            }
         


             }
    }
}
