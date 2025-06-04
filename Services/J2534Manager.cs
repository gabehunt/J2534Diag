// cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Windows.Forms;
using SAE.J2534;

namespace J2534Diag
{
    public enum BitType
    {
        BITS_11 = 0,
        BITS_29 = 1
    }
    public class J2534Manager: IDisposable
    {
        public API Api { get; private set; }
        public Device Device { get; private set; }
        public Channel Channel { get; private set; }
        public List<APIInfo> Devices { get; private set; }
        public byte[] EcmRxId { get; private set; }
        public string Vin { get; private set; }
        public BitType BitMode { get; set; } = BitType.BITS_11; // Default to 11-bit mode

        private IsoTpParser _isoTpParser;
        private Thread _monitorThread;
        private CancellationTokenSource _cts;

        public J2534Manager()
        {
            // Initialize J2534Manager here with your preferred DLL and settings
            Devices = APIFactory.GetAPIinfo().ToList();
            _isoTpParser = new IsoTpParser();
            _isoTpParser.FrameReceived += IsoTpParser_FrameReceived;
            //var vciApi = vciApis.First(x => x.Name == "SuperGoose-Plus");
            //Initialize(vciApi.Filename, Protocol.CAN, Baud.CAN_500000, ConnectFlag.NONE);
        }

        public void Initialize(string dllFileName)
        {
            for (int iBitMode = 0; iBitMode < 2; iBitMode++)
            {
                if (iBitMode == 1)
                {
                    BitMode = BitType.BITS_29; // Switch to 29-bit mode for second device
                }

                InitializeChannel(dllFileName);

                if (Vin == null)
                {
                    Debug.WriteLine("VIN not received.");
                }
                else if (Vin.Length == 17)
                {
                    Debug.WriteLine($"VIN: {Vin} (Time: {DateTime.Now:HH:mm:ss.fff}), trying again.");
                    return;
                }
                else
                {
                    Debug.WriteLine($"VIN Incomplete: {Vin} (Time: {DateTime.Now:HH:mm:ss.fff}), trying again.");
                    // Only allow 1 retry here
                    InitializeChannel(dllFileName);
                }
            }
        }

