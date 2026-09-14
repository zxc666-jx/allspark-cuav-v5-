# Allspark 与 MissionPlanner HITL 联调

修订日期：2026-09-05。外部仍为 Allspark V1.1 的 AB BA / FF 帧。

补丁版本：`2026.09.05.3`。`GET /api/v1/health` 增加 `allspark_bridge`，包含版本、实际加载路径、JBS2/JBF1/JBA1 支持情况。不含该字段的是旧后端。

本次检查发现正在运行的程序来自 E 盘 `PlaneGroup_GCS_HITL_v0.1.1_20260905`，旧后端仅使用 JBS1；G 盘源码更新不会自动更新它。请安全停止 HITL 和 MP 后再应用配套补丁，不要热替换正在运行的模型。

补丁固件在 JSBSim 真值失效、机号不符或链路超时时清除故障真值有效位 bit14，不以飞控内部空槽冒充模型故障解除。接收端必须检查有效位。

## 连接方式

Allspark → 飞控独立串口（SERIALn_PROTOCOL=51）→ 飞控现有 AHRS/HITL 链路（SERIALm_PROTOCOL=36）→ MissionPlanner 启动的 JSBSim 后端。

串口故障和 MP 故障窗口共用同一个 `FaultController`，不是同时向飞控和模型注入两次。执行机构故障改变送入 JSBSim 的实际控制输入；传感器故障改变模型输出给飞控的测量数据。后者不改变 JSBSim 的真实位置/姿态。

模型执行结果经 JBA1 回到飞控，再由外部 ACK 回 Allspark；模型故障真值经 JBS2 → AIRCRAFT_STATE 上报。MP 窗口可查询、更新、清除同一故障槽。多机按照 MAV_SYSID 隔离，每机最多 8 槽。

AIRCRAFT_STATE 的偏移 87 使用原保留字节上报 `active_fault_mask`，bit0..7 对应槽 0..7，载荷长度仍为 102 字节。测试 GUI 会缓存轮转的槽描述、按位图移除已解除槽，并在状态页显示飞控当前 GUIDED 目标。故障按钮发送前必须看到“JBS2 模型链路正常”；否则命令不会进入队列。

## 更新及启动

1. 只用于拆桨、无实际动力输出的实验室 HITL。不要在真实飞行中验证故障注入。
2. 给 MicoAir743v2 刷入本次编译的 `ardupilot_PlaneGroup/build/MicoAir743v2/bin/arduplane.apj`，保留并核对每架 SYSID_THISMAV。该固件不能直接刷 CUAV V5+，不同板卡需单独编译。
3. 停止并重新启动 MP 的多机 HITL 后端；已运行的 Python 进程不会自动加载新代码。
4. 本次更新的是本工程 `JSBSim/hil_supervisor.py`、`fault_injection.py` 和新增 `allspark_hil_bridge.py`。若使用其他位置的发布包，要一起更新这三个文件；无需重编译 MP 主程序。
5. 多机窗口的配置文件应位于本工程 `JSBSim/config` 内。它根据所选配置的父目录启动 `hil_fleet_supervisor.py`，并不一定使用 MP EXE 旁边的脚本。
6. 原串口或 UDP-JBS 设置继续使用。各模型的 sysid 必须与所连飞控一致；旧后端不处理 JBF1，会导致命令超时。

本工程根目录：`G:\Project\免疫算法固定翼编队\Missonplanner-Imunnity-Hardware-In-The-Loop\nuaa_gcs_install\nuaa_gcs_install`。

## 手工测试

测试脚本：`\\wsl.localhost\Ubuntu-24.04\home\zhy\ardupilot_PlaneGroup\Tools\scripts\allspark_serial_test.py`。

方便 Windows 使用的同版副本：`G:\Project\免疫算法固定翼编队\通信协议\allspark_serial_test.py`（2026-09-05 同步；以后更新以 ArduPlane 工程脚本为准）。

使用装有 pyserial 的 Python。以下 `COMxx` 要替换成 **Allspark 专用串口**，不是 MP MAVLink 端口，也不是正在被 HITL 后端占用的串口。

```powershell
# 在测试脚本所在目录执行；向 2 号模型槽 0 注入 30% 推力损失，持续 10 秒
python allspark_serial_test.py --port COMxx --baud 115200 fault --sysid 2 --command-id 101 --action inject --slot 0 --type THRUST_LOSS_STEP --p1 0.3 --duration-ms 10000

# 修改前一条故障为 50%；更新要使用新的命令编号
python allspark_serial_test.py --port COMxx --baud 115200 fault --sysid 2 --command-id 102 --action update --slot 0 --type THRUST_LOSS_STEP --p1 0.5

# 解除该机全部模型故障
python allspark_serial_test.py --port COMxx --baud 115200 fault --sysid 2 --command-id 103 --action reset
```

第一次正常收到 ACK result=1 表示排队，随后 result=0 才表示模型控制器已应用。观察 MP 中选定飞机的故障槽；也可在 MP 点击解除，再监听 Allspark 状态，故障数应归零。重复命令不应重新注入，也不应撤销 MP 的解除。

GUI 会记录所有未关联或字段不匹配的 ACK。若出现“忽略不匹配回执”，按日志中的 src/dst/seq/msg/command 定位机号或版本问题；不再把这类回执静默丢弃。

故障强度直接设置 P1；命令 severity 固定为 1000，不再静默忽略任意 severity。P1=0.3 表示损失 30%；上报 severity=1000 表示完整施加该幅度。duration=0 表示持续到解除。

error=10：模型链路不新鲜、编号不匹配或回执超时。超时属于结果未知，先看 MP 状态，不能换编号盲目重注入。error=8：槽或在途命令忙。error=11：相同标识的数据发生变化。请一条完成后再发下一条。

## 离线验证

在工程 `JSBSim` 目录执行：

```powershell
.\.venv\Scripts\python.exe -m unittest discover -s tests -p 'test_*.py'
```

新增测试覆盖类型映射、编号隔离、重复命令、槽占用、参数拒绝、混合帧和 CRC，以及真实 JSBSim + 本机回环 UDP + MP HTTP 查询/解除的联调。测试不打开真实 COM 口、不向电台地址发包。仍需用户设备上验证双串口、固件回执和实际 MP 窗口显示；离线通过不等于十机无线链路已通过压测。
