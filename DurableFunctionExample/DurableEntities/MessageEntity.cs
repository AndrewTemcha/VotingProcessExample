using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrchestrationExample.DurableEntities;
internal class MessageEntity
{
    private string _message = string.Empty;
    public void SetMessage(string message)
    {
        _message = message;
    }
    public string GetMessage()
    {
        return _message;
    }
    public void ClearMessage()
    {
        _message = string.Empty;
    }
}
