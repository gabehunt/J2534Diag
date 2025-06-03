namespace J2534Diag
{
    partial class SelectedVehicleControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblConnected;        
        private System.Windows.Forms.Label lblVin;
        private System.Windows.Forms.Label lblYMM;
        private System.Windows.Forms.Label lblMake;
        private System.Windows.Forms.Label lblModel;

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

        #region Component Designer generated code


        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblConnected = new System.Windows.Forms.Label();
            this.lblVin = new System.Windows.Forms.Label();
            this.lblYMM = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tableLayoutPanel1.Controls.Add(this.lblConnected, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblVin, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblYMM, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1200, 40);
            // 
            // Labels
            // 
            this.lblConnected.Text = "Status: Disconnected";
            this.lblConnected.AutoSize = true;
            this.lblConnected.Size = new System.Drawing.Size(200, 23);

            this.lblVin.Text = "VIN: --";
            this.lblVin.AutoSize = true;
            this.lblVin.Size = new System.Drawing.Size(200, 23);

            this.lblYMM.Text = "YMM: --";
            this.lblYMM.AutoSize = true;
            this.lblYMM.Size = new System.Drawing.Size(200, 23);

            // 
            // SelectedVehicleControl
            // 
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SelectedVehicleControl";
            this.Size = new System.Drawing.Size(1200, 40);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion
    }
}
