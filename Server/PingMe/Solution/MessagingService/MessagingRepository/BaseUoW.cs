//NOTE: Abort
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingRepository
{
    public abstract class BaseUoW : IUnitOfWork
    {
        private DbContext context;
        private BaseRepository[] subjects;

        public BaseUoW(BaseRepository[] repositories)
        {
            subjects = repositories;

            this.ValidateInstance();
            //NOTE: ValidateInstance should be called
            //      In the derived class the base constructor
            //      should be called post creation of context
            //      The repository collection should have minimum
            //      one repository to work on. The method
            //      AreRepositoriesInitialized should be implemented
            //      to do that check.
            //
            //      The Repository should have a property setter
            //      for them to get the instance of DbContext on which
            //      they will operate. This cannot be a constructor
            //      parameter since, Repository is passed to UoW
            //      from consumer. Consumer is not aware of the 
            //      DbContext classes
        }

        private void ValidateInstance()
        {
            bool repositoriesNotInitialized = !AreRepositoriesInitialized();
            if(repositoriesNotInitialized)
            { throw new InvalidOperationException("An attempt to create Unit of Work is made without necessary repositories to work with."); }
        }

        private bool AreRepositoriesInitialized()
        {
            bool repositoriesAvailable = 0 != subjects.Length;
            if (repositoriesAvailable)
            { return true; }
            else
            { return false; }
        }

        #region Interface defined behavior
        public virtual int Commit()
        {
            throw new NotImplementedException();
        }

        public virtual void Rollback()
        {
            throw new NotImplementedException();
        }

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
