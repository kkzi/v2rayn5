using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

using v2rayN.Forms;
using v2rayN.Mode;
using v2rayN.Tool;

using Xunit;

namespace v2rayN.Tests.Forms
{
    public class BaseFormTests
    {
        [Fact]
        public void BaseForm_UsesDefaultFontByDefault()
        {
            using BaseForm form = RunInSta(() => new TestBaseForm());

            Assert.Equal("Segoe UI", form.Font.FontFamily.Name);
            Assert.Equal(9f, form.Font.SizeInPoints, 3);
            Assert.Equal(FontStyle.Regular, form.Font.Style);
        }

        [Fact]
        public void BaseForm_UsesDpiAutoScaleMode()
        {
            using BaseForm form = RunInSta(() => new TestBaseForm());

            Assert.Equal(AutoScaleMode.Dpi, form.AutoScaleMode);
        }

        [Fact]
        public void MainForm_UsesDpiAutoScaleMode()
        {
            using MainForm form = RunInSta(() => new MainForm());

            Assert.Equal(AutoScaleMode.Dpi, form.AutoScaleMode);
            Assert.Equal(96F, form.AutoScaleDimensions.Width, 3);
            Assert.Equal(96F, form.AutoScaleDimensions.Height, 3);
        }

        [Fact]
        public void MainMsgControl_UsesDpiAutoScaleMode()
        {
            using MainMsgControl control = RunInSta(() => new MainMsgControl());

            Assert.Equal(AutoScaleMode.Dpi, control.AutoScaleMode);
            Assert.Equal(96F, control.AutoScaleDimensions.Width, 3);
            Assert.Equal(96F, control.AutoScaleDimensions.Height, 3);
        }

        [Fact]
        public void MainMsgControl_StatusStripLayout_ScalesHeightForDpi()
        {
            using TestMainMsgControl control = RunInSta(() => new TestMainMsgControl());

            control.ApplyStatusStripLayoutForTest();

            var statusStrip = (StatusStrip)control.Controls.Find("ssMain", true)[0];
            var sysProxy = (ToolStripDropDownButton)statusStrip.Items["toolSdbSysProxy"];
            var routing = (ToolStripDropDownButton)statusStrip.Items["toolSdbRoutingRule"];

            Assert.False(statusStrip.AutoSize);
            Assert.Equal(33, statusStrip.Height);
            Assert.True(sysProxy.Height >= 26);
            Assert.True(routing.Height >= 26);
        }

        [Fact]
        public void MainForm_ToggleLogPanel_UsesCurrentStatusStripHeightWhenCollapsing()
        {
            using MainForm form = RunInSta(() => new MainForm());
            using var mainMsgControl = new AdjustableStatusStripMainMsgControl();

            var splitContainer = (SplitContainer)GetPrivateField(form, "scBig");
            SetPrivateField(form, "mainMsgControl", mainMsgControl);

            mainMsgControl.StatusStripHeightForTest = 33;
            InvokePrivateMethod(form, "ToggleLogPanel");
            int firstCollapsedDistance = splitContainer.SplitterDistance;

            InvokePrivateMethod(form, "ToggleLogPanel");

            mainMsgControl.StatusStripHeightForTest = 55;
            InvokePrivateMethod(form, "ToggleLogPanel");
            int secondCollapsedDistance = splitContainer.SplitterDistance;

            Assert.Equal(22, firstCollapsedDistance - secondCollapsedDistance);
        }

        [Fact]
        public void MainMsgControl_SetRoutingItems_ExpandsRoutingButtonForLongSelectedText()
        {
            using TestMainMsgControl control = RunInSta(() => new TestMainMsgControl());
            var routingItems = new System.Collections.Generic.List<RoutingItem>
            {
                new RoutingItem
                {
                    remarks = "这是一个用于高Dpi状态栏宽度验证的较长路由规则名称"
                }
            };

            control.ApplyStatusStripLayoutForTest();
            control.SetRoutingItems(routingItems, 0, true);

            var statusStrip = (StatusStrip)control.Controls.Find("ssMain", true)[0];
            var routing = (ToolStripDropDownButton)statusStrip.Items["toolSdbRoutingRule"];
            int preferredWidth = routing.GetPreferredSize(Size.Empty).Width;

            Assert.True(routing.Width >= preferredWidth, $"routing width {routing.Width} should be >= preferred width {preferredWidth}");
        }

