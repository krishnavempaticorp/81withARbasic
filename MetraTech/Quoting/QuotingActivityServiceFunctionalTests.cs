﻿using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MetraTech.Core.Services.ClientProxies;
using MetraTech.DataAccess;
using MetraTech.Domain.Quoting;
using MetraTech.DomainModel.BaseTypes;
using MetraTech.TestCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MetraTech.Interop.MTProductCatalog;
using MetraTech.Shared.Test;

namespace MetraTech.Quoting.Test
{
    [TestClass]
    public class QuotingActivityServiceFunctionalTests
    {
        private static TestContext _testContext;
        #region Setup/Teardown

        [ClassInitialize]
        public static void InitTests(TestContext testContext)
        {
            _testContext = testContext;
            SharedTestCode.MakeSureServiceIsStarted("ActivityServices");
            SharedTestCode.MakeSureServiceIsStarted("Pipeline");
        }

        #endregion
        [TestMethod, MTFunctionalTest(TestAreas.Quoting)]
        public void QuotingActivityServiceCreateQuote_BasicScenario_QuoteCreated()
        {
            #region Prepare
            string testShortName = "Q_AS_Basic"; //Account name and perhaps others need a 'short' (less than 40 when combined with testRunUniqueIdentifier
            //string testDescription = @"";
            string testRunUniqueIdentifier = MetraTime.Now.ToString(); //Identifier to make this run unique

            // Create account
            CorporateAccountFactory corpAccountHolder = new CorporateAccountFactory(testShortName, testRunUniqueIdentifier);
            corpAccountHolder.Instantiate();

            Assert.IsNotNull(corpAccountHolder.Item._AccountID, "Unable to create account for test run");
            int idAccountToQuoteFor = (int)corpAccountHolder.Item._AccountID;

            // Create/Verify Product Offering Exists
            var pofConfiguration = new ProductOfferingFactoryConfiguration(_testContext.TestName, testRunUniqueIdentifier);

            pofConfiguration.CountNRCs = 1;
            pofConfiguration.CountPairRCs = 1; //????
            pofConfiguration.CountPairUDRCs = 1;


            IMTProductOffering productOffering = ProductOfferingFactory.Create(pofConfiguration);
            Assert.IsNotNull(productOffering.ID, "Unable to create PO for test run");
            int idProductOfferingToQuoteFor = productOffering.ID;

            //Values to use for verification
            int numOfAccounts = 1;
            int expectedQuoteNRCsCount = pofConfiguration.CountNRCs * numOfAccounts;
            int expectedQuoteFlatRCsCount = pofConfiguration.CountPairRCs + (pofConfiguration.CountPairRCs * numOfAccounts);
            int expectedQuoteUDRCsCount = pofConfiguration.CountPairUDRCs + (pofConfiguration.CountPairUDRCs * numOfAccounts);

            decimal totalAmountForUDRC = 30;

            decimal expectedQuoteTotal = (expectedQuoteFlatRCsCount * pofConfiguration.RCAmount) +
                                         (expectedQuoteUDRCsCount * totalAmountForUDRC) +
                                         (expectedQuoteNRCsCount * pofConfiguration.NRCAmount);
            //decimal expectedQuoteTotalTax = expectedQuoteTotal * 0.05m + expectedQuoteTotal * 0.025m * 4;	//values from dummy stage to calculate taxes (TA818)
            decimal expectedQuoteTotalTax = 0;

            string expectedQuoteCurrency = "USD";

            #endregion

            #region Test and Verify

            var request = new QuoteRequest();
            request.Accounts.Add(idAccountToQuoteFor);
            request.ProductOfferings.Add(idProductOfferingToQuoteFor);
            request.QuoteIdentifier = "MyQuoteId-" + testShortName + "-1234";
            request.QuoteDescription = "Quote generated by Automated Test: " + _testContext.TestName;
            request.ReportParameters = new ReportParams() { PDFReport = QuotingTestScenarios.RunPDFGenerationForAllTestsByDefault };
            request.Localization = "en-US";
            request.SubscriptionParameters.UDRCValues = SharedTestCode.GetUDRCInstanceValuesSetToMiddleValues(productOffering);
            
            QuoteResponse response = null;

            bool clientInvoked = false;
            try
            {
                response = SharedTestCodeQuoting.InvokeCreateQuote(request);
                clientInvoked = true;
            }
            catch (Exception ex)
            {
                Assert.Fail("QuotingService_CreateQuote_Client thrown an exception: " + ex.Message);
            }

            Assert.IsFalse(response.Status == QuoteStatus.Failed, response.FailedMessage);
            Assert.IsTrue(clientInvoked, "QuotingService_CreateQuote_Client didn't executed propely");
            Assert.AreEqual(expectedQuoteTotal, response.TotalAmount, "Wrong TotalAmount");
            Assert.AreEqual(expectedQuoteTotalTax, response.TotalTax, String.Format("Wrong TotalTax. Actual is {0}, but expected {1}", response.TotalTax, expectedQuoteTotalTax));
            Assert.AreEqual(expectedQuoteCurrency, response.Currency, "Wrong Currency");

            #endregion
        }

