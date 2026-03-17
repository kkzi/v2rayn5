using System.Windows.Forms;

namespace v2rayN
{
    class UI
    {
        private static IWin32Window GetDefaultOwner()
        {
            try
            {
                return Form.ActiveForm;
            }
            catch
            {
                return null;
            }
        }

        public static void Show(string msg)
        {
            var owner = GetDefaultOwner();
            if (owner != null)
            {
                MessageBox.Show(owner, msg, "v2rayN", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            MessageBox.Show(msg, "v2rayN", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static void ShowWarning(string msg)
        {
            var owner = GetDefaultOwner();
            if (owner != null)
            {
                MessageBox.Show(owner, msg, "v2rayN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            MessageBox.Show(msg, "v2rayN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        public static void ShowError(string msg)
        {
            var owner = GetDefaultOwner();
            if (owner != null)
            {
                MessageBox.Show(owner, msg, "v2rayN", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show(msg, "v2rayN", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static DialogResult ShowYesNo(string msg)
        {
            var owner = GetDefaultOwner();
            if (owner != null)
            {
                return MessageBox.Show(owner, msg, "v2rayN", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }
            return MessageBox.Show(msg, "v2rayN", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        //public static string GetResourseString(string key)
        //{
        //    CultureInfo cultureInfo = null;
        //    try
        //    {
        //        string languageCode = this.LanguageCode;
        //        cultureInfo = new CultureInfo(languageCode);
        //        return Common.ResourceManager.GetString(key, cultureInfo);
        //    }
        //    catch (Exception)
        //    {
        //        //默认读取英文的多语言
        //        cultureInfo = new CultureInfo(MKey.kDefaultLanguageCode);
        //        return Common.ResourceManager.GetString(key, cultureInfo);
        //    }
        //}

    }


}
