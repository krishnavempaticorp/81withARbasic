﻿using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using MetraTech.DomainModel.Billing;
using MetraTech.UI.Common;
using System.ServiceModel;
using MetraTech.Debug.Diagnostics;
using System.Web.Script.Serialization;
using MetraTech.ActivityServices.Common;
using System.Collections.Generic;
using MetraTech.DataAccess;
using System.Text;



public partial class AjaxServices_QueryService : MTListServicePage
{
  private const int MAX_RECORDS_PER_BATCH = 50;
  protected SQLQueryInfo QueryInfo;
  protected List<int> exportColumns = new List<int>();

  protected bool ExtractDataInternal(ref MTList<SQLRecord> items, int batchID, int limit)
  {
    try
    {
      items.Items.Clear();

      items.PageSize = limit;
      items.CurrentPage = batchID;

      // open the connection
      using (IMTConnection conn = ConnectionManager.CreateConnection())
      {
        // Get a filter/sort statement
        using (IMTFilterSortStatement stmt = conn.CreateFilterSortStatement(QueryInfo.QueryDir, QueryInfo.QueryName))
        {

          // Set the parameters
          foreach (SQLQueryParam param in QueryInfo.Params)
          {
              // ESR-6678 Pass in the values for parameters supported by MetraView
              if (param.FieldName == "%%ACCOUNT_ID%%")
              {
                  if (UI.Subscriber.SelectedAccount._AccountID != null)
                  {
                      stmt.AddParam(param.FieldName, UI.Subscriber.SelectedAccount._AccountID.ToString());
                  }
                  else
                  {
                      throw new MASBasicException("Could not get _AccountID for logged in MetraView account to pass to " +
                          QueryInfo.QueryName + " query for %%ACCOUNT_ID%% parameter.");
                  }
              }
              else if (param.FieldName == "%%PAYER_ID%%")
              {
                  if (UI.Subscriber.SelectedAccount.PayerID != null)
                  {
                      stmt.AddParam(param.FieldName, UI.Subscriber.SelectedAccount.PayerID.ToString());
                  }
                  else
                  {
                      throw new MASBasicException("Could not get PayerID for logged in MetraView account to pass to " +
                          QueryInfo.QueryName + " query for %%PAYER_ID%% parameter.");
                  }
              }
              else if (param.FieldName == "%%INTERVAL_ID%%")
              {
                  var billManager = new BillManager(UI);
                  Interval interval = billManager.GetCurrentInterval();
                  if (interval != null)
                  {
                      stmt.AddParam(param.FieldName, interval.ID.ToString());
                  }
                  else
                  {
                      throw new MASBasicException("Could not get current interval ID for logged in MetraView account to pass to " +
                          QueryInfo.QueryName + " query for %%INTERVAL_ID%% parameter.");
                  }
              }
              else
                  stmt.AddParam(param.FieldName, param.FieldValue);
          }

          #region Apply Sorting
          //add sorting info if applies
          if (items.SortCriteria != null && items.SortCriteria.Count > 0)
          {
            foreach (MetraTech.ActivityServices.Common.SortCriteria sc in items.SortCriteria)
            {
              stmt.SortCriteria.Add(
                  new MetraTech.DataAccess.SortCriteria(
                      sc.SortProperty,
                      ((sc.SortDirection == SortType.Ascending) ? MetraTech.DataAccess.SortDirection.Ascending : MetraTech.DataAccess.SortDirection.Descending)));
            }
          }

          #endregion

          #region Apply Filters
          //apply filters
          foreach (MTBaseFilterElement filterElement in items.Filters)
          {
            BaseFilterElement fe = ConvertMTFilterElement(filterElement);

            stmt.AddFilter(fe);
          }
          #endregion

          #region Apply Pagination
          //set paging info
          stmt.CurrentPage = items.CurrentPage;
          stmt.PageSize = items.PageSize;
          #endregion

          // execute the query
          using (IMTDataReader rdr = stmt.ExecuteReader())
          {

            // process the results
            while (rdr.Read())
            {
              SQLRecord record = new SQLRecord();

              for (int i = 0; i < rdr.FieldCount; i++)
              {
                SQLField field = new SQLField();
                field.FieldDataType = rdr.GetType(i);
                field.FieldName = rdr.GetName(i);

                if (!rdr.IsDBNull(i))
                {
                  field.FieldValue = rdr.GetValue(i);
                }

                record.Fields.Add(field);
              }

              items.Items.Add(record);
            }

            // get the total rows that would be returned without paging
            items.TotalRows = stmt.TotalRows;
          }
        }
      }
    }

    catch (Exception ex)
    {
      Response.StatusCode = 500;
      Logger.LogError(ex.Message);
      Response.End();
      return false;
    }

    return true;
  }

