using MessagingEntities;
using MessagingRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Profile;
using System.Web.Security;
namespace MessagingService
{
    internal class WorkerProcess : IWorkerProcess
    {
        private MessagingContext Storage;
        private IMembershipService MembershipService;
        private IProfileService ProfileService;
        private const string USER_PROFILE_PICTURE = "ProfilePicture";
        internal WorkerProcess(DbContext storageInstance, IMembershipService membershipInstance, IProfileService profileInstance)
        {
            this.Storage = storageInstance as MessagingContext;
            MembershipService = membershipInstance;
            ProfileService = profileInstance;
        }

        public bool PostMessage(MessagePing messageDetails)
        {
            List<ValidationResult> validationSummary;
            //Allowing the Id to be not available
            bool isNotIdPresent = null == messageDetails.Id || Guid.Empty == messageDetails.Id;
            if (isNotIdPresent)
            {
                messageDetails.Id = Guid.NewGuid();
            }
            bool continueProcessingPostValidation = ValidatePostedMessage(messageDetails, out validationSummary);
            DateTime UtcTimeReference = DateTime.UtcNow;

            //Test it is not a loopback message
            bool isNotLoopBack = messageDetails.Source != messageDetails.Destination;

            //Refresh the recieve time always
            messageDetails.MessageRecievedUTC = UtcTimeReference;

            bool dateRangeValidation = continueProcessingPostValidation &&
                                        isNotLoopBack &&
                                        UtcTimeReference.AddDays(-7) <= messageDetails.MessageSentUTC &&
                                        messageDetails.MessageSentUTC <= UtcTimeReference &&
                                        messageDetails.MessageRecievedUTC > messageDetails.MessageSentUTC;
            bool operationResult;
            MessagePing savedMessage = null;
            if (dateRangeValidation)
            {
                try
                {
                    savedMessage = Storage.Add(messageDetails);
                    bool messageSavedSuccessfully = null != savedMessage && null != savedMessage.Id;
                    if (messageSavedSuccessfully)
                    {
                        operationResult = true;
                    }
                    else
                    {
                        operationResult = false;
                    }
                }
                catch (ValidationException)
                {
                    throw;
                }
            }
            else
            {
                operationResult = false;
            }
            return operationResult;
        }

        public List<PingList> ListConversationRoot(string source)
        {
            List<PingList> conversationRoot = null;
            bool correctInput = !String.IsNullOrEmpty(source);
            if (correctInput)
            {
                Expression<Func<MessagePing, bool>> outGoingCriteria = d => d.Source == source;
                Expression<Func<MessagePing, bool>> inComingCriteria = d => d.Destination == source;
                IEnumerable<IGrouping<string, MessagePing>> outGoingPings = Storage.Where(outGoingCriteria).OrderBy<MessagePing, DateTime?>(a=>a.MessageSentUTC).GroupBy<MessagePing, string>(a => { return a.Destination; });
                IEnumerable<IGrouping<string, MessagePing>> inComingPings = Storage.Where(inComingCriteria).OrderBy<MessagePing, DateTime?>(a => a.MessageSentUTC).GroupBy<MessagePing, string>(a => { return a.Source; });

                conversationRoot = BuildList(outGoingPings, source, true);
                var inCompingPingList = BuildList(inComingPings, source, false);
                var differentialPingList = new List<PingList>();
                foreach (PingList candidatePing in inCompingPingList)
                {
                    bool pingersAreOnlyIncomingWithNoReplies = true;
                    foreach (PingList basedOutPing in conversationRoot)
                    {
                        //If the following condition evaluates to true, it is a conversation between the numbers
                        pingersAreOnlyIncomingWithNoReplies &= candidatePing.PingerSource != basedOutPing.PingerDestination;
                    }
                    if (pingersAreOnlyIncomingWithNoReplies)
                    {
                        differentialPingList.Add(candidatePing);
                    }
                }
                conversationRoot.AddRange(differentialPingList);
            }
            else
            {
                throw new ValidationException("Invalid Input");
            }
            return conversationRoot;
        }

