using System;
using Castle.Windsor.Installer;
using MetraTech.DomainModel.Enums.Account.Metratech_com_accountcreation;
using MetraTech.UI.Common;
using MetraTech.PageNav.ClientProxies;
using MetraTech.DomainModel.AccountTypes;
using MetraTech.ActivityServices.Common;
using MetraTech.Approvals;
using MetraTech.DomainModel.BaseTypes;
using System.Windows.Forms;
using MetraTech.Core.Services.ClientProxies;
using System.Collections.Generic;

public partial class Account_UpdateSystemAccount : MTAccountPage
{
  //Approval Framework Code Starts Here 
  public int? bAccountUpdateApprovalsEnabled
  {
    get { return ViewState["bAccountUpdateApprovalsEnabled"] as int?; }
    set { ViewState["bAccountUpdateApprovalsEnabled"] = value; }
  } //so we can read it any time in the session

  public bool bAllowMoreThanOnePendingChange { get; set; }
  public bool bAccountHasPendingChange { get; set; }
  public string strChangeType { get; set; }
  //Approval Framework Code Ends Here 

  protected void Page_Load(object sender, EventArgs e)
  {
    if (!IsPostBack)
    {
      Account = (MetraTech.DomainModel.BaseTypes.Account) PageNav.Data.Out_StateInitData["Account"];
      if (!IsPostBack)
      {
        MTGenericForm1.DataBinderInstanceName = "MTDataBinder1";
        MTGenericForm1.RenderObjectType = BillTo.GetType();
        MTGenericForm1.RenderObjectInstanceName = "BillTo";
        MTGenericForm1.TemplatePath = TemplatePath;
        MTGenericForm1.ReadOnly = false;
      }
      tbAuthenticationType.Text = BaseObject.GetDisplayName(Account.AuthenticationType);
      tbSecurityQuestionText.Visible = Account.AuthenticationType == AuthenticationType.MetraNetInternal;
      tbSecurityAnswer.Visible = Account.AuthenticationType == AuthenticationType.MetraNetInternal;
      ddSecurityQuestion.Visible = Account.AuthenticationType == AuthenticationType.MetraNetInternal;
      if (!MTDataBinder1.DataBind())
      {
        Logger.LogError(MTDataBinder1.BindingErrors.ToHtml());
      }

      //Approval Framework Code Starts Here 

      ApprovalManagementServiceClient client = new ApprovalManagementServiceClient();

      client.ClientCredentials.UserName.UserName = UI.User.UserName;
      client.ClientCredentials.UserName.Password = UI.User.SessionPassword;
      strChangeType = "AccountUpdate";
      bAccountHasPendingChange = false;
      bAccountUpdateApprovalsEnabled = 0;

      MTList<ChangeTypeConfiguration> mactc = new MTList<ChangeTypeConfiguration>();

      client.RetrieveChangeTypeConfiguration(strChangeType, ref mactc);

      if (mactc.Items[0].Enabled)
      {
        bAccountUpdateApprovalsEnabled = 1; // mactc.Items[0].Enabled; 
      }

      if (bAccountUpdateApprovalsEnabled == 1)
      {
        bAllowMoreThanOnePendingChange = mactc.Items[0].AllowMoreThanOnePendingChange;

        List<int> pendingchangeids;
        string straccountid = "";
        straccountid = UI.Subscriber.SelectedAccount._AccountID.ToString();

        client.GetPendingChangeIdsForItem(strChangeType, straccountid, out pendingchangeids);

        if (pendingchangeids.Count != 0)
        {
          bAccountHasPendingChange = true;
        }

        if (!bAllowMoreThanOnePendingChange)
        {
          if (bAccountHasPendingChange)
          {
            SetError(Resources.ErrorMessages.ERROR_MULTIPLE_ACCOUNT_UPDATE_PENDING_CHANGES);
            this.Logger.LogError(
              string.Format(
                "This account {0} already has a pending change of type Account Update. Only one pending change of this type is permitted.",
                UI.Subscriber.SelectedAccount.UserName));
            btnOK.Visible = false;
            client.Abort();
          }

        }

        if (bAccountHasPendingChange)
        {
          string approvalframeworkmanagementurl = 
            "<a href='/MetraNet/ApprovalFrameworkManagement/ShowChangesSummary.aspx?showchangestate=PENDING'</a>";
          string strPendingChangeWarning = string.Format(GetLocalResourceObject("PendingChangeWarning").ToString(), approvalframeworkmanagementurl);
          divLblMessage.Visible = true;
          lblMessage.Text = strPendingChangeWarning;
        }

      }
      //Approval Framework Code Ends Here 

    }
  }

  public override void Validate()
  {
    base.Validate();

    // SECENG: CORE-4848 Modify security questions (password hint)
    // Changed the security question/answer validation
    if (ddSecurityQuestion.SelectedIndex <= 1 && String.IsNullOrWhiteSpace(tbSecurityQuestionText.Text))
    {
      if (!String.IsNullOrWhiteSpace(tbSecurityAnswer.Text))
      {
        throw new ApplicationException(Resources.ErrorMessages.ERROR_SECURITY_ANSWER_INVALID);
      }
    }
    else
    {
      if (ddSecurityQuestion.SelectedIndex > 1 && !String.IsNullOrWhiteSpace(tbSecurityQuestionText.Text))
      {
        throw new ApplicationException(Resources.ErrorMessages.ERROR_SECURITY_QUESTION_INVALID);
      }

      if (String.IsNullOrWhiteSpace(tbSecurityAnswer.Text))
      {
        throw new ApplicationException(Resources.ErrorMessages.ERROR_SECURITY_ANSWER_INVALID);
      }
    }
  }

  protected void btnOK_Click(object sender, EventArgs e)
  {
    try
    {
      Page.Validate();
      if (Page.IsValid)
      {
        MTDataBinder1.Unbind();

        ((InternalView) Account.GetInternalView()).PriceList = null;

        UpdateAccountEvents_UpdateAccount_Client update = new UpdateAccountEvents_UpdateAccount_Client();
        update.In_Account = Account;
        update.In_AccountId = new AccountIdentifier(UI.User.AccountId);

        //Approval Framework related code starts here
        update.In_IsApprovalEnabled = false;

        if (bAccountUpdateApprovalsEnabled == 1)
        {
          update.In_IsApprovalEnabled = true;
        }
        //Approval Framework related code ends here


        PageNav.Execute(update);
      }
    }
    catch
      (Exception
        exp)
    {
      SetError(exp.Message);
    }
  }

protected void btnCancel_Click(object sender, EventArgs e)
  {
    UpdateAccountEvents_CancelUpdateAccount_Client cancel = new UpdateAccountEvents_CancelUpdateAccount_Client();
    cancel.In_AccountId = new AccountIdentifier(UI.User.AccountId);
    PageNav.Execute(cancel);
  }
  

}