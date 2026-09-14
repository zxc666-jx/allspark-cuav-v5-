# ArduPlane + Mesh + JSBSim HIL 系统详细设计与实施方案

状态：Draft v0.1  
日期：2026-08-18  
配套协议：[HIL_MAVLink_Protocol.md](./HIL_MAVLink_Protocol.md)、[4_HIL_Serial_Protocol.md](./4_HIL_Serial_Protocol.md)

> ⚠️ **架构更新（2026-08-21）**：HIL 闭环链路已从「自定义 MAVLink 消息」切换为「**串口二进制协议 + 飞控 `AP_ExternalAHRS_JSBSim` 后端**」（见 [4_HIL_Serial_Protocol.md](./4_HIL_Serial_Protocol.md)）。本文档 §7「私有 MAVLink 方言」、§8「ArduPlane 固件设计」仍按原 MAVLink 方案描述，需与串口方案对齐后再冻结。

## 1. 项目目标

建设一套面向真实 ArduPlane 飞控的硬件在环系统。飞控运行定制固件并通过天空端 Mesh 节点与地面 Mesh 基站通信；地面 HIL Supervisor 运行 JSBSim，接收飞控最终舵面/油门输出，推进飞机动力学，再把模拟 IMU、磁罗盘、气压计、空速和 GPS 数据定向发送回对应飞控。

Mission Planner 不参与高频实时闭环，只承担：

- 飞控连接、参数读取和常规地面站功能；
- HIL 会话配置、启动、暂停、重置和停止；
- 健康状态、告警、日志和轨迹可视化；
- 向 Supervisor 传递 `SERVOx_*` 执行器标定参数；
- 接收 Supervisor 提供的飞控遥测镜像和 JSBSim 真值状态。

最终目标数据链：

```text
真实 ArduPlane 飞控
  ├─ SERVO_OUTPUT_RAW ───────────────────────────────┐
  ├─ HEARTBEAT/状态/参数/任务 ──────────────────────┤
  └─ HIL_IMU_COMPACT/HIL_NAV_COMPACT ◀─────────────┤
                                                     │
天空端 Mesh ⇄ 地面 Mesh 基站 ⇄ HIL Supervisor/Router│
                                      │              │
                                      ├─ JSBSim Worker
                                      ├─ Mission Planner 遥测镜像
                                      ├─ HILPanel 本地 API/WebSocket
                                      ├─ 故障注入
                                      └─ 会话日志
```

## 2. 范围

### 2.1 v1 必须实现

- 单架常规尾翼固定翼；
- `c172x` 和可配置 JSBSim 模型；
- JSBSim 400 Hz 固定仿真步长；
- 飞控最终 PWM 输入，50–100 Hz，步间零阶保持；
- 自定义 IMU/空气数据消息 100 Hz；
- 自定义 GPS/NAV 消息 10 Hz；
- 基于 `target_system/target_component` 的定向注入；
- Supervisor 唯一占用 Mesh 接口；
- Mission Planner UDP 遥测镜像；
- 本地 HTTP API 和 WebSocket 状态流；
- 启动、暂停、恢复、重置、停止；
- 原始链路、控制量、真值、传感器、状态和异常日志；
- 链路延迟、丢包、乱序、频率和实时倍率监测；
- IMU 偏置/噪声、GPS 丢失、气压异常、链路延迟/丢包故障注入。

### 2.2 v1 暂不实现

- CFD 或结构/载荷高保真仿真；
- Elevon、V-tail、差动扰流板的自动解混；
- 多发动机、反推、差动推力；
- SIL 与 HIL 混合编队；
- 通过 Mission Planner UI 处理 100 Hz 传感器数据；
- 依赖 MAVProxy 作为必需路由组件；
- 远程开放 Supervisor 管理 API。

### 2.3 v2 扩展

- 多架飞机，每架独立 Worker、配置、日志和状态机；
- Rascal110 等其他模型；
- 多 IMU、双 GPS、GPS yaw；
- 混控机型和多舵机输出；
- MAVLink 2 签名、会话回放和自动化测试场景。

## 3. 设计约束与基本原则

1. **Mesh 单一所有者**：只有 Supervisor 直接绑定 Mesh 网卡/端口。Mission Planner、HILPanel 和 JSBSim Worker 均不能直接读写 Mesh。
2. **实时路径与 UI 解耦**：UI 卡顿、地图刷新和参数页面操作不能阻塞动力学或传感器发送。
3. **真值与估计分离**：JSBSim 真值只用于生成模拟传感器和监视，不能直接覆盖 ArduPlane EKF/AHRS 结果。
4. **按目标路由**：所有 HIL 传感器消息必须具有目标系统和组件；错误目标不得进入飞控传感器后端。
5. **配置可追溯**：每次会话保存模型、出生点、固件版本、XML 哈希、执行器参数和故障配置快照。
6. **安全失败**：链路或控制超时后采取确定的安全策略，不能无限保持未知的最后油门。
7. **先单机后多机**：单机闭环和频率/延迟验收未通过前，不开始多机功能。
8. **协议先冻结再联调**：XML、生成绑定和固件处理代码必须来自同一版本，禁止手工维护不一致的消息结构。

