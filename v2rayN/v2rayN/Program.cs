using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using v2rayN.Forms;
using v2rayN.Tool;

namespace v2rayN
{
    static class Program
    {
        private const int SW_RESTORE = 9;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;


            //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            if (!IsDuplicateInstance())
            {
                Logging.Setup();
                Utils.SaveLog($"v2rayN start up | {Utils.GetVersion()} | {Utils.GetExePath()} | OS:{Environment.OSVersion}");
                Logging.ClearLogs();

                //设置语言环境
                string lang = Utils.RegReadValue(Global.MyRegPath, Global.MyRegKeyLanguage, "zh-Hans");
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lang);

                Application.Run(new MainForm()); 
            }
            else
            {
                try
                {
                    //read handle from reg and show the window
                    long.TryParse(Utils.RegReadValue(Global.MyRegPath, Utils.WindowHwndKey, ""), out long llong);
                    if (llong > 0)
                    {
                        var hwnd = (IntPtr)llong;
                        if (TryActivateExistingInstance(hwnd))
                        {
                            return;
                        }
                    }

                    var current = Process.GetCurrentProcess();
                    var existing = Process.GetProcessesByName(current.ProcessName)
                        .FirstOrDefault(p => p.Id != current.Id);
                    if (existing != null)
                    {
                        var hwnd2 = existing.MainWindowHandle;
                        if (TryActivateExistingInstance(hwnd2))
                        {
                            return;
                        }
                    }
                }
                catch { }
                UI.ShowWarning($"v2rayN is already running(v2rayN已经运行)");
            }
        }

        private static bool TryActivateExistingInstance(IntPtr hwnd)
        {
            try
            {
                if (hwnd == IntPtr.Zero || !Utils.IsWindow(hwnd))
                {
                    return false;
                }

                Utils.ShowWindow(hwnd, SW_RESTORE);
                Utils.SetForegroundWindow(hwnd);
                Utils.SwitchToThisWindow(hwnd, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        //{
        //    try
        //    {
        //        string resourceName = "v2rayN.LIB." + new AssemblyName(args.Name).Name + ".dll";
        //        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        //        {
        //            if (stream == null)
        //            {
        //                return null;
        //            }
        //            byte[] assemblyData = new byte[stream.Length];
        //            stream.Read(assemblyData, 0, assemblyData.Length);
        //            return Assembly.Load(assemblyData);
        //        }
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        /// <summary> 
        /// 检查是否已在运行
        /// </summary> 
        public static bool IsDuplicateInstance()
        {
            // Only allow a single v2rayN instance by process name (regardless of exe path).
            string name = "v2rayN";

            Global.mutexObj = new Mutex(false, name, out bool bCreatedNew);
            return !bCreatedNew;
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Utils.SaveLog($"Application_ThreadException|IsTerminating:true", e.Exception);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Utils.SaveLog($"CurrentDomain_UnhandledException|IsTerminating:{e.IsTerminating}", ex);
            
            if (e.IsTerminating)
            {
                try
                {
                    Utils.SaveLog($"CurrentDomain_UnhandledException|StackTrace: {ex?.StackTrace}");
                    Utils.SaveLog($"CurrentDomain_UnhandledException|TargetSite: {ex?.TargetSite}");
                }
                catch { }
            }
        }
    }
}
