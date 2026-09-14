using MissionPlanner.Comms;
using MissionPlanner.Utilities;
using System;
using static MissionPlanner.CurrentState;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;
using static MissionPlanner.Swarm.CommandManager;
using DotSpatial.Positioning;

namespace MissionPlanner.Swarm
{
    public class CommandManager
    {
        static uint mpAddress = 10010;
        static uint commandAddress = 10003;
        static uint Shinjuku1 = 0x03643163;
        static ushort command1 = 0x0388;
        static uint Shinjuku2 = 0x02633163;
        static byte[] Xinyuan = { 01, 30, 63 };
        static ushort command2 = 0x0005;

        int readStep = 0;
        UInt64 extralReadTimestamp = 0;
        int timestampIndex = 0;
        int messageSeq = 0;
        int messageLen = 0;
        int mesLenIndex = 0;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TargetLJMessage
        {
            public byte interval;
            public int longitude;
            public int latitude;
            public int alt;
            public ushort radar;
            public ushort direction;
            public uint Batch;
            public byte model;
            public byte rackCount;
        }

        public static TargetLJMessage targetLJMessage = new TargetLJMessage();
        byte[] srcAddress = BitConverter.GetBytes(commandAddress);
        byte[] destAddress = BitConverter.GetBytes(mpAddress);
        byte[] Xinsu1 = BitConverter.GetBytes(Shinjuku1);
        byte[] Zhihui1 = BitConverter.GetBytes(command1);
        byte[] Xinsu2 = BitConverter.GetBytes(Shinjuku2);
        byte[] Zhihui2 = BitConverter.GetBytes(command2);
        byte[] timestampbyte = new byte[8];
        byte[] mesLenByte = new byte[2];
        byte[] payload = new byte[15];
        int payloadIndex = 0;
        public uint timestamp1;
        public uint timestamp2;
        byte local_rx_crc = 0;
        DateTime lastReadTime = DateTime.Now.AddSeconds(-30);
        ICommsSerial comPort = null;
        int cnt_mav = 0;

        public void setPort(ICommsSerial serial)
        {
            comPort = serial;
        }

