using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ChoreographyExample.DAL;
using Shared.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ChoreographyExample
{
    public class ApprovalFunctions
    {
        private readonly VotingDbContext _dbContext;

        public ApprovalFunctions(VotingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [FunctionName(nameof(SubmitApproval))]
        public async Task SubmitApproval(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "SubmitApproval/{userEmail}/{approvedBy}/{isApproved}")] HttpRequest req,
            string userEmail,
            string approvedBy,
            bool isApproved)
        {
            Enum.TryParse(approvedBy, out Approvers approver);

            var voting = await _dbContext
                .Votings
                .Include(v => v.Votes)
                .FirstOrDefaultAsync(voting => voting.VoteFor == userEmail);

            voting.Votes.Add(new Vote
            {
                Id = Guid.NewGuid(),
                Approver = approver,
                IsApproved = isApproved
            });

            await _dbContext.SaveChangesAsync();
        }
    }
}
