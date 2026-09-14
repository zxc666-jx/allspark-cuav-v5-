# 故障注入参数例程（需求表 1~26 全覆盖）

> 适用固件：ArduPlane（免疫集群实验分支）
> 参数前缀：`PLN_FIJ_*` | 场景命令：`FIJ SCEN <id>`（串口，`fault_injection_comm.cpp`）
> 场景表默认值来源：`fault_injection.cpp:639-682`（各等级取最小档）

## 通用例程模板（参数表路径 · 单故障）

```
PLN_FIJ_TYPE=<1~13>
PLN_FIJ_P1=<强度>
PLN_FIJ_P2=<副参数>          # 舵面类必填：0=升降舵 1=方向舵 2=副翼
PLN_FIJ_RAMP_IN=<秒，0=阶跃>
PLN_FIJ_HOLD_S=<秒，保持时长>
PLN_FIJ_RAMP_OUT=<秒，0=阶跃恢复>
PLN_FIJ_DELAY=<毫秒>         # ⚠️ 毫秒，不是秒
PLN_FIJ_AUTO=1               # 触发一次性注入（成功后自动归 0）
```

恢复方式（任选其一）：
- **自动**：走完 `RAMP_IN → ACTIVE(HOLD_S) → RAMP_OUT` 生命周期后恢复；
- **拨杆**：拨动遥控器通道 5（PWM 变化 >30us）→ `reset()` 全部故障立即恢复；
- **命令**：`FIJ RESET`（串口）。

---

## 一、动力故障

### 故障 1：推力损失-阶跃 `THRUST_LOSS_STEP`（TYPE=1）
> 效果：油门指令 ×(1−λ)，阶跃生效。档位 λ=10/20/30/40%

低档（λ=10%，场景默认）：
```
PLN_FIJ_TYPE=1   PLN_FIJ_P1=0.10   PLN_FIJ_HOLD_S=10   PLN_FIJ_AUTO=1
```
高档（λ=40%）：
```
PLN_FIJ_TYPE=1   PLN_FIJ_P1=0.40   PLN_FIJ_HOLD_S=10   PLN_FIJ_AUTO=1
```

### 故障 2：推力损失-缓变 `THRUST_LOSS_RAMP`（TYPE=3）
> 效果：λ 由 0 线性升至目标（RAMP_IN=5s），档位 λ=10/20/30/40%

低档（λ=10%，场景默认 ramp_in=5/out=5）：
```
PLN_FIJ_TYPE=3   PLN_FIJ_P1=0.10   PLN_FIJ_RAMP_IN=5   PLN_FIJ_HOLD_S=10   PLN_FIJ_RAMP_OUT=5   PLN_FIJ_AUTO=1
```

---

## 二、舵面故障（P2=0 升降 / 1 方向 / 2 副翼）

### 故障 3/4/5：舵面卡死 `SURFACE_JAM`（TYPE=5）
> 效果：忽略后续舵指令，输出固定 δjam（度），记录触发前/后舵角。档位 δjam=±5/±10°

升降舵卡死 -5°（场景 3 默认）：
```
PLN_FIJ_TYPE=5   PLN_FIJ_P1=-5   PLN_FIJ_P2=0   PLN_FIJ_HOLD_S=10   PLN_FIJ_AUTO=1
```
方向舵卡死 +10°（场景 4 扩展）：
```
PLN_FIJ_TYPE=5   PLN_FIJ_P1=10   PLN_FIJ_P2=1   PLN_FIJ_HOLD_S=10   PLN_FIJ_AUTO=1
```
副翼卡死 -10°（场景 5 扩展）：
```
PLN_FIJ_TYPE=5   PLN_FIJ_P1=-10   PLN_FIJ_P2=2   PLN_FIJ_HOLD_S=10   PLN_FIJ_AUTO=1
```

