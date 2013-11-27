﻿<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/AmpWizardPageExt.master"
    AutoEventWireup="true" CodeFile="DecisionRange.aspx.cs" Inherits="AmpDecisionRangePage" Culture="auto" UICulture="auto" %>
<%@ Register Assembly="MetraTech.UI.Controls" Namespace="MetraTech.UI.Controls" TagPrefix="MT" %>
<%@ Register src="~/UserControls/AmpTextboxOrDropdown.ascx" tagName="AmpTextboxOrDropdown" tagPrefix="ampc1" %>
<%@ Register src="~/UserControls/AmpTextboxOrDropdown.ascx" tagName="AmpTextboxOrDropdown" tagPrefix="ampc2" %>

<asp:Content ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="CaptionBar">
        <asp:Label ID="lblTitleDecisionRange" runat="server" Text="Decision Range" meta:resourcekey="lblTitleResource1"></asp:Label>
    </div>
    <div>
        <table style="width: 100%">
            <tr>
                <td style="width: 6%; vertical-align: top; padding-top:10px" align="center">
                    <asp:Image ID="ImageDecisionRange" runat="server" ImageUrl="/Res/Images/icons/length.png"
                        Height="25px" Width="45px" />
                </td>
                <td valign="top" style="width: 90%">
                    <div style="line-height: 20px; padding-top: 10px; padding-left: 10px;">
                        <asp:Label ID="lblDecisionRange" meta:resourcekey="lblDecisionRange" runat="server"
                            Font-Bold="False" ForeColor="DarkBlue" Font-Size="9pt" Text="The aggregate value for the Decision Type has a range of values within which the Decision Type is applicable." />
                    </div>
                    <div style="padding-top: 5px; padding-left: 10px;">
                        <span style="color: blue; text-decoration: underline; cursor: pointer" onclick="displayInfoMultiple(TITLE_AMPWIZARD_MORE_INFO, TEXT_AMPWIZARD_MORE_DECISION_RANGE, 450, 70)"
                            id="DecisionRangeMoreLink">
                            <asp:Literal ID="MoreInfoLiteral" runat="server" Text="<%$ Resources:AmpWizard,TEXT_MORE %>" />
                        </span>
                    </div>
                    <table>
                      <tr>
                         <td style="padding-left: 100px">
                           <asp:Label ID="lblStartOfRange" meta:resourcekey="lblStartOfRange" runat="server"
                            Font-Bold="False" ForeColor="DarkBlue" Font-Size="9pt"
                            Text="Start of range:" />
                         </td>
                         <td>
                           <ampc1:AmpTextboxOrDropdown ID="startRange" runat="server" TextboxIsNumeric="true"></ampc1:AmpTextboxOrDropdown>
                         </td>
                      </tr>
                      <tr>
                        <td style="padding-left: 100px">
                          <asp:Label ID="lblEndOfRange" meta:resourcekey="lblEndOfRange" runat="server" 
                            Font-Bold="False" ForeColor="DarkBlue" Font-Size="9pt" 
                            Text="End of range:" />
                        </td>
                        <td>
                          <ampc2:AmpTextboxOrDropdown ID="endRange" runat="server" TextboxIsNumeric="true"></ampc2:AmpTextboxOrDropdown>
                        </td>
                      </tr>
                    </table>
                    <table>
                        <tr>
                            <td style="padding-left: 100px; width: 150px; padding-top: 15px">
                                <asp:Label ID="lblDecisionRangeRestart" meta:resourcekey="lblDecisionRangeRestart"
                                    runat="server" Font-Bold="False" ForeColor="DarkBlue" Font-Size="10pt" Text="Restart the count once the end of the range has been reached?"  />
                                <span style="color: blue; text-decoration: underline; cursor: pointer" onclick="displayInfoMultiple(TITLE_AMPWIZARD_HELP_RESTART_RANGE, TEXT_AMPWIZARD_HELP_DECISION_RANGE, 450, 70)">
                                    <img id="ImageHelp" src='/Res/Images/icons/help.png' />
                                </span>
                            </td>
                            <td style="padding-top:10px">
                                <asp:RadioButtonList runat="server" ID="RBL_DecisionRangeRestart" 
                                  CellSpacing="2">
                                    <asp:ListItem Value="Yes" meta:resourcekey="rblRangeYes"></asp:ListItem>
                                    <asp:ListItem Value="No" meta:resourcekey="rblRangeNo"></asp:ListItem>
                                </asp:RadioButtonList>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td style="width: 6%; vertical-align: top; padding-top: 10px" align="center">
                    <asp:Image ID="ImageDecisionRangeProration" runat="server" ImageUrl="/Res/Images/icons/pie_chart.png"
                        Height="30px" Width="30px" />
                </td>
                <td valign="top" style="width:90%">
                    <div style="line-height: 20px; padding-top: 10px; padding-left: 10px;">
                        <asp:Label ID="lblDecisionRangeProration" meta:resourcekey="lblDecisionRangeProration" runat="server"
                            Font-Bold="False" ForeColor="DarkBlue" Font-Size="9pt" Text="Proration: <br/> You can prorate the range based on subscription activation and/or termination." />
                    </div>
                    <div style="padding-top: 5px; padding-left: 10px;">
                        <span style="color: blue; text-decoration: underline; cursor: pointer" onclick="displayInfoMultiple(TITLE_AMPWIZARD_MORE_INFO, TEXT_AMPWIZARD_MORE_DECISION_RANGE_PRORATION, 450, 190)"
                            id="Span1">
                            <asp:Literal ID="ltrMoreInfo" runat="server" Text="<%$ Resources:AmpWizard,TEXT_MORE %>" />
                        </span>
                    </div>
                    <table>
                        <tr>
                            <td style="padding-left: 100px; width: 150px">
                                <asp:Label ID="lblProrateStart" meta:resourcekey="lblProrateStart" runat="server"
                                    Font-Bold="False" ForeColor="DarkBlue" Font-Size="9pt" Text="Prorate the range on activation?" />
                            </td>
                            <td>
                                <asp:RadioButtonList runat="server" ID="RBL_ProrateRangeStart" CellSpacing="2">
                                    <asp:ListItem Value="Yes" meta:resourcekey="rblRangeYes"></asp:ListItem>
                                    <asp:ListItem Value="No" meta:resourcekey="rblRangeNo"></asp:ListItem>
                                </asp:RadioButtonList>
                            </td>
                        </tr>
                        <tr>
                            <td style="padding-left: 100px; width: 150px; padding-top: 10px">
                                <asp:Label ID="lblProrateEnd" meta:resourcekey="lblProrateEnd" runat="server" Font-Bold="False"
                                    ForeColor="DarkBlue" Font-Size="9pt" Text="Prorate the range on termination?" />
                            </td>
                            <td style="padding-top: 10px">
                                <asp:RadioButtonList runat="server" ID="RBL_ProrateRangeEnd" CellSpacing="2">
                                    <asp:ListItem Value="Yes" meta:resourcekey="rblRangeYes"></asp:ListItem>
                                    <asp:ListItem Value="No" meta:resourcekey="rblRangeNo"></asp:ListItem>
                                </asp:RadioButtonList>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </div>

    <div style="padding-left: 0.85in; padding-top: 0.2in;">
        <table>
            <col style="width: 190px" />
            <col style="width: 190px" />
            <tr>
                <td align="left">
                    <MT:MTButton ID="btnBack" runat="server" Text="<%$Resources:Resource,TEXT_BACK%>"
                        OnClientClick="setLocationHref(ampPreviousPage); return false;" CausesValidation="false"
                        TabIndex="230" />
                </td>
                <td align="right">
                    <MT:MTButton ID="btnSaveAndContinue" runat="server" OnClientClick="if (ValidateForm()) { MPC_setNeedToConfirm(false); } else { MPC_setNeedToConfirm(true); return false; }"
                        OnClick="btnContinue_Click" CausesValidation="true" TabIndex="240" />                 
                </td>
            </tr>
        </table>
    </div>

    <script type="text/javascript" language="javascript">
      
      function ChangeControlState(textBoxControl, dropDownControl, disabledTextBox) {
        var txb = Ext.getCmp(textBoxControl);
        var cmb = Ext.getCmp(dropDownControl);

        if (disabledTextBox) {
          cmb.enable();
          txb.disable();
          txb.setValue('');
        }
        else {
          cmb.disable();
          cmb.setValue('');
          txb.enable();
        }

      }

      Ext.onReady(function () {
        // Record the initial values of the page's controls.
        // (Note:  This is called here, and not on the master page,
        // because the call to document.getElementById() returns null
        // if executed on the master page.)
        MPC_assignInitialValues();
      });

    </script>
</asp:Content>
