using ChoreographyExample.DAL;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ChoreographyExample
{
    public class AvailabilityFunctions
    {
        private readonly VotingDbContext _dbContext;

        public AvailabilityFunctions(VotingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [FunctionName(nameof(CheckAvailabilityFor))]
        public async Task CheckAvailabilityFor(
            [QueueTrigger(QueueConstants.AvailabilityQueue, Connection = "AzureWebJobsStorage")] string newRequesterEmail,
            [Queue(QueueConstants.NotificationsQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> notificationsCollector,
            [Queue(QueueConstants.AvailabilityResultQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<AvailabilityResult> resultCollector,
            ILogger log)
        {
            //check against specific user if project has enough $ to raise his salary
            log.LogInformation($"Checking availability for {newRequesterEmail}");
            //simulate some work
            await Task.Delay(2000);            

            var result = new AvailabilityResult()
            {
                IsAvailable = true,
                Threshold = 500,
                ResultFor = newRequesterEmail
            }; //this is going to be a result of some calculations

            var voring = await _dbContext
                .Votings
                .Where(v => v.VoteFor == newRequesterEmail)
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefaultAsync();

            if (result.IsAvailable)
            {
                voring.VotingStatus = VotingStatus.RaiseAvailable;
                await resultCollector.AddAsync(result);
            }            
            else
            {
                voring.VotingStatus = VotingStatus.Rejected;
                await notificationsCollector.AddAsync(Messages.NoCapacity);
            }
            await _dbContext.SaveChangesAsync();
        }
    }
}
