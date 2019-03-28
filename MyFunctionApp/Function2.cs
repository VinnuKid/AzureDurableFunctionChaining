using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace MyFunctionApp
{
    public static class Function2
    {
        [FunctionName("A_Function")]
        public static  void  Run(
         [ActivityTrigger] DurableActivityContext contextBase ,  
             TraceWriter log)
        {
            string url=contextBase.GetInput<RequestApplication>().LocationUrl;
            url = url + "sample-items";

            

            log.Info($"I am in Activity Function" +"<br/>"+
               $"I have Location URL as {url}" +
                "" +
                "");

            
           // return req.CreateResponse(HttpStatusCode.OK, url);
        }
    }
}
