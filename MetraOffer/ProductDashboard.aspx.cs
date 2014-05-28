using System;
using System.Collections.Generic;
using MetraTech.UI.Common;
using MetraTech.PageNav.ClientProxies;
using MetraTech.DomainModel.BaseTypes;
using MetraTech.UI.Controls;
using MetraTech.OnlineBill;
using MetraTech.ActivityServices.Common;
using MetraTech.DataAccess;
using System.Web.UI.WebControls;

public partial class ProductDashboard : MTPage
{

 
  protected void Page_Load(object sender, EventArgs e)
  {
    if (!IsPostBack)
    {
      // TODO:  Get data to bind to and place in viewstate
     
      // TODO:  Set binding properties and template on MTGenericForm control
    
       
    }
  }

  protected override void OnLoadComplete(EventArgs e)
  {

      try
      {
          loadGrids();
        }
      catch (Exception ex)
      {
          Response.Write(ex.StackTrace);
      }
      base.OnLoadComplete(e);
  }


  private void loadGrids()
  {
     Dictionary<string, object> paramDict = new Dictionary<string, object>();
     string querydir = "..\\Extensions\\SystemConfig\\config\\SqlCore\\Queries\\UI\\Dashboard";


     ConfigureAndLoadGrid(grdRecentOfferingChanges, "__GET_RECENT_OFFERING_CHANGES__", querydir, null);
     /*ConfigureAndLoadGrid(grdRecentRateChanges, "__GET_RECENT_RATE_CHANGES__", querydir, null);
     ConfigureAndLoadGrid(grdMyRecentChanges, "__GET_MY_RECENT_CHANGES__", querydir, null);*/
  }



  private void ConfigureAndLoadGrid(MTFilterGrid grid, string queryName, string queryPath, Dictionary<string, object> paramDict)
  {
      try
      {
          SQLQueryInfo sqi = new SQLQueryInfo();
          sqi.QueryName = queryName;
          sqi.QueryDir = queryPath;
         
        if (paramDict != null)
          {
              foreach (var pair in paramDict)
              {
                  SQLQueryParam param = new SQLQueryParam();
                  param = new SQLQueryParam();
                  param.FieldName = pair.Key;
                  param.FieldValue = pair.Value;
                  sqi.Params.Add(param);
              }
          }

          string qsParam = MetraTech.UI.Common.SQLQueryInfo.Compact(sqi);
          grid.DataSourceURLParams.Add("q", qsParam);

      }
      catch
      {
          throw;
      }
  }


  

  }


