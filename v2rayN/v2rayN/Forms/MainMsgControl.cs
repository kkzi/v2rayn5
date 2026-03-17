using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using v2rayN.Base;
using v2rayN.Mode;
using v2rayN.Resx;

namespace v2rayN.Forms
{
    public partial class MainMsgControl : UserControl
    {
        private string _msgFilter = string.Empty;
        delegate void AppendTextDelegate(string text);

        private static readonly Font StatusBarFont = new Font("Microsoft YaHei UI", 8F, FontStyle.Regular, GraphicsUnit.Point);

        public MainMsgControl()
        {
            InitializeComponent();
            ssMain.Font = StatusBarFont;
            toolSdbSysProxy.DropDownItemClicked += toolSdbSysProxy_DropDownItemClicked;
            toolSdbRoutingRule.DropDownItemClicked += toolSdbRoutingRule_DropDownItemClicked;
        }

        private void MainMsgControl_Load(object sender, EventArgs e)
        {
            _msgFilter = Utils.RegReadValue(Global.MyRegPath, Utils.MainMsgFilterKey, "");
            if (!Utils.IsNullOrEmpty(_msgFilter))
            {
                gbMsgTitle.Text = string.Format(ResUI.MsgInformationTitle, _msgFilter);
            }
        }

        #region 提示信息

        public void AppendText(string text)
        {
            if (txtMsgBox.InvokeRequired)
            {
                Invoke(new AppendTextDelegate(AppendText), text);
            }
            else
            {
                if (!Utils.IsNullOrEmpty(_msgFilter))
                {
                    if (!Regex.IsMatch(text, _msgFilter))
                    {
                        return;
                    }
                }
                ShowMsg(text);
            }
        }

        /// <summary>
        /// 提示信息
        /// </summary>
        /// <param name="msg"></param>
        private void ShowMsg(string msg)
        {
            if (txtMsgBox.Lines.Length > 999)
            {
                var lines = txtMsgBox.Lines;
                int startIndex = Math.Max(0, lines.Length - 1000);
                txtMsgBox.Lines = lines.Skip(startIndex).ToArray();
            }
            txtMsgBox.AppendText(msg);
            if (!msg.EndsWith(Environment.NewLine))
            {
                txtMsgBox.AppendText(Environment.NewLine);
            }
        }

        /// <summary>
        /// 清除信息
        /// </summary>
        public void ClearMsg()
        {
            txtMsgBox.Invoke((Action)delegate
            {
                txtMsgBox.Clear();
            });
        }

        public void DisplayToolStatus(Config config)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{ResUI.LabLocal}:");
            sb.Append($"[{Global.InboundSocks}:{config.GetLocalPort(Global.InboundSocks)}]");
            sb.Append(" | ");
            sb.Append($"[{Global.InboundHttp}:{config.GetLocalPort(Global.InboundHttp)}]");

            if (config.inbound[0].allowLANConn)
            {
                sb.Append($"  {ResUI.LabLAN}:");
                sb.Append($"[{Global.InboundSocks}:{config.GetLocalPort(Global.InboundSocks2)}]");
                sb.Append(" | ");
                sb.Append($"[{Global.InboundHttp}:{config.GetLocalPort(Global.InboundHttp2)}]");
            }

            SetToolSslInfo("inbound", sb.ToString());
        }

        public void SetToolSslInfo(string type, string value)
        {
            switch (type)
            {
                case "speed":
                    toolSslServerSpeed.Text = value;
                    break;
                case "inbound":
                    toolSslInboundInfo.Text = value;
                    break;
                case "routing":
                    SetRoutingText(value);
                    break;
            }

        }

        public void SetRoutingItems(List<RoutingItem> routings, int selectedIndex, bool enabled)
        {
            toolSdbRoutingRule.DropDownItems.Clear();
            toolSdbRoutingRule.Enabled = enabled;
            if (!enabled || routings == null || routings.Count <= 0)
            {
                SetRoutingText(string.Empty);
                return;
            }

            bool hasSelection = false;
            for (int k = 0; k < routings.Count; k++)
            {
                var item = routings[k];
                if (item.locked == true)
                {
                    continue;
                }

                string name = item.remarks ?? string.Empty;
                ToolStripMenuItem ts = new ToolStripMenuItem(name)
                {
                    Tag = k,
                    Checked = selectedIndex.Equals(k)
                };
                if (selectedIndex.Equals(k))
                {
                    SetRoutingText(name);
                    hasSelection = true;
                }
                toolSdbRoutingRule.DropDownItems.Add(ts);
            }

            if (!hasSelection)
            {
                SetRoutingText(string.Empty);
            }
        }

