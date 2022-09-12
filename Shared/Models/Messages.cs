using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public static class Messages
    {
        public const string NoCapacity = $"Sorry, but the project has no capacity to raise your salary, please talk to your PM/DM";
        public const string TimeOut = $"Sorry, but your salary request didnt get positive response during the timeframe set";

        public const string Success = $"Your Request has been successfully processed";

        public const string Rejected = $"Sorry, but your salary request was rejected, talk to your Delivery Manager";
    }
}
