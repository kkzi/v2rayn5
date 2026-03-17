using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using v2rayN.Mode;
using v2rayN.Resx;
using System.Linq;

namespace v2rayN.Forms
{
    public partial class BaseForm : Form
    {
        protected static Config config;
        private static readonly Lazy<Font> appFont = new Lazy<Font>(CreateAppFont);
        private const int DialogBottomButtonHeight = 32;
        private bool bottomButtonsNormalized_;

        public BaseForm()
        {
            InitializeComponent();

            // Set app-wide UI font (one place only).
            // Do it after InitializeComponent and switch autoscaling away from Font, otherwise many forms will
            // scale their size/padding unexpectedly.
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = appFont.Value;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            LoadCustomIcon();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            NormalizeDialogBottomButtonsOnce();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
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

        private static Font CreateAppFont()
        {
            // Prefer Microsoft YaHei 9pt. Fall back to default if unavailable.
            try { return new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point); } catch { }
            try { return new Font("Microsoft YaHei", 9F, FontStyle.Regular, GraphicsUnit.Point); } catch { }
            return Control.DefaultFont;
        }

        protected void CloseCancel()
        {
            DialogResult = DialogResult.Cancel;
        }

        protected void HandleResult(int ret)
        {
            if (ret == 0)
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                UI.ShowWarning(ResUI.OperationFailed);
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
                    UI.Show(emptyMessage ?? ResUI.PleaseSelectRules);
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
