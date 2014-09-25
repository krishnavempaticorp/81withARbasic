using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.Mvc;
using ASP.Models;
using MetraNet.DbContext;
using System.Linq;
using System.Collections.Generic;
using System;
using MetraTech.ActivityServices.Common;
using MetraTech.DataAccess;

namespace ASP.Controllers
{
  [Authorize]
  public class ReportController : MTController
  {
    private const string sqlQueriesPath = @"..\Extensions\SystemConfig\config\SqlCustom\Queries\UI\Dashboard";

    public JsonResult NewCustomers()
    {
      using (var dbDataMart = GetDatamartContext())
      {
        var accountsByMonth = (from c in dbDataMart.Customer
                               join st in dbDataMart.SubscriptionTable on c.AccountId equals st.AccountId
                               where st.StartDate.HasValue
                                     && st.StartDate.Value >= DateTime.Today.AddMonths(-13)
                                     && st.StartDate.Value <= DateTime.Now.AddMonths(-1)
                               select new
                                 {
                                   Account = c.AccountId,
                                   Date = new DateTime(st.StartDate.Value.Year, st.StartDate.Value.Month, 1)
                                 }).ToList();

        return Json(accountsByMonth, JsonRequestBehavior.AllowGet);
      }
    }

    public JsonResult Revenue()
    {
      using (var context = GetNetMeterContext())
      {
        var dateFrom = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-13);
        var dateTo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddDays(-1);
        var result =
          context.T_invoice.Where(i => i.Invoice_date >= dateFrom
                                       && i.Invoice_date <= dateTo)
                           .Select(i => new
                           {
                             Date = new DateTime(i.Invoice_date.Year, i.Invoice_date.Month, 1),
                             Currency = i.Invoice_currency.Trim(),
                             Amount = i.Invoice_amount
                           })
                           .GroupBy(i => new { i.Date, i.Currency })
                           .Select(i => new
                           {
                             Date = i.Key.Date,
                             Currency = i.Key.Currency,
                             Amount = i.Sum(grp => grp.Amount)
                           })
                           .OrderBy(i => i.Date).ThenBy(i => i.Currency)
                           .ToList();
        //var result = new[] {new {Date = new DateTime(2013, 2, 1),  Currency = "USD", Amount = 302678},
        //                    new {Date = new DateTime(2013, 2, 1),  Currency = "EUR", Amount = 22678},
        //                    new {Date = new DateTime(2013, 2, 1),  Currency = "YEN", Amount = 6678},
        //                    new {Date = new DateTime(2013, 3, 1),  Currency = "USD", Amount = 307678},
        //                    new {Date = new DateTime(2013, 3, 1),  Currency = "EUR", Amount = 28678},
        //                    new {Date = new DateTime(2013, 3, 1),  Currency = "YEN", Amount = 7078},
        //                    new {Date = new DateTime(2013, 4, 1),  Currency = "USD", Amount = 312678},
        //                    new {Date = new DateTime(2013, 4, 1),  Currency = "EUR", Amount = 29678},
        //                    new {Date = new DateTime(2013, 4, 1),  Currency = "YEN", Amount = 7678},
        //                    new {Date = new DateTime(2013, 5, 1),  Currency = "USD", Amount = 309678},
        //                    new {Date = new DateTime(2013, 5, 1),  Currency = "EUR", Amount = 32678},
        //                    new {Date = new DateTime(2013, 5, 1),  Currency = "YEN", Amount = 7478},
        //                    new {Date = new DateTime(2013, 6, 1),  Currency = "USD", Amount = 307978},
        //                    new {Date = new DateTime(2013, 6, 1),  Currency = "EUR", Amount = 22678},
        //                    new {Date = new DateTime(2013, 6, 1),  Currency = "YEN", Amount = 6678},
        //                    new {Date = new DateTime(2013, 7, 1),  Currency = "USD", Amount = 302678},
        //                    new {Date = new DateTime(2013, 7, 1),  Currency = "EUR", Amount = 22678},
        //                    new {Date = new DateTime(2013, 7, 1),  Currency = "YEN", Amount = 6678},
        //                    new {Date = new DateTime(2013, 8, 1),  Currency = "USD", Amount = 302678},
        //                    new {Date = new DateTime(2013, 8, 1),  Currency = "EUR", Amount = 22678},
        //                    new {Date = new DateTime(2013, 8, 1),  Currency = "YEN", Amount = 6678},
        //                    new {Date = new DateTime(2013, 9, 1),  Currency = "USD", Amount = 302678},
        //                    new {Date = new DateTime(2013, 9, 1),  Currency = "EUR", Amount = 22678},
        //                    new {Date = new DateTime(2013, 9, 1),  Currency = "YEN", Amount = 6678},
        //                    new {Date = new DateTime(2013, 10, 1), Currency = "USD", Amount = 302678},
        //                    new {Date = new DateTime(2013, 10, 1), Currency = "EUR", Amount = 22678},
        //                    new {Date = new DateTime(2013, 10, 1), Currency = "YEN", Amount = 6678},
        //                    new {Date = new DateTime(2013, 11, 1), Currency = "USD", Amount = 302678},
        //                    new {Date = new DateTime(2013, 11, 1), Currency = "EUR", Amount = 22678},
        //                    new {Date = new DateTime(2013, 11, 1), Currency = "YEN", Amount = 6678},
        //                    new {Date = new DateTime(2014, 1, 1),  Currency = "USD", Amount = 402678},
        //                    new {Date = new DateTime(2014, 1, 1),  Currency = "EUR", Amount = 62678},
        //                    new {Date = new DateTime(2014, 1, 1),  Currency = "YEN", Amount = 9678}
        //                   };
        return Json(result, JsonRequestBehavior.AllowGet);
      }
    }

