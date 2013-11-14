﻿using System;
using MetraTech.TestCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MetraTech.UsageServer.Test.Unit.Cycles
{
  [TestClass]
  public class CycleUtilsUnitTest
  {
    [TestMethod, MTUnitTest]
    public void LastDayOfMonthTest()
    {
      var day = new DateTime(2100, 2, 1);
      var lastDayOfMonth = CycleUtils.LastDayOfMonth(day);
      Assert.AreEqual(new DateTime(2100, 2, 28), lastDayOfMonth);
    }

    [TestMethod, MTUnitTest]
    public void MoveToDayWhenDayOfMonthGreaterThanCountOfDaysInMonthTest()
    {
      var day = new DateTime(2013, 4, 1);
      var lastDayOfMonth = CycleUtils.MoveToDay(day, 31);
      Assert.AreEqual(new DateTime(2013, 4, 30), lastDayOfMonth);
    }

    [TestMethod, MTUnitTest]
    public void MoveToDayWhenDayOfMonthIsLessThanCountOfDaysInMonthTest()
    {
      var day = new DateTime(2013, 4, 1);
      var lastDayOfMonth = CycleUtils.MoveToDay(day, 5);
      Assert.AreEqual(new DateTime(2013, 4, 5), lastDayOfMonth);
    }

    [TestMethod, MTUnitTest]
    public void ParseCycleType()
    {
      Assert.AreEqual(CycleType.Monthly, CycleUtils.ParseCycleType("Monthly"));
      Assert.AreEqual(CycleType.Daily, CycleUtils.ParseCycleType("Daily"));
      Assert.AreEqual(CycleType.Weekly, CycleUtils.ParseCycleType("Weekly"));
      Assert.AreEqual(CycleType.BiWeekly, CycleUtils.ParseCycleType("Bi-weekly"));
      Assert.AreEqual(CycleType.SemiMonthly, CycleUtils.ParseCycleType("Semi-monthly"));
      Assert.AreEqual(CycleType.Quarterly, CycleUtils.ParseCycleType("Quarterly"));
      Assert.AreEqual(CycleType.Annual, CycleUtils.ParseCycleType("Annually"));
      Assert.AreEqual(CycleType.SemiAnnual, CycleUtils.ParseCycleType("Semi-Annually"));
      Assert.AreEqual(CycleType.All, CycleUtils.ParseCycleType("All"));
      ExceptionAssert.Expected<ArgumentException>(() => CycleUtils.ParseCycleType("Monthy"),
        "Invalid cycle type name Monthy");
    }
  }
}
