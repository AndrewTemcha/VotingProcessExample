using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class ApprovalResult
    {
        public Approvers Approver { get; set; }

        public bool IsApproved { get; set; }

        public static ApprovalResult ApprovedBy(Approvers role)
        {
            return new ApprovalResult { Approver = role, IsApproved = true };
        }

        public static ApprovalResult DeclinedBy(Approvers role)
        {
            return new ApprovalResult { Approver = role, IsApproved = false };
        }
    }
}
