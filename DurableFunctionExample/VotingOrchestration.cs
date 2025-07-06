using DurableFunctionExample.DurableEntities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DurableFunctionExample
{
    public static class VotingOrchestration
    {
        private const int DaysForApproval = 10;

        #region sub-orchestration for board of directors voting process

        /// <summary>
        /// Sub Orchestration for voting specifically for Board of Directors: Message will be sent to all BOD members
        /// </summary>
        [FunctionName(nameof(BoardOfDirectorsApprovalOrchestrator))]
        public static async Task BoardOfDirectorsApprovalOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            [DurableClient] IDurableOrchestrationClient client,
            ILogger log)
        {
            var parentInstanceId = ctx.GetInput<string>();

            var expireAt = ctx.CurrentUtcDateTime.AddDays(DaysForApproval);

            using var cts = new CancellationTokenSource();
            var timerTask = ctx.CreateTimer(expireAt, cts.Token);

            //we need at least 3 approvers from BOD Members
            var approversCount = 0;
            while (true)
            {
                var externalEventTask = ctx.WaitForExternalEvent<ApprovalResult>(ApproversVoting.ApprovalResultEventName);
                var completed = await Task.WhenAny(timerTask, externalEventTask);
                if (completed == timerTask)
                {
                    #region 14 days timeout is over
                    await client.RaiseEventAsync(parentInstanceId, ApproversVoting.ApprovalResultEventName, false);
                    break; // end orchestration - we timed out
                    #endregion
                }
                else if (completed == externalEventTask)
                {
                    #region external event received, checking if it's approved and increasing counter
                    
                    if (externalEventTask.Result.Approver != Approvers.BODMember)
                    {
                        log.LogInformation($"This is A BOD voting, no others can participate");
                        //just skip this vote since it came from wrong person
                        continue;
                    }

                    if (externalEventTask.Result.IsApproved)
                    {
                        approversCount++;
                        if (!ctx.IsReplaying) log.LogInformation($"Approval received from one more BOD member");
                        if (approversCount >= 3)
                        {
                            await client.RaiseEventAsync(parentInstanceId,
                                ApproversVoting.ApprovalResultEventName, 
                                ApprovalResult.ApprovedBy(Approvers.BOD));
                            break;
                        }
                    }
                    else
                    {
                        await client.RaiseEventAsync(
                            parentInstanceId,
                            ApproversVoting.ApprovalResultEventName, 
                            ApprovalResult.DeclinedBy(Approvers.BOD));

                        break;
                    }
                    #endregion
                }
                else
                {
                    throw new InvalidOperationException("Unexpected result from Task.WhenAny");
                }
            }
            cts.Cancel();
        }


        #endregion

        #region main orchestration for voting

        /// <summary>
        /// Orchestration for voting process
        /// </summary>
        [FunctionName(nameof(GetApprovalOrchestrator))]
        public static async Task GetApprovalOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx,
            ILogger log)
        {
            var requesterEmail = ctx.GetInput<string>();

            var availabilityResult = await ctx.CallActivityAsync<AvailabilityResult>(
                nameof(ActivityFunctions.CheckAvailabilityFor), 
                requesterEmail);

            if (!availabilityResult.IsAvailable)
            {
                await ctx.CallActivityAsync(
                        nameof(ActivityFunctions.NotifyRequester),
                        Messages.NoCapacity);
                return;
            }

            var expireAt = ctx.CurrentUtcDateTime.AddDays(DaysForApproval);

            var cts = new CancellationTokenSource();

            var timerTask = ctx.CreateTimer(expireAt, cts.Token);

            //starting sub-orchestrator to get approvals from Board of directors
            var BODApproavalOrchestrationInstanceId = ctx.StartNewOrchestration(
                nameof(BoardOfDirectorsApprovalOrchestrator), 
                ctx.InstanceId);

            if (!ctx.IsReplaying) log.LogInformation($"BOD voting started. Process id = {BODApproavalOrchestrationInstanceId}");

            await ctx.CallActivityAsync(nameof(ActivityFunctions.NotifyBODMembers), BODApproavalOrchestrationInstanceId);

            var voting = new ApproversVoting();

            //We address entity by using id
            //In this case we ensure that we have only one entity per voting process by using orchestration ID as a key
            var entityId = new EntityId(nameof(VotingEntity), ctx.InstanceId);

            var voringEntity = ctx.CreateEntityProxy<IVotingEntity>(entityId);

            voringEntity.NewVotingFor(requesterEmail);

            while (true) 
            {
                var externalEventTask = ctx.WaitForExternalEvent<ApprovalResult>(ApproversVoting.ApprovalResultEventName);
                var completed = await Task.WhenAny(timerTask, externalEventTask);
                if (completed == timerTask)
                {
                    // end orchestration - we timed out
                    await ctx.CallActivityAsync(
                        nameof(ActivityFunctions.NotifyRequester),
                        Messages.TimeOut);
                    break; 
                }
                else if (completed == externalEventTask)
                {
                    var approver = externalEventTask.Result.Approver;
                    if (externalEventTask.Result.IsApproved)
                    {
                        await voringEntity.AddApprover(approver);

                        voting.ApprovedBy.Add(approver);
                        if (!ctx.IsReplaying) log.LogInformation($"Approval received from {approver}");
                        if (voting.IsApprovedByAll())
                        {
                            if (!ctx.IsReplaying) log.LogInformation($"Approved ({voting.ApprovedBy.Count} approvals received)");

                            await ctx.CallActivityAsync(
                                nameof(ActivityFunctions.NotifyRequester),
                                Messages.Success);
                            break;
                        }
                    }
                    else
                    {
                        if (!ctx.IsReplaying) log.LogWarning($"Rejected by {approver}");
                        await ctx.CallActivityAsync(
                            nameof(ActivityFunctions.NotifyRequester),
                            Messages.Rejected);
                        break;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Unexpected result from Task.WhenAny");
                }
            }
            cts.Cancel();
        }
        #endregion

        #region http function to trigger orchestration 
        /// <summary>
        /// Function serves as a starting point to trigger orchestration flow
        /// </summary>
        [FunctionName("Start_Orchestration")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Start_Orchestration/{email}")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            string email,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync<string>(nameof(GetApprovalOrchestrator), email);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
        #endregion
    }
}
