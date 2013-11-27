﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using MetraTech.ActivityServices.Common;
using MetraTech.Debug.Diagnostics;
using MetraTech.Core.Services.ClientProxies;
using MetraTech.DomainModel.ProductCatalog;
using MetraTech.UI.Common;

public partial class NonStandardCharges_AjaxServices_ApproveNonStandardCharges : MTListServicePage
{
  protected void Page_Load(object sender, EventArgs e)
  {
    var response = new AjaxResponse();
    using (new HighResolutionTimer("ApproveNonStandardCharges", 5000))
    {
      NonStandardChargeServiceClient client = null;

      try
      {
        client = new NonStandardChargeServiceClient();

        if (client.ClientCredentials != null)
        {
          client.ClientCredentials.UserName.UserName = UI.User.UserName;
          client.ClientCredentials.UserName.Password = UI.User.SessionPassword;
        }

        client.Open();
        string ids = Request.Params["ids"];
        string[] parsedIds = ids.Split(new char[] { ',' });


        List<long> sessionIds = new List<long>();
        foreach (string s in parsedIds)
        {
          long item = System.Convert.ToInt64(s);
          sessionIds.Add(item);
        }
        client.ApproveNonStandardCharges(sessionIds);
        response.Success = true;
        response.Message = "Successfully approved nonstandard charges.";
        client.Close();
        client = null;
      }
      catch (FaultException<MASBasicFaultDetail> ex)
      {
        response.Success = false;
        response.Message = ex.Detail.ErrorMessages[0];
        Logger.LogError(ex.Detail.ErrorMessages[0]);
      }
      catch (Exception ex)
      {
        response.Success = false;
        response.Message = ex.Message;
        Logger.LogException("An unknown exception occurred.  Please check system logs.", ex);
        throw;
      }
      finally
      {
        if (client != null)
        {
          client.Abort();
        }
        Response.Write(response.ToJson());
        Response.End();
      }
    }
  }
}