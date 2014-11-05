﻿using System;
using System.Collections.Generic;
using MetraTech.NotificationEvents.EventHandler.Entities;
using MetraTech.UI.Common;
using MetraTech.UI.Controls;

public partial class Notifications : MTPage
{
  protected void Page_Load(object sender, EventArgs e)
  {

  }

  protected override void OnLoadComplete(EventArgs e)
  {
    NotificationsGrid.DataSourceURL =
      @"/MetraNet/Notifications/AjaxServices/GetNotifications.aspx";
    PopulateNotificationTypesDropDown();

    // For Partition users, suppress the partition filter
    if (PartitionLibrary.IsPartition)
    {
      NotificationsGrid.FindElementByID("id_partition").DefaultFilter = false;
      NotificationsGrid.FindElementByID("id_partition").Filterable = false;
    }
    else
    {
      PopulatePartitionsDropDown();
    }

  }

  protected void PopulateNotificationTypesDropDown()
  {
    List<NotificationEventMetaDataDB> notificaitonTypes = NotificationService.GetExisitingNotificationEventNames();
    int i = 0;
    foreach (NotificationEventMetaDataDB nmdb in notificaitonTypes)
    {
      var dropDownItem = new MTFilterDropdownItem();
      dropDownItem.Key = "" + i++;
      dropDownItem.Value = nmdb.NotificationEventName;
      NotificationsGrid.FindElementByID("notification_event_name").FilterDropdownItems.Add(dropDownItem);
    }

  }

  protected void PopulatePartitionsDropDown()
  {
    Dictionary<string, Int32> partitions = PartitionLibrary.RetrieveAllPartitions();
    foreach (string pname in partitions.Keys)
    {
      Int32 val;
      partitions.TryGetValue(pname, out val);
      var dropDownItem = new MTFilterDropdownItem();
      dropDownItem.Key = "" + val;
      dropDownItem.Value = pname;
      NotificationsGrid.FindElementByID("id_partition").FilterDropdownItems.Add(dropDownItem);
    }

  }

}