        [Fact]
        public void MainForm_LvServersCtrlP_IsIgnored()
        {
            using TestShortcutMainForm form = RunInSta(() => new TestShortcutMainForm());

            var args = new KeyEventArgs(Keys.Control | Keys.P);
            InvokePrivateMethod(form, "lvServers_KeyDown", GetPrivateField(form, "lvServers"), args);

            Assert.False(args.Handled);
            Assert.False(args.SuppressKeyPress);
            Assert.False(form.PingShortcutExecuted);
        }

        [Fact]
        public void MainForm_LvServersCtrlO_IsIgnored()
        {
            using TestShortcutMainForm form = RunInSta(() => new TestShortcutMainForm());

            var args = new KeyEventArgs(Keys.Control | Keys.O);
            InvokePrivateMethod(form, "lvServers_KeyDown", GetPrivateField(form, "lvServers"), args);

            Assert.False(args.Handled);
            Assert.False(args.SuppressKeyPress);
            Assert.False(form.TcpingShortcutExecuted);
        }

        [Theory]
        [InlineData(Keys.A, nameof(TestShortcutMainForm.SelectAllShortcutExecuted))]
        [InlineData(Keys.C, nameof(TestShortcutMainForm.ExportShortcutExecuted))]
        [InlineData(Keys.S, nameof(TestShortcutMainForm.ScanShortcutExecuted))]
        [InlineData(Keys.T, nameof(TestShortcutMainForm.SpeedShortcutExecuted))]
        [InlineData(Keys.E, nameof(TestShortcutMainForm.SortShortcutExecuted))]
        public void MainForm_GlobalCtrlShortcuts_AreHandledFromMainKeyPreview(Keys key, string executedPropertyName)
        {
            using TestShortcutMainForm form = RunInSta(() => new TestShortcutMainForm());

            var args = new KeyEventArgs(Keys.Control | key);
            InvokePrivateMethod(form, "MainForm_KeyDown", form, args);

            Assert.True(args.Handled);
            Assert.True(args.SuppressKeyPress);

            var property = typeof(TestShortcutMainForm).GetProperty(executedPropertyName, BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(property);
            Assert.True((bool)property.GetValue(form));
        }

        [Theory]
        [InlineData(Keys.P)]
        [InlineData(Keys.O)]
        public void MainForm_GlobalCtrlShortcuts_DoNotHandlePingOrTcping(Keys key)
        {
            using TestShortcutMainForm form = RunInSta(() => new TestShortcutMainForm());

            var args = new KeyEventArgs(Keys.Control | key);
            InvokePrivateMethod(form, "MainForm_KeyDown", form, args);

            Assert.False(args.Handled);
            Assert.False(args.SuppressKeyPress);
            Assert.False(form.PingShortcutExecuted);
            Assert.False(form.TcpingShortcutExecuted);
        }

        [Fact]
        public void MainForm_GetLvSelectedIndex_UsesOwnedPromptWhenNoServerIsSelected()
        {
            using TestPromptMainForm form = RunInSta(() => new TestPromptMainForm());

            var method = typeof(MainForm).GetMethod("GetLvSelectedIndex", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(bool) }, null);
            Assert.NotNull(method);
            int index = (int)method.Invoke(form, new object[] { true });

            Assert.Equal(-1, index);
            Assert.False(string.IsNullOrWhiteSpace(form.LastOwnedPromptMessage));
        }

        [Fact]
        public void MainForm_UpdateCurrentSubscription_UsesOwnedWarningPromptWhenUnsubscribedTabIsSelected()
        {
            using TestPromptMainForm form = RunInSta(() => new TestPromptMainForm());

            InvokePrivateMethod(form, "UpdateCurrentSubscription", false);

            Assert.False(string.IsNullOrWhiteSpace(form.LastOwnedWarningMessage));
        }