## 4. 总体架构

### 4.1 目标进程结构

```text
Windows 地面计算机
│
├─ MissionPlanner.exe
│   ├─ 常规飞控页面
│   └─ HILPanel（5–10 Hz UI）
│
├─ hil_supervisor.py（主进程）
│   ├─ MeshTransport
│   ├─ MavlinkRouter
│   ├─ TelemetryMirror
│   ├─ VehicleRegistry
│   ├─ SupervisorAPI
│   ├─ FaultController
│   └─ SessionLogger
│
└─ JSBSim Worker（v1 可同进程，v2 每机独立子进程）
    ├─ ActuatorMapper
    ├─ JSBSim FGFDMExec
    ├─ SensorModel
    └─ FixedRateScheduler
```

### 4.2 为什么 v2 使用每机独立 Worker 进程

- 一架飞机的 JSBSim 异常不会停止其他飞机；
- 每架飞机具有独立 400 Hz 时钟和性能统计；
- 多核并行能力优于单 Python 进程；
- 单机可暂停、重置或更换模型；
- Worker 崩溃后 Supervisor 可以单独重启并记录故障。

v1 单机阶段可继续使用当前 `hil_supervisor.py` 内嵌 Worker，等闭环稳定后再拆分进程，避免过早引入 IPC 调试成本。

## 5. 组件职责

### 5.1 MeshTransport

职责：

- 绑定指定 Mesh 网卡、UDP/TCP 端口或厂商 SDK；
- 收发原始 MAVLink 字节流；
- 记录来源 Mesh 节点、IP、端口、收发时间和字节数；
- 维护 `mesh_node ⇄ MAV_SYSID` 映射；
- 提供有界收发队列和链路统计；
- 支持延迟、丢包、重复和乱序故障注入。

禁止：解析后直接把所有消息广播给所有飞控。

### 5.2 MavlinkRouter

职责：

- 使用项目私有 pymavlink 方言解析 MAVLink 2；
- 校验帧、消息 ID、来源和目标；
- 按 `sysid/compid` 更新车辆注册表；
- `SERVO_OUTPUT_RAW` 送入对应 Worker；
- HIL 传感器消息只送到目标 Mesh 节点；
- 普通飞控遥测镜像给 Mission Planner；
- Mission Planner 发出的命令按目标系统送回 Mesh；
- 统计未知消息、CRC 失败、目标不匹配和序号丢失。

### 5.3 JSBSim Worker

职责：

- 加载模型、出生点和环境配置；
- 以 `dt=0.0025 s` 推进 400 Hz 物理模型；
- 从最新执行器快照读取控制量；
- 生成统一真值帧；
- 按独立采样器输出 100 Hz IMU 和 10 Hz NAV；
- 将状态以 5–10 Hz 发布给 Supervisor API；
- 监视实时倍率、步进耗时、超期和仿真发散。

### 5.4 ActuatorMapper

输入：

- `SERVO_OUTPUT_RAW` 的最终 PWM；
- Mission Planner 提供的 `SERVOx_FUNCTION/MIN/TRIM/MAX/REVERSED`；
- 可选模型方向覆盖参数。

输出：

```text
aileron  ∈ [-1, 1]
elevator ∈ [-1, 1]
rudder   ∈ [-1, 1]
throttle ∈ [0, 1]
```

职责：

- 按 `SERVOx_FUNCTION` 找到逻辑功能，不能永久硬编码通道；
- 按 MIN/TRIM/MAX 进行非对称归一化；
- 解回 `SERVOx_REVERSED`，恢复飞控逻辑控制量；
- 对异常 PWM、缺失功能和重复功能产生明确告警；
- 控制消息间使用零阶保持；
- 执行 100 ms 陈旧和 500 ms 失效安全策略。

### 5.5 SensorModel

输入：JSBSim 真值帧。  
输出：`HIL_IMU_COMPACT` 和 `HIL_NAV_COMPACT`。

职责：

- 坐标、单位和符号转换；
- 采样率、噪声、偏置、漂移、延迟和量化；
- 数值饱和并设置状态位；
- 每消息类型独立 `sample_seq`；
- 重置后设置 `SIM_RESET`；
- GPS 失锁时发送明确无效状态，不冻结成“看似有效”的旧位置。

### 5.6 TelemetryMirror

建议本机链路：

```text
Supervisor UDP 源端口 14551 → Mission Planner UDP 监听端口 14550
Mission Planner 回包 → Supervisor 14551 → 按目标路由至 Mesh
```

端口必须可配置。镜像只处理普通 MAVLink 遥测和指令；HIL 高频传感器默认不镜像到 Mission Planner，避免 UI 链路放大流量。

### 5.7 SupervisorAPI

仅监听回环地址，建议：

```text
HTTP/WebSocket: 127.0.0.1:8765
API 前缀:       /api/v1
状态流:         /ws/v1/status
```

