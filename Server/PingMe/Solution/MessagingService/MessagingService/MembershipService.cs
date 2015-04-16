using System;
using System.Web.Security;

namespace MessagingService
{
    public class MembershipService : IMembershipService
    {
        public MembershipUser GetUser(string userName)
        {
            bool sufficientDetailsProvided = !String.IsNullOrEmpty(userName);
            if (sufficientDetailsProvided)
            {
                MembershipUser userDetails;

                userDetails = Membership.GetUser(userName);

                return userDetails;
            }
            else
            {
                return default(MembershipUser);
            }
        }

        public MembershipCreateStatus CreateUser(string userName, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved)
        {
            bool sufficientDetailsProvided = !String.IsNullOrEmpty(userName) &&
                                                !String.IsNullOrEmpty(password) &&
                                                !String.IsNullOrEmpty(email);
            if (sufficientDetailsProvided)
            {
                bool shouldIDefaultSomeValues = String.IsNullOrEmpty(passwordQuestion) || String.IsNullOrEmpty(passwordAnswer);
                passwordQuestion = shouldIDefaultSomeValues ? "1" : passwordQuestion;
                passwordAnswer = shouldIDefaultSomeValues ? "2" : passwordAnswer;
                isApproved = true;

                MembershipCreateStatus result;

                Membership.CreateUser(userName, password, email, passwordQuestion, passwordAnswer, isApproved, out result);

                return result;
            }
            else
            {
                return MembershipCreateStatus.ProviderError;
            }
        }
    }
}