        [Fact]
        public void MainForm_RemoveServer_UsesOwnedYesNoPrompt()
        {
            using TestPromptMainForm form = RunInSta(() => new TestPromptMainForm());
            var lvServers = (ListView)form.Controls.Find("lvServers", true)[0];
            var vmessList = new List<VmessItem> { new VmessItem() };

            form.Show();
            Application.DoEvents();
            SetPrivateField(form, "lstVmess", vmessList);
            lvServers.Items.Add(new ListViewItem("server-1"));
            lvServers.Items[0].Selected = true;
            Application.DoEvents();

            Assert.Single(lvServers.SelectedIndices.Cast<int>());

            InvokePrivateMethod(form, "menuRemoveServer_Click", null, null);

            Assert.False(string.IsNullOrWhiteSpace(form.LastOwnedYesNoMessage));
        }

        [Fact]
        public void BaseForm_GetLvSelectedIndex_UsesOwnedPromptWhenNothingIsSelected()
        {
            using TestPromptBaseForm form = RunInSta(() => new TestPromptBaseForm());

            int index = form.GetSelectedIndexForTest();

            Assert.Equal(-1, index);
            Assert.False(string.IsNullOrWhiteSpace(form.LastOwnedPromptMessage));
        }

        [Fact]
        public void BaseForm_HandleResult_UsesOwnedWarningPromptWhenOperationFails()
        {
            using TestPromptBaseForm form = RunInSta(() => new TestPromptBaseForm());

            form.HandleResultForTest(-1);

            Assert.False(string.IsNullOrWhiteSpace(form.LastOwnedWarningMessage));
        }

        [Fact]
        public void BaseForm_ShowOwnedInfoPromptSafe_MarshalsToUiThread()
        {
            using TestPromptBaseForm form = RunInSta(() => new TestPromptBaseForm());

            form.Show();
            Application.DoEvents();

            var thread = new Thread(() => form.ShowOwnedInfoPromptSafeForTest("safe-prompt"));
            thread.Start();
            thread.Join();

            var deadline = Stopwatch.StartNew();
            while (string.IsNullOrWhiteSpace(form.LastOwnedPromptMessage) && deadline.Elapsed < TimeSpan.FromSeconds(2))
            {
                Application.DoEvents();
                Thread.Sleep(10);
            }

            Assert.Equal("safe-prompt", form.LastOwnedPromptMessage);
        }

        [Fact]
        public void MainForm_PreservePersistedColumnWidthsOnLoad_DoesNotClearSavedWidths()
        {
            using TestPromptMainForm form = RunInSta(() => new TestPromptMainForm());
            var widths = new Dictionary<string, int>
            {
                ["remarks"] = 180,
                ["address"] = 240
            };

            form.SetMainColumnWidthsForTest(widths);
            form.PreservePersistedColumnWidthsOnLoadForTest();

            Assert.Equal(2, widths.Count);
            Assert.Equal(180, widths["remarks"]);
            Assert.Equal(240, widths["address"]);
        }

        [Fact]
        public void RoutingRuleSettingForm_GetBatchRoutingReplaceMode_UsesOwnedYesNoPrompt()
        {
            using TestRoutingRuleSettingForm form = RunInSta(() => new TestRoutingRuleSettingForm());

            bool replace = form.GetBatchRoutingReplaceModeForTest();

            Assert.True(replace);
            Assert.False(string.IsNullOrWhiteSpace(form.LastOwnedYesNoMessage));
        }

        [Fact]
        public void BaseForm_ManualDpiFonts_NormalizesNestedControlsToLogicalFontOnce()
        {
            using TestDpiForm form = RunInSta(() => new TestDpiForm());

            form.ApplyFontsForTest();
            float firstSize = form.InnerTextBox.Font.Size;

            form.ApplyFontsForTest();
            float secondSize = form.InnerTextBox.Font.Size;

            Assert.Equal(GraphicsUnit.Point, form.InnerTextBox.Font.Unit);
            Assert.Equal(9f, firstSize, 3);
            Assert.Equal(firstSize, secondSize, 3);
        }

