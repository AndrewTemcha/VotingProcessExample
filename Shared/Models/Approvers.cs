using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    [Flags]
    public enum Approvers
    {
        ProjectManager = 1,
        DeliveryManager = 2,
        BOD = 4,
        BODMember = 8,        

        RequiredForDecision = ProjectManager | DeliveryManager | BOD
    }
}