    public JsonResult MRRByProduct()
    {
      using (var dbDataMart = GetDatamartContext())
      {
        var MRRByMonth = (from subByMonth in dbDataMart.SubscriptionsByMonth
                          join sub in dbDataMart.SubscriptionTable on subByMonth.SubscriptionId equals
                            sub.SubscriptionId
                          join c in dbDataMart.Customer on sub.AccountId equals c.AccountId
                          where
                            subByMonth.Month.HasValue &&
                            subByMonth.Month >= DateTime.Today.AddMonths(-13) &&
                            subByMonth.Month < new DateTime(DateTime.Now.AddMonths(12).Year, DateTime.Now.Month, 1)
                          group subByMonth by new
                            {
                              Date = new DateTime(subByMonth.Month.Value.Year, subByMonth.Month.Value.Month, 1),
                              CurrencyCode = sub.FeeCurrency
                            }
                            into grp
                            select new
                              {
                                grp.Key.Date,
                                grp.Key.CurrencyCode,
                                Amount =
                              grp.Sum(
                                x =>
                                x.MRRBase + x.MRRNew + x.MRRRenewal + x.MRRPriceChange + x.MRRChurn + x.MRRCancellation)
                              }).ToList();

        return Json(MRRByMonth, JsonRequestBehavior.AllowGet);
      }
    }


    public MTList<RevRecModel> RevenueRecognition()
    {
      var list = new MTList<RevRecModel>();

      return list;
    }

