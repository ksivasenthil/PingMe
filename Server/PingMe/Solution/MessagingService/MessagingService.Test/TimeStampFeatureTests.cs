using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MessagingRepository;
using System.Data.Entity;
using MessagingEntities;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Web.Security;

namespace MessagingService.Test
{
    [TestClass]
    public class TimeStampFeatureTests
    {
        private static TestContext testContextInstance;
        private static Mock<MessagingContext> repoInstance;
        private static Mock<DbSet<MessagePing>> entInstance;
        private static Mock<IMembershipService> membershipInstance;
        private static Mock<IProfileService> profileInstance;
        private static Mock<MembershipUser> userInstance;
        private WorkerProcess testSubject;
        private static MessagePing dummyMessage;
        private const int Beyond1WeekPast = -8;
        private const int Exactly1WeekPast = -7;
        private const int Beyond1WeekFuture = +8;
        private const int Exacly1WeekFuture = +7;
        private const int Within1HourPast = -1;
        private const string DefaultSource = "+919840200524";
        private const string DefaultDestination = "+9940425465";
        private static MessagePing passedToRepo;
        private static List<MessagePing> testDataCollection;
        private static Guid recordId;

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
            recordId = Guid.NewGuid();
            testContextInstance = contextInstance;
            dummyMessage = new MessagePing();

            #region Prepare test data
            testDataCollection = new List<MessagePing>();
            testDataCollection.Add(new MessagePing()
            {
                Source = "+919840200524",
                Destination = "+919941841903",
                Message = "Hello, Good Morning Appa!",
                MessageSentUTC = DateTime.UtcNow
            });

            testDataCollection.Add(new MessagePing()
            {
                Source = "+919941841903",
                Destination = "+919840200524",
                Message = "Good Morning Beta!",
                MessageSentUTC = DateTime.UtcNow.AddMinutes(2)
            });

            testDataCollection.Add(new MessagePing()
            {
                Source = "+919840200524",
                Destination = "+919941841903",
                Message = "How are you appa",
                MessageSentUTC = DateTime.UtcNow.AddMinutes(1)
            });

            testDataCollection.Add(new MessagePing()
            {
                Source = "+919941841903",
                Destination = "+919840200524",
                Message = "I am very good. Hope you are also good and a rockstar in your own way..",
                MessageSentUTC = DateTime.UtcNow.AddMinutes(3)
            });

            testDataCollection.Add(new MessagePing()
            {
                Source = "+919941841903",
                Destination = "+919551049084",
                Message = "Good morning dear wife",
                MessageSentUTC = DateTime.UtcNow.AddMinutes(3)
            });

            testDataCollection.Add(new MessagePing()
            {
                Source = "+919940425465",
                Destination = "+919840200524",
                Message = "Gud morning <3",
                MessageSentUTC = DateTime.UtcNow.AddMinutes(3)
            });

            #endregion

            entInstance = new Mock<DbSet<MessagePing>>();
            repoInstance = new Mock<MessagingContext>(new string[] { "Kalanjiyam" });
            membershipInstance = new Mock<IMembershipService>();
            profileInstance = new Mock<IProfileService>();
            userInstance = new Mock<MembershipUser>();
            repoInstance.Setup(ent => ent.FindOne(It.IsAny<object[]>())).Returns(dummyMessage);
            repoInstance.Setup(ent => ent.Where(It.IsAny<Expression<Func<MessagePing, bool>>>())).Returns<Expression<Func<MessagePing, bool>>>(
                (expr) =>
                {
                    return testDataCollection.Where(expr.Compile()).ToList<MessagePing>();
                });
            repoInstance.Setup(ent => ent.Add(It.IsAny<MessagePing>())).Callback<MessagePing>((data) => passedToRepo = data).Returns(dummyMessage);

            userInstance.Setup(usr => usr.ProviderUserKey).Returns(recordId);
            membershipInstance.Setup(mem => mem.CreateUser(It.IsAny<string>(), It.IsAny<string>(),
                                                            It.IsAny<string>(), It.IsAny<string>(),
                                                            It.IsAny<string>(), It.IsAny<bool>())).
                                Returns(MembershipCreateStatus.Success);
            membershipInstance.Setup(mem => mem.GetUser(It.IsAny<string>())).Returns(userInstance.Object);
            profileInstance.Setup(pro => pro.GetPropertyValue(It.IsAny<string>(), It.IsAny<string>())).Returns("http://www.google.com");
        }

