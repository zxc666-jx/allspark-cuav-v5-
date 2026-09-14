using System;
using System.Collections.Generic;

using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;
using MissionPlanner.Utilities;
using MissionPlanner.ArduPilot;
using Vector3 = MissionPlanner.Utilities.Vector3;

namespace MissionPlanner.Swarm
{
    /// <summary>
    /// Follow the leader
    /// </summary>
    class Formation : Swarm
    {
        Dictionary<MAVState, Vector3> offsets = new Dictionary<MAVState, Vector3>();
        public Dictionary<MAVState, Vector3> grid2offsets = new Dictionary<MAVState, Vector3>();

        Dictionary<MAVState, Navigation> Navigations = new Dictionary<MAVState, Navigation>();

        public Dictionary<MAVState, Tuple<PID, PID, PID, PID,PID>> pids =
            new Dictionary<MAVState, Tuple<PID, PID, PID, PID,PID>>();

        private PointLatLngAlt masterpos = new PointLatLngAlt();

        public Avoidance avoidance = new Avoidance();
        /// <summary>
        /// 油动，针对改一机型
        /// </summary>
        public bool oilPower = true;
        public static Formation instance = null;
        public Formation()
        {
            instance = this;
        }
        public void setNavigation(MAVState mav)
        {
            Navigations[mav] = new Navigation();
        }
        public void setPowerType(bool oiltype)
        {
            oilPower = oiltype;
        }

        public void setOffsets(MAVState mav, double x, double y, double z)
        {
            offsets[mav] = new Vector3(x, y, z);
            log.Info(mav.ToString() + " " + offsets[mav].ToString());
        }
        public void setgrid2Offsets(MAVState mav, double x, double y, double z)
        {
            grid2offsets[mav] = new Vector3(x, y, z);
        }

        public Vector3 getOffsets(MAVState mav)
        {
            if (offsets.ContainsKey(mav))
            {
                return offsets[mav];
            }
            
            return new Vector3(offsets.Count, 0, 0);
        }
        public Vector3 getgrid2Offsets(MAVState mav)
        {
            if (grid2offsets.ContainsKey(mav))
            {
                return grid2offsets[mav];
            }

            return new Vector3(grid2offsets.Count, 0, 0);
        }

        public override void Update()
        {
            if (MainV2.comPort.MAV.cs.lat == 0 || MainV2.comPort.MAV.cs.lng == 0)
                return;

            if (Leader == null)
                Leader = MainV2.comPort.MAV;

            masterpos = new PointLatLngAlt(Leader.cs.lat, Leader.cs.lng, Leader.cs.alt, "");
        }

        double wrap_180(double input)
        {
            if (input > 180)
                return input - 360;
            if (input < -180)
                return input + 360;
            return input;
        }

        //convert Wgs84ConversionInfo to utm
        CoordinateTransformationFactory ctfac = new CoordinateTransformationFactory();

        GeographicCoordinateSystem wgs84 = GeographicCoordinateSystem.WGS84;
        public override void SendCommand()
        {
            if (masterpos.Lat == 0 || masterpos.Lng == 0)
                return;

            int a = 0;
            foreach (var port in MainV2.Comports.ToArray())
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == Leader)
                        continue;

                    PointLatLngAlt target = new PointLatLngAlt(masterpos);