HILPanel 只使用本地 API，不直接调用 Worker 对象。

### 5.8 SessionLogger

负责统一会话时钟和结构化日志。任何组件不得只把关键异常打印到控制台而不进入会话日志。

## 6. 关键数据流

### 6.1 飞控执行器到 JSBSim

```text
ArduPlane 控制器
  → SRV_Channel 混控和输出限制
  → SERVO_OUTPUT_RAW（最终 PWM）
  → Mesh
  → Supervisor 路由
  → ActuatorMapper
  → 最新控制快照
  → JSBSim 400 Hz 读取
```

JSBSim 每个物理步读取“截至本步最新的一组完整控制量”。接收线程不得逐字段修改 Worker 正在使用的对象，应使用不可变快照或单写单读交换。

### 6.2 JSBSim 到飞控传感器

```text
JSBSim 400 Hz 真值
  ├─ IMU sampler 每 4 步采样 → 100 Hz
  │    → 噪声/偏置/量化
  │    → HIL_IMU_COMPACT
  │
  └─ NAV sampler 每 40 步采样 → 10 Hz
       → GPS误差/失锁/量化
       → HIL_NAV_COMPACT

两类消息 → target_system → Mesh 节点 → ArduPlane HIL 后端 → EKF
```

采样应按仿真步计数，不能依赖 `time.sleep()` 是否准时。即使实时倍率暂时低于 1，仿真时间中的采样间隔仍必须精确。

### 6.3 Mission Planner 配置

```text
Mission Planner 参数缓存
  → HILPanel 读取 SERVO1..32_* 参数
  → POST /api/v1/vehicles/{sysid}/actuators
  → Supervisor 校验并生成映射
  → 返回解析结果和告警
```

如果 Mission Planner 尚未下载参数，HILPanel 应先请求所需参数；不得用默认 `1000/1500/2000` 静默覆盖真实配置。

## 7. 私有 MAVLink 方言

详细字段和单位见 [HIL_MAVLink_Protocol.md](./HIL_MAVLink_Protocol.md)。本设计使用：

| 消息 | 暂定 ID | 方向 | 频率 |
|---|---:|---|---:|
| `SERVO_OUTPUT_RAW` | 36 | 飞控 → Supervisor | 50–100 Hz |
| `HIL_IMU_COMPACT` | 42000 | Supervisor → 飞控 | 100 Hz |
| `HIL_NAV_COMPACT` | 42001 | Supervisor → 飞控 | 10 Hz |

### 7.1 方言唯一来源

建议仓库内建立：

```text
protocol/
  message_definitions/
    nuaa_hil.xml
  generated/
    python/nuaa_hil.py
    c/include/mavlink/v2.0/nuaa_hil/
  generate_mavlink.ps1
  PROTOCOL_VERSION
```

规则：

- XML 是唯一源文件；
- Python 与 C 绑定必须由脚本生成；
- 生成脚本固定 pymavlink/mavgen 版本；
- CI 比较重新生成结果，防止手改生成文件；
- Supervisor 启动日志记录 XML SHA-256 和生成器版本；
- 固件构建产物记录同一协议版本。

### 7.2 消息兼容性

v1 发布后：

- 已有字段不得改变类型、缩放或语义；
- 新增字段只能使用 MAVLink 2 extension，或新增消息版本；
- 消息 ID 不得复用；
- Supervisor 与固件协议版本不匹配时禁止开始 HIL，并显示明确错误。

## 8. ArduPlane 固件设计

### 8.1 需要修改的部分

1. 将 `nuaa_hil.xml` 纳入 ArduPilot MAVLink 生成流程；
2. 在 MAVLink 接收分派中处理两个自定义消息；
3. 新建或扩展 HIL 专用 IMU、Compass、Baro、Airspeed、GPS 后端；
4. 增加 HIL 启用参数和状态；
5. 增加超时、目标过滤、序号和饱和统计；
6. 通过 `STATUSTEXT` 或自定义状态向 Supervisor 报告 HIL 是否就绪。

### 8.2 建议固件参数

名称需按 ArduPilot 参数长度约束最终确认，语义建议如下：

| 参数 | 默认值 | 说明 |
|---|---:|---|
| `HIL_ENABLE` | 0 | 0 禁用；1 接受私有 HIL 输入 |
| `HIL_SRC_SYS` | 0 | 允许的 Supervisor SYSID；0 表示由首次握手锁定 |
| `HIL_IMU_TOUT` | 50 | IMU 超时毫秒 |
| `HIL_GPS_TOUT` | 500 | NAV 超时毫秒 |

正式命名必须检查与现有参数冲突。默认固件必须关闭 HIL，防止普通飞行时接收网络模拟传感器。

### 8.3 接收处理顺序

```text
收到 MAVLink 帧
  → 是否 MAVLink 2
  → 消息是否为私有 HIL
  → HIL_ENABLE 是否开启
  → 来源 SYSID 是否允许
  → target_system/component 是否匹配
  → 序号/时间是否新鲜
  → fields/flags 是否有效
  → 定点解码与范围检查
  → 写入对应传感器后端
  → 更新健康状态和统计
```

