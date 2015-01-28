//NOTE: Abort
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingRepository
{
    public class SaveMessageWork : BaseUoW
    {
        public SaveMessageWork(BaseRepository[] required)
            : base(required)
        { }

        private bool AreRepositoriesInitialized()
        {
            throw new NotImplementedException();
        }
    }
}