                    try
                    {
                        int utmzone = (int) ((masterpos.Lng - -186.0)/6.0);

                        IProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(utmzone,
                            masterpos.Lat < 0 ? false : true);

                        ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(wgs84, utm);

                        double[] pll1 = {target.Lng, target.Lat};

                        double[] p1 = trans.MathTransform.Transform(pll1);

                        double heading = -Leader.cs.yaw;

                        double length = offsets[mav].length();

                        var x = ((Vector3) offsets[mav]).x;
                        var y = ((Vector3) offsets[mav]).y;

                        // add offsets to utm
                        p1[0] += x*Math.Cos(heading*MathHelper.deg2rad) - y*Math.Sin(heading*MathHelper.deg2rad);
                        p1[1] += x*Math.Sin(heading*MathHelper.deg2rad) + y*Math.Cos(heading*MathHelper.deg2rad);

                        // convert back to wgs84
                        IMathTransform inversedTransform = trans.MathTransform.Inverse();
                        double[] point = inversedTransform.Transform(p1);

                        target.Lat = point[1];
                        target.Lng = point[0];
                        target.Alt += ((Vector3) offsets[mav]).z;
                        // 避障
                        if (mav.obsvector.x != 0 || mav.obsvector.y != 0 || mav.obsvector.z != 0)
                        {
                            target = target.gps_offset(mav.obsvector.y, mav.obsvector.x);
                            if (mav.obsvector.z != 0)
                            {
                                target.Alt = mav.obsvector.z;
                            }
                        }
                        if (mav.cs.firmware == Firmwares.ArduPlane)
                        {
                            ArduplaneCommand(port, mav, target, Leader, (Vector3)offsets[mav]);
                            //if (Navigations.ContainsKey(mav))
                            //{
                            //    Navigations[mav].SetYaw(mav, Leader.cs.yaw);
                            //    Navigations[mav].SetSpeed(mav, Leader.cs.groundspeed);
                            //    Navigations[mav].ArduplaneL1Command(port, mav, target);
                            //}
                            //ArduplanePIDCommand(port, mav, target, Leader);
                        }
                        else
                        {
                            ArducopterCommand(port, mav, target, Leader);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to send command " + mav.ToString() + "\n" + ex.ToString());
                    }

                    a++;
                }
            }
        }
        public void ArduplaneCommand(MAVLinkInterface port,MAVState mav, PointLatLngAlt target, MAVState leader,Vector3 offset)
        {
            // 最小高度30M
            target.Alt = Math.Max(30, target.Alt);
            // get distance from target position
            var dist = target.GetDistance(mav.cs.Location);

            // get bearing to target
            var targyaw = mav.cs.Location.GetBearing(target);

            //var targettrailer = target.newpos(Leader.cs.yaw, Math.Abs(dist) * -0.25);
            //var targetleader = target.newpos(Leader.cs.yaw, 10 + dist);
            var targettrailer = target.newpos(leader.cs.yaw, Math.Abs(dist) * -mav.sawmAdvancedParameters.targettrailerscale);
            var targetleader = target.newpos(leader.cs.yaw, mav.sawmAdvancedParameters.targetleaderdis_m + dist);

            var yawerror = wrap_180(targyaw - mav.cs.yaw);
            var mavleadererror = wrap_180(leader.cs.yaw - mav.cs.yaw);
            // dist<100
            if (dist < mav.sawmAdvancedParameters.yawdistanceboundary_m)
            {
                targyaw = mav.cs.Location.GetBearing(targetleader);
                yawerror = wrap_180(targyaw - mav.cs.yaw);

                var targBearing = mav.cs.Location.GetBearing(target);

                // check the bearing for the leader and target are within 45 degrees.
                if (Math.Abs(wrap_180(targBearing - targyaw)) > 45)
                    dist *= -1;
            }
            else
            {
                targyaw = mav.cs.Location.GetBearing(targettrailer);
                yawerror = wrap_180(targyaw - mav.cs.yaw);
            }

            // display update
            mav.GuidedMode.x = (float)target.Lat;
            mav.GuidedMode.y = (float)target.Lng;
            mav.GuidedMode.z = (float)target.Alt;

            MAVLink.mavlink_set_attitude_target_t att_target = new MAVLink.mavlink_set_attitude_target_t();
            att_target.target_system = mav.sysid;
            att_target.target_component = mav.compid;
            att_target.type_mask = 0xff;

            Tuple<PID, PID, PID, PID, PID> pid;

            if (pids.ContainsKey(mav))
            {
                pid = pids[mav];
            }
            else
            {
                pid = new Tuple<PID, PID, PID, PID, PID>(
                    new PID(1f, .03f, 0.02f, 10, 20, 0.1f, 0),
                    new PID(1f, .03f, 0.02f, 10, 20, 0.1f, 0),
                    new PID(1, 0, 0.00f, 15, 20, 0.1f, 0),
                    new PID(1.4f, 0, 0, 10, 1, 0.1f, 0),
                    new PID(0.04f, 0.004f, 0, 0.5f, 20, 0.1f, 0)
                    );
                pids.Add(mav, pid);
            }

            var rollp = pid.Item1;
            var pitchp = pid.Item2;
            var yawp = pid.Item3;
            var speedp = pid.Item4;
            var thrustp = pid.Item5;


            var newroll = 0d;
            var newpitch = 0d;

            var altdelta = target.Alt - mav.cs.alt;

            altdelta = MathHelper.constrain(altdelta, -35, 35);
            //newpitch = altdelta;
            att_target.type_mask -= 0b00000010;

            pitchp.set_input_filter_all((float)altdelta);

            // 积分分离，防止I过大反抗P
            if (Math.Abs(altdelta) > 15)
            {
                pitchp.reset_I();
            }

            newpitch = pitchp.get_pid();
            newpitch = MathHelper.constrain(newpitch, -30, 30);

            var leaderturnrad = leader.cs.radius;
            var mavturnradius = leaderturnrad - offset.x;

            var distToTarget = mav.cs.Location.GetDistance(target);
            var bearingToTarget = mav.cs.Location.GetBearing(target);

            // bearing stability 
            // distToTarget < 30
            if (distToTarget < mav.sawmAdvancedParameters.rolllowdistboundary_m)
                bearingToTarget = mav.cs.Location.GetBearing(targetleader);
            // fly in from behind
            // distToTarget > 100
            if (distToTarget > mav.sawmAdvancedParameters.rollhighdistboundary_m)
                bearingToTarget = mav.cs.Location.GetBearing(targettrailer);

            var bearingDelta = wrap_180(bearingToTarget - mav.cs.yaw);
            var tangent90 = bearingDelta > 0 ? 90 : -90;

            newroll = 0;

            // if the delta is > 90 then we are facing the wrong direction
            if (Math.Abs(bearingDelta) < 85)
            {
                var insideAngle = Math.Abs(tangent90 - bearingDelta);
                var angleCenter = 180 - insideAngle * 2;

                // sine rule
                // Math.Max(distToTarget, 40)
                var sine1 = Math.Max(distToTarget, Math.Abs(mav.sawmAdvancedParameters.rollmindisttotarget_m)) /
                            Math.Sin(angleCenter * MathHelper.deg2rad);
                var radius = sine1 * Math.Sin(insideAngle * MathHelper.deg2rad);
                // average calced + leader offset turnradius - acts as a FF
                radius = (Math.Abs(radius) + Math.Abs(mavturnradius)) / 2;

                var angleBank = Math.Atan(((mav.cs.groundspeed * mav.cs.groundspeed) / radius) / 9.8);

                angleBank *= MathHelper.rad2deg;

                if (bearingDelta > 0)
                    newroll = Math.Abs(angleBank);
                else
                    newroll = -Math.Abs(angleBank);
            }

            //newroll += MathHelper.constrain(bearingDelta, -20, 20);
            newroll += MathHelper.constrain(bearingDelta, -Math.Abs(mav.sawmAdvancedParameters.yawtorollmaxangle_deg), Math.Abs(mav.sawmAdvancedParameters.yawtorollmaxangle_deg));

            newroll = MathHelper.constrain(newroll, -65, 65);

            // tr = gs2 / (9.8 * x)
            // (9.8 * x) * tr = gs2
            // 9.8 * x = gs2 / tr
            // (gs2/tr)/9.8 = x

            var angle = ((mav.cs.groundspeed * mav.cs.groundspeed) / mavturnradius) / 9.8;

            //newroll = angle * MathHelper.rad2deg;

            // 1 degree of roll for ever 1 degree of yaw error
            //newroll += MathHelper.constrain(yawerror, -20, 20);

            //rollp.set_input_filter_all((float)yawdelta);
            // do speed
            //att_target.thrust = (float) MathHelper.mapConstrained(dist, 0, 40, 0, 1);
            // 计算航向Pid
            yawp.set_input_filter_all((float)yawerror);
            double newyawrate = yawp.get_pid();
            att_target.type_mask -= 0b01000000;
            double targetspeed = 0;
            // 油动，针对星睿航空改一等机型。油动存在收油延时问题，需要提前减速
            if (oilPower)
            {
                var disttospeed = dist;
                // 距离超过130米，直接给满油门
                if (disttospeed > mav.sawmAdvancedParameters.predistancetolosespeed)
                {
                    // 期望速度给最高
                    targetspeed = leader.cs.groundspeed * (1 + mav.sawmAdvancedParameters.deltaspeedscale);
                    // 油门给最大
                    att_target.thrust = 1;
                }
                // 提前减速
                else if (disttospeed > 30 && mav.cs.groundspeed > leader.cs.groundspeed + 2)
                {
                    targetspeed = leader.cs.groundspeed - 2;
                    StallProtect(mav, ref targetspeed);
                    targetspeed = Math.Max(mav.sawmAdvancedParameters.mintargetspeed, targetspeed);

                    var speeddelta = targetspeed - mav.cs.groundspeed;
                    // in m out 0-1
                    thrustp.set_input_filter_all((float)speeddelta);

                    // add thrust trim
                    att_target.thrust = (float)MathHelper.constrain(thrustp.get_pid() + mav.sawmAdvancedParameters.thrusttrim, mav.sawmAdvancedParameters.thrustmin, 1);
                }
                else
                {
                    speedp.set_input_filter_all((float)disttospeed);
                    // 积分分离
                    if (Math.Abs(disttospeed) > 40)
                    {
                        speedp.reset_I();
                    }
                    var deltaspeed = speedp.get_pid();
                    var speedscale = mav.sawmAdvancedParameters.deltaspeedscale;
                    deltaspeed = (float)MathHelper.constrain(deltaspeed, -leader.cs.groundspeed * speedscale, leader.cs.groundspeed * speedscale);
                    //targetspeed
                    targetspeed = leader.cs.groundspeed + deltaspeed;

                    StallProtect(mav, ref targetspeed);

                    targetspeed = Math.Max(mav.sawmAdvancedParameters.mintargetspeed, targetspeed);

                    var speeddelta = targetspeed - mav.cs.groundspeed;
                    // in m out 0-1
                    thrustp.set_input_filter_all((float)speeddelta);

                    // 
                    att_target.thrust = (float)MathHelper.constrain(thrustp.get_pid() + mav.sawmAdvancedParameters.thrusttrim, mav.sawmAdvancedParameters.thrustmin, 1);
                }
            }// 电动力
            else
            {
                var disttodeltaspeed = MathHelper.constrain(dist, -120, 120);
                speedp.set_input_filter_all((float)disttodeltaspeed);
                // 积分分离
                if (Math.Abs(disttodeltaspeed) > 40)
                {
                    speedp.reset_I();
                }
                var deltaspeed = speedp.get_pid();
                var speedscale = mav.sawmAdvancedParameters.deltaspeedscale;
                deltaspeed = (float)MathHelper.constrain(deltaspeed, -leader.cs.groundspeed * speedscale, leader.cs.groundspeed * speedscale);
                //targetspeed
                targetspeed = leader.cs.groundspeed + deltaspeed;

                StallProtect(mav,ref targetspeed);

                targetspeed = Math.Max(mav.sawmAdvancedParameters.mintargetspeed, targetspeed);

                var speeddelta = targetspeed - mav.cs.groundspeed;
                // in m out 0-1
                thrustp.set_input_filter_all((float)speeddelta);

                // 0.1 demand + pid results
                att_target.thrust = (float)MathHelper.constrain(thrustp.get_pid() + mav.sawmAdvancedParameters.thrusttrim, mav.sawmAdvancedParameters.thrustmin, 1);
            }
            // 纵向
            mav.SwarmTargetAlt = target.Alt;
            mav.SwarmNavPitch = newpitch;
            mav.SwarmDistToTarg = dist;
            mav.SwarmNavSpeed = targetspeed;
            mav.SwarmNavThurst = att_target.thrust;
            // 横侧向
            mav.SwarmNavRoll = newroll;
            mav.SwarmTargYaw = targyaw;
            mav.SwarmNavBearing = bearingToTarget;
            mav.SwarmYawError = yawerror;
            // 与长机的间距
            mav.SwarmDistToLeader = leader.cs.Location.GetDistance(mav.cs.Location);
            Quaternion q = new Quaternion();
            q.from_vector312(newroll * MathHelper.deg2rad, newpitch * MathHelper.deg2rad, newyawrate * MathHelper.deg2rad);

            att_target.q = new float[4];
            att_target.q[0] = (float)q.q1;
            att_target.q[1] = (float)q.q2;
            att_target.q[2] = (float)q.q3;
            att_target.q[3] = (float)q.q4;

            //0b0= rpy
            att_target.type_mask -= 0b10000101;
            //att_target.type_mask -= 0b10000100;

            port.sendPacket(att_target, mav.sysid, mav.compid);
            if (swarmnavcontrolenable)
            {
                port.sendSwarmNavPosition(mav.sysid, mav.compid, (float)target.Alt);
            }
        }
        public bool swarmnavcontrolenable = true;
        public void ArduplanePIDCommand(MAVLinkInterface port, MAVState mav, PointLatLngAlt target, MAVState leader)
        {
            // 最小高度30M
            target.Alt = Math.Max(30, target.Alt);
            // get distance from target position
            var dist = target.GetDistance(mav.cs.Location);

            // get bearing to target
            var targyaw = mav.cs.Location.GetBearing(target);

            var targettrailer = target.newpos(leader.cs.yaw, Math.Abs(dist) * -mav.sawmAdvancedParameters.targettrailerscale);
            var targetleader = target.newpos(leader.cs.yaw, mav.sawmAdvancedParameters.targetleaderdis_m + dist);

            var yawerror = wrap_180(targyaw - mav.cs.yaw);
            var mavleadererror = wrap_180(leader.cs.yaw - mav.cs.yaw);
            if (dist < mav.sawmAdvancedParameters.yawdistanceboundary_m)
            {
                targyaw = mav.cs.Location.GetBearing(targetleader);
                yawerror = wrap_180(targyaw - mav.cs.yaw);

                var targBearing = mav.cs.Location.GetBearing(target);

                // check the bearing for the leader and target are within 45 degrees.
                if (Math.Abs(wrap_180(targBearing - targyaw)) > 45)
                    dist *= -1;
            }
            else
            {
                targyaw = mav.cs.Location.GetBearing(targettrailer);
                yawerror = wrap_180(targyaw - mav.cs.yaw);
            }

            // display update
            mav.GuidedMode.x = (float)target.Lat;
            mav.GuidedMode.y = (float)target.Lng;
            mav.GuidedMode.z = (float)target.Alt;

            MAVLink.mavlink_set_attitude_target_t att_target = new MAVLink.mavlink_set_attitude_target_t();
            att_target.target_system = mav.sysid;
            att_target.target_component = mav.compid;
            att_target.type_mask = 0xff;

            Tuple<PID, PID, PID, PID, PID> pid;

            if (pids.ContainsKey(mav))
            {
                pid = pids[mav];
            }
            else
            {
                pid = new Tuple<PID, PID, PID, PID, PID>(
                    new PID(1f, .03f, 0.02f, 10, 20, 0.1f, 0),
                    new PID(1f, .03f, 0.02f, 10, 20, 0.1f, 0),
                    new PID(1, 0, 0.00f, 15, 20, 0.1f, 0),
                    new PID(1.4f, 0, 0, 10, 1, 0.1f, 0),
                    new PID(0.04f, 0.004f, 0, 0.5f, 20, 0.1f, 0)
                    );
                pids.Add(mav, pid);
            }

            var rollp = pid.Item1;
            var pitchp = pid.Item2;
            var yawp = pid.Item3;
            var speedp = pid.Item4;
            var thrustp = pid.Item5;


            var newroll = 0d;
            var newpitch = 0d;

            var altdelta = target.Alt - mav.cs.alt;

            altdelta = MathHelper.constrain(altdelta, -35, 35);
            //newpitch = altdelta;
            att_target.type_mask -= 0b00000010;

            pitchp.set_input_filter_all((float)altdelta);

            // 积分分离，防止I过大反抗P
            if (Math.Abs(altdelta) > 15)
            {
                pitchp.reset_I();
            }

            newpitch = pitchp.get_pid();
            newpitch = MathHelper.constrain(newpitch, -30, 30);

            var distToTarget = mav.cs.Location.GetDistance(target);
            var bearingToTarget = mav.cs.Location.GetBearing(target);

            if (distToTarget < mav.sawmAdvancedParameters.rolllowdistboundary_m)
                bearingToTarget = mav.cs.Location.GetBearing(targetleader);
            if (distToTarget > mav.sawmAdvancedParameters.rollhighdistboundary_m)
                bearingToTarget = mav.cs.Location.GetBearing(targettrailer);

            var bearingDelta = wrap_180(bearingToTarget - mav.cs.yaw);

            if (distToTarget < mav.sawmAdvancedParameters.rolllowdistboundary_m)
            {
                newroll = PIDControlUpdate(mav, targettrailer, targetleader, rollp);
            }
            else if (distToTarget < mav.sawmAdvancedParameters.rollhighdistboundary_m)
            {
                newroll = PIDControlUpdate(mav, targettrailer, target, rollp);
            }
            else
            {
                newroll = PIDControlUpdate(mav, mav.cs.Location, targettrailer, rollp);
                newroll += MathHelper.constrain(bearingDelta, -Math.Abs(mav.sawmAdvancedParameters.yawtorollmaxangle_deg), Math.Abs(mav.sawmAdvancedParameters.yawtorollmaxangle_deg));
            }

            newroll = MathHelper.constrain(newroll, -65, 65);
            // 计算航向Pid
            yawp.set_input_filter_all((float)yawerror);
            double newyawrate = yawp.get_pid();
            att_target.type_mask -= 0b01000000;
            double targetspeed = 0;
            var disttodeltaspeed = MathHelper.constrain(dist, -120, 120);
            speedp.set_input_filter_all((float)disttodeltaspeed);
            // 积分分离
            if (Math.Abs(disttodeltaspeed) > 40)
            {
                speedp.reset_I();
            }
            var deltaspeed = speedp.get_pid();
            var speedscale = mav.sawmAdvancedParameters.deltaspeedscale;
            deltaspeed = (float)MathHelper.constrain(deltaspeed, -leader.cs.groundspeed * speedscale, leader.cs.groundspeed * speedscale);
            //targetspeed
            targetspeed = leader.cs.groundspeed + deltaspeed;

            targetspeed = Math.Max(mav.sawmAdvancedParameters.mintargetspeed, targetspeed);

            var speeddelta = targetspeed - mav.cs.groundspeed;
            // in m out 0-1
            thrustp.set_input_filter_all((float)speeddelta);

            // 0.1 demand + pid results
            att_target.thrust = (float)MathHelper.constrain(thrustp.get_pid() + mav.sawmAdvancedParameters.thrusttrim, mav.sawmAdvancedParameters.thrustmin, 1);
            // 纵向
            mav.SwarmTargetAlt = target.Alt;
            mav.SwarmNavPitch = newpitch;
            mav.SwarmDistToTarg = dist;
            mav.SwarmNavSpeed = targetspeed;
            mav.SwarmNavThurst = att_target.thrust;
            // 横侧向
            mav.SwarmNavRoll = newroll;
            mav.SwarmTargYaw = targyaw;
            mav.SwarmNavBearing = bearingToTarget;
            mav.SwarmYawError = yawerror;
            // 与长机的间距
            mav.SwarmDistToLeader = leader.cs.Location.GetDistance(mav.cs.Location);
            Quaternion q = new Quaternion();
            q.from_vector312(newroll * MathHelper.deg2rad, newpitch * MathHelper.deg2rad, newyawrate * MathHelper.deg2rad);

            att_target.q = new float[4];
            att_target.q[0] = (float)q.q1;
            att_target.q[1] = (float)q.q2;
            att_target.q[2] = (float)q.q3;
            att_target.q[3] = (float)q.q4;

            //0b0= rpy
            att_target.type_mask -= 0b10000101;
            //att_target.type_mask -= 0b10000100;

            port.sendPacket(att_target, mav.sysid, mav.compid);
            if (swarmnavcontrolenable)
            {
                port.sendSwarmNavPosition(mav.sysid, mav.compid, (float)target.Alt);
            }
        }
        public float PIDControlUpdate(MAVState mav, PointLatLngAlt start, PointLatLngAlt dest,PID rollpid)
        {
            PointLatLngAlt current_loc = new PointLatLngAlt(mav.cs.Location);
            // Calculate the NE position of WP B relative to WP A
            System.Numerics.Vector2 AB = Avoidance.location_diff(start, dest);
            float AB_length = AB.Length();
            if (AB.Length() < 1.0e-6f)
            {
                AB = Avoidance.location_diff(current_loc, dest);
                if (AB.Length() < 1.0e-6f)
                {
                    AB = new System.Numerics.Vector2((float)Math.Cos(mav.cs.yaw * MathHelper.deg2rad), (float)Math.Sin(mav.cs.yaw * MathHelper.deg2rad));
                }
            }
            // normal
            AB.X /= AB_length;
            AB.Y /= AB_length;
            // Calculate the NE position of the aircraft relative to WP A
            System.Numerics.Vector2 A_air = Avoidance.location_diff(start, current_loc);
            // calculate distance to target track, for reporting   ×积
            float d = Avoidance.cross_mul(A_air, AB);
            rollpid.set_input_filter_all(d);
            if (Math.Abs(d) > 15)
            {
                rollpid.reset_I();
            }
            float ret = rollpid.get_pid();
            ret = (float)MathHelper.constrain(ret, -65, 65);
            return ret;
        }
        public void ArducopterCommand(MAVLinkInterface port, MAVState mav, PointLatLngAlt target, MAVState leader)
        {
            target.Alt = Math.Max(5, target.Alt);
            // 纵向
            mav.SwarmTargetAlt = target.Alt;
            mav.SwarmDistToTarg = target.GetDistance(mav.cs.Location);
            // 横侧向
            // 与长机的间距
            mav.SwarmDistToLeader = leader.cs.Location.GetDistance(mav.cs.Location);

            Vector3 vel = new Vector3(leader.cs.vx, leader.cs.vy, leader.cs.vz);

            // do pos/vel
            port.setPositionTargetGlobalInt(mav.sysid, mav.compid, true,
                true, false, false,
                MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT_INT, target.Lat, target.Lng, target.Alt, vel.x,
                vel.y, vel.z, 0, 0);

            // do yaw
            if (!gimbal)
            {
                // within 3 degrees dont send
                if (Math.Abs(mav.cs.yaw - leader.cs.yaw) > 3)
                    port.doCommand(mav.sysid, mav.compid, MAVLink.MAV_CMD.CONDITION_YAW, leader.cs.yaw,
                        100.0f, 0, 0, 0, 0, 0, false);
            }
            else
            {
                // gimbal direction
                if (Math.Abs(mav.cs.yaw - leader.cs.yaw) > 3)
                    port.setMountControl(mav.sysid, mav.compid, 45, 0, leader.cs.yaw, false);
            }
        }
        List<MAVState> MAVStates = new List<MAVState>();
        public void ObstacleAvoidanceAlgorithm(bool virtualleader,bool ignoreleader)
        {
            MAVStates.Clear();
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    MAVStates.Add(mav);
                }
            }
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    // 长机不去避障
                    if (mav == Leader && ignoreleader)
                    {
                        continue;
                    }
                    MAVStates.Remove(mav);
                    // 虚拟长机模式下长机不作为障碍物
                    if (virtualleader)
                    {
                        MAVStates.Remove(Leader);
                    }
                    avoidance.set_obstacles(MAVStates);
                    mav.obsvector = avoidance.update(mav);
                    mav.obstacleid = avoidance.get_obstacleid();
                }
            }
        }
        // 失速保护
        public void StallProtect(MAVState mav,ref double target_gndpsd)
        {
            // 没有开启失速保护功能后者地面站读取的参数不全，无法确定最小的空速
            if (mav.sawmAdvancedParameters.stallprotect == false || mav.param["ARSPD_TYPE"] == null || mav.param["ARSPD_FBW_MIN"] == null)
            {
                return;
            }
            // 飞控端禁用了空速计或者空速在设定的最小空速以上
            if ((int)mav.param["ARSPD_TYPE"] == 0 || mav.cs.airspeed > (float)mav.param["ARSPD_FBW_MIN"])
            {
                return;
            }
            // 进入失速保护，目的是不让飞机的空速小于最小设定的空速。
            double target_minairspd = (double)mav.param["ARSPD_FBW_MIN"];

            target_gndpsd = target_minairspd + (mav.cs.groundspeed - mav.cs.airspeed);

        }

        public bool gimbal { get; set; }
    }
    public class Navigation
    {
        /// <summary>
        /// 基于L1的编队跟踪横侧向策略
        /// </summary>
        DateTime lastPosRecordTime = DateTime.Now;
        PointLatLngAlt lastTarget = new PointLatLngAlt();

        DateTime last_update_L1control = DateTime.Now;
        float _L1_xtrack_i = 0;
        float _L1_damping = 0.75f;
        float _L1_dist;
        float _L1_period = 15;
        // crosstrack error in meters
        float _crosstrack_error;
        float _nav_bearing;
        float _L1_xtrack_i_gain = 0.02f;
        float _L1_xtrack_i_gain_prev = 0;
        float _last_Nu;
        float _latAccDem;
        float _bearing_error;
        float _target_bearing;
        bool have_speed = false;
        bool have_yaw = false;
        public Navigation()
        {

        }
        public void SetSpeed(MAVState mav,float speed)
        {
            have_speed = true;
            mav.SpeedOfTarget = speed;
        }
        public void SetYaw(MAVState mav, float yaw)
        {
            have_yaw = true;
            mav.CourseOfTarget = yaw;
        }
        public void CalTargetCourseAndSpeed(MAVState mav, PointLatLngAlt target)
        {
            // 航迹推算
            if (DateTime.Now.Subtract(lastPosRecordTime).TotalSeconds > 1 || (lastTarget.Lat.Equals(0) && lastTarget.Lng.Equals(0) && lastTarget.Alt.Equals(0)))
            {
                lastPosRecordTime = DateTime.Now;
                // 第一次进入
                if (lastTarget.Lat.Equals(0) && lastTarget.Lng.Equals(0) && lastTarget.Alt.Equals(0))
                {
                    lastTarget = new PointLatLngAlt(mav.cs.lat, mav.cs.lng, mav.cs.alt);
                    if (!have_yaw)
                    {
                        mav.CourseOfTarget = lastTarget.GetBearing(target);
                    }
                    if (!have_speed)
                    {
                        mav.SpeedOfTarget = mav.cs.groundspeed;
                    }
                }
                else
                {
                    if (!have_yaw)
                    {
                        mav.CourseOfTarget = lastTarget.GetBearing(target);
                    }
                    if (!have_speed)
                    {
                        mav.SpeedOfTarget = (float)lastTarget.GetDistance(target);
                    }
                    lastTarget = new PointLatLngAlt(target);
                }
                have_speed = false;
                have_yaw = false;
            }
        }
        LowPassFilter2p dist_filter = new LowPassFilter2p(10,1);
        LowPassFilter2p target_spd_filter = new LowPassFilter2p(10,1);
        public void ArduplaneL1Command(MAVLinkInterface port, MAVState mav, PointLatLngAlt target)
        {
            CalTargetCourseAndSpeed(mav,target);
            // 最小高度30M
            target.Alt = Math.Max(30, target.Alt);
            // get distance from target position
            var dist = target.GetDistance(mav.cs.Location);

            // get bearing to target
            var targyaw = mav.cs.Location.GetBearing(target);

            var targettrailer = target.newpos(mav.CourseOfTarget, -100);
            var targetleader = target.newpos(mav.CourseOfTarget, mav.sawmAdvancedParameters.targetleaderdis_m + dist);

            var yawerror = wrap_180(targyaw - mav.cs.yaw);
            var mavleadererror = wrap_180(mav.CourseOfTarget - mav.cs.yaw);
            // dist<100
            if (dist < mav.sawmAdvancedParameters.yawdistanceboundary_m)
            {
                targyaw = mav.cs.Location.GetBearing(targetleader);
                yawerror = wrap_180(targyaw - mav.cs.yaw);

                var targBearing = mav.cs.Location.GetBearing(target);

                // check the bearing for the leader and target are within 45 degrees.
                if (Math.Abs(wrap_180(targBearing - targyaw)) > 45)
                    dist *= -1;
            }
            else
            {
                targyaw = mav.cs.Location.GetBearing(targettrailer);
                yawerror = wrap_180(targyaw - mav.cs.yaw);
            }

            // display update
            mav.GuidedMode.x = (float)target.Lat;
            mav.GuidedMode.y = (float)target.Lng;
            mav.GuidedMode.z = (float)target.Alt;

            MAVLink.mavlink_set_attitude_target_t att_target = new MAVLink.mavlink_set_attitude_target_t();
            att_target.target_system = mav.sysid;
            att_target.target_component = mav.compid;
            att_target.type_mask = 0xff;

            Tuple<PID, PID, PID, PID, PID> pid;

            if (Formation.instance.pids.ContainsKey(mav))
            {
                pid = Formation.instance.pids[mav];
            }
            else
            {
                pid = new Tuple<PID, PID, PID, PID, PID>(
                    new PID(1f, .03f, 0.02f, 10, 20, 0.1f, 0),
                    new PID(1f, .03f, 0.02f, 10, 20, 0.1f, 0),
                    new PID(1, 0, 0.00f, 15, 20, 0.1f, 0),
                    new PID(1.4f, 0, 0, 10, 1, 0.1f, 0),
                    new PID(0.04f, 0.004f, 0, 0.5f, 20, 0.1f, 0)
                    );
                Formation.instance.pids.Add(mav, pid);
            }

            var rollp = pid.Item1;
            var pitchp = pid.Item2;
            var yawp = pid.Item3;
            var speedp = pid.Item4;
            var thrustp = pid.Item5;

            var newroll = 0d;
            var newpitch = 0d;

            var distToTarget = mav.cs.Location.GetDistance(target);
            var bearingToTarget = mav.cs.Location.GetBearing(target);
            // bearing stability 
            // distToTarget < 30
            if (distToTarget < mav.sawmAdvancedParameters.rolllowdistboundary_m)
                bearingToTarget = mav.cs.Location.GetBearing(targetleader);
            // fly in from behind
            // distToTarget > 100
            if (distToTarget > mav.sawmAdvancedParameters.rollhighdistboundary_m)
                bearingToTarget = mav.cs.Location.GetBearing(targettrailer);

            var bearingDelta = wrap_180(bearingToTarget - mav.cs.yaw);
            //
            if (distToTarget < mav.sawmAdvancedParameters.rolllowdistboundary_m)
            {
                newroll = L1ControlUpdate(mav, targettrailer, targetleader);
            }
            else if (distToTarget < mav.sawmAdvancedParameters.rollhighdistboundary_m)
            {
                newroll = L1ControlUpdate(mav, targettrailer, target);
            }
            else
            {
                newroll = L1ControlUpdate(mav, mav.cs.Location, targettrailer);
                //newroll += MathHelper.constrain(bearingDelta, -Math.Abs(mav.sawmAdvancedParameters.yawtorollmaxangle_deg), Math.Abs(mav.sawmAdvancedParameters.yawtorollmaxangle_deg));
            }
            newroll = MathHelper.constrain(newroll, -65, 65);

            // 计算航向Pid
            att_target.type_mask -= 0b01000000;
            yawp.set_input_filter_all((float)yawerror);
            double newyawrate = yawp.get_pid();

            att_target.type_mask -= 0b00000010;
            double filtered_dist = dist_filter.apply((float)dist);
            filtered_dist = MathHelper.constrain(filtered_dist, -120, 120);
            speedp.set_input_filter_all((float)filtered_dist);
            // 直接用filtered_dist的d项容易引入噪声，所以改用两机速度差作为阻尼项。
            float deltaspeed = speedp.get_p() + speedp._kd * (mav.SpeedOfTarget - mav.cs.groundspeed);
            var speedscale = mav.sawmAdvancedParameters.deltaspeedscale;
            deltaspeed = (float)MathHelper.constrain(deltaspeed, -mav.SpeedOfTarget * speedscale, mav.SpeedOfTarget * speedscale);
            //targetspeed
            float targetspeed = mav.SpeedOfTarget + deltaspeed;
            targetspeed = Math.Max(mav.sawmAdvancedParameters.mintargetspeed, targetspeed);
            float targetspeed_filter = target_spd_filter.apply(targetspeed);
            // 总能量算法的更新
            TECS_Update(mav, (float)target.Alt, targetspeed_filter);
            newpitch = get_tecs_pitch_demand();
            newpitch = MathHelper.constrain(newpitch, -30, 30);           
            float throttle = get_tecs_throttle_demand();
            att_target.thrust = (float)MathHelper.constrain(throttle, mav.sawmAdvancedParameters.thrustmin, 1);

            // 纵向
            mav.SwarmTargetAlt = target.Alt;
            mav.SwarmNavPitch = newpitch;
            mav.SwarmDistToTarg = dist;
            mav.SwarmNavSpeed = targetspeed_filter;
            mav.SwarmNavThurst = att_target.thrust;
            // 横侧向
            mav.SwarmNavRoll = newroll;
            mav.SwarmTargYaw = targyaw;
            mav.SwarmNavBearing = bearingToTarget;
            mav.SwarmYawError = yawerror;
            // 与长机的间距
            mav.SwarmDistToLeader = Formation.instance.Leader.cs.Location.GetDistance(mav.cs.Location);
            Quaternion q = new Quaternion();
            q.from_vector312(newroll * MathHelper.deg2rad, newpitch * MathHelper.deg2rad, newyawrate * MathHelper.deg2rad);

            att_target.q = new float[4];
            att_target.q[0] = (float)q.q1;
            att_target.q[1] = (float)q.q2;
            att_target.q[2] = (float)q.q3;
            att_target.q[3] = (float)q.q4;

            //0b0= rpy
            att_target.type_mask -= 0b10000101;

            port.sendPacket(att_target, mav.sysid, mav.compid);
            if (Formation.instance.swarmnavcontrolenable)
            {
                port.sendSwarmNavPosition(mav.sysid, mav.compid, (float)target.Alt);
            }
        }
        public float L1ControlUpdate(MAVState mav, PointLatLngAlt start, PointLatLngAlt dest, float dist_min = 0.0f)
        {
            float Nu;
            float xtrackVel;
            float ltrackVel;
            PointLatLngAlt current_loc = new PointLatLngAlt(mav.cs.Location);
            double dt = DateTime.Now.Subtract(last_update_L1control).TotalSeconds;
            if (dt > 0.1)
            {
                dt = 0.1;
                _L1_xtrack_i = 0.0f;
            }
            last_update_L1control = DateTime.Now;
            // Calculate L1 gain required for specified damping
            float K_L1 = 4.0f * _L1_damping * _L1_damping;
            System.Numerics.Vector2 _groundspeed_vector = new System.Numerics.Vector2((float)mav.cs.vx, (float)mav.cs.vy);
            _target_bearing = (float)current_loc.GetBearing(dest);
            //Calculate groundspeed
            float groundSpeed = _groundspeed_vector.Length();
            if (groundSpeed < 0.1f)
            {
                // use a small ground speed vector in the right direction,
                // allowing us to use the compass heading at zero GPS velocity
                groundSpeed = 0.1f;
                _groundspeed_vector = new System.Numerics.Vector2((float)Math.Cos(mav.cs.yaw * MathHelper.deg2rad), (float)Math.Sin(mav.cs.yaw * MathHelper.deg2rad)) * groundSpeed;
            }
            _L1_dist = Math.Max(0.3183099f * _L1_damping * _L1_period * groundSpeed, dist_min);
            // Calculate the NE position of WP B relative to WP A
            System.Numerics.Vector2 AB = Avoidance.location_diff(start, dest);
            float AB_length = AB.Length();
            if (AB.Length() < 1.0e-6f)
            {
                AB = Avoidance.location_diff(current_loc, dest);
                if (AB.Length() < 1.0e-6f)
                {
                    AB = new System.Numerics.Vector2((float)Math.Cos(mav.cs.yaw * MathHelper.deg2rad), (float)Math.Sin(mav.cs.yaw * MathHelper.deg2rad));
                }
            }
            // normal
            AB.X /= AB_length;
            AB.Y /= AB_length;
            // Calculate the NE position of the aircraft relative to WP A
            System.Numerics.Vector2 A_air = Avoidance.location_diff(start, current_loc);
            // calculate distance to target track, for reporting   ×积
            _crosstrack_error = Avoidance.cross_mul(A_air, AB);
            //Determine if the aircraft is behind a +-135 degree degree arc centred on WP A
            //and further than L1 distance from WP A. Then use WP A as the L1 reference point
            //Otherwise do normal L1 guidance
            float WP_A_dist = A_air.Length();
            float alongTrackDist = Avoidance.dot_mul(A_air, AB);
            if (WP_A_dist > _L1_dist && alongTrackDist / Math.Max(WP_A_dist, 1.0f) < -0.7071f)
            {
                ////Calc Nu to fly To WP A
                //float A_air_length = A_air.Length();
                //System.Numerics.Vector2 A_air_unit = new System.Numerics.Vector2(A_air.X / A_air_length, A_air.Y / A_air_length); // Unit vector from WP A to aircraft
                //xtrackVel = Avoidance.cross_mul(_groundspeed_vector, (-A_air_unit));// Velocity across line
                //ltrackVel = Avoidance.dot_mul(_groundspeed_vector, (-A_air_unit)); // Velocity along line
                //Nu = (float)Math.Atan2(xtrackVel, ltrackVel);
                //_nav_bearing = (float)Math.Atan2(-A_air_unit.Y, -A_air_unit.X); // bearing (radians) from AC to L1 point

                // Calc Nu to fly To WP B
                System.Numerics.Vector2 B_air = Avoidance.location_diff(dest, current_loc);
                float B_air_length = B_air.Length();
                System.Numerics.Vector2 B_air_unit = new System.Numerics.Vector2(B_air.X / B_air_length, B_air.Y / B_air_length); // Unit vector from WP B to aircraft
                xtrackVel = Avoidance.cross_mul(_groundspeed_vector, (-B_air_unit)); // Velocity across line
                ltrackVel = Avoidance.dot_mul(_groundspeed_vector, (-B_air_unit)); // Velocity along line
                Nu = (float)Math.Atan2(xtrackVel, ltrackVel);
                _nav_bearing = (float)Math.Atan2(-B_air_unit.Y, -B_air_unit.X); // bearing (radians) from AC to L1 point
            }
            else if (alongTrackDist > AB_length + groundSpeed * 3)
            {
                // we have passed point B by 3 seconds. Head towards B
                // Calc Nu to fly To WP B
                System.Numerics.Vector2 B_air = Avoidance.location_diff(dest, current_loc);
                float B_air_length = B_air.Length();
                System.Numerics.Vector2 B_air_unit = new System.Numerics.Vector2(B_air.X / B_air_length, B_air.Y / B_air_length); // Unit vector from WP B to aircraft
                xtrackVel = Avoidance.cross_mul(_groundspeed_vector, (-B_air_unit)); // Velocity across line
                ltrackVel = Avoidance.dot_mul(_groundspeed_vector, (-B_air_unit)); // Velocity along line
                Nu = (float)Math.Atan2(xtrackVel, ltrackVel);
                _nav_bearing = (float)Math.Atan2(-B_air_unit.Y, -B_air_unit.X); // bearing (radians) from AC to L1 point
            }
            else
            { //Calc Nu to fly along AB line

                //Calculate Nu2 angle (angle of velocity vector relative to line connecting waypoints)
                xtrackVel = Avoidance.cross_mul(_groundspeed_vector, AB); // Velocity cross track
                ltrackVel = Avoidance.dot_mul(_groundspeed_vector, AB); // Velocity along track
                float Nu2 = (float)Math.Atan2(xtrackVel, ltrackVel);
                //Calculate Nu1 angle (Angle to L1 reference point)
                float sine_Nu1 = _crosstrack_error / Math.Max(_L1_dist, 0.1f);
                //Limit sine of Nu1 to provide a controlled track capture angle of 45 deg
                sine_Nu1 = (float)MathHelper.constrain(sine_Nu1, -0.7071, 0.7071);
                float Nu1 = (float)Math.Asin(sine_Nu1);

                // compute integral error component to converge to a crosstrack of zero when traveling
                // straight but reset it when disabled or if it changes. That allows for much easier
                // tuning by having it re-converge each time it changes.
                if (_L1_xtrack_i_gain <= 0 || !_L1_xtrack_i_gain.Equals(_L1_xtrack_i_gain_prev))
                {
                    _L1_xtrack_i = 0;
                    _L1_xtrack_i_gain_prev = _L1_xtrack_i_gain;
                }
                else if (Math.Abs(Nu1) < 5 * MathHelper.deg2rad)
                {
                    _L1_xtrack_i += Nu1 * _L1_xtrack_i_gain * (float)dt;

                    // an AHRS_TRIM_X=0.1 will drift to about 0.08 so 0.1 is a good worst-case to clip at
                    _L1_xtrack_i = (float)MathHelper.constrain(_L1_xtrack_i, -0.1, 0.1);
                }

                // to converge to zero we must push Nu1 harder
                Nu1 += _L1_xtrack_i;

                Nu = Nu1 + Nu2;
                _nav_bearing = (float)Math.Atan2(AB.Y, AB.X) + Nu1; // bearing (radians) from AC to L1 point
            }

            prevent_indecision(ref Nu, mav);
            _last_Nu = Nu;

            //Limit Nu to +-(pi/2)
            Nu = (float)MathHelper.constrain(Nu, -1.5708f, +1.5708f);
            _latAccDem = K_L1 * groundSpeed * groundSpeed / _L1_dist * (float)Math.Sin(Nu);
            _bearing_error = Nu; // bearing error angle (radians), +ve to left of track
            // get nav roll
            float ret;
            ret = (float)(Math.Cos(mav.cs.pitch * MathHelper.deg2rad) * Math.Atan(_latAccDem * 0.101972) * MathHelper.rad2deg); // 0.101972 = 1/9.81
            ret = (float)MathHelper.constrain(ret, -65, 65);
            return ret;
        }
        void prevent_indecision(ref float Nu, MAVState mav)
        {
            float Nu_limit = (float)(0.9f * Math.PI);
            if (Math.Abs(Nu) > Nu_limit &&
                Math.Abs(_last_Nu) > Nu_limit &&
                Math.Abs(wrap_180(_target_bearing - mav.cs.yaw)) > 120 &&
                Nu * _last_Nu < 0.0f)
            {
                // we are moving away from the target waypoint and pointing
                // away from the waypoint (not flying backwards). The sign
                // of Nu has also changed, which means we are
                // oscillating in our decision about which way to go
                Nu = _last_Nu;
            }
        }
        double wrap_180(double input)
        {
            if (input > 180)
                return input - 360;
            if (input < -180)
                return input + 360;
            return input;
        }
        /// <summary>
        /// TECS的固定翼纵向控制
        /// </summary>
        /// 
        float _DT;
        DateTime tecs_last_time = DateTime.Now;
        bool _tecs_initialize = false;
        float GRAVITY_MSS = 9.80665f;
        float _hgt_dem,_EAS_dem, _TAS_state, _THRmaxf, _THRminf, _PITCHmaxf, _PITCHminf;
        float _integTHR_state, _integSEB_state, _last_throttle_dem, _last_pitch_dem, _hgt_dem_adj_last, _hgt_dem_adj, _hgt_dem_prev,
            _hgt_dem_in_old, _TAS_dem_last, _TAS_dem_adj, _TAS_dem,_TASmax, _TASmin, _STEdot_max, _STEdot_min, _maxClimbRate, _minSinkRate,
            _maxSinkRate,_TAS_rate_dem, _hgt_rate_dem, _SPE_dem, _SKE_dem, _SPEdot_dem, _SKEdot_dem, _SPE_est, _SKE_est, _SPEdot, _SKEdot,
            _height, _climb_rate, _vel_dot, _STE_error, _STEdotErrLast, _throttle_dem, time_const, _thrDamp, i_gain, _pitch_dem, _pitch_dem_unc,
            _vertAccLim;
        void TECS_Update(MAVState mav, float hgt_dem,float EAS_dem)
        {
            _DT = (float)DateTime.Now.Subtract(tecs_last_time).TotalSeconds;
            tecs_last_time = DateTime.Now;
            _hgt_dem = hgt_dem;
            _EAS_dem = EAS_dem;
            _TAS_dem = _EAS_dem;
            _TASmax = (float)mav.CourseOfTarget * (1 + mav.sawmAdvancedParameters.deltaspeedscale);
            _TASmin = mav.sawmAdvancedParameters.mintargetspeed;
            System.Numerics.Vector2 _groundspeed_vector = new System.Numerics.Vector2((float)mav.cs.vx, (float)mav.cs.vy);
            _TAS_state = _groundspeed_vector.Length();
            _THRmaxf = 1;
            _THRminf = mav.sawmAdvancedParameters.thrustmin;
            _PITCHmaxf = (float)mav.param["LIM_PITCH_MAX"] / 100.0f;
            _PITCHminf = (float)mav.param["LIM_PITCH_MIN"] / 100.0f;
            _PITCHmaxf = (float)(_PITCHmaxf*MathHelper.deg2rad);
            _PITCHminf = (float)(_PITCHminf * MathHelper.deg2rad);
            if (_DT > 1.0f || !_tecs_initialize)
            {
                _tecs_initialize = true;
                _integTHR_state = 0.0f;
                _integSEB_state = 0.0f;
                _last_throttle_dem = mav.sawmAdvancedParameters.thrusttrim;
                _last_pitch_dem = (float)(mav.cs.pitch * MathHelper.deg2rad);
                _hgt_dem_adj_last = mav.cs.alt;
                _hgt_dem_adj = _hgt_dem_adj_last;
                _hgt_dem_prev = _hgt_dem_adj_last;
                _hgt_dem_in_old = _hgt_dem_adj_last;
                _TAS_dem_last = _TAS_dem;
                _TAS_dem_adj = _TAS_dem;
                _DT = 0.1f; // when first starting TECS, use a
                            // small time constant
            }
            _update_STE_rate_lim(mav);
            _update_speed_demand();
            _update_height_demand(mav);
            _update_energies(mav);
            _update_throttle(mav);
            _update_pitch(mav);
        }
        void _update_STE_rate_lim(MAVState mav)
        {
            // Calculate Specific Total Energy Rate Limits
            // This is a trivial calculation at the moment but will get bigger once we start adding altitude effects
            _maxClimbRate = (float)mav.param["TECS_CLMB_MAX"];
            _minSinkRate = (float)mav.param["TECS_SINK_MIN"];
            _STEdot_max = _maxClimbRate * GRAVITY_MSS;
            _STEdot_min = -_minSinkRate * GRAVITY_MSS;
        }
        void _update_speed_demand()
        {
            // Constrain speed demand, taking into account the load factor
            _TAS_dem = (float)MathHelper.constrain(_TAS_dem, _TASmin, _TASmax);
            // calculate velocity rate limits based on physical performance limits
            // provision to use a different rate limit if bad descent or underspeed condition exists
            // Use 50% of maximum energy rate to allow margin for total energy contgroller
            float velRateMax = 0.5f * _STEdot_max / _TAS_state;
            float velRateMin = 0.5f * _STEdot_min / _TAS_state;
            // Apply rate limit
            if ((_TAS_dem - _TAS_dem_adj) > (velRateMax * 0.1f))
            {
                _TAS_dem_adj = _TAS_dem_adj + velRateMax * 0.1f;
                _TAS_rate_dem = velRateMax;
            }
            else if ((_TAS_dem - _TAS_dem_adj) < (velRateMin * 0.1f))
            {
                _TAS_dem_adj = _TAS_dem_adj + velRateMin * 0.1f;
                _TAS_rate_dem = velRateMin;
            }
            else
            {
                _TAS_dem_adj = _TAS_dem;
                _TAS_rate_dem = (_TAS_dem - _TAS_dem_last) / 0.1f;
            }
            // Constrain speed demand again to protect against bad values on initialisation.
            _TAS_dem_adj = (float)MathHelper.constrain(_TAS_dem_adj, _TASmin, _TASmax);
            _TAS_dem_last = _TAS_dem;
        }
        void _update_height_demand(MAVState mav)
        {
            // Apply 2 point moving average to demanded height
            _hgt_dem = 0.5f * (_hgt_dem + _hgt_dem_in_old);
            _hgt_dem_in_old = _hgt_dem;
            _maxSinkRate=(float)mav.param["TECS_SINK_MAX"];
            float max_sink_rate = _maxSinkRate;
            // Limit height rate of change
            if ((_hgt_dem - _hgt_dem_prev) > (_maxClimbRate * 0.1f))
            {
                _hgt_dem = _hgt_dem_prev + _maxClimbRate * 0.1f;
            }
            else if ((_hgt_dem - _hgt_dem_prev) < (-max_sink_rate * 0.1f))
            {
                _hgt_dem = _hgt_dem_prev - max_sink_rate * 0.1f;
            }
            _hgt_dem_prev = _hgt_dem;

            // Apply first order lag to height demand
            _hgt_dem_adj = 0.05f * _hgt_dem + 0.95f * _hgt_dem_adj_last;

            _hgt_rate_dem = (_hgt_dem_adj - _hgt_dem_adj_last) / 0.1f;
            float new_hgt_dem = _hgt_dem_adj;
            _hgt_dem_adj_last = _hgt_dem_adj;
            _hgt_dem_adj = new_hgt_dem;
        }
        void _update_energies(MAVState mav)
        {
            _height = mav.cs.alt;
            _climb_rate = -(float)mav.cs.vz;
            _vel_dot = (float)Math.Sin(-mav.cs.pitch * MathHelper.deg2rad) * GRAVITY_MSS + mav.cs.ax/1000* GRAVITY_MSS;
            // Calculate specific energy demands
            _SPE_dem = _hgt_dem_adj * GRAVITY_MSS;
            _SKE_dem = 0.5f * _TAS_dem_adj * _TAS_dem_adj;

            // Calculate specific energy rate demands
            _SPEdot_dem = _hgt_rate_dem * GRAVITY_MSS;
            _SKEdot_dem = _TAS_state * _TAS_rate_dem;

            // Calculate specific energy
            _SPE_est = _height * GRAVITY_MSS;
            _SKE_est = 0.5f * _TAS_state * _TAS_state;

            // Calculate specific energy rate
            _SPEdot = _climb_rate * GRAVITY_MSS;
            _SKEdot = _TAS_state * _vel_dot;
        }
        void _update_throttle(MAVState mav)
        {
            // Calculate limits to be applied to potential energy error to prevent over or underspeed occurring due to large height errors
            float SPE_err_max = 0.5f * _TASmax * _TASmax - _SKE_dem;
            float SPE_err_min = 0.5f * _TASmin * _TASmin - _SKE_dem;

            // Calculate total energy error
            _STE_error = (float)MathHelper.constrain((_SPE_dem - _SPE_est), SPE_err_min, SPE_err_max) + _SKE_dem - _SKE_est;
            float STEdot_dem = (float)MathHelper.constrain((_SPEdot_dem + _SKEdot_dem), _STEdot_min, _STEdot_max);
            float STEdot_error = STEdot_dem - _SPEdot - _SKEdot;

            // Apply 0.5 second first order filter to STEdot_error
            // This is required to remove accelerometer noise from the  measurement
            STEdot_error = 0.2f * STEdot_error + 0.8f * _STEdotErrLast;
            _STEdotErrLast = STEdot_error;

            // Calculate throttle demand
            // Calculate gain scaler from specific energy error to throttle
            // (_STEdot_max - _STEdot_min) / (_THRmaxf - _THRminf) is the derivative of STEdot wrt throttle measured across the max allowed throttle range.
            time_const = (float)mav.param["TECS_TIME_CONST"];
            float K_STE2Thr = 1 / (time_const * (_STEdot_max - _STEdot_min) / (_THRmaxf - _THRminf));

            // Calculate feed-forward throttle
            float ff_throttle = 0;
            float nomThr = mav.sawmAdvancedParameters.thrusttrim;
            // Use the demanded rate of change of total energy as the feed-forward demand, but add
            // additional component which scales with (1/cos(bank angle) - 1) to compensate for induced
            // drag increase during turns.
            float _rollComp = (float)mav.param["TECS_RLL2THR"];
            STEdot_dem = STEdot_dem + _rollComp * (1.0f / (float)MathHelper.constrain(Math.Cos(mav.cs.roll* MathHelper.deg2rad), 0.1f, 1.0f) - 1.0f);
            ff_throttle = nomThr + STEdot_dem / (_STEdot_max - _STEdot_min) * (_THRmaxf - _THRminf);

            // Calculate PD + FF throttle
            _thrDamp = (float)mav.param["TECS_THR_DAMP"];
            float throttle_damp = _thrDamp;
            _throttle_dem = (_STE_error + STEdot_error * throttle_damp) * K_STE2Thr + ff_throttle;

            // Constrain throttle demand
            _throttle_dem = (float)MathHelper.constrain(_throttle_dem, _THRminf, _THRmaxf);

            float THRminf_clipped_to_zero = (float)MathHelper.constrain(_THRminf, 0, _THRmaxf);

            // Rate limit PD + FF throttle
            // Calculate the throttle increment from the specified slew time
            byte throttle_slewrate = (byte)mav.param["THR_SLEWRATE"];
            if (throttle_slewrate != 0)
            {
                float thrRateIncr = _DT * (_THRmaxf - THRminf_clipped_to_zero) * throttle_slewrate * 0.01f;

                _throttle_dem = (float)MathHelper.constrain(_throttle_dem,
                                                _last_throttle_dem - thrRateIncr,
                                                _last_throttle_dem + thrRateIncr);
                _last_throttle_dem = _throttle_dem;
            }

            // Calculate integrator state upper and lower limits
            // Set to a value that will allow 0.1 (10%) throttle saturation to allow for noise on the demand
            // Additionally constrain the integrator state amplitude so that the integrator comes off limits faster.
            float maxAmp = 0.5f * (_THRmaxf - THRminf_clipped_to_zero);
            float integ_max = (float)MathHelper.constrain((_THRmaxf - _throttle_dem + 0.1f), -maxAmp, maxAmp);
            float integ_min = (float)MathHelper.constrain((_THRminf - _throttle_dem - 0.1f), -maxAmp, maxAmp);
            i_gain = (float)mav.param["TECS_INTEG_GAIN"];
            // Calculate integrator state, constraining state
            _integTHR_state = _integTHR_state + (_STE_error * i_gain) * _DT * K_STE2Thr;

            _integTHR_state = (float)MathHelper.constrain(_integTHR_state, integ_min, integ_max);
            // Sum the components.
            _throttle_dem = _throttle_dem + _integTHR_state;

            // Constrain throttle demand
            _throttle_dem = (float)MathHelper.constrain(_throttle_dem, _THRminf, _THRmaxf);
        }
        void _update_pitch(MAVState mav)
        {
            // Calculate Speed/Height Control Weighting
            // This is used to determine how the pitch control prioritises speed and height control
            // A weighting of 1 provides equal priority (this is the normal mode of operation)
            // A SKE_weighting of 0 provides 100% priority to height control. This is used when no airspeed measurement is available
            // A SKE_weighting of 2 provides 100% priority to speed control. This is used when an underspeed condition is detected. In this instance, if airspeed
            // rises above the demanded value, the pitch angle will be increased by the TECS controller.
            float _spdWeight = (float)mav.param["TECS_SPDWEIGHT"];
            float SKE_weighting = (float)MathHelper.constrain(_spdWeight, 0.0f, 2.0f);

            float SPE_weighting = 2.0f - SKE_weighting;

            // Calculate Specific Energy Balance demand, and error
            float SEB_dem = _SPE_dem * SPE_weighting - _SKE_dem * SKE_weighting;
            float SEBdot_dem = _SPEdot_dem * SPE_weighting - _SKEdot_dem * SKE_weighting;
            float SEB_error = SEB_dem - (_SPE_est * SPE_weighting - _SKE_est * SKE_weighting);
            float SEBdot_error = SEBdot_dem - (_SPEdot * SPE_weighting - _SKEdot * SKE_weighting);

            // Calculate integrator state, constraining input if pitch limits are exceeded
            float integSEB_input = SEB_error * i_gain;
            if (_pitch_dem > _PITCHmaxf)
            {
                integSEB_input = Math.Min(integSEB_input, _PITCHmaxf - _pitch_dem);
            }
            else if (_pitch_dem < _PITCHminf)
            {
                integSEB_input = Math.Max(integSEB_input, _PITCHminf - _pitch_dem);
            }
            float integSEB_delta = integSEB_input * _DT;


            // Apply max and min values for integrator state that will allow for no more than
            // 5deg of saturation. This allows for some pitch variation due to gusts before the
            // integrator is clipped. Otherwise the effectiveness of the integrator will be reduced in turbulence
            // During climbout/takeoff, bias the demanded pitch angle so that zero speed error produces a pitch angle
            // demand equal to the minimum value (which is )set by the mission plan during this mode). Otherwise the
            // integrator has to catch up before the nose can be raised to reduce speed during climbout.
            // During flare a different damping gain is used
            float gainInv = (_TAS_state * time_const * GRAVITY_MSS);
            float temp = SEB_error + SEBdot_dem * time_const;

            float pitch_damp = (float)mav.param["TECS_PTCH_DAMP"]; 

            temp += SEBdot_error * pitch_damp;

            float integSEB_min = (gainInv * (_PITCHminf - 0.0783f)) - temp;
            float integSEB_max = (gainInv * (_PITCHmaxf + 0.0783f)) - temp;
            float integSEB_range = integSEB_max - integSEB_min;

            // don't allow the integrator to rise by more than 20% of its full
            // range in one step. This prevents single value glitches from
            // causing massive integrator changes. See Issue#4066
            integSEB_delta = (float)MathHelper.constrain(integSEB_delta, -integSEB_range * 0.1f, integSEB_range * 0.1f);

            // integrate
            _integSEB_state = (float)MathHelper.constrain(_integSEB_state + integSEB_delta, integSEB_min, integSEB_max);

            // Calculate pitch demand from specific energy balance signals
            _pitch_dem_unc = (temp + _integSEB_state) / gainInv;

            // Constrain pitch demand
            _pitch_dem = (float)MathHelper.constrain(_pitch_dem_unc, _PITCHminf, _PITCHmaxf);

            // Rate limit the pitch demand to comply with specified vertical
            // acceleration limit
            _vertAccLim = (float)mav.param["TECS_VERT_ACC"];
            float ptchRateIncr = _DT * _vertAccLim / _TAS_state;

            if ((_pitch_dem - _last_pitch_dem) > ptchRateIncr)
            {
                _pitch_dem = _last_pitch_dem + ptchRateIncr;
            }
            else if ((_pitch_dem - _last_pitch_dem) < -ptchRateIncr)
            {
                _pitch_dem = _last_pitch_dem - ptchRateIncr;
            }

            // re-constrain pitch demand
            _pitch_dem = (float)MathHelper.constrain(_pitch_dem, _PITCHminf, _PITCHmaxf);

            _last_pitch_dem = _pitch_dem;
        }
        float get_tecs_pitch_demand()
        {
            return (float)(_pitch_dem * MathHelper.rad2deg);
        }
        float get_tecs_throttle_demand()
        {
            return _throttle_dem;
        }
        float get_tecs_target_speed()
        {
            return _TAS_dem;
        }
    }
    public class PID
    {
        /*
                    previous_error = 0
                    integral = 0
                    loop:
                    error = setpoint - measured_value
                        integral = integral + error * dt
                    derivative = (error - previous_error) / dt
                        output = Kp * error + Ki * integral + Kd * derivative
                    previous_error = error
                        wait(dt)
                        goto loop*/
        private float _dt;
        private float M_2PI = (float)(Math.PI * 2);
        private float _input;
        private float _derivative;
        public float _kp;
        public float _ki;
        private float _integrator;
        public float _imax;
        public float _kd;
        private float _ff;
        public float _filt_hz = AC_PID_FILT_HZ_DEFAULT;

        const float AC_PID_FILT_HZ_DEFAULT = 20.0f; // default input filter frequency
        const float AC_PID_FILT_HZ_MIN = 0.01f; // minimum input filter frequency

        // Constructor
        public PID(float initial_p, float initial_i, float initial_d, float initial_imax, float initial_filt_hz, float dt, float initial_ff)
        {
            _dt = dt;
            _integrator = 0.0f;
            _input = 0.0f;
            _derivative = 0.0f;

            _kp = initial_p;
            _ki = initial_i;
            _kd = initial_d;
            _imax = Math.Abs(initial_imax);
            filt_hz(initial_filt_hz);
            _ff = initial_ff;

            // reset input filter to first value received
            _flags._reset_filter = true;
        }
        public void set_kp(float kp)
        {
            _kp = kp;
        }
        public void set_ki(float ki)
        {
            _ki = ki;
        }
        public void set_kd(float kd)
        {
            _kd = kd;
        }
        public void set_imax(float imax)
        {
            _imax = Math.Abs(imax);
        }
        public void set_dt(float dt)
        {
            _dt = dt;
        }
        public void set_filthz(float filthz)
        {
            filt_hz(filthz);
        }
        public void set_ff(float ff)
        {
            _ff = ff;
        }
        // filt_hz - set input filter hz
        public void filt_hz(float hz)
        {
            _filt_hz = hz;

            // sanity check _filt_hz
            _filt_hz = Math.Max(_filt_hz, AC_PID_FILT_HZ_MIN);
        }

        public void set_input_filter_all(float input)
        {
            // don't process inf or NaN
            if (!isfinite(input))
            {
                return;
            }

            // reset input filter to value received
            if (_flags._reset_filter)
            {
                _flags._reset_filter = false;
                _input = input;
                _derivative = 0.0f;
            }

            // update filter and calculate derivative
            float input_filt_change = get_filt_alpha() * (input - _input);
            _input = _input + input_filt_change;
            if (_dt > 0.0f)
            {
                _derivative = input_filt_change / _dt;
            }
        }

        private bool isfinite(float input)
        {
            return !float.IsInfinity(input);
        }

        public float get_p()
        {
            _pid_info.P = (_input * _kp);
            return _pid_info.P;
        }

        public float get_i()
        {
            if (!is_zero(_ki) && !is_zero(_dt))
            {
                _integrator += ((float)_input * _ki) * _dt;
                if (_integrator < -_imax)
                {
                    _integrator = -_imax;
                }
                else if (_integrator > _imax)
                {
                    _integrator = _imax;
                }

                _pid_info.I = _integrator;
                return _integrator;
            }

            return 0;
        }

        public float get_d()
        {
            // derivative component
            _pid_info.D = (_kd * _derivative);
            return _pid_info.D;
        }

        public float get_ff(float requested_rate)
        {
            _pid_info.FF = (float)requested_rate * _ff;
            return _pid_info.FF;
        }

        public float get_pi()
        {
            return get_p() + get_i();
        }

        public float get_pid()
        {
            return get_p() + get_i() + get_d();
        }

        public void reset_I()
        {
            _integrator = 0;
        }

        public float get_filt_alpha()
        {
            if (is_zero(_filt_hz))
            {
                return 1.0f;
            }

            // calculate alpha
            float rc = 1 / (M_2PI * _filt_hz);
            return _dt / (_dt + rc);
        }

        private bool is_zero(float filt_hz)
        {
            return filt_hz == 0;
        }

        internal class flags
        {
            internal bool _reset_filter;
        }

        flags _flags = new flags();

        pid_info _pid_info = new pid_info();

        internal class pid_info
        {
            internal float P;
            internal float I;
            internal float D;
            internal float FF;
        }
    }


}