        [Fact]
        public void MainForm_ManualDpiFonts_NormalizesRealControlsToLogicalFont()
        {
            using TestMainForm form = RunInSta(() => new TestMainForm());

            form.ApplyFontsForTest();
            var lvServers = (ListView)form.Controls.Find("lvServers", true)[0];
            var txtServerFilter = (TextBox)form.Controls.Find("txtServerFilter", true)[0];

            Assert.Equal(GraphicsUnit.Point, form.Font.Unit);
            Assert.Equal("Segoe UI", form.Font.FontFamily.Name);
            Assert.Equal(GraphicsUnit.Point, lvServers.Font.Unit);
            Assert.Equal("Segoe UI", lvServers.Font.FontFamily.Name);
            Assert.Equal(GraphicsUnit.Point, txtServerFilter.Font.Unit);
            Assert.Equal("Segoe UI", txtServerFilter.Font.FontFamily.Name);
        }

        [Fact]
        public void MainForm_FontAssignment_CanSwitchToLogicalUiFont()
        {
            using MainForm form = RunInSta(() => new MainForm());
            using Font logicalFont = HighDpiHelper.NormalizeFontToPoints(form.Font);

            form.Font = logicalFont;

            Assert.Equal(GraphicsUnit.Point, form.Font.Unit);
            Assert.Equal("Segoe UI", form.Font.FontFamily.Name);
        }

        [Fact]
        public void MainForm_ChildControlFontAssignment_CanSwitchToLogicalUiFont()
        {
            using MainForm form = RunInSta(() => new MainForm());
            var txtServerFilter = (TextBox)form.Controls.Find("txtServerFilter", true)[0];
            form.CreateControl();
            txtServerFilter.CreateControl();
            using Font logicalFont = HighDpiHelper.NormalizeFontToPoints(txtServerFilter.Font);

            txtServerFilter.Font = logicalFont;

            Assert.Equal(GraphicsUnit.Point, txtServerFilter.Font.Unit);
            Assert.Equal("Segoe UI", txtServerFilter.Font.FontFamily.Name);
        }

