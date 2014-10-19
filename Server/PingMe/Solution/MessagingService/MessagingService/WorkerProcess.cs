using MessagingEntities;
using MessagingRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
namespace MessagingService
{
    internal class WorkerProcess : IWorkerProcess
    {
        private MessagingContext Storage;

        internal WorkerProcess(DbContext storageInstance)
        {
            this.Storage = storageInstance as MessagingContext;
        }

        public bool PostMessage(MessagePing messageDetails)
        {
            List<ValidationResult> validationSummary;
            bool continueProcessingPostValidation = ValidatedPostedMessage(messageDetails, out validationSummary);
            bool operationResult;
            MessagePing savedMessage = null;
            if (continueProcessingPostValidation)
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

        public List<string> ListConversationRoot(string source)
        {
            List<string> conversationRoot = null;
            bool correctInput = !String.IsNullOrEmpty(source);
            if (correctInput)
            {
                Expression<Func<MessagePing, bool>> criteria = d => d.Source == source;
                var intermediateResult = Storage.Where(criteria);
                bool conversationsAreAvailable = null != intermediateResult && 0 < intermediateResult.Count();
                if (conversationsAreAvailable)
                {
                    var roots = intermediateResult.Select(d => d.Destination);
                    var conversations = roots.Distinct<string>();
                    conversationRoot = conversations.ToList<string>();
                }
                else
                {
                    conversationRoot = new List<string>();
                }
            }
            else
            {
                throw new ValidationException("Invalid Input");
            }
            return conversationRoot;
        }

        public List<MessagePing> FetchMessages(string destination)
        {
            List<MessagePing> searchResult = null;
            bool correctInput = !String.IsNullOrEmpty(destination);
            if (correctInput)
            {
                Expression<Func<MessagePing, bool>> criteria = d => d.Destination == destination;

                var intermediateResult = Storage.Where(criteria);
                searchResult = intermediateResult.ToList<MessagePing>();
            }
            else
            {
                throw new ValidationException("Invalid Input");
            }
            return searchResult;
        }

        private bool ValidatedPostedMessage(MessagePing data, out List<ValidationResult> validationSummary)
        {
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();
            bool simpleValidationOutcome = Validator.TryValidateObject(data, new ValidationContext(data), validationResults);
            validationSummary = validationResults as List<ValidationResult>;
            return simpleValidationOutcome;
        }
    }
}