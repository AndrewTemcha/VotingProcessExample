using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoreographyExample.DAL
{
    public class Voting
    {
        public Voting()
        {
            Votes = new List<Vote>();
        }
        public Guid Id { get; set; }

        public string VoteFor { get; set; }

        public DateTime CreatedAt { get; set; }

        public VotingStatus VotingStatus { get; set; }

        public ICollection<Vote> Votes { get; set; }
    }
}
