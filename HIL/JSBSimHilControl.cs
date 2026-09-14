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
    /// <summary>
    /// 用于访问本机 hil_supervisor 控制接口的可复用 JSON 客户端。
    /// 该客户端有意与 Mission Planner 的 MAVLink 串口保持独立。
    /// </summary>
    internal sealed class HilSupervisorClient : IDisposable
    {
        private readonly Uri baseUri;

        public HilSupervisorClient(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("API 地址不能为空", "endpoint");
            if (!endpoint.EndsWith("/", StringComparison.Ordinal))
                endpoint += "/";
            baseUri = new Uri(endpoint, UriKind.Absolute);
            if (baseUri.Host != "127.0.0.1" && baseUri.Host != "localhost")
                throw new ArgumentException("控制 API 只允许使用 127.0.0.1 或 localhost");
        }

        public Task<JObject> GetHealthAsync(CancellationToken cancellationToken)
        {
            return SendAsync("GET", "api/v1/health", null, cancellationToken);
        }

        public Task<JObject> ApplyEnvironmentAsync(int sysid, JObject body, CancellationToken cancellationToken)
        {
            return SendAsync("PUT", "api/v1/vehicles/" + sysid + "/environment", body, cancellationToken);
        }

        public Task<JObject> TriggerGustAsync(int sysid, JObject body, CancellationToken cancellationToken)
        {
            return SendAsync("POST", "api/v1/vehicles/" + sysid + "/environment/gust", body,
                cancellationToken);
        }

        public Task<JObject> ResetEnvironmentAsync(int sysid, CancellationToken cancellationToken)
        {
            return SendAsync("POST", "api/v1/vehicles/" + sysid + "/environment/reset", new JObject(),
                cancellationToken);
        }

        public Task<JObject> GetFaultsAsync(int sysid, CancellationToken cancellationToken)
        {
            return SendAsync("GET", "api/v1/vehicles/" + sysid + "/faults", null, cancellationToken);
        }

        public Task<JObject> ConfigureFaultAsync(int sysid, int slot, JObject body,
            CancellationToken cancellationToken)
        {
            return SendAsync("PUT", "api/v1/vehicles/" + sysid + "/faults/" + slot, body,
                cancellationToken);
        }

        public Task<JObject> ClearFaultAsync(int sysid, int slot, CancellationToken cancellationToken)
        {
            return SendAsync("POST", "api/v1/vehicles/" + sysid + "/faults/" + slot + "/clear",
                new JObject(), cancellationToken);
        }

        public Task<JObject> ResetFaultsAsync(int sysid, CancellationToken cancellationToken)
        {
            return SendAsync("POST", "api/v1/vehicles/" + sysid + "/faults/reset", new JObject(),
                cancellationToken);
        }

        public Task<JObject> ShutdownAsync(CancellationToken cancellationToken)
        {
            return SendAsync("POST", "api/v1/shutdown", new JObject(), cancellationToken);
        }

        private async Task<JObject> SendAsync(string method, string relativePath, JObject body,
            CancellationToken cancellationToken)
        {
            var request = (HttpWebRequest) WebRequest.Create(new Uri(baseUri, relativePath));
            request.Method = method;
            request.Accept = "application/json";
            request.ContentType = "application/json; charset=utf-8";
            request.Timeout = 2000;
            request.ReadWriteTimeout = 2000;

            using (cancellationToken.Register(request.Abort))
            {
                try
                {
                    if (body != null)
                    {
                        var bytes = Encoding.UTF8.GetBytes(body.ToString(Formatting.None));
                        request.ContentLength = bytes.Length;
                        using (var stream = await request.GetRequestStreamAsync().ConfigureAwait(false))
                            await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
                    }

                    using (var response = (HttpWebResponse) await request.GetResponseAsync().ConfigureAwait(false))
                    using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        var text = await reader.ReadToEndAsync().ConfigureAwait(false);
                        return JObject.Parse(text);
                    }
                }
                catch (WebException ex)
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException(cancellationToken);
                    var response = ex.Response as HttpWebResponse;
                    if (response != null)
                    {
                        using (response)
                        using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                        {
                            var text = await reader.ReadToEndAsync().ConfigureAwait(false);
                            JObject error;
                            if (TryParseObject(text, out error))
                                throw new InvalidOperationException((string) error["error"] ?? text, ex);
                        }
                    }
                    throw new InvalidOperationException("无法连接 hil_supervisor：" + ex.Message, ex);
                }
            }
        }

        private static bool TryParseObject(string text, out JObject value)
        {
            try
            {
                value = JObject.Parse(text);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }

        public void Dispose()
        {
            // HttpWebRequest 不持有需要由本类释放的客户端对象。
        }
    }

    /// <summary>
    /// 启动 Python 硬件在环监控程序，并控制 JSBSim 的环境输入。
    /// 原有 XPlane 硬件在环功能仍独立保留在 StartHil 中。
    /// </summary>
    public sealed class JSBSimHilControlForm : Form
    {
        private const string SerialTransport = "串口 JBS1/JBO1";
        private const string UdpTransport = "UDP JBS1/JBO1";

        private readonly TextBox pythonPath = new TextBox();
        private readonly TextBox configPath = new TextBox();
        private readonly TextBox apiAddress = new TextBox();
        private readonly NumericUpDown sysid = Number(1, 255, 1, 0, 1);
        private readonly ComboBox transportSelection = new ComboBox();
        private readonly TextBox serialDevice = new TextBox();
        private readonly ComboBox serialBaud = new ComboBox();
        private readonly TextBox udpBindHost = new TextBox();
        private readonly NumericUpDown udpBindPort = Number(1, 65535, 14601, 0, 1);
        private readonly TextBox udpRemoteHost = new TextBox();
        private readonly NumericUpDown udpRemotePort = Number(1, 65535, 14601, 0, 1);
        private readonly Button startButton = new Button();
        private readonly Button stopButton = new Button();
        private readonly Button connectButton = new Button();
        private readonly Label statusLabel = new Label();
        private readonly TextBox console = new TextBox();

        private readonly CheckBox steadyEnabled = new CheckBox();
        private readonly NumericUpDown windSpeed = Number(0, 100, 0, 1, 0.5M);
        private readonly NumericUpDown windDirection = Number(0, 359.9M, 0, 1, 1);
        private readonly NumericUpDown verticalWind = Number(-50, 50, 0, 1, 0.5M);

        private readonly CheckBox turbulenceEnabled = new CheckBox();
        private readonly ComboBox turbulencePreset = new ComboBox();
        private readonly NumericUpDown turbulenceSeverity = Number(0, 7, 3, 0, 1);
        private readonly NumericUpDown turbulenceSpeed = Number(0, 100, 7.62M, 2, 0.5M);
        private readonly NumericUpDown turbulenceSeed = Number(0, 2147483647, 12345, 0, 1);

        private readonly NumericUpDown gustMagnitude = Number(0, 100, 5, 1, 0.5M);
        private readonly NumericUpDown gustDirection = Number(0, 359.9M, 0, 1, 1);
        private readonly NumericUpDown gustVertical = Number(-50, 50, 0, 1, 0.5M);
        private readonly NumericUpDown gustRampIn = Number(0, 300, 1, 1, 0.5M);
        private readonly NumericUpDown gustHold = Number(0, 300, 1, 1, 0.5M);
        private readonly NumericUpDown gustRampOut = Number(0, 300, 1, 1, 0.5M);

        private readonly System.Windows.Forms.Timer statusTimer = new System.Windows.Forms.Timer();
        private HilSupervisorClient client;
        private Process ownedProcess;
        private bool pollInProgress;
        private bool closingAfterStop;

        public JSBSimHilControlForm()
        {
            Text = "JSBSim HIL 环境控制";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(1040, 760);
            Size = new Size(1180, 900);
            BuildInterface();
            SetDefaultPaths();

            statusTimer.Interval = 200;
            statusTimer.Tick += StatusTimerTick;
            FormClosing += FormIsClosing;
        }

        private void BuildInterface()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                ColumnCount = 1,
                RowCount = 5
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 125));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 105));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 285));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            root.Controls.Add(BuildProcessGroup(), 0, 0);
            root.Controls.Add(BuildTransportGroup(), 0, 1);
            root.Controls.Add(BuildStatusGroup(), 0, 2);
            root.Controls.Add(BuildEnvironmentGroup(), 0, 3);

            var consoleGroup = new GroupBox {Text = "hil_supervisor 输出", Dock = DockStyle.Fill};
            console.Multiline = true;
            console.ReadOnly = true;
            console.ScrollBars = ScrollBars.Both;
            console.WordWrap = false;
            console.Font = new Font(FontFamily.GenericMonospace, 9F);
            console.Dock = DockStyle.Fill;
            consoleGroup.Controls.Add(console);
            root.Controls.Add(consoleGroup, 0, 4);
        }

        private Control BuildProcessGroup()
        {
            var group = new GroupBox {Text = "仿真进程", Dock = DockStyle.Fill};
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(6),
                ColumnCount = 5,
                RowCount = 3
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 85));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 75));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 85));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 85));

            AddPathRow(layout, 0, "Python", pythonPath, BrowsePython);
            AddPathRow(layout, 1, "配置 YAML", configPath, BrowseConfig);

            layout.Controls.Add(new Label {Text = "控制 API", AutoSize = true, Anchor = AnchorStyles.Left}, 0, 2);
            apiAddress.Text = "http://127.0.0.1:8765/";
            apiAddress.Dock = DockStyle.Fill;
            layout.Controls.Add(apiAddress, 1, 2);
            layout.Controls.Add(new Label {Text = "SysID", AutoSize = true, Anchor = AnchorStyles.Right}, 2, 2);
            sysid.Dock = DockStyle.Fill;
            layout.Controls.Add(sysid, 3, 2);

            startButton.Text = "启动";
            startButton.Click += StartClicked;
            stopButton.Text = "停止";
            stopButton.Enabled = false;
            stopButton.Click += StopClicked;
            connectButton.Text = "连接测试";
            connectButton.Click += ConnectClicked;
            var buttons = new FlowLayoutPanel {Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight};
            buttons.Controls.Add(startButton);
            buttons.Controls.Add(stopButton);
            buttons.Controls.Add(connectButton);
            layout.Controls.Add(buttons, 4, 0);
            layout.SetRowSpan(buttons, 3);

            group.Controls.Add(layout);
            return group;
        }

        private static void AddPathRow(TableLayoutPanel layout, int row, string labelText, TextBox value,
            EventHandler browseHandler)
        {
            layout.Controls.Add(new Label {Text = labelText, AutoSize = true, Anchor = AnchorStyles.Left}, 0, row);
            value.Dock = DockStyle.Fill;
            layout.Controls.Add(value, 1, row);
            layout.SetColumnSpan(value, 2);
            var browse = new Button {Text = "浏览...", Dock = DockStyle.Fill};
            browse.Click += browseHandler;
            layout.Controls.Add(browse, 3, row);
        }

        private Control BuildStatusGroup()
        {
            var group = new GroupBox {Text = "实时状态（5 Hz）", Dock = DockStyle.Fill};
            statusLabel.Text = "未连接";
            statusLabel.Dock = DockStyle.Fill;
            statusLabel.AutoEllipsis = true;
            statusLabel.Padding = new Padding(8);
            group.Controls.Add(statusLabel);
            return group;
        }

        private Control BuildEnvironmentGroup()
        {
            var group = new GroupBox {Text = "JSBSim 环境输入", Dock = DockStyle.Fill};
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight
            };
            flow.Controls.Add(BuildSteadyWindGroup());
            flow.Controls.Add(BuildTurbulenceGroup());
            flow.Controls.Add(BuildGustGroup());
            group.Controls.Add(flow);
            return group;
        }

        private Control BuildSteadyWindGroup()
        {
            var group = EnvironmentBox("稳态风", 285);
            var layout = EnvironmentLayout();
            steadyEnabled.Text = "启用稳态风";
            AddWide(layout, steadyEnabled, 0);
            AddValue(layout, "水平风速 (m/s)", windSpeed, 1);
            AddValue(layout, "来向 (°)", windDirection, 2);
            AddValue(layout, "垂直风，向上为正", verticalWind, 3);
            var apply = new Button {Text = "应用稳态风和湍流", Dock = DockStyle.Fill};
            apply.Click += ApplyEnvironmentClicked;
            AddWide(layout, apply, 4);
            var reset = new Button {Text = "恢复无风", Dock = DockStyle.Fill};
            reset.Click += ResetEnvironmentClicked;
            AddWide(layout, reset, 5);
            group.Controls.Add(layout);
            return group;
        }

        private Control BuildTurbulenceGroup()
        {
            var group = EnvironmentBox("MILSPEC 湍流", 300);
            var layout = EnvironmentLayout();
            turbulenceEnabled.Text = "启用湍流";
            AddWide(layout, turbulenceEnabled, 0);
            turbulencePreset.DropDownStyle = ComboBoxStyle.DropDownList;
            turbulencePreset.Items.AddRange(new object[] {"light", "moderate", "severe", "custom"});
            turbulencePreset.SelectedIndex = 0;
            turbulencePreset.SelectedIndexChanged += TurbulencePresetChanged;
            AddValue(layout, "预设", turbulencePreset, 1);
            AddValue(layout, "严重等级 (0-7)", turbulenceSeverity, 2);
            AddValue(layout, "20ft 风速 (m/s)", turbulenceSpeed, 3);
            AddValue(layout, "随机种子", turbulenceSeed, 4);
            group.Controls.Add(layout);
            return group;
        }

        private Control BuildGustGroup()
        {
            var group = EnvironmentBox("单次平滑阵风", 340);
            var layout = EnvironmentLayout();
            AddValue(layout, "水平强度 (m/s)", gustMagnitude, 0);
            AddValue(layout, "来向 (°)", gustDirection, 1);
            AddValue(layout, "垂直分量，向上为正", gustVertical, 2);
            AddValue(layout, "渐入/保持/渐出 (s)", DurationPanel(), 3);
            var trigger = new Button {Text = "触发阵风", Dock = DockStyle.Fill};
            trigger.Click += TriggerGustClicked;
            AddWide(layout, trigger, 4);
            group.Controls.Add(layout);
            return group;
        }

        private Control DurationPanel()
        {
            var flow = new FlowLayoutPanel {Dock = DockStyle.Fill, WrapContents = false, Margin = Padding.Empty};
            gustRampIn.Width = 58;
            gustHold.Width = 58;
            gustRampOut.Width = 58;
            flow.Controls.Add(gustRampIn);
            flow.Controls.Add(gustHold);
            flow.Controls.Add(gustRampOut);
            return flow;
        }

        private static GroupBox EnvironmentBox(string text, int width)
        {
            return new GroupBox {Text = text, Width = width, Height = 245, Margin = new Padding(5)};
        }

        private static TableLayoutPanel EnvironmentLayout()
        {
            var layout = new TableLayoutPanel {Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6, Padding = new Padding(5)};
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            return layout;
        }

        private static void AddValue(TableLayoutPanel layout, string text, Control value, int row)
        {
            layout.Controls.Add(new Label {Text = text, AutoSize = true, Anchor = AnchorStyles.Left}, 0, row);
            value.Dock = DockStyle.Fill;
            layout.Controls.Add(value, 1, row);
        }

        private static void AddWide(TableLayoutPanel layout, Control value, int row)
        {
            value.Dock = DockStyle.Fill;
            layout.Controls.Add(value, 0, row);
            layout.SetColumnSpan(value, 2);
        }

        private static NumericUpDown Number(decimal minimum, decimal maximum, decimal value, int decimals,
            decimal increment)
        {
            return new NumericUpDown
            {
                Minimum = minimum,
                Maximum = maximum,
                Value = value,
                DecimalPlaces = decimals,
                Increment = increment,
                ThousandsSeparator = maximum >= 100000
            };
        }

        private void SetDefaultPaths()
        {
            var jsbsimDirectory = FindJSBSimDirectory();
            if (jsbsimDirectory == null)
                return;
            var python = Path.Combine(jsbsimDirectory, ".venv", "Scripts", "python.exe");
            var config = Path.Combine(jsbsimDirectory, "config", "vehicle01_serial_hil.yaml");
            if (File.Exists(python))
                pythonPath.Text = python;
            if (File.Exists(config))
                configPath.Text = config;
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
                    if (File.Exists(Path.Combine(candidate, "hil_supervisor.py")))
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
                    configPath.Text = dialog.FileName;
        }

        private async void StartClicked(object sender, EventArgs e)
        {
            if (!File.Exists(pythonPath.Text))
            {
                MessageBox.Show(this, "找不到 Python：" + pythonPath.Text, "JSBSim HIL", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            if (!File.Exists(configPath.Text))
            {
                MessageBox.Show(this, "找不到配置文件：" + configPath.Text, "JSBSim HIL", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            if (ownedProcess != null && !ownedProcess.HasExited)
                return;

            try
            {
                ReplaceClient();
                var script = Path.Combine(Path.GetDirectoryName(configPath.Text), "..", "hil_supervisor.py");
                script = Path.GetFullPath(script);
                if (!File.Exists(script))
                    throw new FileNotFoundException("配置目录旁未找到 hil_supervisor.py", script);

                var startInfo = new ProcessStartInfo
                {
                    FileName = pythonPath.Text,
                    Arguments = BuildStartArguments(script),
                    WorkingDirectory = Path.GetDirectoryName(script),
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
                    throw new InvalidOperationException("Python 进程未启动");
                ownedProcess.BeginOutputReadLine();
                ownedProcess.BeginErrorReadLine();
                startButton.Enabled = false;
                stopButton.Enabled = true;
                AppendConsole("[Mission Planner] 已启动 hil_supervisor。" + Environment.NewLine);
                await WaitUntilReadyAsync();
            }
            catch (Exception ex)
            {
                AppendConsole("[启动失败] " + ex.Message + Environment.NewLine);
                startButton.Enabled = true;
                stopButton.Enabled = false;
                MessageBox.Show(this, ex.Message, "JSBSim HIL 启动失败", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static string Quote(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        private string BuildStartArguments(string script)
        {
            // 使用命令行参数覆盖链路设置，使操作员可以切换串口或 UDP，
            // 同时保留共享 YAML 中的飞机模型、初始状态和环境参数。
            var arguments = new StringBuilder();
            arguments.Append(Quote(script));
            arguments.Append(" --config ").Append(Quote(Path.GetFullPath(configPath.Text)));
            var udp = String.Equals(transportSelection.SelectedItem as string, UdpTransport,
                StringComparison.Ordinal);
            if (!udp)
            {
                var device = String.IsNullOrWhiteSpace(serialDevice.Text) ? "COM31" : serialDevice.Text.Trim();
                int baud;
                if (!Int32.TryParse(serialBaud.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out baud) ||
                    baud <= 0)
                    throw new InvalidOperationException("串口波特率必须是大于 0 的整数。");
                serialDevice.Text = device;
                arguments.Append(" --transport serial-jbs --serial-device ").Append(Quote(device));
                arguments.Append(" --serial-baud ").Append(baud.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                // bind 地址是本机接收 JBO1 的端点，remote 地址是接收 JBS1 的机载 LQ 网桥。
                var bindHost = String.IsNullOrWhiteSpace(udpBindHost.Text) ? "0.0.0.0" : udpBindHost.Text.Trim();
                var remoteHost = String.IsNullOrWhiteSpace(udpRemoteHost.Text)
                    ? "192.168.1.201"
                    : udpRemoteHost.Text.Trim();
                udpBindHost.Text = bindHost;
                udpRemoteHost.Text = remoteHost;
                arguments.Append(" --transport udp-jbs --udp-bind-host ").Append(Quote(bindHost));
                arguments.Append(" --udp-bind-port ")
                    .Append(Decimal.ToInt32(udpBindPort.Value).ToString(CultureInfo.InvariantCulture));
                arguments.Append(" --udp-remote-host ").Append(Quote(remoteHost));
                arguments.Append(" --udp-remote-port ")
                    .Append(Decimal.ToInt32(udpRemotePort.Value).ToString(CultureInfo.InvariantCulture));
            }
            return arguments.ToString();
        }

        private async Task WaitUntilReadyAsync()
        {
            var deadline = DateTime.UtcNow.AddSeconds(10);
            Exception lastError = null;
            while (DateTime.UtcNow < deadline)
            {
                if (ownedProcess == null || ownedProcess.HasExited)
                    throw new InvalidOperationException("hil_supervisor 在 Ready 前退出，请查看下方输出。");
                try
                {
                    var health = await client.GetHealthAsync(CancellationToken.None);
                    UpdateHealth(health);
                    LoadControlsFromHealth(health);
                    statusTimer.Start();
                    return;
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    await Task.Delay(250);
                }
            }
            throw new TimeoutException("10 秒内未连接控制 API。请检查 YAML 中 control_api 配置。", lastError);
        }

        private async void StopClicked(object sender, EventArgs e)
        {
            await StopOwnedProcessAsync();
        }

        private async Task StopOwnedProcessAsync()
        {
            var process = ownedProcess;
            if (process == null || process.HasExited)
                return;
            stopButton.Enabled = false;
            statusTimer.Stop();
            try
            {
                if (client != null)
                    await client.ShutdownAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                AppendConsole("[停止] 正常关闭请求失败：" + ex.Message + Environment.NewLine);
            }

            var exited = await Task.Run(() => process.WaitForExit(3000));
            if (!exited)
            {
                AppendConsole("[停止] 3 秒未退出，终止本窗口启动的 Python 进程。" + Environment.NewLine);
                process.Kill();
                await Task.Run(() => process.WaitForExit(1000));
            }
            startButton.Enabled = true;
            statusLabel.Text = "已停止";
        }

        private async void ConnectClicked(object sender, EventArgs e)
        {
            try
            {
                ReplaceClient();
                var health = await client.GetHealthAsync(CancellationToken.None);
                UpdateHealth(health);
                LoadControlsFromHealth(health);
                statusTimer.Start();
                AppendConsole("[连接] 控制 API 正常。" + Environment.NewLine);
            }
            catch (Exception ex)
            {
                statusLabel.Text = "连接失败：" + ex.Message;
                AppendConsole("[连接失败] " + ex.Message + Environment.NewLine);
            }
        }

        private void ReplaceClient()
        {
            if (client != null)
                client.Dispose();
            client = new HilSupervisorClient(apiAddress.Text.Trim());
        }

        private async void ApplyEnvironmentClicked(object sender, EventArgs e)
        {
            var turbulence = new JObject
            {
                ["enabled"] = turbulenceEnabled.Checked,
                ["preset"] = turbulencePreset.SelectedItem.ToString(),
                ["severity"] = Decimal.ToInt32(turbulenceSeverity.Value),
                ["windspeed_at_20ft_mps"] = (double) turbulenceSpeed.Value,
                ["random_seed"] = Decimal.ToInt32(turbulenceSeed.Value)
            };
            var body = new JObject
            {
                ["steady_wind"] = new JObject
                {
                    ["enabled"] = steadyEnabled.Checked,
                    ["speed_mps"] = (double) windSpeed.Value,
                    ["direction_from_deg"] = (double) windDirection.Value,
                    ["vertical_mps"] = (double) verticalWind.Value
                },
                ["turbulence"] = turbulence
            };
            await ExecuteCommandAsync("应用环境", c => c.ApplyEnvironmentAsync((int) sysid.Value, body,
                CancellationToken.None));
        }

        private async void TriggerGustClicked(object sender, EventArgs e)
        {
            var body = new JObject
            {
                ["magnitude_mps"] = (double) gustMagnitude.Value,
                ["direction_from_deg"] = (double) gustDirection.Value,
                ["vertical_mps"] = (double) gustVertical.Value,
                ["ramp_in_s"] = (double) gustRampIn.Value,
                ["hold_s"] = (double) gustHold.Value,
                ["ramp_out_s"] = (double) gustRampOut.Value
            };
            await ExecuteCommandAsync("触发阵风", c => c.TriggerGustAsync((int) sysid.Value, body,
                CancellationToken.None));
        }

        private async void ResetEnvironmentClicked(object sender, EventArgs e)
        {
            await ExecuteCommandAsync("恢复无风", c => c.ResetEnvironmentAsync((int) sysid.Value,
                CancellationToken.None));
            steadyEnabled.Checked = false;
            turbulenceEnabled.Checked = false;
        }

        private async Task ExecuteCommandAsync(string name, Func<HilSupervisorClient, Task<JObject>> action)
        {
            try
            {
                if (client == null)
                    ReplaceClient();
                var result = await action(client);
                AppendConsole("[" + name + "] " + result.ToString(Formatting.None) + Environment.NewLine);
            }
            catch (Exception ex)
            {
                AppendConsole("[" + name + "失败] " + ex.Message + Environment.NewLine);
                MessageBox.Show(this, ex.Message, name + "失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void TurbulencePresetChanged(object sender, EventArgs e)
        {
            switch (turbulencePreset.SelectedItem as string)
            {
                case "light":
                    turbulenceSeverity.Value = 3;
                    turbulenceSpeed.Value = 7.62M;
                    break;
                case "moderate":
                    turbulenceSeverity.Value = 4;
                    turbulenceSpeed.Value = 15.24M;
                    break;
                case "severe":
                    turbulenceSeverity.Value = 6;
                    turbulenceSpeed.Value = 22.86M;
                    break;
            }
        }

        private async void StatusTimerTick(object sender, EventArgs e)
        {
            if (pollInProgress || client == null)
                return;
            pollInProgress = true;
            try
            {
                UpdateHealth(await client.GetHealthAsync(CancellationToken.None));
            }
            catch (Exception ex)
            {
                statusLabel.Text = "状态中断：" + ex.Message;
            }
            finally
            {
                pollInProgress = false;
            }
        }

        private void UpdateHealth(JObject health)
        {
            var status = health["status"] as JObject;
            var environment = health["environment"] as JObject;
            var actual = environment == null ? null : environment["actual_total_wind_mps"] as JObject;
            var gust = environment == null ? null : environment["gust"] as JObject;
            statusLabel.Text = string.Format(CultureInfo.InvariantCulture,
                "运行={0}  串口={1}  传感器={2} Hz  舵机={3} Hz  舵机链路={4}\r\n" +
                "总风 N/E/D={5}/{6}/{7} m/s  阵风={8}  仿真时间={9} s  实时倍率={10}",
                Token(health, "running"), Token(status, "serial_open"), Token(status, "sensor_tx_hz"),
                Token(status, "servo_rx_hz"), Token(status, "servo_link_ok"), Token(actual, "north"),
                Token(actual, "east"), Token(actual, "down"), Token(gust, "phase"),
                Token(status, "sim_time_s"), Token(status, "realtime_factor"));
        }

        private void LoadControlsFromHealth(JObject health)
        {
            var settings = health.SelectToken("environment.settings") as JObject;
            if (settings == null)
                return;
            var steady = settings["steady_wind"] as JObject;
            var turbulence = settings["turbulence"] as JObject;
            var gust = settings["gust"] as JObject;
            if (steady != null)
            {
                steadyEnabled.Checked = (bool?) steady["enabled"] ?? false;
                SetNumeric(windSpeed, steady["speed_mps"]);
                SetNumeric(windDirection, steady["direction_from_deg"]);
                SetNumeric(verticalWind, steady["vertical_mps"]);
            }
            if (turbulence != null)
            {
                turbulenceEnabled.Checked = (bool?) turbulence["enabled"] ?? false;
                var preset = (string) turbulence["preset"];
                if (turbulencePreset.Items.Contains(preset))
                    turbulencePreset.SelectedItem = preset;
                SetNumeric(turbulenceSeverity, turbulence["severity"]);
                SetNumeric(turbulenceSpeed, turbulence["windspeed_at_20ft_mps"]);
                SetNumeric(turbulenceSeed, turbulence["random_seed"]);
            }
            if (gust != null)
            {
                SetNumeric(gustMagnitude, gust["magnitude_mps"]);
                SetNumeric(gustDirection, gust["direction_from_deg"]);
                SetNumeric(gustVertical, gust["vertical_mps"]);
                SetNumeric(gustRampIn, gust["ramp_in_s"]);
                SetNumeric(gustHold, gust["hold_s"]);
                SetNumeric(gustRampOut, gust["ramp_out_s"]);
            }
        }

        private static void SetNumeric(NumericUpDown control, JToken token)
        {
            if (token == null)
                return;
            decimal value;
            if (!Decimal.TryParse(token.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return;
            control.Value = Math.Min(control.Maximum, Math.Max(control.Minimum, value));
        }

        private static string Token(JObject source, string name)
        {
            if (source == null || source[name] == null)
                return "--";
            return source[name].ToString(Formatting.None);
        }

        private void ProcessOutput(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                AppendConsole(e.Data + Environment.NewLine);
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            if (IsDisposed || !IsHandleCreated)
                return;
            BeginInvoke((Action) (() =>
            {
                statusTimer.Stop();
                startButton.Enabled = true;
                stopButton.Enabled = false;
                statusLabel.Text = "hil_supervisor 已退出";
            }));
        }

        private void AppendConsole(string text)
        {
            if (IsDisposed)
                return;
            if (InvokeRequired)
            {
                BeginInvoke((Action<string>) AppendConsole, text);
                return;
            }
            console.AppendText(text);
            if (console.TextLength > 150000)
            {
                console.Select(0, 30000);
                console.SelectedText = string.Empty;
            }
        }

        private async void FormIsClosing(object sender, FormClosingEventArgs e)
        {
            if (closingAfterStop || ownedProcess == null || ownedProcess.HasExited)
                return;
            var answer = MessageBox.Show(this, "hil_supervisor 仍在运行，是否停止后关闭？", "JSBSim HIL",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (answer == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            if (answer == DialogResult.No)
            {
                ownedProcess.OutputDataReceived -= ProcessOutput;
                ownedProcess.ErrorDataReceived -= ProcessOutput;
                ownedProcess.Exited -= ProcessExited;
                ownedProcess = null;
                return;
            }
            e.Cancel = true;
            await StopOwnedProcessAsync();
            closingAfterStop = true;
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                statusTimer.Dispose();
                if (client != null)
                    client.Dispose();
                if (ownedProcess != null && ownedProcess.HasExited)
                    ownedProcess.Dispose();
            }
            base.Dispose(disposing);
        }

        private Control BuildTransportGroup()
        {
            var group = new GroupBox {Text = "JBS1/JBO1 硬件在环链路", Dock = DockStyle.Fill};
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(6),
                ColumnCount = 8,
                RowCount = 2
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 85));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));

            transportSelection.DropDownStyle = ComboBoxStyle.DropDownList;
            transportSelection.Items.AddRange(new object[] {SerialTransport, UdpTransport});
            transportSelection.SelectedIndexChanged += delegate { UpdateTransportControls(); };
            AddTransportValue(layout, "链路类型", transportSelection, 0, 0);

            serialDevice.Text = "COM31";
            AddTransportValue(layout, "串口", serialDevice, 2, 0);
            serialBaud.DropDownStyle = ComboBoxStyle.DropDown;
            serialBaud.Items.AddRange(new object[] {"57600", "115200", "230400", "460800", "921600"});
            serialBaud.Text = "115200";
            AddTransportValue(layout, "波特率", serialBaud, 4, 0);

            udpBindHost.Text = "0.0.0.0";
            AddTransportValue(layout, "本地IP", udpBindHost, 0, 1);
            AddTransportValue(layout, "本地端口", udpBindPort, 2, 1);
            udpRemoteHost.Text = "192.168.1.201";
            AddTransportValue(layout, "LQ目标IP", udpRemoteHost, 4, 1);
            AddTransportValue(layout, "目标端口", udpRemotePort, 6, 1);

            var hint = new Label
            {
                Text = "UDP 默认使用 0.0.0.0:14601 ↔ 192.168.1.201:14601",
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            layout.Controls.Add(hint, 6, 0);
            layout.SetColumnSpan(hint, 2);
            group.Controls.Add(layout);
            transportSelection.SelectedIndex = 0;
            UpdateTransportControls();
            return group;
        }

        private static void AddTransportValue(TableLayoutPanel layout, string text, Control value, int column,
            int row)
        {
            layout.Controls.Add(new Label {Text = text, AutoSize = true, Anchor = AnchorStyles.Left}, column, row);
            value.Dock = DockStyle.Fill;
            layout.Controls.Add(value, column + 1, row);
        }

        private void UpdateTransportControls()
        {
            var udp = String.Equals(transportSelection.SelectedItem as string, UdpTransport,
                StringComparison.Ordinal);
            serialDevice.Enabled = !udp;
            serialBaud.Enabled = !udp;
            udpBindHost.Enabled = udp;
            udpBindPort.Enabled = udp;
            udpRemoteHost.Enabled = udp;
            udpRemotePort.Enabled = udp;
        }
    }

    /// <summary>
    /// 对多机总控中勾选的飞机并行施加稳态风、MILSPEC 湍流和阵风。
    /// </summary>
    internal sealed class JSBSimFleetEnvironmentForm : Form
    {
        private sealed class EnvironmentTargetResult
        {
            public HilFaultTarget Target;
            public JObject Response;
            public Exception Error;
        }

        private readonly List<HilFaultTarget> configuredTargets;
        private readonly CheckedListBox targetVehicles = new CheckedListBox();
        private readonly Label operationStatus = new Label();
        private readonly TextBox statusText = new TextBox();
        private readonly System.Windows.Forms.Timer statusTimer = new System.Windows.Forms.Timer();

        private readonly CheckBox steadyEnabled = new CheckBox();
        private readonly NumericUpDown windSpeed = Number(0, 100, 0, 1, 0.5M);
        private readonly NumericUpDown windDirection = Number(0, 359.9M, 0, 1, 1);
        private readonly NumericUpDown verticalWind = Number(-50, 50, 0, 1, 0.5M);

        private readonly CheckBox turbulenceEnabled = new CheckBox();
        private readonly ComboBox turbulencePreset = new ComboBox();
        private readonly NumericUpDown turbulenceSeverity = Number(0, 7, 3, 0, 1);
        private readonly NumericUpDown turbulenceSpeed = Number(0, 100, 7.62M, 2, 0.5M);
        private readonly NumericUpDown turbulenceSeed = Number(0, 2147483647, 12345, 0, 1);
        private readonly CheckBox independentSeeds = new CheckBox();

        private readonly NumericUpDown gustMagnitude = Number(0, 100, 5, 1, 0.5M);
        private readonly NumericUpDown gustDirection = Number(0, 359.9M, 0, 1, 1);
        private readonly NumericUpDown gustVertical = Number(-50, 50, 0, 1, 0.5M);
        private readonly NumericUpDown gustRampIn = Number(0, 300, 1, 1, 0.5M);
        private readonly NumericUpDown gustHold = Number(0, 300, 1, 1, 0.5M);
        private readonly NumericUpDown gustRampOut = Number(0, 300, 1, 1, 0.5M);

        private readonly Button applyButton = new Button();
        private readonly Button gustButton = new Button();
        private readonly Button resetButton = new Button();
        private bool pollInProgress;

        public JSBSimFleetEnvironmentForm(IEnumerable<HilFaultTarget> targets)
        {
            configuredTargets = targets == null ? new List<HilFaultTarget>() : targets.ToList();
            Text = "JSBSim HIL 多机环境干扰";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(980, 620);
            Size = new Size(1120, 720);
            AutoScaleMode = AutoScaleMode.Dpi;
            BuildInterface();

            foreach (var item in configuredTargets)
                targetVehicles.Items.Add(item, true);

            turbulencePreset.Items.AddRange(new object[] {"light", "moderate", "severe", "custom"});
            turbulencePreset.SelectedIndex = 0;
            turbulencePreset.SelectedIndexChanged += TurbulencePresetChanged;
            independentSeeds.Text = "各机使用独立随机序列";
            independentSeeds.Checked = true;

            statusTimer.Interval = 1000;
            statusTimer.Tick += async delegate { await RefreshStatusAsync(); };
            statusTimer.Start();
            FormClosed += delegate { statusTimer.Stop(); };
        }

        private void BuildInterface()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 1,
                RowCount = 4
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 125));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 270));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            root.Controls.Add(BuildTargetsGroup(), 0, 0);
            root.Controls.Add(BuildEnvironmentGroup(), 0, 1);

            applyButton.Text = "应用风和湍流到所选";
            gustButton.Text = "向所选触发阵风";
            resetButton.Text = "所选全部恢复无风";
            applyButton.AutoSize = gustButton.AutoSize = resetButton.AutoSize = true;
            applyButton.Click += async delegate { await ApplyEnvironmentAsync(); };
            gustButton.Click += async delegate { await TriggerGustAsync(); };
            resetButton.Click += async delegate { await ResetEnvironmentAsync(); };
            operationStatus.Text = "等待操作";
            operationStatus.AutoSize = true;
            operationStatus.Margin = new Padding(18, 10, 3, 3);
            var actions = new FlowLayoutPanel {Dock = DockStyle.Fill, WrapContents = false};
            actions.Controls.Add(applyButton);
            actions.Controls.Add(gustButton);
            actions.Controls.Add(resetButton);
            actions.Controls.Add(operationStatus);
            root.Controls.Add(actions, 0, 2);

            var statusGroup = new GroupBox {Text = "所选飞机环境状态（1 Hz）", Dock = DockStyle.Fill};
            statusText.Dock = DockStyle.Fill;
            statusText.Multiline = true;
            statusText.ReadOnly = true;
            statusText.ScrollBars = ScrollBars.Both;
            statusText.WordWrap = false;
            statusText.Font = new Font(FontFamily.GenericMonospace, 9F);
            statusGroup.Controls.Add(statusText);
            root.Controls.Add(statusGroup, 0, 3);
            Controls.Add(root);
        }

        private Control BuildTargetsGroup()
        {
            var group = new GroupBox {Text = "环境干扰目标", Dock = DockStyle.Fill};
            var layout = new TableLayoutPanel {Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1};
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            targetVehicles.Dock = DockStyle.Fill;
            targetVehicles.CheckOnClick = true;
            targetVehicles.HorizontalScrollbar = true;
            layout.Controls.Add(targetVehicles, 0, 0);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            var selectAll = new Button {Text = "全选", Width = 100};
            var selectNone = new Button {Text = "全不选", Width = 100};
            var test = new Button {Text = "测试所选", Width = 100};
            selectAll.Click += delegate { SetAllTargets(true); };
            selectNone.Click += delegate { SetAllTargets(false); };
            test.Click += async delegate { await TestTargetsAsync(); };
            buttons.Controls.Add(selectAll);
            buttons.Controls.Add(selectNone);
            buttons.Controls.Add(test);
            layout.Controls.Add(buttons, 1, 0);
            group.Controls.Add(layout);
            return group;
        }

        private Control BuildEnvironmentGroup()
        {
            var group = new GroupBox {Text = "环境参数", Dock = DockStyle.Fill};
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight
            };
            flow.Controls.Add(BuildSteadyWindGroup());
            flow.Controls.Add(BuildTurbulenceGroup());
            flow.Controls.Add(BuildGustGroup());
            group.Controls.Add(flow);
            return group;
        }

        private Control BuildSteadyWindGroup()
        {
            var group = EnvironmentBox("稳态风", 275);
            var layout = EnvironmentLayout(5);
            steadyEnabled.Text = "启用稳态风";
            AddWide(layout, steadyEnabled, 0);
            AddValue(layout, "水平风速 (m/s)", windSpeed, 1);
            AddValue(layout, "来向 (°)", windDirection, 2);
            AddValue(layout, "垂直风，向上为正", verticalWind, 3);
            group.Controls.Add(layout);
            return group;
        }

        private Control BuildTurbulenceGroup()
        {
            var group = EnvironmentBox("MILSPEC 湍流", 310);
            var layout = EnvironmentLayout(6);
            turbulenceEnabled.Text = "启用湍流";
            AddWide(layout, turbulenceEnabled, 0);
            turbulencePreset.DropDownStyle = ComboBoxStyle.DropDownList;
            AddValue(layout, "预设", turbulencePreset, 1);
            AddValue(layout, "严重等级 (0-7)", turbulenceSeverity, 2);
            AddValue(layout, "20ft 风速 (m/s)", turbulenceSpeed, 3);
            AddValue(layout, "基础随机种子", turbulenceSeed, 4);
            AddWide(layout, independentSeeds, 5);
            group.Controls.Add(layout);
            return group;
        }

        private Control BuildGustGroup()
        {
            var group = EnvironmentBox("单次平滑阵风", 340);
            var layout = EnvironmentLayout(5);
            AddValue(layout, "水平强度 (m/s)", gustMagnitude, 0);
            AddValue(layout, "来向 (°)", gustDirection, 1);
            AddValue(layout, "垂直分量，向上为正", gustVertical, 2);
            AddValue(layout, "渐入/保持/渐出 (s)", DurationPanel(), 3);
            group.Controls.Add(layout);
            return group;
        }

        private Control DurationPanel()
        {
            var flow = new FlowLayoutPanel {Dock = DockStyle.Fill, WrapContents = false, Margin = Padding.Empty};
            gustRampIn.Width = gustHold.Width = gustRampOut.Width = 40;
            gustRampIn.Margin = gustHold.Margin = gustRampOut.Margin = new Padding(1);
            flow.Controls.Add(gustRampIn);
            flow.Controls.Add(gustHold);
            flow.Controls.Add(gustRampOut);
            return flow;
        }

        private static GroupBox EnvironmentBox(string text, int width)
        {
            return new GroupBox {Text = text, Width = width, Height = 230, Margin = new Padding(5)};
        }

        private static TableLayoutPanel EnvironmentLayout(int rows)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = rows,
                Padding = new Padding(5)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            return layout;
        }

        private static void AddValue(TableLayoutPanel layout, string text, Control value, int row)
        {
            layout.Controls.Add(new Label {Text = text, AutoSize = true, Anchor = AnchorStyles.Left}, 0, row);
            value.Dock = DockStyle.Fill;
            layout.Controls.Add(value, 1, row);
        }

        private static void AddWide(TableLayoutPanel layout, Control value, int row)
        {
            value.Dock = DockStyle.Fill;
            layout.Controls.Add(value, 0, row);
            layout.SetColumnSpan(value, 2);
        }

        private static NumericUpDown Number(decimal minimum, decimal maximum, decimal value, int decimals,
            decimal increment)
        {
            return new NumericUpDown
            {
                Minimum = minimum,
                Maximum = maximum,
                Value = value,
                DecimalPlaces = decimals,
                Increment = increment,
                ThousandsSeparator = maximum >= 100000
            };
        }

        private void SetAllTargets(bool value)
        {
            for (var index = 0; index < targetVehicles.Items.Count; index++)
                targetVehicles.SetItemChecked(index, value);
        }

        private List<HilFaultTarget> GetSelectedTargets()
        {
            return targetVehicles.CheckedItems.Cast<HilFaultTarget>().ToList();
        }

        private async Task<EnvironmentTargetResult[]> CallTargetsAsync(
            Func<HilSupervisorClient, HilFaultTarget, Task<JObject>> action)
        {
            var targets = GetSelectedTargets();
            if (targets.Count == 0)
                throw new InvalidOperationException("请至少勾选一架目标飞机。");
            var tasks = targets.Select(async item =>
            {
                try
                {
                    using (var client = new HilSupervisorClient(item.ApiAddress))
                    {
                        return new EnvironmentTargetResult
                        {
                            Target = item,
                            Response = await action(client, item)
                        };
                    }
                }
                catch (Exception ex)
                {
                    return new EnvironmentTargetResult {Target = item, Error = ex};
                }
            });
            return await Task.WhenAll(tasks);
        }

        private JObject BuildEnvironmentBody(HilFaultTarget target)
        {
            var seed = Decimal.ToInt32(turbulenceSeed.Value);
            if (independentSeeds.Checked)
                seed = seed > Int32.MaxValue - target.SysId ? seed - target.SysId : seed + target.SysId;
            return new JObject
            {
                ["steady_wind"] = new JObject
                {
                    ["enabled"] = steadyEnabled.Checked,
                    ["speed_mps"] = (double) windSpeed.Value,
                    ["direction_from_deg"] = (double) windDirection.Value,
                    ["vertical_mps"] = (double) verticalWind.Value
                },
                ["turbulence"] = new JObject
                {
                    ["enabled"] = turbulenceEnabled.Checked,
                    ["preset"] = turbulencePreset.SelectedItem == null
                        ? "light"
                        : turbulencePreset.SelectedItem.ToString(),
                    ["severity"] = Decimal.ToInt32(turbulenceSeverity.Value),
                    ["windspeed_at_20ft_mps"] = (double) turbulenceSpeed.Value,
                    ["random_seed"] = seed
                }
            };
        }

        private JObject BuildGustBody()
        {
            return new JObject
            {
                ["magnitude_mps"] = (double) gustMagnitude.Value,
                ["direction_from_deg"] = (double) gustDirection.Value,
                ["vertical_mps"] = (double) gustVertical.Value,
                ["ramp_in_s"] = (double) gustRampIn.Value,
                ["hold_s"] = (double) gustHold.Value,
                ["ramp_out_s"] = (double) gustRampOut.Value
            };
        }

        private async Task TestTargetsAsync()
        {
            await ExecuteAsync("连接测试", (client, item) =>
                client.GetHealthAsync(CancellationToken.None), false);
        }

        private async Task ApplyEnvironmentAsync()
        {
            await ExecuteAsync("应用环境", (client, item) =>
                client.ApplyEnvironmentAsync(item.SysId, BuildEnvironmentBody(item), CancellationToken.None), true);
        }

        private async Task TriggerGustAsync()
        {
            await ExecuteAsync("触发阵风", (client, item) =>
                client.TriggerGustAsync(item.SysId, BuildGustBody(), CancellationToken.None), true);
        }

        private async Task ResetEnvironmentAsync()
        {
            await ExecuteAsync("恢复无风", (client, item) =>
                client.ResetEnvironmentAsync(item.SysId, CancellationToken.None), true);
            steadyEnabled.Checked = false;
            turbulenceEnabled.Checked = false;
        }

        private async Task ExecuteAsync(string operation,
            Func<HilSupervisorClient, HilFaultTarget, Task<JObject>> action, bool refreshAfter)
        {
            applyButton.Enabled = gustButton.Enabled = resetButton.Enabled = false;
            try
            {
                var results = await CallTargetsAsync(action);
                var failures = results.Where(result => result.Error != null).ToArray();
                operationStatus.Text = operation + "：成功 " + (results.Length - failures.Length) + "/" +
                                       results.Length;
                if (refreshAfter)
                    await RefreshStatusAsync();
                if (failures.Length > 0)
                    MessageBox.Show(this,
                        string.Join(Environment.NewLine, failures.Select(result =>
                            result.Target.Name + "：" + result.Error.Message).ToArray()),
                        operation + "未全部完成", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                operationStatus.Text = operation + "失败";
                MessageBox.Show(this, ex.Message, operation + "失败", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            finally
            {
                applyButton.Enabled = gustButton.Enabled = resetButton.Enabled = true;
            }
        }

        private async Task RefreshStatusAsync()
        {
            if (pollInProgress || GetSelectedTargets().Count == 0)
                return;
            pollInProgress = true;
            try
            {
                var results = await CallTargetsAsync((client, item) =>
                    client.GetHealthAsync(CancellationToken.None));
                var lines = results.Select(result =>
                {
                    if (result.Error != null)
                        return result.Target.Name + "：离线 - " + result.Error.Message;
                    var status = result.Response["status"] as JObject;
                    var environment = result.Response["environment"] as JObject;
                    var actual = environment == null ? null : environment["actual_total_wind_mps"] as JObject;
                    var gust = environment == null ? null : environment["gust"] as JObject;
                    return string.Format(CultureInfo.InvariantCulture,
                        "{0}  SYSID={1}  总风 N/E/D={2}/{3}/{4} m/s  阵风={5}  实时倍率={6}",
                        result.Target.Name, result.Target.SysId, Token(actual, "north"), Token(actual, "east"),
                        Token(actual, "down"), Token(gust, "phase"), Token(status, "realtime_factor"));
                });
                statusText.Text = string.Join(Environment.NewLine, lines.ToArray());
            }
            catch (Exception ex)
            {
                statusText.Text = "状态读取失败：" + ex.Message;
            }
            finally
            {
                pollInProgress = false;
            }
        }

        private void TurbulencePresetChanged(object sender, EventArgs e)
        {
            switch (turbulencePreset.SelectedItem as string)
            {
                case "light":
                    turbulenceSeverity.Value = 3;
                    turbulenceSpeed.Value = 7.62M;
                    break;
                case "moderate":
                    turbulenceSeverity.Value = 4;
                    turbulenceSpeed.Value = 15.24M;
                    break;
                case "severe":
                    turbulenceSeverity.Value = 6;
                    turbulenceSpeed.Value = 22.86M;
                    break;
            }
        }

        private static string Token(JObject source, string name)
        {
            if (source == null || source[name] == null)
                return "--";
            return source[name].ToString(Formatting.None);
        }
    }
}
