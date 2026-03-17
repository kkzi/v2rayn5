using NHotkey;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using v2rayN.Resx;
using v2rayN.Base;
using v2rayN.Handler;
using v2rayN.Mode;
using v2rayN.Tool;

namespace v2rayN.Forms
{
    public partial class MainForm : BaseForm
    {
        private const string UnsubscribedTabId = "__unsubscribed__";

        private V2rayHandler v2rayHandler;
        private List<VmessItem> lstSelecteds = new List<VmessItem>();
        private StatisticsHandler statistics;
        private List<VmessItem> lstVmess;
        private string _subId = UnsubscribedTabId;
        private string serverFilter = string.Empty;
        private bool _isLogHidden;
        private int logPanelSplitterDistance;
        private bool _allowExitOnClose;
        private readonly List<Image> _ownedToolbarImages = new List<Image>();

        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_CLOSE = 0xF060;
        private const int VK_MENU = 0x12;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        #region Window 事件

        public MainForm()
        {
            InitializeComponent();
            // Main window positioning is handled by config restore logic.
            StartPosition = FormStartPosition.Manual;
            // Override BaseForm's FixedSingle to allow resizing the main window.
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new Size(1000, 640);
            mainMsgControl.SysProxySelected += mainMsgControl_SysProxySelected;
            mainMsgControl.RoutingSelected += MainMsgControl_RoutingSelected;
            mainMsgControl.ToggleLogRequested += MainMsgControl_ToggleLogRequested;
            mainMsgControl.OptionSettingRequested += (s, e) => OpenOptionSetting();
            KeyPreview = true;

            ApplyCompactToolStripStyle(tsMain);
            tsMain.ImageScalingSize = new Size(16, 16);
            tsMain.Padding = Padding.Empty;
            foreach (ToolStripItem item in tsMain.Items)
            {
                item.AutoSize = true;
                item.Margin = Padding.Empty;
                item.Padding = new Padding(2, 1, 2, 1);
                if (item is ToolStripDropDownButton ddb)
                {
                    ddb.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                    ddb.TextImageRelation = TextImageRelation.ImageBeforeText;
                }
                else if (item is ToolStripButton btn)
                {
                    btn.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                    btn.TextImageRelation = TextImageRelation.ImageBeforeText;
                }

                // Resource icons are mostly 32x32. ToolStrip will downscale to 16x16 with suboptimal quality.
                // Pre-scale once using high-quality interpolation and disable further scaling to avoid blur.
                NormalizeToolStripItemImage(item, tsMain.ImageScalingSize);
            }

            Text = Utils.GetVersion();
            Global.processJob = new Job();

            Application.ApplicationExit += (sender, args) =>
            {
                MyAppExit(false);
            };

            FormClosed += (s, e) =>
            {
                try
                {
                    foreach (var img in _ownedToolbarImages)
                    {
                        img?.Dispose();
                    }
                    _ownedToolbarImages.Clear();
                }
                catch { }
            };

            // Avoid QR panel flashing during startup (before config/UI restore).
            try { scServers.Panel2Collapsed = true; } catch { }
        }

        private void NormalizeToolStripItemImage(ToolStripItem item, Size targetSize)
        {
            try
            {
                if (item == null || item.Image == null)
                {
                    return;
                }

                // Only resample when needed.
                if (item.Image.Width == targetSize.Width && item.Image.Height == targetSize.Height)
                {
                    item.ImageScaling = ToolStripItemImageScaling.None;
                    return;
                }

                var scaled = CreateHighQualityResizedBitmap(item.Image, targetSize);
                if (scaled == null)
                {
                    return;
                }

                item.Image = scaled;
                item.ImageScaling = ToolStripItemImageScaling.None;
                _ownedToolbarImages.Add(scaled);
            }
            catch { }
        }

        private static Bitmap CreateHighQualityResizedBitmap(Image src, Size targetSize)
        {
            if (src == null)
            {
                return null;
            }

            var bmp = new Bitmap(targetSize.Width, targetSize.Height, PixelFormat.Format32bppPArgb);
            try
            {
                bmp.SetResolution(src.HorizontalResolution, src.VerticalResolution);
                using (var g = Graphics.FromImage(bmp))
                using (var attrs = new ImageAttributes())
                {
                    g.Clear(Color.Transparent);
                    g.CompositingMode = CompositingMode.SourceOver;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    // Reduce edge artifacts when downscaling.
                    attrs.SetWrapMode(WrapMode.TileFlipXY);

                    g.DrawImage(
                        src,
                        new Rectangle(0, 0, targetSize.Width, targetSize.Height),
                        0,
                        0,
                        src.Width,
                        src.Height,
                        GraphicsUnit.Pixel,
                        attrs);
                }
                return bmp;
            }
            catch
            {
                bmp.Dispose();
                return null;
            }
        }

