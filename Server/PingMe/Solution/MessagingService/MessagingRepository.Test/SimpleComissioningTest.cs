using MessagingEntities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Data;
using System.Data.Entity;

namespace MessagingRepository.Test
{
    [TestClass]
    public class SimpleComissioningTest
    {
        private static string connectionStringName, updatedMessage;
        private static Guid recordId;
        private TestContext testContextInstance;

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
        public static void GlobalTestSetup(TestContext contextInstance)
        {
            connectionStringName = "Kalanjiyam";
            recordId = Guid.NewGuid();
            updatedMessage = "Copy that !!";
        }

        [TestMethod]
        [TestCategory("DatabaseTest")]
        public void ConnectToDatabase()
        {
            using (var context = new MessagingContext(connectionStringName))
            {
                Assert.IsNotNull(context, "Context could not be instantiated.");
                Assert.IsNotNull(context.Database, "Context is instantiated, but Database reference is not set.");
                Assert.IsNotNull(context.Database.Connection, "Active connection refernece could not be obtained");
                try
                {
                    context.Database.Connection.Open();
                    Assert.IsTrue(context.Database.Connection.State == ConnectionState.Open, "Connection is not in open state, Current state - " + context.Database.Connection.State.ToString());
                }
                catch { }
                finally
                {
                    context.Database.Connection.Close();
                }
                Assert.IsTrue(context.Database.Connection.State == ConnectionState.Closed, "Connection is successfully closed, Current state - " + context.Database.Connection.State.ToString());
            }
        }

        [TestMethod]
        [TestCategory("DatabaseTest")]
        public void AddRecordsToDatabase()
        {
            #region Test Setup
            MessagePing testSubject = new MessagePing();
            int insertResult;
            #endregion

            #region Test
            testSubject.Id = recordId;
            testSubject.Source = "+919840200524";
            testSubject.Destination = "+918903442090";
            testSubject.Message = "Hello!!";

            using (var context = new MessagingContext(connectionStringName))
            {
                context.MessageEntity.Add(testSubject);
                insertResult = context.SaveChanges();
            }

            Assert.IsTrue(insertResult > 0);
            #endregion
        }

        [TestMethod]
        [TestCategory("DatabaseTest")]
        public void UpdateRecordToDatabase()
        {
            #region Test Setup
            MessagePing testSubject;
            #endregion

            #region Test

            using (var context = new MessagingContext(connectionStringName))
            {
                try
                {
                    testSubject = AssertRecordExistence(recordId, context);
                }
                catch (AssertFailedException)
                {
                    AddRecordsToDatabase();
                    testSubject = AssertRecordExistence(recordId, context);
                }

                testSubject.Message = updatedMessage;
                context.SaveChanges();
            }

            testSubject = null;

            using (var context = new MessagingContext(connectionStringName))
            {
                testSubject = context.MessageEntity.Find(new object[] { recordId });
                Assert.IsNotNull(testSubject, "Record not found in database");
                Assert.IsTrue(updatedMessage == testSubject.Message, "Record was not updated");
            }

            #endregion
        }
        
        [TestMethod]
        [TestCategory("DatabaseTest")]
        public void GetMessageRecordFromDatabase()
        {
            #region Test Setup
            MessagePing testSubject;
            #endregion

            #region Test
            using (var context = new MessagingContext(connectionStringName))
            {
                testSubject = context.MessageEntity.Find(new object[] { recordId });
                Assert.IsNotNull(testSubject, "Record not found in database");
                Assert.IsTrue(recordId == testSubject.Id, "Record found in the database did not match record id");
            }
            #endregion
        }

        [ClassCleanup]
        public static void CleanupTestTrack()
        {
            int deleteResult = -1;
            MessagingContext context = null;
            #region Clean database of records added during test
            try
            {
                using (context = new MessagingContext(connectionStringName))
                {
                    MessagePing recordToDelete = context.MessageEntity.Find(new object[] { recordId });
                    bool recordIsNotAvailable = null != recordToDelete;
                    if (recordIsNotAvailable)
                    {
                        context.MessageEntity.Remove(recordToDelete);
                        deleteResult = context.SaveChanges();
                    }
                }
            }
            catch { }
            finally
            {
                context.Dispose();

            }
            #endregion
        }

        private MessagePing AssertRecordExistence(Guid recordId, MessagingContext context)
        {
            MessagePing testSubject;

            testSubject = context.MessageEntity.Find(new object[] { recordId });

            Assert.IsNotNull(testSubject, "Record not found in database");

            return testSubject;
        }
    }
}
