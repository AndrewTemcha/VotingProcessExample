using Microsoft.EntityFrameworkCore;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoreographyExample.DAL
{
    public class Vote
    {
        public Guid Id { get; set; }
        public Approvers Approver { get; set; }

        public bool IsApproved { get; set; }

        public Voting Voting { get; set; } 
    }
}