        private static void ApplyCompactToolStripStyle(ToolStrip toolStrip)
        {
            if (toolStrip == null) return;

            toolStrip.Padding = new Padding(0);

            foreach (ToolStripItem item in toolStrip.Items)
            {
                if (item == null) continue;

                item.Margin = new Padding(0);

                if (item is ToolStripSeparator)
                {
                    item.Margin = new Padding(2, 0, 2, 0);
                    continue;
                }

                if (item.DisplayStyle == ToolStripItemDisplayStyle.ImageAndText)
                {
                    item.Padding = new Padding(2, 1, 2, 1);
                    item.ImageAlign = ContentAlignment.MiddleLeft;
                    item.TextAlign = ContentAlignment.MiddleLeft;
                    item.TextImageRelation = TextImageRelation.ImageBeforeText;
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (ConfigHandler.LoadConfig(ref config) != 0)
            {
                UI.ShowWarning($"Loading GUI configuration file is abnormal,please restart the application{Environment.NewLine}加载GUI配置文件异常,请重启应用");
                Environment.Exit(0);
                return;
            }

            // Apply window size before the form is first shown.
            // Use last saved size from guiNConfig.json, clamped to MinimumSize, to avoid visible resize "jump".
            RestoreWindowSize();

            // Restore main servers listview column widths to defaults (temporarily).
            // This clears persisted column widths to avoid unexpected layout issues after font changes.
            try
            {
                if (config?.uiItem?.mainLvColWidth != null && config.uiItem.mainLvColWidth.Count > 0)
                {
                    config.uiItem.mainLvColWidth.Clear();
                    ConfigHandler.SaveConfig(ref config, false);
                }
            }
            catch { }

            ConfigHandler.InitBuiltinRouting(ref config);
            MainFormHandler.Instance.BackupGuiNConfig(config, true);
            v2rayHandler = new V2rayHandler();
            v2rayHandler.ProcessEvent += v2rayHandler_ProcessEvent;

            if (config.enableStatistics)
            {
                statistics = new StatisticsHandler(config, UpdateStatisticsHandler);
            }
        }

        private void MainForm_VisibleChanged(object sender, EventArgs e)
        {
            if (statistics == null || !statistics.Enable) return;
            statistics.UpdateUI = ((Form)sender).Visible;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            var exitPreferSubId = config?.uiItem?.mainSelectedSubId ?? string.Empty;
            var activePreferSubId = GetActiveServerTabId() ?? string.Empty;

            if (Utils.IsNullOrEmpty(activePreferSubId))
            {
                // No active server: restore last selected tab if possible; otherwise fallback to first tab.
                InitSubView(exitPreferSubId, string.Empty);
            }
            else
            {
                // Prefer active server's tab; fallback to last selected; otherwise fallback to first tab.
                InitSubView(activePreferSubId, exitPreferSubId);
            }
            InitServersView();
            RestoreMainLvColumns();
            RefreshServers();
            RefreshRoutingsMenu();

            if (!config.uiItem.showMainOnStartup)
            {
                HideForm();
            }
            else
            {
                ShowInTaskbar = true;
                if (WindowState == FormWindowState.Minimized)
                {
                    WindowState = FormWindowState.Normal;
                }
            }

            UpdateWindowHwndInRegistry();

            MainFormHandler.Instance.UpdateTask(config, UpdateTaskHandler);
            MainFormHandler.Instance.RegisterGlobalHotkey(config, OnHotkeyHandler, UpdateTaskHandler);

            _ = LoadV2ray();

            if (!Utils.CheckForDotNetVersion())
            {
                UI.ShowWarning(ResUI.NetFrameworkRequirementsTip);
                AppendText(false, ResUI.NetFrameworkRequirementsTip);
            }

            logPanelSplitterDistance = scBig.SplitterDistance;
            _isLogHidden = false;
            mainMsgControl.SetLogToggleState(true);
            mainMsgControl.SetLogTextVisible(true);

            lvServers.Focus();
        }

        private string GetActiveServerTabId()
        {
            try
            {
                var active = config?.vmess?.FirstOrDefault(it => it != null && it.indexId == config.indexId);
                if (active == null)
                {
                    return string.Empty;
                }
                return Utils.IsNullOrEmpty(active.subid) ? UnsubscribedTabId : active.subid;
            }
            catch { }
            return string.Empty;
        }

        private void UpdateWindowHwndInRegistry()
        {
            try
            {
                if (IsHandleCreated)
                {
                    Utils.RegWriteValue(Global.MyRegPath, Utils.WindowHwndKey, Convert.ToString((long)Handle));
                }
            }
            catch { }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt && !e.Control && !e.Shift)
            {
                if (TrySwitchToTabByAltDigit(e.KeyCode))
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
                if (TryHandleAltPingShortcuts(e))
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }
            }

            // Main form shortcuts (KeyPreview = true):
            // - Ctrl+V: smart import from clipboard (subscription or server)
            // - Ctrl+R / F5: update current subscription group (ignore Unsubscribed)
            // - Ctrl+Shift+R / Ctrl+F5: update all subscription groups
            if (TryHandleMainFormShortcuts(e))
            {
                return;
            }

            if (e.Control && e.KeyCode == Keys.F)
            {
                FocusServerFilter();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        protected override void WndProc(ref Message m)
        {
            try
            {
                if (m.Msg == WM_SYSCOMMAND)
                {
                    int cmd = (m.WParam.ToInt32() & 0xFFF0);
                    if (cmd == SC_CLOSE)
                    {
                        if (!_allowExitOnClose && IsAltKeyDown())
                        {
                            if (MessageBox.Show(this, ResUI.ExitProgramMessage, ResUI.ExitProgramTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                _allowExitOnClose = true;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                }
            }
            catch { }

            base.WndProc(ref m);
        }

        private static bool IsAltKeyDown()
        {
            try
            {
                return (GetAsyncKeyState(VK_MENU) & 0x8000) != 0;
            }
            catch { }
            return (Control.ModifierKeys & Keys.Alt) == Keys.Alt;
        }

        private bool TryHandleAltPingShortcuts(KeyEventArgs e)
        {
            if (e == null)
            {
                return false;
            }
            if (!e.Alt || e.Control || e.Shift)
            {
                return false;
            }

            switch (e.KeyCode)
            {
                case Keys.P:
                    menuPingServer_Click(null, null);
                    return true;
                case Keys.T:
                    menuTcpingServer_Click(null, null);
                    return true;
                case Keys.R:
                    menuRealPingServer_Click(null, null);
                    return true;
                default:
                    return false;
            }
        }

        private bool TryHandleMainFormShortcuts(KeyEventArgs e)
        {
            if (e == null)
            {
                return false;
            }

            // Ctrl+V: only intercept when focused control is not editable (per requirement).
            if (e.Control && !e.Shift && !e.Alt && e.KeyCode == Keys.V)
            {
                if (IsEditableControlFocused())
                {
                    return false;
                }

                if (TryHandleClipboardImport())
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return true;
                }
                return false;
            }

            // Update subscription shortcuts.
            // Current: Ctrl+R or F5
            if (!e.Alt && !e.Shift && e.Control && e.KeyCode == Keys.R)
            {
                TryUpdateCurrentSubscriptionByShortcut();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return true;
            }
            if (!e.Alt && !e.Control && !e.Shift && e.KeyCode == Keys.F5)
            {
                TryUpdateCurrentSubscriptionByShortcut();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return true;
            }

            // All: Ctrl+Shift+R or Ctrl+F5
            if (!e.Alt && e.Control && e.Shift && e.KeyCode == Keys.R)
            {
                UpdateAllSubscriptionsByShortcut();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return true;
            }
            if (!e.Alt && e.Control && e.KeyCode == Keys.F5)
            {
                UpdateAllSubscriptionsByShortcut();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return true;
            }

            return false;
        }

        private void TryUpdateCurrentSubscriptionByShortcut()
        {
            // Ignore "Unsubscribed" tab silently.
            if (_subId == UnsubscribedTabId)
            {
                return;
            }
            UpdateSubscriptionProcess(_subId, IsProxyEnabledForSubscriptionUpdate());
        }

        private void UpdateAllSubscriptionsByShortcut()
        {
            UpdateSubscriptionProcess("", IsProxyEnabledForSubscriptionUpdate());
        }

        private bool IsProxyEnabledForSubscriptionUpdate()
        {
            try
            {
                return config != null && config.sysProxyType == ESysProxyType.ForcedChange;
            }
            catch { }
            return false;
        }

        private bool TryHandleClipboardImport()
        {
            try
            {
                var clipboardData = Utils.GetClipboardData();
                var lines = GetNonEmptyLines(clipboardData);
                if (lines == null || lines.Count <= 0)
                {
                    return false;
                }

                // Prefer server import when any share urls exist in clipboard.
                if (lines.Any(IsServerShareUrl) || lines.Any(LooksLikeOtherServerShareUrl))
                {
                    return TryImportServersFromClipboard(clipboardData);
                }
                // One or more subscription urls: add/switch and update.
                if (lines.Any(IsHttpUrl))
                {
                    return TryAddOrSwitchSubscriptionGroupsAndUpdate(lines.Where(IsHttpUrl).ToList());
                }

                return false;
            }
            catch (Exception ex)
            {
                Utils.SaveLog("TryHandleClipboardImport", ex);
                return false;
            }
        }

        private bool TryImportServersFromClipboard(string clipboardData)
        {
            string data = (clipboardData ?? string.Empty).TrimEx();
            if (Utils.IsNullOrEmpty(data))
            {
                return false;
            }

            try
            {
                int ret = ConfigHandler.AddBatchServers(ref config, data, "");
                if (ret > 0)
                {
                    InitSubView(_subId);
                    SelectUnsubscribedTab();
                    RefreshServers();
                    UI.Show(string.Format(ResUI.SuccessfullyImportedServerViaClipboard, ret));
                    return true;
                }

                // Keep this message consistent with ShareHandler failure semantics.
                UI.ShowWarning(ResUI.NonvmessOrssProtocol);
                return true;
            }
            catch { }
            return false;
        }

        private static List<string> GetNonEmptyLines(string text)
        {
            var result = new List<string>();
            if (Utils.IsNullOrEmpty(text))
            {
                return result;
            }
            try
            {
                var parts = text
                    .Replace("\r\n", "\n")
                    .Replace("\r", "\n")
                    .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var p in parts)
                {
                    var line = (p ?? string.Empty).TrimEx();
                    if (!Utils.IsNullOrEmpty(line))
                    {
                        result.Add(line);
                    }
                }
            }
            catch { }
            return result;
        }

        private static bool IsHttpUrl(string text)
        {
            if (Utils.IsNullOrEmpty(text))
            {
                return false;
            }
            return text.StartsWith(Global.httpProtocol, StringComparison.OrdinalIgnoreCase)
                || text.StartsWith(Global.httpsProtocol, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsServerShareUrl(string text)
        {
            if (Utils.IsNullOrEmpty(text))
            {
                return false;
            }

            // Keep this aligned with ShareHandler.ImportFromClipboardConfig support.
            return text.StartsWith(Global.vmessProtocol, StringComparison.OrdinalIgnoreCase)
                || text.StartsWith(Global.vlessProtocol, StringComparison.OrdinalIgnoreCase)
                || text.StartsWith(Global.ssProtocol, StringComparison.OrdinalIgnoreCase)
                || text.StartsWith(Global.socksProtocol, StringComparison.OrdinalIgnoreCase)
                || text.StartsWith(Global.trojanProtocol, StringComparison.OrdinalIgnoreCase);
        }

        private static bool LooksLikeOtherServerShareUrl(string text)
        {
            if (Utils.IsNullOrEmpty(text))
            {
                return false;
            }

            // Generic scheme://... but exclude http(s) which are treated as subscription urls.
            if (IsHttpUrl(text))
            {
                return false;
            }

            return Regex.IsMatch(text.TrimEx(), @"^[a-z][a-z0-9+\.-]*://", RegexOptions.IgnoreCase);
        }

        private bool TryAddOrSwitchSubscriptionGroupsAndUpdate(List<string> urls)
        {
            if (urls == null || urls.Count <= 0)
            {
                return false;
            }

            string lastSubId = string.Empty;
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var u in urls)
            {
                if (!IsHttpUrl(u))
                {
                    continue;
                }
                var normalizedUrl = (u ?? string.Empty).TrimEx();
                if (Utils.IsNullOrEmpty(normalizedUrl) || visited.Contains(normalizedUrl))
                {
                    continue;
                }
                visited.Add(normalizedUrl);

                lastSubId = TryAddOrSwitchSubscriptionGroup(normalizedUrl);
                if (!Utils.IsNullOrEmpty(lastSubId))
                {
                    UpdateSubscriptionProcess(lastSubId, IsProxyEnabledForSubscriptionUpdate());
                }
            }

            if (!Utils.IsNullOrEmpty(lastSubId))
            {
                InitSubView(lastSubId);
                RefreshServers();
                return true;
            }

            return false;
        }

        private string TryAddOrSwitchSubscriptionGroup(string url)
        {
            if (config == null)
            {
                return string.Empty;
            }

            var normalizedUrl = (url ?? string.Empty).TrimEx();
            if (Utils.IsNullOrEmpty(normalizedUrl))
            {
                return string.Empty;
            }

            // Existing: do not add, just switch.
            var existing = config.subItem?.FirstOrDefault(it =>
                it != null
                && !Utils.IsNullOrEmpty(it.url)
                && string.Equals(it.url.TrimEx(), normalizedUrl, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                if (Utils.IsNullOrEmpty(existing.id))
                {
                    ConfigHandler.SaveSubItem(ref config);
                }
                return existing.id;
            }

            var host = ExtractHostOrIpFromUrl(normalizedUrl);
            if (Utils.IsNullOrEmpty(host))
            {
                host = "import sub";
            }

            if (config.subItem == null)
            {
                config.subItem = new List<SubItem>();
            }

            var newSub = new SubItem
            {
                id = string.Empty,
                remarks = host,
                url = normalizedUrl,
                enabled = true
            };
            config.subItem.Add(newSub);
            ConfigHandler.SaveSubItem(ref config);
            return newSub.id;
        }

        private static string ExtractHostOrIpFromUrl(string url)
        {
            if (Utils.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            try
            {
                if (Uri.TryCreate(url.TrimEx(), UriKind.Absolute, out var u))
                {
                    if (u.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                        || u.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                    {
                        return (u.IdnHost ?? string.Empty).TrimEx();
                    }
                }
            }
            catch { }

            try
            {
                // Fallback: https?://<host>...
                var m = Regex.Match(url.TrimEx(), @"^https?://(?<host>[^/?#]+)", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    var host = (m.Groups["host"].Value ?? string.Empty).TrimEx();
                    // Strip userinfo if any: user:pass@host
                    var at = host.LastIndexOf('@');
                    if (at >= 0)
                    {
                        host = host.Substring(at + 1);
                    }
                    // Strip port (IPv6 may be [::1]:port)
                    if (host.StartsWith("[") && host.Contains("]"))
                    {
                        var end = host.IndexOf(']');
                        return host.Substring(1, end - 1);
                    }
                    var colon = host.LastIndexOf(':');
                    if (colon > 0)
                    {
                        return host.Substring(0, colon);
                    }
                    return host;
                }
            }
            catch { }

            return string.Empty;
        }

        private bool IsEditableControlFocused()
        {
            try
            {
                var focused = GetDeepFocusedControl(this);
                if (focused == null)
                {
                    return false;
                }

                if (focused is TextBoxBase)
                {
                    return true;
                }

                if (focused is ComboBox cb)
                {
                    // DropDownList is not editable; others are.
                    return cb.DropDownStyle != ComboBoxStyle.DropDownList;
                }

                return false;
            }
            catch { }
            return false;
        }

        private static Control GetDeepFocusedControl(Control root)
        {
            if (root == null)
            {
                return null;
            }

            try
            {
                Control c = root;
                while (c is ContainerControl container && container.ActiveControl != null)
                {
                    c = container.ActiveControl;
                }
                return c;
            }
            catch { }
            return root;
        }

        private bool TrySwitchToTabByAltDigit(Keys keyCode)
        {
            int index = -1;
            if (keyCode >= Keys.D1 && keyCode <= Keys.D9)
            {
                index = (int)keyCode - (int)Keys.D1;
            }
            else if (keyCode >= Keys.NumPad1 && keyCode <= Keys.NumPad9)
            {
                index = (int)keyCode - (int)Keys.NumPad1;
            }

            if (index < 0)
            {
                return false;
            }
            if (tabGroup == null || tabGroup.TabPages == null)
            {
                return false;
            }
            if (index >= tabGroup.TabPages.Count)
            {
                return false;
            }

            tabGroup.SelectedIndex = index;
            return true;
        }

        private void FocusServerFilter()
        {
            try
            {
                txtServerFilter.Focus();
                txtServerFilter.SelectAll();
            }
            catch { }
        }

        private void MainMsgControl_ToggleLogRequested(object sender, EventArgs e)
        {
            ToggleLogPanel();
        }

        private void ToggleLogPanel()
        {
            if (_isLogHidden)
            {
                scBig.SplitterDistance = logPanelSplitterDistance > 0 ? logPanelSplitterDistance : 200;
                _isLogHidden = false;
            }
            else
            {
                logPanelSplitterDistance = scBig.SplitterDistance;
                int statusBarHeight = 22;
                scBig.SplitterDistance = scBig.Height - statusBarHeight;
                _isLogHidden = true;
            }
            mainMsgControl.SetLogToggleState(!_isLogHidden);
            mainMsgControl.SetLogTextVisible(!_isLogHidden);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            switch (e.CloseReason)
            {
                case CloseReason.UserClosing:
                    if (_allowExitOnClose)
                    {
                        e.Cancel = false;
                    }
                    else
                    {
                        StorageUI();
                        e.Cancel = true;
                        HideForm();
                    }
                    break;
                case CloseReason.ApplicationExitCall:
                case CloseReason.FormOwnerClosing:
                case CloseReason.TaskManagerClosing:
                    MyAppExit(false);
                    break;
                case CloseReason.WindowsShutDown:
                    MyAppExit(true);
                    break;
            }
        }
        private void MyAppExit(bool blWindowsShutDown)
        {
            try
            {
                Utils.SaveLog("MyAppExit Begin");

                StorageUI();
                ConfigHandler.SaveConfig(ref config);

                //HttpProxyHandle.CloseHttpAgent(config);
                if (blWindowsShutDown)
                {
                    SysProxyHandle.ResetIEProxy4WindowsShutDown();
                }
                else
                {
                    SysProxyHandle.UpdateSysProxy(config, true);
                }

                statistics?.SaveToFile();
                statistics?.Close();

                v2rayHandler.V2rayStop();
                Utils.SaveLog("MyAppExit End");
            }
            catch { }
        }

        private void RestoreUI()
        {
            // Legacy wrapper: keep for compatibility if other call sites exist.
            RestoreMainLvColumns();
        }

        private void RestoreWindowSize()
        {
            try
            {
                if (config?.uiItem == null)
                {
                    return;
                }

                if (config.uiItem.mainSize.IsEmpty)
                {
                    return;
                }

                int w = config.uiItem.mainSize.Width;
                int h = config.uiItem.mainSize.Height;

                if (!MinimumSize.IsEmpty)
                {
                    w = Math.Max(w, MinimumSize.Width);
                    h = Math.Max(h, MinimumSize.Height);
                }

                // Apply once before the form is shown to avoid visible "jump".
                Size = new Size(w, h);
            }
            catch { }
        }

        private void RestoreMainLvColumns()
        {
            try
            {
                if (config?.uiItem == null)
                {
                    return;
                }

                for (int k = 0; k < lvServers.Columns.Count; k++)
                {
                    var key = lvServers.Columns[k].Name;
                    if (Utils.IsNullOrEmpty(key))
                    {
                        key = ((EServerColName)k).ToString();
                    }
                    var width = ConfigHandler.GetformMainLvColWidth(ref config, key, lvServers.Columns[k].Width);
                    lvServers.Columns[k].Width = width;
                }
            }
            catch { }
        }

        private void StorageUI()
        {
            config.uiItem.mainLocation = Location;

            config.uiItem.mainSize = new Size(Width, Height);
            config.uiItem.mainSelectedSubId = _subId;

            for (int k = 0; k < lvServers.Columns.Count; k++)
            {
                var key = lvServers.Columns[k].Name;
                if (Utils.IsNullOrEmpty(key))
                {
                    key = ((EServerColName)k).ToString();
                }
                ConfigHandler.AddformMainLvColWidth(ref config, key, lvServers.Columns[k].Width);
            }
        }

        private void OnHotkeyHandler(object sender, HotkeyEventArgs e)
        {
            switch (Utils.ToInt(e.Name))
            {
                case (int)EGlobalHotkey.ShowForm:
                    if (ShowInTaskbar) HideForm(); else ShowForm();
                    break;
                case (int)EGlobalHotkey.SystemProxyClear:
                    SetListenerType(ESysProxyType.ForcedClear);
                    break;
                case (int)EGlobalHotkey.SystemProxySet:
                    SetListenerType(ESysProxyType.ForcedChange);
                    break;
                case (int)EGlobalHotkey.SystemProxyUnchanged:
                    SetListenerType(ESysProxyType.Unchanged);
                    break;
            }
            e.Handled = true;
        }

        #endregion

        #region 显示服务器 listview 和 menu

        private void txtServerFilter_TextChanged(object sender, EventArgs e)
        {
            try
            {
                serverFilter = (txtServerFilter.Text ?? string.Empty).Trim();
                RefreshServers();
            }
            catch { }
        }

        /// <summary>
        /// 刷新服务器
        /// </summary>
        private void RefreshServers()
        {
            if (EnsureValidSubSelection())
            {
                return;
            }

            lstVmess = config.vmess
                .Where(IsVmessVisibleInCurrentTab)
                .Where(it =>
                    Utils.IsNullOrEmpty(serverFilter)
                    || (it.remarks ?? string.Empty).IndexOf(serverFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(it => it.sort)
                .ToList();

            // Do not change default node when switching tabs/filters.
            ConfigHandler.SetDefaultServer(config, config.vmess);
            BeginInvoke(new Action(() =>
            {
                RefreshServersView();
            }));

            RefreshServersMenu();
        }

        private bool EnsureValidSubSelection()
        {
            try
            {
                if (tabGroup.TabPages.Count <= 0)
                {
                    _subId = UnsubscribedTabId;
                    return false;
                }

                if (Utils.IsNullOrEmpty(_subId))
                {
                    SelectFirstTab();
                    return true;
                }

                if (tabGroup.TabPages.Cast<TabPage>().All(t => t.Name != _subId))
                {
                    SelectFirstTab();
                    return true;
                }
            }
            catch { }
            return false;
        }

        private bool IsVmessVisibleInCurrentTab(VmessItem item)
        {
            if (item == null)
            {
                return false;
            }
            if (_subId == UnsubscribedTabId)
            {
                return Utils.IsNullOrEmpty(item.subid);
            }
            return item.subid == _subId;
        }

        /// <summary>
        /// 初始化服务器列表
        /// </summary>
        private void InitServersView()
        {
            lvServers.BeginUpdate();
            lvServers.Items.Clear();
            lvServers.Columns.Clear();

            lvServers.GridLines = true;
            lvServers.FullRowSelect = true;
            lvServers.View = View.Details;
            lvServers.Scrollable = true;
            lvServers.MultiSelect = true;
            lvServers.HeaderStyle = ColumnHeaderStyle.Clickable;
            lvServers.RegisterDragEvent(UpdateDragEventHandler);

            AddServerColumn(EServerColName.def, string.Empty, 30);
            AddServerColumn(EServerColName.configType, ResUI.LvServiceType, 80);
            AddServerColumn(EServerColName.remarks, ResUI.LvAlias, 100);
            AddServerColumn(EServerColName.address, ResUI.LvAddress, 120);
            AddServerColumn(EServerColName.port, ResUI.LvPort, 100);
            AddServerColumn(EServerColName.security, ResUI.LvEncryptionMethod, 120);
            AddServerColumn(EServerColName.network, ResUI.LvTransportProtocol, 120);
            AddServerColumn(EServerColName.streamSecurity, ResUI.LvTLS, 100);
            AddServerColumn(EServerColName.testResult, ResUI.LvTestResults, 120, HorizontalAlignment.Right);

            if (statistics != null && statistics.Enable)
            {
                AddServerColumn(EServerColName.todayDown, ResUI.LvTodayDownloadDataAmount, 70);
                AddServerColumn(EServerColName.todayUp, ResUI.LvTodayUploadDataAmount, 70);
                AddServerColumn(EServerColName.totalDown, ResUI.LvTotalDownloadDataAmount, 70);
                AddServerColumn(EServerColName.totalUp, ResUI.LvTotalUploadDataAmount, 70);
            }
            lvServers.EndUpdate();
        }

        private ColumnHeader AddServerColumn(EServerColName name, string text, int width, HorizontalAlignment alignment = HorizontalAlignment.Left)
        {
            var col = new ColumnHeader
            {
                Name = name.ToString(),
                Text = text,
                Width = width,
                TextAlign = alignment,
                Tag = name
            };
            lvServers.Columns.Add(col);
            return col;
        }

        private void UpdateDragEventHandler(int index, int targetIndex)
        {
            if (index < 0 || targetIndex < 0)
            {
                return;
            }
            if (ConfigHandler.MoveServer(ref config, ref lstVmess, index, EMove.Position, targetIndex) == 0)
            {
                RefreshServers();
            }
        }

        /// <summary>
        /// 刷新服务器列表
        /// </summary>
        private void RefreshServersView()
        {
            int index = GetLvSelectedIndex(false);

            lvServers.BeginUpdate();
            lvServers.Items.Clear();

            for (int k = 0; k < lstVmess.Count; k++)
            {
                string def = (k + 1).ToString();
                VmessItem item = lstVmess[k];
                if (config.IsActiveNode(item))
                {
                    def = Global.CheckMark;
                }

                ListViewItem lvItem = new ListViewItem(def);
                Utils.AddSubItem(lvItem, EServerColName.configType.ToString(), (item.configType).ToString());
                Utils.AddSubItem(lvItem, EServerColName.remarks.ToString(), item.remarks);
                Utils.AddSubItem(lvItem, EServerColName.address.ToString(), item.address);
                Utils.AddSubItem(lvItem, EServerColName.port.ToString(), item.port.ToString());
                Utils.AddSubItem(lvItem, EServerColName.security.ToString(), item.security);
                Utils.AddSubItem(lvItem, EServerColName.network.ToString(), item.network);
                Utils.AddSubItem(lvItem, EServerColName.streamSecurity.ToString(), item.streamSecurity);
                Utils.AddSubItem(lvItem, EServerColName.testResult.ToString(), item.testResult);

                if (statistics != null && statistics.Enable)
                {
                    string totalUp = string.Empty,
                        totalDown = string.Empty,
                        todayUp = string.Empty,
                        todayDown = string.Empty;
                    ServerStatItem sItem = statistics.Statistic.Find(item_ => item_.itemId == item.indexId);
                    if (sItem != null)
                    {
                        totalUp = Utils.HumanFy(sItem.totalUp);
                        totalDown = Utils.HumanFy(sItem.totalDown);
                        todayUp = Utils.HumanFy(sItem.todayUp);
                        todayDown = Utils.HumanFy(sItem.todayDown);
                    }

                    Utils.AddSubItem(lvItem, EServerColName.todayDown.ToString(), todayDown);
                    Utils.AddSubItem(lvItem, EServerColName.todayUp.ToString(), todayUp);
                    Utils.AddSubItem(lvItem, EServerColName.totalDown.ToString(), totalDown);
                    Utils.AddSubItem(lvItem, EServerColName.totalUp.ToString(), totalUp);
                }

                if (k % 2 == 1) // 隔行着色
                {
                    lvItem.BackColor = Color.WhiteSmoke;
                }
                if (config.IsActiveNode(item))
                {
                    //lvItem.Checked = true;
                    lvItem.ForeColor = Color.DodgerBlue;
                    lvItem.Font = new Font(lvItem.Font, FontStyle.Bold);
                }

                if (lvItem != null) lvServers.Items.Add(lvItem);
            }
            lvServers.EndUpdate();

            if (index >= 0 && index < lvServers.Items.Count && lvServers.Items.Count > 0)
            {
                lvServers.Items[index].Selected = true;
                lvServers.SetScrollPosition(index);
            }
        }

        /// <summary>
        /// 刷新托盘服务器菜单
        /// </summary>
        private void RefreshServersMenu()
        {
            menuServers.DropDownItems.Clear();

            if (lstVmess.Count > config.trayMenuServersLimit)
            {
                menuServers.DropDownItems.Add(new ToolStripMenuItem(ResUI.TooManyServersTip));
                return;
            }

            List<ToolStripMenuItem> lst = new List<ToolStripMenuItem>();
            for (int k = 0; k < lstVmess.Count; k++)
            {
                VmessItem item = lstVmess[k];
                string name = item.GetSummary();

                ToolStripMenuItem ts = new ToolStripMenuItem(name)
                {
                    Tag = k
                };
                if (config.IsActiveNode(item))
                {
                    ts.Checked = true;
                }
                ts.Click += ts_Click;
                lst.Add(ts);
            }
            menuServers.DropDownItems.AddRange(lst.ToArray());
            menuServers.Visible = true;
        }

        private void ts_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripItem ts = (ToolStripItem)sender;
                int index = Utils.ToInt(ts.Tag);
                SetDefaultServer(index);
            }
            catch
            {
            }
        }
        private void lvServers_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column < 0)
            {
                return;
            }

            try
            {
                var col = lvServers.Columns[e.Column];
                var colName = (col?.Tag is EServerColName n) ? n : (EServerColName)e.Column;
                if (colName == EServerColName.def)
                {
                    foreach (ColumnHeader it in lvServers.Columns)
                    {
                        it.Width = -2;
                    }
                    return;
                }

                var tag = lvServers.Columns[e.Column].Tag?.ToString();
                bool asc = Utils.IsNullOrEmpty(tag) || !Convert.ToBoolean(tag);
                if (ConfigHandler.SortServers(ref config, ref lstVmess, colName, asc) != 0)
                {
                    return;
                }
                lvServers.Columns[e.Column].Tag = Convert.ToString(asc);
                RefreshServers();
            }
            catch (Exception ex)
            {
                Utils.SaveLog(ex.Message, ex);
            }

        }

        private void InitSubView(string preferSubId)
        {
            InitSubView(preferSubId, string.Empty);
        }

        private void InitSubView(string preferSubId, string fallbackSubId)
        {
            tabGroup.TabPages.Clear();

            // Subscription tabs
            bool needSave = false;
            if (config.subItem != null)
            {
                foreach (var item in config.subItem)
                {
                    if (item == null)
                    {
                        continue;
                    }
                    if (item.enabled == false)
                    {
                        continue;
                    }

                    var subId = (item.id ?? string.Empty).TrimEx();
                    if (Utils.IsNullOrEmpty(subId))
                    {
                        // Ensure tab identity for selection/update logic.
                        subId = Utils.GetGUID(false);
                        item.id = subId;
                        needSave = true;
                    }

                    var tabPage2 = new TabPage($"   {item.remarks}   ") { Name = subId, Tag = item };
                    tabGroup.TabPages.Add(tabPage2);
                }
            }

            // Unsubscribed tab (manual nodes) - keep it as the last tab
            string title = $"  {ResUI.UngroupedServers}   ";
            var tabPage = new TabPage(title) { Name = UnsubscribedTabId };
            tabGroup.TabPages.Add(tabPage);

            if (needSave)
            {
                ConfigHandler.SaveSubItem(ref config);
            }

            if (!Utils.IsNullOrEmpty(preferSubId))
            {
                var preferred = tabGroup.TabPages.Cast<TabPage>().FirstOrDefault(t => t.Name == preferSubId);
                if (preferred != null)
                {
                    tabGroup.SelectedTab = preferred;
                    _subId = preferred.Name;
                    return;
                }
            }

            if (!Utils.IsNullOrEmpty(fallbackSubId))
            {
                var fallback = tabGroup.TabPages.Cast<TabPage>().FirstOrDefault(t => t.Name == fallbackSubId);
                if (fallback != null)
                {
                    tabGroup.SelectedTab = fallback;
                    _subId = fallback.Name;
                    return;
                }
            }

            SelectFirstTab();
        }

        private void tabGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabGroup.SelectedIndex < 0)
            {
                return;
            }
            _subId = tabGroup.SelectedTab?.Name ?? UnsubscribedTabId;
            if (config?.uiItem != null)
            {
                config.uiItem.mainSelectedSubId = _subId;
            }

            RefreshServers();

            lvServers.Focus();
        }

        private void SelectFirstTab()
        {
            if (tabGroup.TabPages.Count <= 0)
            {
                _subId = UnsubscribedTabId;
                return;
            }

            tabGroup.SelectedIndex = 0;
            _subId = tabGroup.SelectedTab?.Name ?? UnsubscribedTabId;
        }


        #endregion

        #region v2ray 操作
        /// <summary>
        /// 载入V2ray
        /// </summary>
        async Task LoadV2ray()
        {
            BeginInvoke(new Action(() =>
            {
                tsbReload.Enabled = false;
            }));

            if (Global.reloadV2ray)
            {
                mainMsgControl.ClearMsg();
            }
            await Task.Run(() =>
            {
                v2rayHandler.LoadV2ray(config);
            });

            Global.reloadV2ray = false;
            ConfigHandler.SaveConfig(ref config, false);
            statistics?.SaveToFile();

            ChangePACButtonStatus(config.sysProxyType);

            BeginInvoke(new Action(() =>
            {
                tsbReload.Enabled = true;
            }));
        }

        /// <summary>
        /// 关闭V2ray
        /// </summary>
        private void CloseV2ray()
        {
            ConfigHandler.SaveConfig(ref config, false);
            statistics?.SaveToFile();

            ChangePACButtonStatus(ESysProxyType.ForcedClear);

            v2rayHandler.V2rayStop();
        }

        #endregion

        #region 功能按钮

        private void lvServers_Click(object sender, EventArgs e)
        {
            int index = GetLvSelectedIndex(false);
            if (index < 0)
            {
                return;
            }
            qrCodeControl.showQRCode(lstVmess[index]);
        }

        private void lvServers_DoubleClick(object sender, EventArgs e)
        {
            int index = GetLvSelectedIndex();
            if (index < 0)
            {
                return;
            }
            ShowServerForm(lstVmess[index].configType, index);
        }
        private void ShowServerForm(EConfigType configType, int index)
        {
            BaseServerForm fm;
            if (configType == EConfigType.Custom)
            {
                fm = new AddServer2Form();
            }
            else
            {
                fm = new AddServerForm();
            }
            fm.vmessItem = index >= 0 ? lstVmess[index] : null;
            fm.eConfigType = configType;
            if (fm.ShowDialog(this) == DialogResult.OK)
            {
                if (index < 0)
                {
                    // Manual nodes should always be placed under the Unsubscribed tab.
                    SelectUnsubscribedTab();
                }
                RefreshServers();
                _ = LoadV2ray();
            }
        }

        private void SelectUnsubscribedTab()
        {
            var tab = tabGroup.TabPages.Cast<TabPage>().FirstOrDefault(t => t.Name == UnsubscribedTabId);
            if (tab != null)
            {
                tabGroup.SelectedTab = tab;
                _subId = UnsubscribedTabId;
            }
        }


        private void lvServers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        menuSelectAll_Click(null, null);
                        break;
                    case Keys.C:
                        menuExport2ShareUrl_Click(null, null);
                        break;
                    case Keys.V:
                        if (TryHandleClipboardImport())
                        {
                            e.Handled = true;
                            e.SuppressKeyPress = true;
                            return;
                        }
                        break;
                    case Keys.P:
                        // moved to Alt+P
                        break;
                    case Keys.O:
                        // moved to Alt+T
                        break;
                    case Keys.R:
                        // Reserved for subscription update on main form (Ctrl+R),
                        // keep list shortcut consistent: do not trigger other actions here.
                        break;
                    case Keys.S:
                        menuScanScreen_Click(null, null);
                        break;
                    case Keys.T:
                        menuSpeedServer_Click(null, null);
                        break;
                    case Keys.F:
                        FocusServerFilter();
                        break;
                    case Keys.E:
                        menuSortServerResult_Click(null, null);
                        break;
                }
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        menuSetDefaultServer_Click(null, null);
                        break;
                    case Keys.Delete:
                        menuRemoveServer_Click(null, null);
                        break;
                    case Keys.T:
                        menuMoveTop_Click(null, null);
                        break;
                    case Keys.B:
                        menuMoveBottom_Click(null, null);
                        break;
                    case Keys.U:
                        menuMoveUp_Click(null, null);
                        break;
                    case Keys.D:
                        menuMoveDown_Click(null, null);
                        break;
                }
            }
        }

