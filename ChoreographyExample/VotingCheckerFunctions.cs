using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChoreographyExample.DAL;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Extensions;
using Shared.Models;

namespace ChoreographyExample
{
    public class VotingCheckerFunctions
    {
        private readonly VotingDbContext _context;

        public VotingCheckerFunctions(VotingDbContext context)
        {
            _context = context;
        }

        [FunctionName(nameof(VotingProgressChecker))]
        public async Task VotingProgressChecker(
           [TimerTrigger("0 */10 * * * *")] TimerInfo myTimer,
           [Queue(QueueConstants.NotificationsQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> notificationsCollector,
           ILogger log)
        {
            //get all votings which are timed out and are not in complete/rejected state
            var existingVotings = await _context
                .Votings
                .Include(v => v.Votes)
                .Where(v => v.CreatedAt > DateTime.UtcNow.AddDays(-14))
                .Where(v => v.VotingStatus == VotingStatus.RaiseAvailable)
                .ToListAsync();

            foreach(var voting in existingVotings)
            {
                var (isApproved, anyDisapproval) = CheckApprovalState(voting);

                if(anyDisapproval){
                    voting.VotingStatus = VotingStatus.Rejected;
                    //notify user
                    await notificationsCollector.AddAsync(Messages.Rejected);
                }
                else if (isApproved)
                {
                    voting.VotingStatus = VotingStatus.Approved;
                    //notify user
                    await notificationsCollector.AddAsync(Messages.Success);
                }
            }

            await _context.SaveChangesAsync();
        }

        private static (bool, bool) CheckApprovalState( Voting voting)
        {
            var approvers = new List<Approvers>();
            var bodMembersCount = 0;
            foreach (var vote in voting.Votes)
            {
                if (vote.Approver == Approvers.BODMember)
                {
                    bodMembersCount++;
                }
                else
                {
                    approvers.Add(vote.Approver);
                }
                if (bodMembersCount >= 3)
                {
                    approvers.Add(Approvers.BOD);
                }
            }
            return (approvers.ToBitFlags().HasFlag(Approvers.RequiredForDecision), 
                voting.Votes.Any(v=> !v.IsApproved));


        }

        [FunctionName(nameof(VotingStatusChecker))]
        public async Task VotingStatusChecker(
            [TimerTrigger("0 */10 * * * *")]TimerInfo myTimer,
            [Queue(QueueConstants.NotificationsQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> notificationsCollector,
            ILogger log)
        {
            //get all votings which are timed out and are not in complete/rejected state
            var expiredVotings = await _context
                .Votings
                .Where(v => v.CreatedAt < DateTime.UtcNow.AddDays(-14))
                .Where(v => v.VotingStatus != VotingStatus.Rejected && v.VotingStatus != VotingStatus.Approved)
                .ToListAsync();

            var notifications = new List<Task>();

            //update their status to rejected and notify all users
            expiredVotings.ForEach(v =>
            {
                v.VotingStatus = VotingStatus.Rejected;
                notifications.Add(notificationsCollector.AddAsync(Messages.TimeOut));
            });

            await Task.WhenAll(notifications.Append(_context.SaveChangesAsync()));
        }
    }
}