        #region Sent Time tests
        [TestMethod]
        public void DoesMessageHaveMessageSentTime()
        {
            //TODO: Properly instantiate the WorkerProcess
            testSubject = new WorkerProcess(repoInstance.Object, null, null);
            MessagePing data = new MessagePing()
            {
                Id = Guid.NewGuid(),
                Source = DefaultSource,
                Destination = DefaultDestination,
                Message = "Hi!!",
                MessageSentUTC = DateTime.MinValue,
                MessageRecievedUTC = null
            };
            bool returnValue = testSubject.PostMessage(data);
            Assert.IsFalse(returnValue);
        }

        [TestMethod]
        public void SentTimeIsAbove1WeekInPast()
        {
            testSubject = new WorkerProcess(repoInstance.Object, membershipInstance.Object, profileInstance.Object);
            MessagePing data = new MessagePing()
            {
                Id = Guid.NewGuid(),
                Source = DefaultSource,
                Destination = DefaultDestination,
                Message = "Hi!!",
                MessageSentUTC = DateTime.UtcNow.AddDays(Beyond1WeekPast),
                MessageRecievedUTC = null
            };
            bool returnValue = testSubject.PostMessage(data);
            Assert.IsFalse(returnValue);
        }

        [TestMethod]
        public void SentTimeIsExactly1WeekInPast()
        {
            testSubject = new WorkerProcess(repoInstance.Object, membershipInstance.Object, profileInstance.Object);
            MessagePing data = new MessagePing()
            {
                Id = Guid.NewGuid(),
                Source = DefaultSource,
                Destination = DefaultDestination,
                Message = "Hi!!",
                MessageSentUTC = DateTime.UtcNow.AddDays(Exactly1WeekPast),
                MessageRecievedUTC = null
            };
            bool returnValue = testSubject.PostMessage(data);
            Assert.IsTrue(returnValue);
        }

        [TestMethod]
        public void SentTimeIsAbove1WeekInFuture()
        {
            testSubject = new WorkerProcess(repoInstance.Object, membershipInstance.Object, profileInstance.Object);
            MessagePing data = new MessagePing()
            {
                Id = Guid.NewGuid(),
                Source = DefaultSource,
                Destination = DefaultDestination,
                Message = "Hi!!",
                MessageSentUTC = DateTime.UtcNow.AddDays(Beyond1WeekFuture),
                MessageRecievedUTC = null
            };
            bool returnValue = testSubject.PostMessage(data);
            Assert.IsFalse(returnValue);
        }

        [TestMethod]
        public void SentTimeIsExactly1WeekInFuture()
        {
            testSubject = new WorkerProcess(repoInstance.Object, membershipInstance.Object, profileInstance.Object);
            MessagePing data = new MessagePing()
            {
                Id = Guid.NewGuid(),
                Source = DefaultSource,
                Destination = DefaultDestination,
                Message = "Hi!!",
                MessageSentUTC = DateTime.UtcNow.AddDays(Exacly1WeekFuture),
                MessageRecievedUTC = null
            };
            bool returnValue = testSubject.PostMessage(data);
            //Future Messages are not allowed
            Assert.IsFalse(returnValue);
        }

        [TestMethod]
        public void SentTimeIsLessThanMaxTime()
        {
            testSubject = new WorkerProcess(repoInstance.Object, membershipInstance.Object, profileInstance.Object);
            MessagePing data = new MessagePing()
            {
                Id = Guid.NewGuid(),
                Source = DefaultSource,
                Destination = DefaultDestination,
                Message = "Hi!!",
                MessageSentUTC = DateTime.MaxValue,
                MessageRecievedUTC = null
            };
            bool returnValue = testSubject.PostMessage(data);
            Assert.IsFalse(returnValue);
        }

        [TestMethod]
        public void DoesServerPreserveSentTimeForMessage()
        {
            testSubject = new WorkerProcess(repoInstance.Object, membershipInstance.Object, profileInstance.Object);
            DateTime timestamp = DateTime.UtcNow;
            MessagePing data = new MessagePing()
            {
                Id = Guid.NewGuid(),
                Source = DefaultSource,
                Destination = DefaultDestination,
                Message = "Hi!!",
                MessageSentUTC = timestamp,
                MessageRecievedUTC = null
            };
            bool returnValue = testSubject.PostMessage(data);
            Assert.AreEqual(data.MessageSentUTC, timestamp);
        }

        #endregion

