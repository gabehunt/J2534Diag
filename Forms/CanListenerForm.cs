using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SAE.J2534;

namespace J2534Diag
{
    public partial class CanListenerForm : Form
    {
        private const int MAX_RECORDS = 10000;
        private Thread _listenThread;
        private CancellationTokenSource _cts;
        private J2534Manager j2534Manager;
        private long _messageIndex = 0;
        private bool _isListening = false;

        private readonly BindingList<CanMessage> _messages = new BindingList<CanMessage>();
        private readonly List<CanMessage> _allMessages = new List<CanMessage>();
        private readonly object _allMessagesLock = new object();
        private readonly ConcurrentQueue<byte[]> _pendingRawData = new ConcurrentQueue<byte[]>();
        private long _lastFilteredIndex = 0;
        private readonly HashSet<(string ArbId, string Data)> _distinctSet = new HashSet<(string, string)>();
        private Thread _processingThread;
        private volatile bool _processingActive = false;

        public CanListenerForm(J2534Manager j2534Manager)
        {
            InitializeComponent();

            dataGridViewMessages.AutoGenerateColumns = false;
            dataGridViewMessages.DataSource = _messages;

            dataGridViewMessages.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Index",
                HeaderText = "Index",
                DataPropertyName = "Index",
                Width = 60
            });
            dataGridViewMessages.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ArbId",
                HeaderText = "ArbId",
                DataPropertyName = "ArbId",
                Width = 120
            });
            dataGridViewMessages.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ArbIdDescription",
                HeaderText = "ArbId Description",
                DataPropertyName = "ArbIdDescription",
                Width = 200
            });
            dataGridViewMessages.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Data",
                HeaderText = "Data",
                DataPropertyName = "Data",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            this.Load += CanListenerForm_Load;
            this.FormClosing += CanListenerForm_FormClosing;
            this.j2534Manager = j2534Manager;
        }

        private void CanListenerForm_Load(object sender, EventArgs e)
        {
        }

        private void ListenLoop()
        {
            int count = 0;
            var channel = j2534Manager.Channel;
            while (!_cts.IsCancellationRequested && j2534Manager.Channel != null && !j2534Manager.Channel.IsDisposed)
            {
                try
                {
                    var resp = channel.GetMessages(200, 5);
                    foreach (var msg in resp.Messages)
                    {
                        if(count >= MAX_RECORDS)
                        {
                            StopListening();
                            return;
                        }
                        _pendingRawData.Enqueue(msg.Data);
                        count++;
                    }
                }
                catch(J2534Exception ex)
                {
                    StopListening();
                    break;
                }
                Thread.Sleep(1);
            }
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"CAN-{DateTime.Now.ToString("yyyyMMdd-hhmmss")}.csv"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var writer = new System.IO.StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                        {
                            // Write header
                            writer.WriteLine("Index,ArbId,Data");
                            foreach (var msg in _messages)
                            {
                                writer.WriteLine($"{msg.Index},\"{msg.ArbId}\",\"{msg.Data}\"");
                            }
                        }
                        MessageBox.Show("Export complete.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Export failed: " + ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void chkDistinctOnly_CheckedChanged(object sender, EventArgs e)
        {
            ApplyFilterAndDistinct();
        }
        private int _uiTimerBusy = 0;

        private void ProcessingLoop()
        {
            while (_processingActive && !_cts.IsCancellationRequested)
            {
                try
                {
                    var newMessages = new List<CanMessage>();
                    int batchSize = 200;
                    int count = 0;
                    int allMessagesCount = _allMessages.Count; //aprox count is fine no need to lock
                    var mylist = new List<byte[]>();
                    while (_pendingRawData.TryDequeue(out var data) && count < batchSize && allMessagesCount + count < MAX_RECORDS && _processingActive && !_cts.IsCancellationRequested)
                    {
                        mylist.Add(data);
                    }

                    foreach (var data in mylist)
                    {
                        if (!_processingActive || _cts.IsCancellationRequested)
                        {
                            return;
                        }
                        long index = Interlocked.Increment(ref _messageIndex);
                        uint arbIdValue = (uint)(data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3]);
                        string arbId = $"0x{arbIdValue:X3}";
                        string arbIdDescription = GmCanId11Bit.KnownCanIds.TryGetValue(arbIdValue, out var desc) ? desc : "";
                        string payload = BitConverter.ToString(data, 4, data.Length - 4);
                        var msg = new CanMessage { Index = index, ArbId = arbId, ArbIdDescription = arbIdDescription, Data = payload };
                        newMessages.Add(msg);
                        count++;
                    }

                    if (newMessages.Count > 0)
                    {
                        lock (_allMessagesLock)
                        {
                            _allMessages.AddRange(newMessages);

                        }
                    }

                    // Only process/apply new messages since last filter
                    List<CanMessage> snapshot;
                    lock (_allMessagesLock)
                    {
                        snapshot = _allMessages.Where(m => m.Index > _lastFilteredIndex).OrderBy(m => m.Index).ToList();
                    }

                    if (snapshot.Count == 0)
                        continue;

                    string input = txtFilter.InvokeRequired
                        ? (string)txtFilter.Invoke(new Func<string>(() => txtFilter.Text.Trim()))
                        : txtFilter.Text.Trim();
                    bool hasFilter = !string.IsNullOrEmpty(input);
                    if (hasFilter && !input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        input = "0x" + input;

                    bool distinct = chkDistinctOnly.InvokeRequired
                        ? (bool)chkDistinctOnly.Invoke(new Func<bool>(() => chkDistinctOnly.Checked))
                        : chkDistinctOnly.Checked;

                    if (!_processingActive || _cts.IsCancellationRequested)
                    {
                        return;
                    }

                    var toAppend = new List<CanMessage>();

                    if (distinct)
                    {
                        // Build set of existing (ArbId, Data) in the grid
                        HashSet<(string, string)> localDistinctSet;
                        lock (_distinctSet)
                        {
                            if (_messages.Count == 0)
                                _distinctSet.Clear();
                            localDistinctSet = new HashSet<(string, string)>(_distinctSet);
                        }

                        foreach (var msg in snapshot)
                        {
                            if (hasFilter && !string.Equals(msg.ArbId, input, StringComparison.OrdinalIgnoreCase))
                                continue;

                            var key = (msg.ArbId, msg.Data);
                            if (localDistinctSet.Add(key))
                            {
                                toAppend.Add(msg);
                                lock (_distinctSet)
                                {
                                    _distinctSet.Add(key);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var msg in snapshot)
                        {
                            if (hasFilter && !string.Equals(msg.ArbId, input, StringComparison.OrdinalIgnoreCase))
                                continue;
                            toAppend.Add(msg);
                        }
                    }

                    if (toAppend.Count > 0)
                    {
                        _lastFilteredIndex = toAppend.Last().Index;
                        BeginInvoke(new Action(() =>
                        {
                            foreach (var msg in toAppend)
                                _messages.Add(msg);
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Processing error: " + ex.Message);
                    return;
                }
                Thread.Sleep(5); // Reduce CPU usage
            }
        }

        private void CanListenerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopListening();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            StartListening();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopListening();
        }

        public void StartListening()
        {
            if (_isListening) return;

            _allMessages.Clear();
            _messages.Clear();

            _cts = new CancellationTokenSource();

            _processingActive = true;
            _processingThread = new Thread(ProcessingLoop) { IsBackground = true };
            _processingThread.Start();

            // Set up open mask filter
            if (j2534Manager.Channel != null && !j2534Manager.Channel.IsDisposed)
            {
                j2534Manager.Channel.ClearMsgFilters();
                var openFilter = new MessageFilter
                {
                    FilterType = Filter.PASS_FILTER,
                    Mask = new byte[] { 0x00, 0x00, 0x00, 0x00 },
                    Pattern = new byte[] { 0x00, 0x00, 0x00, 0x00 },
                    FlowControl = new byte[] { 0x00, 0x00, 0x00, 0x00 },
                    TxFlags = j2534Manager.BitMode == BitType.BITS_11 ? TxFlag.NONE : TxFlag.CAN_29BIT_ID
                };
                j2534Manager.Channel.StartMsgFilter(openFilter);
            }

            _listenThread = new Thread(ListenLoop) { IsBackground = true };
            _listenThread.Start();
            _isListening = true;

            btnStart.Enabled = false;
            btnStop.Enabled = true;
        }

        public void StopListening()
        {
            try
            {
                if (!_isListening) return;
                if (j2534Manager.Channel != null && !j2534Manager.Channel.IsDisposed)
                {
                    j2534Manager.Channel.ClearMsgFilters();
                    j2534Manager.Channel.ClearTxBuffer();
                    j2534Manager.Channel.ClearRxBuffer();
                }
            }
            catch { }
            try
            {
                _cts?.Cancel();
                _isListening = false;
                _processingActive = false;
                _listenThread?.Join(500);
                _processingThread?.Join(500);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            // Ensure UI updates are on the UI thread
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    btnStart.Enabled = true;
                    btnStop.Enabled = false;
                }));
            }
            else
            {
                btnStart.Enabled = true;
                btnStop.Enabled = false;
            }
        }

        public class CanMessage
        {
            public long Index { get; set; }
            public string ArbId { get; set; }
            public string ArbIdDescription { get; set; }
            public string Data { get; set; }
        }

        private void btnLoadCsv_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv"
            })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var lines = System.IO.File.ReadAllLines(ofd.FileName);
                        if (lines.Length < 2)
                        {
                            MessageBox.Show("CSV file is empty or missing data.", "Load CSV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Parse header
                        var header = lines[0].Split(',').Select(h => h.Trim().ToLowerInvariant()).ToArray();
                        int idxIndex = Array.IndexOf(header, "index");
                        int idxArbId = Array.IndexOf(header, "arbid");
                        int idxArbIdDesc = Array.IndexOf(header, "arbid description");
                        int idxData = Array.IndexOf(header, "data");

                        if (idxIndex == -1 || idxArbId == -1 || idxData == -1)
                        {
                            MessageBox.Show("CSV header missing required columns.", "Load CSV", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        _allMessages.Clear();
                        _messages.Clear();
                        foreach (var line in lines.Skip(1))
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            var parts = SplitCsvLine(line);
                            if (parts.Length < header.Length) continue;

                            long index = 0;
                            long.TryParse(parts[idxIndex].Trim(), out index);

                            string arbIdStr = parts[idxArbId].Trim().Trim('"');
                            string arbIdDescription = "";
                            string data = idxData < parts.Length ? parts[idxData].Trim().Trim('"') : "";

                            // Try to get ArbIdDescription from file, else lookup
                            if (idxArbIdDesc != -1 && idxArbIdDesc < parts.Length)
                            {
                                arbIdDescription = parts[idxArbIdDesc].Trim().Trim('"');
                            }
                            else
                            {
                                // Try to parse ArbId as hex (e.g., 0x100)
                                if (arbIdStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (uint.TryParse(arbIdStr.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out uint arbIdVal))
                                    {
                                        arbIdDescription = GmCanId11Bit.KnownCanIds.TryGetValue(arbIdVal, out var desc) ? desc : "";
                                    }
                                }
                                else if (uint.TryParse(arbIdStr, out uint arbIdVal))
                                {
                                    arbIdDescription = GmCanId11Bit.KnownCanIds.TryGetValue(arbIdVal, out var desc) ? desc : "";
                                }
                            }

                            var msg = new CanMessage
                            {
                                Index = index,
                                ArbId = arbIdStr,
                                ArbIdDescription = arbIdDescription,
                                Data = data
                            };

                            _allMessages.Add(msg);
                        }
                        ApplyFilterAndDistinct();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to load CSV: " + ex.Message, "Load CSV", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Simple CSV splitter (handles quoted fields)
        private static string[] SplitCsvLine(string line)
        {
            var result = new System.Collections.Generic.List<string>();
            var sb = new System.Text.StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            result.Add(sb.ToString());
            return result.ToArray();
        }

        private void txtFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ApplyFilterAndDistinct();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void txtFilter_Leave(object sender, EventArgs e)
        {
            ApplyFilterAndDistinct();
        }

        private void dataGridViewMessages_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                txtFilter.Text = "";
                ApplyFilterAndDistinct();
            }
        }

        private void ApplyFilterAndDistinct()
        {
            string input = txtFilter.Text.Trim();
            bool hasFilter = !string.IsNullOrEmpty(input);

            if (hasFilter && !input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                input = "0x" + input;

            List<CanMessage> snapshot;
            lock (_allMessagesLock)
            {
                snapshot = _allMessages.ToList();
            }

            IEnumerable<CanMessage> filtered = snapshot;

            if (hasFilter)
                filtered = filtered.Where(m => string.Equals(m.ArbId, input, StringComparison.OrdinalIgnoreCase));

            if (chkDistinctOnly.Checked)
                filtered = filtered.GroupBy(m => new { m.ArbId, m.Data }).Select(g => g.First());

            _messages.RaiseListChangedEvents = false;
            _messages.Clear();
            foreach (var msg in filtered)
                _messages.Add(msg);
            _messages.RaiseListChangedEvents = true;
            _messages.ResetBindings();
        }

        private void dataGridViewMessages_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dataGridViewMessages.Rows[e.RowIndex];
                var arbId = row.Cells["ArbId"].Value as string;
                if (!string.IsNullOrEmpty(arbId))
                {
                    txtFilter.Text = arbId;
                    ApplyFilterAndDistinct();
                }
            }
        }


    }
}
