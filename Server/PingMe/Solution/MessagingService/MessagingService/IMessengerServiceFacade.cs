using MessagingEntities;
using System.Collections.Generic;
using System.ServiceModel;

namespace MessagingService
{
    [ServiceContract(Namespace = "http://vosspace.com/FamilyConnect/MessengerService")]
    public interface IMessengerServiceFacade
    {
        [OperationContract]
        bool PostMessage(MessagePing messageDetails);

        [OperationContract]
        List<MessagePing> FetchMessages(string source, string destination);

        [OperationContract]
        List<PingerProfile> Conversation(string source);
    }
}