        private void menuAddVmessServer_Click(object sender, EventArgs e)
        {
            ShowServerForm(EConfigType.VMess, -1);
        }

        private void menuAddVlessServer_Click(object sender, EventArgs e)
        {
            ShowServerForm(EConfigType.VLESS, -1);
        }

        private void menuRemoveServer_Click(object sender, EventArgs e)
        {

            int index = GetLvSelectedIndex();
            if (index < 0)
            {
                return;
            }
            if (UI.ShowYesNo(ResUI.RemoveServer) == DialogResult.No)
            {
                return;
            }

            ConfigHandler.RemoveServer(config, lstSelecteds);

            RefreshServers();
            _ = LoadV2ray();
        }

        private void menuRemoveDuplicateServer_Click(object sender, EventArgs e)
        {
            int oldCount = lstVmess.Count;
            int newCount = ConfigHandler.DedupServerList(ref config, ref lstVmess);
            RefreshServers();
            _ = LoadV2ray();
            UI.Show(string.Format(ResUI.RemoveDuplicateServerResult, oldCount, newCount));
        }

        private void menuCopyServer_Click(object sender, EventArgs e)
        {
            int index = GetLvSelectedIndex();
            if (index < 0)
            {
                return;
            }
            if (ConfigHandler.CopyServer(ref config, lstSelecteds) == 0)
            {
                RefreshServers();
            }
        }

