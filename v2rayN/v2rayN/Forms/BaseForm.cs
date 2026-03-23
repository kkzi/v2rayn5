using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using v2rayN.Mode;
using v2rayN.Resx;
using v2rayN.Tool;

namespace v2rayN.Forms
{
    public partial class BaseForm : Form
    {
        protected static Config config;
        private const int DialogBottomButtonHeight = 32;
        private const string PromptDialogTitle = "v2rayN";
        private bool bottomButtonsNormalized_;
        private bool applyingManualDpiFonts_;

        public BaseForm()
        {
            InitializeComponent();

            Font = HighDpiHelper.NormalizeFontToPoints(Font);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            LoadCustomIcon();
            DpiChanged += BaseForm_DpiChanged;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ApplyManualDpiFonts();
            NormalizeDialogBottomButtonsOnce();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ApplyManualDpiFonts();
            NormalizeDialogBottomButtonsOnce();
        }

        private void NormalizeDialogBottomButtonsOnce()
        {
            try
            {
                if (bottomButtonsNormalized_)
                {
                    return;
                }
                bottomButtonsNormalized_ = true;

                // Find bottom-docked containers that look like a dialog button bar.
                var containers = EnumerateDescendants(this)
                    .Where(c => c != null
                                && c.Dock == DockStyle.Bottom
                                && c is not StatusStrip
                                && c is not ToolStrip
                                && c is not MenuStrip)
                    .Where(c => EnumerateDescendants(c).OfType<Button>().Any())
                    .ToList();

                // Also include the form itself if it directly hosts bottom-anchored dialog buttons.
                if (EnumerateDescendants(this).OfType<Button>().Any(b => b != null && b.Anchor.HasFlag(AnchorStyles.Bottom)))
                {
                    containers.Add(this);
                }

                foreach (var container in containers.Distinct())
                {
                    var buttons = EnumerateDescendants(container).OfType<Button>()
                        .Where(b => b != null)
                        .ToList();

                    if (buttons.Count <= 0)
                    {
                        continue;
                    }

                    foreach (var b in buttons)
                    {
                        // Only touch dialog-style buttons.
                        if (b.Dock != DockStyle.None)
                        {
                            continue;
                        }

                        // Force a consistent dialog button height across the app.
                        b.AutoSize = false;
                        b.Height = DialogBottomButtonHeight;

                        // Keep buttons visually centered in a bottom button bar when possible.
                        try
                        {
                            if (b.Parent is Panel p && p.ClientSize.Height >= DialogBottomButtonHeight)
                            {
                                int top = (p.ClientSize.Height - DialogBottomButtonHeight) / 2;
                                b.Top = Math.Max(p.Padding.Top, top);
                            }
                        }
                        catch { }
                    }

                    // Ensure the button bar is tall enough for the new heights.
                    if (container != this)
                    {
                        int maxBottom = buttons.Max(b => b.Bottom);
                        int desiredHeight = maxBottom + container.Padding.Bottom + 6;
                        if (container.Height < desiredHeight)
                        {
                            container.Height = desiredHeight;
                        }
                    }
                }
            }
            catch { }
        }

        private void BaseForm_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            ApplyManualDpiFonts();
        }

        protected void ApplyManualDpiFonts()
        {
            try
            {
                if (applyingManualDpiFonts_)
                {
                    return;
                }

                applyingManualDpiFonts_ = true;
                var controls = EnumerateSelfAndDescendants(this).ToList();

                foreach (Control control in controls)
                {
                    if (!ShouldApplyManualDpiFont(control) || control.Font == null)
                    {
                        continue;
                    }

                    Font normalizedFont = HighDpiHelper.NormalizeFontToPoints(control.Font);
                    if (HighDpiHelper.AreFontsEquivalent(control.Font, normalizedFont))
                    {
                        normalizedFont.Dispose();
                        continue;
                    }

                    control.Font = normalizedFont;
                }

                foreach (Control control in controls.OfType<ListView>())
                {
                    HighDpiHelper.EnableListViewDpiScaling((ListView)control);
                }

                PerformLayout();
            }
            catch { }
            finally
            {
                applyingManualDpiFonts_ = false;
            }
        }