        [TestMethod, MTFunctionalTest(TestAreas.Quoting)]
        public void QuotingActivityServiceCreateQuote_WrongAccWrongPO_Exception()
        {

            //TODO: Add an activity service test for a failed case to make sure we get error back and can understand it
            #region Prepare
            //string testShortName = "Q_AS_Basic_Exception"; //Account name and perhaps others need a 'short' (less than 40 when combined with testRunUniqueIdentifier
            //string testDescription = @"";
            string testRunUniqueIdentifier = MetraTime.Now.ToString(); //Identifier to make this run unique

            // Create request with wrong Account and PO
            var request = new QuoteRequest();
            request.Accounts.Add(5555555);
            request.ProductOfferings.Add(6666666);
            request.ReportParameters = new ReportParams() { PDFReport = QuotingTestScenarios.RunPDFGenerationForAllTestsByDefault };
            var expectedErrorMessagePartialText = "has no billing cycle";

            #endregion

            #region Test and Verify

            QuoteResponse erroredResponse = null;

            try
            {
                erroredResponse = SharedTestCodeQuoting.InvokeCreateQuote(request);
            }
            catch (Exception ex)
            {
                Assert.Fail("QuotingService_CreateQuote_Client thrown an exception: " + ex.Message);
            }

            Assert.IsTrue(erroredResponse.Status == QuoteStatus.Failed, "Expected response quote status must be failed");
            Assert.IsTrue(!string.IsNullOrEmpty(erroredResponse.FailedMessage), "Failed quote does not have FailedMessage set");

            //Verify the message we expect is there
            Assert.IsTrue(erroredResponse.FailedMessage.Contains(expectedErrorMessagePartialText), "Expected failure message with text '{0}' but got failure message '{1}'", expectedErrorMessagePartialText, erroredResponse.FailedMessage);

            #endregion
        }