任何验证失败都必须先丢弃，再统计；不能“尽量使用”格式或目标不确定的传感器数据。

### 8.4 EKF 边界

- IMU 消息进入惯导传感器前端；
- 磁场进入 Compass 前端；
- 压力进入 Baro/Airspeed 前端；
- NAV 消息进入 GPS 前端；
- 不允许直接写 AHRS 姿态、EKF 位置或飞行控制目标；
- HIL 输入中断时相应传感器应变为不健康，由现有 EKF/failsafe 处理。

### 8.5 固件端测试

- 消息编解码单元测试；
- 错误目标不注入测试；
- 旧序号/乱序包拒绝测试；
- 数值边界和饱和测试；
- 超时变为不健康测试；
- `HIL_ENABLE=0` 完全忽略测试；
- 普通实机固件回归测试。

## 9. Supervisor 软件设计

### 9.1 目标目录

当前单文件骨架验证完成后，建议逐步拆分为：

```text
JSBSim/
  hil_supervisor.py               # CLI 和组件装配
  hil/
    config.py                     # YAML、环境变量、校验
    supervisor.py                 # 生命周期和车辆注册
    vehicle.py                    # VehicleContext/状态机
    jsbsim_worker.py              # FGFDMExec 与 400 Hz 调度
    actuator_mapper.py            # PWM/功能映射
    sensor_model.py               # IMU/NAV 生成
    mavlink_codec.py              # 私有方言编解码
    mesh_transport.py             # Mesh I/O
    telemetry_mirror.py           # Mission Planner UDP
    api.py                        # HTTP/WebSocket
    faults.py                     # 故障注入
    session_log.py                # 结构化日志
    metrics.py                    # 频率、延迟、丢包统计
  config/
    supervisor.yaml
    vehicle01.yaml
  tests/
    test_actuator_mapper.py
    test_sensor_units.py
    test_mavlink_roundtrip.py
    test_target_routing.py
    test_fixed_rate.py
    test_faults.py
```

### 9.2 线程/进程模型

v1：

- 主线程：生命周期和信号处理；
- I/O 事件循环：Mesh、Mission Planner UDP、HTTP/WebSocket；
- Worker 线程：JSBSim 固定步长；
- 日志线程：批量落盘。

v2：

- Supervisor 主进程处理所有网络和路由；
- 每架飞机一个 Worker 子进程；
- Supervisor 与 Worker 使用有界 IPC 队列；
- 控制队列采用 latest-wins，不积压旧 PWM；
- 传感器队列满时丢弃旧帧并产生严重告警，禁止无限增长内存。

### 9.3 VehicleContext

每架飞机独立保存：

```text
sysid / compid / mesh_node
model / spawn / environment
actuator calibration and mapping
latest actuator snapshot and receive time
JSBSim truth state
IMU/NAV sequence counters
link and timing metrics
fault configuration
session logger handles
vehicle state machine
```

不同飞机的数据不得通过全局“当前 MAV”变量共享。

## 10. 生命周期状态机

```text
STOPPED
  │ load config
  ▼
CONFIGURED
  │ connect Mesh + detect vehicle + verify protocol
  ▼
READY
  │ start
  ▼
RUNNING ◀──── resume ──── PAUSED
  │  │                     ▲
  │  └──── pause ──────────┘
  │
  ├─ reset → READY → RUNNING（新 epoch，SIM_RESET）
  ├─ recoverable error → DEGRADED
  ├─ fatal error → FAULTED
  └─ stop → STOPPING → STOPPED
```

### 10.1 各状态允许操作

| 状态 | 允许操作 | HIL 传感器发送 |
|---|---|---|
| `STOPPED` | 配置、打开历史日志 | 否 |
| `CONFIGURED` | 校验、连接 | 否 |
| `READY` | 启动、重新配置 | 否 |
| `RUNNING` | 暂停、重置、停止、故障注入 | 是 |
| `PAUSED` | 恢复、单步、重置、停止 | 发送暂停状态，不发送新样本 |
| `DEGRADED` | 恢复、停止 | 视故障类型决定 |
| `FAULTED` | 保存日志、重置、停止 | 否 |

## 11. 配置设计

建议 `supervisor.yaml`：

```yaml
protocol:
  dialect: nuaa_hil
  version: 1
  sender_sysid: 245
  sender_compid: 240
  mavlink2_signing: false

mesh:
  transport: udp
  bind_address: 192.168.10.2
  bind_port: 14600
  node_timeout_ms: 1000

mission_planner:
  enabled: true
  target_host: 127.0.0.1
  target_port: 14550
  local_port: 14551
  mirror_hil_messages: false

api:
  host: 127.0.0.1
  port: 8765
  status_hz: 10

logging:
  root: sessions
  raw_mavlink: true
  truth_hz: 100
  flush_interval_ms: 200

safety:
  control_stale_ms: 100
  control_lost_ms: 500
  control_lost_action: neutral_and_idle
  imu_timeout_ms: 50
  nav_timeout_ms: 500
```

