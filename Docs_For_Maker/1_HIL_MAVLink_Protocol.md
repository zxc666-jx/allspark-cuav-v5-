# HIL Supervisor 私有 MAVLink 协议

状态：Draft v0.1  
日期：2026-08-18  
适用范围：ArduPlane、HIL Supervisor、JSBSim、Mesh 链路  
传输版本：仅 MAVLink 2  
配套设计：[HIL_System_Design_and_Implementation_Plan.md](./HIL_System_Design_and_Implementation_Plan.md)

> ⚠️ **状态更新（2026-08-21）**：主 HIL 闭环已切换为**串口二进制协议**（专用 USB-to-TTL，对接飞控 `AP_ExternalAHRS_JSBSim`），见 [4_HIL_Serial_Protocol.md](./4_HIL_Serial_Protocol.md)。本文档的 MAVLink 路径（自定义 `HIL_IMU_COMPACT`/`HIL_NAV_COMPACT`）降级为**备用 / 开环监视**链路。已实测确认：标准 `HIL_SENSOR` ArduPlane 不接收；`GPS_INPUT` 经 `AP_GPS_MAV` 可接收。

## 1. 目的

本协议用于真实 ArduPlane 飞控与地面 HIL Supervisor 之间的硬件在环闭环：

```text
ArduPlane 最终执行器 PWM
        │ SERVO_OUTPUT_RAW
        ▼
HIL Supervisor ──控制量──▶ JSBSim（400 Hz）
        ▲                         │
        │ HIL_IMU_COMPACT 100 Hz  │ 真值状态
        │ HIL_NAV_COMPACT 10 Hz   │
        └─────────────────────────┘
```

设计目标：

- Mesh 只由 HIL Supervisor 直接连接和路由；
- 单条 Mesh 链路可承载多架飞机，消息必须具有明确目标；
- 传感器数据采用定点量化，降低 100 Hz IMU 流量；
- 飞控 EKF 使用模拟传感器自行估计状态，不直接注入 JSBSim 真值姿态；
- Mission Planner 只负责参数读取、配置、监视和遥测镜像，不进入 100 Hz 实时闭环。

## 2. MAVLink 身份和路由

### 2.1 发送端身份

Supervisor 的 MAVLink `sysid/compid` 必须可配置，并且不能与飞控或 Mission Planner 冲突。飞控继续使用自身的 `MAV_SYSID`，自动驾驶仪组件通常为 `MAV_COMP_ID_AUTOPILOT1`。

MAVLink 包头中的 `sysid/compid` 表示发送者；消息载荷中的目标字段表示接收者，两者不能混淆。

### 2.2 目标字段命名

虽然项目最初使用了 `target_sysid` 这一说法，XML 中正式采用 MAVLink 标准字段名：

```text
target_system
target_component
```

这样 MAVLink Router、pymavlink 和后续工具能够识别消息的目标语义。`target_sysid` 不作为正式字段名。

接收规则：

- `target_system` 必须等于本机 `MAV_SYSID`；
- `target_component` 为 `0` 或本机组件 ID 时可接收；
- 不匹配的消息必须丢弃，不能注入本机传感器；
- 正式 HIL 运行不建议使用 `target_system=0` 广播传感器数据。

## 3. 消息总表

| 方向 | 消息 | ID | 建议频率 | 说明 |
|---|---|---:|---:|---|
| 飞控 → Supervisor | `SERVO_OUTPUT_RAW` | 36 | 50–100 Hz | 标准消息，飞控最终 PWM 输出 |
| Supervisor → 飞控 | `HIL_IMU_COMPACT` | 42000（暂定） | 100 Hz | IMU、磁场、气压、空速管数据 |
| Supervisor → 飞控 | `HIL_NAV_COMPACT` | 42001（暂定） | 5–10 Hz | GPS 位置、速度和精度数据 |

`42000/42001` 已在当前工程和已安装 pymavlink 方言中做过本地冲突搜索，未发现占用，但在生成正式绑定前仍需对最终使用的完整方言集合做一次冲突检查。消息 ID 一旦部署到固件后不得随意修改。

## 4. HIL_IMU_COMPACT

### 4.1 语义

