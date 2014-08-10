using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using MessagingEntities;

namespace MessagingRepository
{
    public partial class MessagingContext : DbContext
    {
        public MessagingContext(string connectionName)
            : base("name=" + connectionName)
        {

        }

        #region Entities Exposed in this context
        public DbSet<MessagePing> MessageEntity { get; set; }
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