         [TestMethod, MTFunctionalTest(TestAreas.Quoting)]
        public void QuotingActivityServiceCreateQuote_TwoQuotesInParallel_QuotesCreated()
        {
            string testShortName = "Q_AS_D_PO"; //Account name and perhaps others need a 'short' (less than 40 when combined with testRunUniqueIdentifier
            string testRunUniqueIdentifier = MetraTime.NowWithMilliSec.ToString(); //Identifier to make this run unique

            // Create account #1
            CorporateAccountFactory corpAccountHolder = new CorporateAccountFactory(testShortName, testRunUniqueIdentifier);
            corpAccountHolder.Instantiate();

            Assert.IsNotNull(corpAccountHolder.Item._AccountID, "Unable to create account for test run");
            int idAccountToQuoteFor = (int)corpAccountHolder.Item._AccountID;

            // Create account #2
            testRunUniqueIdentifier = MetraTime.NowWithMilliSec;
            corpAccountHolder = new CorporateAccountFactory(testShortName, testRunUniqueIdentifier);
            corpAccountHolder.Instantiate();

            Assert.IsNotNull(corpAccountHolder.Item._AccountID, "Unable to create account for test run");
            int idAccountToQuoteFor2 = (int)corpAccountHolder.Item._AccountID;

            // Create/Verify Product Offering Exists
            var pofConfiguration = new ProductOfferingFactoryConfiguration(_testContext.TestName, testRunUniqueIdentifier);

            pofConfiguration.CountNRCs = 1;
            pofConfiguration.CountPairRCs = 1;

            IMTProductOffering productOffering = ProductOfferingFactory.Create(pofConfiguration);
            Assert.IsNotNull(productOffering.ID, "Unable to create PO for test run");
            int idProductOfferingToQuoteFor = productOffering.ID;

            testRunUniqueIdentifier = MetraTime.Now.ToString();
            pofConfiguration = new ProductOfferingFactoryConfiguration(_testContext.TestName, testRunUniqueIdentifier);

            pofConfiguration.CountNRCs = 1;
            pofConfiguration.CountPairRCs = 1;

            productOffering = ProductOfferingFactory.Create(pofConfiguration);
            Assert.IsNotNull(productOffering.ID, "Unable to create PO for test run");
            int idProductOfferingToQuoteFor2 = productOffering.ID;

            Parallel.Invoke(()
              => CreateAndVerifyQuote(idAccountToQuoteFor,
                                      idProductOfferingToQuoteFor,
                                      _testContext.TestName,
                                      testShortName), ()
                              => CreateAndVerifyQuote(idAccountToQuoteFor2,
                                                      idProductOfferingToQuoteFor2,
                                                       _testContext.TestName,
                                                      testShortName));
        }

        [TestMethod, MTFunctionalTest(TestAreas.Quoting)]
        public void QuotingActivityServiceCreateQuote_TwoQuotesInParallelWithSamePO_QuotesCreated()
        {
            string testShortName = "Q_AS_S_PO"; //Account name and perhaps others need a 'short' (less than 40 when combined with testRunUniqueIdentifier
            string testRunUniqueIdentifier = MetraTime.NowWithMilliSec.ToString(); //Identifier to make this run unique

            // Create account #1
            CorporateAccountFactory corpAccountHolder = new CorporateAccountFactory(testShortName, testRunUniqueIdentifier);
            corpAccountHolder.Instantiate();

            Assert.IsNotNull(corpAccountHolder.Item._AccountID, "Unable to create account for test run");
            int idAccountToQuoteFor = (int)corpAccountHolder.Item._AccountID;

            // Create account #2
            testRunUniqueIdentifier = MetraTime.NowWithMilliSec.ToString();
            corpAccountHolder = new CorporateAccountFactory(testShortName, testRunUniqueIdentifier);
            corpAccountHolder.Instantiate();

            Assert.IsNotNull(corpAccountHolder.Item._AccountID, "Unable to create account for test run");
            int idAccountToQuoteFor2 = (int)corpAccountHolder.Item._AccountID;

            // Create/Verify Product Offering Exists
            var pofConfiguration = new ProductOfferingFactoryConfiguration(_testContext.TestName, testRunUniqueIdentifier);

            pofConfiguration.CountNRCs = 1;
            pofConfiguration.CountPairRCs = 1;

            IMTProductOffering productOffering = ProductOfferingFactory.Create(pofConfiguration);
            Assert.IsNotNull(productOffering.ID, "Unable to create PO for test run");
            int idProductOfferingToQuoteFor = productOffering.ID;

            Parallel.Invoke(()
              => CreateAndVerifyQuote(idAccountToQuoteFor, idProductOfferingToQuoteFor,
                                     _testContext.TestName, testShortName), ()
                => CreateAndVerifyQuote(idAccountToQuoteFor2, idProductOfferingToQuoteFor,
                                      _testContext.TestName, testShortName));
        }