        private void InitializeChannel(string dllFileName)
        {
            if (Channel != null) { try { Channel.Dispose(); } catch { } }
            if (Device != null) { try { Device.Dispose(); } catch { } }
            if (Api != null) { try { Api.Dispose(); } catch { } }
            Api = APIFactory.GetAPI(dllFileName);
            Device = Api.GetDevice();

            EcmRxId = BitMode == BitType.BITS_11 ? new byte[] { 0x00, 0x00, 0x07, 0xE0 } : new byte[] { 0x14, 0xDA, 0x11, 0xF1 };
            Channel = Device.GetChannel(Protocol.CAN, Baud.CAN_500000, BitMode == BitType.BITS_11 ? ConnectFlag.NONE : ConnectFlag.CAN_29BIT_ID);

            MessageFilter passFilter = BitMode == BitType.BITS_11 ? new MessageFilter()
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

            Channel.DefaultTxFlag = BitMode == BitType.BITS_11 ? TxFlag.NONE : TxFlag.CAN_29BIT_ID;

            Channel.ClearMsgFilters();
            var filterId = Channel.StartMsgFilter(passFilter);

            _cts = new CancellationTokenSource(3000);

            //Start background CAN monitor
            _monitorThread = new Thread(() =>
            {
                while (!_cts.IsCancellationRequested && Channel != null && !Channel.IsDisposed)
                {
                    try
                    {
                        var resp = Channel?.GetMessages(200, 5);
                        //Debug.WriteLine($"Received {resp.Messages.Length} messages at {DateTime.Now:HH:mm:ss.fff}");
                        foreach (var msg in resp.Messages)
                        {
                            // Process immediately
                            _isoTpParser.ProcessMessage(msg.Data);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error in Vehicle Tab: {ex.Message}");
                        //StopListening();
                    }
                    Thread.Sleep(1);
                }
            })
            { IsBackground = true };

            _monitorThread.Start();

            // Send VIN request
            var vinRequest = new byte[12];
            if (BitMode == BitType.BITS_11)
            {
                vinRequest[0] = 0x00; // OBD2 Mode 09 PID 02
                vinRequest[1] = 0x00; // OBD2 Mode 09 PID 02
                vinRequest[2] = 0x07; // PID 02
                vinRequest[3] = 0xE0; // VIN request
            }
            else
            {
                vinRequest[0] = 0x14; // UDS Service 22, Sub-function F1 (VIN)
                vinRequest[1] = 0xDA; // UDS Service 22, Sub-function F1 (VIN)
                vinRequest[2] = 0x11; // Service ID for VIN
                vinRequest[3] = 0xF1; // Sub-function for VIN
            }
            vinRequest[4] = 0x02;
            vinRequest[5] = 0x09;
            vinRequest[6] = 0x02;
            Channel.SendMessage(vinRequest);
            Thread.Sleep(2); // Give some time for the request to be sent

            _monitorThread.Join(3000); // Wait for thread to complete
            Channel.StopMsgFilter(filterId);
            Channel.ClearMsgFilters();
        }

        public void SendFlowControl(byte[] canId, int blockSize = 0, int separationTime = 0)
        {
            var fc = new byte[12];
            Array.Copy(canId, 0, fc, 0, 4);
            fc[4] = 0x30;

            //if(BitMode == BitType.BITS_29)
            //{
            //    fc[7] = 0x55;
            //    fc[8] = 0x55;
            //    fc[9] = 0x55;
            //    fc[10] = 0x55;
            //    fc[11] = 0x55;
            //}

            Channel.SendMessage(fc);

            //Debug.WriteLine($"Flow Control sent: {BitConverter.ToString(fc)} (Time: {DateTime.Now:HH:mm:ss.fff})");
        }

        private void IsoTpParser_FrameReceived(object sender, IsoTpFrameEventArgs e)
        {
            switch (e.FrameType)
            {
                case IsoTpFrameType.SingleFrame:
                    // Handle single frame (e.Payload)
                    break;

                case IsoTpFrameType.FirstFrame:
                    // Insert a small delay to ensure timing is right for flow control
                    Thread.Sleep(2);

                    // Always send FC to the sender of the First Frame
                    SendFlowControl(
                        BitMode == BitType.BITS_11
                            ? new byte[] { 0x00, 0x00, 0x07, 0xE0 }
                            : new byte[] { 0x14, 0xDA, 0x11, 0xF1 },
                        blockSize: 0,
                        separationTime: 2
                    );
                    Thread.Sleep(2);

                    // Print partial VIN for debugging
                    Vin = ExtractPartialVin(e.Payload);
                    Debug.WriteLine($"VIN (partial): {Vin} (Time: {DateTime.Now:HH:mm:ss.fff})");

                    break;

                case IsoTpFrameType.ConsecutiveFrame:
                    string newPartialVin = ExtractPartialVin(e.Payload);
                    if (!string.IsNullOrEmpty(newPartialVin) && newPartialVin.Length > 16)
                    {
                        Vin = newPartialVin;
                        //mainForm.UpdateSelectedVehicle(newPartialVin, "", "");
                    }
                    break;

            }
        }

        string ExtractPartialVin(byte[] payload)
        {
            // Handle OBD2 VIN
            if (payload.Length >= 2 && payload[0] == 0x49 && payload[1] == 0x02)
            {
                int vinStart = 3;
                if (payload.Length > vinStart)
                {
                    int vinLen = Math.Min(17, payload.Length - vinStart);
                    if (vinLen > 0)
                    {
                        return System.Text.Encoding.ASCII.GetString(payload, vinStart, vinLen);
                    }
                }
            }
            // Handle UDS VIN
            else if (payload.Length >= 3 && payload[0] == 0x62 && payload[1] == 0xF1 && payload[2] == 0x90)
            {
                int vinStart = 3;
                if (payload.Length > vinStart)
                {
                    int vinLen = Math.Min(17, payload.Length - vinStart);
                    if (vinLen > 0)
                    {
                        return System.Text.Encoding.ASCII.GetString(payload, vinStart, vinLen);
                    }
                }
            }
            return string.Empty;
        }

        public void Dispose()
        {
            Channel?.Dispose();
            Device?.Dispose();
            Api?.Dispose();
            Channel = null;
            Device = null;
            Api = null;
        }
    }
}
