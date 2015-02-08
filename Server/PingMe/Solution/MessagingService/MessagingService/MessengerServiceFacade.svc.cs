using MessagingEntities;
using MessagingRepository;
using System;
using System.Collections.Generic;

namespace MessagingService
{
    public class MessengerServiceFacade : IMessengerServiceFacade
    {
        internal IWorkerProcess postSorter;

        public MessengerServiceFacade()
        {
            //postSorter = postmaster;
            postSorter = new WorkerProcess(new MessagingContext("Kalanjiyam"));
        }

        #region Service Contract Method Implementation
        public bool PostMessage(MessagePing messageDetails)
        {
            bool postResult = default(bool);
            try
            {
                postResult = postSorter.PostMessage(messageDetails);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return postResult;
        }

        public List<MessagePing> FetchMessages(string source, string destination)
        {
            List<MessagePing> messagesForMe = new List<MessagePing>();
            try
            {
                messagesForMe = postSorter.FetchMessages(source, destination);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return messagesForMe;
        }

        public List<string> Conversation(string source)
        {
            List<string> myConversations = new List<string>();
            try
            {
                myConversations = postSorter.ListConversationRoot(source);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return myConversations;
        }
        #endregion

    }
}
