using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EWSEditor.Forms.Controls;
using EWSEditor.Logging;
using EWSEditor.Settings;
using Microsoft.Exchange.WebServices.Autodiscover;
using Microsoft.Exchange.WebServices.Data;
using EWSEditor.Common;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace EWSEditor.Forms
{
    public partial class AutodiscoverViewerForm : BaseForm
    {
        private EnumComboBox<ExchangeVersion> exchangeVersionCombo = new EnumComboBox<ExchangeVersion>();

        public new static void Show()
        {
            AutodiscoverViewerForm dialog = new AutodiscoverViewerForm();
            ((Form)dialog).Show();
        }

        private AutodiscoverViewerForm()
        {
            InitializeComponent();
        }

        private void AutodiscoverViewerForm_Load(object sender, EventArgs e)
        {
            this.exchangeVersionCombo.TransformComboBox(this.TempExchangeVersionCombo);
            this.exchangeVersionCombo.HasEmptyItem = true;
            this.exchangeVersionCombo.Text = "Exchange2013";
            //this.TempExchangeVersionCombo.Text = "Exchange2007_SP1";

            AutodiscoverService oTempService = new AutodiscoverService();

            cmboUserAgent.Items.Clear();
            cmboUserAgent.Items.Add(oTempService.UserAgent);
            UserAgentHelper.AddUserAgentsToComboBox(ref cmboUserAgent);
            cmboUserAgent.Text = oTempService.UserAgent;

            rdoUseAutoDiscover.Checked = true;
            rdoUseUserSpecifiedUrl.Checked = false;
            txtAutodiscoverServiceURL.Enabled = false;

            SetFields();

            SetEnablementOptionalHeaders();
        }

        private void SetEnablementOptionalHeaders()
        {
            txtHeader1Name.Enabled = chkOptHeader1.Checked;
            txtHeader1Value.Enabled = chkOptHeader1.Checked;

            txtHeader2Name.Enabled = chkOptHeader2.Checked;
            txtHeader2Value.Enabled = chkOptHeader2.Checked;

            txtHeader3Name.Enabled = chkOptHeader3.Checked;
            txtHeader3Value.Enabled = chkOptHeader3.Checked;

        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                txtResults.Text = string.Empty;
                txtResults.Update();


                // Create the AutodiscoverService object and set the request
                // ExchangeVersion if one was selected
                AutodiscoverService service = null;
                Uri oURI = null;

                if (this.rdoUseAutoDiscover.Checked == true)
                {
                    if (this.exchangeVersionCombo.SelectedItem.HasValue)
                    {
                        service = new AutodiscoverService(this.exchangeVersionCombo.SelectedItem.Value);
                    }
                    else
                    {
                        service = new AutodiscoverService();
                    }
                }
                else
                {
                    oURI = new Uri(this.txtAutodiscoverServiceURL.Text);
                    if (this.exchangeVersionCombo.SelectedItem.HasValue)
                    {
                        service = new AutodiscoverService(oURI, this.exchangeVersionCombo.SelectedItem.Value);
                    }
                    else
                    {
                        service = new AutodiscoverService(oURI);
                    }
                }
                 
                // Set the AutodiscoverService credentials
                service.UseDefaultCredentials = this.chkDefaultWindowsCredentials.Checked;
                if (!service.UseDefaultCredentials)
                {
                    service.Credentials = new WebCredentials(txtUser.Text.Trim(), txtPassword.Text.Trim(), txtDomain.Text.Trim());
                }

                // Enable/Disable the SCP lookup against Active Directory
                service.EnableScpLookup = this.chkEnableScpLookup.Checked;

                service.ReturnClientRequestId = true;  // This will give us more data back about the servers used in the response headers

                // Enable/Disable pre-authenticating requests
                service.PreAuthenticate = this.chkPreAuthenticate.Checked;

                if (cmboUserAgent.Text.Trim().Length != 0)
                    service.UserAgent = cmboUserAgent.Text.Trim();

                if (chkOptHeader1.Checked == true)
                    service.HttpHeaders.Add(txtHeader1Name.Text, txtHeader1Value.Text);
                if (chkOptHeader2.Checked == true)
                    service.HttpHeaders.Add(txtHeader2Name.Text, txtHeader2Value.Text);
                if (chkOptHeader3.Checked == true)
                    service.HttpHeaders.Add(txtHeader3Name.Text, txtHeader3Value.Text);


                // Create and set the trace listener
                service.TraceEnabled = true;
                //service.TraceEnablePrettyPrinting = true;  // Hmmm not implemented in the 2.2 version of the manage api for the autodiscover object - it is for service
                service.TraceListener = new EwsTraceListener();

                System.Net.WebProxy oWebProxy = null;
                if (this.rdoSpecifyProxySettings.Checked == true)
                {
                    oWebProxy = new System.Net.WebProxy(this.txtProxyServerName.Text.Trim(), Convert.ToInt32(this.txtProxyServerPort.Text.Trim()));

                    service.WebProxy = oWebProxy;
                }

                // Allow/Disallow following 302 redirects in the Autodiscover sequence
                service.RedirectionUrlValidationCallback = ValidationCallbackHelper.RedirectionUrlValidationCallback;

                AutodiscoverGetUserSettings(service, this.TargetMailboxesText.Lines);

                //GetUserSettingsResponse response = service.GetUserSettings(this.TargetMailboxText.Text, System.Enum.GetValues(typeof(UserSettingName)) as UserSettingName[]);
                //ErrorDialog.ShowInfo("Autodiscover completed successfully!  Check the EWSEditor Log Viewer for detailed output.");

                 
                //ErrorDialog.ShowInfo(sResponse);
                this.Cursor = Cursors.Default;
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        public void AutodiscoverGetUserSettings(AutodiscoverService service, string[] sUserSmtpAddresses)
        { 
            string sRet = string.Empty;
            txtResults.Text = string.Empty;

            try
            {
                GetUserSettingsResponseCollection response = service.GetUsersSettings(
                    sUserSmtpAddresses.Select(addr => addr.Trim()).Where(addr => addr != ""),
                    System.Enum.GetValues(typeof(UserSettingName)) as UserSettingName[]);
                
               
                if (response.ErrorCode == AutodiscoverErrorCode.NoError)
                {
                    AutodiscoverResultForm.Show(this, response.Select(r => new AutodiscoverResultForm.AutodiscoverResult(r, service.Url)));
                }
                else
                {
                    sRet += "Response Error:\r\n\r\n";
                    sRet += "    AutodiscoverErrorCode : " + response.ErrorCode.ToString() + "\r\n";
                    sRet += "    Error Message:          " + response.ErrorMessage + "\r\n";
                }
            }
            catch (AutodiscoverLocalException oAutodiscoverLocalException)
            {
                sRet += "Caught AutodiscoverLocalException Exception:\r\n\r\n";
                sRet += "    Error Message: " + oAutodiscoverLocalException.Message + "\r\n";
                sRet += "    Inner Error Message: " + oAutodiscoverLocalException.InnerException + "\r\n";
                sRet += "    Stack Trace: " + oAutodiscoverLocalException.StackTrace + "\r\n";
                sRet += "    See: " + oAutodiscoverLocalException.HelpLink + "\r\n";
            }

            catch (AutodiscoverRemoteException oAutodiscoverRemoteException)
            {
                sRet += "Caught AutodiscoverRemoteException Exception:\r\n\r\n";
                sRet += "    Error Message: " + oAutodiscoverRemoteException.Message + "\r\n";
                sRet += "    Inner Error Message: " + oAutodiscoverRemoteException.InnerException + "\r\n";
                sRet += "    Stack Trace: " + oAutodiscoverRemoteException.StackTrace + "\r\n";
                sRet += "    See: " + oAutodiscoverRemoteException.HelpLink + "\r\n";
            }
            catch (AutodiscoverResponseException oAutodiscoverResponseException)
            {
                sRet += "Caught AutodiscoverResponseException Exception:\r\n\r\n";
                sRet += "    Error Message: " + oAutodiscoverResponseException.Message + "\r\n";
                sRet += "    Inner Error Message: " + oAutodiscoverResponseException.InnerException + "\r\n";
                sRet += "    Stack Trace: " + oAutodiscoverResponseException.StackTrace + "\r\n";
                sRet += "    See: " + oAutodiscoverResponseException.HelpLink + "\r\n";
            }
            catch (ServerBusyException srBusyException)  // 2013+
            {
                Console.WriteLine(srBusyException);
                sRet += "Caught ServerBusyException Exception:\r\n\r\n";
                sRet += "    BackOffMilliseconds: " + srBusyException.BackOffMilliseconds.ToString() + "\r\n";
                sRet += "    Error Message: " + srBusyException.Message + "\r\n";
                sRet += "    Inner Error Message: " + srBusyException.InnerException + "\r\n";
                sRet += "    Stack Trace: " + srBusyException.StackTrace + "\r\n";
                sRet += "    See: " + srBusyException.HelpLink + "\r\n";
            }
            catch (Exception ex)
            {
                sRet += "Caught Exception:\r\n\r\n";
                sRet += "    Error Message: " + ex.Message + "\r\n";
                sRet += "    Inner Error Message: " + ex.InnerException + "\r\n";
                sRet += "    Stack Trace: " + ex.StackTrace + "\r\n";
                sRet += "    See: " + ex.HelpLink + "\r\n";
            }

            txtResults.Text = sRet;
        }

        private void chkDefaultWindowsCredentials_CheckedChanged(object sender, EventArgs e)
        {
            SetFields();  
        }

        private void SetFields()
        {
            if (chkDefaultWindowsCredentials.Checked == true)
            {
                txtUser.Enabled = false;
                txtPassword.Enabled = false;
                txtDomain.Enabled = false;
            }
            else
            {
                txtUser.Enabled = true;
                txtPassword.Enabled = true;
                txtDomain.Enabled = true;
            }

            
            this.cmboUserAgent.Enabled = chkOverrideUserAgent.Checked;

             
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void chkOverrideUserAgent_CheckedChanged(object sender, EventArgs e)
        {
            this.cmboUserAgent.Enabled = chkOverrideUserAgent.Checked;
        }

        private void btnDefaultSmtp_Click(object sender, EventArgs e)
        {
            TargetMailboxesText.Text = UserPrincipal.Current.EmailAddress;
        }

        private void btnDefaultUser_Click(object sender, EventArgs e)
        {
            txtUser.Text = UserPrincipal.Current.EmailAddress;
        }

        private void txtValues_TextChanged(object sender, EventArgs e)
        {
 
        }

        private void rdoUseUserSpecifiedUrl_CheckedChanged(object sender, EventArgs e)
        {
            btnDefault365Settings.Enabled = txtAutodiscoverServiceURL.Enabled = rdoUseUserSpecifiedUrl.Checked;
        }

        private void rdoUseAutoDiscover_CheckedChanged(object sender, EventArgs e)
        {
            btnDefault365Settings.Enabled = txtAutodiscoverServiceURL.Enabled = rdoUseUserSpecifiedUrl.Checked;
        }

        private void lblInfo_Click(object sender, EventArgs e)
        {

        }

        private void chkOptHeader1_CheckedChanged(object sender, EventArgs e)
        {
            txtHeader1Name.Enabled = chkOptHeader1.Checked;
            txtHeader1Value.Enabled = chkOptHeader1.Checked;
        }

        private void chkOptHeader2_CheckedChanged(object sender, EventArgs e)
        {
            txtHeader2Name.Enabled = chkOptHeader2.Checked;
            txtHeader2Value.Enabled = chkOptHeader2.Checked;
        }

        private void chkOptHeader3_CheckedChanged(object sender, EventArgs e)
        {
            txtHeader3Name.Enabled = chkOptHeader3.Checked;
            txtHeader3Value.Enabled = chkOptHeader3.Checked;
        }

        private void TempExchangeVersionCombo_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnDefault365Settings_Click(object sender, EventArgs e)
        {
            txtAutodiscoverServiceURL.Text = "https://autodiscover-s.outlook.com/autodiscover/autodiscover.svc";
        }
    }
}