`HIL_IMU_COMPACT` 表示一个采样时刻的虚拟惯性与空气数据。所有量都来自同一个 JSBSim 真值帧，再叠加 Supervisor 配置的偏置、噪声、延迟和故障。

坐标系采用 MAVLink/ArduPilot 机体系：

```text
X：机头向前
Y：机体向右
Z：机体向下
```

### 4.2 字段

| 字段 | 类型 | 单位/缩放 | 有效范围或说明 |
|---|---|---|---|
| `time_usec` | `uint64_t` | µs | Supervisor 单调仿真时间，不是 Unix 时间 |
| `abs_pressure_pa` | `uint32_t` | Pa | 绝对静压 |
| `pressure_alt_cm` | `int32_t` | cm | 气压高度，海平面以上为正 |
| `xacc_mg` | `int16_t` | milli-g | X 轴比力 |
| `yacc_mg` | `int16_t` | milli-g | Y 轴比力 |
| `zacc_mg` | `int16_t` | milli-g | Z 轴比力 |
| `xgyro_mrad_s` | `int16_t` | mrad/s | X 轴角速度 |
| `ygyro_mrad_s` | `int16_t` | mrad/s | Y 轴角速度 |
| `zgyro_mrad_s` | `int16_t` | mrad/s | Z 轴角速度 |
| `xmag_mgauss` | `int16_t` | milli-Gauss | X 轴磁场 |
| `ymag_mgauss` | `int16_t` | milli-Gauss | Y 轴磁场 |
| `zmag_mgauss` | `int16_t` | milli-Gauss | Z 轴磁场 |
| `diff_pressure_pa` | `int16_t` | Pa | 空速管差压，允许负值 |
| `temperature_cdeg` | `int16_t` | 0.01 °C | 传感器温度 |
| `sample_seq` | `uint16_t` | 计数 | 每个目标系统独立递增，回绕允许 |
| `fields_updated` | `uint16_t` | 位掩码 | 表示本帧哪些数据有效 |
| `target_system` | `uint8_t` | ID | 目标飞控 SYSID |
| `target_component` | `uint8_t` | ID | 目标组件，通常为 1 |
| `sensor_id` | `uint8_t` | ID | 虚拟 IMU 实例，第一套为 0 |

量化公式：

```text
accel_mg       = round(accel_m_s2 / 9.80665 × 1000)
gyro_mrad_s    = round(gyro_rad_s × 1000)
mag_mgauss     = round(mag_gauss × 1000)
pressure_alt_cm = round(pressure_alt_m × 100)
temperature_cdeg = round(temperature_degC × 100)
```

超出整数范围时发送端必须饱和，不允许整数回绕。

### 4.3 fields_updated

| Bit | 名称 | 含义 |
|---:|---|---|
| 0 | `ACCEL_VALID` | 三轴加速度有效 |
| 1 | `GYRO_VALID` | 三轴角速度有效 |
| 2 | `MAG_VALID` | 三轴磁场有效 |
| 3 | `ABS_PRESSURE_VALID` | 绝对静压有效 |
| 4 | `DIFF_PRESSURE_VALID` | 差压有效 |
| 5 | `PRESSURE_ALT_VALID` | 气压高度有效 |
| 6 | `TEMPERATURE_VALID` | 温度有效 |
| 14 | `SIM_RESET` | 本帧是重置后的首帧 |
| 15 | `DATA_SATURATED` | 至少一个字段发生数值饱和 |

## 5. HIL_NAV_COMPACT

### 5.1 语义

`HIL_NAV_COMPACT` 模拟 GPS/GNSS 接收机输出。它不是飞控融合后的导航结果，也不携带 JSBSim 真值姿态。飞控应将其送入 GPS 后端，再由 EKF 完成状态估计。

地球坐标使用 NED：北、东、下。

### 5.2 字段

