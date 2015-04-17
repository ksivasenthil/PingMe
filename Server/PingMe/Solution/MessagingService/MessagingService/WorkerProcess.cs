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

        public List<PingerProfile> ListConversationRoot(string source)
        {
            List<PingerProfile> conversationRoot = null;
            bool correctInput = !String.IsNullOrEmpty(source);
            if (correctInput)
            {
                Expression<Func<MessagePing, bool>> criteria = d => d.Source == source;
                IEnumerable<IGrouping<string, MessagePing>> intermediateResult = Storage.Where(criteria).GroupBy<MessagePing, string>(a => { return a.Destination; });
                bool conversationsAreAvailable = null != intermediateResult && 0 < intermediateResult.Count();
                if (conversationsAreAvailable)
                {
                    conversationRoot = new List<PingerProfile>();

                    MessagePing latestPing;
                    MembershipUser destinedUser;
                    string userImage;

                    foreach (IGrouping<string, MessagePing> destination in intermediateResult)
                    {
                        latestPing = destination.First<MessagePing>();
                        destinedUser = MembershipService.GetUser(latestPing.Destination);
                        userImage = ProfileService.GetPropertyValue(destinedUser.UserName, "ProfilePicture") as string;

                        conversationRoot.Add(new PingerProfile()
                        {
                            PingerSource = source,
                            PingerDestination = destination.Key,
                            LastMessage = destination.First<MessagePing>().Message,
                            PingerImage = userImage
                        });
                    }
                }
                else
                {
                    conversationRoot = new List<PingerProfile>();
                }
            }
            else
            {
                throw new ValidationException("Invalid Input");
            }
            return conversationRoot;
        }

        public List<MessagePing> FetchMessages(string source, string destination)
        {
            List<MessagePing> searchResult = null;
            bool correctInput = !String.IsNullOrEmpty(destination);
            if (correctInput)
            {
                //Conversation is the union of messages sent from you (source) to other person (destination) and 
                //messages sent from the other person (destination) to you (source)
                Expression<Func<MessagePing, bool>> criteria = d => (d.Destination == destination && d.Source == source) || (d.Source == destination && d.Destination == source);

                //Conversation should be sorted based on timestamp
                var intermediateResult = Storage.Where(criteria).OrderBy<MessagePing, DateTime?>(d => d.MessageSentUTC);
                searchResult = intermediateResult.ToList<MessagePing>();
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
    }
}