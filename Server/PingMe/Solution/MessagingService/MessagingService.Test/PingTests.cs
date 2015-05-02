using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MessagingRepository;
using System.Data.Entity;
using MessagingEntities;
using System.Web.Security;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace MessagingService.Test
{
    [TestClass]
    public class PingTests
    {
        private static Mock<MessagingContext> repoInstance;
        private static Mock<DbSet<MessagePing>> entInstance;
        private static Mock<IMembershipService> membershipInstance;
        private static Mock<IProfileService> profileInstance;
        private static Mock<MembershipUser> userInstance;
        private static TestContext testContextInstance;
        private static MessagePing dummyPing, dummyPingReply;
        private static IQueryable<MessagePing> pingsSearchResult, pings;
        private static Guid recordId;
        private const string DEFAULT_DESTINATION = "+919941841903";
        private const string DEFAULT_SOURCE = "+919842000524";
        private WorkerProcess testSubject;
        private static Expression<Func<MessagePing, bool>> passedExpression;

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [ClassInitialize]
        public static void InitializeForTests(TestContext contextInstance)
        {
            testContextInstance = contextInstance;
            recordId = Guid.NewGuid();
            dummyPing = new MessagePing()
            {
                Id = recordId,
                Source = DEFAULT_SOURCE,
                Destination = DEFAULT_DESTINATION,
                Message = "Hello!!",
                MessageSentUTC = DateTime.UtcNow
            };
            dummyPingReply = new MessagePing()
            {
                Id = recordId,
                Source = DEFAULT_DESTINATION,
                Destination = DEFAULT_SOURCE,
                Message = "Vanakkam!!",
                MessageSentUTC = DateTime.UtcNow
            };
            pings = new List<MessagePing>() { dummyPing, dummyPingReply }.AsQueryable<MessagePing>();
            entInstance = new Mock<DbSet<MessagePing>>();
            repoInstance = new Mock<MessagingContext>(new string[] { "Kalanjiyam" });
            membershipInstance = new Mock<IMembershipService>();
            profileInstance = new Mock<IProfileService>();
            userInstance = new Mock<MembershipUser>();
            repoInstance.Setup(ent => ent.FindOne(It.IsAny<object[]>())).Returns(dummyPing);
            repoInstance.Setup(ent => ent.Add(It.IsAny<MessagePing>())).Returns(dummyPing);
            userInstance.Setup(usr => usr.ProviderUserKey).Returns(recordId);
            membershipInstance.Setup(mem => mem.CreateUser(It.IsAny<string>(), It.IsAny<string>(),
                                                            It.IsAny<string>(), It.IsAny<string>(),
                                                            It.IsAny<string>(), It.IsAny<bool>())).
                                Returns(MembershipCreateStatus.Success);
            membershipInstance.Setup(mem => mem.GetUser(It.IsAny<string>())).Returns(userInstance.Object);
            profileInstance.Setup(pro => pro.GetPropertyValue(It.IsAny<string>(), It.IsAny<string>())).Returns("http://www.google.com");

            repoInstance.Setup(
                                ent => ent.Where(
                                    It.IsAny<Expression<Func<MessagePing, bool>>>()
                                )
                            ).Returns(
                                pings
                            ).Callback<Expression<Func<MessagePing, bool>>>(
                                (expr) => { passedExpression = expr; }
                            );


            entInstance.As<IQueryable<MessagePing>>().Setup(rec => rec.Provider).Returns(pings.Provider);
            entInstance.As<IQueryable<MessagePing>>().Setup(rec => rec.Expression).Returns(pings.Expression);
            entInstance.As<IQueryable<MessagePing>>().Setup(rec => rec.ElementType).Returns(pings.ElementType);
            entInstance.As<IQueryable<MessagePing>>().Setup(rec => rec.GetEnumerator()).Returns(pings.GetEnumerator());
        }

        [TestMethod]
        public void ListMessagesSucceeds()
        {
            #region Test Setup
            List<MessagePing> callResult;
            testSubject = new WorkerProcess(repoInstance.Object, membershipInstance.Object, profileInstance.Object);
            #endregion

            #region Test Operations
            callResult = testSubject.FetchMessages(DEFAULT_SOURCE, DEFAULT_DESTINATION);
            #endregion

            #region Assert Operation Result
            CollectionAssert.AllItemsAreNotNull(callResult);
            #endregion
        }

        [TestMethod]
        public void MessagesAreFetchedWithProfileDetails()
        {
            #region Test Setup
            List<MessagePing> callResult;
            bool valueCheckForAllItems = true;
            testSubject = new WorkerProcess(repoInstance.Object, membershipInstance.Object, profileInstance.Object);
            #endregion

            #region Test Operations
            callResult = testSubject.FetchMessages(DEFAULT_SOURCE, DEFAULT_DESTINATION);
            foreach (MessagePing message in callResult)
            {
                valueCheckForAllItems &= null != message.DestinedUserProfile;
            }
            #endregion

            #region Assert Operation Result
            Assert.IsTrue(valueCheckForAllItems);
            #endregion

        }

        [TestMethod]
        public void MessagesAreFetchedWithProperProfileDetails()
        {
            #region Test Setup
            List<MessagePing> callResult;
            List<string> profilePictures;
            testSubject = new WorkerProcess(repoInstance.Object, membershipInstance.Object, profileInstance.Object);
            #endregion

            #region Test Operations
            callResult = testSubject.FetchMessages(DEFAULT_SOURCE, DEFAULT_DESTINATION);
            profilePictures = callResult.Select<MessagePing, string>(r => r.DestinedUserProfile.PingerImage).ToList<string>();
            #endregion

            #region Assert Operation Result
            CollectionAssert.AllItemsAreNotNull(profilePictures);
            #endregion

        }
        [ClassCleanup]
        public static void ClearSetups()
        {
            entInstance = null;
            repoInstance = null;
            membershipInstance = null;
            profileInstance = null;
            userInstance = null;
        }

    }
}
