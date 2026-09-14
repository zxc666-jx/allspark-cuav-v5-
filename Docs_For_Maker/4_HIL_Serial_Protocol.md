# HIL Supervisor 串口二进制协议（JSBSim ↔ 飞控）

状态：Draft v0.1  
日期：2026-08-21  
适用范围：`hil_supervisor.py`（JSBSim Worker）、飞控 `AP_ExternalAHRS_JSBSim` 后端  
传输：专用 USB-to-TTL 串口（MicoAir743v2），默认 921600  
与 MAVLink 的关系：本协议是**主 HIL 闭环链路**（双向）；MAVLink/UDP 路径见 [1_HIL_MAVLink_Protocol.md](./1_HIL_MAVLink_Protocol.md)，为**备用 / 开环监视**链路

## 1. 目的

在 JSBSim Worker 与真实 ArduPlane 飞控之间建立一条**双向、低开销、确定性**的 HIL 链路：

- 下行（PC → 飞控）：JSBSim 真值状态，供飞控 `AP_ExternalAHRS_JSBSim` 后端作为外部 AHRS / GPS / 气压数据源；
- 上行（飞控 → PC）：飞控最终 SERVO1~4 PWM，驱动 JSBSim 舵面，形成闭环。

## 2. 帧格式

```text
┌─────────────┬───────────────┬─────────┬────────┐
│ magic (4 B) │ len (uint16)  │ payload │ CRC16  │
└─────────────┴───────────────┴─────────┴────────┘
```

- 小端序；
- `len` 为 payload 字节数（不含 magic 与 CRC）；
- CRC16：多项式 `0x1021`、初值 `0`、MSB-first、无反射，覆盖 `magic + len + payload`；
- 帧头错位、长度不符、CRC 不匹配时接收端必须丢弃该帧并统计，不得注入半帧数据。

## 3. 消息总表

| 方向 | magic | payload struct | 频率 | 说明 |
|---|---|---|---|---|
| PC → 飞控 | `JBS1` | `JSBSIM_SENSOR_PAYLOAD` | 100 Hz（默认，可配） | 全状态传感器 |
| 飞控 → PC | `JBO1` | `JSBSIM_OUTPUT_PAYLOAD` | 跟随飞控 | SERVO1~4 PWM |

## 4. JBS1 传感器 payload（116 字节）

结构：`<I 12f 3i 6f B B H I 5f`

| 序号 | 类型 | JSBSim 源属性 | 单位 | 说明 |
|---|---|---|---|---|
| 1 | uint32 | `sim_time_s × 1000` | ms | 仿真时间 |
| 2 | float | `attitude/phi-rad` | rad | 滚转 |
| 3 | float | `attitude/theta-rad` | rad | 俯仰 |
| 4 | float | `attitude/psi-rad` | rad | 偏航 |
| 5 | float | `velocities/p-rad_sec` | rad/s | 滚转角速度 |
| 6 | float | `velocities/q-rad_sec` | rad/s | 俯仰角速度 |
| 7 | float | `velocities/r-rad_sec` | rad/s | 偏航角速度 |
| 8 | float | `accelerations/a-pilot-x-ft_sec2 × FT_TO_M` | m/s² | 机体系 X 比力 |
| 9 | float | `accelerations/a-pilot-y-ft_sec2 × FT_TO_M` | m/s² | 机体系 Y 比力 |
| 10 | float | `accelerations/a-pilot-z-ft_sec2 × FT_TO_M` | m/s² | 机体系 Z 比力 |
| 11~13 | float | `0.0` | — | 磁场 X/Y/Z（未建模，置 0） |
| 14 | int32 | `position/lat-geod-deg × 1e7` | degE7 | 纬度 |
| 15 | int32 | `position/long-gc-deg × 1e7` | degE7 | 经度 |
| 16 | int32 | `position/h-sl-ft × FT_TO_M × 100` | cm | MSL 高度 |
| 17 | float | `velocities/v-north-fps × FPS_TO_MPS` | m/s | 北向速度 |
| 18 | float | `velocities/v-east-fps × FPS_TO_MPS` | m/s | 东向速度 |
| 19 | float | `velocities/v-down-fps × FPS_TO_MPS` | m/s | 地向速度 |
| 20 | float | `atmosphere/P-psf × PSF_TO_PA` | Pa | 绝对静压 |
| 21 | float | `aero/qbar-psf × PSF_TO_PA` | Pa | 动压（差压） |
| 22 | float | `(atmosphere/T-R − 491.67) × 5/9` | °C | 温度 |
| 23 | uint8 | `3` | — | GPS fix_type |
| 24 | uint8 | `12` | — | 可见卫星数 |
| 25 | uint16 | `gps_week` | — | GPS 周 |
| 26 | uint32 | `gps_week_ms` | ms | 周内毫秒 |
| 27~31 | float ×5 | `0.8, 1.2, 0.3, 0.7, 1.2` | — | ⚠️ 精度值，顺序/语义待与飞控端 `AP_ExternalAHRS_JSBSim` 对齐 |

> GPS 时间基点为 `1980-01-06 00:00:00 UTC`。PC 侧在启动时固定 `gps_epoch_s`，此后只累加 `sim_time_s`，保证**可复现**。

## 5. JBO1 输出 payload（12 字节）

结构：`<I 4H`

| 序号 | 类型 | 说明 |
|---|---|---|
| 1 | uint32 | 飞控时间戳（PC 侧当前未使用，解析时丢弃） |
| 2 | uint16 | SERVO1 PWM → 副翼 |
| 3 | uint16 | SERVO2 PWM → 升降舵 |
| 4 | uint16 | SERVO3 PWM → 油门 |
| 5 | uint16 | SERVO4 PWM → 方向舵 |

PWM 反算（`apply_pwm_outputs`）：

```text
舵面  = clamp((pwm − 1500) / 500, −1, 1)
油门  = clamp((pwm − 1000) / 1000, 0, 1)
```

⚠️ 当前硬编码 SERVO1/2/3/4 顺序，未按 `SERVOx_FUNCTION` 动态查找、未解回 `SERVOx_REVERSED`；待接入 Mission Planner 标定参数后替换。

## 6. 闭环时序

```text
每个物理步（400 Hz）：
  1. receive_controls()：读串口缓冲，解析 JBO1 → 写 JSBSim fcs/*-cmd-norm
  2. step()：推进 JSBSim 一个 dt
  3. publish_due()：到采样点则发 JBS1（默认 100 Hz，按仿真时钟调度）
```

采样调度以**仿真时钟**为基准，不依赖墙钟是否准时；`--no-realtime` 下同样得到一致的采样时刻。

## 7. 与飞控端 `AP_ExternalAHRS_JSBSim` 的约定

- 飞控端需存在 `AP_ExternalAHRS_JSBSim` 后端：解析 JBS1，注入外部 AHRS / GPS / 气压；
- 磁力计字段（序号 11~13）当前恒为 0，后端应忽略；
- 5 个精度字段（序号 27~31）的语义以飞控端解码为准，本表待对齐；
- 上行 JBO1 的 SERVO 映射必须与飞控 `SERVOx_FUNCTION` 一致（1=副翼 2=升降 3=油门 4=方向舵）。

## 8. 尚未冻结的事项

- 5 个精度字段的确切语义与顺序；
- 磁力计建模（当前 JSBSim 无磁场模型）；
- SERVO 映射是否改为按 `SERVOx_FUNCTION` 动态解析；
- 是否需要 ACK/心跳/超时检测（当前协议无逐帧 ACK）。
