 <% 
' ---------------------------------------------------------------------------------------------------------------------------------------
'  @doc $Workfile: IntervalManagement.RunHistory.List.asp$
' 
'  Copyright 1998-2003 by MetraTech Corporation
'  All rights reserved.
' 
'  THIS SOFTWARE IS PROVIDED "AS IS", AND MetraTech Corporation MAKES
'  NO REPRESENTATIONS OR WARRANTIES, EXPRESS OR IMPLIED. By way of
'  example, but not limitation, MetraTech Corporation MAKES NO
'  REPRESENTATIONS OR WARRANTIES OF MERCHANTABILITY OR FITNESS FOR ANY
'  PARTICULAR PURPOSE OR THAT THE USE OF THE LICENSED SOFTWARE OR
'  DOCUMENTATION WILL NOT INFRINGE ANY THIRD PARTY PATENTS,
'  COPYRIGHTS, TRADEMARKS OR OTHER RIGHTS.
' 
'  Title to copyright in this software and any associated
'  documentation shall at all times remain with MetraTech Corporation,
'  and USER agrees to preserve the same.
' 
'  Created by: Rudi
' 
'  $Date: 11/14/2002 12:13:29 PM$
'  $Author: Rudi Perkins$
'  $Revision: 3$
'
' - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
'
' CLASS       : 
' DESCRIPTION : 
' ---------------------------------------------------------------------------------------------------------------------------------------
Option Explicit
%>
<!-- #INCLUDE FILE="../../auth.asp" -->
<!-- #INCLUDE VIRTUAL="/mdm/mdm.asp" -->
<!-- #INCLUDE FILE="../../default/lib/momLibrary.asp"                   -->
<%
Form.Version                    = MDM_VERSION     ' Set the dialog version - we are version 2.0.
Form.ErrorHandler               = FALSE  
Form.ShowExportIcon             = TRUE
Form.Page.MaxRow                = CLng(mom_GetDictionary("PV_ROW_PER_PAGE"))
Form.Page.NoRecordUserMessage   = mom_GetDictionary("PRODUCT_VIEW_BROWSER_NO_RECORDS_FOUND")

mdm_PVBrowserMain ' invoke the mdm framework

PRIVATE FUNCTION Form_Initialize(EventArg) ' As Boolean
    'BreadCrumb.SetCrumb mom_GetDictionary("TEXT_VIEW_AUDIT_LOG")
    ProductView.Clear  ' Set all the property of the service to empty or to the default value
   	ProductView.Properties.ClearSelection
    ProductView.Properties.Flags = eMSIX_PROPERTIES_FLAG_PRODUCTVIEW

    'response.write(Service.Properties.ToString)
    'response.end
    Form("InstanceId") = CLng(request("InstanceId"))
    Form("IntervalId") = CLng(request("IntervalId"))
    Form("BillingGroupId") = CLng(request("BillingGroupId"))
    Form("Title") = request("Title")
    
    'Set the screen title
    if len(Form("Title"))>0 then
      mdm_GetDictionary().Add "ADAPTER_RUN_PAGE_TITLE", Form("Title") 
    else
      mdm_GetDictionary().Add "ADAPTER_RUN_PAGE_TITLE", "" 
    end if

    
	  Form_Initialize = true
END FUNCTION

