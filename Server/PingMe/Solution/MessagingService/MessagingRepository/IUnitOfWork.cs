//NOTE: Abort
using System;

namespace MessagingRepository
{
    public interface IUnitOfWork : IDisposable
    {
        int Commit();
        void Rollback();
    }
}