### 故障 6/7/8：舵效下降-阶跃 `SURFACE_EFF_STEP`（TYPE=2）
> 效果：舵指令 ×(1−λ) 阶跃。档位 λ=10/20/30/40%

升降舵效降 10%（场景 6 默认）：
```
PLN_FIJ_TYPE=2   PLN_FIJ_P1=0.10   PLN_FIJ_P2=0   PLN_FIJ_HOLD_S=10   PLN_FIJ_AUTO=1
```
副翼舵效降 40%（场景 8 扩展）：
```
PLN_FIJ_TYPE=2   PLN_FIJ_P1=0.40   PLN_FIJ_P2=2   PLN_FIJ_HOLD_S=10   PLN_FIJ_AUTO=1
```

### 故障 9/10/11：舵效下降-缓变 `SURFACE_EFF_RAMP`（TYPE=4）
> 效果：舵效按 λ 缓变下降（RAMP_IN）。档位 λ=10/20/30/40%

方向舵效缓降 10%（场景 10 默认，ramp_in=5）：
```
PLN_FIJ_TYPE=4   PLN_FIJ_P1=0.10   PLN_FIJ_P2=1   PLN_FIJ_RAMP_IN=5   PLN_FIJ_HOLD_S=10   PLN_FIJ_AUTO=1
```

---

## 三、空速计故障（对传感器读数注入）

### 故障 12：空速噪声 `AIRSPEED_NOISE`（TYPE=6）
> 效果：Vfault = Vnormal + n, n~N(0,σ²)。档位 σ=2/4/6 m/s

σ=2（场景 12 默认）：
```
PLN_FIJ_TYPE=6   PLN_FIJ_P1=2   PLN_FIJ_HOLD_S=10   PLN_FIJ_AUTO=1
```

### 故障 13：空速漂移 `AIRSPEED_DRIFT`（TYPE=7）
> 效果：Vfault = Vnormal + r·(t−t0)，达上限后保持。档位 r=±0.2/±0.5 m/s²，上限 |ΔV|=5/10 m/s

r=0.2、上限 5（场景 13 默认）：
```
PLN_FIJ_TYPE=7   PLN_FIJ_P1=0.2   PLN_FIJ_P2=5   PLN_FIJ_HOLD_S=10   PLN_FIJ_AUTO=1
```
r=0.5、上限 10（高档）：
```
PLN_FIJ_TYPE=7   PLN_FIJ_P1=0.5   PLN_FIJ_P2=10   PLN_FIJ_HOLD_S=10   PLN_FIJ_AUTO=1
```

### 故障 14：空速偏差 `AIRSPEED_BIAS`（TYPE=8）
> 效果：Vfault = Vnormal + b。档位 b=±5/±10 m/s

b=+5（场景 14 默认）：
```
PLN_FIJ_TYPE=8   PLN_FIJ_P1=5   PLN_FIJ_HOLD_S=10   PLN_FIJ_AUTO=1
```
b=−10（高档）：
```
PLN_FIJ_TYPE=8   PLN_FIJ_P1=-10   PLN_FIJ_HOLD_S=10   PLN_FIJ_AUTO=1
```

### 故障 15：空速冻结 `AIRSPEED_FREEZE`（TYPE=9）
> 效果：冻结最后有效值，valid=0（数据失效）。档位：无（P1/P2 不用）

```
PLN_FIJ_TYPE=9   PLN_FIJ_HOLD_S=10   PLN_FIJ_AUTO=1
```

---

## 四、高度计故障（对高度读数注入）

### 故障 16：高度噪声 `ALTITUDE_NOISE`（TYPE=10）
> 效果：Hfault = Hnormal + n, n~N(0,σ²)。档位 σ=5/15/30 m

σ=5（场景 16 默认）：
```
PLN_FIJ_TYPE=10   PLN_FIJ_P1=5   PLN_FIJ_HOLD_S=10   PLN_FIJ_AUTO=1
```

