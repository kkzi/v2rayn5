using System;
using System.Windows.Forms;
using v2rayN.Base;
using v2rayN.Handler;
using v2rayN.Mode;
using System.Linq;
using System.Collections.Generic;

namespace v2rayN.Forms
{
    public delegate void ChangeEventHandler(object sender, EventArgs e);
    public partial class SubSettingControl : UserControl
    {
        public event ChangeEventHandler OnButtonClicked;

        private const int QrCodeExtraHeight = 170;
        private const int BottomPadding = 8;

        public SubItem subItem
        {
            get; set;
        }

        public int CollapsedHeight => grbMain.Height;

        public SubSettingControl()
        {
            InitializeComponent();
        }

        private void SubSettingControl_Load(object sender, EventArgs e)
        {
            UpdateCollapsedLayout();

            BindingSub();
        }

        private void UpdateCollapsedLayout()
        {
            try
            {
                // Calculate minimal group box height to remove excessive blank area.
                int bottom = 0;
                Control[] parts =
                {
                    txtRemarks, txtUrl, txtUserAgent,
                    label1, label2, label3,
                    chkEnabled, btnShare, btnRemove
                };
                foreach (var c in parts)
                {
                    if (c == null) continue;
                    bottom = Math.Max(bottom, c.Bottom);
                }

                grbMain.Height = bottom + BottomPadding;
                Height = grbMain.Height;
                picQRCode.Top = grbMain.Height;

                if (Parent != null)
                {
                    Parent.Height = Height + Parent.Padding.Bottom;
                }
            }
            catch { }
        }

        private void BindingSub()
        {
            if (subItem != null)
            {
                txtRemarks.Text = subItem.remarks.ToString();
                txtUrl.Text = subItem.url.ToString();
                chkEnabled.Checked = subItem.enabled;
                txtUserAgent.Text = subItem.userAgent;
            }
        }
        private void EndBindingSub()
        {
            if (subItem != null)
            {
                subItem.remarks = txtRemarks.Text.TrimEx();
                subItem.url = txtUrl.Text.TrimEx();
                subItem.enabled = chkEnabled.Checked;
                subItem.userAgent = txtUserAgent.Text.TrimEx();
            }
        }
        private void txtRemarks_Leave(object sender, EventArgs e)
        {
            EndBindingSub();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (subItem != null)
            {
                subItem.remarks = string.Empty;
                subItem.url = string.Empty;
            }

            OnButtonClicked?.Invoke(sender, e);
        }

        private void btnShare_Click(object sender, EventArgs e)
        {
            if (Height <= grbMain.Height)
            {
                if (Utils.IsNullOrEmpty(subItem.url))
                {
                    picQRCode.Image = null;
                    return;
                }
                picQRCode.Image = QRCodeHelper.GetQRCode(subItem.url);
                Height = grbMain.Height + QrCodeExtraHeight;
            }
            else
            {
                Height = grbMain.Height;
            }

            // Keep wrapper panel height in sync (SubSettingForm wraps each row in a Panel for spacing).
            try
            {
                if (Parent != null)
                {
                    Parent.Height = Height + Parent.Padding.Bottom;
                }
            }
            catch { }
        }
    }
}