        public void ParseReadData(byte[] a)
        {
            for (int i = 0; i < a.Length; i++)
            {
                byte data = a[i];
                switch (readStep)
                {
                    // 0-3报头类型
                    case 0:
                        if (Xinsu1[0] == data)
                        {
                            readStep++;
                        }
                        local_rx_crc = 0;
                        break;
                    case 1:
                        if (Xinsu1[1] == data)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    case 2:
                        if (Xinsu1[2] == data)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    case 3:
                        if (Xinsu1[3] == data)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    // 4-6 目标地址
                    case 4:
                        if (true)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    case 5:
                        if (true)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    case 6:
                        if (true)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    //7,8指挥语句
                    case 7:
                        if (Zhihui1[0] == data)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    case 8:
                        if (Zhihui1[1] == data)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    // 9-10帧头
                    case 9:
                        if (data == 0xef)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    case 10:
                        if (data == 0x0f)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    //11-13报文内容时间
                    case 11: // 解析第一个 3 字节时间戳
                        if (timestampIndex == 0) Array.Clear(timestampbyte, 0, timestampbyte.Length);
                        timestampbyte[timestampIndex++] = data;
                        if (timestampIndex == 3)
                        {
                            timestamp1 = (UInt32)(timestampbyte[0] | (timestampbyte[1] << 8) | (timestampbyte[2] << 16));
                            timestampIndex = 0;
                            readStep++;
                        }
                        break;

                    case 12: // 解析第二个 3 字节时间戳
                        timestampbyte[timestampIndex++] = data;
                        if (timestampIndex == 3)
                        {
                            timestamp2 = (UInt32)(timestampbyte[0] | (timestampbyte[1] << 8) | (timestampbyte[2] << 16));
                            timestampIndex = 0;
                            readStep++;
                        }
                        break;

                    case 13: // 继续后续解析
                        messageSeq++;
                        if (messageSeq >= 2)
                        {
                            readStep++;
                            messageLen = 0;
                            local_rx_crc = 0;
                        }
                        break;
                }
            }

        }
        public void ParseReadDataR(byte[] a)
        {
            for (int i = 0; i < a.Length; i++)
            {
                byte data = a[i];
                switch (readStep)
                {
                    // 0-3报头类型
                    case 0:
                        if (Xinsu2[0] == data)
                        {
                            readStep++;
                        }
                        local_rx_crc = 0;
                        break;
                    case 1:
                        if (Xinsu2[1] == data)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    case 2:
                        if (Xinsu2[2] == data)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    case 3:
                        if (Xinsu2[3] == data)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    // 4-6 信源
                    case 4:
                        if (Xinyuan[0] == data)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    case 5:
                        if (Xinyuan[1] == data)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    case 6:
                        if (Xinyuan[2] == data)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    //7,8指挥语句
                    case 7:
                        if (Zhihui2[0] == data)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    case 8:
                        if (Zhihui2[1] == data)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    // 9-10帧头
                    case 9:
                        if (data == 0xff)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    case 10:
                        if (data == 0x1a)
                        {
                            readStep++;
                            break;
                        }
                        readStep = 0;
                        break;
                    //11雷达标识
                    case 11:
                        if (timestampIndex == 0)
                        {
                            targetLJMessage.radar = (ushort)(data); // 存储第一个字节
                            timestampIndex++;
                        }
                        else
                        {
                            targetLJMessage.radar |= (ushort)(data << 8); // 存储第二个字节
                            timestampIndex = 0; // 重置索引
                            readStep++; // 进入时间戳解析
                        }
                        break;
                    //12目标批次
                    case 12:
                        targetLJMessage.Batch |= (uint)(data << (8 * (3 - timestampIndex))); // 逐字节存储并反转字节顺序
                        timestampIndex++;

                        if (timestampIndex == 4) // 4 字节读取完成
                        {
                            timestampIndex = 0; // 重置索引
                            readStep++; // 进入解析下一个字段
                        }
                        break;
                    //经度
                    case 13:
                        targetLJMessage.longitude |= (data << (8 * (3 - timestampIndex))); // 逐字节存储并反转字节顺序
                        timestampIndex++;

                        if (timestampIndex == 4) // 4 字节读取完成
                        {
                            timestampIndex = 0; // 重置索引
                            readStep++; // 进入解析下一个字段
                        }
                        break;
                    //纬度
                    case 14:
                        targetLJMessage.latitude |= (data << (8 * (3 - timestampIndex))); // 逐字节存储并反转字节顺序
                        timestampIndex++;

                        if (timestampIndex == 4) // 4 字节读取完成
                        {
                            timestampIndex = 0; // 重置索引
                            readStep++; // 进入解析下一个字段
                        }
                        break;
                    //高度
                    case 15:
                        targetLJMessage.alt |= (data << (8 * timestampIndex)); // 存储经度
                        timestampIndex++;

                        if (timestampIndex == 4) // 4 字节读取完成
                        {
                            timestampIndex = 0; // 重置索引
                            readStep++; // 进入解析纬度
                        }
                        break;
                    case 16: //机型
                        targetLJMessage.model = data;  // 存储机型
                        readStep++;
                        break;
                    case 17: //架数
                        targetLJMessage.rackCount = data;  // 存储机型
                        readStep++;
                        break;
                    case 18://发送时间
                        if (timestampIndex == 0) Array.Clear(timestampbyte, 0, timestampbyte.Length);
                        timestampbyte[timestampIndex++] = data;
                        if (timestampIndex == 3)
                        {
                            timestamp1 = (UInt32)(timestampbyte[0] | (timestampbyte[1] << 8) | (timestampbyte[2] << 16));
                            timestampIndex = 0;
                            readStep++;
                        }
                        break;
                }
            }

        }

        // 火力设备状态发送
        DateTime FirepowerStateSendNext = DateTime.Now;
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct FireState
        {
            public ushort mesSeq;
            public ushort mesLength;
            public byte commandMain;
            public byte commandSlave;
            public byte controlState;
            public byte droneCount;
            public byte leaderMask;
            public int leaderLon;
            public int leaderLat;
            public int leaderAlt;
            public byte crc;
        }
        FireState FirState = new FireState();

