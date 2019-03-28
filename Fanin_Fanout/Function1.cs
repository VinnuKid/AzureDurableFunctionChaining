using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Fanin_Fanout
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer,
            [OrchestrationClient] DurableOrchestrationClient starter,
            TraceWriter log)
        {

            starter.StartNewAsync("E2_BackupSiteContent", null);
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }

    public static class BackupSiteContent
    {
       static  string  connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
       static string inputContainerReference = Environment.GetEnvironmentVariable("InputContainerReference");
        static string outputContainerReference = Environment.GetEnvironmentVariable("outputContainerReference");


        [FunctionName("E2_BackupSiteContent")]
        public static async void Run(
            [OrchestrationTrigger] DurableOrchestrationContext backupContext)
        {
           

            List<RequestApplication> files = await backupContext.CallActivityAsync<List<RequestApplication>>(
                 "E2_GetFileList",
                 null);
            files.Count();
            var Fileslist = files.ToArray();//.ToArray<IListBlobItem>();


            var tasks = new Task<string>[files.Count()];
            for (int i = 0; i < Fileslist.Count(); i++)
            {
                tasks[i] = backupContext.CallActivityAsync<string>(
                    "E2_CopyFileToBlob", Fileslist[i]);
            }
            // Object reference not set to an instance of an object.
            await Task.WhenAll(tasks);

           
        }

        [FunctionName("E2_GetFileList")]
        public static List<RequestApplication> GetFileList(
            [ActivityTrigger] string rootDirectory,
                       TraceWriter log)
        {
            CloudBlobClient blobClient = GetBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(inputContainerReference);
            var blobList = container.ListBlobs();
          // var ArrayList= (IEnumerable<CloudBlockBlob>)blobList;
            // return ArrayList;
            ICloudBlob bi;
            List<RequestApplication> inputBlobDetails = new List<RequestApplication>();
            foreach (var blob in blobList)
            {
                bi = (CloudBlockBlob)blob;
                RequestApplication requestApplication = new RequestApplication();
                requestApplication.name = bi.Name;
                requestApplication.LocationUrl = bi.Uri;
                inputBlobDetails.Add(requestApplication);
               

            }
            return inputBlobDetails;
        }

       

        [FunctionName("E2_CopyFileToBlob")]
        public static async Task<string> CopyFileToBlob(
            [ActivityTrigger] DurableActivityContext  filePath,
            Binder binder,
            TraceWriter log)
        {


            CloudBlobClient blobClient = GetBlobClient();
            CloudBlobContainer destinationContainer = blobClient.GetContainerReference(outputContainerReference);
            await destinationContainer.CreateIfNotExistsAsync();
            var blob = filePath.GetInput<RequestApplication>();
            var sourceBlob = blobClient.GetBlobReferenceFromServerAsync(new Uri(blob.LocationUrl.ToString())).Result;
            var destinationBlob = destinationContainer.GetBlobReference(sourceBlob.Name);
            await destinationBlob.StartCopyAsync(sourceBlob.Uri);
            Task.Delay(TimeSpan.FromSeconds(15)).Wait();
            await sourceBlob.DeleteAsync();
            return "Copied";

        }

        private static CloudBlobClient GetBlobClient()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            return blobClient;
        }
    }
}