        public List<MessagePing> FetchMessages(string source, string destination)
        {
            List<MessagePing> searchResult = new List<MessagePing>();
            bool correctInput = !String.IsNullOrEmpty(destination);
            if (correctInput)
            {
                //Conversation is the union of messages sent from you (source) to other person (destination) and 
                //messages sent from the other person (destination) to you (source)
                Expression<Func<MessagePing, bool>> outGoingCriteria = d => (d.Source == source && d.Destination == destination);
                Expression<Func<MessagePing, bool>> inComingCriteria = d => (d.Source == destination && d.Destination == source);

                //Conversation should be sorted based on timestamp
                var outGoingPings = Storage.Where(outGoingCriteria);
                var inComingPings = Storage.Where(inComingCriteria);

                searchResult = BuildPings(outGoingPings, source, true);
                searchResult.AddRange(BuildPings(inComingPings, source, false));

                bool validPingsNotAvailable = 0 >= searchResult.Count;

                if (validPingsNotAvailable)
                {
                    searchResult = null;
                }

                searchResult = (searchResult as IEnumerable<MessagePing>).OrderBy<MessagePing, DateTime?>(d => d.MessageSentUTC).ToList<MessagePing>();

            }
            else
            {
                throw new ValidationException("Invalid Input");
            }
            return searchResult;
        }

        private bool ValidatePostedMessage(MessagePing data, out List<ValidationResult> validationSummary)
        {
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();
            bool simpleValidationOutcome = Validator.TryValidateObject(data, new ValidationContext(data), validationResults);
            validationSummary = validationResults as List<ValidationResult>;
            return simpleValidationOutcome;
        }

        private List<PingList> BuildList(IEnumerable<IGrouping<string, MessagePing>> pings, string pinger, bool usePingerAsSource)
        {
            List<PingList> conversationRoot = null;
            bool usePingerAsDestination = !usePingerAsSource;
            bool conversationsAreAvailable = null != pings && 0 < pings.Count();
            if (conversationsAreAvailable)
            {
                conversationRoot = new List<PingList>();

                MessagePing latestPing;
                MembershipUser destinedUser;
                string userImage = string.Empty;

                foreach (IGrouping<string, MessagePing> destination in pings)
                {
                    latestPing = destination.Last<MessagePing>();
                    var applicableDestination = usePingerAsDestination ? latestPing.Source : latestPing.Destination;
                    destinedUser = MembershipService.GetUser(applicableDestination);
                    bool destinedUserHasProfile = null != destinedUser;
                    if (destinedUserHasProfile)
                    {
                        userImage = ProfileService.GetPropertyValue(destinedUser.UserName, USER_PROFILE_PICTURE) as string;
                    }
                    conversationRoot.Add(new PingList()
                    {
                        PingerSource = (usePingerAsSource) ? pinger : destination.Key,
                        PingerDestination = (usePingerAsDestination) ? pinger : destination.Key,
                        DestinationPingerProfile = new PingerProfile()
                        {
                            LastMessage = latestPing.Message,
                            PingerImage = userImage
                        }
                    });
                }
            }
            else
            {
                conversationRoot = new List<PingList>();
            }
            return conversationRoot;
        }

        private List<MessagePing> BuildPings(IEnumerable<MessagePing> pings, string pinger, bool usePingerAsSource)
        {
            List<MessagePing> searchResult = new List<MessagePing>();

            #region Fetch Destined User Profile
            MembershipUser destinedUser;
            PingerProfile destinedUserProfile;
            bool usePingerAsDestination = !usePingerAsSource;
            string profilePicture;
            foreach (var ping in pings)
            {
                var applicableDestination = usePingerAsDestination ? ping.Source : ping.Destination;
                var applicableSource = usePingerAsSource ? pinger : ping.Destination;
                destinedUser = MembershipService.GetUser(applicableDestination);
                bool userHasProfile = null != destinedUser;
                if (userHasProfile)
                {
                    profilePicture = ProfileService.GetPropertyValue(destinedUser.UserName, USER_PROFILE_PICTURE) as string;
                    var lastMessage = pings.Where(r => r.Source == applicableSource).LastOrDefault<MessagePing>();
                    destinedUserProfile = new PingerProfile()
                    {
                        PingerImage = profilePicture,
                        LastMessage = (null != lastMessage) ? lastMessage.Message : string.Empty
                    };
                    searchResult.Add(new MessagePing()
                    {
                        Source = ping.Source,
                        Destination = ping.Destination,
                        Id = ping.Id,
                        Asset = ping.Asset,
                        Message = ping.Message,
                        MessageRecievedUTC = ping.MessageRecievedUTC,
                        MessageSentUTC = ping.MessageSentUTC,
                        DestinedUserProfile = destinedUserProfile
                    });
                }
            }
            #endregion
            bool validPingsNotAvailable = 0 >= searchResult.Count;

            if (validPingsNotAvailable)
            {
                searchResult = new List<MessagePing>();
            }
            return searchResult;
        }
    }
}