`vehicle01.yaml`：

```yaml
vehicle:
  sysid: 1
  component_id: 1
  mesh_node: aircraft-01
  model: c172x

spawn:
  latitude_deg: -34.98106
  longitude_deg: 117.85201
  altitude_msl_m: 140.0
  terrain_elevation_m: 40.0
  heading_deg: 0.0
  airspeed_mps: 30.0

simulation:
  physics_hz: 400
  imu_hz: 100
  nav_hz: 10
  status_hz: 10
  realtime: true

actuators:
  source: mission_planner
  fallback_allowed: false

faults:
  enabled: false
```

示例 IP 和端口仅用于设计，接入真实 Mesh 前必须替换为现场网络规划。

## 12. 本地 API

### 12.1 REST

| 方法 | 路径 | 作用 |
|---|---|---|
| `GET` | `/api/v1/health` | Supervisor 总体健康 |
| `GET` | `/api/v1/vehicles` | 所有车辆摘要 |
| `GET` | `/api/v1/vehicles/{sysid}` | 单机完整状态 |
| `PUT` | `/api/v1/vehicles/{sysid}/config` | 更新停止状态下的配置 |
| `PUT` | `/api/v1/vehicles/{sysid}/actuators` | 下发 SERVO 标定和功能映射 |
| `POST` | `/api/v1/vehicles/{sysid}/start` | 启动 |
| `POST` | `/api/v1/vehicles/{sysid}/pause` | 暂停 |
| `POST` | `/api/v1/vehicles/{sysid}/resume` | 恢复 |
| `POST` | `/api/v1/vehicles/{sysid}/reset` | 重置并产生新 epoch |
| `POST` | `/api/v1/vehicles/{sysid}/stop` | 停止 |
| `PUT` | `/api/v1/vehicles/{sysid}/faults` | 更新故障注入 |
| `GET` | `/api/v1/sessions/{id}/logs` | 返回日志目录和文件清单 |

所有改变状态的请求返回：

```json
{
  "accepted": true,
  "vehicle_sysid": 1,
  "previous_state": "READY",
  "current_state": "RUNNING",
  "operation_id": "...",
  "message": "started"
}
```

### 12.2 WebSocket 状态

`/ws/v1/status` 以 5–10 Hz 发布：

```json
{
  "type": "vehicle_status",
  "sysid": 1,
  "state": "RUNNING",
  "mesh_connected": true,
  "control_hz": 50.1,
  "imu_tx_hz": 100.0,
  "nav_tx_hz": 10.0,
  "control_age_ms": 8.2,
  "link_rtt_ms": 23.4,
  "packet_loss_pct": 0.2,
  "realtime_factor": 0.999,
  "ekf_ok": true,
  "truth": {},
  "estimate": {},
  "warnings": []
}
```

大体积日志和 100 Hz 原始数组不得通过 UI WebSocket 持续发送。

## 13. Mission Planner HILPanel

### 13.1 文件结构

```text
MissionPlanner/
  Controls/
    HILPanel.cs
    HILPanel.Designer.cs
  HIL/
    HilSupervisorClient.cs
    HilVehicleStatus.cs
    HilActuatorConfig.cs
```

实际工程路径对应当前仓库的 `Controls/` 和新建 `HIL/` 目录。

### 13.2 页面区域

| 区域 | 内容 |
|---|---|
| 顶部配置 | SYSID、Mesh 节点、模型、出生点、仿真频率 |
| 控制区 | 校验、启动、暂停、恢复、停止、重置、打开日志 |
| 状态表 | Mesh、控制频率、IMU/NAV 频率、延迟、丢包、EKF、实时倍率 |
| 地图区 | 飞控估计轨迹与 JSBSim 真值轨迹双颜色显示 |
| 执行器区 | 自动识别通道、MIN/TRIM/MAX/REVERSED、实时 PWM/归一值 |
| 故障区 | IMU/GPS/气压/链路故障配置 |
| 日志区 | Worker 输出、STATUSTEXT、Supervisor 告警 |

### 13.3 参数读取

遍历 `SERVO1` 至飞控支持的最大通道，读取：

```text
SERVOx_FUNCTION
SERVOx_MIN
SERVOx_TRIM
SERVOx_MAX
SERVOx_REVERSED
```

读取后先在 UI 显示解析结果，确认 Aileron/Elevator/Throttle/Rudder 均唯一且有效，再允许启动。缺少参数时按钮保持禁用并给出具体缺失项。

当前分支连接函数默认可能不下载参数，因此 HILPanel 必须检测缓存是否完整，并主动请求缺失参数。

### 13.4 UI 线程规则

- HTTP/WebSocket 使用异步客户端；
- 收到状态后写入线程安全的最新快照；
- UI Timer 以 5–10 Hz 读取快照并刷新；
- 不在 UI 线程解析 Mesh、运行 pymavlink 或处理 100 Hz 数据；
- 地图轨迹设置点数上限和抽样，防止长时间运行内存增长。