        [TestMethod, MTFunctionalTest(TestAreas.Quoting), Ignore]
        public void QuotingActivityServiceCreateQuote_TwoQuotesInParallelWithSamePOAndSameAcc_QuotesCreated()
        {
            string testShortName = "Q_AS_S_PO"; //Account name and perhaps others need a 'short' (less than 40 when combined with testRunUniqueIdentifier
            string testRunUniqueIdentifier = MetraTime.NowWithMilliSec.ToString(); //Identifier to make this run unique

            CorporateAccountFactory corpAccountHolder = new CorporateAccountFactory(testShortName, testRunUniqueIdentifier);
            corpAccountHolder.Instantiate();

            Assert.IsNotNull(corpAccountHolder.Item._AccountID, "Unable to create account for test run");
            int idAccountToQuoteFor = (int)corpAccountHolder.Item._AccountID;

            // Create/Verify Product Offering Exists
            var pofConfiguration = new ProductOfferingFactoryConfiguration(_testContext.TestName, testRunUniqueIdentifier);

            pofConfiguration.CountNRCs = 1;
            pofConfiguration.CountPairRCs = 1;

            IMTProductOffering productOffering = ProductOfferingFactory.Create(pofConfiguration);
            Assert.IsNotNull(productOffering.ID, "Unable to create PO for test run");
            int idProductOfferingToQuoteFor = productOffering.ID;

            Parallel.Invoke(()
              => CreateAndVerifyQuote(idAccountToQuoteFor,
                                      idProductOfferingToQuoteFor,
                                      _testContext.TestName,
                                      testShortName,
                                      "The account is already subscribed"), ()
                        => CreateAndVerifyQuote(idAccountToQuoteFor,
                                               idProductOfferingToQuoteFor,
                                               _testContext.TestName,
                                               testShortName));
        }


       [TestMethod, MTFunctionalTest(TestAreas.Quoting)]
        // TODO: Do we need the test, looks like it's the same as  
        public void QuotingActivityServiceCreateQuote_GenerateQuoteTwoTimesNonParallel_QuoteCreated()
        {
            #region Prepare
            string testShortName = "Q_AS_2Qs"; //Account name and perhaps others need a 'short' (less than 40 when combined with testRunUniqueIdentifier
            string testRunUniqueIdentifier = MetraTime.Now.ToString(); //Identifier to make this run unique

            // Create account
            CorporateAccountFactory corpAccountHolder = new CorporateAccountFactory(testShortName, testRunUniqueIdentifier);
            corpAccountHolder.Instantiate();

            Assert.IsNotNull(corpAccountHolder.Item._AccountID, "Unable to create account for test run");
            int idAccountToQuoteFor = (int)corpAccountHolder.Item._AccountID;

            // Create/Verify Product Offering Exists
            var pofConfiguration = new ProductOfferingFactoryConfiguration(_testContext.TestName, testRunUniqueIdentifier);

            pofConfiguration.CountNRCs = 1;
            pofConfiguration.CountPairRCs = 1;

            IMTProductOffering productOffering = ProductOfferingFactory.Create(pofConfiguration);
            Assert.IsNotNull(productOffering.ID, "Unable to create PO for test run");
            int idProductOfferingToQuoteFor = productOffering.ID;

            //Values to use for verification
            decimal expectedQuoteTotal = (pofConfiguration.CountPairRCs * pofConfiguration.RCAmount * 2) + (pofConfiguration.CountNRCs * pofConfiguration.NRCAmount);
            string expectedQuoteCurrency = "USD";

            #endregion

            #region Test and Verify

            var request = new QuoteRequest();
            request.Accounts.Add(idAccountToQuoteFor);
            request.ProductOfferings.Add(idProductOfferingToQuoteFor);
            request.QuoteIdentifier = "MyQuoteId-" + testShortName + "-1234";
            request.QuoteDescription = "Quote generated by Automated Test: " + _testContext.TestName;
            request.ReportParameters = new ReportParams() { PDFReport = QuotingTestScenarios.RunPDFGenerationForAllTestsByDefault };
            request.EffectiveDate = MetraTime.Now;
            request.EffectiveEndDate = MetraTime.Now;

            QuoteResponse response = null;

            bool clientInvoked = false;
            try
            {
                response = SharedTestCodeQuoting.InvokeCreateQuote(request);
                clientInvoked = true;
            }
            catch (Exception ex)
            {
                Assert.Fail("QuotingService_CreateQuote_Client thrown an exception: " + ex.Message);
            }

            Assert.IsFalse(response.Status == QuoteStatus.Failed, response.FailedMessage);
            Assert.IsTrue(clientInvoked, "QuotingService_CreateQuote_Client didn't executed propely");
            Assert.AreEqual(expectedQuoteTotal, response.TotalAmount, "Wrong TotalAmount");
            Assert.AreEqual(expectedQuoteCurrency, response.Currency, "Wrong Currency");

            QuoteResponse response2 = null;

            clientInvoked = false;
            try
            {
                response2 = SharedTestCodeQuoting.InvokeCreateQuote(request);
                clientInvoked = true;
            }
            catch (Exception ex)
            {
                Assert.Fail("QuotingService_CreateQuote_Client thrown an exception: " + ex.Message);
            }

            Assert.IsFalse(response.Status == QuoteStatus.Failed, response.FailedMessage);
            Assert.IsTrue(clientInvoked, "QuotingService_CreateQuote_Client didn't executed propely");
            Assert.AreEqual(expectedQuoteTotal, response.TotalAmount, "Wrong TotalAmount");
            Assert.AreEqual(expectedQuoteCurrency, response.Currency, "Wrong Currency");

            Assert.AreEqual(response.TotalAmount, response2.TotalAmount, "Total amount was different on the second run");

            #endregion
        }


