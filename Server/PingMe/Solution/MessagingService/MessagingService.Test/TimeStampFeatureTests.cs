using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MessagingRepository;
using System.Data.Entity;
using MessagingEntities;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace MessagingService.Test
{
    [TestClass]
    public class TimeStampFeatureTests
    {
        private static TestContext testContextInstance;
        private static Mock<MessagingContext> repoInstance;
        private static Mock<DbSet<MessagePing>> entInstance;
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
            dummyMessage = new MessagePing();

            //TODO: Add instantiation logic here
            entInstance = new Mock<DbSet<MessagePing>>();
            repoInstance = new Mock<MessagingContext>(new string[] { "Kalanjiyam" });
            repoInstance.Setup(ent => ent.FindOne(It.IsAny<object[]>())).Returns(dummyMessage);
            repoInstance.Setup(ent => ent.Where(It.IsAny<Expression<Func<MessagePing, bool>>>())).Returns(new List<MessagePing>().ToArray());
            repoInstance.Setup(ent => ent.Add(It.IsAny<MessagePing>())).Callback<MessagePing>((data) => passedToRepo = data).Returns(dummyMessage);
        }

        #region Sent Time tests
        [TestMethod]
        public void DoesMessageHaveMessageSentTime()
        {
            testSubject = new WorkerProcess(repoInstance.Object);
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
            testSubject = new WorkerProcess(repoInstance.Object);
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
            testSubject = new WorkerProcess(repoInstance.Object);
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
            testSubject = new WorkerProcess(repoInstance.Object);
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
            testSubject = new WorkerProcess(repoInstance.Object);
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
            Assert.IsTrue(returnValue);
        }

        [TestMethod]
        public void SentTimeIsLessThanMaxTime()
        {
            testSubject = new WorkerProcess(repoInstance.Object);
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
            testSubject = new WorkerProcess(repoInstance.Object);
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
            Assert.AreEqual(passedToRepo.MessageSentUTC, timestamp);
        }

        #endregion

        #region Recieved Time tests
        [TestMethod]
        public void DoesServerAddRecievedTimeForMessage()
        {
            testSubject = new WorkerProcess(repoInstance.Object);
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
            testSubject = new WorkerProcess(repoInstance.Object);
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
            testSubject = new WorkerProcess(repoInstance.Object);
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
            testSubject = new WorkerProcess(repoInstance.Object);
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
            testSubject = new WorkerProcess(repoInstance.Object);
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

        #endregion

        #region Conversation Order Tests
        [TestMethod]
        public void CheckConversationOrder()
        {
            //TODO: Implement this.
        }
        #endregion

        [ClassCleanup]
        public static void CleanUpAfterTests()
        {
            dummyMessage = null;
            entInstance = null;
            repoInstance = null;
        }
    }
}
