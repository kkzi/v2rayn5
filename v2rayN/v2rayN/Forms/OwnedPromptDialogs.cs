using System;
using System.Drawing;
using System.Windows.Forms;
using v2rayN.Resx;
using v2rayN.Tool;

namespace v2rayN.Forms
{
    internal sealed class OwnedSingleButtonPromptDialog : Form
    {
        private const int ButtonHeight = 32;
        private const int ButtonWidth = 88;
        private const int DialogHeight = 156;
        private const int DialogWidth = 360;
        private const int SidePadding = 14;
        private const int TopPadding = 14;
        private const int BottomPadding = 12;

        private readonly TextBox messageBox_;
        private readonly Button btnOk_;

        public OwnedSingleButtonPromptDialog(string title, string message, Font ownerFont, Icon ownerIcon)
        {
            Text = title ?? string.Empty;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = ownerFont ?? Font;
            if (ownerIcon != null)
            {
                Icon = ownerIcon;
            }

            Size = new Size(DialogWidth, DialogHeight);
            MinimumSize = Size;
            MaximumSize = Size;

            messageBox_ = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Multiline = true,
                ReadOnly = true,
                TabStop = false,
                Text = message ?? string.Empty,
                BackColor = SystemColors.Control,
                ScrollBars = ScrollBars.None,
                Left = SidePadding,
                Top = TopPadding,
                Width = ClientSize.Width - SidePadding * 2
            };

            btnOk_ = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                AutoSize = false,
                Size = new Size(ButtonWidth, ButtonHeight)
            };

            AcceptButton = btnOk_;
            CancelButton = btnOk_;

            Controls.Add(messageBox_);
            Controls.Add(btnOk_);