        #region Helpers


        /// <summary>
        /// Creates and verifies the created quote if failed message is not expected,
        /// otherwise checks that quote response status is failed and that the failed message
        /// contains the expected text
        /// </summary>
        /// <param name="idAccount">Account id to quote for</param>
        /// <param name="idPO">PO id to quote for</param>
        /// <param name="testName"></param>
        /// <param name="testShortName"></param>
        /// <param name="partialFailedMessage">Part of the failed message that is expected.
        /// Skip or set to null if the request is expected to pass and to be verified</param>
        /// <returns>Created and verified response</returns>
        private QuoteResponse CreateAndVerifyQuote(int idAccount, int idPO, string testName, string testShortName, string partialFailedMessage = null)
        {
            #region Prepare

            //Values to use for verification
            decimal expectedQuoteTotal = 19.95M * 2 + 9.95M;
            string expectedQuoteCurrency = "USD";

            #endregion

            #region Test and Verify

            var request = new QuoteRequest();
            request.Accounts.Add(idAccount);
            request.ProductOfferings.Add(idPO);
            request.QuoteIdentifier = "MyQuoteId-" + testShortName + "-1234";
            request.QuoteDescription = "Quote generated by Automated Test: " + testName;
            request.ReportParameters = new ReportParams() { PDFReport = QuotingTestScenarios.RunPDFGenerationForAllTestsByDefault };
            request.EffectiveDate = MetraTime.Now;
            request.EffectiveEndDate = MetraTime.Now.AddMonths(2);

            QuoteResponse response = null;

            bool clientInvoked = false;
            try
            {
                response = SharedTestCodeQuoting.InvokeCreateQuote(request);
                clientInvoked = true;
            }
            catch (Exception ex)
            {
                Assert.Fail("QuotingService_CreateQuote_Client thrown an exception: " + ex.Message);
            }

            if (partialFailedMessage == null)
            {
                Assert.IsFalse(response.Status == QuoteStatus.Failed, response.FailedMessage);
                Assert.IsTrue(clientInvoked, "QuotingService_CreateQuote_Client didn't executed propely");
                Assert.AreEqual(expectedQuoteTotal, response.TotalAmount, "Wrong TotalAmount");
                Assert.AreEqual(expectedQuoteCurrency, response.Currency, "Wrong Currency");
            }
            else
            {
                Assert.IsTrue(response.Status == QuoteStatus.Failed, "The quote response status was not failed");
                Assert.IsTrue(response.FailedMessage.Contains(partialFailedMessage), "The quote response message was incorrect");
            }

            #endregion

            return response;
        }
        #endregion
    }
}
