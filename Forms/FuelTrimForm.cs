using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SAE.J2534;

namespace J2534Diag
{
    public partial class FuelTrimForm : Form
    {

        public struct MSG
        {
            public string id;
            public string data;
        }
        private J2534Manager j2534Manager;

        private bool useImperialUnits = true; // Set to true for imperial, false for metric
        private ConcurrentDictionary<byte, PidDisplayControl> _pidControls = new ConcurrentDictionary<byte, PidDisplayControl>();
        readonly List<ObdPid> ObdPids;

        // ISO-TP reassembly state
        List<byte> isotpBuffer = new List<byte>();
        int isotpExpectedLength = 0;
        bool isotpCollecting = false;

        DateTime lastFrameTime = DateTime.Now;
        string partialVin = "";
        private Thread _monitorThread;
        private Thread _pollingThread;
        private CancellationTokenSource _cts;
        private const int ThrottleBins = 10;
        private const int RpmBins = 10;
        private const int cellHeight = 40; // or any value you prefer

        private double?[,] trimGridBank1 = new double?[ThrottleBins, RpmBins];
        private double?[,] trimGridBank2 = new double?[ThrottleBins, RpmBins];

        private double lastThrottle = 0;
        private double lastRpm = 0;
        private double lastStft1 = 0, lastLtft1 = 0;
        private double lastStft2 = 0, lastLtft2 = 0;

        public bool BatchMode { get; private set; } = false;

        public FuelTrimForm(J2534Manager j2534Manager)
        {
            InitializeComponent();

            SetupGrids();

            ObdPids = ObdPidMap.GetObdPids(useImperialUnits);
            SetupPidDisplayControls();

            // Start everything after the form is loaded
            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;
            this.j2534Manager = j2534Manager;
        }

        private void SetupPidDisplayControls()
        {
            foreach (var pid in ObdPids)
            {
                var pidControl = new PidDisplayControl
                {
                    PidName = pid.Name,
                    Unit = pid.Unit,
                    IsEnabled = pid.Enabled
                };
                pidControl.IsEnabledChanged += (s, e) => pid.Enabled = pidControl.IsEnabled;
                flpPids.Controls.Add(pidControl);
                _pidControls[pid.Pid] = pidControl; // Add this line
            }
        }

