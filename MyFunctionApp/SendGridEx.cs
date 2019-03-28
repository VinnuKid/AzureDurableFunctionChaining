// The 'From' and 'To' fields are automatically populated with the values specified by the binding settings.
//
// You can also optionally configure the default From/To addresses globally via host.config, e.g.:
//
// {
//   "sendGrid": {
//      "to": "user@host.com",
//      "from": "Azure Functions <samples@functions.com>"
//   }
// }
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SendGrid.Helpers.Mail;
using SendGrid;
using System;
using System.Threading.Tasks;

namespace MyFunctionApp
{
    public static class SendGridEx
    {
        [FunctionName("A_SendGridEx")]
        [return: SendGrid(ApiKey = "SendGridApiKey", To = "CustomerMail", From = "n.vinod.kumar777@gmail.com")]
        public static Mail
            Run([ActivityTrigger] RequestApplication request, 
                        TraceWriter log)
        {
            log.Info("Sending Mail....Please wait");
           var ToAddress= Environment.GetEnvironmentVariable("CustomerMail");
            string storagePath = Environment.GetEnvironmentVariable("StorageaccountBaseURL") + Environment.GetEnvironmentVariable("inputStorageContainer") + request.name;
            string FunctionBasePath = Environment.GetEnvironmentVariable("FunctionBasePath");
            string mailTemplate = Environment.GetEnvironmentVariable("MailTemplate");
            string instanceid = request.InstanceId;

            Mail message = new Mail();
            message = new Mail();
            var personalization = new Personalization();
            personalization.AddTo(new Email(ToAddress));
            message.AddPersonalization(personalization);       
            log.Info( request.LocationUrl+ request.name+request.InstanceId);
            var messageContent = new Content("text/html", string.Format(mailTemplate, storagePath, FunctionBasePath, instanceid));
            message.AddContent(messageContent);
            message.Subject = "Request for Approval";
            log.Info("Mail Sent....Please chek with your admin");

            return message;
        }
    }
}