        #region Recieved Time tests
        [TestMethod]
        public void DoesServerAddRecievedTimeForMessage()
        {
            testSubject = new WorkerProcess(repoInstance.Object, membershipInstance.Object, profileInstance.Object);
            MessagePing data = new MessagePing()
            {
                Id = Guid.NewGuid(),
                Source = DefaultSource,
                Destination = DefaultDestination,
                Message = "Hi!!",
                MessageSentUTC = DateTime.UtcNow,
                MessageRecievedUTC = null
            };
            bool returnValue = testSubject.PostMessage(data);
            Assert.IsNotNull(passedToRepo.MessageRecievedUTC);
        }

        [TestMethod]
        public void IsRecievedTimeBetweenMinAndMaxDateTimeValue()
        {
            testSubject = new WorkerProcess(repoInstance.Object, membershipInstance.Object, profileInstance.Object);
            MessagePing data = new MessagePing()
            {
                Id = Guid.NewGuid(),
                Source = DefaultSource,
                Destination = DefaultDestination,
                Message = "Hi!!",
                MessageSentUTC = DateTime.UtcNow,
                MessageRecievedUTC = null
            };
            bool returnValue = testSubject.PostMessage(data);
            Assert.IsTrue(DateTime.MinValue < passedToRepo.MessageRecievedUTC &&
                            DateTime.MaxValue > passedToRepo.MessageRecievedUTC);
        }

        [TestMethod]
        public void IsRecievedTimeWithinPast1Hour()
        {
            testSubject = new WorkerProcess(repoInstance.Object, membershipInstance.Object, profileInstance.Object);
            MessagePing data = new MessagePing()
            {
                Id = Guid.NewGuid(),
                Source = DefaultSource,
                Destination = DefaultDestination,
                Message = "Hi!!",
                MessageSentUTC = DateTime.UtcNow,
                MessageRecievedUTC = null
            };
            bool returnValue = testSubject.PostMessage(data);
            Assert.IsTrue(DateTime.UtcNow.AddHours(Within1HourPast) < passedToRepo.MessageRecievedUTC);
        }


        [TestMethod]
        public void BothSentAndRecievedTimeAreNotPresent()
        {
            testSubject = new WorkerProcess(repoInstance.Object, membershipInstance.Object, profileInstance.Object);
            MessagePing data = new MessagePing()
            {
                Id = Guid.NewGuid(),
                Source = DefaultSource,
                Destination = DefaultDestination,
                Message = "Hi!!",
                MessageSentUTC = null,
                MessageRecievedUTC = null
            };
            bool returnValue = testSubject.PostMessage(data);
            Assert.IsFalse(returnValue);
        }

        [TestMethod]
        public void IsTimeRecievedInUTCForMessage()
        {
            testSubject = new WorkerProcess(repoInstance.Object, membershipInstance.Object, profileInstance.Object);
            DateTime timestamp = DateTime.Now;
            MessagePing data = new MessagePing()
            {
                Id = Guid.NewGuid(),
                Source = DefaultSource,
                Destination = DefaultDestination,
                Message = "Hi!!",
                MessageSentUTC = timestamp,
                MessageRecievedUTC = null
            };
            bool returnValue = testSubject.PostMessage(data);
            Assert.IsTrue(DateTimeKind.Utc == data.MessageRecievedUTC.Value.Kind);
        }

        [TestMethod]
        public void RecievedTimeIsAlwaysGreaterThanSentTime()
        {
            testSubject = new WorkerProcess(repoInstance.Object, membershipInstance.Object, profileInstance.Object);
            DateTime timestamp = DateTime.UtcNow;
            MessagePing data = new MessagePing()
            {
                Id = Guid.NewGuid(),
                Source = DefaultSource,
                Destination = DefaultDestination,
                Message = "Hi!!",
                MessageSentUTC = timestamp.AddMilliseconds(1),
                MessageRecievedUTC = timestamp
            };
            bool returnValue = testSubject.PostMessage(data);
            Assert.IsFalse(returnValue);
        }
        #endregion

        #region Conversation Order Tests

        [TestMethod]
        public void CheckConversationOrder()
        {
            testSubject = new WorkerProcess(repoInstance.Object, membershipInstance.Object, profileInstance.Object);
            string source = "+919941841903";
            string destination = "+919840200524";
            List<MessagePing> conversation = testSubject.FetchMessages(source, destination);
            for (int i = 1; i < conversation.Count - 1; i++)
            {
                Assert.IsTrue(conversation[i].MessageSentUTC > conversation[i - 1].MessageSentUTC,
                            "The test failed for " + i + " iteration");
            }
        }
        #endregion

        [ClassCleanup]
        public static void CleanUpAfterTests()
        {
            dummyMessage = null;
            entInstance = null;
            repoInstance = null;
            membershipInstance = null;
            profileInstance = null;
            userInstance = null;
        }
    }
}
