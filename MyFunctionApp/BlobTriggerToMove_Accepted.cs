using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using System;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;

namespace MyFunctionApp
{
    public static class BlobTriggerToMove_Accepted
    {
        [FunctionName("MoveToAcceptBlob")]
        public static async Task<string> Run([ActivityTrigger] DurableActivityContextBase  myBlob, TraceWriter log)
        {
            //log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            log.Info("in final step......................");
            string storagePath = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            CloudStorageAccount storageAccount =  CloudStorageAccount.Parse(storagePath);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer destinationContainer = blobClient.GetContainerReference(Environment.GetEnvironmentVariable("AcceptDestinationPath"));
            //await cloudBlobContainer.CreateAsync();
            await destinationContainer.CreateIfNotExistsAsync();
          
          


            // Set the permissions so the blobs are public. 
            BlobContainerPermissions permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };

            await destinationContainer.SetPermissionsAsync(permissions);
            RequestApplication requestMetadata = myBlob.GetInput<RequestApplication>();
           
            var sourceBlob = blobClient.GetBlobReferenceFromServerAsync(new Uri(requestMetadata.LocationUrl)).Result;
           // var destinationContainer = blobClient.GetContainerReference(destinationContainer);
            var destinationBlob = destinationContainer.GetBlobReference(sourceBlob.Name);
           await  destinationBlob.StartCopyAsync(sourceBlob.Uri);
            Task.Delay(TimeSpan.FromSeconds(15)).Wait();
           await  sourceBlob.DeleteAsync();
            return "Copied";
        }
    }
}