        private static T RunInSta<T>(Func<T> action)
        {
            T result = default;
            Exception error = null;

            var thread = new Thread(() =>
            {
                try
                {
                    result = action();
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (error != null)
            {
                throw error;
            }

            return result;
        }

        private static object GetPrivateField(object instance, string fieldName)
        {
            var field = FindField(instance.GetType(), fieldName);
            Assert.NotNull(field);
            return field.GetValue(instance);
        }

        private static object InvokePrivateMethod(object instance, string methodName, params object[] parameters)
        {
            var method = FindMethod(instance.GetType(), methodName);
            Assert.NotNull(method);
            return method.Invoke(instance, parameters);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            var field = FindField(instance.GetType(), fieldName);
            Assert.NotNull(field);
            field.SetValue(instance, value);
        }

        private static FieldInfo FindField(Type type, string fieldName)
        {
            while (type != null)
            {
                var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    return field;
                }
                type = type.BaseType;
            }
            return null;
        }

        private static MethodInfo FindMethod(Type type, string methodName)
        {
            while (type != null)
            {
                var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (method != null)
                {
                    return method;
                }
                type = type.BaseType;
            }
            return null;
        }

        private sealed class TestBaseForm : BaseForm
        {
        }

        private sealed class TestPromptBaseForm : BaseForm
        {
            private readonly ListView listView_ = new ListView();
            private readonly List<int> selectedIndices_ = new List<int>();

            public TestPromptBaseForm()
            {
                listView_.Parent = this;
                listView_.Items.Add(new ListViewItem("rule-1"));
            }

            public string LastOwnedPromptMessage { get; private set; }
            public string LastOwnedWarningMessage { get; private set; }

            public int GetSelectedIndexForTest()
            {
                return GetLvSelectedIndex(listView_, selectedIndices_);
            }

            public void HandleResultForTest(int ret)
            {
                HandleResult(ret);
            }

            public void ShowOwnedInfoPromptSafeForTest(string message)
            {
                ShowOwnedInfoPromptSafe(message);
            }

            protected override void ShowOwnedInfoPrompt(string message)
            {
                LastOwnedPromptMessage = message;
            }

            protected override void ShowOwnedWarningPrompt(string message)
            {
                LastOwnedWarningMessage = message;
            }
        }

        private sealed class TestDpiForm : BaseForm
        {
            public TestDpiForm()
            {
                Font = new Font("Microsoft Sans Serif", 11f, FontStyle.Regular, GraphicsUnit.Pixel);

                var panel = new Panel
                {
                    Dock = DockStyle.Fill
                };

                InnerTextBox = new TextBox
                {
                    Parent = panel,
                    Location = new Point(8, 8),
                    Width = 120
                };

                Controls.Add(panel);
            }

            public TextBox InnerTextBox { get; }

            public void ApplyFontsForTest()
            {
                ApplyManualDpiFonts();
            }
        }

        private sealed class TestMainForm : MainForm
        {
            public void ApplyFontsForTest()
            {
                ApplyManualDpiFonts();
            }
        }

        private sealed class TestShortcutMainForm : MainForm
        {
            public bool PingShortcutExecuted { get; private set; }
            public bool TcpingShortcutExecuted { get; private set; }
            public bool SelectAllShortcutExecuted { get; private set; }
            public bool ExportShortcutExecuted { get; private set; }
            public bool ScanShortcutExecuted { get; private set; }
            public bool SpeedShortcutExecuted { get; private set; }
            public bool SortShortcutExecuted { get; private set; }

            protected override void ExecutePingShortcut()
            {
                PingShortcutExecuted = true;
            }

            protected override void ExecuteTcpingShortcut()
            {
                TcpingShortcutExecuted = true;
            }

            protected override void ExecuteSelectAllServersShortcut()
            {
                SelectAllShortcutExecuted = true;
            }

            protected override void ExecuteExportShareUrlShortcut()
            {
                ExportShortcutExecuted = true;
            }

            protected override void ExecuteScanScreenShortcut()
            {
                ScanShortcutExecuted = true;
            }

            protected override void ExecuteSpeedTestShortcut()
            {
                SpeedShortcutExecuted = true;
            }

            protected override void ExecuteSortServerResultShortcut()
            {
                SortShortcutExecuted = true;
            }
        }

        private sealed class TestPromptMainForm : MainForm
        {
            public string LastOwnedPromptMessage { get; private set; }
            public string LastOwnedWarningMessage { get; private set; }
            public string LastOwnedYesNoMessage { get; private set; }

            public void SetMainColumnWidthsForTest(Dictionary<string, int> widths)
            {
                config = new Config
                {
                    uiItem = new UIItem
                    {
                        mainLvColWidth = widths
                    }
                };
            }

            public void PreservePersistedColumnWidthsOnLoadForTest()
            {
                PreservePersistedMainLvColumnWidthsOnLoad();
            }

            protected override void ShowOwnedInfoPrompt(string message)
            {
                LastOwnedPromptMessage = message;
            }

            protected override void ShowOwnedWarningPrompt(string message)
            {
                LastOwnedWarningMessage = message;
            }

            protected override DialogResult ShowOwnedYesNoPrompt(string message)
            {
                LastOwnedYesNoMessage = message;
                return DialogResult.No;
            }
        }

        private sealed class TestRoutingRuleSettingForm : RoutingRuleSettingForm
        {
            public string LastOwnedYesNoMessage { get; private set; }

            public bool GetBatchRoutingReplaceModeForTest()
            {
                return GetBatchRoutingReplaceMode();
            }

            protected override DialogResult ShowOwnedYesNoPrompt(string message)
            {
                LastOwnedYesNoMessage = message;
                return DialogResult.No;
            }
        }

        private sealed class TestMainMsgControl : MainMsgControl
        {
            protected override int GetEffectiveDpiForStatusStrip()
            {
                return 144;
            }

            public void ApplyStatusStripLayoutForTest()
            {
                NormalizeStatusStripLayout();
            }
        }

        private sealed class AdjustableStatusStripMainMsgControl : MainMsgControl
        {
            public int StatusStripHeightForTest { get; set; } = 22;

            public override int GetStatusStripHeight()
            {
                return StatusStripHeightForTest;
            }
        }
    }
}
