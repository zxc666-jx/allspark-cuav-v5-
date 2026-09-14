using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using MissionPlanner.Utilities;

namespace MissionPlanner.HIL
{
    public class XPlane : Hil, IDisposable
    {
        Socket SimulatorRECV;
        UdpClient XplanesSEND;
        EndPoint Remote = (EndPoint) (new IPEndPoint(IPAddress.Any, 0));

        // place to store the xplane packet data
        float[][] DATA = new float[113][];

        public override void SetupSockets(int recvPort, int SendPort, string simIP)
        {
            // setup receiver
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, recvPort);

            SimulatorRECV = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram, ProtocolType.Udp);

            SimulatorRECV.Bind(ipep);

            UpdateStatus(-1, "Listerning on port UDP " + recvPort + " (sim->planner)\n");


            // setup sender
            XplanesSEND = new UdpClient(simIP, SendPort);

            UpdateStatus(-1, "Sending to port UDP " + SendPort + " (planner->sim)\n");

            setupXplane();

            UpdateStatus(-1, "Sent xplane settings\n");
        }

        public override void Shutdown()
        {
            try
            {
                SimulatorRECV.Close();
            }
            catch
            {
            }
            try
            {
                XplanesSEND.Close();
            }
            catch
            {
            }
        }

        //public override void GetFromSim(ref sitl_fdm sitldata)
        //{
        //    if (SimulatorRECV.Available > 0)
        //    {
        //        byte[] udpdata = new byte[1500];
        //        int receviedbytes = 0;
        //        try
        //        {
        //            while (SimulatorRECV.Available > 0)
        //            {
        //                receviedbytes = SimulatorRECV.ReceiveFrom(udpdata, ref Remote);
        //            }
        //        }
        //        catch
        //        {
        //        }

        //        if (udpdata[0] == 'D' && udpdata[1] == 'A')
        //        {
        //            // Xplanes sends
        //            // 5 byte header
        //            // 1 int for the index - numbers on left of output
        //            // 8 floats - might be useful. or 0 if not
        //            int count = 5;
        //            while (count < receviedbytes)
        //            {
        //                int index = BitConverter.ToInt32(udpdata, count);

        //                DATA[index] = new float[8];

        //                DATA[index][0] = BitConverter.ToSingle(udpdata, count + 1*4);
        //                ;
        //                DATA[index][1] = BitConverter.ToSingle(udpdata, count + 2*4);
        //                ;
        //                DATA[index][2] = BitConverter.ToSingle(udpdata, count + 3*4);
        //                ;
        //                DATA[index][3] = BitConverter.ToSingle(udpdata, count + 4*4);
        //                ;
        //                DATA[index][4] = BitConverter.ToSingle(udpdata, count + 5*4);
        //                ;
        //                DATA[index][5] = BitConverter.ToSingle(udpdata, count + 6*4);
        //                ;
        //                DATA[index][6] = BitConverter.ToSingle(udpdata, count + 7*4);
        //                ;
        //                DATA[index][7] = BitConverter.ToSingle(udpdata, count + 8*4);
        //                ;

        //                count += 36; // 8 * float
        //            }

        //            bool xplane9 = !xplane10;

        //            if (xplane9)
        //            {
        //                sitldata.pitchDeg = (DATA[18][0]);
        //                sitldata.rollDeg = (DATA[18][1]);
        //                sitldata.yawDeg = (DATA[18][2]);
        //                sitldata.pitchRate = (DATA[17][0]*MathHelper.rad2deg);
        //                sitldata.rollRate = (DATA[17][1]*MathHelper.rad2deg);
        //                sitldata.yawRate = (DATA[17][2]*MathHelper.rad2deg);

        //                sitldata.heading = ((float) DATA[19][2]);
        //            }
        //            else
        //            {
        //                sitldata.pitchDeg = (DATA[17][0]);
        //                sitldata.rollDeg = (DATA[17][1]);
        //                sitldata.yawDeg = (DATA[17][2]);
        //                sitldata.pitchRate = (DATA[16][0]*MathHelper.rad2deg);
        //                sitldata.rollRate = (DATA[16][1]*MathHelper.rad2deg);
        //                sitldata.yawRate = (DATA[16][2]*MathHelper.rad2deg);

        //                sitldata.heading = (DATA[18][2]);
        //            }

        //            sitldata.airspeed = ((DATA[3][5]*.44704));

        //            sitldata.latitude = (DATA[20][0]);
        //            sitldata.longitude = (DATA[20][1]);
        //            sitldata.altitude = (DATA[20][2]*ft2m);

        //            sitldata.speedN = DATA[21][3]; // (DATA[3][7] * 0.44704 * Math.Sin(sitldata.heading * MathHelper.deg2rad));
        //            sitldata.speedE = -DATA[21][5]; // (DATA[3][7] * 0.44704 * Math.Cos(sitldata.heading * MathHelper.deg2rad));

        //            Matrix3 dcm = new Matrix3();
        //            dcm.from_euler(sitldata.rollDeg*MathHelper.deg2rad, sitldata.pitchDeg*MathHelper.deg2rad, sitldata.yawDeg*MathHelper.deg2rad);

        //            // rad = tas^2 / (tan(angle) * G)
        //            float turnrad =
        //                (float)
        //                    (((DATA[3][7]*0.44704)*(DATA[3][7]*0.44704))/
        //                     (float) (9.8f*Math.Tan(sitldata.rollDeg*MathHelper.deg2rad)));

        //            float gload = (float) (1/Math.Cos(sitldata.rollDeg*MathHelper.deg2rad)); // calculated Gs

        //            // a = v^2/r
        //            float centripaccel = (float) ((DATA[3][7]*0.44704)*(DATA[3][7]*0.44704))/turnrad;

        //            Vector3 accel_body = dcm.transposed()*(new Vector3(0, 0, -9.8));

        //            Vector3 centrip_accel = new Vector3(0, centripaccel*Math.Cos(sitldata.rollDeg*MathHelper.deg2rad),
        //                centripaccel*Math.Sin(sitldata.rollDeg*MathHelper.deg2rad));

        //            accel_body -= centrip_accel;

        //            sitldata.xAccel = DATA[4][5]*9.8;
        //            sitldata.yAccel = DATA[4][6]*9.8;
        //            sitldata.zAccel = (0 - DATA[4][4])*9.8;

        //            //      Console.WriteLine(accel_body.ToString());
        //            //      Console.WriteLine("        {0} {1} {2}",sitldata.xAccel, sitldata.yAccel, sitldata.zAccel);
        //        }
        //    }
        //}
        public override void GetFromSim()
        {
            if (SimulatorRECV.Available > 0)
            {
                byte[] udpdata = new byte[1500];
                int receviedbytes = 0;
                try
                {
                    while (SimulatorRECV.Available > 0)
                    {
                        receviedbytes = SimulatorRECV.ReceiveFrom(udpdata, ref Remote);
                    }
                }
                catch
                {
                }
                if (!(udpdata[0] == 'D' && udpdata[1] == 'A'&& udpdata[2] == 'T'&& udpdata[3] == 'A') || receviedbytes<37)
                {
                    //setupXplane();
                    return;
                }
                if (udpdata[0] == 'D' && udpdata[1] == 'A' && udpdata[2] == 'T' && udpdata[3] == 'A')
                {
                    // Xplanes sends
                    // 5 byte header
                    // 1 int for the index - numbers on left of output
                    // 8 floats - might be useful. or 0 if not
                    int count = 5;
                    while (count < receviedbytes)
                    {
                        int index = BitConverter.ToInt32(udpdata, count);

                        DATA[index] = new float[8];

                        DATA[index][0] = BitConverter.ToSingle(udpdata, count + 1 * 4);
                        ;
                        DATA[index][1] = BitConverter.ToSingle(udpdata, count + 2 * 4);
                        ;
                        DATA[index][2] = BitConverter.ToSingle(udpdata, count + 3 * 4);
                        ;
                        DATA[index][3] = BitConverter.ToSingle(udpdata, count + 4 * 4);
                        ;
                        DATA[index][4] = BitConverter.ToSingle(udpdata, count + 5 * 4);
                        ;
                        DATA[index][5] = BitConverter.ToSingle(udpdata, count + 6 * 4);
                        ;
                        DATA[index][6] = BitConverter.ToSingle(udpdata, count + 7 * 4);
                        ;
                        DATA[index][7] = BitConverter.ToSingle(udpdata, count + 8 * 4);
                        ;

                        count += 36; // 8 * float


                    }

                    bool xplane9 = !xplane10;

                    if (xplane9)
                    {
                        sitl_data.pitchDeg = (DATA[18][0]);
                        sitl_data.rollDeg = (DATA[18][1]);
                        sitl_data.yawDeg = (DATA[18][2]);
                        sitl_data.pitchRate = (DATA[17][0] * MathHelper.rad2deg);
                        sitl_data.rollRate = (DATA[17][1] * MathHelper.rad2deg);
                        sitl_data.yawRate = (DATA[17][2] * MathHelper.rad2deg);

                        sitl_data.heading = ((float)DATA[19][2]);
                    }
                    else
                    {
                        sitl_data.pitchDeg = (DATA[17][0]);
                        sitl_data.rollDeg = (DATA[17][1]);
                        sitl_data.yawDeg = (DATA[17][2]);
                        sitl_data.pitchRate = (DATA[16][0] * MathHelper.rad2deg);
                        sitl_data.rollRate = (DATA[16][1] * MathHelper.rad2deg);
                        sitl_data.yawRate = (DATA[16][2] * MathHelper.rad2deg);
                    }

                    sitl_data.airspeed = ((DATA[3][1] * 0.51444));

                    sitl_data.latitude = (DATA[20][0]);
                    sitl_data.longitude = (DATA[20][1]);
                    sitl_data.altitude = (DATA[20][2] * ft2m);

                    sitl_data.speedN = -DATA[21][5];
                    sitl_data.speedE = DATA[21][3];
                    sitl_data.speedD = -DATA[21][4];

                    sitl_data.xAccel = DATA[4][5] * gravity_mss;
                    sitl_data.yAccel = DATA[4][6] * gravity_mss;
                    sitl_data.zAccel = -DATA[4][4] * gravity_mss;
                }
            }
        }
        public override void SendToSim()
        {
            roll_out = (float)MainV2.comPort.MAV.cs.hilch1 / rollgain;
            pitch_out = (float)MainV2.comPort.MAV.cs.hilch2 / pitchgain;
            throttle_out = ((float)MainV2.comPort.MAV.cs.hilch3) / throttlegain;
            rudder_out = (float)MainV2.comPort.MAV.cs.hilch4 / ruddergain;
            roll_out *= REV_roll;
            pitch_out *= REV_pitch;
            rudder_out *= REV_rudder;

            // Limit min and max
            roll_out = Constrain(roll_out, -1, 1);
            pitch_out = Constrain(pitch_out, -1, 1);
            rudder_out = Constrain(rudder_out, -1, 1);
            throttle_out = Constrain(throttle_out, 0, 1);

            // sending only 1 packet instead of many.

            byte[] XplaneThr = new byte[5 + 4 + 32];

            XplaneThr[0] = (byte)'D';
            XplaneThr[1] = (byte)'A';
            XplaneThr[2] = (byte)'T';
            XplaneThr[3] = (byte)'A';
            XplaneThr[4] = (byte)'0';

            Array.Copy(BitConverter.GetBytes((int)25), 0, XplaneThr, 5, 4); // packet index

            Array.Copy(BitConverter.GetBytes((float)throttle_out), 0, XplaneThr, 9, 4); // start data
            Array.Copy(BitConverter.GetBytes((float)throttle_out), 0, XplaneThr, 13, 4);
            Array.Copy(BitConverter.GetBytes((float)throttle_out), 0, XplaneThr, 17, 4);
            Array.Copy(BitConverter.GetBytes((float)throttle_out), 0, XplaneThr, 21, 4);

            Array.Copy(BitConverter.GetBytes((float)0), 0, XplaneThr, 25, 4);
            Array.Copy(BitConverter.GetBytes((float)0), 0, XplaneThr, 29, 4);
            Array.Copy(BitConverter.GetBytes((float)0), 0, XplaneThr, 33, 4);
            Array.Copy(BitConverter.GetBytes((float)0), 0, XplaneThr, 37, 4);

            try
            {
                XplanesSEND.Send(XplaneThr, XplaneThr.Length);
            }
            catch (Exception e)
            {
                log.Info("Xplanes udp send error " + e.Message);
            }

            byte[] XplaneAtt = new byte[5 + 4 + 32];
            XplaneAtt[0] = (byte)'D';
            XplaneAtt[1] = (byte)'A';
            XplaneAtt[2] = (byte)'T';
            XplaneAtt[3] = (byte)'A';
            XplaneAtt[4] = (byte)'0';
            // NEXT ONE - control surfaces

            Array.Copy(BitConverter.GetBytes((int)11), 0, XplaneAtt, 5, 4); // packet index

            Array.Copy(BitConverter.GetBytes((float)(pitch_out)), 0, XplaneAtt, 9, 4); // start data
            Array.Copy(BitConverter.GetBytes((float)(roll_out)), 0, XplaneAtt, 13, 4);
            Array.Copy(BitConverter.GetBytes((float)(rudder_out)), 0, XplaneAtt, 17, 4);
            Array.Copy(BitConverter.GetBytes((float)0), 0, XplaneAtt, 21, 4);

            Array.Copy(BitConverter.GetBytes((float)(rudder_out)), 0, XplaneAtt, 25, 4);
            Array.Copy(BitConverter.GetBytes((float)0), 0, XplaneAtt, 29, 4);
            Array.Copy(BitConverter.GetBytes((float)0), 0, XplaneAtt, 33, 4);
            Array.Copy(BitConverter.GetBytes((float)0), 0, XplaneAtt, 37, 4);

            try
            {
                XplanesSEND.Send(XplaneAtt, XplaneAtt.Length);
            }
            catch (Exception e)
            {
                log.Info("Xplanes udp send error " + e.Message);
            }
        }

        public override void SendToAP(MAVLinkInterface port, MAVState mav)
        {
            TimeSpan gpsspan = DateTime.Now - lastgpsupdate;

            // add gps delay
            if (gpsspan.TotalMilliseconds >= GPS_rate)
            {
                lastgpsupdate = DateTime.Now;

                // save current fix = 3
                sitl_fdmbuffer[gpsbufferindex%sitl_fdmbuffer.Length] = sitl_data;

                //                Console.WriteLine((gpsbufferindex % gpsbuffer.Length) + " " + ((gpsbufferindex + (gpsbuffer.Length - 1)) % gpsbuffer.Length));

                // return buffer index + 5 = (3 + 5) = 8 % 6 = 2
                oldgps = sitl_fdmbuffer[(gpsbufferindex + (sitl_fdmbuffer.Length - 1))%sitl_fdmbuffer.Length];

                //comPort.sendPacket(oldgps);

                gpsbufferindex++;
            }

            MAVLink.mavlink_hil_state_t hilstate = new MAVLink.mavlink_hil_state_t();

            hilstate.time_usec = (UInt64) DateTime.Now.Ticks; // microsec

            hilstate.lat = (int) (oldgps.latitude*1e7); // * 1E7
            hilstate.lon = (int) (oldgps.longitude*1e7); // * 1E7
            hilstate.alt = (int) (oldgps.altitude*1000); // mm

            //   Console.WriteLine(hilstate.alt);

            hilstate.pitch = (float)(sitl_data.pitchDeg*MathHelper.deg2rad); // (rad)
            hilstate.pitchspeed = (float)(sitl_data.pitchRate*MathHelper.deg2rad); // (rad/s)
            hilstate.roll = (float)(sitl_data.rollDeg*MathHelper.deg2rad); // (rad)
            hilstate.rollspeed = (float)(sitl_data.rollRate*MathHelper.deg2rad); // (rad/s)
            hilstate.yaw = (float)(sitl_data.yawDeg*MathHelper.deg2rad); // (rad)
            hilstate.yawspeed = (float)(sitl_data.yawRate*MathHelper.deg2rad); // (rad/s)

            hilstate.vx = (short)(sitl_data.speedN * 100); // m/s * 100
            hilstate.vy = (short)(sitl_data.speedE * 100); // m/s * 100
            hilstate.vz = (short)(sitl_data.speedD * 100);// m/s * 100

            hilstate.xacc = (short) (sitl_data.xAccel*1000 / gravity_mss); // (mg)
            hilstate.yacc = (short) (sitl_data.yAccel*1000 / gravity_mss); // (mg)
            hilstate.zacc = (short) (sitl_data.zAccel*1000 / gravity_mss); // (mg)

            port.sendPacket(hilstate, mav.sysid, mav.compid);

            port.sendPacket(new MAVLink.mavlink_vfr_hud_t()
            {
                airspeed = (float)sitl_data.airspeed
            }, mav.sysid, mav.compid);
        }
        float airspeedone = 0;
        public void TestSendToAP()
        {
            MAVLink.mavlink_hil_state_t hilstate = new MAVLink.mavlink_hil_state_t();
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav.sysid == 1)
                    {
                        hilstate.time_usec = (UInt64)DateTime.Now.Ticks; // microsec

                        hilstate.lat = (int)(mav.cs.lat * 1e7); // * 1E7
                        hilstate.lon = (int)(mav.cs.lng * 1e7); // * 1E7
                        hilstate.alt = (int)(mav.cs.alt * 1000); // mm

                        //   Console.WriteLine(hilstate.alt);

                        hilstate.pitch = (float)(mav.cs.pitch * MathHelper.deg2rad); // (rad)
                        hilstate.pitchspeed = (float)(mav.cs.gy / 1000); // (rad/s)
                        hilstate.roll = (float)(mav.cs.roll * MathHelper.deg2rad); // (rad)
                        hilstate.rollspeed = (float)(mav.cs.gx / 1000); // (rad/s)
                        hilstate.yaw = (float)(mav.cs.yaw * MathHelper.deg2rad); // (rad)
                        hilstate.yawspeed = (float)(mav.cs.gz / 1000); // (rad/s)

                        hilstate.vx = (short)(mav.cs.vx * 100); // m/s * 100
                        hilstate.vy = (short)(mav.cs.vy * 100); // m/s * 100
                        hilstate.vz = (short)(mav.cs.vz * 100); // m/s * 100

                        hilstate.xacc = (short)(mav.cs.ax); // (mg)
                        hilstate.yacc = (short)(mav.cs.ay); // (mg)
                        hilstate.zacc = (short)(mav.cs.az); // (mg)

                        airspeedone = mav.cs.airspeed;
                        //---------------------------------------------//
                        MAVLink.mavlink_rc_channels_override_t rc = new MAVLink.mavlink_rc_channels_override_t();
                        mav.cs.rcoverridech1 = (short)MapInputToOutput(roll_out,-1,1,1100,1900);
                        rc.chan1_raw = (ushort)mav.cs.rcoverridech1;
                        mav.cs.rcoverridech2 = (short)MapInputToOutput(pitch_out, -1, 1, 1100, 1900);
                        rc.chan2_raw = (ushort)mav.cs.rcoverridech2;
                        mav.cs.rcoverridech3 = (short)MapInputToOutput(throttle_out, 0, 1, 1100, 1900);
                        rc.chan3_raw = (ushort)mav.cs.rcoverridech3;
                        mav.cs.rcoverridech4 = (short)MapInputToOutput(rudder_out, -1, 1, 1000, 1900);
                        rc.chan4_raw = (ushort)mav.cs.rcoverridech4;
                        rc.target_system = mav.sysid;
                        rc.target_component = mav.compid;
                        port.sendPacket(rc, rc.target_system, rc.target_component);
                    }
                }
            }
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav.sysid == 2)
                    {
                        port.sendPacket(hilstate, 2, 1);
                        roll_out = (float)mav.cs.hilch1 / rollgain;
                        pitch_out = (float)mav.cs.hilch2 / pitchgain;
                        throttle_out = ((float)mav.cs.hilch3) / throttlegain;
                        rudder_out = (float)mav.cs.hilch4 / ruddergain;
                    }
                }
            }
        }
        public double MapInputToOutput(double input, double inputmin, double inputmax,double outputmin,double outputmax)
        {
            return (input - inputmin) * (outputmax - outputmin) / (inputmax - inputmin) + outputmin;
        }
        public override void GetFromAP()
        {
        }

        public void MoveToPos(double lat, double lon, double alt, float roll, float pitch, float yaw)
        {
            // sending only 1 packet instead of many.

            byte[] Xplane = new byte[5 + 4 + 3*8 + 6*4];

            Xplane[0] = (byte) 'V';
            Xplane[1] = (byte) 'E';
            Xplane[2] = (byte) 'H';
            Xplane[3] = (byte) '1';
            Xplane[4] = 0;

            int pos = 5;
            Array.Copy(BitConverter.GetBytes((int) 0), 0, Xplane, pos, 4); // plane
            pos += 4;
            Array.Copy(BitConverter.GetBytes(lat), 0, Xplane, pos, 8);
            pos += 8;
            Array.Copy(BitConverter.GetBytes(lon), 0, Xplane, pos, 8);
            pos += 8;
            Array.Copy(BitConverter.GetBytes(alt), 0, Xplane, pos, 8);
            pos += 8;

            Array.Copy(BitConverter.GetBytes((float) yaw), 0, Xplane, pos, 4);
            pos += 4;
            Array.Copy(BitConverter.GetBytes((float) pitch), 0, Xplane, pos, 4);
            pos += 4;
            Array.Copy(BitConverter.GetBytes((float) roll), 0, Xplane, pos, 4);
            pos += 4;

            Array.Copy(BitConverter.GetBytes((float) 0f), 0, Xplane, pos, 4);
            pos += 4;
            Array.Copy(BitConverter.GetBytes((float) 0f), 0, Xplane, pos, 4);
            pos += 4;
            Array.Copy(BitConverter.GetBytes((float) 0f), 0, Xplane, pos, 4);
            pos += 4;

            try
            {
                XplanesSEND.Send(Xplane, Xplane.Length);
            }
            catch (Exception e)
            {
                log.Info("Xplanes udp send error " + e.Message);
            }
        }

        void setupXplane()
        {
            // sending only 1 packet instead of many.

            byte[] Xplane = new byte[5 + 4*8];

            Xplane[0] = (byte) 'D';
            Xplane[1] = (byte) 'S';
            Xplane[2] = (byte) 'E';
            Xplane[3] = (byte) 'L';
            Xplane[4] = 0;

            if (xplane10)
            {
                int pos = 5;
                Xplane[pos] = 0x3;
                pos += 4;
                Xplane[pos] = 0x4;
                pos += 4;
                Xplane[pos] = 0x6;
                pos += 4;
                Xplane[pos] = 0x10;
                pos += 4;
                Xplane[pos] = 0x11;
                pos += 4;
                Xplane[pos] = 0x12;
                pos += 4;
                Xplane[pos] = 0x14;
                pos += 4;
                Xplane[pos] = 0x15;
                pos += 4;
            }
            else
            {
                int pos = 5;
                Xplane[pos] = 0x3;
                pos += 4;
                Xplane[pos] = 0x4;
                pos += 4;
                Xplane[pos] = 0x6;
                pos += 4;
                Xplane[pos] = 0x11;
                pos += 4;
                Xplane[pos] = 0x12;
                pos += 4;
                Xplane[pos] = 0x13;
                pos += 4;
                Xplane[pos] = 0x14;
                pos += 4;
                Xplane[pos] = 0x15;
                pos += 4;
            }

            try
            {
                XplanesSEND.Send(Xplane, Xplane.Length);
            }
            catch (Exception e)
            {
                log.Info("Xplanes udp send error " + e.Message);
            }
        }

        public void Dispose()
        {
            if (SimulatorRECV != null)
                SimulatorRECV.Dispose();
        }
    }
}