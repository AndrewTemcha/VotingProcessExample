using Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class ApproversVoting
    {
        public const string ApprovalResultEventName = nameof(ApprovalResultEventName);
        public List<Approvers> ApprovedBy { get; set; } = new List<Approvers>();

        public bool IsApprovedByAll()
        {
            return ApprovedBy.ToBitFlags().HasFlag(Approvers.RequiredForDecision);
        }
    }
}
