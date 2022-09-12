using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Extensions
{
    public static class ApproversExtensions
    {
        public static Approvers ToBitFlags(this List<Approvers> flags)
        {
            Approvers result = 0;
            foreach (Approvers f in flags)
            {
                result |= f;
            }
            return result;
        }
    }
}