        // 1HZ发送
        public void SendFirepowerState(MAVState leader, int controlState, int mavCount)
        {
            uint srcAddress;
            uint destAddress;
            UInt32 Absolutetime = (UInt32)((DateTime.UtcNow - DateTime.UtcNow.Date).TotalMilliseconds / 10);
            if (DateTime.Now < FirepowerStateSendNext)
            {
                return;
            }
            FirepowerStateSendNext = DateTime.Now.AddSeconds(1.0d);
            srcAddress = mpAddress;
            destAddress = commandAddress;
            FirState.mesSeq = 0;
            FirState.mesLength = 17;
            FirState.commandMain = 0xA4;
            FirState.commandSlave = 0;
            FirState.controlState = (byte)controlState;
            FirState.droneCount = (byte)mavCount;

            byte[] writeData = MavlinkUtil.StructureToByteArray(CommState);
            if (comPort != null)
            {
                comPort.Write(writeData, 0, writeData.Length);
            }
        }

        //通信状态发送
        DateTime CommStateSendNext = DateTime.Now;
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TongState
        {
            public byte Baotou;
            public byte[] Xinsu;
            public byte[] Xinyuan;
            public ushort Zhihui;
            public byte commandMain;
            public byte commandSlave;
            public byte AbsolutetimeH;
            public byte AbsolutetimeM;
            public byte AbsolutetimeL;
        }
        TongState CommState = new TongState();



        // 3s发送
        public void SendCommState(MAVState leader, int controlState, int mavCount)
        {
            uint srcAddress;
            uint destAddress;
            UInt32 Absolutetime = (UInt32)((DateTime.UtcNow - DateTime.UtcNow.Date).TotalMilliseconds / 10);
            if (DateTime.Now < CommStateSendNext)
            {
                return;
            }
            CommStateSendNext = DateTime.Now.AddSeconds(3.0d);
            srcAddress = mpAddress;
            destAddress = commandAddress;
            CommState.Baotou = 02;
            CommState.Xinsu[0] = 01;
            CommState.Xinsu[1] = 30;
            CommState.Xinsu[2] = 63;
            CommState.Xinyuan[0] = 81;
            CommState.Xinyuan[1] = 31;
            CommState.Xinyuan[2] = 63;
            CommState.Zhihui = 0x0551;
            CommState.commandMain = 0xef;
            CommState.commandSlave = 0x0d;
            CommState.AbsolutetimeH = (byte)((Absolutetime >> 16) & 0xFF);
            CommState.AbsolutetimeM = (byte)((Absolutetime >> 8) & 0xFF);
            CommState.AbsolutetimeL = (byte)(Absolutetime & 0xFF);


            byte[] writeData = MavlinkUtil.StructureToByteArray(CommState);
            if (comPort != null)
            {
                comPort.Write(writeData, 0, writeData.Length);
            }
        }

        // 地理坐标发送
        DateTime LocationStateSendNext = DateTime.Now;
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct LocState
        {
            public byte Baotou;
            public byte[] Xinsu;
            public byte[] Xinyuan;
            public ushort Zhihui;
            public byte commandMain;
            public byte commandSlave;
            public byte Danwei1;
            public byte Danwei2;
            public byte Danwei3;
            public int Longitude;
            public int Latitude;
            public int Altitdue;
            public byte AbsolutetimeH;
            public byte AbsolutetimeM;
            public byte AbsolutetimeL;
        }
        LocState LocaState = new LocState();

