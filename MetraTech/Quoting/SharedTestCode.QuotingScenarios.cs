﻿using System;
using System.Collections.Generic;
using System.ServiceProcess;
using MetraTech.Interop.MTAuth;
using MetraTech.Interop.MTProductCatalog;
using MetraTech.Core.Services.ClientProxies;
using MetraTech.ActivityServices.Common;

using MetraTech.DomainModel.ProductCatalog;
using MetraTech.Account.ClientProxies;
using MetraTech.DomainModel.BaseTypes;
using MetraTech.DomainModel.AccountTypes;
using MetraTech.DomainModel.Enums.Core.Metratech_com_billingcycle;
using MetraTech.DomainModel.Enums.Account.Metratech_com_accountcreation;
using MetraTech.DomainModel.Enums.Core.Global;
using MetraTech.DomainModel.Enums.Core.Global_SystemCurrencies;
using MetraTech.DataAccess;
using MetraTech.Quoting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MetraTech.Shared.Test
{
  public class QuotingTestScenarios
  {
    public const bool RunPDFGenerationForAllTestsByDefault = false; //Might eventually be a test setting

    public static QuoteResponse CreateQuoteAndVerifyResults(QuoteRequest request,
                                                            decimal expectedQuoteTotal,
                                                            string expectedQuoteCurrency,
                                                            int expectedQuoteFlatRCsCount,
                                                            int expectedQuoteNRCsCount,
                                                            int? expectedQuoteUDRCsCount = null,
                                                            QuotingImplementation quotingImplementation = null)
    {
      //Instantiate our implementation
      var quotingRepositoryForTestRun = new QuotingRepository();

      //Record pv counts before test
      int beforeQuoteNRCsCount = SharedTestCodeQuoting.GetNRCsCount();
      int beforeQuoteFlatRCsCount = SharedTestCodeQuoting.GetFlatRCsCount();
      int beforeQuoteUDRCsCount = SharedTestCodeQuoting.GetUDRCsCount();
      int beforeQuoteHeadersCount = quotingRepositoryForTestRun.GetQuoteHeaderCount();
      int beforeQuoteContentsCount = quotingRepositoryForTestRun.GetQuoteContentCount();
      int beforeAccountsForQuoteCount = quotingRepositoryForTestRun.GetAccountForQuoteCount();
      int beforePOsforQuoteCount = quotingRepositoryForTestRun.GetPOForQuoteCount();

      UsageAndFailedTransactionCount usageAndFailedTransactionCount = UsageAndFailedTransactionCount.CreateSnapshot();

      // Record expected values
      const int expectedQuoteHeadersCount = 1;
      const int expectedQuoteContentsCount = 1;
      int expectedAccountsForQuoteCount = request.Accounts.Count;
      int expectedPOsforQuoteCount = request.ProductOfferings.Count;

      //Instantiate our implementation
      if (quotingImplementation == null)
      {
        quotingImplementation = GetDefaultQuotingImplementationForTestRun(quotingRepositoryForTestRun);
      }

      #region CreateQuote

      int idCurrentQuote = quotingImplementation.StartQuote(request);

      int duringQuoteHeadersCount = quotingRepositoryForTestRun.GetQuoteHeaderCount();
      int duringQuoteContentsCount = quotingRepositoryForTestRun.GetQuoteContentCount();
      int duringQuoteAccountsCount = quotingRepositoryForTestRun.GetAccountForQuoteCount();
      int duringQuotePOsCount = quotingRepositoryForTestRun.GetPOForQuoteCount();

      SharedTestCodeQuoting.VerifyQuoteRequestCorrectInRepository(idCurrentQuote, request, quotingImplementation.QuotingRepository);
        
      // Ask backend to calculate RCs
      quotingImplementation.AddRecurringChargesToQuote();

      int duringQuoteFlatRCsCount = SharedTestCodeQuoting.GetFlatRCsCount();
        
      // Ask backend to calculate NRCs
      quotingImplementation.AddNonRecurringChargesToQuote();

      int duringQuoteNRCsCount = SharedTestCodeQuoting.GetNRCsCount();

      int duringQuoteUDRCsCount = SharedTestCodeQuoting.GetUDRCsCount();

      // Ask backend to finalize quote
      QuoteResponse preparedQuote = quotingImplementation.FinalizeQuote();

      int afterQuoteFlatRCsCount = SharedTestCodeQuoting.GetFlatRCsCount();
      int afterQuoteNRCsCount = SharedTestCodeQuoting.GetNRCsCount();
      int afterQuoteUDRCsCount = SharedTestCodeQuoting.GetUDRCsCount();

      #endregion

      #region Check

      //Verify the number of charges was as expected
      Assert.AreEqual(expectedQuoteFlatRCsCount, duringQuoteFlatRCsCount - beforeQuoteFlatRCsCount,
                      "Quoting process did not generate expected number of RCs");
      Assert.AreEqual(expectedQuoteNRCsCount, duringQuoteNRCsCount - beforeQuoteNRCsCount,
                      "Quoting process did not generate expected number of NRCs");

      // Verify the number of UDRCs if needed
      if (expectedQuoteUDRCsCount.HasValue)
      {
        Assert.AreEqual(expectedQuoteUDRCsCount, duringQuoteUDRCsCount - beforeQuoteUDRCsCount,
                        "Quoting process did not generate expected number of UDRCs");
      }

      //Verify the number of instances in the tables for quoting was as expected
      Assert.AreEqual(expectedQuoteHeadersCount, duringQuoteHeadersCount - beforeQuoteHeadersCount,
                      "Quoting process did not generate expected number of headers for quote");
      Assert.AreEqual(expectedQuoteContentsCount, duringQuoteContentsCount - beforeQuoteContentsCount,
                      "Quoting process did not generate expected number of contents for quote");
      Assert.AreEqual(expectedAccountsForQuoteCount, duringQuoteAccountsCount - beforeAccountsForQuoteCount,
                      "Quoting process did not generate expected number of accounts for quote");
      Assert.AreEqual(expectedPOsforQuoteCount, duringQuotePOsCount - beforePOsforQuoteCount,
                      "Quoting process did not generate expected number of POs for quote");

      // Verify the quote total is as expected. If UDRCs are expected than TotalAmount with them is greater than without 
      Assert.AreEqual(expectedQuoteTotal, preparedQuote.TotalAmount, "Created quote total is not what was expected");
      /*if (expectedQuoteUDRCsCount.HasValue)
      {
        Assert.AreEqual(expectedQuoteTotal < preparedQuote.TotalAmount, "Created quote total does not contain UDRCs amount");
      }
      else
      {
        Assert.AreEqual(expectedQuoteTotal, preparedQuote.TotalAmount, "Created quote total is not what was expected");
      }*/
      Assert.AreEqual(expectedQuoteCurrency, preparedQuote.Currency);

      //Verify response is in repository
      SharedTestCodeQuoting.VerifyQuoteResponseCorrectInRepository(idCurrentQuote, preparedQuote,
                                                                   quotingImplementation.QuotingRepository);

      //Todo: Verify PDF generated

      //Verify usage cleaned up: check count of RCs and NRCs before and after
      if (!request.DebugDoNotCleanupUsage)
      {
        Assert.AreEqual(beforeQuoteFlatRCsCount, afterQuoteFlatRCsCount, "Quoting left behind/didn't cleanup usage");
        Assert.AreEqual(beforeQuoteNRCsCount, afterQuoteNRCsCount, "Quoting left behind/didn't cleanup usage");
        Assert.AreEqual(beforeQuoteUDRCsCount, afterQuoteUDRCsCount, "Quoting left behind/didn't cleanup usage");

        usageAndFailedTransactionCount.VerifyNoChange();
      }

      #endregion

      return preparedQuote;
    }

    public static QuotingImplementation GetDefaultQuotingImplementationForTestRun(IQuotingRepository quotingRepositoryForTestRun = null)
    {
      //Instantiate our implementation
      if (quotingRepositoryForTestRun == null)
        quotingRepositoryForTestRun = new QuotingRepositoryDummy();

      QuotingImplementation quotingImplementation = new QuotingImplementation(QuotingConfigurationManager.LoadConfigurationFromFile(),
                                                                              SharedTestCode.LoginAsAdmin(),
                                                                              quotingRepositoryForTestRun);
      return quotingImplementation;
    }

    public static void RunTestCheckingBadInputs(IEnumerable<int> accountIds, IEnumerable<int> poIds, string expectedErrorMessagePartial)
    {
      var request = new QuoteRequest();

      request.Accounts.AddRange(accountIds);

      request.ProductOfferings.AddRange(poIds);

      request.ReportParameters = new ReportParams()
        {
          PDFReport = RunPDFGenerationForAllTestsByDefault
        };

      // Run quote and make sure it throws the expected message
      try
      {
        CreateQuoteAndVerifyResults(request, 0, string.Empty, 0, 0);
        Assert.Fail("An exception should have been thrown due to invalid input parameters");
      }
      catch (Exception ex)
      {
        Assert.IsTrue(ex.Message.Contains(expectedErrorMessagePartial));
      }
    }
  }
}
