using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Profile;

namespace MessagingService
{
    public class ProfileService : IProfileService
    {
        public object GetPropertyValue(string userName, string propertyName)
        {
            bool sufficientDetailsAreAvailable = !String.IsNullOrEmpty(userName) && !String.IsNullOrEmpty(propertyName);
            object returnValue;

            if (sufficientDetailsAreAvailable)
            {

                ProfileBase profileHandle = ProfileBase.Create(userName);
                returnValue = profileHandle.GetPropertyValue(propertyName);
            }
            else
            {
                returnValue = default(object);
            }
            return returnValue;
        }

        public void SetPropertyValue(string userName, string propertyName, object propertyValue)
        {
            throw new NotImplementedException();
        }

        public void Save(string userName)
        {
            throw new NotImplementedException();
        }
    }
}