        private void menuSetDefaultServer_Click(object sender, EventArgs e)
        {
            int index = GetLvSelectedIndex();
            if (index < 0)
            {
                return;
            }
            SetDefaultServer(index);
        }

        private void menuPingServer_Click(object sender, EventArgs e)
        {
            Speedtest(ESpeedActionType.Ping);
        }
        private void menuTcpingServer_Click(object sender, EventArgs e)
        {
            Speedtest(ESpeedActionType.Tcping);
        }

        private void menuRealPingServer_Click(object sender, EventArgs e)
        {
            Speedtest(ESpeedActionType.Realping);
        }

        private void menuSpeedServer_Click(object sender, EventArgs e)
        {
            Speedtest(ESpeedActionType.Speedtest);
        }
        private void Speedtest(ESpeedActionType actionType)
        {
            if (GetLvSelectedIndex() < 0) return;
            ClearTestResult();
            SpeedtestHandler statistics = new SpeedtestHandler(config, v2rayHandler, lstSelecteds, actionType, UpdateSpeedtestHandler);
        }
        private void menuSortServerResult_Click(object sender, EventArgs e)
        {
            lvServers_ColumnClick(null, new ColumnClickEventArgs((int)EServerColName.testResult));
        }

        private void tsbTestMe_Click(object sender, EventArgs e)
        {
            var updateHandle = new UpdateHandle();
            updateHandle.RunAvailabilityCheck(UpdateTaskHandler);
        }