            DpiChanged += OwnedPromptDialog_DpiChanged;
            ApplyScaledLayout();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            try
            {
                ApplyScaledLayout();
            }
            catch { }
        }

        private void OwnedPromptDialog_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            ApplyScaledLayout();
        }

        private void ApplyScaledLayout()
        {
            try
            {
                int deviceDpi = HighDpiHelper.GetControlDeviceDpi(this);
                int dialogWidth = HighDpiHelper.ScaleLogicalValue(DialogWidth, deviceDpi);
                int dialogHeight = HighDpiHelper.ScaleLogicalValue(DialogHeight, deviceDpi);
                int buttonWidth = HighDpiHelper.ScaleLogicalValue(ButtonWidth, deviceDpi);
                int buttonHeight = HighDpiHelper.ScaleLogicalValue(ButtonHeight, deviceDpi);
                int sidePadding = HighDpiHelper.ScaleLogicalValue(SidePadding, deviceDpi);
                int topPadding = HighDpiHelper.ScaleLogicalValue(TopPadding, deviceDpi);
                int bottomPadding = HighDpiHelper.ScaleLogicalValue(BottomPadding, deviceDpi);
                int messageBottomSpacing = HighDpiHelper.ScaleLogicalValue(6, deviceDpi);

                Size = new Size(dialogWidth, dialogHeight);
                MinimumSize = Size;
                MaximumSize = Size;

                btnOk_.Size = new Size(buttonWidth, buttonHeight);
                btnOk_.Left = Math.Max(0, (ClientSize.Width - btnOk_.Width) / 2);
                btnOk_.Top = Math.Max(0, ClientSize.Height - bottomPadding - buttonHeight);

                messageBox_.Left = sidePadding;
                messageBox_.Top = topPadding;
                messageBox_.Width = Math.Max(0, ClientSize.Width - sidePadding * 2);
                messageBox_.Height = Math.Max(0, btnOk_.Top - topPadding - messageBottomSpacing);
            }
            catch { }
        }
    }

    internal sealed class OwnedYesNoPromptDialog : Form
    {
        private const int ButtonHeight = 32;
        private const int ButtonWidth = 80;
        private const int DialogHeight = 160;
        private const int DialogWidth = 320;
        private const int ButtonSpacing = 12;
        private const int SidePadding = 14;
        private const int TopPadding = 14;
        private const int BottomPadding = 12;

        private readonly TextBox messageBox_;
        private readonly Button btnYes_;
        private readonly Button btnNo_;

        public OwnedYesNoPromptDialog(string title, string message, Font ownerFont, Icon ownerIcon)
        {
            Text = title ?? string.Empty;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = ownerFont ?? Font;
            if (ownerIcon != null)
            {
                Icon = ownerIcon;
            }

            Size = new Size(DialogWidth, DialogHeight);
            MinimumSize = Size;
            MaximumSize = Size;

            messageBox_ = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Multiline = true,
                ReadOnly = true,
                TabStop = false,
                Text = message ?? string.Empty,
                BackColor = SystemColors.Control,
                ScrollBars = ScrollBars.None,
                Left = SidePadding,
                Top = TopPadding,
                Width = ClientSize.Width - SidePadding * 2
            };

            btnYes_ = new Button
            {
                Text = ResUI.DialogYes,
                DialogResult = DialogResult.Yes,
                AutoSize = false,
                Size = new Size(ButtonWidth, ButtonHeight)
            };

            btnNo_ = new Button
            {
                Text = ResUI.DialogNo,
                DialogResult = DialogResult.No,
                AutoSize = false,
                Size = new Size(ButtonWidth, ButtonHeight)
            };

            AcceptButton = btnYes_;
            CancelButton = btnNo_;

            Controls.Add(messageBox_);
            Controls.Add(btnYes_);
            Controls.Add(btnNo_);

            DpiChanged += OwnedPromptDialog_DpiChanged;
            ApplyScaledLayout();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            try
            {
                ApplyScaledLayout();
            }
            catch { }
        }

        private void OwnedPromptDialog_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            ApplyScaledLayout();
        }

        private void ApplyScaledLayout()
        {
            try
            {
                int deviceDpi = HighDpiHelper.GetControlDeviceDpi(this);
                int dialogWidth = HighDpiHelper.ScaleLogicalValue(DialogWidth, deviceDpi);
                int dialogHeight = HighDpiHelper.ScaleLogicalValue(DialogHeight, deviceDpi);
                int buttonWidth = HighDpiHelper.ScaleLogicalValue(ButtonWidth, deviceDpi);
                int buttonHeight = HighDpiHelper.ScaleLogicalValue(ButtonHeight, deviceDpi);
                int buttonSpacing = HighDpiHelper.ScaleLogicalValue(ButtonSpacing, deviceDpi);
                int sidePadding = HighDpiHelper.ScaleLogicalValue(SidePadding, deviceDpi);
                int topPadding = HighDpiHelper.ScaleLogicalValue(TopPadding, deviceDpi);
                int bottomPadding = HighDpiHelper.ScaleLogicalValue(BottomPadding, deviceDpi);
                int messageBottomSpacing = HighDpiHelper.ScaleLogicalValue(6, deviceDpi);

                Size = new Size(dialogWidth, dialogHeight);
                MinimumSize = Size;
                MaximumSize = Size;

                btnYes_.Size = new Size(buttonWidth, buttonHeight);
                btnNo_.Size = new Size(buttonWidth, buttonHeight);

                messageBox_.Left = sidePadding;
                messageBox_.Top = topPadding;
                messageBox_.Width = Math.Max(0, ClientSize.Width - sidePadding * 2);

                int buttonsTotalWidth = buttonWidth * 2 + buttonSpacing;
                int buttonsLeft = Math.Max(0, (ClientSize.Width - buttonsTotalWidth) / 2);
                int buttonsTop = Math.Max(0, ClientSize.Height - bottomPadding - buttonHeight);

                btnYes_.Left = buttonsLeft;
                btnYes_.Top = buttonsTop;

                btnNo_.Left = btnYes_.Right + buttonSpacing;
                btnNo_.Top = buttonsTop;

                messageBox_.Height = Math.Max(0, btnYes_.Top - topPadding - messageBottomSpacing);
            }
            catch { }
        }
    }
}
