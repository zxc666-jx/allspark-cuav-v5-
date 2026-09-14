# JSBSim HIL 环境控制

## 从 Mission Planner 启动

1. 打开 Mission Planner 的工具页面。
2. 点击“JSBSim HIL 环境”。原来的“Start Xplane”入口仍然保留。
3. 确认 Python 指向 `JSBSim\.venv\Scripts\python.exe`，配置指向
   `JSBSim\config\vehicle01_serial_hil.yaml`。
4. 点击“启动”。窗口会等待最多 10 秒，连接
   `http://127.0.0.1:8765` 后开始显示传感器、舵机和环境状态。

COM6 是飞控与 JSBSim 的专用 JBS1/JBO1 硬件在环串口。环境窗口只使用
本机 HTTP，不会打开 COM6；不要让其他串口软件同时占用 COM6。

## 参数约定

- 风向采用气象“来向”：0°表示北风，90°表示东风。
- 垂直风正值表示向上；程序会转换为 JSBSim 的 NED（Down 为正）坐标。
- 稳态风和湍流修改后点击“应用稳态风和湍流”。
- 阵风使用渐入、保持、渐出三段余弦平滑曲线，点击“触发阵风”开始一次。
- “恢复无风”会立即关闭稳态风和湍流，并取消正在进行的阵风。
- 界面修改只影响本次运行，不会覆盖 YAML。

## 控制 API

API 只监听回环地址：

- `GET /api/v1/health`
- `GET /api/v1/vehicles/1/environment`
- `PUT /api/v1/vehicles/1/environment`
- `POST /api/v1/vehicles/1/environment/gust`
- `POST /api/v1/vehicles/1/environment/reset`
- `POST /api/v1/shutdown`

所有修改先进入命令队列，再由 400 Hz 物理线程写入 JSBSim，HTTP 线程不会直接
操作飞行动力学实例。

## 日志

每次运行在 `JSBSim\logs\<UTC时间>_sysid1\` 下生成：

- `environment_events.jsonl`：环境命令、修改前后值和应用仿真时间。
- `environment_status.csv`：5 Hz 实际总风、飞机状态和串口链路状态。

## 自动测试

在 PowerShell 中运行：

```powershell
& .\.venv\Scripts\python.exe -m unittest discover -s .\tests -v
```

测试覆盖风向转换、垂直风符号、平滑阵风、复位、输入校验、固定随机种子以及
HTTP API 路由。
