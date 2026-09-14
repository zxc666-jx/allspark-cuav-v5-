using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MissionPlanner.HIL
{
    internal sealed class FleetSupervisorClient : IDisposable
    {
        private readonly Uri baseUri;

        public FleetSupervisorClient(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("总控 API 地址不能为空", "endpoint");
            if (!endpoint.EndsWith("/", StringComparison.Ordinal))
                endpoint += "/";
            baseUri = new Uri(endpoint, UriKind.Absolute);
            if (baseUri.Host != "127.0.0.1" && baseUri.Host != "localhost")
                throw new ArgumentException("多机总控 API 只允许使用 127.0.0.1 或 localhost");
        }

        public Task<JObject> GetFleetAsync(CancellationToken token)
        {
            return SendAsync("GET", "api/v1/fleet", null, token);
        }

        public Task<JObject> StartAllAsync(CancellationToken token)
        {
            return SendAsync("POST", "api/v1/start-all", new JObject(), token);
        }

        public Task<JObject> StopAllAsync(CancellationToken token)
        {
            return SendAsync("POST", "api/v1/stop-all", new JObject(), token);
        }

        public Task<JObject> ShutdownAsync(CancellationToken token)
        {
            return SendAsync("POST", "api/v1/shutdown", new JObject(), token);
        }

        public Task<JObject> VehicleActionAsync(int sysid, string action, CancellationToken token)
        {
            return SendAsync("POST", "api/v1/vehicles/" + sysid + "/process/" + action,
                new JObject(), token);
        }

        private async Task<JObject> SendAsync(string method, string relativePath, JObject body,
            CancellationToken token)
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(baseUri, relativePath));
            request.Method = method;
            request.Accept = "application/json";
            request.ContentType = "application/json; charset=utf-8";
            request.Timeout = 3000;
            request.ReadWriteTimeout = 3000;

            using (token.Register(request.Abort))
            {
                try
                {
                    if (body != null)
                    {
                        var bytes = Encoding.UTF8.GetBytes(body.ToString(Formatting.None));
                        request.ContentLength = bytes.Length;
                        using (var stream = await request.GetRequestStreamAsync().ConfigureAwait(false))
                            await stream.WriteAsync(bytes, 0, bytes.Length, token).ConfigureAwait(false);
                    }
                    using (var response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
                    using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                        return JObject.Parse(await reader.ReadToEndAsync().ConfigureAwait(false));
                }
                catch (WebException ex)
                {
                    if (token.IsCancellationRequested)
                        throw new OperationCanceledException(token);
                    throw new InvalidOperationException("无法连接多机总控：" + ex.Message, ex);
                }
            }
        }

        public void Dispose()
        {
        }
    }

    public sealed class JSBSimFleetControlForm : Form
    {
        private const int MaximumVehicles = 10;
        // 这里选择的是 JSBSim 与飞控之间的原始 JBS1/JBO1 硬件在环链路。
        // Mission Planner 用于遥测和飞行指令的 MAVLink 连接仍需单独配置。
        private const string SerialTransport = "串口 JBS";
        private const string UdpTransport = "UDP JBS";

        private readonly TextBox pythonPath = new TextBox();
        private readonly TextBox configPath = new TextBox();
        private readonly TextBox apiAddress = new TextBox();
        private readonly NumericUpDown vehicleCount = new NumericUpDown();
        private readonly Button applyVehicleCountButton = new Button();
        private readonly Button saveConfigurationButton = new Button();
        private readonly Button reloadConfigurationButton = new Button();
        private readonly Button configureUdpButton = new Button();
        private readonly Button selectAllButton = new Button();
        private readonly Button clearSelectionButton = new Button();
        private readonly Button startAllButton = new Button();
        private readonly Button stopAllButton = new Button();
        private readonly Button shutdownButton = new Button();
        private readonly Button refreshButton = new Button();
        private readonly Button startVehicleButton = new Button();
        private readonly Button stopVehicleButton = new Button();
        private readonly Button restartVehicleButton = new Button();
        private readonly Button armButton = new Button();
        private readonly Button autoButton = new Button();
        private readonly Button guidedButton = new Button();
        private readonly Button environmentButton = new Button();
        private readonly Button faultButton = new Button();
        private readonly Label statusLabel = new Label();
        private readonly DataGridView vehiclesGrid = new DataGridView();
        private readonly TextBox console = new TextBox();
        private readonly System.Windows.Forms.Timer statusTimer = new System.Windows.Forms.Timer();

        private FleetSupervisorClient client;
        private Process ownedProcess;
        private bool pollInProgress;
        private bool closingAfterStop;
        private bool loadingGrid;
        private bool configurationDirty;

        public JSBSimFleetControlForm()
        {
            Text = "JSBSim HIL 多机总控";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(1120, 680);
            Size = new Size(1420, 840);
            AutoScaleMode = AutoScaleMode.Dpi;
            BuildLayout();
            ConfigureGrid();
            LoadDefaults();
            statusTimer.Interval = 1000;
            statusTimer.Tick += PollStatus;
            FormClosing += FleetFormClosing;
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(8)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 42));

            var paths = new TableLayoutPanel {Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 3};
            paths.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82));
            paths.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            paths.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            AddPathRow(paths, 0, "Python", pythonPath, BrowsePython);
            AddPathRow(paths, 1, "多机配置", configPath, BrowseConfig);
            AddPathRow(paths, 2, "总控 API", apiAddress, null);
            apiAddress.TextChanged += delegate
            {
                if (!loadingGrid)
                    configurationDirty = true;
            };

            vehicleCount.Minimum = 1;
            vehicleCount.Maximum = MaximumVehicles;
            vehicleCount.Value = 2;
            vehicleCount.Width = 55;
            applyVehicleCountButton.Text = "应用架数";
            saveConfigurationButton.Text = "保存连接配置";
            reloadConfigurationButton.Text = "重新读取配置";
            configureUdpButton.Text = "设置选中 JBS UDP...";
            selectAllButton.Text = "全选已启用";
            clearSelectionButton.Text = "清空选择";
            applyVehicleCountButton.Click += ApplyVehicleCountClicked;
            saveConfigurationButton.Click += SaveConfigurationClicked;
            reloadConfigurationButton.Click += ReloadConfigurationClicked;
            configureUdpButton.Click += ConfigureUdpClicked;
            selectAllButton.Click += delegate { SetEnabledVehicleSelection(true); };
            clearSelectionButton.Click += delegate { SetEnabledVehicleSelection(false); };
            var configurationButtons = Flow(new Label
            {
                Text = "飞机数量",
                AutoSize = true,
                Margin = new Padding(3, 11, 2, 3)
            }, vehicleCount, applyVehicleCountButton, saveConfigurationButton, reloadConfigurationButton,
                configureUdpButton, selectAllButton, clearSelectionButton);

            startAllButton.Text = "启动全部";
            stopAllButton.Text = "停止全部";
            shutdownButton.Text = "关闭总控";
            refreshButton.Text = "刷新状态";
            startAllButton.Click += StartAllClicked;
            stopAllButton.Click += StopAllClicked;
            shutdownButton.Click += ShutdownClicked;
            refreshButton.Click += PollStatus;
            stopAllButton.Enabled = false;
            shutdownButton.Enabled = false;
            var fleetButtons = Flow(startAllButton, stopAllButton, shutdownButton, refreshButton, statusLabel);
            statusLabel.AutoSize = true;
            statusLabel.Text = "未启动";
            statusLabel.Margin = new Padding(18, 9, 3, 3);

            startVehicleButton.Text = "启动选中";
            stopVehicleButton.Text = "停止选中";
            restartVehicleButton.Text = "重启选中";
            startVehicleButton.Click += delegate { RunVehicleAction("start"); };
            stopVehicleButton.Click += delegate { RunVehicleAction("stop"); };
            restartVehicleButton.Click += delegate { RunVehicleAction("restart"); };
            armButton.Text = "一键解锁所选";
            autoButton.Text = "所选切 AUTO";
            guidedButton.Text = "所选切 GUIDED";
            environmentButton.Text = "多机环境干扰...";
            faultButton.Text = "多机故障注入...";
            armButton.Click += async delegate { await RunMavlinkActionAsync("解锁", null); };
            autoButton.Click += async delegate { await RunMavlinkActionAsync("切换 AUTO", "AUTO"); };
            guidedButton.Click += async delegate { await RunMavlinkActionAsync("切换 GUIDED", "GUIDED"); };
            environmentButton.Click += OpenFleetEnvironment;
            faultButton.Click += OpenFleetFaultInjection;
            var vehicleButtons = Flow(startVehicleButton, stopVehicleButton, restartVehicleButton,
                armButton, autoButton, guidedButton, environmentButton, faultButton);

            console.Dock = DockStyle.Fill;
            console.Multiline = true;
            console.ReadOnly = true;
            console.ScrollBars = ScrollBars.Both;
            console.WordWrap = false;
            console.Font = new Font(FontFamily.GenericMonospace, 9F);

            root.Controls.Add(paths, 0, 0);
            root.Controls.Add(configurationButtons, 0, 1);
            root.Controls.Add(fleetButtons, 0, 2);
            root.Controls.Add(vehicleButtons, 0, 3);
            root.Controls.Add(vehiclesGrid, 0, 4);
            root.Controls.Add(console, 0, 5);
            Controls.Add(root);
        }

        private static FlowLayoutPanel Flow(params Control[] controls)
        {
            var panel = new FlowLayoutPanel {Dock = DockStyle.Fill, WrapContents = false};
            foreach (var control in controls)
            {
                control.AutoSize = true;
                control.Margin = new Padding(3, 5, 8, 3);
                panel.Controls.Add(control);
            }
            return panel;
        }

        private static void AddPathRow(TableLayoutPanel table, int row, string label, TextBox textBox,
            EventHandler browse)
        {
            var caption = new Label {Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft};
            textBox.Dock = DockStyle.Fill;
            table.Controls.Add(caption, 0, row);
            table.Controls.Add(textBox, 1, row);
            if (browse != null)
            {
                var button = new Button {Text = "浏览...", Dock = DockStyle.Fill};
                button.Click += browse;
                table.Controls.Add(button, 2, row);
            }
        }

        private void ConfigureGrid()
        {
            vehiclesGrid.Dock = DockStyle.Fill;
            vehiclesGrid.AllowUserToAddRows = false;
            vehiclesGrid.AllowUserToDeleteRows = false;
            vehiclesGrid.AllowUserToOrderColumns = true;
            vehiclesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            vehiclesGrid.MultiSelect = true;
            vehiclesGrid.RowHeadersVisible = false;
            vehiclesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            vehiclesGrid.EditMode = DataGridViewEditMode.EditOnEnter;
            vehiclesGrid.Columns.Add(new DataGridViewCheckBoxColumn {Name = "selected", HeaderText = "选择", Width = 48});
            vehiclesGrid.Columns.Add(new DataGridViewCheckBoxColumn {Name = "enabled", HeaderText = "启用", Width = 48});
            vehiclesGrid.Columns.Add(new DataGridViewTextBoxColumn {Name = "sysid", HeaderText = "SYSID", Width = 55});
            vehiclesGrid.Columns.Add(new DataGridViewTextBoxColumn {Name = "name", HeaderText = "名称", Width = 105});
            var transport = new DataGridViewComboBoxColumn
            {
                Name = "transport",
                HeaderText = "HIL链路",
                Width = 75,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
            };
            transport.Items.AddRange(SerialTransport, UdpTransport);
            vehiclesGrid.Columns.Add(transport);
            var serialPort = new DataGridViewComboBoxColumn
            {
                Name = "serialPort",
                HeaderText = "串口",
                Width = 78,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
            };
            for (var number = 1; number <= 64; number++)
                serialPort.Items.Add("COM" + number);
            vehiclesGrid.Columns.Add(serialPort);
            var baud = new DataGridViewComboBoxColumn
            {
                Name = "baud",
                HeaderText = "波特率",
                Width = 92,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
            };
            baud.Items.AddRange("57600", "115200", "230400", "460800", "921600", "1000000", "1500000", "2000000");
            vehiclesGrid.Columns.Add(baud);
            vehiclesGrid.Columns.Add(new DataGridViewTextBoxColumn {Name = "udpHost", HeaderText = "LQ目标IP", Width = 125});
            vehiclesGrid.Columns.Add(new DataGridViewTextBoxColumn {Name = "udpPort", HeaderText = "JBS UDP端口", Width = 90});
            vehiclesGrid.Columns.Add(new DataGridViewTextBoxColumn {Name = "apiPort", HeaderText = "控制端口", Width = 78});
            vehiclesGrid.Columns.Add(new DataGridViewTextBoxColumn {Name = "state", HeaderText = "状态", Width = 72, ReadOnly = true});
            vehiclesGrid.Columns.Add(new DataGridViewTextBoxColumn {Name = "pid", HeaderText = "PID", Width = 60, ReadOnly = true});
            vehiclesGrid.Columns.Add(new DataGridViewTextBoxColumn {Name = "error", HeaderText = "最近错误", Width = 240, ReadOnly = true});
            vehiclesGrid.Columns.Add(new DataGridViewTextBoxColumn {Name = "sourceConfig", Visible = false});
            vehiclesGrid.CellValueChanged += VehicleGridCellValueChanged;
            vehiclesGrid.CurrentCellDirtyStateChanged += delegate
            {
                if (vehiclesGrid.IsCurrentCellDirty)
                    vehiclesGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            vehiclesGrid.CellDoubleClick += VehicleGridCellDoubleClick;
            vehiclesGrid.DataError += delegate(object sender, DataGridViewDataErrorEventArgs e) { e.ThrowException = false; };
        }

        private void LoadDefaults()
        {
            apiAddress.Text = "http://127.0.0.1:8750";
            InitializeVehicleRows();
            var jsbsimDirectory = FindJSBSimDirectory();
            if (jsbsimDirectory == null)
                return;
            var python = Path.Combine(jsbsimDirectory, ".venv", "Scripts", "python.exe");
            var config = Path.Combine(jsbsimDirectory, "config", "fleet_example.yaml");
            if (File.Exists(python))
                pythonPath.Text = python;
            if (File.Exists(config))
                configPath.Text = config;
            InitializeVehicleRows();
            if (File.Exists(configPath.Text))
                LoadVehicleRowsFromConfiguration(configPath.Text);
        }

        private sealed class FleetVehicleConfigEntry
        {
            public string Name;
            public string Config;
            public bool Enabled = true;
        }

        private void InitializeVehicleRows()
        {
            loadingGrid = true;
            try
            {
                vehiclesGrid.Rows.Clear();
                for (var index = 1; index <= MaximumVehicles; index++)
                {
                    var enabledByDefault = index <= 2;
                    var rowIndex = vehiclesGrid.Rows.Add(enabledByDefault, enabledByDefault, index,
                        index + "号飞机", SerialTransport, index == 1 ? "COM31" : index == 2 ? "COM35" : "COM" + (40 + index),
                        "921600", "192.168.1." + (200 + index), 14600 + index, 8764 + index, "未启动", "", "", "");
                    UpdateTransportCells(vehiclesGrid.Rows[rowIndex]);
                }
                vehicleCount.Value = 2;
                configurationDirty = false;
            }
            finally
            {
                loadingGrid = false;
            }
        }

        private void LoadVehicleRowsFromConfiguration(string fleetPath)
        {
            var entries = ParseFleetEntries(fleetPath);
            if (entries.Count == 0)
                return;

            InitializeVehicleRows();
            loadingGrid = true;
            try
            {
                var enabledCount = 0;
                for (var index = 0; index < entries.Count && index < MaximumVehicles; index++)
                {
                    var entry = entries[index];
                    var row = vehiclesGrid.Rows[index];
                    var vehiclePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(fleetPath), entry.Config));
                    var values = ParseTopLevelYamlValues(vehiclePath);
                    var sysid = IntValue(values, "vehicle.sysid", index + 1);
                    var serialEnabled = BoolValue(values, "serial_hil.enabled", true);
                    var udpEnabled = BoolValue(values, "udp_hil.enabled",
                        BoolValue(values, "mavlink.enabled", false));
                    var transport = udpEnabled && !serialEnabled ? UdpTransport : SerialTransport;

                    row.Cells["selected"].Value = entry.Enabled;
                    row.Cells["enabled"].Value = entry.Enabled;
                    row.Cells["sysid"].Value = sysid;
                    row.Cells["name"].Value = string.IsNullOrWhiteSpace(entry.Name) ? sysid + "号飞机" : entry.Name;
                    row.Cells["transport"].Value = transport;
                    row.Cells["serialPort"].Value = Value(values, "serial_hil.device", "COM" + (41 + index));
                    row.Cells["baud"].Value = Value(values, "serial_hil.baud", "921600");
                    row.Cells["udpHost"].Value = Value(values, "udp_hil.remote_host",
                        Value(values, "mavlink.host", "192.168.1." + (200 + sysid)));
                    row.Cells["udpPort"].Value = IntValue(values, "udp_hil.remote_port",
                        IntValue(values, "udp_hil.bind_port", 14600 + sysid));
                    row.Cells["apiPort"].Value = IntValue(values, "control_api.port", 8764 + sysid);
                    row.Cells["sourceConfig"].Value = vehiclePath;
                    UpdateTransportCells(row);
                    if (entry.Enabled)
                        enabledCount++;
                }
                vehicleCount.Value = Math.Max(1, Math.Min(MaximumVehicles, enabledCount));
                configurationDirty = false;
            }
            finally
            {
                loadingGrid = false;
            }
        }

        private static List<FleetVehicleConfigEntry> ParseFleetEntries(string path)
        {
            var result = new List<FleetVehicleConfigEntry>();
            if (!File.Exists(path))
                return result;
            var text = File.ReadAllText(path);
            if (text.TrimStart().StartsWith("{", StringComparison.Ordinal))
            {
                var document = JObject.Parse(text);
                var vehicles = document["vehicles"] as JArray;
                if (vehicles != null)
                    foreach (var token in vehicles.OfType<JObject>())
                        result.Add(new FleetVehicleConfigEntry
                        {
                            Name = (string) token["name"] ?? "",
                            Config = (string) token["config"] ?? "",
                            Enabled = (bool?) token["enabled"] ?? true
                        });
                return result.Where(item => !string.IsNullOrWhiteSpace(item.Config)).ToList();
            }
            FleetVehicleConfigEntry current = null;
            var inVehicles = false;
            foreach (var raw in text.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None))
            {
                var line = StripYamlComment(raw);
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                if (!char.IsWhiteSpace(line[0]))
                {
                    inVehicles = line.Trim() == "vehicles:";
                    continue;
                }
                if (!inVehicles)
                    continue;
                var trimmed = line.Trim();
                if (trimmed.StartsWith("- ", StringComparison.Ordinal))
                {
                    if (current != null)
                        result.Add(current);
                    current = new FleetVehicleConfigEntry();
                    trimmed = trimmed.Substring(2).Trim();
                }
                if (current == null)
                    continue;
                var separator = trimmed.IndexOf(':');
                if (separator <= 0)
                    continue;
                var key = trimmed.Substring(0, separator).Trim();
                var value = UnquoteYaml(trimmed.Substring(separator + 1).Trim());
                if (key == "name")
                    current.Name = value;
                else if (key == "config")
                    current.Config = value;
                else if (key == "enabled")
                    current.Enabled = !value.Equals("false", StringComparison.OrdinalIgnoreCase);
            }
            if (current != null)
                result.Add(current);
            return result.Where(item => !string.IsNullOrWhiteSpace(item.Config)).ToList();
        }

        private static Dictionary<string, string> ParseTopLevelYamlValues(string path)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(path))
                return result;
            var text = File.ReadAllText(path);
            if (text.TrimStart().StartsWith("{", StringComparison.Ordinal))
            {
                var document = JObject.Parse(text);
                foreach (var key in new[]
                {
                    "vehicle.sysid", "serial_hil.enabled", "serial_hil.device", "serial_hil.baud",
                    "mavlink.enabled", "mavlink.host", "mavlink.port",
                    "udp_hil.enabled", "udp_hil.bind_host", "udp_hil.bind_port",
                    "udp_hil.remote_host", "udp_hil.remote_port", "control_api.port"
                })
                {
                    var token = document.SelectToken(key);
                    if (token != null)
                        result[key] = token.ToString();
                }
                return result;
            }
            string section = null;
            foreach (var raw in text.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None))
            {
                var line = StripYamlComment(raw);
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                if (!char.IsWhiteSpace(line[0]))
                {
                    var trimmedSection = line.Trim();
                    section = trimmedSection.EndsWith(":", StringComparison.Ordinal)
                        ? trimmedSection.Substring(0, trimmedSection.Length - 1)
                        : null;
                    continue;
                }
                if (section == null || line.TakeWhile(char.IsWhiteSpace).Count() != 2)
                    continue;
                var trimmed = line.Trim();
                var separator = trimmed.IndexOf(':');
                if (separator <= 0)
                    continue;
                var key = trimmed.Substring(0, separator).Trim();
                result[section + "." + key] = UnquoteYaml(trimmed.Substring(separator + 1).Trim());
            }
            return result;
        }

        private static string StripYamlComment(string line)
        {
            var comment = line.IndexOf('#');
            return comment < 0 ? line : line.Substring(0, comment);
        }

        private static string UnquoteYaml(string value)
        {
            if (value.Length >= 2 && ((value[0] == '\"' && value[value.Length - 1] == '\"') ||
                                      (value[0] == '\'' && value[value.Length - 1] == '\'')))
                return value.Substring(1, value.Length - 2);
            return value;
        }

        private static string Value(IDictionary<string, string> values, string key, string fallback)
        {
            string value;
            return values.TryGetValue(key, out value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;
        }

        private static int IntValue(IDictionary<string, string> values, string key, int fallback)
        {
            int value;
            return Int32.TryParse(Value(values, key, ""), NumberStyles.Integer, CultureInfo.InvariantCulture,
                out value) ? value : fallback;
        }

        private static bool BoolValue(IDictionary<string, string> values, string key, bool fallback)
        {
            bool value;
            return Boolean.TryParse(Value(values, key, ""), out value) ? value : fallback;
        }

        private void VehicleGridCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (loadingGrid || e.RowIndex < 0)
                return;
            var row = vehiclesGrid.Rows[e.RowIndex];
            var column = vehiclesGrid.Columns[e.ColumnIndex].Name;
            if (column != "selected" && column != "state" && column != "pid" && column != "error")
                configurationDirty = true;
            if (column == "transport")
                UpdateTransportCells(row);
            if (column == "enabled" && CellBoolean(row, "enabled"))
                row.Cells["selected"].Value = true;
        }

        private void VehicleGridCellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;
            var column = vehiclesGrid.Columns[e.ColumnIndex].Name;
            var row = vehiclesGrid.Rows[e.RowIndex];
            if (CellString(row, "transport") == UdpTransport &&
                (column == "transport" || column == "udpHost" || column == "udpPort"))
                ConfigureUdp(row);
        }

        private static void UpdateTransportCells(DataGridViewRow row)
        {
            var udp = CellString(row, "transport") == UdpTransport;
            SetCellAvailability(row.Cells["serialPort"], !udp);
            SetCellAvailability(row.Cells["baud"], !udp);
            SetCellAvailability(row.Cells["udpHost"], udp);
            SetCellAvailability(row.Cells["udpPort"], udp);
        }

        private static void SetCellAvailability(DataGridViewCell cell, bool enabled)
        {
            cell.ReadOnly = !enabled;
            cell.Style.BackColor = enabled ? SystemColors.Window : SystemColors.Control;
            cell.Style.ForeColor = enabled ? SystemColors.WindowText : SystemColors.GrayText;
        }

        private void ApplyVehicleCountClicked(object sender, EventArgs e)
        {
            loadingGrid = true;
            try
            {
                var count = (int) vehicleCount.Value;
                foreach (DataGridViewRow row in vehiclesGrid.Rows)
                {
                    var enabled = row.Index < count;
                    row.Cells["enabled"].Value = enabled;
                    row.Cells["selected"].Value = enabled;
                }
                configurationDirty = true;
            }
            finally
            {
                loadingGrid = false;
            }
        }

        private void SetEnabledVehicleSelection(bool selected)
        {
            loadingGrid = true;
            try
            {
                foreach (DataGridViewRow row in vehiclesGrid.Rows)
                    row.Cells["selected"].Value = selected && CellBoolean(row, "enabled");
            }
            finally
            {
                loadingGrid = false;
            }
        }

        private void ConfigureUdpClicked(object sender, EventArgs e)
        {
            var row = vehiclesGrid.CurrentRow;
            if (row == null)
            {
                MessageBox.Show(this, "请先选择一架飞机。", "UDP 配置", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            row.Cells["transport"].Value = UdpTransport;
            ConfigureUdp(row);
        }

        private void ConfigureUdp(DataGridViewRow row)
        {
            var sysid = ParseCellInt(row, "sysid", row.Index + 1);
            var defaultHost = sysid <= 54 ? "192.168.1." + (200 + sysid) : "192.168.1.201";
            var defaultPort = 14600 + sysid;
            using (var dialog = new Form
            {
                Text = (CellString(row, "name") ?? sysid + "号飞机") + " JBS UDP 设置",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                ClientSize = new Size(470, 170)
            })
            {
                var host = new TextBox {Dock = DockStyle.Fill, Text = CellString(row, "udpHost")};
                var port = new TextBox {Dock = DockStyle.Fill, Text = CellString(row, "udpPort")};
                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(12),
                    ColumnCount = 2,
                    RowCount = 4
                };
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                layout.Controls.Add(new Label {Text = "目标 IP/主机名", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft}, 0, 0);
                layout.Controls.Add(host, 1, 0);
                layout.Controls.Add(new Label {Text = "目标 UDP 端口", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft}, 0, 1);
                layout.Controls.Add(port, 1, 1);
                var hint = new Label
                {
                    Text = "本机监听 0.0.0.0 的同一端口；留空使用：" + defaultHost + ":" + defaultPort,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                layout.Controls.Add(hint, 0, 2);
                layout.SetColumnSpan(hint, 2);
                var buttons = new FlowLayoutPanel {Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft};
                var ok = new Button {Text = "确定", DialogResult = DialogResult.OK, Width = 85};
                var cancel = new Button {Text = "取消", DialogResult = DialogResult.Cancel, Width = 85};
                buttons.Controls.Add(ok);
                buttons.Controls.Add(cancel);
                layout.Controls.Add(buttons, 0, 3);
                layout.SetColumnSpan(buttons, 2);
                dialog.Controls.Add(layout);
                dialog.AcceptButton = ok;
                dialog.CancelButton = cancel;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
                int parsedPort;
                var portText = string.IsNullOrWhiteSpace(port.Text) ? defaultPort.ToString(CultureInfo.InvariantCulture) : port.Text.Trim();
                if (!Int32.TryParse(portText, out parsedPort) || parsedPort < 1 || parsedPort > 65535)
                {
                    MessageBox.Show(this, "UDP 端口必须在 1～65535 之间。", "UDP 配置",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                row.Cells["udpHost"].Value = string.IsNullOrWhiteSpace(host.Text) ? defaultHost : host.Text.Trim();
                row.Cells["udpPort"].Value = parsedPort;
                configurationDirty = true;
                UpdateTransportCells(row);
            }
        }

        private void ReloadConfigurationClicked(object sender, EventArgs e)
        {
            try
            {
                LoadVehicleRowsFromConfiguration(configPath.Text);
                AppendConsole("[配置] 已重新读取 " + configPath.Text + Environment.NewLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "读取多机配置失败", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void SaveConfigurationClicked(object sender, EventArgs e)
        {
            try
            {
                var path = SaveGeneratedConfiguration();
                AppendConsole("[配置] 已生成 " + path + Environment.NewLine);
                MessageBox.Show(this, "连接配置已保存：\r\n" + path +
                                      "\r\n\r\n原配置文件没有被覆盖。", "多机连接配置",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "保存多机配置失败", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private string SaveGeneratedConfiguration()
        {
            vehiclesGrid.EndEdit();
            var enabledRows = vehiclesGrid.Rows.Cast<DataGridViewRow>()
                .Where(row => CellBoolean(row, "enabled")).ToList();
            if (enabledRows.Count == 0 || enabledRows.Count > MaximumVehicles)
                throw new InvalidOperationException("启用飞机数量必须在 1～" + MaximumVehicles + " 之间。");

            var sysids = new HashSet<int>();
            var controlPorts = new HashSet<int>();
            var serialPorts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var udpEndpoints = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            // 每架 UDP HIL 飞机都会创建一个独立的本地 socket。原始 JBS 帧没有
            // MAVLink SYSID 路由能力，因此本地监听端口和远端 LQ 端点都不能复用。
            var udpLocalPorts = new HashSet<int>();
            var jsbsimDirectory = FindJSBSimDirectory();
            if (jsbsimDirectory == null)
                throw new DirectoryNotFoundException("找不到 JSBSim 目录，请从本项目的 MissionPlanner.exe 启动。");
            var configDirectory = Path.Combine(jsbsimDirectory, "config");
            Directory.CreateDirectory(configDirectory);
            var fleetVehicles = new JArray();
            for (var index = 0; index < enabledRows.Count; index++)
            {
                var row = enabledRows[index];
                var sysid = ParseCellInt(row, "sysid", index + 1);
                if (sysid < 1 || sysid > 255 || !sysids.Add(sysid))
                    throw new InvalidOperationException("SYSID 必须在 1～255 之间且不能重复：" + sysid);
                var controlPort = ParseCellInt(row, "apiPort", 8764 + sysid);
                if (controlPort < 1 || controlPort > 65535 || !controlPorts.Add(controlPort))
                    throw new InvalidOperationException("控制 API 端口无效或重复：" + controlPort);
                var isUdp = CellString(row, "transport") == UdpTransport;
                var serial = CellString(row, "serialPort");
                var baud = ParseCellInt(row, "baud", 921600);
                var defaultUdpHost = sysid <= 54 ? "192.168.1." + (200 + sysid) : "192.168.1.201";
                var udpHost = DefaultIfBlank(CellString(row, "udpHost"), defaultUdpHost);
                var udpPort = ParseCellInt(row, "udpPort", 14600 + sysid);
                if (!isUdp && (string.IsNullOrWhiteSpace(serial) || !serialPorts.Add(serial)))
                    throw new InvalidOperationException("串口不能为空且不能重复：" + serial);
                if (!isUdp && baud <= 0)
                    throw new InvalidOperationException("波特率必须大于 0：" + baud);
                if (isUdp && (udpPort < 1 || udpPort > 65535 || !udpEndpoints.Add(udpHost + ":" + udpPort) ||
                              !udpLocalPorts.Add(udpPort)))
                    throw new InvalidOperationException("UDP 目标无效或重复：" + udpHost + ":" + udpPort);

                var name = DefaultIfBlank(CellString(row, "name"), sysid + "号飞机");
                var fileName = "vehicle" + sysid.ToString("000", CultureInfo.InvariantCulture) + "_ui_generated.yaml";
                var vehiclePath = Path.Combine(configDirectory, fileName);
                File.WriteAllText(vehiclePath,
                    BuildVehicleConfiguration(sysid, index, isUdp, serial, baud, udpHost, udpPort, controlPort)
                        .ToString(Formatting.Indented), new UTF8Encoding(false));
                fleetVehicles.Add(new JObject
                {
                    ["name"] = name,
                    ["config"] = fileName,
                    ["enabled"] = true
                });
                row.Cells["sourceConfig"].Value = vehiclePath;
            }

            Uri fleetUri;
            if (!Uri.TryCreate(apiAddress.Text.Trim(), UriKind.Absolute, out fleetUri) ||
                (fleetUri.Host != "127.0.0.1" && fleetUri.Host != "localhost"))
                throw new InvalidOperationException("总控 API 必须是本机地址，例如 http://127.0.0.1:8750。");
            var fleet = new JObject
            {
                ["fleet"] = new JObject
                {
                    ["host"] = fleetUri.Host,
                    ["port"] = fleetUri.Port,
                    ["status_hz"] = 2,
                    ["shutdown_timeout_s"] = 5
                },
                ["vehicles"] = fleetVehicles
            };
            var fleetPath = Path.Combine(configDirectory, "fleet_ui_generated.yaml");
            File.WriteAllText(fleetPath, fleet.ToString(Formatting.Indented), new UTF8Encoding(false));
            configPath.Text = fleetPath;
            configurationDirty = false;
            return fleetPath;
        }

        private static JObject BuildVehicleConfiguration(int sysid, int index, bool udp, string serial,
            int baud, string udpHost, int udpPort, int controlPort)
        {
            var column = index % 5;
            var row = index / 5;
            return new JObject
            {
                ["vehicle"] = new JObject
                {
                    ["sysid"] = sysid,
                    ["mesh_node"] = "aircraft-" + sysid.ToString("00", CultureInfo.InvariantCulture),
                    ["model"] = "c172x"
                },
                ["spawn"] = new JObject
                {
                    ["latitude_deg"] = -34.98106 - row * 0.0008,
                    ["longitude_deg"] = 117.85201 + column * 0.0008,
                    ["altitude_msl_m"] = 41.315,
                    ["terrain_elevation_m"] = 40.0,
                    ["heading_deg"] = 0.0,
                    ["airspeed_mps"] = 0.0
                },
                ["simulation"] = new JObject { ["rate_hz"] = 400, ["status_hz"] = 5, ["realtime"] = true },
                ["control_api"] = new JObject { ["enabled"] = true, ["host"] = "127.0.0.1", ["port"] = controlPort },
                ["environment"] = new JObject
                {
                    ["steady_wind"] = new JObject { ["enabled"] = false, ["speed_mps"] = 0.0, ["direction_from_deg"] = 0.0, ["vertical_mps"] = 0.0 },
                    ["turbulence"] = new JObject { ["enabled"] = false, ["preset"] = "light", ["severity"] = 3, ["windspeed_at_20ft_mps"] = 7.62, ["random_seed"] = 12345 + index },
                    ["gust"] = new JObject { ["magnitude_mps"] = 0.0, ["direction_from_deg"] = 0.0, ["vertical_mps"] = 0.0, ["ramp_in_s"] = 1.0, ["hold_s"] = 1.0, ["ramp_out_s"] = 1.0 }
                },
                ["fault_injection"] = new JObject { ["enabled"] = true, ["max_channels"] = 8, ["random_seed"] = 24680 + index },
                ["mavlink"] = new JObject
                {
                    // UDP JBS 只替代原始传感器/舵机链路，不能与 MAVLink 发布器同时启用。
                    ["enabled"] = false,
                    ["host"] = "127.0.0.1",
                    ["port"] = 14560 + sysid,
                    ["sensor_rate_hz"] = 100,
                    ["gps_rate_hz"] = 10,
                    ["system_id"] = sysid,
                    ["component_id"] = 200
                },
                ["serial_hil"] = new JObject
                {
                    ["enabled"] = !udp,
                    ["device"] = serial ?? "",
                    ["baud"] = baud,
                    ["sensor_rate_hz"] = 100,
                    ["gps_rate_hz"] = 10
                },
                ["udp_hil"] = new JObject
                {
                    ["enabled"] = udp,
                    ["bind_host"] = "0.0.0.0",
                    ["bind_port"] = udpPort,
                    ["remote_host"] = udpHost,
                    ["remote_port"] = udpPort,
                    ["sensor_rate_hz"] = 100,
                    ["gps_rate_hz"] = 10
                }
            };
        }

        private static bool CellBoolean(DataGridViewRow row, string name)
        {
            return row.Cells[name].Value != null && Convert.ToBoolean(row.Cells[name].Value, CultureInfo.InvariantCulture);
        }

        private static string CellString(DataGridViewRow row, string name)
        {
            var value = row.Cells[name].Value;
            return value == null ? "" : Convert.ToString(value, CultureInfo.InvariantCulture).Trim();
        }

        private static int ParseCellInt(DataGridViewRow row, string name, int fallback)
        {
            int value;
            return Int32.TryParse(CellString(row, name), NumberStyles.Integer, CultureInfo.InvariantCulture,
                out value) ? value : fallback;
        }

        private static string DefaultIfBlank(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static string FindJSBSimDirectory()
        {
            var starts = new[] {AppDomain.CurrentDomain.BaseDirectory, Environment.CurrentDirectory};
            foreach (var start in starts)
            {
                var directory = new DirectoryInfo(start);
                for (var depth = 0; directory != null && depth < 7; depth++, directory = directory.Parent)
                {
                    var candidate = Path.Combine(directory.FullName, "JSBSim");
                    if (File.Exists(Path.Combine(candidate, "hil_fleet_supervisor.py")))
                        return candidate;
                }
            }
            return null;
        }

        private void BrowsePython(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog {Filter = "Python|python.exe|所有文件|*.*"})
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    pythonPath.Text = dialog.FileName;
        }

        private void BrowseConfig(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog {Filter = "YAML 配置|*.yaml;*.yml|所有文件|*.*"})
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    configPath.Text = dialog.FileName;
                    LoadVehicleRowsFromConfiguration(configPath.Text);
                }
        }

        private async void StartAllClicked(object sender, EventArgs e)
        {
            try
            {
                if (ownedProcess != null && !ownedProcess.HasExited)
                {
                    EnsureClient();
                    await client.StartAllAsync(CancellationToken.None);
                    await RefreshStatusAsync();
                    return;
                }
                if (configurationDirty)
                {
                    var generated = SaveGeneratedConfiguration();
                    AppendConsole("[配置] 已应用界面连接配置：" + generated + Environment.NewLine);
                }
                if (!File.Exists(pythonPath.Text))
                    throw new FileNotFoundException("找不到 Python", pythonPath.Text);
                if (!File.Exists(configPath.Text))
                    throw new FileNotFoundException("找不到多机配置", configPath.Text);
                var jsbsimDirectory = Directory.GetParent(Path.GetDirectoryName(configPath.Text)).FullName;
                var script = Path.Combine(jsbsimDirectory, "hil_fleet_supervisor.py");
                if (!File.Exists(script))
                    throw new FileNotFoundException("配置目录旁未找到 hil_fleet_supervisor.py", script);

                ReplaceClient();
                var startInfo = new ProcessStartInfo
                {
                    FileName = pythonPath.Text,
                    Arguments = Quote(script) + " --config " + Quote(Path.GetFullPath(configPath.Text)),
                    WorkingDirectory = jsbsimDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                ownedProcess = new Process {StartInfo = startInfo, EnableRaisingEvents = true};
                ownedProcess.OutputDataReceived += ProcessOutput;
                ownedProcess.ErrorDataReceived += ProcessOutput;
                ownedProcess.Exited += ProcessExited;
                if (!ownedProcess.Start())
                    throw new InvalidOperationException("多机总控进程未启动");
                ownedProcess.BeginOutputReadLine();
                ownedProcess.BeginErrorReadLine();
                startAllButton.Enabled = false;
                stopAllButton.Enabled = true;
                shutdownButton.Enabled = true;
                AppendConsole("[Mission Planner] 已启动多机总控。" + Environment.NewLine);
                await WaitUntilReadyAsync();
            }
            catch (Exception ex)
            {
                AppendConsole("[启动失败] " + ex.Message + Environment.NewLine);
                statusLabel.Text = "启动失败";
                startAllButton.Enabled = true;
                MessageBox.Show(this, ex.Message, "JSBSim HIL 多机启动失败", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ReplaceClient()
        {
            if (client != null)
                client.Dispose();
            client = new FleetSupervisorClient(apiAddress.Text.Trim());
        }

        private void EnsureClient()
        {
            if (client == null)
                ReplaceClient();
        }

        private async Task WaitUntilReadyAsync()
        {
            var deadline = DateTime.UtcNow.AddSeconds(15);
            Exception lastError = null;
            while (DateTime.UtcNow < deadline)
            {
                if (ownedProcess == null || ownedProcess.HasExited)
                    throw new InvalidOperationException("多机总控在 Ready 前退出，请查看下方输出。");
                try
                {
                    await RefreshStatusAsync();
                    statusTimer.Start();
                    return;
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    await Task.Delay(250);
                }
            }
            throw new TimeoutException("15 秒内未连接多机总控 API，请检查 fleet.port。", lastError);
        }

        private async void StopAllClicked(object sender, EventArgs e)
        {
            try
            {
                EnsureClient();
                await client.StopAllAsync(CancellationToken.None);
                await RefreshStatusAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "停止多机失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ShutdownClicked(object sender, EventArgs e)
        {
            await StopOwnedProcessAsync();
        }

        private async void RunVehicleAction(string action)
        {
            var rows = GetSelectedVehicleRows();
            if (rows.Count == 0)
            {
                MessageBox.Show(this, "请在“选择”列勾选至少一架飞机。", "JSBSim HIL 多机", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            try
            {
                EnsureClient();
                foreach (var row in rows)
                {
                    var sysid = ParseCellInt(row, "sysid", row.Index + 1);
                    await client.VehicleActionAsync(sysid, action, CancellationToken.None);
                }
                await RefreshStatusAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "单机操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void PollStatus(object sender, EventArgs e)
        {
            if (pollInProgress)
                return;
            pollInProgress = true;
            try
            {
                EnsureClient();
                await RefreshStatusAsync();
            }
            catch (Exception ex)
            {
                statusLabel.Text = "总控不可达";
                AppendConsole("[状态] " + ex.Message + Environment.NewLine);
            }
            finally
            {
                pollInProgress = false;
            }
        }

        private async Task RefreshStatusAsync()
        {
            var fleet = await client.GetFleetAsync(CancellationToken.None);
            var vehicles = fleet["vehicles"] as JArray;
            if (vehicles != null)
                foreach (var token in vehicles)
                {
                    var item = token as JObject;
                    if (item == null)
                        continue;
                    var sysid = (int?) item["sysid"] ?? 0;
                    var row = vehiclesGrid.Rows.Cast<DataGridViewRow>()
                        .FirstOrDefault(value => ParseCellInt(value, "sysid", -1) == sysid);
                    if (row == null)
                        continue;
                    row.Cells["state"].Value = (string) item["state"] ?? "";
                    row.Cells["pid"].Value = item["pid"] == null || item["pid"].Type == JTokenType.Null
                        ? ""
                        : item["pid"].ToString();
                    row.Cells["error"].Value = (string) item["last_error"] ?? "";
                }
            statusLabel.Text = "总控在线，飞机 " + ((int?)fleet["vehicle_count"] ?? 0) + " 架";
            startAllButton.Enabled = true;
            stopAllButton.Enabled = true;
            shutdownButton.Enabled = ownedProcess != null && !ownedProcess.HasExited;
        }

        private List<DataGridViewRow> GetSelectedVehicleRows()
        {
            vehiclesGrid.EndEdit();
            return vehiclesGrid.Rows.Cast<DataGridViewRow>()
                .Where(row => CellBoolean(row, "enabled") && CellBoolean(row, "selected"))
                .ToList();
        }

        private sealed class MavlinkTarget
        {
            public MAVLinkInterface Port;
            public MAVState State;
        }

        private async Task RunMavlinkActionAsync(string operation, string mode)
        {
            var rows = GetSelectedVehicleRows();
            if (rows.Count == 0)
            {
                MessageBox.Show(this, "请在“选择”列勾选至少一架飞机。", operation,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var selectedSysids = new HashSet<int>(rows.Select(row => ParseCellInt(row, "sysid", -1)));
            armButton.Enabled = autoButton.Enabled = guidedButton.Enabled = false;
            try
            {
                var messages = await Task.Run(() => ExecuteMavlinkAction(selectedSysids, operation, mode));
                AppendConsole("[飞控命令] " + operation + "：" + string.Join("；", messages.ToArray()) +
                              Environment.NewLine);
                var failures = messages.Where(message => message.Contains("失败") || message.Contains("未连接"))
                    .ToArray();
                if (failures.Length > 0)
                    MessageBox.Show(this, string.Join(Environment.NewLine, failures), operation + "未全部完成",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                armButton.Enabled = autoButton.Enabled = guidedButton.Enabled = true;
            }
        }

        private static List<string> ExecuteMavlinkAction(HashSet<int> selectedSysids, string operation,
            string mode)
        {
            var available = new Dictionary<int, MavlinkTarget>();
            foreach (var port in MainV2.Comports.ToArray())
            {
                if (port == null || port.BaseStream == null || !port.BaseStream.IsOpen)
                    continue;
                foreach (var mav in port.MAVlist)
                {
                    var id = (int) mav.sysid;
                    if (!selectedSysids.Contains(id))
                        continue;
                    MavlinkTarget previous;
                    if (!available.TryGetValue(id, out previous) || (previous.State.compid != 1 && mav.compid == 1))
                        available[id] = new MavlinkTarget {Port = port, State = mav};
                }
            }

            var messages = new List<string>();
            foreach (var sysid in selectedSysids.OrderBy(value => value))
            {
                MavlinkTarget target;
                if (!available.TryGetValue(sysid, out target))
                {
                    messages.Add("SYSID " + sysid + " 未连接");
                    continue;
                }
                try
                {
                    if (mode == null)
                    {
                        if (target.State.cs.armed)
                        {
                            messages.Add("SYSID " + sysid + " 已经解锁");
                            continue;
                        }

                        // In a multi-link setup the normal synchronous doARM path can race the
                        // serial reader for COMMAND_ACK.  The aircraft may arm successfully while
                        // Mission Planner reports a rejected or timed-out ACK.  Send without
                        // consuming the ACK here, then confirm the result from HEARTBEAT state.
                        target.Port.doCommand(target.State.sysid, target.State.compid,
                            MAVLink.MAV_CMD.COMPONENT_ARM_DISARM, 1, 21196, 0, 0, 0, 0, 0, false);
                        var armed = WaitForMavState(() => target.State.cs.armed, 3000);
                        if (!armed)
                        {
                            target.Port.doCommand(target.State.sysid, target.State.compid,
                                MAVLink.MAV_CMD.COMPONENT_ARM_DISARM, 1, 21196, 0, 0, 0, 0, 0, false);
                            armed = WaitForMavState(() => target.State.cs.armed, 3000);
                        }
                        messages.Add("SYSID " + sysid + (armed
                            ? " 解锁成功（状态已确认）"
                            : " 解锁指令已发送，暂未收到状态确认，请查看预检消息"));
                    }
                    else
                    {
                        if (string.Equals(target.State.cs.mode, mode, StringComparison.OrdinalIgnoreCase))
                        {
                            messages.Add("SYSID " + sysid + " 已经处于 " + mode);
                            continue;
                        }
                        target.Port.setMode(target.State.sysid, target.State.compid, mode);
                        var changed = WaitForMavState(
                            () => string.Equals(target.State.cs.mode, mode, StringComparison.OrdinalIgnoreCase), 3000);
                        messages.Add("SYSID " + sysid + (changed
                            ? " 已切换 " + mode + "（状态已确认）"
                            : " 已发送 " + mode + "，暂未收到状态确认"));
                    }
                }
                catch (Exception ex)
                {
                    messages.Add("SYSID " + sysid + " " + operation + "失败：" + ex.Message);
                }
            }
            return messages;
        }

        private static bool WaitForMavState(Func<bool> condition, int timeoutMilliseconds)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            while (DateTime.UtcNow < deadline)
            {
                if (condition())
                    return true;
                Thread.Sleep(100);
            }
            return condition();
        }

        private List<HilFaultTarget> GetSelectedHilTargets(string operation)
        {
            var rows = GetSelectedVehicleRows();
            if (rows.Count == 0)
            {
                MessageBox.Show(this, "请先勾选目标飞机。", operation,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            return rows.Select(row => new HilFaultTarget(
                DefaultIfBlank(CellString(row, "name"), "SYSID " + CellString(row, "sysid")),
                ParseCellInt(row, "sysid", row.Index + 1),
                "http://127.0.0.1:" + ParseCellInt(row, "apiPort", 8765 + row.Index)))
                .ToList();
        }

        private void OpenFleetEnvironment(object sender, EventArgs e)
        {
            var targets = GetSelectedHilTargets("多机环境干扰");
            if (targets == null)
                return;
            var form = new JSBSimFleetEnvironmentForm(targets);
            form.Show(this);
        }

        private void OpenFleetFaultInjection(object sender, EventArgs e)
        {
            var targets = GetSelectedHilTargets("多机故障注入");
            if (targets == null)
                return;
            var form = new JSBSimFaultInjectionForm(targets);
            form.Show(this);
        }

        private async Task StopOwnedProcessAsync()
        {
            statusTimer.Stop();
            var process = ownedProcess;
            if (process == null || process.HasExited)
            {
                SetStoppedState();
                return;
            }
            try
            {
                EnsureClient();
                await client.ShutdownAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                AppendConsole("[关闭] " + ex.Message + Environment.NewLine);
            }
            var exited = await Task.Run(() => process.WaitForExit(10000));
            if (!exited)
            {
                process.Kill();
                await Task.Run(() => process.WaitForExit(3000));
            }
            SetStoppedState();
        }

        private void SetStoppedState()
        {
            statusLabel.Text = "已停止";
            startAllButton.Enabled = true;
            stopAllButton.Enabled = false;
            shutdownButton.Enabled = false;
        }

        private void ProcessOutput(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data) || e.Data.StartsWith("{", StringComparison.Ordinal))
                return;
            AppendConsole(e.Data + Environment.NewLine);
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing)
                return;
            BeginInvoke((Action)(() =>
            {
                statusTimer.Stop();
                SetStoppedState();
                AppendConsole("[Mission Planner] 多机总控已退出。" + Environment.NewLine);
            }));
        }

        private void AppendConsole(string text)
        {
            if (IsDisposed || Disposing)
                return;
            if (InvokeRequired)
            {
                BeginInvoke((Action<string>)AppendConsole, text);
                return;
            }
            if (console.TextLength > 100000)
                console.Text = console.Text.Substring(console.TextLength - 75000);
            console.AppendText(text);
        }

        private async void FleetFormClosing(object sender, FormClosingEventArgs e)
        {
            if (closingAfterStop || ownedProcess == null || ownedProcess.HasExited)
            {
                if (client != null)
                    client.Dispose();
                return;
            }
            var answer = MessageBox.Show(this, "多机总控仍在运行，是否停止全部 Worker 后关闭？",
                "JSBSim HIL 多机", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            e.Cancel = true;
            await StopOwnedProcessAsync();
            closingAfterStop = true;
            Close();
        }

        private static string Quote(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }
    }
}
