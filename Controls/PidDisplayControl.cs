using System;
using System.Drawing;
using System.Windows.Forms;

namespace J2534Diag
{
    public partial class PidDisplayControl : UserControl
    {
        public event EventHandler IsEnabledChanged;

        public string PidName
        {
            get => lblName.Text;
            set => lblName.Text = value;
        }

        public string ValueText
        {
            get => lblValue.Text;
            set => lblValue.Text = value;
        }

        public string Unit
        {
            get => lblUnit.Text;
            set => lblUnit.Text = value;
        }

        public bool IsEnabled
        {
            get => chkEnable.Checked;
            set => chkEnable.Checked = value;
        }

        public PidDisplayControl()
        {
            InitializeComponent();
            chkEnable.CheckedChanged += (s, e) => IsEnabledChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
