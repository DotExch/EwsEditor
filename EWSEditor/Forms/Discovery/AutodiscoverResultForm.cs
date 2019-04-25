using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Exchange.WebServices.Autodiscover;

namespace EWSEditor.Forms
{
    public partial class AutodiscoverResultForm : BaseContentForm
    {
        public class AutodiscoverResult
        {
            public AutodiscoverResult(GetUserSettingsResponse response, Uri serviceUrl)
            {
                Response = response;
                ServiceUrl = serviceUrl;
            }

            public Uri ServiceUrl { get; set; }
            public GetUserSettingsResponse Response { get; set; }
        }

        private const string colNameSmtpAddress = "colSmtpAddress";

        private const string colNameRedirectTarget = "colNameRedirectTarget";

        private List<AutodiscoverResult> responses;

        protected override bool IsServiceLess => true;

        public AutodiscoverResultForm()
        {
            InitializeComponent();
        }

        public static void Show(Form parentForm, IEnumerable<AutodiscoverResult> responses)
        {
            AutodiscoverResultForm form = new AutodiscoverResultForm();

            form.responses = responses.ToList();
            form.Text = "Autodiscover results";
            form.CallingForm = parentForm;
            form.Show();
        }

        protected override void SetupForm()
        {
            base.SetupForm();
            PropertyDetailsGrid.ShowKnownNamesColumn = false;
            ContentsGrid.Columns.Clear();
            var cols = ContentsGrid.Columns;
            int col;
            col = cols.Add(colNameSmtpAddress, "SMTP Address");
            cols[col].Width = 175;
            col = cols.Add(colNameRedirectTarget, "Autodiscover URL");
            cols[col].Width = 550;

            ContentIdColumnName = colNameSmtpAddress;
        }

        protected override void LoadContents()
        {
            // Load new results
            foreach (AutodiscoverResult res in responses)
            {
                int rowIdx = ContentsGrid.Rows.Add();
                DataGridViewRow row = ContentsGrid.Rows[rowIdx];
                row.Cells[colNameSmtpAddress].Value = res.Response.SmtpAddress;
                row.Cells[colNameRedirectTarget].Value = res.ServiceUrl;

                row.Tag = res;
            }
        }

        protected override void LoadSelectionDetails()
        {
            // Only load details if a content row is selected
            if (ContentsGrid.SelectedRows.Count == 0)
                return;

            PropertyDetailsGrid.Clear();
            var res = (AutodiscoverResult)ContentsGrid.SelectedRows[0].Tag;
            if (res == null)
                return;

            PropertyDetailsGrid.LoadObject(res.Response);
        }
    }
}