        // 修改后发送
        private bool hasSentLocaState = false; // 标记是否已经发送
        public void SendLocaState(MAVState leader, int controlState, int mavCount)
        {
            uint srcAddress;
            uint destAddress;
            UInt32 Absolutetime = (UInt32)((DateTime.UtcNow - DateTime.UtcNow.Date).TotalMilliseconds / 10);
            if (hasSentLocaState) // 如果已经发送过，直接返回
            {
                return;
            }

            srcAddress = mpAddress;
            destAddress = commandAddress;
            LocaState.Baotou = 02;
            LocaState.Xinsu[0] = 01;
            LocaState.Xinsu[1] = 30;
            LocaState.Xinsu[2] = 63;
            LocaState.Xinyuan[0] = 81;
            LocaState.Xinyuan[1] = 31;
            LocaState.Xinyuan[2] = 63;
            LocaState.Zhihui = 0x0551;
            LocaState.commandMain = 0xe0;
            LocaState.commandSlave = 0x05;
            LocaState.Danwei1 = 51;
            LocaState.Danwei2 = 31;
            LocaState.Danwei3 = 63;
            LocaState.Longitude = (int)MainV2.comPort.MAV.cs.HomeLocation.Lng * 360;
            LocaState.Latitude = (int)MainV2.comPort.MAV.cs.HomeLocation.Lat * 360;
            LocaState.Altitdue = (short)MainV2.comPort.MAV.cs.HomeLocation.Alt;
            LocaState.AbsolutetimeH = (byte)((Absolutetime >> 16) & 0xFF);
            LocaState.AbsolutetimeM = (byte)((Absolutetime >> 8) & 0xFF);
            LocaState.AbsolutetimeL = (byte)(Absolutetime & 0xFF);

            byte[] writeData = MavlinkUtil.StructureToByteArray(LocaState);
            if (comPort != null)
            {
                comPort.Write(writeData, 0, writeData.Length);
            }

            hasSentLocaState = true; // 标记为已发送
        }


        // 目标跟踪和设计状态发送
        DateTime TrackingStateSendNext = DateTime.Now;
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TrackState
        {
            public byte Baotou;
            public byte[] Xinsu;
            public byte[] Xinyuan;
            public ushort Zhihui;
            public byte commandMain;
            public byte commandSlave;
            public byte Danwei1;
            public byte Danwei2;
            public byte Danwei3;
            public uint Mubiaopihao;
            public byte Mubiaotdh;
            public byte FireState;
            public byte AbsolutetimeH;
            public byte AbsolutetimeM;
            public byte AbsolutetimeL;
        }
        TrackState TracState = new TrackState();

        // 1HZ发送
        public void SendTrackState(MAVState leader, int controlState, int mavCount, byte fireState)
        {
            uint srcAddress;
            uint destAddress;
            UInt32 Absolutetime = (UInt32)((DateTime.UtcNow - DateTime.UtcNow.Date).TotalMilliseconds / 10);
            TracState.FireState = fireState;
            // 满足以下两种情况之一就发送：
            // 1. 时间超过 1 秒（每秒发送一次）
            // 2. FireState == 5（立即发送）
            if (DateTime.Now >= TrackingStateSendNext || TracState.FireState == 5)
            {
                return;
            }
            TrackingStateSendNext = DateTime.Now.AddSeconds(1.0d);
            srcAddress = mpAddress;
            destAddress = commandAddress;
            TracState.Baotou = 02;
            TracState.Xinsu[0] = 01;
            TracState.Xinsu[1] = 30;
            TracState.Xinsu[2] = 63;
            TracState.Xinyuan[0] = 81;
            TracState.Xinyuan[1] = 31;
            TracState.Xinyuan[2] = 63;
            TracState.Zhihui = 0x21e6;
            TracState.commandMain = 0xe1;
            TracState.commandSlave = 0x0c;
            TracState.Danwei1 = 51;
            TracState.Danwei2 = 31;
            TracState.Danwei3 = 63;
            TracState.Mubiaopihao = 0x00002710;
            TracState.Mubiaotdh = 01;
            TracState.AbsolutetimeH = (byte)((Absolutetime >> 16) & 0xFF);
            TracState.AbsolutetimeM = (byte)((Absolutetime >> 8) & 0xFF);
            TracState.AbsolutetimeL = (byte)(Absolutetime & 0xFF);

            byte[] writeData = MavlinkUtil.StructureToByteArray(TracState);
            if (comPort != null)
            {
                comPort.Write(writeData, 0, writeData.Length);
            }
        }

        //火力设备状态发送
        DateTime EquipmentStateSendNext = DateTime.Now;
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct EquipState
        {
            public byte Baotou;
            public byte[] Xinsu;
            public byte[] Xinyuan;
            public ushort Zhihui;
            public byte commandMain;
            public byte commandSlave;
            public byte Danwei1;
            public byte Danwei2;
            public byte Danwei3;
            public byte systemState;
            public int equLongitude;
            public int equLatitude;
            public int equAltitude;
            public byte messNum;
            public byte messTotal;
            public byte TanceNum;
            public short fireType;
            public int fireEquNum;
            public int fireLongitude;
            public int fireLatitude;
            public int fireAltitude;
            public byte fireState;
            public byte fireworkState;
            public double fireAzimuth;
            public double firePitch;
            public byte fireNum;
            public byte AbsolutetimeH;
            public byte AbsolutetimeM;
            public byte AbsolutetimeL;
        }
        EquipState EquState = new EquipState();

