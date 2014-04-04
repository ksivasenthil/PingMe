using MessagingEntities;
using MessagingRepository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MessagingService.Test
{
    [TestClass]
    public class SimpleComissioningTest
    {
        private static Mock<MessagingContext> repoInstance;
        private static TestContext testContextInstance;
        private static MessagePing dummyMessage;
        private static List<MessagePing> messageSearchResult;
        private static Guid recordId;
        private static string defaultDestination;
        private WorkerProcess testSubject;

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
            dummyMessage = new MessagePing()
            {
                Id = recordId,
                Source = "+919840200524",
                Destination = "+918903442090",
                Message = "Hello!!"
            };
            messageSearchResult = new List<MessagePing>() { dummyMessage };
            repoInstance = new Mock<MessagingContext>();
            repoInstance.Setup(ent => ent.MessageEntity.Find(It.IsAny<object[]>())).Returns(dummyMessage);
            repoInstance.Setup(ent => ent.MessageEntity.Where(It.IsAny<Expression<Func<MessagePing, bool>>>())).Returns(messageSearchResult.AsQueryable<MessagePing>());
            repoInstance.Setup(ent => ent.MessageEntity.Add(It.IsAny<MessagePing>())).Returns(dummyMessage);
            defaultDestination = "+919840200524";
        }

        [TestMethod]
        public void PostMessageSucceeds()
        {
            #region Setup for test
            testSubject = new WorkerProcess(repoInstance.Object);
            bool returnValue;
            #endregion

            #region Test
            returnValue = testSubject.PostMessage(dummyMessage);
            repoInstance.Verify(ent => ent.MessageEntity.Add(dummyMessage));
            Assert.IsTrue(returnValue);
            #endregion
        }

        [TestMethod]
        public void FetchMessageSucceds()
        {
            #region Setup for test
            testSubject = new WorkerProcess(repoInstance.Object);
            List<MessagePing> returnValue;
            #endregion

            #region Test
            returnValue = testSubject.FetchMessages(defaultDestination);
            repoInstance.Verify(ent => ent.MessageEntity.Add(dummyMessage));
            Assert.IsNotNull(returnValue);
            Assert.IsTrue(0 < returnValue.Count);
            MessagePing retrievedMessage = returnValue[0];
            Assert.IsTrue(dummyMessage.Id == retrievedMessage.Id);
            #endregion
        }
    }
}
