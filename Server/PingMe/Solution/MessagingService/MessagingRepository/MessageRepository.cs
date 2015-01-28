//NOTE: Abort
using MessagingEntities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MessagingRepository
{
    public class MessageRepository : BaseRepository, IRepository<MessagePing>
    {
        public int Add(MessagePing record)
        {
            base.EnsureContextAvailability();
            throw new NotImplementedException();
        }

        public int Update(MessagePing record)
        {
            base.EnsureContextAvailability();
            throw new NotImplementedException();
        }

        public MessagePing GetById(Guid id)
        {
            base.EnsureContextAvailability();
            throw new NotImplementedException();
        }

        public MessagePing Find(params object[] conditions)
        {
            base.EnsureContextAvailability();
            throw new NotImplementedException();
        }

        public IEnumerable<MessagePing> Where(Expression<Func<MessagePing, bool>> condition)
        {
            base.EnsureContextAvailability();
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            base.EnsureContextAvailability();
            throw new NotImplementedException();
        }
    }
}