        private static bool ShouldApplyManualDpiFont(Control control)
        {
            return control is not ToolStrip
                   && control is not MenuStrip
                   && control is not StatusStrip;
        }

        private static IEnumerable<Control> EnumerateSelfAndDescendants(Control root)
        {
            yield return root;

            foreach (var control in EnumerateDescendants(root))
            {
                yield return control;
            }
        }

        private static IEnumerable<Control> EnumerateDescendants(Control root)
        {
            if (root == null)
            {
                yield break;
            }

            foreach (Control child in root.Controls)
            {
                if (child == null) continue;
                yield return child;
                foreach (var grand in EnumerateDescendants(child))
                {
                    yield return grand;
                }
            }
        }

        protected void CloseCancel()
        {
            DialogResult = DialogResult.Cancel;
        }

        private void ShowOwnedSingleButtonPrompt(string message, Action<string> fallback)
        {
            try
            {
                using (var dialog = new OwnedSingleButtonPromptDialog(PromptDialogTitle, message, Font, Icon))
                {
                    dialog.ShowDialog(this);
                }
            }
            catch
            {
                fallback?.Invoke(message);
            }
        }

        protected virtual void ShowOwnedInfoPrompt(string message)
        {
            ShowOwnedSingleButtonPrompt(message, UI.Show);
        }

        protected bool ShowOwnedInfoPromptSafe(string message)
        {
            try
            {
                if (IsDisposed)
                {
                    return false;
                }

                if (InvokeRequired)
                {
                    if (!IsHandleCreated)
                    {
                        return false;
                    }

                    BeginInvoke((MethodInvoker)delegate
                    {
                        ShowOwnedInfoPrompt(message);
                    });
                    return true;
                }

                ShowOwnedInfoPrompt(message);
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected virtual void ShowOwnedWarningPrompt(string message)
        {
            ShowOwnedSingleButtonPrompt(message, UI.ShowWarning);
        }

        protected virtual DialogResult ShowOwnedYesNoPrompt(string message)
        {
            try
            {
                using (var dialog = new OwnedYesNoPromptDialog(PromptDialogTitle, message, Font, Icon))
                {
                    return dialog.ShowDialog(this);
                }
            }
            catch
            {
                return UI.ShowYesNo(message);
            }
        }

        protected void HandleResult(int ret)
        {
            if (ret == 0)
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                ShowOwnedWarningPrompt(ResUI.OperationFailed);
            }
        }

        protected void InitListView(ListView lv, (string name, int width)[] columns, bool multiSelect = true)
        {
            lv.GridLines = true;
            lv.FullRowSelect = true;
            lv.View = View.Details;
            lv.MultiSelect = multiSelect;
            lv.HeaderStyle = ColumnHeaderStyle.Clickable;

            lv.Columns.Clear();
            foreach (var column in columns)
            {
                lv.Columns.Add(column.name, column.width);
            }
        }

        protected int GetLvSelectedIndex(ListView lv, List<int> selectedIndices, string emptyMessage = null)
        {
            int index = -1;
            selectedIndices?.Clear();
            try
            {
                if (lv.SelectedIndices.Count <= 0)
                {
                    ShowOwnedInfoPrompt(emptyMessage ?? ResUI.PleaseSelectRules);
                    return index;
                }

                index = lv.SelectedIndices[0];
                if (selectedIndices != null)
                {
                    foreach (int i in lv.SelectedIndices)
                    {
                        selectedIndices.Add(i);
                    }
                }
                return index;
            }
            catch
            {
                return index;
            }
        }

        private void LoadCustomIcon()
        {
            try
            {
                string file = Utils.GetPath(Global.CustomIconName);
                if (System.IO.File.Exists(file))
                {
                    Icon = new System.Drawing.Icon(file);
                    return;
                }

                Icon = Properties.Resources.NotifyIcon1;
            }
            catch (Exception e)
            {
                Utils.SaveLog($"Loading custom icon failed: {e.Message}");
            }
        }

    }
}
