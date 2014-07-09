﻿using System;
using System.ServiceModel;
using System.Web;
using System.Web.Script.Serialization;

using MetraTech.ActivityServices.Common;
using MetraTech.Debug.Diagnostics;
using MetraTech.Core.Services.ClientProxies;
using MetraTech.DomainModel.ProductCatalog;
using MetraTech.UI.Common;
using MetraTech.UI.Controls;

public partial class Adjustments_ManageAdjustments : MTPage
{
  protected override void OnLoadComplete(EventArgs e)
  {
    if (!String.IsNullOrEmpty(Request.QueryString["ParentSessionId"]))
    {
      string username = Request.QueryString["ParentSessionId"];

      MTGridDataElement el = MTFilterGrid1.FindElementByID("ParentSessionId");
      if (el != null)
      {
        el.ElementValue = username;
        MTFilterGrid1.SearchOnLoad = true;
      }
    }
    else if (!String.IsNullOrEmpty(Request.QueryString["SessionId"]))
    {
      string username = Request.QueryString["SessionId"];

      MTGridDataElement el = MTFilterGrid1.FindElementByID("SessionId");
      if (el != null)
      {
        el.ElementValue = username;
        MTFilterGrid1.SearchOnLoad = true;
      }
    }
//TODO uncomment this when defect about datebox will be fixed -  ESR-7253
//var createDate = MTFilterGrid1.FindElementByID("AdjustmentCreationDate"); 
//if (createDate != null) 
//{ 
//  createDate.ElementValue = DateTime.Today.AddDays(-1).ToString(CultureInfo.CurrentUICulture); 
//  createDate.ElementValue2 = DateTime.Today.ToString(CultureInfo.CurrentUICulture); 
//}  
    base.OnLoadComplete(e);
  }

}