    public ActionResult RevRec()
    {
      var startDate = new DateTime(2014, 09, 1);
      var endDate = new DateTime(2014, 10, 1);

      var paramDictIncremental = new Dictionary<string, object>
        {
          {"%%START_DATE%%", startDate},
          {"%%END_DATE%%", endDate}
        };

      var paramDictDeferred = new Dictionary<string, object>
        {
          {"%%END_DATE%%", endDate},
        };

      var paramDictEarned = new Dictionary<string, object>
        {
          {"%%START_DATE%%", startDate},
        };

      var incremental = GetData("__GET_INCREMENTAL_EARNED_REVENUE__", paramDictIncremental);
      var deferred = GetData("__GET_DEFERRED_REVENUE__", paramDictDeferred);
      var earned = GetData("__GET_EARNED_REVENUE__", paramDictEarned);

      var groups =
        earned.Select(x => new {x.Currency, x.RevenueCode, x.DeferredRevenueCode})
              .Union(incremental.Select(x => new {x.Currency, x.RevenueCode, x.DeferredRevenueCode}))
              .Union(deferred.Select(x => new {x.Currency, x.RevenueCode, x.DeferredRevenueCode}))
              .Distinct()
              .ToList();

      var data = new List<string[]>();

      foreach (var rowGroup in groups)
      {
        var earnedRow = new RevRecModel
          {
            Currency = rowGroup.Currency,
            RevenueCode = rowGroup.RevenueCode,
            DeferredRevenueCode = rowGroup.DeferredRevenueCode,
            RevenuePart = "Earned"
          };

        var incrementalRow = new RevRecModel
        {
          Currency = rowGroup.Currency,
          RevenueCode = rowGroup.RevenueCode,
          DeferredRevenueCode = rowGroup.DeferredRevenueCode,
          RevenuePart = "Incremental"
        };

        var deferredRow = new RevRecModel
        {
          Currency = rowGroup.Currency,
          RevenueCode = rowGroup.RevenueCode,
          DeferredRevenueCode = rowGroup.DeferredRevenueCode,
          RevenuePart = "Deferred"
        };

        var decimalTotalEarned = new Dictionary<string, double>();
        var decimalIncrementalEarned = new Dictionary<string, double>();
        var decimalDeferred = new Dictionary<string, double>();

        var calculatedDeferred = deferred.Where(x => x.Currency.Equals(rowGroup.Currency)
                                                       && x.RevenueCode.Equals(rowGroup.RevenueCode)
                                                       && x.DeferredRevenueCode.Equals(rowGroup.DeferredRevenueCode))
                                           .Select(x => x.ProrationDate * x.ProrationAmount).Sum();

        var calculatedIncrementalEarned = incremental.Where(x => x.Currency.Equals(rowGroup.Currency)
                                                       && x.RevenueCode.Equals(rowGroup.RevenueCode)
                                                       && x.DeferredRevenueCode.Equals(rowGroup.DeferredRevenueCode))
                                           .Select(x => x.ProrationDate * x.ProrationAmount).Sum();

        var calculatedTotalEarned = earned.Where(x => x.Currency.Equals(rowGroup.Currency)
                                                       && x.RevenueCode.Equals(rowGroup.RevenueCode)
                                                       && x.DeferredRevenueCode.Equals(rowGroup.DeferredRevenueCode))
                                           .Select(x => x.ProrationDate * x.ProrationAmount).Sum() + calculatedIncrementalEarned;


        decimalTotalEarned.Add("1", (double)calculatedTotalEarned);
        decimalIncrementalEarned.Add("1", (double)calculatedIncrementalEarned);
        decimalDeferred.Add("1", (double)calculatedDeferred);

        for (var i = 1; i < 13; i++)
        {
          var monthNext = endDate.AddMonths(i).AddDays(-1);
          var calculatedDeferredPrev = calculatedDeferred;

          calculatedDeferred = deferred.Where(x => x.Currency.Equals(rowGroup.Currency)
                                                       && x.RevenueCode.Equals(rowGroup.RevenueCode)
                                                       && x.DeferredRevenueCode.Equals(rowGroup.DeferredRevenueCode)
                                                       && x.EndSubscriptionDate > monthNext)
                                           .Select(x => (x.EndSubscriptionDate - monthNext).Days * x.ProrationAmount).Sum();

          calculatedIncrementalEarned = calculatedDeferredPrev - calculatedDeferred;

          calculatedTotalEarned = calculatedTotalEarned + calculatedIncrementalEarned;

          decimalIncrementalEarned.Add(i.ToString(CultureInfo.InvariantCulture), (double)calculatedIncrementalEarned);
          decimalDeferred.Add(i.ToString(CultureInfo.InvariantCulture), (double)calculatedDeferred);
          decimalTotalEarned.Add(i.ToString(CultureInfo.InvariantCulture), (double)calculatedTotalEarned);
        }

        RoundRevRecModel(earnedRow, decimalTotalEarned);
        RoundRevRecModel(incrementalRow, decimalIncrementalEarned);
        RoundRevRecModel(deferredRow, decimalDeferred);
        
        data.Add(SerializeItems(earnedRow));
        data.Add(SerializeItems(incrementalRow));
        data.Add(SerializeItems(deferredRow));
      }

      return Json(new
      {
        sEcho = "Test",
        iTotalRecords = data.Count,
        iTotalDisplayRecords = 10,
        aaData = data
      }, JsonRequestBehavior.AllowGet);
    }

