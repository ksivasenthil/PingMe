//NOTE : Abort
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MessagingRepository
{
    public interface IRepository<T> : IDisposable where T : class
    {
        int Add(T record);
        int Update(T record);
        T GetById(Guid id);
        T Find(params object[] conditions);
        IEnumerable<T> Where(Expression<Func<T, bool>> condition);
    }
}
