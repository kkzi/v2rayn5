using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using v2rayN.Mode;
using v2rayN.Resx;

namespace v2rayN.Forms
{
    public partial class BaseForm : Form
    {
        protected static Config config;
        private static readonly Lazy<Font> appFont = new Lazy<Font>(CreateAppFont);

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
