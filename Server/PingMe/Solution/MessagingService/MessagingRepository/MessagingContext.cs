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
    public partial class MessagingContext : DbContext
    {
        private IMappingExpression<MessagePing,MessagePing> messageCopier;

        public MessagingContext(string connectionName)
            : base("name=" + connectionName)
        {
            messageCopier = Mapper.CreateMap<MessagePing,MessagePing>();
        }

        #region Entities Exposed in this context
        public virtual DbSet<MessagePing> MessageEntity { get; set; }
        #endregion

        #region Methods available to consumers
        public virtual IEnumerable<MessagePing> Where(Expression<Func<MessagePing, bool>> condition)
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
        public virtual MessagePing FindOne(params object[] keys)
        {
            MessagePing soughtRecord;
            try
            {
                soughtRecord = this.MessageEntity.Find(keys);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            bool recordNoFound = null == soughtRecord;
            if (recordNoFound)
            {
                soughtRecord = null;
            }
            return soughtRecord;
        }
        public virtual MessagePing Add(MessagePing data)
        {
            MessagePing returnValue;
            int recordAdded;
            try
            {
                returnValue = this.MessageEntity.Add(data);
                recordAdded = this.SaveChanges();
            }
            catch (Exception ex)
            { throw ex; }

            bool couldNotAddRecord = 0 >= recordAdded;

            if (couldNotAddRecord)
            {
                returnValue = null;
            }
            return returnValue;
        }
        public virtual MessagePing Update(MessagePing data)
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
        public virtual MessagePing Remove(MessagePing data)
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