        private void menuClearStatistic_Click(object sender, EventArgs e)
        {
            if (statistics != null)
            {
                statistics.ClearAllServerStatistics();
                RefreshServers();
            }
        }

        private void menuExport2ClientConfig_Click(object sender, EventArgs e)
        {
            int index = GetLvSelectedIndex();
            if (index < 0)
            {
                return;
            }
            MainFormHandler.Instance.Export2ClientConfig(lstVmess[index], config);
        }

        private void menuExport2ServerConfig_Click(object sender, EventArgs e)
        {
            int index = GetLvSelectedIndex();
            if (index < 0)
            {
                return;
            }
            MainFormHandler.Instance.Export2ServerConfig(lstVmess[index], config);
        }

        private string BuildShareUrlsFromSelected()
        {
            GetLvSelectedIndex();
            var sb = new StringBuilder();
            foreach (var it in lstSelecteds)
            {
                string url = ShareHandler.GetShareUrl(it);
                if (!Utils.IsNullOrEmpty(url))
                {
                    sb.Append(url);
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        private void menuExport2ShareUrl_Click(object sender, EventArgs e)
        {
            string urls = BuildShareUrlsFromSelected();
            if (urls.Length > 0)
            {
                Utils.SetClipboardData(urls);
                AppendText(false, ResUI.BatchExportURLSuccessfully);
            }
        }

        private void menuExport2SubContent_Click(object sender, EventArgs e)
        {
            string urls = BuildShareUrlsFromSelected();
            if (urls.Length > 0)
            {
                Utils.SetClipboardData(Utils.Base64Encode(urls));
                UI.Show(ResUI.BatchExportSubscriptionSuccessfully);
            }
        }

        private void OpenOptionSetting()
        {
            var fm = new OptionSettingForm();
            if (fm.ShowDialog(this) == DialogResult.OK)
            {
                RefreshServers();
                _ = LoadV2ray();
            }
        }

        private void tsbOptionSetting_Click(object sender, EventArgs e)
        {
            OpenOptionSetting();
        }

        private void tsbRoutingSetting_Click(object sender, EventArgs e)
        {
            var fm = new RoutingSettingForm();
            if (fm.ShowDialog(this) == DialogResult.OK)
            {
                RefreshRoutingsMenu();
                RefreshServers();
                _ = LoadV2ray();
            }
        }

        private void tsbGlobalHotkeySetting_Click(object sender, EventArgs e)
        {
            var fm = new GlobalHotkeySettingForm();
            fm.ShowDialog(this);
        }

        private void tsbReload_Click(object sender, EventArgs e)
        {
            Global.reloadV2ray = true;
            _ = LoadV2ray();
        }

        /// <summary>
        /// 设置活动服务器
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private int SetDefaultServer(int index)
        {
            if (index < 0)
            {
                UI.Show(ResUI.PleaseSelectServer);
                return -1;
            }
            if (ConfigHandler.SetDefaultServer(ref config, lstVmess[index]) == 0)
            {
                //RefreshServers();
                var boldFont = new Font(lvServers.Font, FontStyle.Bold);
                var regularFont = new Font(lvServers.Font, FontStyle.Regular);
                for (int k = 0; k < lstVmess.Count; k++)
                {
                    if (config.IsActiveNode(lstVmess[k]))
                    {
                        lvServers.Items[k].SubItems[0].Text = Global.CheckMark;
                        lvServers.Items[k].ForeColor = Color.DodgerBlue;
                        lvServers.Items[k].Font = boldFont;
                    }
                    else
                    {
                        lvServers.Items[k].SubItems[0].Text = (k + 1).ToString();
                        lvServers.Items[k].ForeColor = lvServers.ForeColor;
                        lvServers.Items[k].Font = regularFont;
                    }
                }
                RefreshServersMenu();
                _ = LoadV2ray();
            }
            return 0;
        }

        /// <summary>
        /// 取得ListView选中的行
        /// </summary>
        /// <returns></returns>
        private int GetLvSelectedIndex(bool show = true)
        {
            int index = -1;
            lstSelecteds.Clear();
            try
            {
                if (lvServers.SelectedIndices.Count <= 0)
                {
                    if (show)
                    {
                        UI.Show(ResUI.PleaseSelectServer);
                    }
                    return index;
                }

                index = lvServers.SelectedIndices[0];
                foreach (int i in lvServers.SelectedIndices)
                {
                    lstSelecteds.Add(lstVmess[i]);
                }
                return index;
            }
            catch
            {
                return index;
            }
        }

        private void menuAddCustomServer_Click(object sender, EventArgs e)
        {
            ShowServerForm(EConfigType.Custom, -1);
        }

        private void menuAddShadowsocksServer_Click(object sender, EventArgs e)
        {
            ShowServerForm(EConfigType.Shadowsocks, -1);
            ShowForm();
        }

        private void menuAddSocksServer_Click(object sender, EventArgs e)
        {
            ShowServerForm(EConfigType.Socks, -1);
            ShowForm();
        }

        private void menuAddTrojanServer_Click(object sender, EventArgs e)
        {
            ShowServerForm(EConfigType.Trojan, -1);
            ShowForm();
        }

        private void menuAddServers_Click(object sender, EventArgs e)
        {
            string clipboardData = Utils.GetClipboardData();
            int ret = ConfigHandler.AddBatchServers(ref config, clipboardData, "");
            if (ret > 0)
            {
                InitSubView(_subId);
                SelectUnsubscribedTab();
                RefreshServers();
                UI.Show(string.Format(ResUI.SuccessfullyImportedServerViaClipboard, ret));
            }
        }

        private void menuScanScreen_Click(object sender, EventArgs e)
        {
            _ = ScanScreenTaskAsync();
        }

        public async Task ScanScreenTaskAsync()
        {
            HideForm();

            string result = await Task.Run(() =>
            {
                return Utils.ScanScreen();
            });

            ShowForm();

            if (Utils.IsNullOrEmpty(result))
            {
                UI.ShowWarning(ResUI.NoValidQRcodeFound);
            }
            else
            {
                int ret = ConfigHandler.AddBatchServers(ref config, result, "");
                if (ret > 0)
                {
                    InitSubView(_subId);
                    SelectUnsubscribedTab();
                    RefreshServers();
                    UI.Show(ResUI.SuccessfullyImportedServerViaScan);
                }
            }
        }

        private void menuUpdateSubscriptions_Click(object sender, EventArgs e)
        {
            UpdateSubscriptionProcess("", false);
        }
        private void menuUpdateSubViaProxy_Click(object sender, EventArgs e)
        {
            UpdateSubscriptionProcess("", true);
        }

        private void tsbBackupGuiNConfig_Click(object sender, EventArgs e)
        {
            MainFormHandler.Instance.BackupGuiNConfig(config);
        }

        private void tsbRestoreGuiNConfig_Click(object sender, EventArgs e)
        {
            if (MainFormHandler.Instance.RestoreGuiNConfig(ref config))
            {
                InitSubView(_subId);
                RefreshServers();
            }
        }
        #endregion


        #region 提示信息

        /// <summary>
        /// 消息委托
        /// </summary>
        /// <param name="notify"></param>
        /// <param name="msg"></param>
        void v2rayHandler_ProcessEvent(bool notify, string msg)
        {
            AppendText(notify, msg);
        }

        void AppendText(bool notify, string msg)
        {
            try
            {
                mainMsgControl.AppendText(msg);
                if (notify)
                {
                    notifyMsg(msg);
                }
            }
            catch { }
        }

        /// <summary>
        /// 托盘信息
        /// </summary>
        /// <param name="msg"></param>
        private void notifyMsg(string msg)
        {
            notifyMain.Text = (msg.Length <= 63 ? msg : msg.Substring(0, 63));
        }

        #endregion


        #region 托盘事件

        private void notifyMain_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowForm();
            }
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            Visible = false;
            Close();

            Application.Exit();
        }


        private void ShowForm()
        {
            Show();
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }
            Activate();
            ShowInTaskbar = true;
            UpdateWindowHwndInRegistry();
            //this.notifyIcon1.Visible = false;
            mainMsgControl.ScrollToCaret();

            int index = GetLvSelectedIndex(false);
            if (index >= 0 && index < lvServers.Items.Count && lvServers.Items.Count > 0)
            {
                lvServers.Items[index].Selected = true;
                lvServers.SetScrollPosition(index);
            }

            SetVisibleCore(true);
        }

