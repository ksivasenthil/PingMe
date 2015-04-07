using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Profile;

namespace MessagingService
{
    public interface IProfileService
    {
        object GetPropertyValue(string userName, string propertyName);
        void SetPropertyValue(string userName, string propertyName, object propertyValue);
        void Save(string userName);
    }
}
