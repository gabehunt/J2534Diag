using System;
using System.Drawing;
using System.Windows.Forms;

namespace J2534Diag
{
    partial class FuelTrimForm
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel tlpMain;
        private FlowLayoutPanel flpPids;
        private FlowLayoutPanel flpActions;
        private Panel pnlBottom;
        private TableLayoutPanel tlpRight;
        private Panel pnlGrids;
        private DataGridView dgvTrimGridBank1;
        private DataGridView dgvTrimGridBank2;
        private Label lblBank1;
        private Label lblBank2;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Text = "J2534 OBD-II Scan Tool";

            // Main TableLayoutPanel
            this.tlpMain = new TableLayoutPanel();
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.RowCount = 2;
            this.tlpMain.Dock = DockStyle.Fill;
            this.tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // Right panel fills rest
            this.tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));        // Main area
            this.tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));        // Bottom panel height

            // PID Panel (right, top)
            this.flpPids = new FlowLayoutPanel();
            this.flpPids.Dock = DockStyle.Fill;
            this.flpPids.AutoScroll = true;
            this.flpPids.WrapContents = true;

            // Bottom Panel (spans both columns)
            this.pnlBottom = new Panel();
            this.pnlBottom.Dock = DockStyle.Fill;

            // --- Grids Panel (right, bottom) ---
            this.pnlGrids = new Panel();
            this.pnlGrids.Dock = DockStyle.Fill;
            this.pnlGrids.AutoScroll = true;

            this.lblBank1 = new Label
            {
                Text = "Fuel Trim Grid - Bank 1",
                Location = new Point(0, 0),
                AutoSize = true,
                ForeColor = Color.Blue,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            // --- Add DataGridView for Bank 1 ---
            this.dgvTrimGridBank1 = new DataGridView
            {
                Name = "dgvTrimGridBank1",
                Location = new Point(0, 20),
                Size = new Size(482, 421),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false,
                RowHeadersWidth = 80,
                ReadOnly = true,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                MultiSelect = false,
                Font = new Font("Consolas", 9F, FontStyle.Regular),
                BorderStyle = BorderStyle.FixedSingle,
                GridColor = Color.DarkCyan,
                BackgroundColor = Color.FromArgb(30, 30, 40),
                EnableHeadersVisualStyles = false
            };
            this.dgvTrimGridBank1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 60);
            this.dgvTrimGridBank1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Cyan;


            this.lblBank2 = new Label
            {
                Text = "Fuel Trim Grid - Bank 2",
                Location = new Point(492, 0),
                AutoSize = true,
                ForeColor = Color.Orange,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            // --- Add DataGridView for Bank 2 ---
            this.dgvTrimGridBank2 = new DataGridView
            {
                Name = "dgvTrimGridBank2",
                Location = new Point(492, 20),
                Size = new Size(482, 421),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false,
                RowHeadersWidth = 80,
                ReadOnly = true,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                MultiSelect = false,
                Font = new Font("Consolas", 9F, FontStyle.Regular),
                BorderStyle = BorderStyle.FixedSingle,
                GridColor = Color.DarkOrange,
                BackgroundColor = Color.FromArgb(30, 30, 40),
                EnableHeadersVisualStyles = false
            };
            this.dgvTrimGridBank2.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 60);
            this.dgvTrimGridBank2.ColumnHeadersDefaultCellStyle.ForeColor = Color.Orange;

            // Add a FlowLayoutPanel for action buttons in pnlBottom
            flpActions = new FlowLayoutPanel();
            flpActions.Dock = DockStyle.Fill; // Or DockStyle.Fill for center/left alignment
            flpActions.AutoSize = true;
            flpActions.FlowDirection = FlowDirection.LeftToRight;
            //flpActions.Padding = new Padding(10, 10, 10, 10);

            // Add the Clear Fuel Trims button
            var btnClearFuelTrims = new Button();
            btnClearFuelTrims.Name = "btnClearFuelTrims";
            btnClearFuelTrims.Text = "Clear Charts";
            btnClearFuelTrims.AutoSize = true;
            btnClearFuelTrims.Click += btnClearFuelTrims_Click;
            flpActions.Controls.Add(btnClearFuelTrims);

            // Add grids and labels to pnlGrids
            this.pnlGrids.Controls.Add(this.lblBank1);
            this.pnlGrids.Controls.Add(this.dgvTrimGridBank1);
            this.pnlGrids.Controls.Add(this.lblBank2);
            this.pnlGrids.Controls.Add(this.dgvTrimGridBank2);

            // --- Right TableLayoutPanel to stack flpPids and pnlGrids ---
            this.tlpRight = new TableLayoutPanel();
            this.tlpRight.ColumnCount = 1;
            this.tlpRight.RowCount = 2;
            this.tlpRight.Dock = DockStyle.Fill;
            this.tlpRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F)); // Height for PID controls
            this.tlpRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F)); // Height for PID controls
            this.tlpRight.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Grids fill the rest
            this.tlpRight.Controls.Add(this.flpPids, 0, 0);
            this.tlpRight.Controls.Add(this.flpActions, 0, 1);
            this.tlpRight.Controls.Add(this.pnlGrids, 0, 2);

            // Add panels to TableLayoutPanel
            this.tlpMain.Controls.Add(this.tlpRight, 0, 0);

            // Add TableLayoutPanel to Form
            this.Controls.Add(this.tlpMain);
        }

        #endregion
    }
}