    private static void RoundRevRecModel(RevRecModel revRecModel, Dictionary<string, double> calculations)
    {
      revRecModel.Amount1 = calculations["1"];
      revRecModel.Amount2 = calculations["2"];
      revRecModel.Amount3 = calculations["3"];
      revRecModel.Amount4 = calculations["4"];
      revRecModel.Amount5 = calculations["5"];
      revRecModel.Amount6 = calculations["6"];
      revRecModel.Amount7 = calculations["7"];
      revRecModel.Amount8 = calculations["8"];
      revRecModel.Amount9 = calculations["9"];
      revRecModel.Amount10 = calculations["10"];
      revRecModel.Amount11 = calculations["11"];
      revRecModel.Amount12 = calculations["12"];
      revRecModel.Amount13 = calculations["13"];
    }


    private IEnumerable<SelectListItem> GetProductCodes()
    {
      var list = new List<SelectListItem> { new SelectListItem { Selected = true, Text = "All", Value = "all" } };

      using (var dbContext = GetDatamartContext())
      {
        var products =
          (from subByMonth in dbContext.SubscriptionsByMonth
           join sub in dbContext.SubscriptionTable on subByMonth.SubscriptionId equals sub.SubscriptionId
           where
             subByMonth.Month.HasValue &&
             subByMonth.Month >= DateTime.Today.AddMonths(-13) &&
             subByMonth.Month <= DateTime.Now
           select sub.ProductCode).Distinct().OrderBy(x => x).ToList();

        list.AddRange(products.Select(productCode => new SelectListItem { Text = productCode, Value = productCode }));
      }
      return list;
    }

    private IEnumerable<SelectListItem> GetTerritoryCodes()
    {
      var list = new List<SelectListItem> { new SelectListItem { Selected = true, Text = "All", Value = "all" } };

      using (var dbContext = GetDatamartContext())
      {
        var codes =
          (from st in dbContext.SubscriptionTable
           join c in dbContext.Customer on st.AccountId equals c.AccountId
           where
             st.StartDate.HasValue &&
             st.StartDate.Value >= DateTime.Today.AddMonths(-13) &&
             st.StartDate.Value <= DateTime.Now
           select c.Territorycode).Distinct().OrderBy(x => x).ToList();

        list.AddRange(codes.Select(code => new SelectListItem { Text = code, Value = code }));
      }
      return list;
    }

    private static DataMart GetDatamartContext()
    {
      return new DataMart(GetDefaultDatabaseConnection("localhost", "AnalyticsDatamart", "nmdbo", "MetraTech1"));
    }

    private static NetMeter GetNetMeterContext()
    {
      return new NetMeter(GetDefaultDatabaseConnection("localhost", "NetMeter", "nmdbo", "MetraTech1"));
    }

    private static DbConnection GetDefaultDatabaseConnection(string serverName, string dbName, string userName, string password)
    {
      //var connectionInfo = new ConnectionInfo("NetMeter"){ Catalog = "Subscriptiondatamart" };
      //return ConnectionBase.GetDbConnection(connectionInfo, false);
      var connString = new SqlConnectionStringBuilder
      {
        DataSource = serverName,
        InitialCatalog = dbName,
        UserID = userName,
        Password = password
      };
      return new SqlConnection(connString.ToString());
    }





