using System;
using System.Collections.Generic;
using System.Windows.Forms;
using v2rayN.Mode;

namespace v2rayN.Forms
{
    public partial class BaseServerForm : BaseForm
    {
        public VmessItem vmessItem = null;
        public EConfigType eConfigType;

        public BaseServerForm()
        {
            InitializeComponent();
        }

        protected void InitCoreTypeComboBox(ComboBox cmb, bool excludeV2rayN = false)
        {
            if (excludeV2rayN)
            {
                List<string> coreTypes = new List<string>();
                foreach (ECoreType it in Enum.GetValues(typeof(ECoreType)))
                {
                    if (it == ECoreType.v2rayN)
                        continue;
                    coreTypes.Add(it.ToString());
                }
                cmb.Items.AddRange(coreTypes.ToArray());
            }
            else
            {
                cmb.Items.AddRange(Global.coreTypes.ToArray());
            }
            cmb.Items.Add(string.Empty);
        }

        protected ECoreType? ParseCoreType(string text)
        {
            if (Utils.IsNullOrEmpty(text))
            {
                return null;
            }
            return (ECoreType)Enum.Parse(typeof(ECoreType), text);
        }

        protected void SetCoreTypeText(ComboBox cmb, ECoreType? coreType)
        {
            cmb.Text = coreType == null ? string.Empty : coreType.ToString();
        }
    }
}
