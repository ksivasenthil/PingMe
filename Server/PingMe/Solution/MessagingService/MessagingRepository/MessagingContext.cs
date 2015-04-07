using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using MessagingEntities;
using System.Linq.Expressions;
using AutoMapper;

namespace MessagingRepository
{
#if DEBUG
    public class MessagingContext : DbContext
#elif RELEASE
    public sealed class MessagingContext : DbContext
#endif
    {
        private IMappingExpression<MessagePing, MessagePing> messageCopier;
        private DbSet<MessagePing> tableInstance;

        public MessagingContext(string connectionName)
            : base("name=" + connectionName)
        {
            messageCopier = Mapper.CreateMap<MessagePing, MessagePing>();
        }

        #region Entities Exposed in this context
        private DbSet<MessagePing> MessageEntity { get { return tableInstance ?? this.Set<MessagePing>(); } }
        #endregion

        #region Methods available to consumers
#if DEBUG
        public virtual IEnumerable<MessagePing> Where(Expression<Func<MessagePing, bool>> condition)
#elif RELEASE
        public IEnumerable<MessagePing> Where(Expression<Func<MessagePing, bool>> condition)

#endif
        {
            IEnumerable<MessagePing> searchResult = new List<MessagePing>();

            var queryResult = this.MessageEntity.Where(condition);

            bool valueObtained = null != searchResult;
            if (valueObtained)
            {
                searchResult = queryResult.AsEnumerable<MessagePing>();
            }
            return searchResult;
        }
#if DEBUG
        public virtual MessagePing FindOne(params object[] keys)
#elif RELEASE
        public MessagePing FindOne(params object[] keys)
#endif
        {
            MessagePing soughtRecord;
            try
            {
                soughtRecord = this.MessageEntity.Find(keys);
            }
            catch (Exception ex)
            {
                //TODO: Log the exception to disk
                throw;
            }
            bool recordNoFound = null == soughtRecord;
            if (recordNoFound)
            {
                soughtRecord = null;
            }
            return soughtRecord;
        }

#if DEBUG
        public virtual MessagePing Add(MessagePing data)
#elif RELEASE
        public MessagePing Add(MessagePing data)
#endif

        {
            MessagePing returnValue;
            int recordAdded;
            try
            {
                returnValue = this.MessageEntity.Add(data);
                recordAdded = this.SaveChanges();
            }
            catch (Exception ex)
            {
                //TODO: Log the exception 
                throw;
            }

            bool couldNotAddRecord = 0 >= recordAdded;

            if (couldNotAddRecord)
            {
                returnValue = null;
            }
            return returnValue;
        }

#if DEBUG
        public virtual MessagePing Update(MessagePing data)
#elif RELEASE
        public MessagePing Update(MessagePing data)
#endif

        {
            bool validDataIsPassed = null != data && !String.IsNullOrEmpty(data.Source) && !String.IsNullOrEmpty(data.Destination);
            if (validDataIsPassed)
            {

            }
            else
            {
                throw new Exception("Invalid data");
            }

            throw new NotImplementedException();
        }

#if DEBUG
        public virtual MessagePing Remove(MessagePing data)
#elif RELEASE
        public MessagePing Remove(MessagePing data)
#endif

        {
            bool validDataIsPassed = null != data && null != data.Id && Guid.Empty != data.Id;
            int operationResult;
            MessagePing returnValue;
            if (validDataIsPassed)
            {
                returnValue = this.MessageEntity.Remove(data);
                operationResult = this.SaveChanges();
                bool dataNoRemoved = 0 >= operationResult;
                if (dataNoRemoved)
                {
                    returnValue = null;
                }
            }
            else
            {
                throw new Exception("Invalid data");
            }

            return returnValue;
        }
        #endregion

        #region Event Handling
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<MessagePing>().ToTable("Messages");
            base.OnModelCreating(modelBuilder);
        }
        #endregion
    }
}
