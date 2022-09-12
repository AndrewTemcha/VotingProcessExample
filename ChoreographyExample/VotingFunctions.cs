using System;
using System.Net.Http;
using System.Threading.Tasks;
using ChoreographyExample.DAL;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Shared;

namespace ChoreographyExample
{
    public class VotingFunctions
    {
        [FunctionName(nameof(StartVotingProcess))]
        public async Task StartVotingProcess(
            [QueueTrigger(QueueConstants.VotingItemsQueue, Connection = "ChoreographyExampleConnectionString")]string newRequesterEmail,
            [Sql("dbo.Voting", ConnectionStringSetting = "SqlConnectionString")] IAsyncCollector<Voting> votingItems,
            [Queue(QueueConstants.AvailabilityQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> availabilityQueueCollector,
            ILogger log)
        {
            var votingProcess = new Voting
            {
                Id = Guid.NewGuid(),
                VoteFor = newRequesterEmail,
                VotingStatus = VotingStatus.Initialised,
                CreatedAt = DateTime.UtcNow
            };

            await votingItems.AddAsync(votingProcess);

            log.LogInformation($"Initialased new voting with id = {votingProcess.Id}");

            await availabilityQueueCollector.AddAsync(newRequesterEmail);
        }

        [FunctionName("Start_Choreography")]
        public static async Task<HttpResponseMessage> HttpStart(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Start_Choreography/{email}")] HttpRequestMessage req,
           [Queue(QueueConstants.VotingItemsQueue, Connection = "AzureWebJobsStorage")] IAsyncCollector<string> queueCollector,
           string email,
           ILogger log)
        {
            await queueCollector.AddAsync(email);

            var message = $"Voting process for {email} has just started";

            log.LogInformation(message);

            return req.CreateResponse(System.Net.HttpStatusCode.Accepted, message);
        }
    }
}
