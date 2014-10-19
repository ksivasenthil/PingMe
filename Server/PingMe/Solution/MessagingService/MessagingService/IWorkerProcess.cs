using MessagingEntities;
using System.Collections.Generic;

namespace MessagingService
{
    public interface IWorkerProcess
    {
        bool PostMessage(MessagePing messageDetails);

        List<string> ListConversationRoot(string source);

        List<MessagePing> FetchMessages(string destination);
    }
}
