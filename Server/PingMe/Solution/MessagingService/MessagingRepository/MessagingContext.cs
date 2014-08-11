using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using MessagingEntities;
using System.Linq.Expressions;

namespace MessagingRepository
{
    public partial class MessagingContext : DbContext
    {
        public MessagingContext(string connectionName)
            : base("name=" + connectionName)
        {

        }

        #region Entities Exposed in this context
        public virtual DbSet<MessagePing> MessageEntity { get; set; }
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
