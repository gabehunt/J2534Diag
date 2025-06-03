// MainForm.cs
using System;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Windows.Forms;
using SAE.J2534;

namespace J2534Diag
{
    public partial class MainForm : Form
    {
        private J2534Manager j2534Manager;
        private VehicleTabForm vehicleTabForm;
        private FuelTrimForm fuelTrimForm;
        private CanListenerForm canListenerForm;
        private MisfireForm misfireForm;

        public MainForm()
        {
            InitializeComponent();

            j2534Manager = new J2534Manager();

            vehicleTabForm = new VehicleTabForm(this, j2534Manager) { TopLevel = false, Dock = DockStyle.Fill, FormBorderStyle = FormBorderStyle.None };
            fuelTrimForm = new FuelTrimForm(j2534Manager) { TopLevel = false, Dock = DockStyle.Fill, FormBorderStyle = FormBorderStyle.None };
            canListenerForm = new CanListenerForm(j2534Manager) { TopLevel = false, Dock = DockStyle.Fill, FormBorderStyle = FormBorderStyle.None };
            misfireForm = new MisfireForm(j2534Manager) { TopLevel = false, Dock = DockStyle.Fill, FormBorderStyle = FormBorderStyle.None };
            tabControl1.TabPages["tabVehicle"].Controls.Add(vehicleTabForm);
            tabControl1.TabPages["tabFuelTrim"].Controls.Add(fuelTrimForm);
            tabControl1.TabPages["tabCanListener"].Controls.Add(canListenerForm);
            tabControl1.TabPages["tabMisfireMonitor"].Controls.Add(misfireForm);

            vehicleTabForm.Show();
            fuelTrimForm.Show();
            canListenerForm.Show();
            misfireForm.Show();
        }

        public void UpdateSelectedVehicle(bool connected, string vin)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateSelectedVehicle(connected, vin)));
                return;
            }

            var sb = new StringBuilder();

            if (connected && vin.Length == 17)
            {
                var vehicle = VinDecoder.DecodeVin(vin);
                var space = " ";

                var sbEngine = new StringBuilder();
                if (!string.IsNullOrEmpty(vehicle.EngineCylinders))
                {
                    sbEngine.Append(vehicle.EngineCylinders).Append(" Cyl, ");
                }

                if (!string.IsNullOrEmpty(vehicle.DisplacementL))
                {
                    sbEngine.Append(vehicle.DisplacementL).Append(" L, ");
                }

                if (!string.IsNullOrEmpty(vehicle.EngineHP))
                {
                    sbEngine.Append(vehicle.EngineHP).Append(" HP, ");
                }


                if (!string.IsNullOrEmpty(vehicle.ModelYear))
                {
                    sb.Append(vehicle.ModelYear).Append(space);
                }

                if (!string.IsNullOrEmpty(vehicle.Make))
                {
                    sb.Append(vehicle.Make).Append(space);
                }

                if (!string.IsNullOrEmpty(vehicle.Model))
                {
                    sb.Append(vehicle.Model).Append(space);
                }

                if (!string.IsNullOrEmpty(vehicle.Series))
                {
                    sb.Append(vehicle.Series).Append(space);
                }

                if (sbEngine.Length > 0)
                {
                    sb.Append(" (").Append(sbEngine.ToString().TrimEnd(new char[] { ',', ' '})).Append(")");
                }
            }
            selectedVehicleControl.SetVehicle(connected, vin, sb.ToString());
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            j2534Manager.Dispose();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //vehicleTabForm?.StopListening();
            fuelTrimForm?.StopListening();
            canListenerForm?.StopListening();
            misfireForm?.StopListening();

            if (tabControl1.SelectedTab == tabControl1.TabPages["tabFuelTrim"])
            {
                fuelTrimForm.StartListening();
            }
            else if (tabControl1.SelectedTab == tabControl1.TabPages["tabMisfireMonitor"])
            {
                misfireForm.StartListening();
            }
        }
    }
}
