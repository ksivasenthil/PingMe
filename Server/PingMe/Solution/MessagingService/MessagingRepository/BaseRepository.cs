//NOTE: Abort
using System;
using System.Data.Entity;

namespace MessagingRepository
{
    public class BaseRepository
    {
        private DbContext storage;

        protected DbContext SetStorageInstance
        {
            private get { return storage; }
            set { storage = value; }
        }

        protected void EnsureContextAvailability()
        {
            bool contextIsNotAvailable = null == storage;
            if (contextIsNotAvailable)
            { throw new TypeLoadException("Context is not available for this type (" + this.GetType().FullName + ") to perform its operations"); }
        }
    }
}