## 14. 传感器模型

### 14.1 IMU

真值来源：JSBSim 机体系比力和角速度。处理顺序：

```text
真值
 → 坐标/单位转换
 → 安装角
 → 比例误差
 → 固定偏置
 → 温度漂移
 → 白噪声/随机游走
 → 可选振动
 → 延迟队列
 → 定点量化与饱和
```

第一阶段只启用坐标转换和量化，确认飞控方向正确；第二阶段再逐项启用噪声，避免方向错误被随机噪声掩盖。

### 14.2 磁罗盘

- 根据经纬度/高度和日期生成地磁 NED 矢量，或使用可配置固定地磁；
- 转换到机体系；
- 增加硬铁、软铁、电机电流干扰、噪声和延迟；
- v1 可先使用 JSBSim/地磁库真值加白噪声。

### 14.3 气压计和空速

- 高度与大气模型转换为绝对压力；
- JSBSim 动压转换为差压；
- 支持偏置、漂移、噪声、冻结、突变和延迟；
- 单位在进入消息前统一为 Pa；
- 必须验证 `diff_pressure_pa int16` 是否覆盖目标最大空速。

### 14.4 GPS

- 从 JSBSim 经纬度、MSL 高度和 NED 速度生成；
- 10 Hz 采样；
- 支持锁定时间、位置噪声、速度噪声、延迟和失锁；
- GPS 丢失时发送 `fix_type=0` 并清除有效位；
- 不将飞控 EKF 估计重新作为 GPS 输入，防止闭环自证。

## 15. 故障注入

### 15.1 故障参数模型

每个故障包含：

```text
enabled
start_sim_time_s
duration_s
mode
magnitude
seed
```

所有随机故障必须记录 seed，以便复现。

### 15.2 v1 故障

| 类别 | 模式 |
|---|---|
| IMU | 固定偏置、白噪声放大、冻结、丢样 |
| GPS | 失锁、位置阶跃、延迟、低更新率 |
| Baro | 偏置、漂移、冻结、压力突变 |
| Airspeed | 堵塞、比例误差、噪声 |
| Link | 延迟、抖动、随机丢包、突发丢包、重复、乱序 |

故障必须作用在正确层次。例如 GPS 失锁修改 `fix_type/flags`，而不是简单停止所有 HIL 消息。

## 16. 日志与可追溯性

### 16.1 会话目录

```text
sessions/2026-08-18T03-30-00_sysid1/
  manifest.json
  supervisor.jsonl
  mesh_rx.mavraw
  mesh_tx.mavraw
  actuator.csv
  truth.csv
  imu.csv
  nav.csv
  metrics.csv
  faults.jsonl
  worker.stderr.log
```

### 16.2 manifest 必须包含

- 会话 UUID、开始/结束时间和停止原因；
- Supervisor Git commit；
- JSBSim 版本和模型文件哈希；
- ArduPlane版本、Git hash、板卡类型；
- MAVLink XML 哈希和协议版本；
- 完整配置快照；
- `SERVOx_*` 参数快照；
- Mesh 节点/SYSID 映射；
- 随机故障 seed。

### 16.3 时间戳

同时保存：

- `sim_time_usec`：仿真时序和传感器采样；
- `monotonic_rx_ns`：本机到达时间和延迟统计；
- `wall_time_utc`：跨系统日志对齐。

业务逻辑不得使用可跳变的系统墙钟计算实时步长。

## 17. 性能和验收指标

以下为 v1 初始指标，Mesh 实测后可以调整，但调整必须记录理由。

| 指标 | 验收值 |
|---|---:|
| JSBSim 仿真步长 | 精确 0.0025 s |
| 10 分钟平均实时倍率 | 0.98–1.02 |
| Worker 调度迟到 p99 | ≤ 5 ms |
| IMU 仿真时间频率 | 100 Hz ± 0.1 Hz |
| NAV 仿真时间频率 | 10 Hz ± 0.05 Hz |
| 控制数据年龄 p99 | ≤ 50 ms（待 Mesh 实测确认） |
| 单机连续运行 | ≥ 30 min 无崩溃/无队列增长 |
| 目标隔离 | 10 万帧测试 0 次跨机注入 |
| 重置 | 2 s 内重新进入 RUNNING，首帧含 SIM_RESET |
| UI 更新 | 5–10 Hz，关闭/拖动窗口不影响 Worker |
| 日志完整性 | 会话结束后 manifest 和所有已启用流可读取 |

不要求 Windows `sleep` 每 2.5 ms 都绝对准时；要求仿真时间固定步进、无无限追赶、平均实时倍率满足指标，并对迟到进行统计。

## 18. 测试方案

### 18.1 单元测试

- PWM 在 min/trim/max 和非对称范围的归一化；
- `REVERSED` 的反算；
- 无效 PWM、零分母和缺失参数；
- SI 单位到定点消息的边界、负值和饱和；
- 经纬度、高度、NED 速度和坐标方向；
- `sample_seq` 回绕；
- target 路由和错误目标丢弃；
- 配置 schema 和非法值拒绝。

