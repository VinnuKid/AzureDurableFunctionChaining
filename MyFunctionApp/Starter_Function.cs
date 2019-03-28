using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace MyFunctionApp
{
    public static class Starter_Function
    {
        [FunctionName("Starter_Function")]
        public static async void Run([BlobTrigger("sample-items/{name}")] Stream req,
         [OrchestrationClient] DurableOrchestrationClient starter, string name,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            RequestApplication blobDetails = new RequestApplication();

            string fileName = name;
            string storagePath = Environment.GetEnvironmentVariable("StorageaccountBaseURL") + Environment.GetEnvironmentVariable("inputStorageContainer") + name;
            blobDetails.name = fileName;
            blobDetails.LocationUrl = storagePath;

           await  starter.StartNewAsync("Orchestration_Function", blobDetails);


           // string id = await Starter.StartNewAsync("Orchestration_Function", blobDetails);

        }


        [FunctionName("Orchestration_Function")]
        public static async Task<string>
          Orchestration_Function([OrchestrationTrigger] DurableOrchestrationContext contextBase,
          TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            RequestApplication details = contextBase.GetInput<RequestApplication>();
            details.InstanceId = contextBase.InstanceId;
            //  await contextBase.CallActivityAsync("A_Function", details);   //Function2 has this function name
            await contextBase.CallActivityAsync("A_SendGridEx", details);

            var cts = new CancellationTokenSource();
            DateTime ExpirationTime = contextBase.CurrentUtcDateTime.AddMinutes(5);
            Task timeoutTask = contextBase.CreateTimer(ExpirationTime, cts.Token);

            var ApprovalTask = contextBase.WaitForExternalEvent<bool>("Approved");

            Task Winner = await Task.WhenAny(ApprovalTask, timeoutTask);

            if (Winner == ApprovalTask)
            {
                bool isapproved = ApprovalTask.Result;//contextBase.WaitForExternalEvent<bool>("Approved").Result;
                if (isapproved)
                {
                    await contextBase.CallActivityAsync("MoveToAcceptBlob", details);
                    cts.Cancel();
                }

            }
            else if (Winner == timeoutTask)
            {
                log.Info("File not Accepted. Admin haven't replied to the mail.Timeout");

            }

            else
            {

                log.Info("Error Happened........................");
                cts.Cancel();
            }
            return " ";

        }
    }
}