        private void SetupGrids()
        {
            // dgvTrimGridBank1
            dgvTrimGridBank1.CellFormatting += TrimGrid_CellFormatting;
            dgvTrimGridBank1.CellPainting += TrimGrid_CellPainting;
            dgvTrimGridBank1.RowTemplate.Height = cellHeight;

            //dgvTrimGridBank2
            dgvTrimGridBank2.CellFormatting += TrimGrid_CellFormatting;
            dgvTrimGridBank2.CellPainting += TrimGrid_CellPainting;
            dgvTrimGridBank2.RowTemplate.Height = cellHeight;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void SendSessionRequest(params byte[] data)
        {
            var request = new byte[12];
            int len = Math.Min(data.Length, 12);
            Array.Copy(data, request, len);
            // The rest of request[] is already 0x00 by default
            j2534Manager.Channel.SendMessage(request);
            Thread.Sleep(10); // Give some time for the request to be sent
        }

        private void btnClearFuelTrims_Click(object sender, EventArgs e)
        {

            //This attempt wasgetting 7F failure
            //var clearFuelTrims = new byte[12];
            //clearFuelTrims[0] = 0x14;
            //clearFuelTrims[1] = 0xDA;
            //clearFuelTrims[2] = 0x11;
            //clearFuelTrims[3] = 0xF1;

            ////clearFuelTrims[0] = 0x18;
            ////clearFuelTrims[1] = 0xDB;
            ////clearFuelTrims[2] = 0x33;
            ////clearFuelTrims[3] = 0xF1;

            //clearFuelTrims[4] = 0x03;    // Length: 3 bytes follow (service + 2 bytes RID)
            //clearFuelTrims[5] = 0x31;    // Service: RoutineControl
            //clearFuelTrims[6] = 0x01;    // Subfunction: StartRoutine
            //clearFuelTrims[7] = 0xFF;    // Routine Identifier High byte
            //clearFuelTrims[8] = 0x00;    // Routine Identifier Low byte
            //                             // clearFuelTrims[9]..[11] remain 0x00

            //j2534Manager.Channel.SendMessage(clearFuelTrims);
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Signal threads to stop
            StopListening();
        }

        private void StartApiAndThreads()
        {
            _cts = new CancellationTokenSource();

            if (j2534Manager.Channel == null || j2534Manager.Channel.IsDisposed) return;

            // Set up filters, etc.
            MessageFilter passFilter = j2534Manager.BitMode == BitType.BITS_11 ? new MessageFilter()
            {
                FilterType = Filter.PASS_FILTER,
                Mask = new byte[] { 0xF0, 0xFF, 0xFF, 0x00 },
                Pattern = new byte[] { 0x00, 0x00, 0x07, 0xD0 },
                FlowControl = new byte[] { 0x00, 0x00, 0x07, 0xE0 },
                TxFlags = TxFlag.NONE
            } : new MessageFilter()
            {
                FilterType = Filter.PASS_FILTER,
                Mask = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF },
                Pattern = new byte[] { 0x14, 0x2A, 0xF1, 0x11 },
                FlowControl = new byte[] { 0x00, 0x00, 0x00, 0x00 },
                TxFlags = TxFlag.CAN_29BIT_ID
            };


            MessageFilter passFilter2 = new MessageFilter()
            {
                FilterType = Filter.PASS_FILTER,
                Mask = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF },
                Pattern = new byte[] { 0x14, 0x2C, 0xF1, 0x11 },
                FlowControl = new byte[] { 0x00, 0x00, 0x00, 0x00 },
                TxFlags = TxFlag.CAN_29BIT_ID
            };

            MessageFilter passFilter3 = new MessageFilter()
            {
                FilterType = Filter.PASS_FILTER,
                Mask = new byte[] { 0x00, 0x00, 0x00, 0x00 },
                Pattern = new byte[] { 0x18, 0x2C, 0xF1, 0x11 },
                FlowControl = new byte[] { 0x00, 0x00, 0x00, 0x00 },
                TxFlags = TxFlag.CAN_29BIT_ID
            };

            j2534Manager.Channel.DefaultTxFlag = j2534Manager.BitMode == BitType.BITS_11 ? TxFlag.NONE : TxFlag.CAN_29BIT_ID;
            j2534Manager.Channel.ClearMsgFilters();
            j2534Manager.Channel.StartMsgFilter(passFilter);
            if(j2534Manager.BitMode == BitType.BITS_29)
            {
                j2534Manager.Channel.StartMsgFilter(passFilter2);
                j2534Manager.Channel.StartMsgFilter(passFilter3);
            }

            //Start background CAN monitor
            _monitorThread = new Thread(() =>
            {
                while (!_cts.IsCancellationRequested && j2534Manager.Channel != null && !j2534Manager.Channel.IsDisposed)
                {
                    try
                    {
                        var resp = j2534Manager?.Channel?.GetMessages(200, 2);
                        //Debug.WriteLine($"Received {resp.Messages.Length} messages at {DateTime.Now:HH:mm:ss.fff}");
                        foreach (var msg in resp.Messages)
                        {
                            if(_cts.IsCancellationRequested || j2534Manager.Channel == null || j2534Manager.Channel.IsDisposed)
                            {
                                return;
                            }
                            // Process immediately
                            ParseIsoTpMessage(msg.Data, j2534Manager.Channel);
                        }
                    }
                    catch
                    {
                        return;
                    }
                    Thread.Sleep(1);
                }
            })
            { IsBackground = true };
            _monitorThread.Start();