  private BaseFilterElement ConvertMTFilterElement(MTBaseFilterElement filterElement)
  {
    BaseFilterElement bfe = null;

    if (filterElement.GetType() == typeof(MTBinaryFilterOperator))
    {
      MTBinaryFilterOperator bfo = filterElement as MTBinaryFilterOperator;

      bfe = new BinaryFilterElement(
                  ConvertMTFilterElement(bfo.LeftHandElement),
                  (BinaryFilterElement.BinaryOperatorType)((int)bfo.OperatorType),
                  ConvertMTFilterElement(bfo.RightHandElement));
    }
    else if (filterElement.GetType() == typeof(MTFilterElement))
    {
      MTFilterElement fe = filterElement as MTFilterElement;
      object filterValue = fe.Value;

      bfe = new FilterElement(fe.PropertyName.Replace('.', '_'),
        (FilterElement.OperationType)((int)fe.Operation),
        filterValue);
    }
    else
    {
      throw new MASBasicException("Unexpected MTBaseFilterElement type");
    }

    return bfe;
  }

  protected bool ExtractData(ref MTList<SQLRecord> items)
  {
    if (Page.Request["mode"] == "csv")
    {
      Response.BufferOutput = false;
      Response.ContentType = "application/csv";
      Response.AddHeader("Content-Disposition", "attachment; filename=export.csv");
      Response.BinaryWrite(BOM);
    }

    //if there are more records to process than we can process at once, we need to break up into multiple batches
    if ((items.PageSize > MAX_RECORDS_PER_BATCH) && ((Page.Request["mode"] == "csv") || (Page.Request["mode"] == "SelectAll")))
    {
      int advancePage = (items.PageSize % MAX_RECORDS_PER_BATCH != 0) ? 1 : 0;

      var sb = new StringBuilder();
      int numBatches = advancePage + (items.PageSize / MAX_RECORDS_PER_BATCH);
      for (int batchID = 0; batchID < numBatches; batchID++)
      {
        ExtractDataInternal(ref items, batchID + 1, MAX_RECORDS_PER_BATCH);
        if (Page.Request["mode"] == "csv")
        {
          string strCSV = ConvertObjectToCSV(items, (batchID == 0));
          Response.Write(strCSV);
        }

        // select all
        if (Page.Request["mode"] == "SelectAll")
        {
          string column = Request["idNode"];
          foreach (var r in items.Items)
          {
            foreach (var c in r.Fields)
            {
              if (c.FieldName.Equals(column))
              {
                sb.Append(c.FieldValue);
                continue;
              }
            }
            sb.Append(",");
          }
        }
      }

      if (Page.Request["mode"] == "SelectAll")
      {
        Session["SelectedIDs"] = sb.ToString().Trim(new[] { ',' });
        var result = new AjaxResponse();
        result.Success = true;
        result.Message = String.Format("Selected {0} items.", items.TotalRows);
        Response.Write(result.ToJson());
        Response.End();
      }
    }
    else
    {
      ExtractDataInternal(ref items, items.CurrentPage, items.PageSize);
      if (Page.Request["mode"] == "csv")
      {
        string strCSV = ConvertObjectToCSV(items, true);
        Response.Write(strCSV);
      }

      // select all
      if (Page.Request["mode"] == "SelectAll")
      {
        var sb = new StringBuilder();
        string column = Request["idNode"];
        foreach (var r in items.Items)
        {
          foreach (var c in r.Fields)
          {
            if (c.FieldName.Equals(column))
            {
              sb.Append(c.FieldValue);
              continue;
            }
          }
          sb.Append(",");
        }
        Session["SelectedIDs"] = sb.ToString().Trim(new[] { ',' });
        var result = new AjaxResponse();
        result.Success = true;
        result.Message = String.Format("Selected {0} items.", items.TotalRows);
        Response.Write(result.ToJson());
        Response.End();
      }
    }

    return true;
  }

