using Shared.Models;
using System.Threading.Tasks;

namespace DurableFunctionExample.DurableEntities
{
    public interface IVotingEntity
    {
        Task AddApprover(Approvers approver);
        void NewVotingFor(string votingFor);
    }
}