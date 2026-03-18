using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using v2rayN.Base;
using v2rayN.Handler;
using v2rayN.Mode;
using v2rayN.Resx;

namespace v2rayN.Forms
{
    public partial class RoutingRuleSettingDetailsForm : BaseForm
    {
        public RulesItem rulesItem
        {
            get; set;
        }

        public RoutingRuleSettingDetailsForm()
        {
            InitializeComponent();
        }

        private void RoutingRuleSettingDetailsForm_Load(object sender, EventArgs e)
        {
            if (Utils.IsNullOrEmpty(rulesItem.outboundTag))
            {
                ClearBind();
            }
            else
            {
                BindingData();
            }
        }

        private void EndBindingData()
        {
            if (rulesItem != null)
            {
                rulesItem.port = txtPort.Text.TrimEx();

                var inboundTag = new List<String>();
                if (chkTag_socks.Checked) inboundTag.Add("socks");
                if (chkTag_socks2.Checked) inboundTag.Add("socks2");
                if (chkTag_http.Checked) inboundTag.Add("http");
                if (chkTag_http2.Checked) inboundTag.Add("http2");
                rulesItem.inboundTag = inboundTag;
                rulesItem.outboundTag = cmbOutboundTag.Text;
                if (chkAutoSort.Checked)
                {
                    rulesItem.domain = Utils.String2ListSorted(txtDomain.Text);
                    rulesItem.ip = Utils.String2ListSorted(txtIP.Text);
                }
                else
                {
                    rulesItem.domain = Utils.String2List(txtDomain.Text);
                    rulesItem.ip = Utils.String2List(txtIP.Text);
                }

                var protocol = new List<string>();
                if (chkProto_socks.Checked) protocol.Add("socks");
                if (chkProto_socks2.Checked) protocol.Add("socks2");
                if (chkProto_http.Checked) protocol.Add("http");
                if (chkProto_http2.Checked) protocol.Add("http2");
                rulesItem.protocol = protocol;
                rulesItem.enabled = chkEnabled.Checked;
            }
        }
        private void BindingData()
        {
            if (rulesItem != null)
            {
                txtPort.Text = rulesItem.port ?? string.Empty;
                cmbOutboundTag.Text = rulesItem.outboundTag;
                txtDomain.Text = Utils.List2String(rulesItem.domain, true);
                txtIP.Text = Utils.List2String(rulesItem.ip, true);

                if (rulesItem.inboundTag != null)
                {
                    chkTag_socks.Checked = rulesItem.inboundTag.Contains("socks");
                    chkTag_socks2.Checked = rulesItem.inboundTag.Contains("socks2");
                    chkTag_http.Checked = rulesItem.inboundTag.Contains("http");
                    chkTag_http2.Checked = rulesItem.inboundTag.Contains("http2");
                }

                if (rulesItem.protocol != null)
                {
                    chkProto_socks.Checked = rulesItem.protocol.Contains("socks");
                    chkProto_socks2.Checked = rulesItem.protocol.Contains("socks2");
                    chkProto_http.Checked = rulesItem.protocol.Contains("http");
                    chkProto_http2.Checked = rulesItem.protocol.Contains("http2");
                }
                chkEnabled.Checked = rulesItem.enabled;
            }
        }
        private void ClearBind()
        {
            txtPort.Text = string.Empty;
            cmbOutboundTag.Text = Global.agentTag;
            txtDomain.Text = string.Empty;
            txtIP.Text = string.Empty;
            chkEnabled.Checked = true;
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            EndBindingData();

            bool hasRule = 
                rulesItem.domain != null 
                && rulesItem.domain.Count > 0 
                || rulesItem.ip != null 
                && rulesItem.ip.Count > 0 
                || rulesItem.protocol != null 
                && rulesItem.protocol.Count > 0 
                || !Utils.IsNullOrEmpty(rulesItem.port);

            if (!hasRule)
            {
                UI.ShowWarning(string.Format(ResUI.RoutingRuleDetailRequiredTips, "Port/Protocol/Domain/IP"));
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseCancel();
        }

        private void linkRuleobjectDoc_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.v2fly.org/config/routing.html#ruleobject");
        }
    }
}
