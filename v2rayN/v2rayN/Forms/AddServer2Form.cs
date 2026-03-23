using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using v2rayN.Handler;
using v2rayN.Mode;
using v2rayN.Resx;

namespace v2rayN.Forms
{
    public partial class AddServer2Form : BaseServerForm
    {

        public AddServer2Form()
        {
            InitializeComponent();
        }

        private void AddServer2Form_Load(object sender, EventArgs e)
        {
            InitCoreTypeComboBox(cmbCoreType, excludeV2rayN: true);

            txtAddress.ReadOnly = true;
            if (vmessItem != null)
            {
                BindingServer();
            }
            else
            {
                vmessItem = new VmessItem
                {
                    subid = string.Empty
                };
                ClearServer();
            }
        }

        /// <summary>
        /// 绑定数据
        /// </summary>
        private void BindingServer()
        {
            txtRemarks.Text = vmessItem.remarks;
            txtAddress.Text = vmessItem.address;
            txtPreSocksPort.Text = vmessItem.preSocksPort.ToString();

            SetCoreTypeText(cmbCoreType, vmessItem.coreType);
        }


        /// <summary>
        /// 清除设置
        /// </summary>
        private void ClearServer()
        {
            txtRemarks.Text = "";
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string remarks = txtRemarks.Text;
            if (Utils.IsNullOrEmpty(remarks))
            {
                ShowOwnedInfoPrompt(ResUI.PleaseFillRemarks);
                return;
            }
            if (Utils.IsNullOrEmpty(txtAddress.Text))
            {
                ShowOwnedInfoPrompt(ResUI.FillServerAddressCustom);
                return;
            }
            vmessItem.remarks = remarks;
            vmessItem.preSocksPort = Utils.ToInt(txtPreSocksPort.Text);

            vmessItem.coreType = ParseCoreType(cmbCoreType.Text);

            HandleResult(ConfigHandler.EditCustomServer(ref config, vmessItem));
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = Utils.IsNullOrEmpty(vmessItem.indexId) ? DialogResult.Cancel : DialogResult.OK;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            ShowOwnedInfoPrompt(ResUI.CustomServerTips);

            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Config|*.json|YAML|*.yaml;*.yml|All|*.*"
            };
            if (fileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            string fileName = fileDialog.FileName;
            if (Utils.IsNullOrEmpty(fileName))
            {
                return;
            }

            vmessItem.address = fileName;
            vmessItem.remarks = txtRemarks.Text;

            if (ConfigHandler.AddCustomServer(ref config, vmessItem, false) == 0)
            {
                BindingServer();
                ShowOwnedInfoPrompt(ResUI.SuccessfullyImportedCustomServer);
            }
            else
            {
                ShowOwnedWarningPrompt(ResUI.FailedImportedCustomServer);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            var address = txtAddress.Text;
            if (Utils.IsNullOrEmpty(address))
            {
                ShowOwnedInfoPrompt(ResUI.FillServerAddressCustom);
                return;
            }

            address = Utils.GetConfigPath(address);
            Process.Start(address);
        }
    }
}