### 故障 17：高度漂移 `ALTITUDE_DRIFT`（TYPE=11）
> 效果：Hfault = Hnormal + r·(t−t0)，达上限后保持。档位 r=±1/±3 m/s，上限 |ΔH|=50/100 m

r=1、上限 50（场景 17 默认）：
```
PLN_FIJ_TYPE=11   PLN_FIJ_P1=1   PLN_FIJ_P2=50   PLN_FIJ_HOLD_S=10   PLN_FIJ_AUTO=1
```
r=3、上限 100（高档）：
```
PLN_FIJ_TYPE=11   PLN_FIJ_P1=3   PLN_FIJ_P2=100   PLN_FIJ_HOLD_S=10   PLN_FIJ_AUTO=1
```

### 故障 18：高度偏差 `ALTITUDE_BIAS`（TYPE=12）
> 效果：Hfault = Hnormal + b。档位 b=±20/±40 m

b=+20（场景 18 默认）：
```
PLN_FIJ_TYPE=12   PLN_FIJ_P1=20   PLN_FIJ_HOLD_S=10   PLN_FIJ_AUTO=1
```

### 故障 19：高度冻结 `ALTITUDE_FREEZE`（TYPE=13）
> 效果：冻结最后有效值，valid=0。档位：无

```
PLN_FIJ_TYPE=13   PLN_FIJ_HOLD_S=10   PLN_FIJ_AUTO=1
```

---

## 五、复合故障（表 20~26，双槽同步注入）

> ⚠️ 参数表路径（`check_auto_inject`）**一次只能注入单故障**；复合故障必须用
> `FIJ SCEN <id>`（串口）或两条 `FIJ SET`（slot 0 + slot 1）。

### 场景 20：推力损失-阶跃 + 升降舵效下降-阶跃
```
FIJ SCEN 20
```
等价双命令（λ 各 0.10，P2=0 升降舵）：
```
FIJ SET 0 THRUST_LOSS_STEP 0.10 0 10 0 0 0
FIJ SET 1 SURFACE_EFF_STEP 0.10 0 10 0 0 0
```

### 场景 21：升降舵效下降 + 方向舵效下降
```
FIJ SCEN 21
```

### 场景 22：推力损失-阶跃 + 空速冻结
```
FIJ SCEN 22
```

### 场景 23：推力损失-阶跃 + 高度冻结
```
FIJ SCEN 23
```

### 场景 24：升降舵效下降 + 空速冻结
```
FIJ SCEN 24
```

### 场景 25：升降舵效下降 + 高度冻结
```
FIJ SCEN 25
```

### 场景 26：空速冻结 + 高度冻结
```
FIJ SCEN 26
```

> 复合故障自定义强度：用两条 `FIJ SET` 分别设 slot 0 / slot 1 的参数
> （格式：`FIJ SET <slot 0|1|A> <type> <p1> [p2] [hold] [ramp_in] [ramp_out] [delay_s]`，
> 注意此处 delay 单位是**秒**）。

---

## 附：TYPE ↔ 故障速查

| TYPE | 故障 | 表号 |
|---|---|---|
| 1 | THRUST_LOSS_STEP | 1 |
| 2 | SURFACE_EFF_STEP | 6~8 |
| 3 | THRUST_LOSS_RAMP | 2 |
| 4 | SURFACE_EFF_RAMP | 9~11 |
| 5 | SURFACE_JAM | 3~5 |
| 6 | AIRSPEED_NOISE | 12 |
| 7 | AIRSPEED_DRIFT | 13 |
| 8 | AIRSPEED_BIAS | 14 |
| 9 | AIRSPEED_FREEZE | 15 |
| 10 | ALTITUDE_NOISE | 16 |
| 11 | ALTITUDE_DRIFT | 17 |
| 12 | ALTITUDE_BIAS | 18 |
| 13 | ALTITUDE_FREEZE | 19 |
| —（FIJ SCEN） | 复合 20~26 | 20~26 |
