using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace J2534Diag
{
    public partial class PidDisplayControl : UserControl
    {
        private Label lblValue;
        private Label lblUnit;
        private Label lblName;
        private CheckBox chkEnable;

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(30, 30, 40);
            this.BorderStyle = BorderStyle.None;
            this.Padding = new Padding(8);

            this.lblValue = new Label
            {
                Font = new Font("Consolas", 24F, FontStyle.Bold),
                ForeColor = Color.Lime,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.lblUnit = new Label
            {
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Cyan,
                Dock = DockStyle.Top,
                Height = 20,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.lblName = new Label
            {
                Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                ForeColor = Color.Silver,
                Dock = DockStyle.Bottom,
                Height = 20,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.chkEnable = new CheckBox
            {
                Dock = DockStyle.Bottom,
                Text = "Enable",
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };

            this.Controls.Add(lblValue);
            this.Controls.Add(lblUnit);
            this.Controls.Add(lblName);
            this.Controls.Add(chkEnable);

            this.Height = 120;
            this.Width = 140;

            this.Paint += (s, e) =>
            {
                var g = e.Graphics;
                var rect = this.ClientRectangle;
                rect.Inflate(-2, -2);
                using (var pen = new Pen(Color.Cyan, 2))
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect, Color.FromArgb(40, 40, 60), Color.FromArgb(20, 20, 30), 90f))
                {
                    g.FillRectangle(brush, rect);
                    g.DrawRectangle(pen, rect);
                }
            };
        }

    }
}