' ---------------------------------------------------------------------------------------------------------------------------------------
' FUNCTION:  Form_LoadProductView
' PARAMETERS:  EventArg
' DESCRIPTION: 
' RETURNS:  Return TRUE if ok else FALSE
PRIVATE FUNCTION Form_LoadProductView(EventArg) ' As Boolean

  Form_LoadProductView = FALSE
  
  dim rowset, sQuery
  set rowset = server.CreateObject("MTSQLRowset.MTSQLRowset.1")
  rowset.Init "queries\mom"
 
  if Form("BillingGroupId") > 0 then
    rowset.SetQueryTag("__GET_BILLINGGROUP_ADAPER_HISTORY__")  
    rowset.AddParam "%%ID_BILLGROUP%%", CLng(Form("BillingGroupId"))
    mdm_GetDictionary().Add "ADAPTER_RUN_PAGE_TITLE", "Adapter Run History For " & "Bill Group " & Form("Title") 
  else 
	if Form("InstanceId") > 0 then
		rowset.SetQueryTag("__GET_ADAPTER_INSTANCE_HISTORY__")  
		rowset.AddParam "%%ID_INSTANCE%%", CLng(Form("InstanceId"))
		mdm_GetDictionary().Add "ADAPTER_RUN_PAGE_TITLE", "Adapter Run History For " & Form("Title") 
	else
		if Form("IntervalId") > 0 then
		rowset.SetQueryTag("__GET_INTERVAL_ADAPER_HISTORY__")  
		rowset.AddParam "%%ID_INTERVAL%%", CLng(Form("IntervalId"))
		mdm_GetDictionary().Add "ADAPTER_RUN_PAGE_TITLE", "Adapter Run History For Interval " & Form("Title") 
		else
		response.write("Instance Id or Interval Id Not Passed")
		response.end
		end if
	end if
  end if
	rowset.Execute
  
  '// Filter out everything except the requested entity type
  If false then
  If len(request("InstanceId"))>0 Then
    dim objMTFilter
    Set objMTFilter = mdm_CreateObject("MTSQLRowset.MTDataFilter")
    objMTFilter.Add "InstanceId", OPERATOR_TYPE_EQUAL, CLng(request("InstanceId"))
    set rowset.filter = objMTFilter
  End If
  end if
  
  ' Load a Rowset from a SQL Queries and build the properties collection of the product view based on the columns of the rowset
  Set ProductView.Properties.RowSet = rowset
  ProductView.Properties.AddPropertiesFromRowset rowset
  
  'ProductView.Properties.SelectAll
  dim i
  i=1
  if Form("IntervalId") > 0 then 
  ProductView.Properties.ClearSelection                       ' Select the properties I want to print in the PV Browser   Order
  ProductView.Properties("InstanceId").Selected 			      = i : i=i+1
  ProductView.Properties("Time").Selected 			      = i : i=i+1
  ProductView.Properties("Adapter").Selected 			      = i : i=i+1
  ProductView.Properties("Action").Selected 			      = i : i=i+1
  ProductView.Properties("UserName").Selected 			      = i : i=i+1


  ProductView.Properties("InstanceId").Caption 		    = "Instance" 'mom_GetDictionary("TEXT_AUDIT_TIME")
  ProductView.Properties("Time").Caption 		          = mom_GetDictionary("TEXT_AUDIT_TIME")
  ProductView.Properties("Adapter").Caption 	        = "Adapter" 'mom_GetDictionary("TEXT_AUDIT_EVENTNAME")
  ProductView.Properties("Action").Caption 	          = "Action" 'mom_GetDictionary("TEXT_AUDIT_ENTITYNAME")
  ProductView.Properties("UserName").Caption 	        = mom_GetDictionary("TEXT_AUDIT_USERNAME")
  
  mdm_SetMultiColumnFilteringMode TRUE  

  
  Set Form.Grid.FilterProperty                        = ProductView.Properties("UserName") ' Set the property on which to apply the filter  
  else
    ProductView.Properties.SelectAll
    Form.Grid.FilterMode = 0' MDM_FILTER_MODE_ON ' Filter
    'mdm_SetMultiColumnFilteringMode TRUE
  end if

  
  ProductView.Properties("Time").Sorted               = MTSORT_ORDER_DESCENDING

  ' REQUIRED because we must generate the property type info in javascript. When the user change the property which he
  ' wants to use to do a filter we use the type of the property (JAVASCRIPT code) to show 2 textbox if it is a date
  ' else one.
  ProductView.LoadJavaScriptCode
  
  ProductView.Properties.CancelLocalization
  
  Form_LoadProductView                                  = TRUE ' Must Return TRUE To Render The Dialog
  
END FUNCTION

PRIVATE FUNCTION Form_DisplayCell(EventArg) ' As Boolean

       if Form.Grid.Col<=3 then
          Form_DisplayCell = Inherited("Form_DisplayCell(EventArg)")
       else
         Select Case lcase(Form.Grid.SelectedProperty.Name)
         Case "action"
            dim strForced
            
            if ProductView.Properties.RowSet.Value("Forced")="Y" and UCASE(ProductView.Properties.RowSet.Value("type"))<>"CHECKPOINT" then
              strForced = "<br><img src='../localized/us/images/errorsmall.gif' align='absmiddle' border='0'><strong> The adapter run was forced to ignore dependencies</strong>"
            else
              strForced = ""
            end if
            
            EventArg.HTMLRendered     =  "<td class='" & Form.Grid.CellClass & "'>" & ProductView.Properties.RowSet.Value("Action") & strForced & "</td>"
            
  			    Form_DisplayCell = TRUE
         Case "time"
            EventArg.HTMLRendered     =  "<td class='" & Form.Grid.CellClass & "'>"  & mdm_Format(ProductView.Properties.RowSet.Value("time"),mom_GetDictionary("DATE_TIME_FORMAT")) & "</td>"
            Form_DisplayCell = TRUE
  	     Case else
            Form_DisplayCell = Inherited("Form_DisplayCell(EventArg)")
      End Select
     end if

    Form_DisplayCell = true

END FUNCTION

' ---------------------------------------------------------------------------------------------------------------------------------------
' FUNCTION      :  inheritedForm_DisplayEndOfPage
' PARAMETERS    :  EventArg
' DESCRIPTION   :  Override end of table to place add button
' RETURNS       :  Return TRUE if ok else FALSE
PRIVATE FUNCTION Form_DisplayEndOfPage(EventArg) ' As Boolean

    Dim strEndOfPageHTMLCode, strTmp
    
    
    strTmp = "</table><div align=center><BR><BR><button  name='REFRESH' Class='clsOkButton' onclick='window.location=window.location'>Refresh</button><button  name='CLOSE' Class='clsOkButton' onclick='window.close();'>Close</button>" & vbNewLine
    strEndOfPageHTMLCode = strEndOfPageHTMLCode & strTmp
        
    strEndOfPageHTMLCode = strEndOfPageHTMLCode & "</FORM></BODY></HTML>"
    
    ' Here we must not forget to concat rather than set because we want to keep the result of the inherited event.
    EventArg.HTMLRendered = EventArg.HTMLRendered & REPLACE(strEndOfPageHTMLCode,"[LOCALIZED_IMAGE_PATH]",mom_GetLocalizeImagePath())
    
    Form_DisplayEndOfPage = TRUE
END FUNCTION




%>