    private List<SegregatedCharges> GetData(string sqlQueryTag, Dictionary<string, object> paramDict)
    {
      using (IMTConnection conn = ConnectionManager.CreateConnection())
      {
        using (IMTAdapterStatement stmt = conn.CreateAdapterStatement(sqlQueriesPath, sqlQueryTag))
        {
          if (paramDict != null)
          {
            foreach (var pair in paramDict)
            {
              stmt.AddParam(pair.Key, pair.Value);
            }
          }

          using (IMTDataReader reader = stmt.ExecuteReader())
          {

            return ConstructItems(reader);
            // get the total rows that would be returned without paging
          }
        }
      }
    }

    protected List<SegregatedCharges> ConstructItems(IMTDataReader rdr)
    {
      var res = new List<SegregatedCharges>();

      while (rdr.Read())
      {
        var sch = new SegregatedCharges
        {
          Currency = rdr.GetString("am_currency"),
          RevenueCode = !rdr.IsDBNull("c_RevenueCode") ? rdr.GetString("c_RevenueCode") : "",
          DeferredRevenueCode = !rdr.IsDBNull("c_DeferredRevenueCode") ? rdr.GetString("c_DeferredRevenueCode") : "",
          StartSubscriptionDate = rdr.GetDateTime("SubscriptionStart"),
          EndSubscriptionDate = rdr.GetDateTime("SubscriptionEnd"),
          ProrationDate = rdr.GetInt32("c_ProratedDays"),
          ProrationAmount = rdr.GetDecimal("c_ProratedDailyRate")
        };

        res.Add(sch);
      }

      return res;
    }

    protected string[] SerializeItems(RevRecModel item)
    {
      //var res = new string[] { "USD", "Earned", "124.34", "124.34", "124.34", "124.34", "124.34", "124.34", "124.34", "124.34", "124.34", "124.34", "124.34", "124.34" };
      var res = new string[17];
      res[0] = item.Currency;
      res[1] = item.RevenueCode;
      res[2] = item.DeferredRevenueCode;
      res[3] = item.RevenuePart;
      res[4] = item.Amount1.ToString("N2");
      res[5] = item.Amount2.ToString("N2");
      res[6] = item.Amount3.ToString("N2");
      res[7] = item.Amount4.ToString("N2");
      res[8] = item.Amount5.ToString("N2");
      res[9] = item.Amount6.ToString("N2");
      res[10] = item.Amount7.ToString("N2");
      res[11] = item.Amount8.ToString("N2");
      res[12] = item.Amount9.ToString("N2");
      res[13] = item.Amount10.ToString("N2");
      res[14] = item.Amount11.ToString("N2");
      res[15] = item.Amount12.ToString("N2");
      res[16] = item.Amount13.ToString("N2");

      return res;
    }

    public ActionResult DefRevScheduleWidgetReport()
    {
      return Json(new[]
        {
          new { date = DateTime.Parse("2014-08-01"), deferred = 900, earned = 300 },
          new { date = DateTime.Parse("2014-09-01"), deferred = 800, earned = 400 },
          new { date = DateTime.Parse("2014-10-01"), deferred = 700, earned = 500 },
          new { date = DateTime.Parse("2014-11-01"), deferred = 600, earned = 600 },
          new { date = DateTime.Parse("2014-12-01"), deferred = 500, earned = 700 },
          new { date = DateTime.Parse("2015-01-01"), deferred = 400, earned = 800 },
          new { date = DateTime.Parse("2015-02-01"), deferred = 300, earned = 900 },
          new { date = DateTime.Parse("2015-03-01"), deferred = 200, earned = 1000 },
          new { date = DateTime.Parse("2015-04-01"), deferred = 100, earned = 1100 },
          new { date = DateTime.Parse("2015-05-01"), deferred = 0, earned = 1200 }      
        }, JsonRequestBehavior.AllowGet);
    }