            // Start polling thread
            _pollingThread = new Thread(() =>
            {
 
                while (!_cts.IsCancellationRequested && j2534Manager.Channel != null && !j2534Manager.Channel.IsDisposed)
                {
                    //if (j2534Manager.BitMode == BitType.BITS_11)
                    //{
                    foreach (var pid in ObdPids.Where(p => p.Enabled))
                    {
                        if (_cts.IsCancellationRequested || j2534Manager.Channel == null || j2534Manager.Channel.IsDisposed)
                        {
                            break;
                        }
                        var request = BuildObdPidRequest(pid, j2534Manager.BitMode);
                        if (_cts.IsCancellationRequested || j2534Manager.Channel == null || j2534Manager.Channel.IsDisposed) return;
                        try
                        {
                            Debug.WriteLine($"Sending OBD PID request: {BitConverter.ToString(request)} (Time: {DateTime.Now:HH:mm:ss.fff})");
                            j2534Manager.Channel.SendMessage(request);
                        }
                        catch (J2534Exception ex)
                        {
                            Debug.WriteLine($"Error sending message: {ex.Message}");
                            return;
                        }
                        Thread.Sleep(2);
                    }
                    Thread.Sleep(5);

                }
            })
            { IsBackground = true };
            _pollingThread.Start();
        }

        private IEnumerable<byte> BuildUdsPidRequest(UdsPid pid)
        {
            // 4 bytes ArbId, 1 byte Sid, 1 byte Mode, 1 byte Pid, pad with 0x00 to 12 bytes
            var request = new byte[12];
            Array.Copy(pid.ArbId, 0, request, 0, 4);
            request[4] = pid.Length;
            request[5] = pid.Service;
            request[6] = pid.DidHighByte;
            request[7] = pid.DidLowByte;
            // The rest (request[7]..request[11]) are already 0x00 by default
            return request;

        }

        void ParseIsoTpMessage(byte[] data, Channel channel)
        {
            if (data == null || data.Length < 5) return; // 4 bytes CAN ID + at least 1 byte payload


            //// Only process messages from known UDS/OBD2 response IDs
            //uint canId = (uint)(data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3]);
            //if (canId != 0x07E8 && canId != 0x07E0 && canId != 0x07DF)
            //    return;

            var payload = data.Skip(4).ToArray();
            if (payload.Length == 0) return;

            Debug.WriteLine($"Processing CAN ID: {BitConverter.ToString(data.Take(4).ToArray())} Payload: {BitConverter.ToString(payload)} (Time: {DateTime.Now:HH:mm:ss.fff})");

            byte pci = payload[0];
            int frameType = (pci & 0xF0) >> 4;

            // Update timestamp for ISO-TP timeout detection
            lastFrameTime = DateTime.Now;

            switch (frameType)
            {
                case 0x0: // Single Frame
                    {
                        int sfLen = pci & 0x0F;
                        if (payload.Length >= 1 + sfLen)
                        {
                            var msg = payload.Skip(1).Take(sfLen).ToArray();
                            TryPrintObdPidValue(msg);
                        }
                        //else
                        //Console.WriteLine("Single Frame: Payload too short");
                        break;
                    }
                case 0x1: // First Frame
                    {
                        if (payload.Length >= 2)
                        {
                            var responseCanId = data.Take(4).ToArray();

                            isotpExpectedLength = ((pci & 0x0F) << 8) + payload[1];
                            isotpBuffer.Clear();
                            isotpBuffer.AddRange(payload.Skip(2));
                            isotpCollecting = true;

                            // Insert a small delay to ensure timing is right for flow control
                            Thread.Sleep(2);

                            // Always send FC to the sender of the First Frame
                            SendFlowControl(responseCanId, channel, blockSize: 0, separationTime: 2);
                        }
                        break;
                    }
                case 0x2: // Consecutive Frame
                    {
                        int seqNum = pci & 0x0F;
                        if (payload.Length > 1 && isotpCollecting)
                        {
                            isotpBuffer.AddRange(payload.Skip(1));

                            if (isotpBuffer.Count >= isotpExpectedLength)
                            {
                                isotpCollecting = false;
                                var msg = isotpBuffer.Take(isotpExpectedLength).ToArray();
                                TryPrintObdPidValue(msg);
                            }
                        }
                        break;
                    }
                case 0x3: // Flow Control
                    break;
                default:
                    break;
            }
        }

