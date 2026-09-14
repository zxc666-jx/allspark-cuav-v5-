using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Reflection;
using MissionPlanner.Utilities;
using MissionPlanner;
using System.Windows.Forms;

namespace MissionPlanner.Swarm
{
    abstract class Swarm
    {
        internal static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal MAVState Leader = null;

        public void setLeader(MAVState lead)
        {
            Leader = lead;
        }

        public MAVState getLeader()
        {
            return Leader;
        }

        public void Arm()
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    //if (mav == Leader)
                    //    continue;
                    try
                    {
                        if (!mav.cs.armed)
                        {
                            bool ans = port.doARM(mav.sysid, mav.compid, true);
                            if (ans == false)
                                CustomMessageBox.Show(port.BaseStream.PortName + " " + mav.sysid + " " + mav.compid + "解锁失败，请查看报错消息", port.BaseStream.PortName + " " + mav.sysid + " " + mav.compid);
                        }
                    }
                    catch
                    {
                        MessageBox.Show(port.BaseStream.PortName + " " + mav.sysid + " " + mav.compid + "解锁失败，请查看报错消息");
                    }
                }
            }
        }

        public void Disarm()
        {
            int armedmavcounts = 0;
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav.cs.armed)
                    {
                        armedmavcounts++;
                    }
                }
            }
            if (armedmavcounts >= 1)
            {
                if (CustomMessageBox.Show("是否确定全部锁定?", "全部锁定?", MessageBoxButtons.YesNo) != (int)DialogResult.Yes)
                    return;
            }
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    //if (mav == Leader)
                    //    continue;

                    port.doARM(mav.sysid, mav.compid, false);
                }
            }
        }

        public void Takeoff(uint takeoffalt = 5)
        {
            List<string> errors = new List<string>();

            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    string aircraft = port.BaseStream.PortName + " SYSID " + mav.sysid;

                    try
                    {
                        if (mav.cs.firmware == MissionPlanner.ArduPilot.Firmwares.ArduPlane)
                        {
                            // Conventional fixed-wing aircraft cannot take off from rest
                            // with the Copter GUIDED/TAKEOFF command.  They must execute a
                            // mission whose first flying item is NAV_TAKEOFF in AUTO mode.
                            bool hasTakeoffMission = false;
                            foreach (var waypoint in mav.wps.Values)
                            {
                                if (waypoint.command == (ushort)MAVLink.MAV_CMD.TAKEOFF)
                                {
                                    hasTakeoffMission = true;
                                    break;
                                }
                            }

                            if (!hasTakeoffMission)
                            {
                                errors.Add(aircraft + "：未检测到起飞航点，请先写入包含 TAKEOFF 的航线");
                                continue;
                            }

                            if (!mav.cs.armed && !port.doARM(mav.sysid, mav.compid, true))
                            {
                                errors.Add(aircraft + "：解锁失败，请查看“消息”页中的预检错误");
                                continue;
                            }

                            port.setMode(mav.sysid, mav.compid, "AUTO");
                        }
                        else
                        {
                            // Preserve the original multicopter takeoff behaviour.
                            port.setMode(mav.sysid, mav.compid, "GUIDED");
                            if (!port.doCommand(mav.sysid, mav.compid, MAVLink.MAV_CMD.TAKEOFF,
                                0, 0, 0, 0, 0, 0, takeoffalt))
                            {
                                errors.Add(aircraft + "：起飞指令被拒绝");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(aircraft + " takeoff failed", ex);
                        errors.Add(aircraft + "：" + ex.Message);
                    }
                }
            }

            if (errors.Count > 0)
            {
                CustomMessageBox.Show(string.Join(Environment.NewLine, errors.ToArray()),
                    "编队起飞未完成");
            }
        }

        public void Land()
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    port.setMode(mav.sysid, mav.compid, "Land");
                }
            }
        }
        public void RTL()
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    port.setMode(mav.sysid, mav.compid, "RTL");
                }
            }
        }
        public void Auto()
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    port.setMode(mav.sysid, mav.compid, "Auto");
                }
            }
        }

        public void Stop()
        {
        }

        public void GuidedMode()
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == Leader)
                        continue;

                    port.setMode(mav.sysid, mav.compid, "GUIDED");
                }
            }
        }

        public abstract void Update();

        public abstract void SendCommand();
    }
}