    /*public ActionResult Churn()
    {
      Title = "Churn Report";

      var monthsArr = new string[12];
      var lastMonth = (new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).AddMonths(-1);

      for (int i = 0; i <= 11; i++)
        monthsArr[11 - i] = lastMonth.AddMonths(-i).ToString("MMM yy");

      ViewBag.GridMonthsArr = monthsArr;

      ViewBag.ProductsList = GetProductCodes();

      return View();
    }

    public ActionResult ExpiringSubscriptions()
    {
      Title = "Expiring Subscriptions Report";

      var monthsArr = new string[12];
      var lastMonth = (new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).AddMonths(-1);

      for (int i = 0; i <= 11; i++)
        monthsArr[11 - i] = lastMonth.AddMonths(-i).ToString("MMM yy");

      ViewBag.GridMonthsArr = monthsArr;

      ViewBag.TerritoryList = GetTerritoryCodes();

      return View();
    }

    public ActionResult NewTCV()
    {
      Title = "New Total Contract Value Report";

      var monthsArr = new string[12];
      var lastMonth = (new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).AddMonths(-1);

      for (int i = 0; i <= 11; i++)
        monthsArr[11 - i] = lastMonth.AddMonths(-i).ToString("MMM yy");

      ViewBag.GridMonthsArr = monthsArr;

      ViewBag.TerritoryList = GetTerritoryCodes();

      return View();
    }

    public ActionResult RevenueRecognition()
    {
      Title = "Revenue Recognition Report";

      ViewBag.ProductsList = GetProductCodes();
      ViewBag.SubscriptionId = "N/A";
      ViewBag.TotalRevenue = "$0.00";
      return View();
    }

    public JsonResult RevenueRecognitionTotal()
    {
      var result = "";
      var subscriptionIdStr = GetParamValueFromRequest("subscriptionId");
      int subscriptionId;
      int.TryParse(subscriptionIdStr, out subscriptionId);

      using (var dbDataMart = new DataMart(GetDefaultDatabaseConnectionSubscriptiondatamart()))
      {

        var amount = (from sr in dbDataMart.RevenueRecognitionReport
                  where (String.IsNullOrEmpty(subscriptionIdStr) || sr.SubscriptionId == subscriptionId)
                  select sr).Sum(x => (decimal?)x.DRAmount) ?? 0m;
        result = amount.ToString("0.00");
      }
      return Json(new {TotalRevenue = result}, JsonRequestBehavior.AllowGet);
    }

    public ActionResult SubscriptionCounters()
    {
      Title = "Subscription Counters Report";

      ViewBag.ProductsList = GetProductCodes();
      ViewBag.SalesRepList = GetSalesReps();
      ViewBag.CustomerList = GetCustomers();

      return View();
    }

    public JsonResult SubscriptionCountersPieCharts()
    {
      var productCode = GetParamValueFromRequest("productCode");
      var salesRepCode = GetParamValueFromRequest("salesrepCode");
      var customerCode = GetParamValueFromRequest("customerCode");
      var resultArr = new String[4];
      int customerId;
      int.TryParse(customerCode, out customerId);
      using (var dbDataMart = new DataMart(GetDefaultDatabaseConnectionSubscriptiondatamart()))
      {
        var counters = (from c in dbDataMart.Counters
                      where c.StartDate >= DateTime.Today.AddMonths(-13)
                         && c.StartDate < new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)
                         && (String.IsNullOrEmpty(customerCode) || c.PayeeID == customerId)
                         && c.BundledUnits != 999999.00m
                      group c by new
                      {
                        c.SubscriptionID
                      } into grp
                      select new
                      {
                        Count = grp.Key.SubscriptionID,
                        ConsumedUnits = grp.Sum(x => x.UnitsConsumed > x.BundledUnits ? x.BundledUnits : x.UnitsConsumed),
                        BundledUnits = grp.Sum(x => x.BundledUnits),
                        OverageConsumedUnits = grp.Sum(x => x.UnitsConsumed > x.BundledUnits ? x.UnitsConsumed - x.BundledUnits : 0m),
                        PricePerUnit = grp.Sum(x => x.ProratedBundledPricePerUnit)
                      }).ToList();


        var consumedUnitsPercents = counters.Sum(x => x.ConsumedUnits / x.BundledUnits * 100);
        var countSubsribers = counters.Count();
        var consumedOverageUnitsPercents = counters.Sum(x => x.OverageConsumedUnits / x.BundledUnits * 100);
        var countSubsribersOveraged = counters.Count(x => x.OverageConsumedUnits > 0);
        var bundledConsumedUnits = counters.Sum(x => x.ConsumedUnits);
        var overageConsumedUnits = counters.Sum(x => x.OverageConsumedUnits);
        var pricesPerUnit = counters.Sum(x => x.PricePerUnit);

        // Average % Bundled Units Consumed
        resultArr[0] = (consumedUnitsPercents / countSubsribers).ToString("0.00");
        // Average % Overage Units Consumed
        resultArr[1] = (consumedOverageUnitsPercents / countSubsribersOveraged).ToString("0.00");
        // Average Bundled vs. Overage Units
        resultArr[2] = String.Format("{0}/{1}",
                        (bundledConsumedUnits / countSubsribers).ToString("0."),
                        (overageConsumedUnits / countSubsribersOveraged).ToString("0."));
        // Average Price per Bundled Unit
        resultArr[3] = ((double)pricesPerUnit / countSubsribers).ToString("0.00");
      }
      return Json(resultArr, JsonRequestBehavior.AllowGet);
    }

    public XmlResult Chart(string id)
    {
      XElement result = null;
      var productCode = GetParamValueFromRequest("productCode");
      var territoryCode = GetParamValueFromRequest("territoryCode");
      var salesRepCode = GetParamValueFromRequest("salesrepCode");
      var customerCode = GetParamValueFromRequest("customerCode");

      switch (id)
      {
        case "NewCustomersByMonth":
          using (var dbDataMart = new DataMart(GetDefaultDatabaseConnectionSubscriptiondatamart()))
            result = ReportsChartService.NewCustomersByMonth(dbDataMart, territoryCode);
          break;
        case "MRRByMonth":
          var MRRBase = DataGridService.GetFilterBoolValueFromRequest(Request.QueryString, "MRRBase");
          var MRRNew = DataGridService.GetFilterBoolValueFromRequest(Request.QueryString, "MRRNew");
          var MRRRenewal = DataGridService.GetFilterBoolValueFromRequest(Request.QueryString, "MRRRenewal");
          var MRRPriceChange = DataGridService.GetFilterBoolValueFromRequest(Request.QueryString, "MRRPriceChange");
          var MRRChurn = DataGridService.GetFilterBoolValueFromRequest(Request.QueryString, "MRRChurn");
          var MRRCancellation = DataGridService.GetFilterBoolValueFromRequest(Request.QueryString, "MRRCancellation");

          using (var DataMart = new DataMart(GetDefaultDatabaseConnectionSubscriptiondatamart()))
            result = ReportsChartService.MRRByMonth(DataMart, productCode, territoryCode, MRRBase, MRRNew, MRRRenewal, MRRPriceChange, MRRChurn, MRRCancellation);
          break;
        case "NewTCVByMonth":
          using (var DataMart = new DataMart(GetDefaultDatabaseConnectionSubscriptiondatamart()))
            result = ReportsChartService.NewTCVByMonth(DataMart, territoryCode);
          break;
        case "ChurnByMonth":
          using (var DataMart = new DataMart(GetDefaultDatabaseConnectionSubscriptiondatamart()))
            result = ReportsChartService.ChurnByMonth(DataMart, productCode);
          break;
        case "ExpiringSubscriptionsByMonth":
          using (var DataMart = new DataMart(GetDefaultDatabaseConnectionSubscriptiondatamart()))
            result = ReportsChartService.ExpiringSubscriptionsByMonth(DataMart, territoryCode);
          break;
        case "ExpiringSubscriptionsQuantityByMonth":
          using (var DataMart = new DataMart(GetDefaultDatabaseConnectionSubscriptiondatamart()))
            result = ReportsChartService.ExpiringSubscriptionsQuantityByMonth(DataMart, territoryCode);
          break;
        case "SubscriptionCountersBundledUnitsPercentsByMonth":
          using (var DataMart = new DataMart(GetDefaultDatabaseConnectionSubscriptiondatamart()))
            result = ReportsChartService.SubscriptionCountersBundledUnitsPercentsByMonth(DataMart, productCode, salesRepCode, customerCode);
          break;
        case "SubscriptionCountersOverageByMonth":
          using (var dataMart = new DataMart(GetDefaultDatabaseConnectionSubscriptiondatamart()))
            result = ReportsChartService.SubscriptionCountersOverageByMonth(dataMart, productCode, salesRepCode, customerCode);
          break;
        case "SubscriptionCountersOveragePercentsByMonth":
          using (var dataMart = new DataMart(GetDefaultDatabaseConnectionSubscriptiondatamart()))
            result = ReportsChartService.SubscriptionCountersOveragePercentsByMonth(dataMart, productCode, salesRepCode, customerCode);
          break;
        case "SubscriptionCountersUnitsByMonth":
          using (var dataMart = new DataMart(GetDefaultDatabaseConnectionSubscriptiondatamart()))
            result = ReportsChartService.SubscriptionCountersUnitsByMonth(dataMart, productCode, salesRepCode, customerCode);
          break;
        case "SubscriptionCountersPricePerBundledUnitByMonth":
          using (var DataMart = new DataMart(GetDefaultDatabaseConnectionSubscriptiondatamart()))
            result = ReportsChartService.SubscriptionCountersPricePerBundledUnitByMonth(DataMart, productCode, salesRepCode, customerCode);
          break;
      }

      return new XmlResult(result);
    }

    private string GetParamValueFromRequest(string paramName)
    {
      string paramValue = null;

      if (Request.QueryString.AllKeys.Contains(paramName))
        paramValue = Request.QueryString.Get(paramName);

      return paramValue;
    }
     
    private IEnumerable<SelectListItem> GetSalesReps()
    {
      var list = new List<SelectListItem>();
      list.Add(new SelectListItem() { Selected = true, Text = "All", Value = "all" });

      using (var dbContext = new DataMart(GetDefaultDatabaseConnectionSubscriptiondatamart()))
      {
        var codes =
          (from st in dbContext.TerritorryCodeCSRMap
           where
             st.C_firstname != string.Empty || st.C_lastname != string.Empty
           select new { Name = st.C_firstname + " " + st.C_lastname, AccountId = st.Id_acc.Value }).Distinct().OrderBy(x => x.Name).ToList();

        foreach (var code in codes)
          list.Add(new SelectListItem() { Text = code.Name, Value = code.AccountId.ToString() });
      }
      return list;
    }

    private IEnumerable<SelectListItem> GetCustomers()
    {
      var list = new List<SelectListItem>();
      list.Add(new SelectListItem() { Selected = true, Text = "All", Value = "all" });

      using (var dbContext = new DataMart(GetDefaultDatabaseConnectionSubscriptiondatamart()))
      {
        var codes =
          (from c in dbContext.Customer
           where
             c.CompanyName != string.Empty
           select new { Name = c.CompanyName, AccountId = c.AccountId }).Distinct().OrderBy(x => x.Name).ToList();

        list.AddRange(codes.Select(code => new SelectListItem() {Text = code.Name, Value = code.AccountId.ToString()}));
      }
      return list;
    }
     
    */

  }
}

