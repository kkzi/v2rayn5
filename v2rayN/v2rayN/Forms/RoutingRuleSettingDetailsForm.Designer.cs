namespace v2rayN.Forms
{
    partial class RoutingRuleSettingDetailsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RoutingRuleSettingDetailsForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.linkRuleobjectDoc = new System.Windows.Forms.LinkLabel();
            this.chkEnabled = new System.Windows.Forms.CheckBox();
            this.flpInboundTag = new System.Windows.Forms.FlowLayoutPanel();
            this.chkTag_socks = new System.Windows.Forms.CheckBox();
            this.chkTag_socks2 = new System.Windows.Forms.CheckBox();
            this.chkTag_http = new System.Windows.Forms.CheckBox();
            this.chkTag_http2 = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.flpProtocol = new System.Windows.Forms.FlowLayoutPanel();
            this.chkProto_socks = new System.Windows.Forms.CheckBox();
            this.chkProto_socks2 = new System.Windows.Forms.CheckBox();
            this.chkProto_http = new System.Windows.Forms.CheckBox();
            this.chkProto_http2 = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labRoutingTips = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbOutboundTag = new System.Windows.Forms.ComboBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.chkAutoSort = new System.Windows.Forms.CheckBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtDomain = new System.Windows.Forms.TextBox();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // panel3
            // 
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Controls.Add(this.linkRuleobjectDoc);
            this.panel3.Controls.Add(this.chkEnabled);
            this.panel3.Controls.Add(this.flpInboundTag);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Controls.Add(this.flpProtocol);
            this.panel3.Controls.Add(this.label3);
            this.panel3.Controls.Add(this.txtPort);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Controls.Add(this.labRoutingTips);
            this.panel3.Controls.Add(this.label4);
            this.panel3.Controls.Add(this.cmbOutboundTag);
            this.panel3.Name = "panel3";
            // 
            // linkRuleobjectDoc
            // 
            resources.ApplyResources(this.linkRuleobjectDoc, "linkRuleobjectDoc");
            this.linkRuleobjectDoc.Name = "linkRuleobjectDoc";
            this.linkRuleobjectDoc.TabStop = true;
            this.linkRuleobjectDoc.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkRuleobjectDoc_LinkClicked);
            // 
            // chkEnabled
            // 
            resources.ApplyResources(this.chkEnabled, "chkEnabled");
            this.chkEnabled.Name = "chkEnabled";
            this.chkEnabled.UseVisualStyleBackColor = true;
            //
            // flpInboundTag
            //
            resources.ApplyResources(this.flpInboundTag, "clbInboundTag");
            this.flpInboundTag.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpInboundTag.AutoSize = true;
            this.flpInboundTag.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpInboundTag.WrapContents = false;
            this.flpInboundTag.Controls.Add(this.chkTag_socks);
            this.flpInboundTag.Controls.Add(this.chkTag_socks2);
            this.flpInboundTag.Controls.Add(this.chkTag_http);
            this.flpInboundTag.Controls.Add(this.chkTag_http2);
            this.flpInboundTag.Name = "flpInboundTag";
            //
            // chkTag_socks
            //
            this.chkTag_socks.AutoSize = true;
            this.chkTag_socks.Text = "socks";
            this.chkTag_socks.Name = "chkTag_socks";
            //
            // chkTag_socks2
            //
            this.chkTag_socks2.AutoSize = true;
            this.chkTag_socks2.Text = "socks2";
            this.chkTag_socks2.Name = "chkTag_socks2";
            //
            // chkTag_http
            //
            this.chkTag_http.AutoSize = true;
            this.chkTag_http.Text = "http";
            this.chkTag_http.Name = "chkTag_http";
            //
            // chkTag_http2
            //
            this.chkTag_http2.AutoSize = true;
            this.chkTag_http2.Text = "http2";
            this.chkTag_http2.Name = "chkTag_http2";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            //
            // flpProtocol
            //
            resources.ApplyResources(this.flpProtocol, "clbProtocol");
            this.flpProtocol.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpProtocol.AutoSize = true;
            this.flpProtocol.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpProtocol.WrapContents = false;
            this.flpProtocol.Controls.Add(this.chkProto_socks);
            this.flpProtocol.Controls.Add(this.chkProto_socks2);
            this.flpProtocol.Controls.Add(this.chkProto_http);
            this.flpProtocol.Controls.Add(this.chkProto_http2);
            this.flpProtocol.Name = "flpProtocol";
            //
            // chkProto_socks
            //
            this.chkProto_socks.AutoSize = true;
            this.chkProto_socks.Text = "socks";
            this.chkProto_socks.Name = "chkProto_socks";
            //
            // chkProto_socks2
            //
            this.chkProto_socks2.AutoSize = true;
            this.chkProto_socks2.Text = "socks2";
            this.chkProto_socks2.Name = "chkProto_socks2";
            //
            // chkProto_http
            //
            this.chkProto_http.AutoSize = true;
            this.chkProto_http.Text = "http";
            this.chkProto_http.Name = "chkProto_http";
            //
            // chkProto_http2
            //
            this.chkProto_http2.AutoSize = true;
            this.chkProto_http2.Text = "http2";
            this.chkProto_http2.Name = "chkProto_http2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // txtPort
            // 
            resources.ApplyResources(this.txtPort, "txtPort");
            this.txtPort.Name = "txtPort";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // labRoutingTips
            // 
            resources.ApplyResources(this.labRoutingTips, "labRoutingTips");
            this.labRoutingTips.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labRoutingTips.ForeColor = System.Drawing.Color.Brown;
            this.labRoutingTips.Name = "labRoutingTips";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // cmbOutboundTag
            // 
            resources.ApplyResources(this.cmbOutboundTag, "cmbOutboundTag");
            this.cmbOutboundTag.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOutboundTag.FormattingEnabled = true;
            this.cmbOutboundTag.Items.AddRange(new object[] {
            resources.GetString("cmbOutboundTag.Items"),
            resources.GetString("cmbOutboundTag.Items1"),
            resources.GetString("cmbOutboundTag.Items2")});
            this.cmbOutboundTag.Name = "cmbOutboundTag";
            // 
            // panel4
            // 
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.Controls.Add(this.chkAutoSort);
            this.panel4.Controls.Add(this.btnClose);
            this.panel4.Controls.Add(this.btnOK);
            this.panel4.Name = "panel4";
            // 
            // chkAutoSort
            // 
            resources.ApplyResources(this.chkAutoSort, "chkAutoSort");
            this.chkAutoSort.Name = "chkAutoSort";
            this.chkAutoSort.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            resources.ApplyResources(this.btnClose, "btnClose");
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Name = "btnClose";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.groupBox2);
            this.panel2.Controls.Add(this.groupBox1);
            this.panel2.Name = "panel2";
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.txtIP);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // txtIP
            // 
            resources.ApplyResources(this.txtIP, "txtIP");
            this.txtIP.Name = "txtIP";
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.txtDomain);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // txtDomain
            // 
            resources.ApplyResources(this.txtDomain, "txtDomain");
            this.txtDomain.Name = "txtDomain";
            // 
            // RoutingRuleSettingDetailsForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.btnClose;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Name = "RoutingRuleSettingDetailsForm";
            this.Load += new System.EventHandler(this.RoutingRuleSettingDetailsForm_Load);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbOutboundTag;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtDomain;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.Label labRoutingTips;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.FlowLayoutPanel flpProtocol;
        private System.Windows.Forms.CheckBox chkProto_socks;
        private System.Windows.Forms.CheckBox chkProto_socks2;
        private System.Windows.Forms.CheckBox chkProto_http;
        private System.Windows.Forms.CheckBox chkProto_http2;
        private System.Windows.Forms.FlowLayoutPanel flpInboundTag;
        private System.Windows.Forms.CheckBox chkTag_socks;
        private System.Windows.Forms.CheckBox chkTag_socks2;
        private System.Windows.Forms.CheckBox chkTag_http;
        private System.Windows.Forms.CheckBox chkTag_http2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkEnabled;
        private System.Windows.Forms.CheckBox chkAutoSort;
        private System.Windows.Forms.LinkLabel linkRuleobjectDoc;
    }
}