  protected void Page_Load(object sender, EventArgs e)
  {
    //parse query name
    String qsQuery = Request["urlparam_q"];
    if (string.IsNullOrEmpty(qsQuery))
    {
      Logger.LogWarning("No query specified");
      Response.Write("{\"TotalRows\":\"0\",\"Items\":[]}");
      Response.End();
      return;
    }

    //populate the object
    try
    {
      QueryInfo = SQLQueryInfo.Extract(qsQuery);
    }
    catch
    {
      Logger.LogWarning("Unable to parse query parameters " + qsQuery);
      Response.Write("{\"TotalRows\":\"0\",\"Items\":[]}");
      Response.End();
      return;
    }
    if (QueryInfo == null)
    {
      Logger.LogWarning("Unable to load query parameters");
      Response.Write("{\"TotalRows\":\"0\",\"Items\":[]}");
      Response.End();
      return;
    }

    using (new HighResolutionTimer("QueryService", 5000))
    {

      MTList<SQLRecord> items = new MTList<SQLRecord>();

      SetPaging(items);
      SetSorting(items);
      SetFilters(items);

      //unable to extract data
      if (!ExtractData(ref items))
      {
        return;
      }

      if (items.Items.Count == 0)
      {
        Response.Write("{\"TotalRows\":\"0\",\"Items\":[]}");
        Response.End();
        return;
      }

      if ((Page.Request["mode"] != "csv") || (Page.Request["mode"] != "SelectAll"))
      {
        //convert paymentMethods into JSON
        string json = SerializeItems(items);

        Response.Write(json);
      }

      Response.End();
    }

  }

  //populate an array of columns, that are indices into the Fields collection
  protected void PopulateColumnList(MTList<SQLRecord> mtList)
  {

    int columnIndex = 0;
    while (!String.IsNullOrEmpty(Request["column[" + columnIndex.ToString() + "][columnID]"]))
    {
      //column name that we need to look up in the fields collection
      string columnID = Request["column[" + columnIndex.ToString() + "][columnID]"];

      if (mtList.Items.Count > 0)
      {
        SQLRecord curRecord = mtList.Items[0];
        for (int i = 0; i < curRecord.Fields.Count; i++)
        {
          SQLField curField = curRecord.Fields[i];

          if (curField.FieldName.ToLower() == columnID.ToLower() && !exportColumns.Contains(i))
          {
            exportColumns.Add(i);
            break;
          }
        }
      }

      columnIndex++;
    }

  }

  protected string ConvertObjectToCSV(MTList<SQLRecord> mtList, bool bIncludeHeaders)
  {
    var sb = new StringBuilder();
    //add headers if necessary
    if (bIncludeHeaders)
    {
      sb.AppendLine("sep=,");
      sb.Append(base.GenerateCSVHeader());
      if (sb.ToString().Length > 0)
      {
        sb.Append("\n");
      }
    }

    PopulateColumnList(mtList);

    //iterate through the list of items
    foreach (SQLRecord curRecord in mtList.Items)
    {
      string curRowCSV = ConvertRowToCSV(curRecord);
      sb.Append(curRowCSV);
      sb.Append("\n");
    }

    return sb.ToString();
  }