        void SendFlowControl(byte[] canId, Channel channel, int blockSize = 0, int separationTime = 0)
        {
            var fc = new byte[] { 0x00, 0x00, 0x07, 0xE0,
            0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
            channel.SendMessage(fc);
        }

        private void TryPrintObdPidValue(byte[] payload)
        {
            if (payload.Length < 2) return;
            if (payload[0] != 0x41) return; // Only Mode 01 responses

            byte pid = payload[1];
            var pidDef = ObdPids.FirstOrDefault(p => p.Pid == pid);
            if (pidDef != null)
            {
                var data = payload.Skip(2).ToArray();
                double value = pidDef.Formula(data);
                UpdatePidDisplay(pid, value, pidDef.Unit);

                switch (pid)
                {
                    case 0x11: // Throttle Position
                        lastThrottle = value;
                        break;
                    case 0x0C: // Engine RPM
                        lastRpm = value;
                        break;
                    case 0x06: // STFT Bank 1
                        lastStft1 = value;
                        break;
                    case 0x07: // LTFT Bank 1
                        lastLtft1 = value;
                        break;
                    case 0x08: // STFT Bank 2
                        lastStft2 = value;
                        break;
                    case 0x09: // LTFT Bank 2
                        lastLtft2 = value;
                        break;
                }

                // Update Bank 1 grid
                if (lastThrottle >= 0 && lastRpm >= 0)
                {
                    int tBin = Math.Min((int)(lastThrottle / 10.0), ThrottleBins - 1);
                    int rBin = Math.Min((int)(lastRpm / 500.0), RpmBins - 1);

                    double totalTrim1 = lastStft1 + lastLtft1;
                    trimGridBank1[tBin, rBin] = totalTrim1;

                    double totalTrim2 = lastStft2 + lastLtft2;
                    trimGridBank2[tBin, rBin] = totalTrim2;

                    UpdateTrimGridDisplay();
                }
            }
        }

        byte[] BuildObdPidRequest(ObdPid pid, BitType bitMode)
        {
            // 4 bytes ArbId, 1 byte Sid, 1 byte Mode, 1 byte Pid, pad with 0x00 to 12 bytes
            var request = new byte[12];
            if(bitMode == BitType.BITS_11)
            {
                Array.Copy(pid.ArbId, 0, request, 0, 4);
            }
            else
            {
                request[0] = 0x18;
                request[1] = 0xDB;
                request[2] = 0x33;
                request[3] = 0xF1;
            }
            request[4] = pid.Sid;
            request[5] = pid.Mode;
            request[6] = pid.Pid;
            // The rest (request[7]..request[11]) are already 0x00 by default
            return request;
        }

        private void UpdatePidDisplay(byte pid, double value, string unit)
        {
            if (_pidControls.TryGetValue(pid, out var control))
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(new Action(() => control.ValueText = value.ToString()));

                }
            }
            else
            {
                control.ValueText = value.ToString();
            }
            //Debug.WriteLine($"PID {pid:X2} Value: {value} {unit} (Time: {DateTime.Now:HH:mm:ss.fff})");
        }
        private void UpdateTrimGridDisplay()
        {
            UpdateSingleTrimGridDisplay(dgvTrimGridBank1, trimGridBank1, "Bank 1");
            UpdateSingleTrimGridDisplay(dgvTrimGridBank2, trimGridBank2, "Bank 2");
        }

        private void UpdateSingleTrimGridDisplay(DataGridView dgv, double?[,] grid, string bankLabel)
        {
            if (dgv.InvokeRequired)
            {
                dgv.Invoke(new Action(() => UpdateSingleTrimGridDisplay(dgv, grid, bankLabel)));
                return;
            }

            dgv.SuspendLayout();

            int cellHeight = 40;
            int cellWidth = (int)(cellHeight);

            // Setup columns if not already done
            if (dgv.ColumnCount != RpmBins)
            {
                dgv.Columns.Clear();
                for (int r = 0; r < RpmBins; r++)
                {
                    int rpmStart = r * 500;
                    int rpmEnd = (r + 1) * 500;
                    dgv.Columns.Add($"rpm{r}", $"{rpmStart}");
                    dgv.Columns[r].Width = cellWidth;
                }
            }

            // Setup rows if not already done
            if (dgv.RowCount != ThrottleBins)
            {
                dgv.Rows.Clear();
                for (int t = 0; t < ThrottleBins; t++)
                {
                    int binIdx = ThrottleBins - 1 - t; // Flip: 0=100%, 9=0%
                    int tpStart = binIdx * 10;
                    int tpEnd = (binIdx + 1) * 10;
                    dgv.Rows.Add();
                    dgv.Rows[t].HeaderCell.Value = $"{tpStart}%";
                }
            }

            dgv.RowTemplate.Height = cellHeight;

            // Fill grid (flip Y)
            for (int t = 0; t < ThrottleBins; t++)
            {
                int binIdx = ThrottleBins - 1 - t; // Flip: 0=100%, 9=0%
                for (int r = 0; r < RpmBins; r++)
                {
                    double? dbl = grid[binIdx, r];
                    int? val = dbl.HasValue ? (int?)dbl : null;

                    if (val.HasValue && val < -99) val = null;
                    if (val.HasValue && val > 99) val = null;

                    dgv.Rows[t].Cells[r].Value = $"{val}";
                    ;
                }
            }

            dgv.ResumeLayout();
        }
        private void TrimGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv == null) return;

            // Only color data cells, not headers
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var value = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return;

            if (double.TryParse(value.ToString(), out double trim))
            {
                // Good: -5% to +5% (green)
                if (trim >= -8 && trim <= 8)
                {
                    e.CellStyle.BackColor = Color.FromArgb(60, 200, 60); // Green
                    e.CellStyle.ForeColor = Color.Black;
                }
                // Marginal: -10% to -5% or 5% to 10% (yellow)
                else if ((trim >= -13 && trim < -8) || (trim > 8 && trim <= 13))
                {
                    e.CellStyle.BackColor = Color.FromArgb(255, 220, 60); // Yellow
                    e.CellStyle.ForeColor = Color.Black;
                }
                // Bad: < -10% or > 10% (red)
                else
                {
                    e.CellStyle.BackColor = Color.FromArgb(220, 60, 60); // Red
                    e.CellStyle.ForeColor = Color.White;
                }
            }
        }

        private void TrimGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv == null) return;

            // Only paint data cells, not headers
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            // Paint everything as normal
            e.Paint(e.ClipBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.Focus);

            // If this cell is selected, draw a blue border
            if (dgv.CurrentCell != null && e.RowIndex == dgv.CurrentCell.RowIndex && e.ColumnIndex == dgv.CurrentCell.ColumnIndex)
            {
                dgv.CurrentCell.Selected = false;
                dgv.CurrentCell.Style.SelectionBackColor = Color.FromArgb(0, 10, 10, 10);
            }

            e.Handled = true;
        }
        public void StartListening()
        {
            if (_monitorThread == null || !_monitorThread.IsAlive)
            {
                _cts = new CancellationTokenSource();
                StartApiAndThreads();
            }
        }

        public void StopListening()
        {
            try
            {
                _cts?.Cancel();
                _pollingThread?.Join(500);
                _monitorThread?.Join(500);
                _pollingThread = null;
                _monitorThread = null;
                if (j2534Manager.Channel == null || j2534Manager.Channel.IsDisposed) return;
                j2534Manager.Channel.ClearTxBuffer();
                j2534Manager.Channel.ClearRxBuffer();
            }
            catch
            {
            }
        }
    }
}
