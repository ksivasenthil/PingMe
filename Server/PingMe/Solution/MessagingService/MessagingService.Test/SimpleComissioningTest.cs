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
using System.Web.Security;

namespace MessagingService.Test
{
    [TestClass]
    public class SimpleComissioningTest
    {
        private static Mock<MessagingContext> repoInstance;
        private static Mock<DbSet<MessagePing>> entInstance;
        private static Mock<IMembershipService> membershipInstance;
        private static Mock<IProfileService> profileInstance;
        private static Mock<MembershipUser> userInstance;
        private static TestContext testContextInstance;
        private static MessagePing dummyMessage, messagesDestination;
        private static IQueryable<MessagePing> messageSearchResult, conversationGroups;
        private static Guid recordId;
        private static string defaultDestination;
        private static string defaultSource;
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
            dummyMessage = new MessagePing()
            {
                Id = recordId,
                Source = "+919840200524",
                Destination = "+918903442090",
                Message = "Hello!!",
                MessageSentUTC = DateTime.UtcNow
            };
            messagesDestination = new MessagePing()
            {
                Id = recordId,
                Source = "+918903442090",
                Destination = "+919840200524",
                Message = "Hello!!",
                MessageSentUTC = DateTime.UtcNow
            };
            messageSearchResult = new List<MessagePing>() { dummyMessage, messagesDestination }.AsQueryable<MessagePing>();
            conversationGroups = new List<MessagePing>() { messagesDestination }.AsQueryable<MessagePing>();
            membershipInstance = new Mock<IMembershipService>();
            profileInstance = new Mock<IProfileService>();
            userInstance = new Mock<MembershipUser>();
            entInstance = new Mock<DbSet<MessagePing>>();
            repoInstance = new Mock<MessagingContext>(new string[] { "Kalanjiyam" });

            userInstance.Setup(usr => usr.ProviderUserKey).Returns(recordId);
            membershipInstance.Setup(mem => mem.CreateUser(It.IsAny<string>(), It.IsAny<string>(),
                                                            It.IsAny<string>(), It.IsAny<string>(),
                                                            It.IsAny<string>(), It.IsAny<bool>())).
                                Returns(MembershipCreateStatus.Success);
            membershipInstance.Setup(mem => mem.GetUser(It.IsAny<string>())).Returns(userInstance.Object);
            profileInstance.Setup(pro => pro.GetPropertyValue(It.IsAny<string>(), It.IsAny<string>())).Returns("http://www.google.com");

            
            repoInstance.Setup(ent => ent.FindOne(It.IsAny<object[]>())).Returns(dummyMessage);
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
            //Making the mock change to the change in expression which now includes the Source and Destination check for a conversation to be listed
            /*
             * The new and changed expression looks like this - 
             * Left Expression = {((d.Destination == value(MessagingService.WorkerProcess+<>c__DisplayClass4).destination) AndAlso (d.Source == value(MessagingService.WorkerProcess+<>c__DisplayClass4).source))}
             * Right Expression = {((d.Source == value(MessagingService.WorkerProcess+<>c__DisplayClass4).destination) AndAlso (d.Destination == value(MessagingService.WorkerProcess+<>c__DisplayClass4).source))}
             */
            /*repoInstance.Setup(ent => ent.Where(It.Is<Expression<Func<MessagePing, bool>>>
                                                (expr => Expression.Lambda<Func<string>>
                                                    ((MemberExpression)(((expr.Body as BinaryExpression).Right as BinaryExpression).Right as BinaryExpression).Right).Compile()() 
                                                    == "+919840200524") &&
                                                (expr => Expression.Lambda<Func<string>>
                                                    ((MemberExpression)(((expr.Body as BinaryExpression).Right as BinaryExpression).Right as BinaryExpression).Right).Compile()()
                                                    == "+919840200524")))
                                                    .Returns(conversationGroups);*/
            repoInstance.Setup(
                                ent => ent.Where(
                                    It.IsAny<Expression<Func<MessagePing, bool>>>()
                                )
                            ).Returns(
                                conversationGroups
                            ).Callback<Expression<Func<MessagePing, bool>>>(
                                (expr) => { passedExpression = expr; }
                            );


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
            defaultSource = "+919840200524";
        }


        [TestMethod]
        public void PostMessageSucceeds()
        {
            #region Setup for test
            testSubject = new WorkerProcess(repoInstance.Object, membershipInstance.Object, profileInstance.Object);
            bool returnValue;
            repoInstance.Setup(ent => ent.Add(It.IsAny<MessagePing>())).Returns(dummyMessage);
            #endregion

            #region Test
            returnValue = testSubject.PostMessage(dummyMessage);
            repoInstance.Verify(ent => ent.Add(dummyMessage),Times.AtLeastOnce,"Was called lesser than once");
            Assert.IsTrue(returnValue);
            #endregion
        }

        [TestMethod]
        public void FetchMessageSucceds()
        {
            #region Setup for test
            testSubject = new WorkerProcess(repoInstance.Object, membershipInstance.Object, profileInstance.Object);
            List<MessagePing> returnValue;
            Expression<Func<MessagePing, bool>> expr = d => d.Destination == defaultDestination;
            repoInstance.Setup(ent => ent.Add(It.IsAny<MessagePing>())).Returns(dummyMessage);
            #endregion

            #region Test
            returnValue = testSubject.FetchMessages(defaultSource, defaultDestination);
            //repoInstance.Verify(e => e.Where(
            //                                    It.Is<Expression<Func<MessagePing, bool>>>(
            //                                        param => Expression.Lambda<Func<string>>(
            //                                            (MemberExpression)(param.Body as BinaryExpression).Right
            //                                        ).Compile()() == "+919840200524")
            //                                    )
            //                                );
            //repoInstance.Verify(e => e.Where(
            //                                    It.Is<Expression<Func<MessagePing, bool>>>(
            //                                        param => Expression.Lambda<Func<string>>(
            //                                            (MemberExpression)(param.Body as BinaryExpression).Left
            //                                        ).Body.ToString() == "d.Destination")
            //                                    )
            //                                );
            //Assert.IsNotNull(returnValue);
            Assert.IsTrue(0 < returnValue.Count);
            MessagePing retrievedMessage = returnValue[0];
            Assert.IsTrue(dummyMessage.Id == retrievedMessage.Id);
            #endregion
        }

        [TestMethod]
        public void SourceAndDestinationAreNotSame()
        {
            #region Setup for test
            testSubject = new WorkerProcess(repoInstance.Object, membershipInstance.Object, profileInstance.Object);
            repoInstance.Setup(ent => ent.Add(It.IsAny<MessagePing>())).Returns(dummyMessage);
            MessagePing testData = new MessagePing()
            {
                Source = "+919840200524",
                Destination = "+919840200524",
                Message = "Hello!",
                MessageSentUTC = DateTime.UtcNow
            };
            #endregion

            #region Test and Assert
            bool postResult = testSubject.PostMessage(testData);
            Assert.IsFalse(postResult);
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