        // 1HZ发送
        public void SendEquipmentState(MAVState leader, int controlState, int mavCount)
        {
            uint srcAddress;
            uint destAddress;
            UInt32 Absolutetime = (UInt32)((DateTime.UtcNow - DateTime.UtcNow.Date).TotalMilliseconds / 10);
            if (DateTime.Now < EquipmentStateSendNext)
            {
                return;
            }
            EquipmentStateSendNext = DateTime.Now.AddSeconds(1.0d);
            srcAddress = mpAddress;
            destAddress = commandAddress;
            EquState.Baotou = 02;
            EquState.Xinsu[0] = 01;
            EquState.Xinsu[1] = 30;
            EquState.Xinsu[2] = 63;
            EquState.Xinyuan[0] = 81;
            EquState.Xinyuan[1] = 31;
            EquState.Xinyuan[2] = 63;
            EquState.Zhihui = 0x0551;
            EquState.commandMain = 0xaa;
            EquState.commandSlave = 0x12;
            EquState.Danwei1 = 51;
            EquState.Danwei2 = 31;
            EquState.Danwei3 = 63;
            EquState.systemState = 1;
            EquState.equLongitude = (int)MainV2.comPort.MAV.cs.Location.Lng * 360;
            EquState.equLatitude = (int)MainV2.comPort.MAV.cs.Location.Lat * 360;
            EquState.equAltitude = (int)MainV2.comPort.MAV.cs.Location.Alt;
            EquState.messNum = 1;
            EquState.messNum = 1;
            EquState.TanceNum = 1;
            EquState.fireType = 0x55;
            EquState.fireEquNum = 0x0551;
            EquState.fireLongitude = targetLJMessage.longitude;
            EquState.fireLatitude = targetLJMessage.latitude;
            EquState.fireAltitude = targetLJMessage.alt;
            EquState.fireState = 1;
            EquState.fireworkState = 1;
            double deltaLat = EquState.fireLatitude - EquState.equLatitude;
            double deltaLng = EquState.fireLongitude - EquState.equLongitude;
            double a = Math.Pow(Math.Sin(deltaLat / 2), 2) +
                   Math.Cos(EquState.fireLatitude * Math.PI / 180) * Math.Cos(EquState.equLatitude * Math.PI / 180) * Math.Pow(Math.Sin(deltaLng / 2), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = 6378137 * c;
            EquState.fireAzimuth = Math.Atan(deltaLng / deltaLat);
            EquState.firePitch = Math.Atan((EquState.fireAltitude - EquState.equAltitude / distance));
            EquState.fireNum = 5;
            EquState.AbsolutetimeH = (byte)((Absolutetime >> 16) & 0xFF);
            EquState.AbsolutetimeM = (byte)((Absolutetime >> 8) & 0xFF);
            EquState.AbsolutetimeL = (byte)(Absolutetime & 0xFF);


            byte[] writeData = MavlinkUtil.StructureToByteArray(EquState);
            if (comPort != null)
            {
                comPort.Write(writeData, 0, writeData.Length);
            }
        }

        // 目标对应拦截机参数发送
        DateTime TargetStateSendNext = DateTime.Now;
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TargetState
        {
            public byte Baotou;
            public byte[] Xinsu;
            public byte[] Xinyuan;
            public ushort Zhihui;
            public byte commandMain;
            public byte commandSlave;
            public byte Danwei1;
            public byte Danwei2;
            public byte Danwei3;
            public byte raderNum;
            public int raderID;
            public byte Beifen1;
            public byte Beifen2;
            public byte Beifen3;
            public byte InterNum;
            public byte InterState;
            public byte InterShot;
            public byte InterEnco;
            public double distance;
            public double distance1;
            public double distance2;
            public byte HeiErr;
            public byte AziErr;
            public byte LotNum;
            public byte AbsolutetimeH;
            public byte AbsolutetimeM;
            public byte AbsolutetimeL;
        }
        TargetState TargState = new TargetState();

        // 1HZ发送
        public void SendTargetState(MAVState leader, int controlState, int mavCount)
        {
            uint srcAddress;
            uint destAddress;
            UInt32 Absolutetime = (UInt32)((DateTime.UtcNow - DateTime.UtcNow.Date).TotalMilliseconds / 10);
            if (DateTime.Now < TargetStateSendNext)
            {
                return;
            }
            TargetStateSendNext = DateTime.Now.AddSeconds(1.0d);
            srcAddress = mpAddress;
            destAddress = commandAddress;
            TargState.Baotou = 02;
            TargState.Xinsu[0] = 01;
            TargState.Xinsu[1] = 30;
            TargState.Xinsu[2] = 63;
            TargState.Xinyuan[0] = 51;
            TargState.Xinyuan[1] = 31;
            TargState.Xinyuan[2] = 63;
            TargState.Zhihui = 0x0551;
            TargState.commandMain = 0xaa;
            TargState.commandSlave = 0x05;
            TargState.Danwei1 = 51;
            TargState.Danwei2 = 31;
            TargState.Danwei3 = 63;
            TargState.raderNum = 1;
            TargState.raderID = 0x0110;
            TargState.Beifen1 = 0;
            TargState.Beifen2 = 0;
            TargState.Beifen3 = 0;
            TargState.InterNum = 1;
            TargState.InterState = 1;
            TargState.InterShot = 1;
            TargState.InterEnco = 2;
            double deltaLat = targetLJMessage.latitude - MainV2.comPort.MAV.cs.Location.Lat;
            double deltaLng = targetLJMessage.longitude - MainV2.comPort.MAV.cs.Location.Lng;
            double a = Math.Pow(Math.Sin(deltaLat / 2), 2) +
                   Math.Cos(targetLJMessage.latitude * Math.PI / 180) * Math.Cos(EquState.equLatitude * Math.PI / 180) * Math.Pow(Math.Sin(deltaLng / 2), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = 6378137 * c;
            TargState.distance = Math.Sqrt(Math.Pow(distance, 2) + Math.Pow(targetLJMessage.alt - MainV2.comPort.MAV.cs.Location.Alt, 2));
            TargState.distance1 = TargState.distance * Math.Sin(MainV2.comPort.MAV.cs.yaw - EquState.fireAzimuth);
            TargState.distance2 = (targetLJMessage.alt - MainV2.comPort.MAV.cs.Location.Alt);
            TargState.HeiErr = 0;
            TargState.AziErr = 0;
            TargState.LotNum = 1;
            TargState.AbsolutetimeH = (byte)((Absolutetime >> 16) & 0xFF);
            TargState.AbsolutetimeM = (byte)((Absolutetime >> 8) & 0xFF);
            TargState.AbsolutetimeL = (byte)(Absolutetime & 0xFF);



            byte[] writeData = MavlinkUtil.StructureToByteArray(CommState);
            if (comPort != null)
            {
                comPort.Write(writeData, 0, writeData.Length);
            }
        }

        DateTime gcsStateSendNext = DateTime.Now;
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct GcsState
        {
            public uint srcAddress;
            public uint destAddress;
            public UInt64 sendTimeStamp;
            public ushort mesSeq;
            public ushort mesLength;
            public byte commandMain;
            public byte commandSlave;
            public byte controlState;
            public byte droneCount;
            public byte leaderMask;
            public int leaderLon;
            public int leaderLat;
            public int leaderAlt;
            public byte crc;
        }
        GcsState gcsState = new GcsState();

        // 1HZ发送
        public void SendMPState(MAVState leader, int controlState, int mavCount)
        {
            if (DateTime.Now < gcsStateSendNext)
            {
                return;
            }
            gcsStateSendNext = DateTime.Now.AddSeconds(1.0d);
            gcsState.srcAddress = mpAddress;
            gcsState.destAddress = commandAddress;
            gcsState.sendTimeStamp = (UInt64)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds);
            gcsState.mesSeq = 0;
            gcsState.mesLength = 17;
            gcsState.commandMain = 0xA4;
            gcsState.commandSlave = 0;
            gcsState.controlState = (byte)controlState;
            gcsState.droneCount = (byte)mavCount;
            if (leader == null)
            {
                gcsState.leaderMask = 0;
                gcsState.leaderLon = 0;
                gcsState.leaderLat = 0;
                gcsState.leaderAlt = 0;
            }
            else
            {
                gcsState.leaderMask = 1;
                gcsState.leaderLon = (int)(leader.cs.lng * 1e7);
                gcsState.leaderLat = (int)(leader.cs.lat * 1e7);
                gcsState.leaderAlt = (int)(leader.cs.alt * 1e2);
            }
            gcsState.crc = 0;
            // 计算crc
            byte[] dataBytes;
            dataBytes = BitConverter.GetBytes(gcsState.mesLength);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                gcsState.crc += dataBytes[i];
            }
            gcsState.crc += gcsState.commandMain;
            gcsState.crc += gcsState.commandSlave;
            gcsState.crc += gcsState.controlState;
            gcsState.crc += gcsState.droneCount;
            gcsState.crc += gcsState.leaderMask;
            dataBytes = BitConverter.GetBytes(gcsState.leaderLon);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                gcsState.crc += dataBytes[i];
            }
            dataBytes = BitConverter.GetBytes(gcsState.leaderLat);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                gcsState.crc += dataBytes[i];
            }
            dataBytes = BitConverter.GetBytes(gcsState.leaderAlt);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                gcsState.crc += dataBytes[i];
            }
            byte[] writeData = MavlinkUtil.StructureToByteArray(gcsState);
            if (comPort != null)
            {
                comPort.Write(writeData, 0, writeData.Length);
            }
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SwarmState
        {
            public uint srcAddress;
            public uint destAddress;
            public UInt64 sendTimeStamp;
            public ushort mesSeq;
            public ushort mesLength;
            public byte commandMain;
            public byte commandSlave;
            public byte swarmShape;
            public byte droneCount;
            public byte swarmDistance;
            public byte swarmRadius;
            public int leaderLon;
            public int leaderLat;
            public int leaderAlt;
            public ushort leaderYaw;
            public ushort inAirTime;
            public int mavID;
            public byte isLeader;
            public byte armState;
            public int batVol;
            public int mavLon;
            public int mavLat;
            public int mavAlt;
            public int mavPitch;
            public int mavYaw;
            public int mavRoll;
            public int mavSpeed;
            public byte crc;
        }
        SwarmState swarmState = new SwarmState();

        // 10HZ发送
        public void SendSwamState(MAVState leader, MAVState mav_current, string swarmShape, int mavCount, int swarmDistance)
        {
            swarmState.srcAddress = mpAddress;
            swarmState.destAddress = commandAddress;
            swarmState.sendTimeStamp = (UInt64)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds);
            swarmState.mesSeq = 0;
            swarmState.mesLength = 59; //22
            swarmState.commandMain = 0xA5;
            swarmState.commandSlave = 0;
            swarmState.swarmShape = GetSwarmShape(swarmShape);
            swarmState.droneCount = (byte)mavCount;
            swarmState.swarmDistance = (byte)swarmDistance;
            swarmState.swarmRadius = 0;
            if (leader == null)
            {
                swarmState.leaderLon = 0;
                swarmState.leaderLat = 0;
                swarmState.leaderAlt = 0;
                swarmState.leaderYaw = 0;
            }
            else
            {
                swarmState.leaderLon = (int)(leader.cs.lng * 1e7);
                swarmState.leaderLat = (int)(leader.cs.lat * 1e7);
                swarmState.leaderAlt = (int)(leader.cs.alt * 1e2);
                swarmState.leaderYaw = (ushort)(leader.cs.yaw * 1e2);
            }
            swarmState.inAirTime = 0;

            if (mav_current == null)
            {
                swarmState.mavID = 0;
                swarmState.isLeader = 0;
                swarmState.armState = 0;
                swarmState.batVol = 0;
                swarmState.mavLon = 0;
                swarmState.mavLat = 0;
                swarmState.mavAlt = 0;
                swarmState.mavPitch = 0;
                swarmState.mavYaw = 0;
                swarmState.mavRoll = 0;
                swarmState.mavSpeed = 0;
            }
            else
            {
                swarmState.mavID = (int)(mav_current.sysid);
                swarmState.isLeader = 0;
                if (mav_current.cs.armed)
                {
                    swarmState.armState = 1;
                }
                else
                {
                    swarmState.armState = 0;
                }
                swarmState.batVol = (int)(mav_current.cs.battery_voltage * 1e2);
                swarmState.mavLon = (int)(mav_current.cs.lng * 1e7);
                swarmState.mavLat = (int)(mav_current.cs.lat * 1e7);
                swarmState.mavAlt = (int)(mav_current.cs.alt * 1e2);
                swarmState.mavPitch = (int)(mav_current.cs.pitch * 1e2);
                swarmState.mavYaw = (int)(mav_current.cs.yaw * 1e2);
                swarmState.mavRoll = (int)(mav_current.cs.roll * 1e2);
                swarmState.mavSpeed = (int)(mav_current.cs.groundspeed * 1e2);
            }

            swarmState.crc = 0;
            // 计算crc
            byte[] dataBytes;
            dataBytes = BitConverter.GetBytes(swarmState.mesLength);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                swarmState.crc += dataBytes[i];
            }
            swarmState.crc += swarmState.commandMain;
            swarmState.crc += swarmState.commandSlave;
            swarmState.crc += swarmState.swarmShape;
            swarmState.crc += swarmState.droneCount;
            swarmState.crc += swarmState.swarmDistance;
            swarmState.crc += swarmState.swarmRadius;

            dataBytes = BitConverter.GetBytes(swarmState.leaderLon);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                swarmState.crc += dataBytes[i];
            }
            dataBytes = BitConverter.GetBytes(swarmState.leaderLat);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                swarmState.crc += dataBytes[i];
            }
            dataBytes = BitConverter.GetBytes(swarmState.leaderAlt);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                swarmState.crc += dataBytes[i];
            }
            dataBytes = BitConverter.GetBytes(swarmState.leaderYaw);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                swarmState.crc += dataBytes[i];
            }
            dataBytes = BitConverter.GetBytes(swarmState.inAirTime);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                swarmState.crc += dataBytes[i];
            }
            dataBytes = BitConverter.GetBytes(swarmState.mavID);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                swarmState.crc += dataBytes[i];
            }

            swarmState.crc += swarmState.isLeader;
            swarmState.crc += swarmState.armState;

            dataBytes = BitConverter.GetBytes(swarmState.batVol);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                swarmState.crc += dataBytes[i];
            }
            dataBytes = BitConverter.GetBytes(swarmState.mavLon);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                swarmState.crc += dataBytes[i];
            }
            dataBytes = BitConverter.GetBytes(swarmState.mavLat);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                swarmState.crc += dataBytes[i];
            }
            dataBytes = BitConverter.GetBytes(swarmState.mavAlt);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                swarmState.crc += dataBytes[i];
            }
            dataBytes = BitConverter.GetBytes(swarmState.mavPitch);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                swarmState.crc += dataBytes[i];
            }
            dataBytes = BitConverter.GetBytes(swarmState.mavYaw);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                swarmState.crc += dataBytes[i];
            }
            dataBytes = BitConverter.GetBytes(swarmState.mavRoll);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                swarmState.crc += dataBytes[i];
            }
            dataBytes = BitConverter.GetBytes(swarmState.mavSpeed);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                swarmState.crc += dataBytes[i];
            }

            byte[] writeData = MavlinkUtil.StructureToByteArray(swarmState);
            if (comPort != null)
            {
                comPort.Write(writeData, 0, writeData.Length);
            }
        }

        byte GetSwarmShape(string swarmShape)
        {
            byte result = 0;
            if (swarmShape == "竖网形")
            {
                result = 1;
            }
            else if (swarmShape == "斜网形")
            {
                result = 2;
            }
            else if (swarmShape == "横网形")
            {
                result = 3;
            }
            else
            {
                result = 0;
            }
            return result;
        }

        public PointLatLngAlt GetInterceptPoint()
        {
            return new PointLatLngAlt(targetLJMessage.latitude / 1e7, targetLJMessage.longitude / 1e7, targetLJMessage.alt / 1e2);
        }

        public DateTime GetLastReadTime()
        {
            return lastReadTime;
        }

        public byte[] GetSwarmStateByteArray()
        {
            return MavlinkUtil.StructureToByteArray(swarmState);
        }
    }


}
