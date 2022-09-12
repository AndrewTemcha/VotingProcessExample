using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoreographyExample
{
    public class NotificationFunctions
    {
        [FunctionName(nameof(ProcessUserNotification))]
        public static void ProcessUserNotification(
            [QueueTrigger(QueueConstants.NotificationsQueue, Connection = "AzureWebJobsStorage")] string message,
            ILogger log)
        {
            log.LogInformation(message);
        }

        [FunctionName(nameof(NotifyMembers))]
        public static void NotifyMembers(
            [QueueTrigger(QueueConstants.AvailabilityResultQueue, Connection = "AzureWebJobsStorage")] 
            AvailabilityResult result,
            ILogger log) //add Email sender binding
        {
            //send an email with html containing options to either approve or decline the request
            //or just put the message to queue to process it the way you want (using bindings)

            var approvers = new List<Approvers>() 
                { Approvers.ProjectManager, Approvers.DeliveryManager, Approvers.BODMember};

            foreach(var approver in approvers)
            {
                var approveLink = $"/SubmitApproval/{result.ResultFor}/{approver}/true";
                var rejectLink = $"/SubmitApproval/{result.ResultFor}/{approver}/false";

                //send an email with links to each person/groups
                log.LogInformation($"{approver}: Use next link to approve salary request: {approveLink} or reject it with {rejectLink}");
            }
        }
    }
}
