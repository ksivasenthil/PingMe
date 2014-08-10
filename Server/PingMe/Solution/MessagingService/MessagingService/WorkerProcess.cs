using MessagingEntities;
using MessagingRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

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
            if (continueProcessingPostValidation)
            {
                try
                {
                    Storage.MessageEntity.Add(messageDetails);
                    Storage.SaveChanges();
                }
                catch (ValidationException ve)
                {

                    throw;
                }
            }
            throw new NotImplementedException();
        }

        public List<MessagePing> FetchMessages(string destination)
        {
            throw new NotImplementedException();
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