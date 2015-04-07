using MessagingEntities;
using System.Collections.Generic;

namespace MessagingService
{
    public interface IWorkerProcess
    {
        bool PostMessage(MessagePing messageDetails);

        List<PingerProfile> ListConversationRoot(string source);

        List<MessagePing> FetchMessages(string source, string destination);
    }
}
