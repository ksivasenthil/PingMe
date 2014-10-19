using MessagingEntities;
using MessagingRepository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace MessagingService.Test
{
    [TestClass]
    public class SimpleComissioningTest
    {
        private static Mock<MessagingContext> repoInstance;
        private static Mock<DbSet<MessagePing>> entInstance;
        private static TestContext testContextInstance;
        private static MessagePing dummyMessage, messagesDestination;
        private static IQueryable<MessagePing> messageSearchResult, conversationGroups;
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
            messagesDestination = new MessagePing()
            {
                Id = recordId,
                Source = "+918903442090",
                Destination = "+919840200524",
                Message = "Hello!!"
            };
            messageSearchResult = new List<MessagePing>() { dummyMessage, messagesDestination }.AsQueryable<MessagePing>();
            conversationGroups = new List<MessagePing>() { messagesDestination }.AsQueryable<MessagePing>();
            entInstance = new Mock<DbSet<MessagePing>>();
            repoInstance = new Mock<MessagingContext>(new string[] { "Kalanjiyam" });
            repoInstance.Setup(ent => ent.FindOne(It.IsAny<object[]>())).Returns(dummyMessage);
            repoInstance.Setup(ent => ent.Add(It.IsAny<MessagePing>())).Returns(dummyMessage);
            /*
             * NOTE: - Very Important notice to mock expressions in Moq.
             * The code does not do proper set up for the scenario described here
             * repoInstance.Setup(ent => ent.Where(d => d.Destination.Equals("+919840200524"))).Returns(conversationGroups).Verifiable();
             * 
             * Since, the call to a method in another calls with the phone number as parameter modifies the above expression to look like this - 
             *  d => (d.Destination == value(MessagingService.WorkerProcess+<>c__DisplayClass0).destination)
             *  
             * Notice the string value(MessagingService.WorkerProcess+<>c_DisplayClass0).destination) since, the number is passed as parameter to the function in the class
             * The syntax derived here is by trial and error. The procedure to get to this needs to be determined by me. 
             * 
             * For now this solves the problem so I am rolling on. It took multiple sessions of 2 hrs across 3+ days to pinpoint this behavior.
             * 
             * It was pinpointed by not googling extensively but by reading the error message given by Moq very cautiously.
             * 
             * The code which follows 2 line down is the actual syntax that does the job.
             */
            //
            //repoInstance.Setup(ent => ent.Where(It.Is<Expression<Func<MessagePing, bool>>>(expr => expr.ToString() == "d => (d.Destination == value(MessagingService.WorkerProcess+<>c__DisplayClass0).destination)"))).Returns(conversationGroups);
            //Particular number match
            repoInstance.Setup(ent => ent.Where(It.Is<Expression<Func<MessagePing, bool>>>(expr => Expression.Lambda<Func<string>>((MemberExpression)(expr.Body as BinaryExpression).Right).Compile()() == "+919840200524"))).Returns(conversationGroups);


            /*repoInstance.Setup(ent => ent.Where(
                    It.IsAny<Expression<Func<MessagePing, bool>>>()
                )).
                Returns(
                    new Func<Expression<Func<MessagePing, bool>>, IQueryable<MessagePing>>(
                            expr => messageSearchResult.Where(expr.Compile()).AsQueryable()
                            )
                ).Verifiable();*/
            entInstance.As<IQueryable<MessagePing>>().Setup(rec => rec.Provider).Returns(messageSearchResult.Provider);
            entInstance.As<IQueryable<MessagePing>>().Setup(rec => rec.Expression).Returns(messageSearchResult.Expression);
            entInstance.As<IQueryable<MessagePing>>().Setup(rec => rec.ElementType).Returns(messageSearchResult.ElementType);
            entInstance.As<IQueryable<MessagePing>>().Setup(rec => rec.GetEnumerator()).Returns(messageSearchResult.GetEnumerator());
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
            repoInstance.Verify(ent => ent.Add(dummyMessage));
            Assert.IsTrue(returnValue);
            #endregion
        }

        [TestMethod]
        public void FetchMessageSucceds()
        {
            #region Setup for test
            testSubject = new WorkerProcess(repoInstance.Object);
            List<MessagePing> returnValue;
            Expression<Func<MessagePing, bool>> expr = d => d.Destination == defaultDestination;
            #endregion

            #region Test
            returnValue = testSubject.FetchMessages(defaultDestination);
            repoInstance.Verify(e => e.Where(
                                                It.Is<Expression<Func<MessagePing, bool>>>(
                                                    param => Expression.Lambda<Func<string>>(
                                                        (MemberExpression)(param.Body as BinaryExpression).Right
                                                    ).Compile()() == "+919840200524")
                                                )
                                            );
            repoInstance.Verify(e => e.Where(
                                                It.Is<Expression<Func<MessagePing, bool>>>(
                                                    param => Expression.Lambda<Func<string>>(
                                                        (MemberExpression)(param.Body as BinaryExpression).Left
                                                    ).Body.ToString() == "d.Destination")
                                                )
                                            );
            Assert.IsNotNull(returnValue);
            Assert.IsTrue(0 < returnValue.Count);
            MessagePing retrievedMessage = returnValue[0];
            Assert.IsTrue(dummyMessage.Id == retrievedMessage.Id);
            #endregion
        }

        [TestMethod]
        public void ListConversationRootSucceeds()
        {
            #region Test Setup
            List<string> callResult;
            string source = "+919840200524";
            testSubject = new WorkerProcess(repoInstance.Object);
            #endregion

            #region Test Operations
            callResult = testSubject.ListConversationRoot(source);
            #endregion

            #region Assert Operation Result
            repoInstance.Verify(e => e.Where(
                                                It.Is<Expression<Func<MessagePing, bool>>>(
                                                    param => Expression.Lambda<Func<string>>(
                                                        (MemberExpression)(param.Body as BinaryExpression).Right
                                                    ).Compile()() == "+919840200524")
                                                )
                                            );
            repoInstance.Verify(e => e.Where(
                                                It.Is<Expression<Func<MessagePing, bool>>>(
                                                    param => Expression.Lambda<Func<string>>(
                                                        (MemberExpression)(param.Body as BinaryExpression).Left
                                                    ).Body.ToString() == "d.Source")
                                                )
                                            );
            Assert.IsNotNull(callResult);
            Assert.IsTrue(0 < callResult.Count);
            string conversation = callResult[0];
            Assert.IsTrue(!String.IsNullOrEmpty(conversation));
            #endregion
        }

        [TestMethod]
        public void ListConversationRootReportsFailure()
        {
            #region Test Setup
            List<string> callResult;
            string source = "+919840200527";
            testSubject = new WorkerProcess(repoInstance.Object);
            #endregion

            #region Test Operations
            callResult = testSubject.ListConversationRoot(source);
            #endregion

            #region Assert Operation Result
            repoInstance.Verify(e => e.Where(
                                                It.Is<Expression<Func<MessagePing, bool>>>(
                                                    param => Expression.Lambda<Func<string>>(
                                                        (MemberExpression)(param.Body as BinaryExpression).Right
                                                    ).Compile()() == "+919840200527")
                                                )
                                            );
            repoInstance.Verify(e => e.Where(
                                                It.Is<Expression<Func<MessagePing, bool>>>(
                                                    param => Expression.Lambda<Func<string>>(
                                                        (MemberExpression)(param.Body as BinaryExpression).Left
                                                    ).Body.ToString() == "d.Source")
                                                )
                                            );
            Assert.IsNotNull(callResult);
            Assert.IsTrue(0 == callResult.Count);
            #endregion
        }

        [ClassCleanup]
        public static void ClearSetups()
        {
            entInstance = null;
            repoInstance = null;
        }
    }
}
