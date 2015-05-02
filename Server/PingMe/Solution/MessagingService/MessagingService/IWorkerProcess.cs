using MessagingEntities;
using System.Collections.Generic;

namespace MessagingService
{
    public interface IWorkerProcess
    {
        bool PostMessage(MessagePing messageDetails);

        List<PingList> ListConversationRoot(string source);

        List<MessagePing> FetchMessages(string source, string destination);
    }
}
