using System;

namespace J2534Diag
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mainLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabVehicle = new System.Windows.Forms.TabPage();
            this.tabFuelTrim = new System.Windows.Forms.TabPage();
            this.tabCanListener = new System.Windows.Forms.TabPage();
            this.selectedVehicleControl = new J2534Diag.SelectedVehicleControl();
            this.tabMisfireMonitor = new System.Windows.Forms.TabPage();
            this.mainLayoutPanel.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainLayoutPanel
            // 
            this.mainLayoutPanel.ColumnCount = 1;
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayoutPanel.Controls.Add(this.tabControl1, 0, 0);
            this.mainLayoutPanel.Controls.Add(this.selectedVehicleControl, 0, 1);
            this.mainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainLayoutPanel.Name = "mainLayoutPanel";
            this.mainLayoutPanel.RowCount = 2;
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.mainLayoutPanel.Size = new System.Drawing.Size(800, 562);
            this.mainLayoutPanel.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabVehicle);
            this.tabControl1.Controls.Add(this.tabFuelTrim);
            this.tabControl1.Controls.Add(this.tabCanListener);
            this.tabControl1.Controls.Add(this.tabMisfireMonitor);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(794, 516);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabVehicle
            // 
            this.tabVehicle.Location = new System.Drawing.Point(4, 22);
            this.tabVehicle.Name = "tabVehicle";
            this.tabVehicle.Size = new System.Drawing.Size(786, 490);
            this.tabVehicle.TabIndex = 0;
            this.tabVehicle.Text = "Vehicle";
            // 
            // tabFuelTrim
            // 
            this.tabFuelTrim.Location = new System.Drawing.Point(4, 22);
            this.tabFuelTrim.Name = "tabFuelTrim";
            this.tabFuelTrim.Padding = new System.Windows.Forms.Padding(3);
            this.tabFuelTrim.Size = new System.Drawing.Size(786, 490);
            this.tabFuelTrim.TabIndex = 0;
            this.tabFuelTrim.Text = "Fuel Trims";
            this.tabFuelTrim.UseVisualStyleBackColor = true;
            // 
            // tabCanListener
            // 
            this.tabCanListener.Location = new System.Drawing.Point(4, 22);
            this.tabCanListener.Name = "tabCanListener";
            this.tabCanListener.Padding = new System.Windows.Forms.Padding(3);
            this.tabCanListener.Size = new System.Drawing.Size(786, 490);
            this.tabCanListener.TabIndex = 1;
            this.tabCanListener.Text = "CAN Listener";
            this.tabCanListener.UseVisualStyleBackColor = true;
            // 
            // selectedVehicleControl
            // 
            this.selectedVehicleControl.Location = new System.Drawing.Point(3, 525);
            this.selectedVehicleControl.Name = "selectedVehicleControl";
            this.selectedVehicleControl.Size = new System.Drawing.Size(794, 34);
            this.selectedVehicleControl.TabIndex = 1;
            // 
            // tabMisfireMonitor
            // 
            this.tabMisfireMonitor.Location = new System.Drawing.Point(4, 22);
            this.tabMisfireMonitor.Name = "tabMisfireMonitor";
            this.tabMisfireMonitor.Size = new System.Drawing.Size(786, 490);
            this.tabMisfireMonitor.TabIndex = 2;
            this.tabMisfireMonitor.Text = "Misfire Monitor";
            this.tabMisfireMonitor.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 562);
            this.Controls.Add(this.mainLayoutPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "J2534Diag";
            this.mainLayoutPanel.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        #endregion
        private System.Windows.Forms.TabPage tabVehicle;
        private System.Windows.Forms.TabPage tabFuelTrim;
        private System.Windows.Forms.TabPage tabCanListener;
        private System.Windows.Forms.TabControl tabControl1;
        private J2534Diag.SelectedVehicleControl selectedVehicleControl;
        private System.Windows.Forms.TableLayoutPanel mainLayoutPanel;
        private System.Windows.Forms.TabPage tabMisfireMonitor;
    }
}