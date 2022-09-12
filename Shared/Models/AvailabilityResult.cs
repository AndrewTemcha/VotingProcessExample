using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class AvailabilityResult
    {
        public decimal Threshold { get; set; }

        public bool IsAvailable { get; set; }

        public bool IsNegotiable { get; set; }

        public string ResultFor { get; set; }
    }
}
