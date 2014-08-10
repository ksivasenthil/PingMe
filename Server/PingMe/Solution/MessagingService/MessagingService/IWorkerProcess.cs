using MessagingEntities;
using System.Collections.Generic;

namespace MessagingService
{
    internal interface IWorkerProcess
    {
        bool PostMessage(MessagePing messageDetails);

        List<MessagePing> FetchMessages(string destination);
    }
}