  protected new string GenerateCSVHeader()
  {
    StringBuilder sb = new StringBuilder();

    //iterate through the fields that we want to extract
    int columnIndex = 0;
    while (!String.IsNullOrEmpty(Request["column[" + columnIndex.ToString() + "][columnID]"]))
    {
      string headerText = Request["column[" + columnIndex.ToString() + "][headerText]"];
      string columnID = Request["column[" + columnIndex.ToString() + "][columnID]"];
      if (columnIndex > 0)
      {
        sb.Append(",");
      }

      sb.Append("\"");
      if (headerText != null)
      {
        sb.Append(headerText.ToString().Replace("\"", "\"\""));
      }
      else
      {
        sb.Append(columnID.ToString().Replace("\"", "\"\""));
      }
      sb.Append("\"");

      columnIndex++;
    }

    return sb.ToString();
  }

  protected string ConvertRowToCSV(SQLRecord curRecord)
  {
    if (curRecord == null)
    {
      return string.Empty;
    }
    StringBuilder sb = new StringBuilder();

    //iterate through columns
    for (int i = 0; i < exportColumns.Count; i++)
    {
      if (i > 0)
      {
        sb.Append(",");
      }

      sb.Append("\"");

      int realFieldID = exportColumns[i];
      if (curRecord.Fields[realFieldID].FieldValue != null)
      {
        string renderer = Request["column[" + i + "][renderer]"];
        if (curRecord.Fields[realFieldID].FieldDataType == typeof (DateTime) && !String.IsNullOrEmpty(renderer))
        {
          if (renderer.Equals("shortdatestring", StringComparison.Ordinal))
          {
            try
            {
              DateTime dt = (DateTime) curRecord.Fields[realFieldID].FieldValue;
              sb.Append(dt.ToShortDateString().Replace("\"", "\"\""));
            }
            catch
            {
              sb.Append(curRecord.Fields[realFieldID].FieldValue.ToString().Replace("\"", "\"\""));
            }
          }
          else
          {
            sb.Append(curRecord.Fields[realFieldID].FieldValue.ToString().Replace("\"", "\"\""));
          }
        }
        else
        {
          sb.Append(curRecord.Fields[realFieldID].FieldValue.ToString().Replace("\"", "\"\""));
        }
      }
      sb.Append("\"");
    }
    return sb.ToString();
  }

  protected string SerializeItems(MTList<SQLRecord> items)
  {
    StringBuilder json = new StringBuilder();

    json.Append("{\"TotalRows\":");
    json.Append(items.TotalRows.ToString());
    json.Append(", \"Items\":[");

    for (int i = 0; i < items.Items.Count; i++)
    {
      SQLRecord record = items.Items[i];

      if (i > 0)
      {
        json.Append(",");
      }

      json.Append("{");

      //iterate through fields
      for (int j = 0; j < record.Fields.Count; j++)
      {
        SQLField field = record.Fields[j];
        if (j > 0)
        {
          json.Append(",");
        }

        json.Append("\"");
        json.Append(field.FieldName);
        json.Append("\":");

        if (field.FieldValue == null)
        {
          json.Append("null");
        }
        else
        {

          if (typeof(String) == field.FieldDataType || typeof(DateTime) == field.FieldDataType || typeof(Guid) == field.FieldDataType)
          {
            json.Append("\"");
          }

          json.Append(field.FieldValue.ToString());

          if (typeof(String) == field.FieldDataType || typeof(DateTime) == field.FieldDataType || typeof(Guid) == field.FieldDataType)
          {
            json.Append("\"");
          }

        }
      }

      json.Append("}");
    }

    json.Append("]");
    json.Append(", \"CurrentPage\":");
    json.Append(items.CurrentPage.ToString());
    json.Append(", \"PageSize\":");
    json.Append(items.PageSize.ToString());
    // json.Append(", \"Filters\":");
    // json.Append(jss.Serialize(items.Filters));
    json.Append(", \"SortProperty\":");
    if (items.SortCriteria == null || items.SortCriteria.Count == 0)
    {
      json.Append("null");
      json.Append(", \"SortDirection\":\"");
      json.Append(SortType.Ascending.ToString());
    }
    else
    {
      json.Append("\"");
      json.Append(items.SortCriteria[0].SortProperty);
      json.Append("\"");
      json.Append(", \"SortDirection\":\"");
      json.Append(items.SortCriteria[0].SortDirection.ToString());

    }
    json.Append("\"}");

    return json.ToString();
  }
}
