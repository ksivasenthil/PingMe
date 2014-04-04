using MessagingEntities;
using System;
using System.Collections.Generic;

namespace MessagingService
{
    public class MessengerServiceFacade : IMessengerServiceFacade
    {
        #region Service Contract Method Implementation
        public bool PostMessage(MessagePing messageDetails)
        {
            throw new NotImplementedException();
        }

        public List<MessagePing> FetchMessages(string destination)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