### 18.2 协议测试

- XML 经 mavgen 生成 Python/C；
- Python 打包 → C 解包逐字段相等；
- C 打包 → Python 解包逐字段相等；
- CRC extra 一致；
- MAVLink 1 明确拒绝；
- 签名关闭/开启场景按配置运行；
- Wireshark/pymavlink 能识别两条消息。

### 18.3 软件闭环测试

在没有真实飞控时建立执行器发生器：

```text
固定 PWM/阶跃/正弦扫频
  → ActuatorMapper
  → JSBSim
  → HIL 消息
  → 本地解码器
```

检查舵面符号、姿态响应、频率、时间戳和量化误差。

### 18.4 固件台架测试

1. 飞控不上桨、不接真实舵机或断开动力；
2. `HIL_ENABLE=0`，确认消息不影响传感器；
3. 开启 HIL，静止真值下检查 IMU、姿态和高度；
4. 分别施加副翼、升降舵、方向舵、油门阶跃；
5. 检查飞控输出方向和 JSBSim 响应；
6. 注入 GPS 丢失、IMU 偏置和链路超时；
7. 检查 EKF/failsafe 行为和恢复；
8. 完成 30 分钟闭环运行。

### 18.5 多机测试

- 三个 SYSID、三个 Mesh 节点、三个 Worker；
- 对每架飞机发送不同传感器模式；
- 验证命令、执行器和 HIL 数据不串机；
- 单独停止/重置一架，其他两架继续；
- 模拟一节点掉线和重新加入；
- 监视 CPU、内存、网络和日志吞吐。

## 19. 分阶段实施计划

### 阶段 0：基线动力学骨架——已完成

成果：

- `hil_supervisor.py` 可加载 `c172x`；
- 400 Hz 固定步长；
- 实时/非实时运行；
- 5 Hz JSON 真值状态；
- YAML 基础配置。

退出条件：已通过本机冒烟测试。

### 阶段 1A：私有 MAVLink 方言

任务：

1. 评审并冻结协议文档字段、缩放和消息 ID；
2. 建立 `nuaa_hil.xml`；
3. 编写可重复的 `generate_mavlink.ps1`；
4. 生成 pymavlink MAVLink 2 方言；
5. 生成 ArduPilot C/C++ 头文件；
6. 编写 Python↔C 编解码互操作测试；
7. 记录协议版本和 XML 哈希。

退出条件：同一测试向量在 Python/C 两端逐字段一致，消息 ID 无冲突。

### 阶段 1B：ArduPlane 固件接收端

任务：

1. 将方言加入目标 ArduPilot 源码；
2. 增加 HIL 启用和来源限制参数；
3. 实现两个消息处理器；
4. 接入虚拟 IMU/Compass/Baro/Airspeed/GPS 后端；
5. 实现目标、时间、序号、范围和超时检查；
6. 增加固件单元测试和 STATUSTEXT 状态；
7. 为目标飞控板构建并台架刷写。

退出条件：本地 pymavlink 发送测试消息时，飞控能读取对应虚拟传感器；错误目标和禁用状态下完全不注入。

### 阶段 1C：执行器接收与映射

任务：

1. Supervisor 接收 `SERVO_OUTPUT_RAW`；
2. 实现 ActuatorMapper；
3. 支持 HILPanel/测试 JSON 下发 `SERVOx_*`；
4. 将归一值写入 JSBSim；
5. 启动 c172x 发动机并确认油门有效；
6. 实现控制超时安全策略；
7. 完成四通道阶跃和符号测试。

退出条件：副翼、升降舵、方向舵和油门逐项动作时，JSBSim 响应方向正确，其他控制面无非预期跳变。

### 阶段 2：SensorModel 与本机软件闭环

任务：

1. JSBSim 真值统一为 SI/NED/机体系；
2. 生成 100 Hz IMU 与 10 Hz NAV；
3. 实现量化、饱和、序号、reset；
4. 本机 UDP 环回发送并用 pymavlink 解码；
5. 添加频率、延迟和丢包统计；
6. 先无噪声验方向，再加入基础噪声。

退出条件：10 分钟频率和实时倍率达标，消息可被定制固件持续接收。

### 阶段 3：Mesh 单机闭环

任务：

1. 明确 Mesh 传输方式、IP、MTU 和端口；
2. 实现 MeshTransport；
3. 建立 Mesh 节点与 SYSID 绑定；
4. 连接真实飞控；
5. 测量单向延迟、RTT、丢包和最大稳定频率；
6. 调整频率或量化但不改变已冻结字段语义；
7. 完成 30 分钟单机闭环。

退出条件：闭环稳定、目标正确、超时安全策略有效、日志完整。

### 阶段 4：Mission Planner HILPanel

任务：

1. 实现 Supervisor REST/WebSocket API；
2. 新建 HILPanel 和客户端数据结构；
3. 自动读取并校验 `SERVOx_*`；
4. 启停和状态表；
5. JSBSim 真值与飞控估计双轨迹；
6. 日志和异常区域；
7. 验证 UI 卡顿不影响闭环。

