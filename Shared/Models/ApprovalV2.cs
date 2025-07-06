using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models;
internal class ApprovalV2
{
    const int version = 2; // Version of the approval model

    void GetVersion()
    {
        // This method returns the version of the approval model
        Console.WriteLine($"Approval Model Version: {version}");
    }
}
    