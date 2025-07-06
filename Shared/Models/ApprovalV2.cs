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

    void SetVersion(int newVersion)
    {
        // This method sets a new version for the approval model
        if (newVersion > version)
        {
            Console.WriteLine($"Updating Approval Model Version from {version} to {newVersion}");
            // Logic to update the version can be added here
        }
        else
        {
            Console.WriteLine($"Cannot set version to {newVersion}. It must be greater than current version {version}.");
        }
    }
}
    