| 字段 | 类型 | 单位/缩放 | 有效范围或说明 |
|---|---|---|---|
| `time_usec` | `uint64_t` | µs | 与 IMU 使用同一单调仿真时钟 |
| `lat` | `int32_t` | degE7 | 纬度 × 1e7 |
| `lon` | `int32_t` | degE7 | 经度 × 1e7 |
| `alt_msl_mm` | `int32_t` | mm | 海拔高度 |
| `vel_n_cms` | `int16_t` | cm/s | 北向速度 |
| `vel_e_cms` | `int16_t` | cm/s | 东向速度 |
| `vel_d_cms` | `int16_t` | cm/s | 向下速度 |
| `cog_cdeg` | `uint16_t` | 0.01 deg | 地面航迹角，0–35999 |
| `eph_cm` | `uint16_t` | cm | 水平位置精度估计 |
| `epv_cm` | `uint16_t` | cm | 垂直位置精度估计 |
| `vel_acc_cms` | `uint16_t` | cm/s | 速度精度估计 |
| `yaw_cdeg` | `uint16_t` | 0.01 deg | GPS 航向；不可用时为 `UINT16_MAX` |
| `sample_seq` | `uint16_t` | 计数 | 每个目标系统独立递增 |
| `flags` | `uint16_t` | 位掩码 | 导航数据有效性和重置状态 |
| `target_system` | `uint8_t` | ID | 目标飞控 SYSID |
| `target_component` | `uint8_t` | ID | 目标组件，通常为 1 |
| `fix_type` | `uint8_t` | GPS fix | 采用 MAVLink `GPS_FIX_TYPE` 数值 |
| `satellites_visible` | `uint8_t` | 数量 | 可见卫星数；未知为 255 |

### 5.3 flags

| Bit | 名称 | 含义 |
|---:|---|---|
| 0 | `POSITION_VALID` | 经纬度和海拔有效 |
| 1 | `VELOCITY_VALID` | NED 速度有效 |
| 2 | `COG_VALID` | 地面航迹角有效 |
| 3 | `YAW_VALID` | GPS 航向有效 |
| 4 | `ACCURACY_VALID` | eph、epv、速度精度有效 |
| 14 | `SIM_RESET` | 本帧是重置后的首帧 |
| 15 | `DATA_SATURATED` | 至少一个字段发生数值饱和 |

GPS 丢失时 Supervisor 不伪造有效位置。推荐继续低频发送 `fix_type=0` 且清除有效位，使飞控明确进入无定位状态。

## 6. 执行器输入协议

### 6.1 数据源

第一阶段不定义新的舵面消息，直接使用标准 `SERVO_OUTPUT_RAW`。必须使用飞控最终执行器输出，不能使用 `RC_CHANNELS` 或遥控器摇杆值。

Supervisor 在两个舵面消息之间保持上一组控制量，JSBSim 仍以 400 Hz 固定步长运行。

### 6.2 Mission Planner 提供的标定参数

HILPanel 从 Mission Planner 当前飞控参数缓存读取每个输出通道的：

```text
SERVOx_FUNCTION
SERVOx_MIN
SERVOx_TRIM
SERVOx_MAX
SERVOx_REVERSED
```

HILPanel 通过 Supervisor 本地配置 API 下发这些参数。Supervisor 不依赖手工重复填写的 YAML；YAML 仅作为无 Mission Planner 时的覆盖或测试配置。

### 6.3 普通舵面 PWM 反算

对副翼、升降舵和方向舵：

```text
if pwm >= trim:
    normalized = (pwm - trim) / (max - trim)
else:
    normalized = (pwm - trim) / (trim - min)

normalized = clamp(normalized, -1, 1)

if SERVOx_REVERSED:
    normalized = -normalized
```

恢复出的逻辑控制量写入：

```text
fcs/aileron-cmd-norm  ∈ [-1, 1]
fcs/elevator-cmd-norm ∈ [-1, 1]
fcs/rudder-cmd-norm   ∈ [-1, 1]
```

由于 `SERVO_OUTPUT_RAW` 是应用输出反向后的最终 PWM，反算飞控逻辑控制量时需要解回 `SERVOx_REVERSED`。如果未来改为模拟真实舵机连杆，则应使用物理方向模型，不能再次解回反向。

### 6.4 油门 PWM 反算

普通单向油门：

```text
normalized = clamp((pwm - min) / (max - min), 0, 1)

if SERVOx_REVERSED:
    normalized = 1 - normalized
```

写入：

