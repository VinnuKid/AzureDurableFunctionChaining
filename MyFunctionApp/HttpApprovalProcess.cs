using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace MyFunctionApp
{
    public static class HttpApprovalProcess
    {
        [FunctionName("HttpApprovalProcess")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "approval")]HttpRequestMessage req,
            [OrchestrationClient] DurableOrchestrationClient orchestrationClient,
            
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string InstanceId;
            string response;
            
           // req.RequestUri.ParseQueryString().GetValues("name");
            InstanceId= req.RequestUri.ParseQueryString().GetValues("instanceid")[0];

            response =  req.RequestUri.ParseQueryString().GetValues("response")[0];
          //bool choice=  response.Equals("approved");
            if (response.Equals("approved".ToLower()))
            { 
                        await  orchestrationClient.RaiseEventAsync(InstanceId,"Approved",true);
               }

           return  orchestrationClient.CreateCheckStatusResponse(req, InstanceId);
         // return req.CreateResponse(HttpStatusCode.OK,"Request Processed..");
        }
    }
}
