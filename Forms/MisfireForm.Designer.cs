using System.Windows.Forms;

namespace J2534Diag
{
    partial class MisfireForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private FlowLayoutPanel flpMisfires;
        private TableLayoutPanel tlpRight;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.flpMisfires = new System.Windows.Forms.FlowLayoutPanel();
            this.tlpRight = new System.Windows.Forms.TableLayoutPanel();
            this.tlpRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // flpMisfires
            // 
            this.flpMisfires.AutoScroll = true;
            this.flpMisfires.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpMisfires.Location = new System.Drawing.Point(3, 3);
            this.flpMisfires.Name = "flpMisfires";
            this.flpMisfires.Size = new System.Drawing.Size(794, 444);
            this.flpMisfires.TabIndex = 0;
            // 
            // tlpRight
            // 
            this.tlpRight.ColumnCount = 1;
            this.tlpRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpRight.Controls.Add(this.flpMisfires, 0, 0);
            this.tlpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRight.Location = new System.Drawing.Point(0, 0);
            this.tlpRight.Name = "tlpRight";
            this.tlpRight.RowCount = 1;
            this.tlpRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRight.Size = new System.Drawing.Size(800, 450);
            this.tlpRight.TabIndex = 0;
            // 
            // MisfireForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tlpRight);
            this.Name = "MisfireForm";
            this.Text = "MisfireForm";
            this.tlpRight.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
    }
}