        private void HideForm()
        {
            Hide();
            notifyMain.Visible = true;
            ShowInTaskbar = false;

            SetVisibleCore(false);
            UpdateWindowHwndInRegistry();
        }

        #endregion

        #region 后台测速
        private void SetTestResult(string indexId, string txt)
        {
            int k = lstVmess.FindIndex(it => it.indexId == indexId);
            if (k >= 0 && k < lvServers.Items.Count)
            {
                lstVmess[k].testResult = txt;
                lvServers.Items[k].SubItems["testResult"].Text = txt;
            }
            else
            {
                AppendText(false, txt);
            }
        }
        private void SetTestResult(int k, string txt)
        {
            if (k < lvServers.Items.Count)
            {
                lstVmess[k].testResult = txt;
                lvServers.Items[k].SubItems["testResult"].Text = txt;
            }
        }
        private void ClearTestResult()
        {
            foreach (var it in lstSelecteds)
            {
                SetTestResult(it.indexId, "");
            }
        }
        private void UpdateSpeedtestHandler(string indexId, string msg)
        {
            lvServers.Invoke((MethodInvoker)delegate
            {
                SetTestResult(indexId, msg);
            });
        }

        private void UpdateStatisticsHandler(ulong up, ulong down, List<ServerStatItem> statistics)
        {
            try
            {
                up /= (ulong)(config.statisticsFreshRate);
                down /= (ulong)(config.statisticsFreshRate);
                mainMsgControl.SetToolSslInfo("speed", string.Format("{0}/s↑ | {1}/s↓", Utils.HumanFy(up), Utils.HumanFy(down)));

                lvServers.Invoke((MethodInvoker)delegate
                {
                    lvServers.BeginUpdate();
                    foreach (var it in statistics)
                    {
                        int index = lstVmess.FindIndex(item => item.indexId == it.itemId);
                        if (index < 0) continue;
                        lvServers.Items[index].SubItems["todayDown"].Text = Utils.HumanFy(it.todayDown);
                        lvServers.Items[index].SubItems["todayUp"].Text = Utils.HumanFy(it.todayUp);
                        lvServers.Items[index].SubItems["totalDown"].Text = Utils.HumanFy(it.totalDown);
                        lvServers.Items[index].SubItems["totalUp"].Text = Utils.HumanFy(it.totalUp);
                    }
                    lvServers.EndUpdate();
                });

            }
            catch (Exception ex)
            {
                Utils.SaveLog(ex.Message, ex);
            }
        }

