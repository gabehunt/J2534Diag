using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using J2534Diag;
using SAE.J2534;
using static System.Net.Mime.MediaTypeNames;

namespace J2534Diag
{
    public partial class MisfireForm : Form
    {

        private J2534Manager j2534Manager;
        readonly List<ObdPid> MisfirePids;
        private Thread _monitorThread;
        private Thread _pollingThread;
        private CancellationTokenSource _cts;
        private IsoTpParser _isoTpParser;


        public MisfireForm(J2534Manager j2534Manager)
        {
            InitializeComponent();
            this.j2534Manager = j2534Manager;

            SetupMisfireDisplayControls();

            _isoTpParser = new IsoTpParser();
            _isoTpParser.FrameReceived += IsoTpParser_FrameReceived;

            this.Load += MisfireForm_Load;
            this.FormClosing += MisfireForm_FormClosing;
        }

        private void MisfireForm_Load(object sender, EventArgs e)
        {
            StartApiAndThreads();
        }

        private void MisfireForm_FormClosing(object sender, FormClosingEventArgs e)
        {
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
                Pattern = new byte[] { 0x00, 0x00, 0x07, 0xE8 },
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
            if (j2534Manager.BitMode == BitType.BITS_29)
            {
                j2534Manager.Channel.StartMsgFilter(passFilter2);
                j2534Manager.Channel.StartMsgFilter(passFilter3);
            }
            // Start background CAN monitor (similar to FuelTrimForm)
            _monitorThread = new Thread(() =>
            {
            while (!_cts.IsCancellationRequested && j2534Manager.Channel != null && !j2534Manager.Channel.IsDisposed)
            {
                try
                {
                    var resp = j2534Manager?.Channel?.GetMessages(200, 2);
                    foreach (var msg in resp.Messages)
                    {
                        // Process immediately
                        //Debug.WriteLine($"Rx {BitConverter.ToString(msg.Data)}");
                        _isoTpParser.ProcessMessage(msg.Data);
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

            // Start polling thread for misfire PIDs
            _pollingThread = new Thread(() =>
            {
                //var testerPresent = j2534Manager.BitMode == BitType.BITS_11 ?
                //        new byte[12] { 0x00, 0x00, 0x07, 0xE0, 0x02, 0x3E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } :
                //        new byte[12] { 0x18, 0xDB, 0x33, 0xF1, 0x02, 0x3E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                //j2534Manager.Channel.SendMessage(testerPresent);
                //Thread.Sleep(10); // Give some time for the request to be sent

                var mode6request = j2534Manager.BitMode == BitType.BITS_11 ?
                    new byte[12] { 0x00, 0x00, 0x07, 0xDF, 0x02, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } :
                    new byte[12] { 0x18, 0xDB, 0x33, 0xF1, 0x02, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                j2534Manager.Channel.SendMessage(mode6request);

                Thread.Sleep(10); // Give some time for the request to be sent

                while (!_cts.IsCancellationRequested && j2534Manager.Channel != null && !j2534Manager.Channel.IsDisposed)
                {
                    foreach (var tid in Mode6TidMap.MisfireTests.Where(p => p.Enabled).Select(t => t.Tid).Distinct())
                    {
                        if (_cts.IsCancellationRequested || j2534Manager.Channel == null || j2534Manager.Channel.IsDisposed) break;

                        var request = new byte[12];
                        if (j2534Manager.BitMode == BitType.BITS_11)
                        {
                            request[0] = 0x00;
                            request[1] = 0x00;
                            request[2] = 0x07;
                            request[3] = 0xDF;
                        }
                        else
                        {
                            request[0] = 0x18;
                            request[1] = 0xDB;
                            request[2] = 0x33;
                            request[3] = 0xF1;
                        }

                        request[4] = 0x02; // SID
                        request[5] = 0x06; // Mode $06
                        request[6] = tid;

                        j2534Manager.Channel.SendMessage(request);
                        Thread.Sleep(100);
                    }
                    Thread.Sleep(500); // Adjust polling rate as needed
                }
            })
            { IsBackground = true };
            _pollingThread.Start();
        }

        byte[] BuildMisfireObdPidRequest(ObdPid pid, BitType bitMode)
        {
            // 4 bytes ArbId, 1 byte Sid, 1 byte Mode, 1 byte Pid, pad with 0x00 to 12 bytes
            var request = new byte[12];
            if (bitMode == BitType.BITS_11)
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

        private void IsoTpParser_FrameReceived(object sender, IsoTpFrameEventArgs e)
        {
            //Debug.WriteLine($"Complete Mode $06 Frame: {BitConverter.ToString(e.Payload)}");

            switch (e.FrameType)
            {
                case IsoTpFrameType.SingleFrame:
                    // Handle single frame (e.Payload)
                    break;

                case IsoTpFrameType.FirstFrame:
                    // Insert a small delay to ensure timing is right for flow control
                    Thread.Sleep(5);

                    // Always send FC to the sender of the First Frame
                    j2534Manager.SendFlowControl(
                        j2534Manager.BitMode == BitType.BITS_11
                            ? new byte[] { 0x00, 0x00, 0x07, 0xE0 }
                            : new byte[] { 0x14, 0xDA, 0x11, 0xF1 },
                        blockSize: 0,
                        separationTime: 10
                    );

                    //Do domething with e.Payload byte[]

                    break;

                case IsoTpFrameType.ConsecutiveFrame:
                    //Do domething with e.Payload byte[]
                    if(e.IsComplete)
                    {
                        if (e.Payload[0] == 0x46)
                        {
                            ParseMode6Response(e.Payload.Skip(1).ToArray()); // full, complete data
                        }
                    }
                    break;

            }
        }

        private readonly Dictionary<(byte tid, byte cid), PidDisplayControl> _misfireControls = new Dictionary<(byte tid, byte cid), PidDisplayControl>();

        private void UpdatePidDisplay(byte tid, byte cid, double value, string unit)
        {
            var control = _misfireControls.FirstOrDefault(t => t.Key.tid == tid && t.Key.cid == cid).Value;
            if (control != null)
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
        private void SetupMisfireDisplayControls()
        {
            foreach (var tid in Mode6TidMap.MisfireTests)
            {
                var control = new PidDisplayControl
                {
                    PidName = tid.Label,
                    Unit = tid.Units,
                    IsEnabled = tid.Enabled
                };

                flpMisfires.Controls.Add(control);
                _misfireControls[(tid.Tid, tid.Cid)] = control;
            }
        }


        private void ParseMode6Response(byte[] payload)
        {
            try
            {
                int i = 0;
                while (i + 7 < payload.Length)
                {
                    byte tid = payload[i];
                    byte cid = payload[i + 1];
                    byte subid = payload[i + 2];

                    //Debug.WriteLine($"tid: {tid:X2}, cid: {cid:X2}, subid: {subid:X2}");

                    var test = Mode6TidMap.MisfireTests.FirstOrDefault(t => t.Tid == tid && t.Cid == cid && t.SubId == subid);
                    if (test != null)
                    {
                        ushort value = test.DecodeValue(new[] { payload[i + 3], payload[i + 4] });
                        UpdatePidDisplay(tid, cid, value, test.Units);
                        //Debug.WriteLine($"{test.Label}: {value} {test.Units}");
                    }
                    else
                    {
                        //Debug.WriteLine($"Unknown TID={tid:X2} CID={cid:X2} SubID={subid:X2}");
                    }

                    i += 9; // Always step forward correctly
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"Error parsing Mode $06: {ex}");
            }
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
            catch { }
        }
    }
}
