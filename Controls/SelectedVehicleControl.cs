using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace J2534Diag
{
    public partial class SelectedVehicleControl : UserControl
    {
        public SelectedVehicleControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the vehicle information to display.
        /// </summary>
        /// <param name="vin">Vehicle Identification Number</param>
        /// <param name="year">Year</param>
        /// <param name="make">Make</param>
        /// <param name="model">Model</param>
        public void SetVehicle(bool connected, string vin, string ymm)
        {
            var state = connected ? "Connected" : "Disconnected";
            lblConnected.Text = $"Status:{state}";
            lblVin.Text = $"VIN:{vin}";
            lblYMM.Text = ymm;
        }
    }
}