        public void SetSysProxyItems(List<KeyValuePair<ESysProxyType, string>> items, ESysProxyType selectedType, string defaultText)
        {
            toolSdbSysProxy.DropDownItems.Clear();
            toolSdbSysProxy.Enabled = items != null && items.Count > 0;

            string selectedText = Utils.IsNullOrEmpty(defaultText) ? ResUI.SystemProxy : defaultText;

            if (items != null)
            {
                foreach (var item in items)
                {
                    string text = item.Value ?? string.Empty;
                    ToolStripMenuItem ts = new ToolStripMenuItem(text)
                    {
                        Tag = (int)item.Key,
                        Checked = selectedType == item.Key
                    };
                    if (selectedType == item.Key && !Utils.IsNullOrEmpty(text))
                    {
                        selectedText = text;
                    }
                    toolSdbSysProxy.DropDownItems.Add(ts);
                }
            }

            toolSdbSysProxy.Text = selectedText ?? string.Empty;
            toolSdbSysProxy.ToolTipText = toolSdbSysProxy.Text;
        }

        private void SetRoutingText(string text)
        {
            toolSdbRoutingRule.Text = text ?? string.Empty;
            toolSdbRoutingRule.ToolTipText = toolSdbRoutingRule.Text;
        }

        private void toolSdbRoutingRule_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == null || e.ClickedItem.Tag == null)
            {
                return;
            }

            int index = Utils.ToInt(e.ClickedItem.Tag);
            RoutingSelected?.Invoke(this, new RoutingSelectedEventArgs(index));
        }

        private void toolSdbSysProxy_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == null || e.ClickedItem.Tag == null)
            {
                return;
            }

            int typeValue = Utils.ToInt(e.ClickedItem.Tag);
            if (!Enum.IsDefined(typeof(ESysProxyType), typeValue))
            {
                return;
            }

            SysProxySelected?.Invoke(this, new SysProxySelectedEventArgs((ESysProxyType)typeValue));
        }

        public void ScrollToCaret()
        {
            txtMsgBox.ScrollToCaret();
        }
        #endregion


        #region MsgBoxMenu
        private void menuMsgBoxSelectAll_Click(object sender, EventArgs e)
        {
            txtMsgBox.Focus();
            txtMsgBox.SelectAll();
        }

        private void menuMsgBoxCopy_Click(object sender, EventArgs e)
        {
            var data = txtMsgBox.SelectedText.TrimEx();
            Utils.SetClipboardData(data);
        }

        private void menuMsgBoxCopyAll_Click(object sender, EventArgs e)
        {
            var data = txtMsgBox.Text;
            Utils.SetClipboardData(data);
        }
        private void menuMsgBoxClear_Click(object sender, EventArgs e)
        {
            txtMsgBox.Clear();
        }
        private void menuMsgBoxAddRoutingRule_Click(object sender, EventArgs e)
        {
            menuMsgBoxCopy_Click(null, null);
            var fm = new RoutingSettingForm();
            fm.ShowDialog();

        }

        private void txtMsgBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        menuMsgBoxSelectAll_Click(null, null);
                        break;
                    case Keys.C:
                        menuMsgBoxCopy_Click(null, null);
                        break;
                    case Keys.V:
                        menuMsgBoxAddRoutingRule_Click(null, null);
                        break;

                }
            }

        }
        private void menuMsgBoxFilter_Click(object sender, EventArgs e)
        {
            var fm = new MsgFilterSetForm();
            fm.MsgFilter = _msgFilter;
            fm.ShowDefFilter = true;
            if (fm.ShowDialog() == DialogResult.OK)
            {
                _msgFilter = fm.MsgFilter;
                gbMsgTitle.Text = string.Format(ResUI.MsgInformationTitle, _msgFilter);
                Utils.RegWriteValue(Global.MyRegPath, Utils.MainMsgFilterKey, _msgFilter);
            }
        }

        private void ssMain_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == toolSdbRoutingRule || e.ClickedItem == toolSdbSysProxy || e.ClickedItem == toolBtnToggleLog)
            {
                return;
            }
            if (e.ClickedItem == toolSslInboundInfo)
            {
                OptionSettingRequested?.Invoke(this, EventArgs.Empty);
                return;
            }
            if (!Utils.IsNullOrEmpty(e.ClickedItem.Text))
            {
                Utils.SetClipboardData(e.ClickedItem.Text);
            }
        }
        #endregion

        private void toolBtnToggleLog_Click(object sender, EventArgs e)
        {
            ToggleLogRequested?.Invoke(this, EventArgs.Empty);
        }

        public void SetLogToggleState(bool isVisible)
        {
            toolBtnToggleLog.ToolTipText = isVisible ? ResUI.LogPanelHide : ResUI.LogPanelShow;
        }

        public void SetLogTextVisible(bool isVisible)
        {
            txtMsgBox.Visible = isVisible;
        }


        public event EventHandler<RoutingSelectedEventArgs> RoutingSelected;
        public event EventHandler<SysProxySelectedEventArgs> SysProxySelected;
        public event EventHandler ToggleLogRequested;
        public event EventHandler OptionSettingRequested;

        public class RoutingSelectedEventArgs : EventArgs
        {
            public RoutingSelectedEventArgs(int selectedIndex)
            {
                SelectedIndex = selectedIndex;
            }

            public int SelectedIndex { get; }
        }

        public class SysProxySelectedEventArgs : EventArgs
        {
            public SysProxySelectedEventArgs(ESysProxyType selectedType)
            {
                SelectedType = selectedType;
            }

            public ESysProxyType SelectedType { get; }
        }
    }
}
