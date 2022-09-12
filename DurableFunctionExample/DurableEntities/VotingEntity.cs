using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Shared;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DurableFunctionExample.DurableEntities
{
    public class VotingEntity : IVotingEntity
    {
        [JsonIgnore]
        private readonly IAsyncCollector<string> _queueCollector;

        public string VotingFor { get; set; }

        public HashSet<Approvers> Approvers { get; set; } = new HashSet<Approvers>();
        public VotingEntity(IAsyncCollector<string> queueCollector)
        {
            _queueCollector = queueCollector;
        }

        public void NewVotingFor(string votingFor)
        {
            VotingFor = votingFor;
        }

        public async Task AddApprover(Approvers approver)
        {
            if (Approvers.Add(approver))
            {
                await NotifyApproverAdded(approver);
            }            
        }

        private async Task NotifyApproverAdded(Approvers approver)
        {
            await _queueCollector.AddAsync($"{approver} has just voted for salary request for {VotingFor}");
        }

        [FunctionName(nameof(VotingEntity))]
        public static Task Run(
            [EntityTrigger] IDurableEntityContext ctx,
            [Queue(QueueConstants.ApproversQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> queueCollector)         
                => ctx.DispatchAsync<VotingEntity>(queueCollector);
    }
}
