using MessagingEntities;
using MessagingRepository;
using System;
using System.Collections.Generic;
using System.Web.Security;

namespace MessagingService
{
    public class MessengerServiceFacade : IMessengerServiceFacade
    {
        internal IWorkerProcess postSorter;
        internal IMembershipService membershipHandle;
        internal IProfileService profileHandle;

        public MessengerServiceFacade()
        {
            //postSorter = postmaster;
            //TODO: Properly instantiate the WorkerProcess

            membershipHandle = new MembershipService();
            profileHandle = new ProfileService();

            postSorter = new WorkerProcess(new MessagingContext("Kalanjiyam"), membershipHandle, profileHandle);
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
                //TODO: Log exception properly
                throw;
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
                //TODO: Log exception to disk
                throw;
            }
            return messagesForMe;
        }

        public List<PingerProfile> Conversation(string source)
        {
            List<PingerProfile> myConversations = new List<PingerProfile>();
            try
            {
                myConversations = postSorter.ListConversationRoot(source);
            }
            catch (Exception ex)
            {
                //TODO: Log exception to disk
                throw;
            }
            return myConversations;
        }
        #endregion

    }
}