        private async void UpdateTaskHandler(bool success, string msg)
        {
            AppendText(false, msg);
            if (success)
            {
                RefreshServers();
                Global.reloadV2ray = true;
                await LoadV2ray();
            }
        }
        #endregion

        #region 移动服务器

        private void menuMoveTop_Click(object sender, EventArgs e)
        {
            MoveServer(EMove.Top);
        }

        private void menuMoveUp_Click(object sender, EventArgs e)
        {
            MoveServer(EMove.Up);
        }

        private void menuMoveDown_Click(object sender, EventArgs e)
        {
            MoveServer(EMove.Down);
        }

        private void menuMoveBottom_Click(object sender, EventArgs e)
        {
            MoveServer(EMove.Bottom);
        }

        private void MoveServer(EMove eMove)
        {
            int index = GetLvSelectedIndex();
            if (index < 0)
            {
                return;
            }
            if (ConfigHandler.MoveServer(ref config, ref lstVmess, index, eMove) == 0)
            {
                //TODO: reload is not good.
                RefreshServers();
                //LoadV2ray();
            }
        }
        private void menuSelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvServers.Items)
            {
                item.Selected = true;
            }
        }
        #endregion

        #region 系统代理相关
        private void menuKeepClear_Click(object sender, EventArgs e)
        {
            SetListenerType(ESysProxyType.ForcedClear);
        }
        private void menuGlobal_Click(object sender, EventArgs e)
        {
            SetListenerType(ESysProxyType.ForcedChange);
        }

        private void menuKeepNothing_Click(object sender, EventArgs e)
        {
            SetListenerType(ESysProxyType.Unchanged);
        }
        private void mainMsgControl_SysProxySelected(object sender, MainMsgControl.SysProxySelectedEventArgs e)
        {
            SetListenerType(e.SelectedType);
        }
        private void SetListenerType(ESysProxyType type)
        {
            config.sysProxyType = type;
            ChangePACButtonStatus(type);
        }

        private void ChangePACButtonStatus(ESysProxyType type)
        {
            SysProxyHandle.UpdateSysProxy(config, false);

            for (int k = 0; k < menuSysAgentMode.DropDownItems.Count; k++)
            {
                ToolStripMenuItem item = ((ToolStripMenuItem)menuSysAgentMode.DropDownItems[k]);
                item.Checked = ((int)type == k);
            }

            UpdateSysProxyStatusItems();

            ConfigHandler.SaveConfig(ref config, false);

            mainMsgControl.DisplayToolStatus(config);

            BeginInvoke(new Action(() =>
            {
                notifyMain.Icon = Icon = MainFormHandler.Instance.GetNotifyIcon(config, Icon);
            }));
        }

        private void UpdateSysProxyStatusItems()
        {
            var items = new List<KeyValuePair<ESysProxyType, string>>
            {
                new KeyValuePair<ESysProxyType, string>(ESysProxyType.ForcedClear, menuKeepClear.Text),
                new KeyValuePair<ESysProxyType, string>(ESysProxyType.ForcedChange, menuGlobal.Text),
                new KeyValuePair<ESysProxyType, string>(ESysProxyType.Unchanged, menuKeepNothing.Text)
            };

            mainMsgControl.SetSysProxyItems(items, config.sysProxyType, menuSysAgentMode.Text);
        }

        #endregion


        #region CheckUpdate

        private void tsbCheckUpdateN_Click(object sender, EventArgs e)
        {
            Process.Start(Global.UpdateUrl);
        }

        private void tsbCheckUpdateCore_Click(object sender, EventArgs e)
        {
            CheckUpdateCore(ECoreType.v2fly_v5);
        }

        private void tsbCheckUpdateSagerNetCore_Click(object sender, EventArgs e)
        {
            CheckUpdateCore(ECoreType.SagerNet);
        }

        private void tsbCheckUpdateXrayCore_Click(object sender, EventArgs e)
        {
            CheckUpdateCore(ECoreType.Xray);
        }

        private void tsbCheckUpdateClashCore_Click(object sender, EventArgs e)
        {
            CheckUpdateCore(ECoreType.clash);
        }

        private void tsbCheckUpdateClashMetaCore_Click(object sender, EventArgs e)
        {
            CheckUpdateCore(ECoreType.clash_meta);
        }

        private void CheckUpdateCore(ECoreType type)
        {
            void _updateUI(bool success, string msg)
            {
                AppendText(false, msg);
                if (success)
                {
                    CloseV2ray();

                    string fileName = Utils.GetPath(Utils.GetDownloadFileName(msg));
                    FileManager.ZipExtractToFile(fileName, config.ignoreGeoUpdateCore ? "geo" : "");

                    AppendText(false, ResUI.MsgUpdateV2rayCoreSuccessfullyMore);

                    Global.reloadV2ray = true;
                    _ = LoadV2ray();

                    AppendText(false, ResUI.MsgUpdateV2rayCoreSuccessfully);
                }
            };
            (new UpdateHandle()).CheckUpdateCore(type, config, _updateUI, config.checkPreReleaseUpdate);
        }

        private void tsbCheckUpdateGeo_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                var updateHandle = new UpdateHandle();
                updateHandle.UpdateGeoFile("geosite", config, UpdateTaskHandler);
                updateHandle.UpdateGeoFile("geoip", config, UpdateTaskHandler);
            });
        }

        #endregion

        #region Help


        private void tsbAbout_Click(object sender, EventArgs e)
        {
            Process.Start(Global.AboutUrl);
        }

        private void tsbV2rayWebsite_Click(object sender, EventArgs e)
        {
            Process.Start(Global.v2rayWebsiteUrl);
        }
        #endregion

        #region 订阅
        private void tsbSubSetting_Click(object sender, EventArgs e)
        {
            // Ensure sub ids are generated and changes are persisted before opening settings dialog.
            try
            {
                ConfigHandler.SaveSubItem(ref config);
            }
            catch { }

            SubSettingForm fm = new SubSettingForm();
            if (fm.ShowDialog(this) == DialogResult.OK)
            {
                var prefer = _subId;
                InitSubView(prefer);
                RefreshServers();
            }
        }

        private void tsbSubUpdate_Click(object sender, EventArgs e)
        {
            UpdateSubscriptionProcess("", false);
        }

        private void tsbSubUpdateViaProxy_Click(object sender, EventArgs e)
        {
            UpdateSubscriptionProcess("", true);
        }
        private void tsbSubGroupUpdate_Click(object sender, EventArgs e)
        {
            UpdateCurrentSubscription(false);
        }

        private void tsbSubGroupUpdateViaProxy_Click(object sender, EventArgs e)
        {
            UpdateCurrentSubscription(true);
        }

        /// <summary>
        /// the subscription update process
        /// </summary>
        private void UpdateSubscriptionProcess(string subId, bool blProxy)
        {
            void _updateUI(bool success, string msg)
            {
                AppendText(false, msg);
                if (success)
                {
                    RefreshServers();
                    if (config.uiItem.enableAutoAdjustMainLvColWidth)
                    {
                        foreach (ColumnHeader it in lvServers.Columns)
                        {
                            it.Width = -2;
                        }
                    }
                }
            };

            (new UpdateHandle()).UpdateSubscriptionProcess(config, subId, blProxy, _updateUI);
        }

        private void UpdateCurrentSubscription(bool blProxy)
        {
            if (_subId == UnsubscribedTabId)
            {
                UI.ShowWarning(ResUI.PleaseSwitchToSubscriptionTabToUpdate);
                return;
            }
            UpdateSubscriptionProcess(_subId, blProxy);
        }

        private void tsbQRCodeSwitch_CheckedChanged(object sender, EventArgs e)
        {
            bool bShow = tsbQRCodeSwitch.Checked;
            scServers.Panel2Collapsed = !bShow;
        }
        #endregion

        #region Language

        private void tsbLanguageDef_Click(object sender, EventArgs e)
        {
            SetCurrentLanguage("en");
        }

        private void tsbLanguageZhHans_Click(object sender, EventArgs e)
        {
            SetCurrentLanguage("zh-Hans");
        }
        private void SetCurrentLanguage(string value)
        {
            Utils.RegWriteValue(Global.MyRegPath, Global.MyRegKeyLanguage, value);
            //Application.Restart();
        }

        #endregion


        #region RoutingsMenu

        /// <summary>
        ///  
        /// </summary>
        private void RefreshRoutingsMenu()
        {
            menuRoutings.Visible = config.enableRoutingAdvanced;
            if (!config.enableRoutingAdvanced)
            {
                mainMsgControl.SetRoutingItems(null, -1, false);
                return;
            }

            menuRoutings.DropDownItems.Clear();

            List<ToolStripMenuItem> lst = new List<ToolStripMenuItem>();
            for (int k = 0; k < config.routings.Count; k++)
            {
                var item = config.routings[k];
                if (item.locked == true)
                {
                    continue;
                }
                string name = item.remarks;

                ToolStripMenuItem ts = new ToolStripMenuItem(name)
                {
                    Tag = k
                };
                if (config.routingIndex.Equals(k))
                {
                    ts.Checked = true;
                }
                ts.Click += ts_Routing_Click;
                lst.Add(ts);
            }
            menuRoutings.DropDownItems.AddRange(lst.ToArray());
            mainMsgControl.SetRoutingItems(config.routings, config.routingIndex, true);
        }

        private void ts_Routing_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripItem ts = (ToolStripItem)sender;
                int index = Utils.ToInt(ts.Tag);

                if (ConfigHandler.SetDefaultRouting(ref config, index) == 0)
                {
                    RefreshRoutingsMenu();
                    _ = LoadV2ray();
                }
            }
            catch
            {
            }
        }

        private void MainMsgControl_RoutingSelected(object sender, MainMsgControl.RoutingSelectedEventArgs e)
        {
            try
            {
                if (ConfigHandler.SetDefaultRouting(ref config, e.SelectedIndex) == 0)
                {
                    RefreshRoutingsMenu();
                    _ = LoadV2ray();
                }
            }
            catch
            {
            }
        }
        #endregion

    }
}
