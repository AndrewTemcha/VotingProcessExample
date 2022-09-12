using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Models;
using System;
using System.Threading.Tasks;

namespace DurableFunctionExample
{
    public static class ActivityFunctions
    {
        [FunctionName(nameof(CheckAvailabilityFor))]
        public static async Task<AvailabilityResult> CheckAvailabilityFor([ActivityTrigger] string email, ILogger log)
        {
            //check against specific user if project has enough $ to raise his salary
            log.LogInformation($"Checking availability for {email}");

            await Task.Delay(2000);

            return new AvailabilityResult()
            {
                IsAvailable = true,
                Threshold = 500,
            };
        }
        [FunctionName(nameof(NotifyBODMembers))]
        public static void NotifyBODMembers(
            [ActivityTrigger] string instanceId,
            ILogger log) //add Email sender binding
        {
            //send an email with html containing options to either approve or decline the request
            //or just put the message to queue to process it the way you want (using bindings)

            var approveLink = $"/SubmitApproval/{instanceId}/{Approvers.BODMember}/true";
            var rejectLink = $"/SubmitApproval/{instanceId}/{Approvers.BODMember}/false";

            log.LogInformation($"Use next link to approve salary request: {approveLink} or reject it with {rejectLink}");
        }

        [FunctionName(nameof(NotifyRequester))]
        public static void NotifyRequester([ActivityTrigger] string message, ILogger log) //add Twillio binding
        {
            log.LogInformation(message);
        }

        [FunctionName(nameof(ProcessQueueMessages))]
        public static void ProcessQueueMessages([QueueTrigger(QueueConstants.ApproversQueue, Connection = "AzureWebJobsStorage")] string message, ILogger log)
        {
            log.LogInformation(message);
        }

        [FunctionName(nameof(SubmitApproval))]
        public static async Task SubmitApproval(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "SubmitApproval/{instanceId}/{approvedBy}/{isApproved}")] HttpRequest req,
            string instanceId,
            string approvedBy,
            bool isApproved,
            [DurableClient] IDurableOrchestrationClient client)
        {
            Enum.TryParse(approvedBy, out Approvers approver);

            ValidateApprover(approver);

            await client.RaiseEventAsync(
                instanceId, 
                ApproversVoting.ApprovalResultEventName, 
                isApproved ? 
                ApprovalResult.ApprovedBy(approver) : 
                ApprovalResult.DeclinedBy(approver));
        }

        private static void ValidateApprover(Approvers approvedBy)
        {
            if (approvedBy != Approvers.ProjectManager && approvedBy != Approvers.DeliveryManager && approvedBy != Approvers.BODMember)
            {
                throw new ArgumentException($"Approver {approvedBy} is not allowed to vote");
            }
        }
    }
}