```text
fcs/throttle-cmd-norm ∈ [0, 1]
```

反推、双发和差动推力不属于 v0.1 范围，需要后续扩展执行器模型。

### 6.5 通道映射

不能永久硬编码 `SERVO1/2/3/4`。Supervisor 根据 `SERVOx_FUNCTION` 查找 Aileron、Elevator、Throttle、Rudder。v0.1 首先支持常规尾翼固定翼；Elevon、V-tail、差动扰流板和多舵机平均必须单独定义解混或直接映射到相应 JSBSim 控制面。

### 6.6 无效值与超时

- PWM 为 0、`UINT16_MAX` 或超出合理范围时视为无效；
- 单个错误包不立即归零，保持上一有效控制量并累计错误；
- 超过 100 ms 未收到有效执行器数据时标记 `CONTROL_STALE`；
- 超过 500 ms 时进入可配置安全策略：保持、舵面中立并怠速，或暂停仿真；
- 默认安全策略建议为“舵面中立、油门归零、仿真继续”，同时产生高优先级日志。

## 7. 时序要求

| 项目 | 要求 |
|---|---|
| JSBSim 物理步长 | 400 Hz，`dt=0.0025 s` |
| 执行器接收 | 建议 50–100 Hz，步间零阶保持 |
| IMU 输出 | 100 Hz |
| NAV/GPS 输出 | 默认 10 Hz，可配置 5–20 Hz |
| Mission Planner UI | 5–10 Hz，不进入实时链路 |

IMU 和 NAV 的 `time_usec` 必须来自同一个 Supervisor 仿真时钟。实时倍率改变时，采样周期按仿真时间推进，而不是按 UI 或网络到达时间推进。

## 8. 丢包、乱序和重置

- `sample_seq` 按消息类型、目标系统和传感器实例分别递增；
- 接收端根据模 65536 差值识别丢包和旧包；
- 旧包不得覆盖更新的传感器样本；
- Supervisor 重置 JSBSim 后，时间允许回到零，但首帧必须设置 `SIM_RESET`；
- 飞控收到 `SIM_RESET` 后应清理对应 HIL 采样缓存，避免跨重置计算错误时间差；
- MAVLink 本身不保证送达，IMU 和 NAV 不进行逐包 ACK 或重传；
- 配置、启动、停止等低频控制命令应使用另外的可靠请求/应答机制。

## 9. 带宽估算

按当前字段设计：

- `HIL_IMU_COMPACT` 载荷约 45 字节；MAVLink 2 未签名帧约 57 字节，100 Hz 约 5.7 kB/s/机；
- `HIL_NAV_COMPACT` 载荷约 44 字节；MAVLink 2 未签名帧约 56 字节，10 Hz 约 0.56 kB/s/机；
- MAVLink 2 签名会为每帧增加 13 字节；
- 估算未包含 Mesh、UDP、IP 或串口封装开销。

## 10. XML 草案

以下内容是后续 `nuaa_hil.xml` 的基准。生成绑定前必须通过 mavgen 校验和完整 ID 冲突检查。

