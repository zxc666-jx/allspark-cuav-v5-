using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace MissionPlanner.HIL
{
    internal sealed class HilFaultTarget
    {
        public readonly string Name;
        public readonly int SysId;
        public readonly string ApiAddress;

        public HilFaultTarget(string name, int sysId, string apiAddress)
        {
            Name = name;
            SysId = sysId;
            ApiAddress = apiAddress;
        }

        public override string ToString()
        {
            return Name + " (SYSID " + SysId + ", " + ApiAddress + ")";
        }
    }

    /// <summary>
    /// 独立的 JSBSim HIL 故障注入窗口。它只访问本机 HTTP API，
    /// 不占用飞控的 MAVLink 或 JBS1/JBO1 串口。
    /// </summary>
    public sealed class JSBSimFaultInjectionForm : Form
    {
        private sealed class Choice
        {
            public readonly string Text;
            public readonly string Value;

            public Choice(string text, string value)
            {
                Text = text;
                Value = value;
            }

            public override string ToString()
            {
                return Text;
            }
        }

        private readonly TextBox apiAddress = new TextBox();
        private readonly NumericUpDown sysid = Number(1, 255, 1, 0, 1);
        private readonly NumericUpDown slot = Number(0, 31, 0, 0, 1);
        private readonly CheckBox enabled = new CheckBox();
        private readonly ComboBox faultType = new ComboBox();
        private readonly ComboBox target = new ComboBox();
        private readonly ComboBox profile = new ComboBox();
        private readonly NumericUpDown level = Number(-10000, 10000, 0.1M, 2, 0.1M);
        private readonly NumericUpDown secondary = Number(0, 10000, 0, 2, 0.1M);
        private readonly NumericUpDown startDelay = Number(0, 3600, 0, 1, 0.5M);
        private readonly NumericUpDown rampIn = Number(0, 3600, 0, 1, 0.5M);
        private readonly NumericUpDown duration = Number(0, 3600, 10, 1, 0.5M);
        private readonly NumericUpDown rampOut = Number(0, 3600, 0, 1, 0.5M);
        private readonly CheckBox autoRecover = new CheckBox();
        private readonly Label levelLabel = new Label();
        private readonly Label secondaryLabel = new Label();
        private readonly Label helpLabel = new Label();
        private readonly Label connectionState = new Label();
        private readonly CheckedListBox targetVehicles = new CheckedListBox();
        private readonly TextBox statusText = new TextBox();
        private readonly System.Windows.Forms.Timer statusTimer = new System.Windows.Forms.Timer();

        private readonly List<HilFaultTarget> configuredTargets;
        private readonly bool multiTargetMode;
        private bool pollInProgress;

        public JSBSimFaultInjectionForm() : this(null)
        {
        }

        internal JSBSimFaultInjectionForm(IEnumerable<HilFaultTarget> targets)
        {
            configuredTargets = targets == null ? new List<HilFaultTarget>() : targets.ToList();
            multiTargetMode = configuredTargets.Count > 0;
            Text = "JSBSim HIL 故障注入";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(980, 680);
            Size = new Size(1120, 780);
            AutoScaleMode = AutoScaleMode.None;
            BuildInterface();
            SetChoices();
            UpdateFaultTypeHelp(null, EventArgs.Empty);

            foreach (var item in configuredTargets)
                targetVehicles.Items.Add(item, true);

            statusTimer.Interval = multiTargetMode ? 1000 : 250;
            statusTimer.Tick += PollStatus;
            FormClosed += delegate
            {
                statusTimer.Stop();
            };
        }

        private void BuildInterface()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ColumnCount = 1,
                RowCount = 3
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, multiTargetMode ? 165F : 95F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 340F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            Controls.Add(root);
            root.Controls.Add(BuildConnectionGroup(), 0, 0);
            root.Controls.Add(BuildConfigurationGroup(), 0, 1);
            root.Controls.Add(BuildStatusGroup(), 0, 2);
        }

        private Control BuildConnectionGroup()
        {
            var group = new GroupBox {Text = "本机控制接口", Dock = DockStyle.Fill};
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                ColumnCount = 6,
                RowCount = multiTargetMode ? 3 : 2
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 85F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));

            layout.Controls.Add(LabelFor("API 地址"), 0, 0);
            apiAddress.Text = "http://127.0.0.1:8765/";
            apiAddress.Dock = DockStyle.Fill;
            apiAddress.Enabled = !multiTargetMode;
            layout.Controls.Add(apiAddress, 1, 0);
            layout.Controls.Add(LabelFor("SysID"), 2, 0);
            sysid.Dock = DockStyle.Fill;
            sysid.Enabled = !multiTargetMode;
            layout.Controls.Add(sysid, 3, 0);
            var connect = new Button {Text = multiTargetMode ? "测试所选" : "连接测试", Dock = DockStyle.Fill};
            connect.Click += ConnectClicked;
            layout.Controls.Add(connect, 4, 0);
            connectionState.Text = "未连接";
            connectionState.Dock = DockStyle.Fill;
            connectionState.TextAlign = ContentAlignment.MiddleLeft;
            layout.Controls.Add(connectionState, 5, 0);

            var hintRow = 1;
            if (multiTargetMode)
            {
                targetVehicles.Dock = DockStyle.Fill;
                targetVehicles.CheckOnClick = true;
                targetVehicles.IntegralHeight = false;
                layout.Controls.Add(LabelFor("目标飞机"), 0, 1);
                layout.Controls.Add(targetVehicles, 1, 1);
                layout.SetColumnSpan(targetVehicles, 5);
                hintRow = 2;
            }

            var hint = new Label
            {
                Text = multiTargetMode
                    ? "勾选一架或多架飞机；同一组故障参数会并行发送到全部勾选目标。"
                    : "请先在“JSBSim HIL 环境”窗口启动 hil_supervisor，再在这里配置故障通道。",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.Controls.Add(hint, 0, hintRow);
            layout.SetColumnSpan(hint, 6);
            group.Controls.Add(layout);
            return group;
        }

        private Control BuildConfigurationGroup()
        {
            var group = new GroupBox {Text = "故障通道参数", Dock = DockStyle.Fill};
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 6,
                RowCount = 7
            };
            for (var i = 0; i < 3; i++)
            {
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125F));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            }

            AddField(layout, "通道编号", slot, 0, 0);
            enabled.Text = "启用该通道";
            enabled.Checked = true;
            enabled.Dock = DockStyle.Fill;
            layout.Controls.Add(enabled, 2, 0);
            layout.SetColumnSpan(enabled, 2);
            autoRecover.Text = "持续时间结束后自动恢复";
            autoRecover.Checked = true;
            autoRecover.Dock = DockStyle.Fill;
            layout.Controls.Add(autoRecover, 4, 0);
            layout.SetColumnSpan(autoRecover, 2);

            AddField(layout, "故障类型", faultType, 0, 1);
            AddField(layout, "故障对象", target, 2, 1);
            AddField(layout, "变化形式", profile, 4, 1);

            levelLabel.Text = "故障程度";
            AddField(layout, levelLabel, level, 0, 2);
            secondaryLabel.Text = "副参数";
            AddField(layout, secondaryLabel, secondary, 2, 2);
            AddField(layout, "触发延迟 (s)", startDelay, 4, 2);

            AddField(layout, "渐入时间 (s)", rampIn, 0, 3);
            AddField(layout, "保持时间 (s)", duration, 2, 3);
            AddField(layout, "恢复时间 (s)", rampOut, 4, 3);

            helpLabel.Dock = DockStyle.Fill;
            helpLabel.AutoEllipsis = true;
            helpLabel.TextAlign = ContentAlignment.MiddleLeft;
            helpLabel.Padding = new Padding(6);
            layout.Controls.Add(helpLabel, 0, 4);
            layout.SetColumnSpan(helpLabel, 6);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(4)
            };
            var inject = new Button {Text = "注入 / 更新通道", Width = 160, Height = 36};
            var clear = new Button {Text = "清除当前通道", Width = 150, Height = 36};
            var reset = new Button {Text = "全部故障恢复", Width = 150, Height = 36};
            inject.Click += InjectClicked;
            clear.Click += ClearClicked;
            reset.Click += ResetClicked;
            buttons.Controls.Add(inject);
            buttons.Controls.Add(clear);
            buttons.Controls.Add(reset);
            layout.Controls.Add(buttons, 0, 5);
            layout.SetColumnSpan(buttons, 6);

            var warning = new Label
            {
                Text = "提示：保持时间设为 0 或取消“自动恢复”时，故障会持续到手动清除。多个通道可同时启用。",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.Controls.Add(warning, 0, 6);
            layout.SetColumnSpan(warning, 6);
            group.Controls.Add(layout);
            return group;
        }

        private Control BuildStatusGroup()
        {
            var group = new GroupBox {Text = "实时故障状态（4 Hz）", Dock = DockStyle.Fill};
            statusText.Multiline = true;
            statusText.ReadOnly = true;
            statusText.ScrollBars = ScrollBars.Both;
            statusText.WordWrap = false;
            statusText.Font = new Font(FontFamily.GenericMonospace, 9F);
            statusText.Dock = DockStyle.Fill;
            group.Controls.Add(statusText);
            return group;
        }

        private void SetChoices()
        {
            faultType.DropDownStyle = ComboBoxStyle.DropDownList;
            faultType.Items.AddRange(new object[]
            {
                new Choice("推力损失", "THRUST_LOSS"),
                new Choice("舵效下降", "SURFACE_LOSS"),
                new Choice("舵面卡死", "SURFACE_JAM"),
                new Choice("传感器噪声", "SENSOR_NOISE"),
                new Choice("传感器漂移", "SENSOR_DRIFT"),
                new Choice("传感器固定偏差", "SENSOR_BIAS"),
                new Choice("传感器冻结/失效", "SENSOR_FAIL")
            });
            faultType.SelectedIndex = 0;
            faultType.SelectedIndexChanged += UpdateFaultTypeHelp;

            profile.DropDownStyle = ComboBoxStyle.DropDownList;
            profile.Items.AddRange(new object[]
            {
                new Choice("阶跃", "step"),
                new Choice("渐变", "ramp")
            });
            profile.SelectedIndex = 0;
            target.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void UpdateFaultTypeHelp(object sender, EventArgs e)
        {
            var type = SelectedValue(faultType);
            target.Items.Clear();
            secondary.Enabled = false;
            profile.Enabled = true;
            level.Enabled = true;
            level.DecimalPlaces = 2;
            level.Increment = 0.1M;

            if (type == "THRUST_LOSS")
            {
                target.Items.Add(new Choice("动力系统", "PROPULSION"));
                levelLabel.Text = "损失比例 (0~1)";
                secondaryLabel.Text = "未使用";
                level.Value = 0.3M;
                helpLabel.Text = "保留飞控油门 PWM，在 JSBSim 实际推进执行量处降低输出。";
            }
            else if (type == "SURFACE_LOSS" || type == "SURFACE_JAM")
            {
                AddSurfaceTargets();
                if (type == "SURFACE_LOSS")
                {
                    levelLabel.Text = "损失比例 (0~1)";
                    level.Value = 0.3M;
                    helpLabel.Text = "保留飞控舵面 PWM，只降低送入 JSBSim 的实际舵面作用。";
                }
                else
                {
                    levelLabel.Text = "卡死角度 (°)";
                    level.Value = 5M;
                    helpLabel.Text = "故障激活后实际舵面保持在指定角度，飞控输出仍正常记录。";
                }
                secondaryLabel.Text = "未使用";
            }
            else
            {
                target.Items.Add(new Choice("空速", "AIRSPEED"));
                target.Items.Add(new Choice("气压高度", "ALTITUDE"));
                if (type == "SENSOR_NOISE")
                {
                    levelLabel.Text = "噪声 σ";
                    level.Value = 2M;
                    helpLabel.Text = "在测量输出叠加可复现高斯噪声；空速单位 m/s，高度单位 m。";
                }
                else if (type == "SENSOR_DRIFT")
                {
                    levelLabel.Text = "漂移速率 /s";
                    secondaryLabel.Text = "最大漂移量";
                    secondary.Enabled = true;
                    level.Value = 1M;
                    secondary.Value = 20M;
                    helpLabel.Text = "从触发时刻累计漂移，到最大漂移量后保持。";
                }
                else if (type == "SENSOR_BIAS")
                {
                    levelLabel.Text = "固定偏差";
                    level.Value = 5M;
                    helpLabel.Text = "只改变发送给飞控的测量值，不改变 JSBSim 飞机真值。";
                }
                else
                {
                    levelLabel.Text = "未使用";
                    level.Enabled = false;
                    profile.Enabled = false;
                    helpLabel.Text = "保持最后一个有效测量值，API 状态同时标记 valid=false。";
                }
            }
            if (target.Items.Count > 0)
                target.SelectedIndex = 0;
        }

        private void AddSurfaceTargets()
        {
            target.Items.Add(new Choice("副翼", "AILERON"));
            target.Items.Add(new Choice("升降舵", "ELEVATOR"));
            target.Items.Add(new Choice("方向舵", "RUDDER"));
        }

        private sealed class FaultTargetResult
        {
            public HilFaultTarget Target;
            public JObject Response;
            public Exception Error;
        }

        private List<HilFaultTarget> GetSelectedTargets()
        {
            if (!multiTargetMode)
                return new List<HilFaultTarget>
                {
                    new HilFaultTarget("SYSID " + (int) sysid.Value, (int) sysid.Value, apiAddress.Text.Trim())
                };
            return targetVehicles.CheckedItems.Cast<HilFaultTarget>().ToList();
        }

        private async Task<FaultTargetResult[]> CallTargetsAsync(
            Func<HilSupervisorClient, HilFaultTarget, Task<JObject>> action)
        {
            var targets = GetSelectedTargets();
            if (targets.Count == 0)
                throw new InvalidOperationException("请至少勾选一架目标飞机。");
            var tasks = targets.Select(async item =>
            {
                try
                {
                    using (var value = new HilSupervisorClient(item.ApiAddress))
                    {
                        return new FaultTargetResult
                        {
                            Target = item,
                            Response = await action(value, item)
                        };
                    }
                }
                catch (Exception ex)
                {
                    return new FaultTargetResult {Target = item, Error = ex};
                }
            });
            return await Task.WhenAll(tasks);
        }

        private async void ConnectClicked(object sender, EventArgs e)
        {
            try
            {
                var results = await CallTargetsAsync((value, item) =>
                    value.GetFaultsAsync(item.SysId, CancellationToken.None));
                var failures = results.Where(result => result.Error != null).ToArray();
                connectionState.Text = failures.Length == 0
                    ? "已连接 " + results.Length + " 架"
                    : "成功 " + (results.Length - failures.Length) + "/" + results.Length;
                statusTimer.Start();
                await RefreshStatus();
                if (failures.Length > 0)
                    MessageBox.Show(this,
                        string.Join(Environment.NewLine, failures.Select(result =>
                            result.Target.Name + "：" + result.Error.Message).ToArray()),
                        "部分故障接口连接失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                connectionState.Text = "连接失败";
                MessageBox.Show(this, ex.Message, "故障注入连接失败", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private async void InjectClicked(object sender, EventArgs e)
        {
            var body = new JObject
            {
                ["enabled"] = enabled.Checked,
                ["fault_type"] = SelectedValue(faultType),
                ["target"] = SelectedValue(target),
                ["profile"] = SelectedValue(profile) ?? "step",
                ["level"] = (double) level.Value,
                ["secondary"] = (double) secondary.Value,
                ["start_delay_s"] = (double) startDelay.Value,
                ["ramp_in_s"] = (double) rampIn.Value,
                ["duration_s"] = (double) duration.Value,
                ["ramp_out_s"] = (double) rampOut.Value,
                ["auto_recover"] = autoRecover.Checked
            };
            await ExecuteAsync("注入故障", delegate(HilSupervisorClient value, HilFaultTarget item)
            {
                return value.ConfigureFaultAsync(item.SysId, (int) slot.Value, body,
                    CancellationToken.None);
            });
        }

        private async void ClearClicked(object sender, EventArgs e)
        {
            await ExecuteAsync("清除故障", delegate(HilSupervisorClient value, HilFaultTarget item)
            {
                return value.ClearFaultAsync(item.SysId, (int) slot.Value,
                    CancellationToken.None);
            });
        }

        private async void ResetClicked(object sender, EventArgs e)
        {
            await ExecuteAsync("全部故障恢复", delegate(HilSupervisorClient value, HilFaultTarget item)
            {
                return value.ResetFaultsAsync(item.SysId, CancellationToken.None);
            });
        }

        private async Task ExecuteAsync(string operation,
            Func<HilSupervisorClient, HilFaultTarget, Task<JObject>> action)
        {
            try
            {
                var results = await CallTargetsAsync(action);
                var failures = results.Where(result => result.Error != null).ToArray();
                connectionState.Text = operation + "：成功 " + (results.Length - failures.Length) + "/" +
                                       results.Length;
                statusTimer.Start();
                await RefreshStatus();
                if (failures.Length > 0)
                    MessageBox.Show(this,
                        string.Join(Environment.NewLine, failures.Select(result =>
                            result.Target.Name + "：" + result.Error.Message).ToArray()),
                        operation + "未全部完成", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, operation + "失败", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private async void PollStatus(object sender, EventArgs e)
        {
            await RefreshStatus();
        }

        private async Task RefreshStatus()
        {
            if (pollInProgress)
                return;
            pollInProgress = true;
            try
            {
                var results = await CallTargetsAsync((value, item) =>
                    value.GetFaultsAsync(item.SysId, CancellationToken.None));
                var builder = new StringBuilder();
                foreach (var result in results)
                {
                    builder.AppendLine("===== " + result.Target.Name + " / SYSID " + result.Target.SysId + " =====");
                    builder.AppendLine(result.Error == null ? FormatStatus(result.Response) : result.Error.Message);
                }
                statusText.Text = builder.ToString();
                var success = results.Count(result => result.Error == null);
                connectionState.Text = "状态在线 " + success + "/" + results.Length;
            }
            catch (Exception ex)
            {
                connectionState.Text = "状态中断";
                statusText.Text = ex.Message;
                statusTimer.Stop();
            }
            finally
            {
                pollInProgress = false;
            }
        }

        private static string FormatStatus(JObject faults)
        {
            var builder = new StringBuilder();
            builder.AppendLine("仿真时间: " + TextOf(faults["sim_time_s"]) + " s");
            builder.AppendLine("通道上限: " + TextOf(faults["max_channels"]));
            builder.AppendLine();
            var channels = faults["channels"] as JArray;
            if (channels == null || channels.Count == 0)
                builder.AppendLine("当前没有已配置故障通道。\r\n");
            else
            {
                foreach (var token in channels)
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture,
                        "通道 {0}: {1} / {2}  阶段={3}  系数={4}\r\n",
                        TextOf(token["slot"]), TextOf(token["fault_type"]), TextOf(token["target"]),
                        TextOf(token["phase"]), TextOf(token["factor"]));
                }
                builder.AppendLine();
            }

            builder.AppendLine("执行机构（正常 → 实际）:");
            var normal = faults.SelectToken("actuators.normal") as JObject;
            var actual = faults.SelectToken("actuators.actual") as JObject;
            if (normal != null && actual != null)
                foreach (var property in normal.Properties())
                    builder.AppendLine("  " + property.Name + ": " + TextOf(property.Value) + " → " +
                                       TextOf(actual[property.Name]));

            builder.AppendLine();
            builder.AppendLine("传感器（真值 → 测量，valid）:");
            foreach (var name in new[] {"AIRSPEED", "ALTITUDE"})
            {
                var sensor = faults.SelectToken("sensors." + name) as JObject;
                if (sensor != null)
                    builder.AppendLine("  " + name + ": " + TextOf(sensor["raw"]) + " → " +
                                       TextOf(sensor["measured"]) + ", " + TextOf(sensor["valid"]));
            }
            return builder.ToString();
        }

        private static string SelectedValue(ComboBox combo)
        {
            var choice = combo.SelectedItem as Choice;
            return choice == null ? null : choice.Value;
        }

        private static string TextOf(JToken token)
        {
            return token == null || token.Type == JTokenType.Null
                ? "-"
                : token.ToString();
        }

        private static Label LabelFor(string text)
        {
            return new Label {Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft};
        }

        private static void AddField(TableLayoutPanel layout, string text, Control value, int column, int row)
        {
            AddField(layout, LabelFor(text), value, column, row);
        }

        private static void AddField(TableLayoutPanel layout, Label label, Control value, int column, int row)
        {
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
            value.Dock = DockStyle.Fill;
            value.Margin = new Padding(4);
            layout.Controls.Add(label, column, row);
            layout.Controls.Add(value, column + 1, row);
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
                ThousandsSeparator = maximum >= 10000
            };
        }
    }
}
