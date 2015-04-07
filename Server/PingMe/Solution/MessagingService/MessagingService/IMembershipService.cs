using System.Web.Security;

namespace MessagingService
{
    public interface IMembershipService
    {
        MembershipUser GetUser(string userName);
        MembershipCreateStatus CreateUser(string userName, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved);
    }
}