退出条件：无需命令行即可配置和管理单机 HIL，且关闭 HILPanel 不终止 Supervisor。

### 阶段 5：故障注入和可复现日志

任务：

1. 实现故障配置模型和确定性随机数；
2. 实现 IMU/GPS/Baro/Airspeed/Link 故障；
3. UI 故障控制；
4. manifest、事件和数据流日志；
5. 完成故障恢复测试。

退出条件：相同配置和 seed 可复现相同故障时间线。

### 阶段 6：多机

任务：

1. Worker 子进程化；
2. VehicleRegistry 和每机状态机；
3. 严格目标路由；
4. 多机 UI；
5. 三机 30 分钟压力测试；
6. 单机故障隔离和恢复。

退出条件：三机无串流、无队列增长，任一 Worker 重启不影响其他飞机。

## 20. 依赖关系和禁止反序

```text
协议冻结
  ├─▶ pymavlink/C 绑定
  │      ├─▶ Supervisor 编解码
  │      └─▶ ArduPlane 固件处理
  │
  └─▶ Python/C 互操作测试

执行器映射 + SensorModel + 固件处理
  └─▶ 本机闭环
        └─▶ Mesh 单机闭环
              └─▶ Mission Planner 完整 UI
                    └─▶ 故障注入
                          └─▶ 多机
```

禁止在协议/固件/本机闭环未稳定时先做完整 UI 或多机，否则故障难以定位。

## 21. 风险与缓解

| 风险 | 影响 | 缓解措施 |
|---|---|---|
| ArduPlane 传感器后端不适合运行时外部注入 | 固件改造量增大 | 先做最小原型，逐类接入并保留 HIL_ENABLE 开关 |
| 100 Hz 通过 Mesh 不稳定 | EKF 传感器超时 | 实测 MTU/带宽；紧凑定点；优先 IMU；有界队列和丢包统计 |
| Windows 400 Hz 调度抖动 | 实时倍率波动 | 固定仿真步长；迟到统计；避免 UI/日志同步写；必要时 Worker 提升优先级或迁移 Linux |
| 舵面符号错误 | 闭环立即发散 | 无噪声单通道阶跃；读取 SERVOx_*；启动前显示映射并人工确认 |
| `SERVO_OUTPUT_RAW` 频率不足 | 控制延迟 | 请求消息间隔；实测；必要时增加专用执行器消息 |
| 消息 XML/固件/Python 不一致 | 无法解码或静默错误 | 单一 XML、脚本生成、协议 hash、Python↔C 测试 |
| 多机目标串流 | 严重安全问题 | target_system + Mesh 节点双重绑定；负向测试；默认拒绝广播 |
| 日志阻塞实时线程 | 丢步 | 专用日志队列/线程、批量写、有界降级策略 |
| UI 意外停止仿真 | 会话不可控 | Supervisor 独立进程；UI 关闭只断开监视，不发送 stop |
| c172x 模型与目标飞机差异大 | 控制参数不适配 | 先验证链路；随后建立目标机型 JSBSim 模型和参数辨识流程 |

## 22. 开始执行前需要确认的外部信息

1. ArduPilot 源码仓库路径、目标分支/commit；
2. 飞控板卡型号和固件构建命令；
3. 飞控与天空端 Mesh 的串口号、波特率和 `SERIALn_PROTOCOL`；
4. 地面 Mesh 基站的传输形式：UDP、TCP、虚拟串口或厂商 SDK；
5. Mesh IP/端口、MTU、典型带宽、延迟和丢包；
6. 是否要求 MAVLink 2 签名；
7. 目标飞机是常规尾翼还是混控机型；
8. 实际 `SERVOx_FUNCTION/MIN/TRIM/MAX/REVERSED`；
9. 最大设计空速、过载和角速度，用于确认紧凑字段范围；
10. 是否需要在第一版支持多 IMU/双 GPS。

缺少第 1–5 项时仍可完成本机软件闭环，但不能完成真实飞控 Mesh 闭环。

## 23. 下一步执行清单

当前应从阶段 1A 开始：

- [ ] 评审 `HIL_MAVLink_Protocol.md` 的字段和缩放；
- [ ] 确认消息 ID `42000/42001`；
- [ ] 确认 IMU 100 Hz、NAV 10 Hz；
- [ ] 确认目标最大空速是否适合 `int16 diff_pressure_pa`；
- [ ] 创建 `protocol/message_definitions/nuaa_hil.xml`；
- [ ] 创建可重复的生成脚本；
- [ ] 生成 Python/C MAVLink 2 绑定；
- [ ] 完成 Python↔C 编解码互操作测试；
- [ ] 提供 ArduPilot 源码路径和飞控板卡信息；
- [ ] 再进入固件消息接收端实现。

每完成一个阶段，应把验收证据、测试命令和结果追加到独立的 `HIL_Validation_Record.md`，不要只在聊天或控制台中保留结果。