```xml
<?xml version="1.0"?>
<mavlink>
  <include>ardupilotmega.xml</include>
  <dialect>0</dialect>
  <messages>
    <message id="42000" name="HIL_IMU_COMPACT">
      <description>Targeted compact simulated IMU and air-data sample.</description>
      <field type="uint64_t" name="time_usec">Monotonic simulation time in microseconds.</field>
      <field type="uint32_t" name="abs_pressure_pa">Absolute pressure in Pa.</field>
      <field type="int32_t" name="pressure_alt_cm">Pressure altitude in cm.</field>
      <field type="int16_t" name="xacc_mg">Body X specific force in milli-g.</field>
      <field type="int16_t" name="yacc_mg">Body Y specific force in milli-g.</field>
      <field type="int16_t" name="zacc_mg">Body Z specific force in milli-g.</field>
      <field type="int16_t" name="xgyro_mrad_s">Body X angular rate in mrad/s.</field>
      <field type="int16_t" name="ygyro_mrad_s">Body Y angular rate in mrad/s.</field>
      <field type="int16_t" name="zgyro_mrad_s">Body Z angular rate in mrad/s.</field>
      <field type="int16_t" name="xmag_mgauss">Body X magnetic field in milli-Gauss.</field>
      <field type="int16_t" name="ymag_mgauss">Body Y magnetic field in milli-Gauss.</field>
      <field type="int16_t" name="zmag_mgauss">Body Z magnetic field in milli-Gauss.</field>
      <field type="int16_t" name="diff_pressure_pa">Differential pressure in Pa.</field>
      <field type="int16_t" name="temperature_cdeg">Sensor temperature in centi-degrees Celsius.</field>
      <field type="uint16_t" name="sample_seq">Per-target sample sequence.</field>
      <field type="uint16_t" name="fields_updated">Validity and status bitmask.</field>
      <field type="uint8_t" name="target_system">Target system ID.</field>
      <field type="uint8_t" name="target_component">Target component ID.</field>
      <field type="uint8_t" name="sensor_id">Virtual IMU instance.</field>
    </message>

    <message id="42001" name="HIL_NAV_COMPACT">
      <description>Targeted compact simulated GNSS sample.</description>
      <field type="uint64_t" name="time_usec">Monotonic simulation time in microseconds.</field>
      <field type="int32_t" name="lat">Latitude in degrees times 1E7.</field>
      <field type="int32_t" name="lon">Longitude in degrees times 1E7.</field>
      <field type="int32_t" name="alt_msl_mm">MSL altitude in mm.</field>
      <field type="int16_t" name="vel_n_cms">North velocity in cm/s.</field>
      <field type="int16_t" name="vel_e_cms">East velocity in cm/s.</field>
      <field type="int16_t" name="vel_d_cms">Down velocity in cm/s.</field>
      <field type="uint16_t" name="cog_cdeg">Course over ground in centi-degrees.</field>
      <field type="uint16_t" name="eph_cm">Horizontal position accuracy in cm.</field>
      <field type="uint16_t" name="epv_cm">Vertical position accuracy in cm.</field>
      <field type="uint16_t" name="vel_acc_cms">Velocity accuracy in cm/s.</field>
      <field type="uint16_t" name="yaw_cdeg">GNSS yaw in centi-degrees; UINT16_MAX when unavailable.</field>
      <field type="uint16_t" name="sample_seq">Per-target sample sequence.</field>
      <field type="uint16_t" name="flags">Validity and status bitmask.</field>
      <field type="uint8_t" name="target_system">Target system ID.</field>
      <field type="uint8_t" name="target_component">Target component ID.</field>
      <field type="uint8_t" name="fix_type" enum="GPS_FIX_TYPE">GNSS fix type.</field>
      <field type="uint8_t" name="satellites_visible">Visible satellites; 255 when unknown.</field>
    </message>
  </messages>
</mavlink>
```

## 11. 固件端处理边界

ArduPlane 固件收到消息后应：

1. 验证 MAVLink 2、目标 SYSID/COMPID、字段有效位和时间新鲜度；
2. 解码定点量并恢复 SI 单位；
3. `HIL_IMU_COMPACT` 注入 SITL/HIL 专用 IMU、Compass、Baro、Airspeed 后端；
4. `HIL_NAV_COMPACT` 注入 HIL 专用 GPS 后端；
5. 不得直接覆盖 AHRS/EKF 的位置、速度或姿态结果；
6. 记录序号丢失、超时、饱和、重置和目标不匹配计数；
7. 仅在明确启用 HIL 模式时接受这两类消息，普通飞行固件必须忽略。

## 12. 尚未冻结的事项

以下事项在 XML 和固件接口首次合并前需要确认：

- 最终消息 ID 和 dialect 编号；
- 飞控端具体传感器后端接口和 HIL 启用开关；
- IMU 100 Hz 是否满足目标飞控 EKF，是否需要提高到 200 Hz；
- Mesh 单包 MTU、链路净带宽和是否启用 MAVLink 2 签名；
- `diff_pressure_pa` 的目标最大空速是否可能超过 `int16_t` 范围；
- 是否需要第二套 IMU、双 GPS 或 GPS yaw；
- Elevon、V-tail、差动舵面、多发动机和反推的执行器映射。
