using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;
using ProjNet.Converters;
using MissionPlanner;
using MissionPlanner.Utilities;
using System.Net.Sockets;
using System.Net;
using System.IO;
using MissionPlanner.ArduPilot;
using ZedGraph;
using MissionPlanner.Controls;
using DirectShowLib;
using System.Runtime.InteropServices;
using static MissionPlanner.GCSViews.ConfigurationView.ConfigPlanner;
using WebCamService;
using MissionPlanner.GCSViews.ConfigurationView;
using System.Drawing.Imaging;
using MissionPlanner.Comms;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET;
using MissionPlanner.GCSViews;
using MissionPlanner.Maps;
using GMap.NET.ObjectModel;
using MissionPlanner.HIL;

namespace MissionPlanner.Swarm
{
    public partial class FormationControl : Form
    {
        Formation SwarmInterface = null;
        bool threadrun = false;
        // UDP接收相关
        struct UdpStateStruct
        {
            public UdpClient UdpClient;
            public IPAddress IP;
            public int Port;
            public IPEndPoint LocalIPEndPoint;
            public IPEndPoint remoteIpAndPort;
            public bool isBind;
            public int cnt;
        }
        UdpStateStruct udpState,udpSend;
        FileStream fs = null;
        StreamReader streamReader = null;
        List<string> lines = new List<string>();
        List<string> formationshapelines = new List<string>();
        Tuple<PID, PID, PID, PID,PID> defaultpid = null;
        uint maybecollisiontimes = 0;
        struct FormationMonitoring
        {
            public uint armmavcount;
            public uint healthlinkmavcount;
            public uint guidedmodemavcount;
            public uint totalhavemavcount;
            public uint lowspeedmavcount;
            public uint lowaltmavcount;
        }
        FormationMonitoring formationMonitoring;
        public struct FormationOverallParameters
        {
            public uint CopterTakeoffAlt_m;
            public float FailTimeHorizon;
            public float WarnTimeHorizon;
            public float FailDistance_xy;
            public float FailDistance_z;
            public float WarnDistance_xy;
            public float WarnDistance_z;
            public float FailAltitudeMinimum; // 最低的高度限制，低于此高度不避障
            public uint AvoidanceVertical_m;
            public uint AvoidanceHorizontal_m;
            public int FailAction;
            public bool AvoidanceEnable;
            public float MinApproachTime_s;
            public bool VirtualLeaderEnable;
            public bool APOilPower;
            public bool UdpTransferEnable;
        }
        FormationOverallParameters Formationoverallparameters;
        public class Vector3Array
        {
            public Vector3[] vector3Array;

            public Vector3Array(uint count)
            {
                //记录所有添加的数量
                vector3Array = new Vector3[count];
                //初始化数组里面的元素
                for (int i = 0; i < count; i++)
                {
                    vector3Array[i] = new Vector3();
                }
                Count = count;
            }
            public uint Count = 0;
        }
        List<Vector3Array> formationShapeList = new List<Vector3Array>();
        int tickStart;
        RollingPointPairList list1 = new RollingPointPairList(1200);
        RollingPointPairList list2 = new RollingPointPairList(1200);
        RollingPointPairList list3 = new RollingPointPairList(1200);
        RollingPointPairList list4 = new RollingPointPairList(1200);
        RollingPointPairList list5 = new RollingPointPairList(1200);
        RollingPointPairList list6 = new RollingPointPairList(1200);
        RollingPointPairList list7 = new RollingPointPairList(1200);
        RollingPointPairList list8 = new RollingPointPairList(1200);
        RollingPointPairList list9 = new RollingPointPairList(1200);
        RollingPointPairList list10 = new RollingPointPairList(1200);
        RollingPointPairList list11 = new RollingPointPairList(1200);
        RollingPointPairList list12 = new RollingPointPairList(1200);
        RollingPointPairList list13 = new RollingPointPairList(1200);
        RollingPointPairList list14 = new RollingPointPairList(1200);
        RollingPointPairList list15 = new RollingPointPairList(1200);
        RollingPointPairList list16 = new RollingPointPairList(1200);
        RollingPointPairList list17 = new RollingPointPairList(1200);

        CurveItem list1curve;
        CurveItem list2curve;
        CurveItem list3curve;
        CurveItem list4curve;
        CurveItem list5curve;
        CurveItem list6curve;
        CurveItem list7curve;
        CurveItem list8curve;
        CurveItem list9curve;
        CurveItem list10curve;
        CurveItem list11curve;
        CurveItem list12curve;
        CurveItem list13curve;
        CurveItem list14curve;
        CurveItem list15curve;
        CurveItem list16curve;
        CurveItem list17curve;

        CheckBox targetalt = null;
        CheckBox alt = null;
        CheckBox navpitch = null;
        CheckBox pitch = null;
        CheckBox distotarget = null;
        CheckBox navthurst = null;
        CheckBox navroll = null;
        CheckBox roll = null;
        CheckBox targetyaw = null;
        CheckBox yaw = null;
        CheckBox navbearing = null;
        CheckBox yawerror = null;
        CheckBox disttoleader = null;
        CheckBox targetspeed = null;
        CheckBox groudspeed = null;
        CheckBox courceoftarget = null;
        CheckBox speedoftarget = null;
        // 多视频显示
        public WebCamService.Capture cam1,cam2;
        //List<WebCamService.Capture> camsList = new List<WebCamService.Capture>();
        public static FormationControl instance = null;
        GMapMarker center = new GMarkerGoogle(new PointLatLng(0.0, 0.0), GMarkerGoogleType.none);
        GMapMarker start = new GMarkerGoogle(new PointLatLng(0.0, 0.0), GMarkerGoogleType.none);

        Dictionary<MAVState, SimUDPLink> HILLinks = new Dictionary<MAVState, SimUDPLink>();
        Dictionary<MAVState, Hil> HILSims = new Dictionary<MAVState, Hil>();
        string lastShape = "";
        int lastDistance = -1;

        public enum ShapeChangeAction
        {
            NONE = 0,
            HORIZONTAL2VERTICAL,
            VERTICA2HORIZONTAL,
            HORIZONTAL2TILT,
            TILT2VERTICAL,
            VERTICAL2TILT,
            TILT2HORIZONTAL,
            HORIZONTAL2HORIZONTAL,
            VERTICA2VERTICA,
            TILT2TILT,
        }

        ShapeChangeAction shapeChangeAction = ShapeChangeAction.NONE;
        Dictionary<MAVState, int> mavIndex = new Dictionary<MAVState, int>();

        public FormationControl()
        {
            InitializeComponent();

            SwarmInterface = new Formation();

            TopMost = true;

            Dictionary<String,MAVState> mavStates = new Dictionary<string, MAVState>();

            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    mavStates.Add(port.BaseStream.PortName + " " + mav.sysid + " " + mav.compid, mav);
                }
            }
            comboBox_video1imageHZ.SelectedIndex = 0;
            comboBox_video2imageHZ.SelectedIndex = 0;
            CMB_PortNameClick(this,null);
            cmb_baudrate.SelectedIndex = 0;
            cmb_sendrate.SelectedIndex = 0;
            CMB_UdpSendRate.SelectedIndex = 0;
            MapLoad();
            SearchInfoLoad();
            if (mavStates.Count == 0)
            {
                MessageBox.Show("检测没有飞机连入，请先连接上所有飞机再进入编队界面");
                return;
            }                
            bindingSource_1.DataSource = mavStates;
            CMB_mavs.DataSource = bindingSource_1;
            CMB_mavs.ValueMember = "Value";
            CMB_mavs.DisplayMember = "Key";

            bindingSource_2.DataSource = mavStates;
            CMB_pid.DataSource = bindingSource_2;
            CMB_pid.ValueMember = "Value";
            CMB_pid.DisplayMember = "Key";

            comboBox_chartshow.DataSource = bindingSource_2;
            comboBox_chartshow.ValueMember = "Value";
            comboBox_chartshow.DisplayMember = "Key";


            bindingSource_3.DataSource = mavStates;
            CMB_3DMAP.DataSource = bindingSource_3;
            CMB_3DMAP.ValueMember = "Value";
            CMB_3DMAP.DisplayMember = "Key";

            bindingSource_4.DataSource = mavStates;
            comboBox_mavsoffset.DataSource = bindingSource_4;
            comboBox_mavsoffset.ValueMember = "Value";
            comboBox_mavsoffset.DisplayMember = "Key";
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    SwarmInterface.setgrid2Offsets(mav, 0, 0, 0);
                }
            }
            updateicons();
            grid2updateicons();
            udpState.isBind = false;
            udpState.cnt = 0;
            udpSend.isBind = false;
            udpSend.cnt = 0;

            this.MouseWheel += new MouseEventHandler(FollowLeaderControl_MouseWheel);

            //MessageBox.Show("编队控制存在高风险，请确保您已熟悉全部流程");
            // 第一步先设置默认的高级参数
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    mav.sawmAdvancedParameters.targettrailerscale = (float)target_trailer_scale.Value;
                    mav.sawmAdvancedParameters.targetleaderdis_m = (int)target_leader_dist_m.Value;
                    mav.sawmAdvancedParameters.yawdistanceboundary_m = (int)yaw_distance_boundary_m.Value;
                    mav.sawmAdvancedParameters.rolllowdistboundary_m = (int)roll_low_dist_boundary_m.Value;
                    mav.sawmAdvancedParameters.rollhighdistboundary_m = (int)roll_high_dist_boundary_m.Value;
                    mav.sawmAdvancedParameters.rollmindisttotarget_m = (int)roll_min_disttotarget_m.Value;
                    mav.sawmAdvancedParameters.yawtorollmaxangle_deg = (int)yaw_to_roll_max_angle_deg.Value;
                    mav.sawmAdvancedParameters.maxdisttodspd_m = (int)max_dist_to_delta_speed_m.Value;
                    mav.sawmAdvancedParameters.distintegseparation_m = (int)dist_integral_separation_m.Value;
                    mav.sawmAdvancedParameters.deltaspeedscale = (float)delta_speed_scale.Value;
                    mav.sawmAdvancedParameters.thrusttrim = (float)thrust_trim.Value;
                    mav.sawmAdvancedParameters.thrustmin = (float)thrust_min.Value;
                    mav.sawmAdvancedParameters.mintargetspeed = (int)min_target_speed.Value;
                    mav.sawmAdvancedParameters.predistancetolosespeed = (int)pre_distance_to_lose_speed.Value;
                    mav.sawmAdvancedParameters.stallprotect = stall_protect.Value == 1 ? true : false;
                }
            }
            Formationoverallparameters.CopterTakeoffAlt_m = (uint)copter_takeoff_m.Value;
            Formationoverallparameters.FailTimeHorizon = (float)fail_time_horizon_s.Value;
            Formationoverallparameters.WarnTimeHorizon = (float)warn_time_horizon_s.Value;
            Formationoverallparameters.FailDistance_xy = (float)fail_distance_xy_m.Value;
            Formationoverallparameters.FailDistance_z = (float)fail_distance_z_m.Value;
            Formationoverallparameters.WarnDistance_xy = (float)warn_distance_xy_m.Value;
            Formationoverallparameters.WarnDistance_z = (float)warn_distance_z_m.Value;
            Formationoverallparameters.FailAltitudeMinimum = (float)fail_alt_min_m.Value;
            Formationoverallparameters.AvoidanceVertical_m = (uint)avoidance_vertical_m.Value;
            Formationoverallparameters.AvoidanceHorizontal_m = (uint)avoidance_horizontal_m.Value;
            Formationoverallparameters.FailAction = (int)fail_action.Value;
            Formationoverallparameters.AvoidanceEnable = avoidance_enable.Value ==1 ? true:false;
            Formationoverallparameters.MinApproachTime_s = (float)min_approach_times.Value;
            Formationoverallparameters.VirtualLeaderEnable = virtualleaderenable.Value == 1 ? true:false;
            Formationoverallparameters.APOilPower = AP_oil_power.Value == 1 ? true : false;
            Formationoverallparameters.UdpTransferEnable = udp_transfer_enable.Value == 1 ? true : false;
            ReadParam();
            SwarmInterface.avoidance.set_params(Formationoverallparameters);
            SwarmInterface.setPowerType(Formationoverallparameters.APOilPower);
            setUdpMirrorTransferEnable(Formationoverallparameters.UdpTransferEnable);
            lbl_firstgrammer.Text = "一级语意:";
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == CMB_pid.SelectedValue)
                    {
                        updateParam(mav, true);
                    }
                    lbl_firstgrammer.Text += mav.ToString() + "号机"+" ";
                }
            }
            if (fs!=null)
            {
                fs.Close();
            }
            if (streamReader!=null)
            {
                streamReader.Close();
            }
            CreateChart(zg1);
            AddVaribleCheckBox();
            UpdateSettings();
            timer3.Enabled = true;
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    SimUDPLink simUDPLink = new SimUDPLink();
                    simUDPLink.setConnectMav(port.BaseStream.PortName + " " + mav.sysid + " " + mav.compid);
                    this.flowLayoutPanel_udplinks.Controls.Add(simUDPLink);
                    HILLinks[mav] = simUDPLink;
                }
            }
            MissionPlanner.Utilities.Tracking.AddPage(this.GetType().ToString(), this.Text);
            swarmControlState = 1;
            isInMirroringUdpData = false;
            btn_mirrorUdpForward.Enabled = true;
            instance = this;
        }
        public GMapOverlay searchareaoverlay;
        public GMapOverlay virtualtargetoverlay;
        GMapOverlay targetsfoundoverlay;
        GMapOverlay routesOverlay;
        GMapOverlay mavsOverlay;
        void MapLoad()
        {
            gMap.MapProvider = MainV2.instance.FlightData.gMapControl1.MapProvider;
            gMap.CacheLocation = Settings.GetDataDirectory() + "gmapcache" + Path.DirectorySeparatorChar;
            gMap.MinZoom = 0;
            gMap.MaxZoom = 24;
            gMap.Zoom = 3;
            gMap.DisableFocusOnMouseEnter = true;
            //gMap.RoutesEnabled = true;
            gMap.PolygonsEnabled = true;
            gMap.OnMapZoomChanged += gMap_OnMapZoomChanged;
            gMap.OnPositionChanged += gMap_OnCurrentPositionChanged;
            if (Settings.Instance["maplast_lat"] != "")
            {
                try
                {
                    gMap.Position = new PointLatLng(Settings.Instance.GetDouble("maplast_lat"),
                        Settings.Instance.GetDouble("maplast_lng"));
                    if (Math.Round(Settings.Instance.GetDouble("maplast_lat"), 1) == 0)
                    {
                        // no zoom in
                        track_zoom.Value = 3;
                    }
                    else
                    {
                        var zoom = Settings.Instance.GetFloat("maplast_zoom");
                        track_zoom.Value = zoom;
                    }
                    if (gMap.MaxZoom + 1 == (double)track_zoom.Value)
                    {
                        gMap.Zoom = track_zoom.Value - .1;
                    }
                    else
                    {
                        gMap.Zoom = track_zoom.Value;
                    }
                }
                catch
                {
                }
            }

            PointLatLng mavposition = GetMavPosition();
            if (mavposition.Lat != 0 && mavposition.Lng != 0)
            {
                gMap.Position = new PointLatLng(mavposition.Lat, mavposition.Lng);
            }
            center = new GMarkerGoogle(gMap.Position,GMarkerGoogleType.none);
            start = new GMarkerGoogle(gMap.Position, GMarkerGoogleType.none);
            searchareaoverlay = new GMapOverlay("search area overlay");
            gMap.Overlays.Add(searchareaoverlay);

            virtualtargetoverlay = new GMapOverlay("virtual target overlay");
            gMap.Overlays.Add(virtualtargetoverlay);

            targetsfoundoverlay = new GMapOverlay("targets found");
            gMap.Overlays.Add(targetsfoundoverlay);

            routesOverlay = new GMapOverlay("routes");
            gMap.Overlays.Add(routesOverlay);

            mavsOverlay = new GMapOverlay("mavs icon");
            gMap.Overlays.Add(mavsOverlay);
            mavsOverlay.Markers.Clear();

            // 清除标记
            targetsFoundListRecord.Clear();
            targetsFound.Items.Clear();
            targetsfoundoverlay.Markers.Clear();

            routesOverlay.Markers.Clear();
            routesOverlay.Polygons.Clear();
            routesOverlay.Routes.Clear();
        }
        void SearchInfoLoad()
        {
            if (FlightData.instance != null)
            {
                lbl_serachareacounts.Text = FlightData.instance.searchareaoverlay.Polygons.Count.ToString();
                searchareaoverlay.Polygons.Clear();
                virtualtargetoverlay.Markers.Clear();
                foreach (var polygon in FlightData.instance.searchareaoverlay.Polygons)
                {
                    List<PointLatLng> points = new List<PointLatLng>();
                    GMapPolygon searchPolygon = new GMapPolygon(points, "serachPolygon");
                    searchPolygon.Fill = new SolidBrush(Color.FromArgb(50, Color.Blue));
                    searchPolygon.Stroke = new Pen(Color.Blue, 2);
                    foreach (var point in polygon.Points)
                    {
                        searchPolygon.Points.Add(new PointLatLng(point.Lat, point.Lng));
                    }
                    searchareaoverlay.Polygons.Add(searchPolygon);
                }
                foreach (GMapMarkerVirtualTarget marker in FlightData.instance.virtualtargetoverlay.Markers)
                {
                    GMapMarkerVirtualTarget gMapMarker = new GMapMarkerVirtualTarget(marker.Position, marker.heading)
                    {
                        ToolTipMode = MarkerTooltipMode.OnMouseOver,
                        ToolTipText = "Virtual Target",
                        Tag = FlightData.instance.virtualtargetoverlay.Markers.IndexOf(marker),
                    };
                    virtualtargetoverlay.Markers.Add(gMapMarker);
                }
            }
            searchaltset = (double)numericUpDown_searchalt.Value;
            overshot = (double)numericUpDown_overshot.Value;
            createWaypointListFromPolygons(searchaltset, overshot,100);
        }

        // polygon中的points集合
        List<List<PointLatLngAlt>> lists = new List<List<PointLatLngAlt>>();
        // polygon生成航点集合
        List<List<PointLatLngAlt>> grids = new List<List<PointLatLngAlt>>();
        private void createWaypointListFromPolygons(double alt, double overshot_m,double distance)
        {
            // 取出每个polygon中的points
            lists.Clear();
            foreach (var polygon in searchareaoverlay.Polygons)
            {
                List<PointLatLngAlt> list = new List<PointLatLngAlt>();
                polygon.Points.ForEach(x => { list.Add(x); });
                lists.Add(list);
            }
            // 生成grid航点集合
            grids.Clear();
            foreach (var list in lists)
            {
                List<PointLatLngAlt> grid = new List<PointLatLngAlt>();
                double angle = (getAngleOfLongestSide(list) + 360) % 360;
                grid = Utilities.Grid.CreateGrid(list, alt, distance, 0, angle, overshot_m, overshot_m, Utilities.Grid.StartPosition.BottomLeft, false, 0, (float)overshot_m, MainV2.comPort.MAV.cs.HomeLocation);
                grids.Add(grid);
            }
        }
        private double getAngleOfLongestSide(List<PointLatLngAlt> list)
        {
            if (list.Count == 0)
                return 0;
            double angle = 0;
            double maxdist = 0;
            PointLatLngAlt last = list[list.Count - 1];
            foreach (var item in list)
            {
                if (item.GetDistance(last) > maxdist)
                {
                    angle = item.GetBearing(last);
                    maxdist = item.GetDistance(last);
                }
                last = item;
            }

            return (angle + 360) % 360;
        }

        private PointLatLng GetMavPosition()
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav.cs.lat != 0 && mav.cs.lng != 0)
                    {
                        return new PointLatLng(mav.cs.lat, mav.cs.lng);
                    }
                }
            }
            return new PointLatLng(0, 0);
        }
        void UpdateSettings()
        {
            try
            {
                if (Settings.Instance["video1_device"] != null)
                {
                    CMB_video1sources_Click(this, null);
                    CMB_video1sources.SelectedIndex = Settings.Instance.GetInt32("video1_device");

                    if (Settings.Instance["video1_options"] != "" && CMB_video1sources.Text != "")
                    {
                        CMB_video1resolutions.SelectedIndex = Settings.Instance.GetInt32("video1_options");
                    }
                }
                if (Settings.Instance["video2_device"] != null)
                {
                    CMB_video2sources_Click(this, null);
                    CMB_video2sources.SelectedIndex = Settings.Instance.GetInt32("video2_device");

                    if (Settings.Instance["video2_options"] != "" && CMB_video2sources.Text != "")
                    {
                        CMB_video2resolutions.SelectedIndex = Settings.Instance.GetInt32("video2_options");
                    }
                }
                if (Settings.Instance["video1_pathfile"] != null)
                {
                    txt_video1path.Text = Settings.Instance["video1_pathfile"];
                }
                if (Settings.Instance["video2_pathfile"] != null)
                {
                    txt_video2path.Text = Settings.Instance["video2_pathfile"];
                }
            }
            catch
            {
            }
        }
        void AddVaribleCheckBox()
        {
            //CheckBox targetAlt
            targetalt = new CheckBox();
            targetalt.Name = "targetalt";
            targetalt.AutoSize = true;
            targetalt.Font = new Font("宋体", 11);
            targetalt.Text = "target_alt_m";
            this.variable_check.Controls.Add(targetalt);
            //CheckBox Alt
            alt = new CheckBox();
            alt.Name = "alt";
            alt.AutoSize = true;
            alt.Font = new Font("宋体", 11);
            alt.Text = "alt_m";
            this.variable_check.Controls.Add(alt);
            //CheckBox navpitch
            navpitch = new CheckBox();
            navpitch.Name = "navpitch";
            navpitch.AutoSize = true;
            navpitch.Font = new Font("宋体", 11);
            navpitch.Text = "navpitch_deg";
            this.variable_check.Controls.Add(navpitch);
            //CheckBox pitch
            pitch = new CheckBox();
            pitch.Name = "pitch";
            pitch.AutoSize = true;
            pitch.Font = new Font("宋体", 11);
            pitch.Text = "pitch_deg";
            this.variable_check.Controls.Add(pitch);
            //CheckBox distotarget
            distotarget = new CheckBox();
            distotarget.Name = "distotarget";
            distotarget.AutoSize = true;
            distotarget.Font = new Font("宋体", 11);
            distotarget.Text = "dist_to_target_m";
            this.variable_check.Controls.Add(distotarget);
            //CheckBox navthurst
            navthurst = new CheckBox();
            navthurst.Name = "navthurst";
            navthurst.AutoSize = true;
            navthurst.Font = new Font("宋体", 11);
            navthurst.Text = "nav_thurst";
            this.variable_check.Controls.Add(navthurst);
            //CheckBox navroll
            navroll = new CheckBox();
            navroll.Name = "navroll";
            navroll.AutoSize = true;
            navroll.Font = new Font("宋体", 11);
            navroll.Text = "nav_roll_deg";
            this.variable_check.Controls.Add(navroll);
            //CheckBox roll
            roll = new CheckBox();
            roll.Name = "roll";
            roll.AutoSize = true;
            roll.Font = new Font("宋体", 11);
            roll.Text = "roll_deg";
            this.variable_check.Controls.Add(roll);
            //CheckBox targetyaw
            targetyaw = new CheckBox();
            targetyaw.Name = "targetyaw";
            targetyaw.AutoSize = true;
            targetyaw.Font = new Font("宋体", 11);
            targetyaw.Text = "target_yaw_deg";
            this.variable_check.Controls.Add(targetyaw);
            //CheckBox yaw
            yaw = new CheckBox();
            yaw.Name = "yaw";
            yaw.AutoSize = true;
            yaw.Font = new Font("宋体", 11);
            yaw.Text = "yaw_deg";
            this.variable_check.Controls.Add(yaw);
            //CheckBox navbearing
            navbearing = new CheckBox();
            navbearing.Name = "navbearing";
            navbearing.AutoSize = true;
            navbearing.Font = new Font("宋体", 11);
            navbearing.Text = "nav_bearing_deg";
            this.variable_check.Controls.Add(navbearing);
            //CheckBox yawerror
            yawerror = new CheckBox();
            yawerror.Name = "yawerror";
            yawerror.AutoSize = true;
            yawerror.Font = new Font("宋体", 11);
            yawerror.Text = "yaw_error_deg";
            this.variable_check.Controls.Add(yawerror);
            //CheckBox disttoleader
            disttoleader = new CheckBox();
            disttoleader.Name = "disttoleader";
            disttoleader.AutoSize = true;
            disttoleader.Font = new Font("宋体", 11);
            disttoleader.Text = "dist_to_leader_m";
            this.variable_check.Controls.Add(disttoleader);
            //CheckBox targetspeed
            targetspeed = new CheckBox();
            targetspeed.Name = "targetspeed";
            targetspeed.AutoSize = true;
            targetspeed.Font = new Font("宋体", 11);
            targetspeed.Text = "targetspeed_m/s";
            this.variable_check.Controls.Add(targetspeed);
            //CheckBox groudspeed
            groudspeed = new CheckBox();
            groudspeed.Name = "groudspeed";
            groudspeed.AutoSize = true;
            groudspeed.Font = new Font("宋体", 11);
            groudspeed.Text = "groudspeed_m/s";
            this.variable_check.Controls.Add(groudspeed);
            // CheckBox courceoftarget
            courceoftarget = new CheckBox();
            courceoftarget.Name = "courceoftarget";
            courceoftarget.AutoSize = true;
            courceoftarget.Font = new Font("宋体", 11);
            courceoftarget.Text = "courceoftarget_deg";
            this.variable_check.Controls.Add(courceoftarget);
            // CheckBox speedoftarget
            speedoftarget = new CheckBox();
            speedoftarget.Name = "speedoftarget";
            speedoftarget.AutoSize = true;
            speedoftarget.Font = new Font("宋体", 11);
            speedoftarget.Text = "speedoftarget_m/s";
            this.variable_check.Controls.Add(speedoftarget);
        }
        public void CreateChart(ZedGraphControl zgc)
        {
            GraphPane myPane = zgc.GraphPane;

            // 设置标题
            myPane.Title.Text = "Tuning";
            myPane.XAxis.Title.Text = "Time (s)";
            myPane.YAxis.Title.Text = "Unit";

            // 显示X轴的栅格
            myPane.XAxis.MajorGrid.IsVisible = true;

            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = 5;

            myPane.YAxis.Scale.FontSpec.FontColor = Color.White;
            myPane.YAxis.Title.FontSpec.FontColor = Color.White;

            myPane.YAxis.MajorTic.IsOpposite = false;
            myPane.YAxis.MinorTic.IsOpposite = false;
            // Y轴为0则不显示
            myPane.YAxis.MajorGrid.IsZeroLine = true;
            // 对齐设置
            myPane.YAxis.Scale.Align = AlignP.Inside;
            // 定义定时器周期
            ZedGraphTimer.Interval = 200;

            tickStart = Environment.TickCount;
        }
        void FollowLeaderControl_MouseWheel(object sender, MouseEventArgs e)
        {
            // 更新编队控制界面
            if (tabControl1.SelectedTab.Name.Equals(tabPage1.Name))
            {
                if (e.Delta < 0)
                {
                    grid1.setScale(grid1.getScale() + 5);
                }
                else
                {
                    grid1.setScale(grid1.getScale() - 5);
                }
            }   // 更新队形变换界面
            else if (tabControl1.SelectedTab.Name.Equals(tabPage4.Name))
            {
                if (e.Delta < 0)
                {
                    grid2.setScale(grid2.getScale() + 5);
                }
                else
                {
                    grid2.setScale(grid2.getScale() - 5);
                }
            }
        }
        void grid2updateicons()
        {
            grid2.ClearOnlyShow();
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    var vector2 = SwarmInterface.getgrid2Offsets(mav);
                    grid2.UpdateIcon(mav, (float)vector2.x, (float)vector2.y, (float)vector2.z, true, Color.Red);
                }
            }
            grid2.Invalidate();
        }
        void updateicons()
        {
            bindingSource_1.ResetBindings(false);

            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == SwarmInterface.getLeader())
                    {
                        ((Formation) SwarmInterface).setOffsets(mav, 0, 0, 0);
                        var vector = SwarmInterface.getOffsets(mav);
                        grid1.UpdateIcon(mav, (float) vector.x, (float) vector.y, (float) vector.z, false, Color.Red);
                    }
                    else
                    {
                        var vector = SwarmInterface.getOffsets(mav);
                        grid1.UpdateIcon(mav, (float) vector.x, (float) vector.y, (float) vector.z, true, Color.Red);
                    }
                }
            }
            grid1.Invalidate();
        }

        private void CMB_mavs_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == CMB_mavs.SelectedValue)
                    {
                        MainV2.comPort = port;
                        port.sysidcurrent = mav.sysid;
                        port.compidcurrent = mav.compid;
                    }
                }
            }
        }

        private void BUT_Start_Click(object sender, EventArgs e)
        {
            if (threadrun == true)
            {
                threadrun = false;
                BUT_Start.Text = Strings.Start;
                return;
            }
            // 先让搜索停止
            if (gotorearch)
            {
                BTN_search_Click(this, null);
            }
            if (SwarmInterface != null)
            {
                new System.Threading.Thread(mainloop) {IsBackground = true}.Start();
                BUT_Start.Text = Strings.Stop;
            }
            lastShape = "";
            lastDistance = -1;
        }

        void mainloop()
        {
            threadrun = true;
            SwarmInterface.Leader.parent.requestDatastream(MAVLink.MAV_DATA_STREAM.POSITION, 10, SwarmInterface.Leader.sysid, SwarmInterface.Leader.compid);
            SwarmInterface.Leader.cs.rateposition = 10;
            SwarmInterface.Leader.cs.rateattitude = 10;
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    SwarmInterface.setNavigation(mav);
                }
            }

            if (SwarmInterface != null)
            {
                SwarmInterface.GuidedMode();
            }

            while (threadrun && !this.IsDisposed)
            {
                // update leader pos
                SwarmInterface.Update();
                if (Formationoverallparameters.AvoidanceEnable)
                {
                    SwarmInterface.ObstacleAvoidanceAlgorithm(Formationoverallparameters.VirtualLeaderEnable,true);
                }
                else
                {
                    Reset_Obstacle_ControlVecter();
                }
                // update other mavs
                SwarmInterface.SendCommand();

                // 10 hz
                System.Threading.Thread.Sleep(100);
            }
        }
        private void Reset_Obstacle_ControlVecter()
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    mav.obsvector.zero();
                    mav.obstacleid = 0;
                }
            }
        }
        private void BUT_Arm_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                SwarmInterface.Arm();
                swarmControlState = 2;
            }
        }

        private void BUT_Disarm_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                SwarmInterface.Disarm();
                swarmControlState = 3;
            }
        }

        private void BUT_Takeoff_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                SwarmInterface.Takeoff(Formationoverallparameters.CopterTakeoffAlt_m);
                swarmControlState = 4;
            }
        }

        private void BUT_Land_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                SwarmInterface.Land();
            }
        }
        private void BTN_RTL(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                SwarmInterface.RTL();
            }
        }
        private void BUT_Auto_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                SwarmInterface.Auto();
            }
        }
        private void BUT_leader_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                var vectorlead = SwarmInterface.getOffsets(MainV2.comPort.MAV);

                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        var vector = SwarmInterface.getOffsets(mav);

                        SwarmInterface.setOffsets(mav, (float) (vector.x - vectorlead.x),
                            (float) (vector.y - vectorlead.y),
                            (float) (vector.z - vectorlead.z));
                    }
                }

                SwarmInterface.setLeader(MainV2.comPort.MAV);
                updateicons();
                BUT_Start.Enabled = true;
                BUT_Updatepos.Enabled = true;
                Btn_Stop.Enabled = true;
            }
        }

        private void BUT_connect_Click(object sender, EventArgs e)
        {
            Comms.CommsSerialScan.Scan(true);

            DateTime deadline = DateTime.Now.AddSeconds(50);

            while (Comms.CommsSerialScan.foundport == false)
            {
                System.Threading.Thread.Sleep(100);

                if (DateTime.Now > deadline)
                {
                    CustomMessageBox.Show("Timeout waiting for autoscan/no mavlink device connected");
                    return;
                }
            }

            bindingSource_1.ResetBindings(false);
        }

        public Vector3 getOffsetFromLeader(MAVState leader, MAVState mav)
        {
            //convert Wgs84ConversionInfo to utm
            CoordinateTransformationFactory ctfac = new CoordinateTransformationFactory();

            GeographicCoordinateSystem wgs84 = GeographicCoordinateSystem.WGS84;

            int utmzone = (int) ((leader.cs.lng - -186.0)/6.0);

            IProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(utmzone,
                leader.cs.lat < 0 ? false : true);

            ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(wgs84, utm);

            double[] masterpll = {leader.cs.lng, leader.cs.lat};

            // get leader utm coords
            double[] masterutm = trans.MathTransform.Transform(masterpll);

            double[] mavpll = {mav.cs.lng, mav.cs.lat};

            //getLeader follower utm coords
            double[] mavutm = trans.MathTransform.Transform(mavpll);

            var heading = -leader.cs.yaw;

           var norotation = new Vector3(masterutm[1] - mavutm[1], masterutm[0] - mavutm[0], 0);

            norotation.x *= -1;
            norotation.y *= -1;

            return new Vector3( norotation.x * Math.Cos(heading * MathHelper.deg2rad) - norotation.y * Math.Sin(heading * MathHelper.deg2rad), norotation.x * Math.Sin(heading * MathHelper.deg2rad) + norotation.y * Math.Cos(heading * MathHelper.deg2rad), 0);
        }

        private void grid1_UpdateOffsets(MAVState mav, float x, float y, float z, Grid.icon ico)
        {
            if (mav == SwarmInterface.Leader)
            {
                CustomMessageBox.Show("Can not move Leader");
                ico.z = 0;
            }
            else
            {
                ((Formation) SwarmInterface).setOffsets(mav, x, y, z);
            }
        }
        private void grid2_UpdateOffsets(MAVState mav, float x, float y, float z, Grid.icon ico)
        {
            ((Formation)SwarmInterface).setgrid2Offsets(mav, x, y, z);
        }
        private void Control_FormClosing(object sender, FormClosingEventArgs e)
        {
            WriteParam();
            if (cam1 != null)
            {
                cam1.Dispose();
                cam1 = null;
            }
            if (cam2 != null)
            {
                cam2.Dispose();
                cam2 = null;
            }
            if (map3D != null)
            {
                map3D.Dispose();
                map3D = null;
            }
            if (video1AviWrite != null)
            {
                BTN_Video1WriteStop(this, null);
            }
            if (video2AviWrite != null)
            {
                BTN_Video2WriteStop(this, null);
            }
            newRecognizedline = "";
            oldRecognizedline = "";
            if (voiceIdentification != null)
            {
                BTN_voiceIdentificationstop(this, null);
            }
            if (searchsimulation)
            {
                BTN_searchsimulation_Click(this,null);
            }
            if (gotorearch)
            {
                BTN_search_Click(this,null);
            }
            instance = null;
            threadrun = false;
        }

        private void BUT_Updatepos_Click(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    mav.cs.UpdateCurrentSettings(null, true, port, mav);

                    if (mav == SwarmInterface.Leader)
                        continue;

                    Vector3 offset = getOffsetFromLeader(((Formation) SwarmInterface).getLeader(), mav);

                    if (Math.Abs(offset.x) < 1000 && Math.Abs(offset.y) < 1000)
                    {
                        grid1.UpdateIcon(mav, (float) offset.y, (float) offset.x, (float) offset.z, true, Color.Red);
                        ((Formation) SwarmInterface).setOffsets(mav, offset.y, offset.x, offset.z);
                    }
                }
            }
        }
        private void timer1_status_Tick(object sender, EventArgs e)
        {
            // clean up old
            foreach (Control ctl in PNL_status.Controls)
            {
                bool match = false;
                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        if (mav == (MAVState) ctl.Tag)
                        {
                            match = true;
                            
                        }
                    }
                }

                if (match == false)
                    ctl.Dispose();
            }

            // setup new
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    bool exists = false;
                    foreach (Control ctl in PNL_status.Controls)
                    {
                        if (ctl is Status && ctl.Tag == mav)
                        {
                            exists = true;
                            if(mav.cs.satcount > 10)
                            {
                                ((Status)ctl).GPS.ForeColor = Color.White;
                            }
                            else if (mav.cs.satcount >6)
                            {
                                ((Status)ctl).GPS.ForeColor = Color.Yellow;
                            }
                            else
                            {
                                ((Status)ctl).GPS.ForeColor = Color.Red;
                            }
                            ((Status)ctl).GPS.Text = mav.cs.satcount.ToString("f0");
                            if (!mav.cs.armed)
                            {
                                ((Status)ctl).Armed.ForeColor = Color.Red;
                            }
                            else
                            {
                                ((Status)ctl).Armed.ForeColor = Color.White;
                            }
                            ((Status) ctl).Armed.Text = mav.cs.armed.ToString();
                            ((Status) ctl).Mode.Text = mav.cs.mode;
                            ((Status) ctl).MAV.Text = mav.ToString();
                            //((Status) ctl).Guided.Text = mav.GuidedMode.x + "," + mav.GuidedMode.y + "," +
                            //                             mav.GuidedMode.z;
                            //((Status) ctl).Location1.Text = mav.cs.lat + "," + mav.cs.lng + "," +
                            //                                mav.cs.alt;

                            ((Status)ctl).Alt.Text = mav.cs.alt.ToString("f1");
                            ((Status)ctl).Gndspd.Text = mav.cs.groundspeed.ToString("f1");
                            ((Status)ctl).Airspd.Text = mav.cs.airspeed.ToString("f1");
                            if (mav.cs.linkqualitygcs > 80)
                            {
                                ((Status)ctl).Link.ForeColor = Color.White;
                            }
                            else if (mav.cs.linkqualitygcs > 50)
                            {
                                ((Status)ctl).Link.ForeColor = Color.Yellow;
                            }
                            else
                            {
                                ((Status)ctl).Link.ForeColor = Color.Red;
                            }
                            ((Status)ctl).Link.Text = mav.cs.linkqualitygcs.ToString() + "%";
                            if (mav == SwarmInterface.Leader)
                            {
                                ((Status) ctl).MAV.ForeColor = Color.Red;
                            }
                            else
                            {
                                ((Status) ctl).MAV.ForeColor = Color.White;
                            }
                        }
                    }

                    if (!exists)
                    {
                        Status newstatus = new Status();
                        newstatus.Tag = mav;
                        PNL_status.Controls.Add(newstatus);
                    }
                }
            }

            formationMonitoring.armmavcount = 0;
            formationMonitoring.healthlinkmavcount = 0;
            formationMonitoring.guidedmodemavcount = 0;
            formationMonitoring.totalhavemavcount = 0;
            formationMonitoring.lowspeedmavcount = 0;
            formationMonitoring.lowaltmavcount = 0;
            
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    // 更新已有的飞机总数
                    formationMonitoring.totalhavemavcount++;
                    if ( mav.cs.armed )
                    {
                        formationMonitoring.armmavcount++;
                    }
                    // 通信质量大于65%
                    if (mav.cs.linkqualitygcs > 65)
                    {
                        formationMonitoring.healthlinkmavcount++;
                    }
                    //计算处于引导模式的飞机数量
                    if (mav.cs.mode.ToLower().Equals("guided"))
                    {
                        formationMonitoring.guidedmodemavcount++;
                    }
                    if (mav.cs.firmware == Firmwares.ArduPlane)
                    {
                        if (mav.cs.groundspeed < 5)
                        {
                            formationMonitoring.lowspeedmavcount++;
                        }
                        if (mav.cs.alt < 20)
                        {
                            formationMonitoring.lowaltmavcount++;
                        }
                    }
                    else
                    {
                        if (mav.cs.groundspeed < 1.5)
                        {
                            formationMonitoring.lowspeedmavcount++;
                        }
                        if (mav.cs.alt < 3)
                        {
                            formationMonitoring.lowaltmavcount++;
                        }
                    }
                }
            }
            label_armcount.Text = formationMonitoring.armmavcount.ToString();
            label_heathlinkcount.Text = formationMonitoring.healthlinkmavcount.ToString();
            label_guidedcount.Text = formationMonitoring.guidedmodemavcount.ToString();
            label_connectcount.Text = formationMonitoring.totalhavemavcount.ToString();
            label_lowspeedcount.Text = formationMonitoring.lowspeedmavcount.ToString();
            label_lowaltcount.Text = formationMonitoring.lowaltmavcount.ToString();
            // 避障状态监测
            if (threadrun || gotorearch)
            {
                List<MAVState> obsmavs = new List<MAVState>();
                obsmavs.Clear();
                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        if (mav.obsvector.x != 0 || mav.obsvector.y != 0 || mav.obsvector.z != 0)
                        {
                            obsmavs.Add(mav);
                        }
                    }
                }
                if (obsmavs.Count == 0)
                {
                    label_avoidancecraftid1.Text = "0";
                    label_obstacleid1.Text = "0";
                    label_craftdisttoobstacle1.Text = "∞";
                    label_avoidanceeastoffset1.Text = "0";
                    label_avoidancenorthoffset1.Text = "0";
                    label_avoidancetargetalt1.Text = "0";

                    label_avoidancecraftid2.Text = "0";
                    label_obstacleid2.Text = "0";
                    label_craftdisttoobstacle2.Text = "∞";
                    label_avoidanceeastoffset2.Text = "0";
                    label_avoidancenorthoffset2.Text = "0";
                    label_avoidancetargetalt2.Text = "0";
                }
                else if (obsmavs.Count == 1)
                {
                    label_avoidancecraftid1.Text = obsmavs[0].sysid.ToString();
                    label_obstacleid1.Text = obsmavs[0].obstacleid.ToString();
                    // 计算障碍物距离
                    double distancetoobstacle1 = get_distance_to_obstacle(obsmavs[0], obsmavs[0].obstacleid);
                    label_craftdisttoobstacle1.Text = distancetoobstacle1.Equals(-1) ? "∞" : distancetoobstacle1.ToString("f1");
                    label_avoidanceeastoffset1.Text = obsmavs[0].obsvector.y.ToString("f1");
                    label_avoidancenorthoffset1.Text = obsmavs[0].obsvector.x.ToString("f1");
                    label_avoidancetargetalt1.Text = obsmavs[0].obsvector.z.ToString("f1");

                    label_avoidancecraftid2.Text = "0";
                    label_obstacleid2.Text = "0";
                    label_craftdisttoobstacle2.Text = "∞";
                    label_avoidanceeastoffset2.Text = "0";
                    label_avoidancenorthoffset2.Text = "0";
                    label_avoidancetargetalt2.Text = "0";
                }
                else
                {
                    label_avoidancecraftid1.Text = obsmavs[0].sysid.ToString();
                    label_obstacleid1.Text = obsmavs[0].obstacleid.ToString();
                    // 计算障碍物距离
                    double distancetoobstacle1 = get_distance_to_obstacle(obsmavs[0], obsmavs[0].obstacleid);
                    label_craftdisttoobstacle1.Text = distancetoobstacle1.Equals(-1) ? "∞" : distancetoobstacle1.ToString("f1");
                    label_avoidanceeastoffset1.Text = obsmavs[0].obsvector.y.ToString("f1");
                    label_avoidancenorthoffset1.Text = obsmavs[0].obsvector.x.ToString("f1");
                    label_avoidancetargetalt1.Text = obsmavs[0].obsvector.z.ToString("f1");

                    label_avoidancecraftid2.Text = obsmavs[1].sysid.ToString();
                    label_obstacleid2.Text = obsmavs[1].obstacleid.ToString();
                    // 计算障碍物距离
                    double distancetoobstacle2 = get_distance_to_obstacle(obsmavs[1], obsmavs[1].obstacleid);
                    label_craftdisttoobstacle2.Text = distancetoobstacle2.Equals(-1) ? "∞" : distancetoobstacle2.ToString("f1");
                    label_avoidanceeastoffset2.Text = obsmavs[1].obsvector.y.ToString("f1");
                    label_avoidancenorthoffset2.Text = obsmavs[1].obsvector.x.ToString("f1");
                    label_avoidancetargetalt2.Text = obsmavs[1].obsvector.z.ToString("f1");
                }
                int avoidancecraftcounts = 0;
                List<byte> listobstacleids = new List<byte>();
                listobstacleids.Clear();
                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        if (mav.obsvector.x != 0 || mav.obsvector.y != 0 || mav.obsvector.z != 0)
                        {
                            avoidancecraftcounts++;
                            listobstacleids.Add(mav.sysid);
                        }
                    }
                }
                label_avoidancecraftcounts.Text = avoidancecraftcounts.ToString();
                label_totalavoidancecraftid.Text = "";
                if (listobstacleids.Count == 0)
                {
                    label_totalavoidancecraftid.Text = "0";
                }
                else
                {
                    foreach (var id in listobstacleids)
                    {
                        label_totalavoidancecraftid.Text += id.ToString()+" ";
                    }
                }
                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        if (checkBox_VLmode.Checked && mav == SwarmInterface.Leader)
                        {
                            continue;
                        }
                        if (get_cloest_distance_to_other_crafts(mav)<500)
                        {
                            maybecollisiontimes++;
                        }
                    }
                }
                label_maybecollisiontimes.Text = maybecollisiontimes.ToString();
            }
            // 没有UDP的接收和发送，则关闭定时器2
            if (!udpState.isBind && !udpSend.isBind && timer2.Enabled)
            {
                timer2.Enabled = false;
            }
            if (cam1 != null)
            {
                cam1.camimage += cam1_camimage;
                userVideo1HudShow.showtimestring = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                if (video1AviWrite != null)
                {
                    using (MemoryStream mem = new MemoryStream())
                    {
                        Bitmap bmp = new Bitmap(userVideo1HudShow.bgimage);
                        bmp.Save(mem, userVideo1HudShow.GetImageCodecInfo(), userVideo1HudShow.GetEncoderParameters());
                        video1AviWrite.avi_add(mem.ToArray(), (uint)mem.Length);
                        // write header - so even partial files will play
                        video1AviWrite.avi_end(userVideo1HudShow.Width, userVideo1HudShow.Height, 5);
                    }
                    //userVideo1HudShow.streamjpgenable = true;
                    //video1AviWrite.avi_add(userVideo1HudShow.streamjpg.ToArray(), (uint)userVideo1HudShow.streamjpg.Length);
                    //// write header - so even partial files will play
                    //video1AviWrite.avi_end(userVideo1HudShow.Width, userVideo1HudShow.Height, 5);
                }
                if (checkBox_video1imagesave.Checked)
                {
                    if (DateTime.Now > video1imagenextsave)
                    {
                        video1imagenextsave = DateTime.Now.AddMilliseconds(1000/ video1imagefrequency);
                        string imagepath = txt_video1path.Text + Path.DirectorySeparatorChar + "video1_image" + (video1imagenumber++).ToString() + "_"+ DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".jpeg";
                        //照片另存
                        using (MemoryStream mem = new MemoryStream())
                        {
                            Bitmap bmp = new Bitmap(userVideo1HudShow.bgimage);
                            //保存到磁盘文件
                            bmp.Save(@imagepath, ImageFormat.Jpeg);
                            bmp.Dispose();
                        }
                    }                    
                }
            }
            else
            {
                if (video1AviWrite != null)
                {
                    BTN_Video1WriteStop(this,null);
                }
            }
            if (cam2 != null)
            {
                cam2.camimage += cam2_camimage;
                userVideo2HudShow.showtimestring = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                if (video2AviWrite != null)
                {
                    using (MemoryStream mem = new MemoryStream())
                    {
                        Bitmap bmp = new Bitmap(userVideo2HudShow.bgimage);
                        bmp.Save(mem, userVideo2HudShow.GetImageCodecInfo(), userVideo2HudShow.GetEncoderParameters());
                        video2AviWrite.avi_add(mem.ToArray(), (uint)mem.Length);
                        // write header - so even partial files will play
                        video2AviWrite.avi_end(userVideo2HudShow.Width, userVideo2HudShow.Height, 5);
                    }
                }
                if (checkBox_video2imagesave.Checked)
                {
                    if (DateTime.Now > video2imagenextsave)
                    {
                        video2imagenextsave = DateTime.Now.AddMilliseconds(1000 / video2imagefrequency);
                        string imagepath = txt_video2path.Text + Path.DirectorySeparatorChar + "video2_image" + (video2imagenumber++).ToString() + "_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")+".jpeg";
                        //照片另存
                        using (MemoryStream mem = new MemoryStream())
                        {
                            Bitmap bmp = new Bitmap(userVideo2HudShow.bgimage);
                            //保存到磁盘文件
                            bmp.Save(@imagepath, ImageFormat.Jpeg);
                            bmp.Dispose();
                        }
                    }
                }
            }
            else
            {
                if (video2AviWrite != null)
                {
                    BTN_Video2WriteStop(this, null);
                }
            }
            if (video1AviWrite == null)
            {
                label_video1state.Text = "状态:未录制";
            }
            else
            {
                label_video1state.Text = "状态:正在录制";
            }
            if (video2AviWrite == null)
            {
                label_video2state.Text = "状态:未录制";
            }
            else
            {
                label_video2state.Text = "状态:正在录制";
            }
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == CMB_3DMAP.SelectedValue)
                    {
                        if (map3D != null)
                        {
                            map3D.rpy = new OpenTK.Vector3(mav.cs.roll, mav.cs.pitch, mav.cs.yaw);
                            map3D.LocationCenter = new PointLatLngAlt(mav.cs.lat,
                                mav.cs.lng, mav.cs.altasl / CurrentState.multiplieralt, "here");
                            map3D.WPs = mav.wps.Values.Select(a => (Locationwp)a).ToList();
                        }
                    }
                }
            }
            if (!newRecognizedline.Equals(oldRecognizedline))
            {
                txt_Identificateresult.Text = "正在听:"+newRecognizedline;
                oldRecognizedline = newRecognizedline;
            }
            if (searchsimulation)
            {
                updateSimulationMarkersPosition(vtspeedms,headingchangeintervl,(double)timer1_status.Interval);
            }
            if (instance != null)
            {
                updateClearMavsIconsMarkers();
                // 搜寻界面显示所有的飞机
                foreach (var port in MainV2.Comports.ToArray())
                {
                    foreach (var MAV in port.MAVlist)
                    {
                        var marker = ArduPilot.Common.getMAVMarker(MAV);

                        if (marker.Position.Lat == 0 && marker.Position.Lng == 0)
                            continue;

                        addMavsIconsMarker(marker);
                    }
                }
            }
            show_sim_data();
            updateDistanceLeaderToIntercept();
        }
        private void updateDistanceLeaderToIntercept()
        {
            if (instance == null || commandManager == null)
            {
                return;
            }
            DateTime lastReadTime = commandManager.GetLastReadTime();
            // 3秒没有收到数据，显示为0，字体红色
            if (DateTime.Now > lastReadTime.AddSeconds(3))
            {
                distBwtLeadToIntercept.Text = "距离:0.00米";
                distBwtLeadToIntercept.ForeColor = Color.Red;
                heightToIntercept.Text = "高度差:0.00米";
                heightToIntercept.ForeColor = Color.Red;
            }
            else
            {
                PointLatLngAlt intercept = commandManager.GetInterceptPoint();
                double dist = SwarmInterface.Leader.cs.Location.GetDistance(intercept);
                distBwtLeadToIntercept.Text = "距离:" + dist.ToString("f2") + "米";
                distBwtLeadToIntercept.ForeColor = Color.White;
                heightToIntercept.Text = "高度差:" + (intercept.Alt - SwarmInterface.Leader.cs.alt).ToString("f2") + "米";
                heightToIntercept.ForeColor = Color.White;
            }
        }

        private void updateClearMavsIconsMarkers()
        {
            // not async
            Invoke((MethodInvoker)delegate
            {
                mavsOverlay.Markers.Clear();
            });
        }
        private void addMavsIconsMarker(GMapMarker marker)
        {
            // not async
            Invoke((MethodInvoker)delegate
            {
                mavsOverlay.Markers.Add(marker);
            });
        }
        double wrap_360(double input)
        {
            if (input > 360)
                return input - 360;
            if (input < 0)
                return input + 360;
            return input;
        }
        Random yawIncrement = new Random();
        private void updateSimulationMarkersPosition(double speed,double time,double timerinterval)
        {
            double deltatime = timerinterval / 1000.0;
            double distance_m = deltatime * speed;
            foreach (GMapMarkerVirtualTarget marker in virtualtargetoverlay.Markers)
            {
                float newyaw = marker.heading;
                PointLatLngAlt markerpoint = new PointLatLngAlt(marker.Position.Lat, marker.Position.Lng);
                if (DateTime.Now > nextUpdateHeadingDateTimes[virtualtargetoverlay.Markers.IndexOf(marker)])
                {
                    nextUpdateHeadingDateTimes[virtualtargetoverlay.Markers.IndexOf(marker)] = DateTime.Now.AddSeconds(time);
                    // 让虚拟目标航向在规定时间间隔内随机变化
                    if (InsidePolyGons(searchareaoverlay.Polygons, new PointLatLng(markerpoint.Lat, markerpoint.Lng)))
                    {
                        newyaw += yawIncrement.Next(-30, 30);
                    }
                    else
                    {
                        PointLatLngAlt cloestcenterpoint = FindClosestPolygonCenterPoints(searchareaoverlay.Polygons, markerpoint);
                        newyaw = (float)markerpoint.GetBearing(cloestcenterpoint);
                    }
                }
                newyaw = (float)wrap_360(newyaw);
                var newmarkerpoint = markerpoint.newpos(newyaw, distance_m);
                PointLatLng point = new PointLatLng(newmarkerpoint.Lat, newmarkerpoint.Lng);
                marker.SetPosition(point);
                marker.heading = newyaw;
            }
            if (FlightData.instance != null)
            {
                foreach (GMapMarkerVirtualTarget marker in virtualtargetoverlay.Markers)
                {
                    foreach (GMapMarkerVirtualTarget flightDatamarker in FlightData.instance.virtualtargetoverlay.Markers)
                    {
                        if (marker.Tag.Equals(flightDatamarker.Tag))
                        {
                            PointLatLng point = new PointLatLng(marker.Position.Lat, marker.Position.Lng);
                            float yawset = marker.heading;
                            flightDatamarker.SetPosition(point);
                            flightDatamarker.heading = yawset;
                        }
                    }
                }
            }
        }
        private bool InsidePolyGons(ObservableCollectionThreadSafe<GMapPolygon> PolygonLists, PointLatLng point)
        {
            foreach (var polygon in PolygonLists)
            {
                return polygon.IsInside(point);
            }
            return false;
        }
        struct LatAndLng
        {
            public double lat;
            public double lng;
        }
        struct pointanddistance
        {
            public LatAndLng Point;
            public int distance;
        }
        private PointLatLngAlt FindClosestPolygonCenterPoints(ObservableCollectionThreadSafe<GMapPolygon> PolygonLists, PointLatLngAlt markerpoint)
        {
            List<pointanddistance> pointanddislist = new List<pointanddistance>();
            foreach (var polygon in PolygonLists)
            {
                pointanddistance pointanddis = new pointanddistance();
                List<LatAndLng> pointslatlng = new List<LatAndLng>();
                foreach (var point in polygon.Points)
                {
                    LatAndLng latAndLng = new LatAndLng();
                    latAndLng.lat = point.Lat;
                    latAndLng.lng = point.Lng;
                    pointslatlng.Add(latAndLng);
                }
                pointanddis.Point.lat = (pointslatlng.Min(x => x.lat) + pointslatlng.Max(x => x.lat)) / 2;
                pointanddis.Point.lng = (pointslatlng.Min(x => x.lng) + pointslatlng.Max(x => x.lng)) / 2;
                PointLatLngAlt pointLatLngAlt = new PointLatLngAlt(pointanddis.Point.lat, pointanddis.Point.lng);
                pointanddis.distance = (int)(100 * markerpoint.GetDistance(pointLatLngAlt));
                pointanddislist.Add(pointanddis);
            }
            if (pointanddislist.Count > 0)
            {
                pointanddistance pad = pointanddislist.Where(p => p.distance == (int)pointanddislist.Min(a => a.distance)).FirstOrDefault();
                return new PointLatLngAlt(pad.Point.lat, pad.Point.lng);
            }
            else
            {
                return new PointLatLngAlt();
            }
        }
        private Map3D map3D = null;
        private void BTN_Show3dMap_Click(object sender, EventArgs e)
        {
            if (map3D == null)
            {
                map3D = new Map3D();
                map3D.Dock =DockStyle.Fill;
                splitContainer8.Panel2.Controls.Add(map3D);
            }
        }
        private void BTN_3DmapClose(object sender, EventArgs e)
        {
            if (map3D != null)
            {
                map3D.Dispose();
                map3D = null;
            }
        }
        public struct mavdistance
        {
            public MAVState GetMAVState;
            public int distance;
        }

        private double get_cloest_distance_to_other_crafts(MAVState state)
        {

            List <mavdistance> craftlists= new List<mavdistance>();
            craftlists.Clear();
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    mavdistance mavanddistance = new mavdistance();
                    if (mav.Equals(state))
                    {
                        continue;
                    }
                    if (checkBox_VLmode.Checked && mav == SwarmInterface.Leader)
                    {
                        continue;
                    }
                    mavanddistance.GetMAVState = mav;
                    // 转化为单位cm
                    mavanddistance.distance = (int)(state.cs.Location.GetDistance(mav.cs.Location)*100);
                    craftlists.Add(mavanddistance);
                }
            }
            if (craftlists.Count > 0)
            {
                mavdistance md = craftlists.Where(p => p.distance == (int)craftlists.Min(a => a.distance)).FirstOrDefault();
                return md.distance;
            }
            else
            {
                return int.MaxValue;
            }
        }
        private double get_distance_to_obstacle(MAVState state, byte id)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav.sysid == id)
                    {
                        double horizontal_distance = mav.cs.Location.GetDistance(state.cs.Location);
                        double vertical_distance = Math.Abs(mav.cs.alt - state.cs.alt);
                        return Math.Sqrt(horizontal_distance* horizontal_distance+ vertical_distance* vertical_distance);
                    }
                }
            }
            return -1;
        }
        private void updateParam(MAVState mavparam,bool read)
        {
            if (!SwarmInterface.pids.ContainsKey(mavparam))
            {
                return;
            }
            // 读取参数，显示到界面
            if (read)
            {
                // TurnError To Roll
                roll_P.Value = (decimal)SwarmInterface.pids[mavparam].Item1._kp;
                roll_I.Value = (decimal)SwarmInterface.pids[mavparam].Item1._ki;
                roll_D.Value = (decimal)SwarmInterface.pids[mavparam].Item1._kd;
                roll_IMAX.Value = (decimal)SwarmInterface.pids[mavparam].Item1._imax;
                roll_FILT.Value = (decimal)SwarmInterface.pids[mavparam].Item1._filt_hz;
                // AltError To Pitch
                pitch_P.Value = (decimal)SwarmInterface.pids[mavparam].Item2._kp;
                pitch_I.Value = (decimal)SwarmInterface.pids[mavparam].Item2._ki;
                pitch_D.Value = (decimal)SwarmInterface.pids[mavparam].Item2._kd;
                pitch_IMAX.Value = (decimal)SwarmInterface.pids[mavparam].Item2._imax;
                pitch_FILT.Value = (decimal)SwarmInterface.pids[mavparam].Item2._filt_hz;
                // YawError(not use)
                yaw_P.Value = (decimal)SwarmInterface.pids[mavparam].Item3._kp;
                yaw_I.Value = (decimal)SwarmInterface.pids[mavparam].Item3._ki;
                yaw_D.Value = (decimal)SwarmInterface.pids[mavparam].Item3._kd;
                yaw_IMAX.Value = (decimal)SwarmInterface.pids[mavparam].Item3._imax;
                yaw_FILT.Value = (decimal)SwarmInterface.pids[mavparam].Item3._filt_hz;
                // Dist To Speed
                speed_P.Value = (decimal)SwarmInterface.pids[mavparam].Item4._kp;
                speed_I.Value = (decimal)SwarmInterface.pids[mavparam].Item4._ki;
                speed_D.Value = (decimal)SwarmInterface.pids[mavparam].Item4._kd;
                speed_IMAX.Value = (decimal)SwarmInterface.pids[mavparam].Item4._imax;
                speed_FILT.Value = (decimal)SwarmInterface.pids[mavparam].Item4._filt_hz;
                // Speed To thrust
                thrust_P.Value = (decimal)SwarmInterface.pids[mavparam].Item5._kp;
                thrust_I.Value = (decimal)SwarmInterface.pids[mavparam].Item5._ki;
                thrust_D.Value = (decimal)SwarmInterface.pids[mavparam].Item5._kd;
                thrust_IMAX.Value = (decimal)SwarmInterface.pids[mavparam].Item5._imax;
                thrust_FILT.Value = (decimal)SwarmInterface.pids[mavparam].Item5._filt_hz;
                // 高级参数
                target_trailer_scale.Value = (decimal)mavparam.sawmAdvancedParameters.targettrailerscale;
                target_leader_dist_m.Value = (decimal)mavparam.sawmAdvancedParameters.targetleaderdis_m;
                yaw_distance_boundary_m.Value = (decimal)mavparam.sawmAdvancedParameters.yawdistanceboundary_m;
                roll_low_dist_boundary_m.Value = (decimal)mavparam.sawmAdvancedParameters.rolllowdistboundary_m;
                roll_high_dist_boundary_m.Value = (decimal)mavparam.sawmAdvancedParameters.rollhighdistboundary_m;
                roll_min_disttotarget_m.Value = (decimal)mavparam.sawmAdvancedParameters.rollmindisttotarget_m;
                yaw_to_roll_max_angle_deg.Value = (decimal)mavparam.sawmAdvancedParameters.yawtorollmaxangle_deg;
                thrust_trim.Value = (decimal)mavparam.sawmAdvancedParameters.thrusttrim;
                max_dist_to_delta_speed_m.Value = (decimal)mavparam.sawmAdvancedParameters.maxdisttodspd_m;
                dist_integral_separation_m.Value = (decimal)mavparam.sawmAdvancedParameters.distintegseparation_m;
                delta_speed_scale.Value = (decimal)mavparam.sawmAdvancedParameters.deltaspeedscale;
                thrust_min.Value = (decimal)mavparam.sawmAdvancedParameters.thrustmin;
                min_target_speed.Value = (decimal)mavparam.sawmAdvancedParameters.mintargetspeed;
                pre_distance_to_lose_speed.Value = (decimal)mavparam.sawmAdvancedParameters.predistancetolosespeed;
                stall_protect.Value = (decimal)(mavparam.sawmAdvancedParameters.stallprotect == true ? 1 : 0);
                // 编队参数
                copter_takeoff_m.Value = (decimal)Formationoverallparameters.CopterTakeoffAlt_m;
                fail_time_horizon_s.Value = (decimal)Formationoverallparameters.FailTimeHorizon;
                warn_time_horizon_s.Value = (decimal)Formationoverallparameters.WarnTimeHorizon;
                fail_distance_xy_m.Value = (decimal)Formationoverallparameters.FailDistance_xy;
                fail_distance_z_m.Value = (decimal)Formationoverallparameters.FailDistance_z;
                warn_distance_xy_m.Value = (decimal)Formationoverallparameters.WarnDistance_xy;
                warn_distance_z_m.Value = (decimal)Formationoverallparameters.WarnDistance_z;
                fail_alt_min_m.Value = (decimal)Formationoverallparameters.FailAltitudeMinimum;
                avoidance_vertical_m.Value = (decimal)Formationoverallparameters.AvoidanceVertical_m;
                avoidance_horizontal_m.Value = (decimal)Formationoverallparameters.AvoidanceHorizontal_m;
                fail_action.Value = (decimal)Formationoverallparameters.FailAction;
                avoidance_enable.Value = (decimal)(Formationoverallparameters.AvoidanceEnable==true?1:0);
                min_approach_times.Value = (decimal)Formationoverallparameters.MinApproachTime_s;
                virtualleaderenable.Value = (decimal)(Formationoverallparameters.VirtualLeaderEnable == true ? 1 : 0);
                AP_oil_power.Value = (decimal)(Formationoverallparameters.APOilPower == true ? 1 : 0);
                udp_transfer_enable.Value = (decimal)(Formationoverallparameters.UdpTransferEnable == true ? 1 : 0);

            }
            else // 写入参数
            {
                // TurnError To Roll
                SwarmInterface.pids[mavparam].Item1._kp = (float)roll_P.Value;
                SwarmInterface.pids[mavparam].Item1._ki = (float)roll_I.Value;
                SwarmInterface.pids[mavparam].Item1._kd = (float)roll_D.Value;
                SwarmInterface.pids[mavparam].Item1._imax = (float)roll_IMAX.Value;
                SwarmInterface.pids[mavparam].Item1._filt_hz = (float)roll_FILT.Value;
                // AltError To Pitch
                SwarmInterface.pids[mavparam].Item2._kp = (float)pitch_P.Value;
                SwarmInterface.pids[mavparam].Item2._ki = (float)pitch_I.Value;
                SwarmInterface.pids[mavparam].Item2._kd = (float)pitch_D.Value;
                SwarmInterface.pids[mavparam].Item2._imax = (float)pitch_IMAX.Value;
                SwarmInterface.pids[mavparam].Item2._filt_hz = (float)pitch_FILT.Value;
                // YawError(not use)
                SwarmInterface.pids[mavparam].Item3._kp = (float)yaw_P.Value;
                SwarmInterface.pids[mavparam].Item3._ki = (float)yaw_I.Value;
                SwarmInterface.pids[mavparam].Item3._kd = (float)yaw_D.Value;
                SwarmInterface.pids[mavparam].Item3._imax = (float)yaw_IMAX.Value;
                SwarmInterface.pids[mavparam].Item3._filt_hz = (float)yaw_FILT.Value;
                // Dist To Speed
                SwarmInterface.pids[mavparam].Item4._kp = (float)speed_P.Value;
                SwarmInterface.pids[mavparam].Item4._ki = (float)speed_I.Value;
                SwarmInterface.pids[mavparam].Item4._kd = (float)speed_D.Value;
                SwarmInterface.pids[mavparam].Item4._imax = (float)speed_IMAX.Value;
                SwarmInterface.pids[mavparam].Item4._filt_hz = (float)speed_FILT.Value;
                // Speed To thrust
                SwarmInterface.pids[mavparam].Item5._kp = (float)thrust_P.Value;
                SwarmInterface.pids[mavparam].Item5._ki = (float)thrust_I.Value;
                SwarmInterface.pids[mavparam].Item5._kd = (float)thrust_D.Value;
                SwarmInterface.pids[mavparam].Item5._imax = (float)thrust_IMAX.Value;
                SwarmInterface.pids[mavparam].Item5._filt_hz = (float)thrust_FILT.Value;
                // 高级参数
                mavparam.sawmAdvancedParameters.targettrailerscale = (float)target_trailer_scale.Value;
                mavparam.sawmAdvancedParameters.targetleaderdis_m = (int)target_leader_dist_m.Value;
                mavparam.sawmAdvancedParameters.yawdistanceboundary_m = (int)yaw_distance_boundary_m.Value;
                mavparam.sawmAdvancedParameters.rolllowdistboundary_m = (int)roll_low_dist_boundary_m.Value;
                mavparam.sawmAdvancedParameters.rollhighdistboundary_m = (int)roll_high_dist_boundary_m.Value;
                mavparam.sawmAdvancedParameters.rollmindisttotarget_m = (int)roll_min_disttotarget_m.Value;
                mavparam.sawmAdvancedParameters.yawtorollmaxangle_deg = (int)yaw_to_roll_max_angle_deg.Value;
                mavparam.sawmAdvancedParameters.maxdisttodspd_m = (int)max_dist_to_delta_speed_m.Value;
                mavparam.sawmAdvancedParameters.distintegseparation_m = (int)dist_integral_separation_m.Value;
                mavparam.sawmAdvancedParameters.deltaspeedscale = (float)delta_speed_scale.Value;
                mavparam.sawmAdvancedParameters.thrusttrim = (float)thrust_trim.Value;
                mavparam.sawmAdvancedParameters.thrustmin = (float)thrust_min.Value;
                mavparam.sawmAdvancedParameters.mintargetspeed = (int)min_target_speed.Value;
                mavparam.sawmAdvancedParameters.predistancetolosespeed = (int)pre_distance_to_lose_speed.Value;
                mavparam.sawmAdvancedParameters.stallprotect = (int)stall_protect.Value == 1 ? true : false;

                // 编队参数
                Formationoverallparameters.CopterTakeoffAlt_m = (uint)copter_takeoff_m.Value;
                Formationoverallparameters.FailTimeHorizon = (float)fail_time_horizon_s.Value;
                Formationoverallparameters.WarnTimeHorizon = (float)warn_time_horizon_s.Value;
                Formationoverallparameters.FailDistance_xy = (float)fail_distance_xy_m.Value;
                Formationoverallparameters.FailDistance_z = (float)fail_distance_z_m.Value;
                Formationoverallparameters.WarnDistance_xy = (float)warn_distance_xy_m.Value;
                Formationoverallparameters.WarnDistance_z = (float)warn_distance_z_m.Value;
                Formationoverallparameters.FailAltitudeMinimum = (float)fail_alt_min_m.Value;
                Formationoverallparameters.AvoidanceVertical_m = (uint)avoidance_vertical_m.Value;
                Formationoverallparameters.AvoidanceHorizontal_m = (uint)avoidance_horizontal_m.Value;
                Formationoverallparameters.AvoidanceEnable = (int)avoidance_enable.Value==1?true:false;
                Formationoverallparameters.MinApproachTime_s = (float)min_approach_times.Value;
                Formationoverallparameters.VirtualLeaderEnable = (int)virtualleaderenable.Value == 1 ? true : false;
                Formationoverallparameters.APOilPower = (int)AP_oil_power.Value == 1 ? true : false;
                Formationoverallparameters.UdpTransferEnable = (int)udp_transfer_enable.Value == 1 ? true : false;
                SwarmInterface.setPowerType(Formationoverallparameters.APOilPower);
                SwarmInterface.avoidance.set_params(Formationoverallparameters);
                setUdpMirrorTransferEnable(Formationoverallparameters.UdpTransferEnable);
            }
        }
        private void updateAdvanceParam(MAVState mavparam, bool write = true)
        {
            if (write)
            {
                mavparam.sawmAdvancedParameters.targettrailerscale = (float)target_trailer_scale.Value;
                mavparam.sawmAdvancedParameters.targetleaderdis_m = (int)target_leader_dist_m.Value;
                mavparam.sawmAdvancedParameters.yawdistanceboundary_m = (int)yaw_distance_boundary_m.Value;
                mavparam.sawmAdvancedParameters.rolllowdistboundary_m = (int)roll_low_dist_boundary_m.Value;
                mavparam.sawmAdvancedParameters.rollhighdistboundary_m = (int)roll_high_dist_boundary_m.Value;
                mavparam.sawmAdvancedParameters.rollmindisttotarget_m = (int)roll_min_disttotarget_m.Value;
                mavparam.sawmAdvancedParameters.yawtorollmaxangle_deg = (int)yaw_to_roll_max_angle_deg.Value;
                mavparam.sawmAdvancedParameters.maxdisttodspd_m = (int)max_dist_to_delta_speed_m.Value;
                mavparam.sawmAdvancedParameters.distintegseparation_m = (int)dist_integral_separation_m.Value;
                mavparam.sawmAdvancedParameters.deltaspeedscale = (float)delta_speed_scale.Value;
                mavparam.sawmAdvancedParameters.thrusttrim = (float)thrust_trim.Value;
                mavparam.sawmAdvancedParameters.thrustmin = (float)thrust_min.Value;
                mavparam.sawmAdvancedParameters.mintargetspeed = (int)min_target_speed.Value;
                mavparam.sawmAdvancedParameters.predistancetolosespeed = (int)pre_distance_to_lose_speed.Value;
                mavparam.sawmAdvancedParameters.stallprotect = (int)stall_protect.Value == 1 ? true : false;
            }
        }
        private void but_guided_Click(object sender, EventArgs e)
        {
            if (SwarmInterface != null)
            {
                SwarmInterface.GuidedMode();
            }
        }
        private void Time2_Tick(object sender, EventArgs e)
        {
            if (udpState.isBind)
            {
                UdpReceProcess();
            }
            if (udpSend.isBind)
            {
                UdpSendProcess();
            }
        }
        private void ReadParam()
        {
            string controlParamDirectory = Settings.GetUserDataDirectory() + "formation" + Path.DirectorySeparatorChar + "control_param.txt";
            //string controlParamDirectory = @"../../../formation/control_param.txt";
            if (!File.Exists(controlParamDirectory))
            {
                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        defaultpid = new Tuple<PID, PID, PID, PID, PID>(
                                     new PID(1f, .03f, 0.02f, 10, 20, 0.1f, 0),
                                     new PID(1f, .03f, 0.02f, 10, 20, 0.1f, 0),
                                     new PID(1, 0, 0.00f, 15, 20, 0.1f, 0),
                                     new PID(1.4f, 0, 0, 10, 1, 0.1f, 0),
                                     new PID(0.04f, 0.004f, 0, 0.5f, 20, 0.1f, 0)
                                     );
                        SwarmInterface.pids.Add(mav, defaultpid);
                    }
                }
                MessageBox.Show("缺少编队参数控制文件，请正确添加，否则将使用默认参数进行控制");
                return;
            }
            else
            {
                try
                {
                    fs = new FileStream(@controlParamDirectory, FileMode.Open, FileAccess.ReadWrite);

                }
                catch
                {
                    MessageBox.Show("文件打开失败！");//文件打开失败
                    return;
                }
                streamReader = new StreamReader(fs, Encoding.Default);
                fs.Seek(0, SeekOrigin.Begin);

                string linetext = streamReader.ReadLine();

                if (linetext != null)
                {
                    string[] data = linetext.Split(':');
                    if (data[0].Equals("FormationControlParamStart") == false)
                    {
                        MessageBox.Show("不是位置文件！");
                        fs.Close();
                        return;
                    }
                    else
                    {
                        lines.Clear();
                        while (linetext != null)
                        {
                            linetext = streamReader.ReadLine();
                            lines.Add(linetext);
                            if (linetext.Equals("FormationControlParamFinished"))
                            {
                                break;
                            }
                        }
                        foreach (var overallparamesline in lines)
                        {
                            if (overallparamesline.Equals("FormationOverallParameters Begin"))
                            {
                                // 获取下一行
                                string ovweallparams = lines[lines.IndexOf(overallparamesline)+1];
                                string[] paramsData = ovweallparams.Split(':');
                                Formationoverallparameters.CopterTakeoffAlt_m = uint.Parse(paramsData[1]);
                                Formationoverallparameters.FailTimeHorizon = float.Parse(paramsData[3]);
                                Formationoverallparameters.WarnTimeHorizon = float.Parse(paramsData[5]);
                                Formationoverallparameters.FailDistance_xy = float.Parse(paramsData[7]);
                                Formationoverallparameters.FailDistance_z = float.Parse(paramsData[9]);
                                Formationoverallparameters.WarnDistance_xy = float.Parse(paramsData[11]);
                                Formationoverallparameters.WarnDistance_z = float.Parse(paramsData[13]);
                                Formationoverallparameters.FailAltitudeMinimum = float.Parse(paramsData[15]);
                                Formationoverallparameters.AvoidanceVertical_m = uint.Parse(paramsData[17]);
                                Formationoverallparameters.AvoidanceHorizontal_m = uint.Parse(paramsData[19]);
                                Formationoverallparameters.FailAction = int.Parse(paramsData[21]);
                                Formationoverallparameters.AvoidanceEnable = int.Parse(paramsData[23])==1?true:false;
                                
                                if (paramsData.Length > 25)
                                {
                                    Formationoverallparameters.MinApproachTime_s = float.Parse(paramsData[25]);
                                }
                                if (paramsData.Length > 27)
                                {
                                    Formationoverallparameters.VirtualLeaderEnable = int.Parse(paramsData[27]) == 1 ? true : false;
                                }
                                if (paramsData.Length > 29)
                                {
                                    Formationoverallparameters.APOilPower = int.Parse(paramsData[29]) == 1 ? true : false;
                                } 
                                if (paramsData.Length > 31)
                                {
                                    Formationoverallparameters.UdpTransferEnable = int.Parse(paramsData[31]) == 1 ? true : false;
                                }
                                break;
                            }
                            if (overallparamesline.Equals("FormationOverallParameters End"))
                            {
                                break;
                            }
                        }
                        //linetext = streamReader.ReadLine();//继续读取下一行
                        foreach (var port in MainV2.Comports)
                        {
                            foreach (var mav in port.MAVlist)
                            {
                                string portType = port.BaseStream.PortName + " " + mav.sysid + " " + mav.compid + " " + "Begin";
                                string showToUser = port.BaseStream.PortName + " " + mav.sysid + " " + mav.compid;
                                for (int j = 0; j < lines.Count; j++)
                                {
                                    if (lines[j].Equals(portType))
                                    {
                                        Tuple<PID, PID, PID, PID,PID> pid;
                                        pid = new Tuple<PID, PID, PID, PID,PID>(
                                              new PID(1f, .03f, 0.02f, 10, 20, 0.1f, 0),
                                              new PID(1f, .03f, 0.02f, 10, 20, 0.1f, 0),
                                              new PID(1, 0, 0.00f, 15, 20, 0.1f, 0),
                                              new PID(1.4f, 0, 0, 10, 1, 0.1f, 0),
                                              new PID(0.04f, 0.004f, 0, 0.5f, 20, 0.1f, 0)
                                              );
                                        for (int i = 0; i < 6; i++)
                                        {
                                            string getline = lines[j + 1 + i];
                                            string[] validData = getline.Split(':');
                                            switch (i)
                                            {
                                                case 0:
                                                    pid.Item1.set_kp(float.Parse(validData[2]));
                                                    pid.Item1.set_ki(float.Parse(validData[4]));
                                                    pid.Item1.set_kd(float.Parse(validData[6]));
                                                    pid.Item1.set_imax(float.Parse(validData[8]));
                                                    pid.Item1.set_filthz(float.Parse(validData[10]));
                                                    pid.Item1.set_dt(float.Parse(validData[12]));
                                                    pid.Item1.set_ff(float.Parse(validData[14]));
                                                    break;
                                                case 1:
                                                    pid.Item2.set_kp(float.Parse(validData[2]));
                                                    pid.Item2.set_ki(float.Parse(validData[4]));
                                                    pid.Item2.set_kd(float.Parse(validData[6]));
                                                    pid.Item2.set_imax(float.Parse(validData[8]));
                                                    pid.Item2.set_filthz(float.Parse(validData[10]));
                                                    pid.Item2.set_dt(float.Parse(validData[12]));
                                                    pid.Item2.set_ff(float.Parse(validData[14]));
                                                    break;
                                                case 2:
                                                    pid.Item3.set_kp(float.Parse(validData[2]));
                                                    pid.Item3.set_ki(float.Parse(validData[4]));
                                                    pid.Item3.set_kd(float.Parse(validData[6]));
                                                    pid.Item3.set_imax(float.Parse(validData[8]));
                                                    pid.Item3.set_filthz(float.Parse(validData[10]));
                                                    pid.Item3.set_dt(float.Parse(validData[12]));
                                                    pid.Item3.set_ff(float.Parse(validData[14]));
                                                    break;
                                                case 3:
                                                    pid.Item4.set_kp(float.Parse(validData[2]));
                                                    pid.Item4.set_ki(float.Parse(validData[4]));
                                                    pid.Item4.set_kd(float.Parse(validData[6]));
                                                    pid.Item4.set_imax(float.Parse(validData[8]));
                                                    pid.Item4.set_filthz(float.Parse(validData[10]));
                                                    pid.Item4.set_dt(float.Parse(validData[12]));
                                                    pid.Item4.set_ff(float.Parse(validData[14]));
                                                    break;
                                                case 4:
                                                    pid.Item5.set_kp(float.Parse(validData[2]));
                                                    pid.Item5.set_ki(float.Parse(validData[4]));
                                                    pid.Item5.set_kd(float.Parse(validData[6]));
                                                    pid.Item5.set_imax(float.Parse(validData[8]));
                                                    pid.Item5.set_filthz(float.Parse(validData[10]));
                                                    pid.Item5.set_dt(float.Parse(validData[12]));
                                                    pid.Item5.set_ff(float.Parse(validData[14]));
                                                    break;
                                                case 5:
                                                    mav.sawmAdvancedParameters.targettrailerscale = float.Parse(validData[2]);
                                                    mav.sawmAdvancedParameters.targetleaderdis_m = int.Parse(validData[4]);
                                                    mav.sawmAdvancedParameters.yawdistanceboundary_m = int.Parse(validData[6]);
                                                    mav.sawmAdvancedParameters.rolllowdistboundary_m = int.Parse(validData[8]);
                                                    mav.sawmAdvancedParameters.rollhighdistboundary_m = int.Parse(validData[10]);
                                                    mav.sawmAdvancedParameters.rollmindisttotarget_m = int.Parse(validData[12]);
                                                    mav.sawmAdvancedParameters.yawtorollmaxangle_deg = int.Parse(validData[14]);
                                                    mav.sawmAdvancedParameters.maxdisttodspd_m = int.Parse(validData[16]);
                                                    mav.sawmAdvancedParameters.distintegseparation_m = int.Parse(validData[18]);
                                                    mav.sawmAdvancedParameters.deltaspeedscale = float.Parse(validData[20]);
                                                    mav.sawmAdvancedParameters.thrusttrim = float.Parse(validData[22]);
                                                    mav.sawmAdvancedParameters.thrustmin = float.Parse(validData[24]);
                                                    mav.sawmAdvancedParameters.mintargetspeed = int.Parse(validData[26]);
                                                    mav.sawmAdvancedParameters.predistancetolosespeed = int.Parse(validData[28]);
                                                    mav.sawmAdvancedParameters.stallprotect = int.Parse(validData[30]) == 1 ? true : false;
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                        SwarmInterface.pids.Add(mav, pid);
                                        break;
                                    }
                                    else if (lines[j].Equals("FormationControlParamFinished")|| lines[j]==null)
                                    {
                                        defaultpid = new Tuple<PID, PID, PID, PID, PID>(
                                                     new PID(1f, .03f, 0.02f, 10, 20, 0.1f, 0),
                                                     new PID(1f, .03f, 0.02f, 10, 20, 0.1f, 0),
                                                     new PID(1, 0, 0.00f, 15, 20, 0.1f, 0),
                                                     new PID(1.4f, 0, 0, 10, 1, 0.1f, 0),
                                                     new PID(0.04f, 0.004f, 0, 0.5f, 20, 0.1f, 0)
                                                     );
                                        SwarmInterface.pids.Add(mav, defaultpid);
                                        if (mav.cs.firmware == Firmwares.ArduPlane)
                                        {
                                            MessageBox.Show(showToUser + "未能匹配到参数，系统将使用默认参数");
                                        }
                                        break;
                                    }
                                }                               
                            }
                        }
                    }
                }
            }
        }
        string writeLine = null;
        private void WriteParam()
        {
            string controlParamDirectory = Settings.GetUserDataDirectory() + "formation" + Path.DirectorySeparatorChar + "control_param.txt";
            //string controlParamDirectory = @"../../../formation/control_param.txt";
            if (!File.Exists(controlParamDirectory))
            {
                string dirPath = Settings.GetUserDataDirectory() + "formation";
                Directory.CreateDirectory(dirPath);
                StartWriteParam(controlParamDirectory);
                MessageBox.Show("文件不存在，已创建新文件");
                return;
            }
            else
            {
                uint mavcounts = getMavCounts();
                if (mavcounts == 0)
                {
                    MessageBox.Show("未检测到连接的飞机，参数写入失败");
                    return;
                }
                //FileStream writefs = new FileStream(@controlParamDirectory, FileMode.OpenOrCreate);
                //StreamWriter streamWriter = new StreamWriter(writefs);
                StartWriteParam(controlParamDirectory);
            }
        }

        private void StartWriteParam(string path)
        {
            StreamWriter streamWriter = null;
            try
            {
                streamWriter = new StreamWriter(File.Create(@path));
                writeLine = "FormationControlParamStart:byZengXu";
                streamWriter.WriteLine(writeLine);
                writeLine = "FormationOverallParameters Begin";
                streamWriter.WriteLine(writeLine);
                // 避障参数
                writeLine = "TFALT:" + Formationoverallparameters.CopterTakeoffAlt_m.ToString() + ":FTIME:" + Formationoverallparameters.FailTimeHorizon.ToString("f1") + ":WTIME:" + Formationoverallparameters.WarnTimeHorizon.ToString("f1") + ":FDISXY:" + Formationoverallparameters.FailDistance_xy.ToString("f0") + ":FDISZ:" +
                    Formationoverallparameters.FailDistance_z.ToString("f0") + ":WDISXY:" + Formationoverallparameters.WarnDistance_xy.ToString("f0") + ":WDISZ:" + Formationoverallparameters.WarnDistance_z.ToString("f0") + ":FAMIN:" + Formationoverallparameters.FailAltitudeMinimum.ToString("f0") + ":AV:" + Formationoverallparameters.AvoidanceVertical_m.ToString("f0")
                    + ":AH:" + Formationoverallparameters.AvoidanceHorizontal_m.ToString("f0") + ":FACT:" + Formationoverallparameters.FailAction.ToString() + ":AE:" + (Formationoverallparameters.AvoidanceEnable == true ? 1 : 0).ToString() + ":APPROTIME:" + Formationoverallparameters.MinApproachTime_s.ToString("f1") + ":VLE:" + (Formationoverallparameters.VirtualLeaderEnable == true ? 1 : 0).ToString()
                    + ":OILP:" + (Formationoverallparameters.APOilPower == true ? 1 : 0).ToString() + ":TRANE:" + (Formationoverallparameters.UdpTransferEnable == true ? 1 : 0).ToString();
                streamWriter.WriteLine(writeLine);
                writeLine = "FormationOverallParameters End";
                streamWriter.WriteLine(writeLine);
                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        writeLine = port.BaseStream.PortName + " " + mav.sysid + " " + mav.compid + " " + "Begin";
                        streamWriter.WriteLine(writeLine);
                        writeLine = "Roll:" + "P:" + SwarmInterface.pids[mav].Item1._kp.ToString("f2") + ":I:" + SwarmInterface.pids[mav].Item1._ki.ToString("f2") + ":D:" + SwarmInterface.pids[mav].Item1._kd.ToString("f2") + ":IMAX:" + SwarmInterface.pids[mav].Item1._imax.ToString("f2") + ":FILT:" + SwarmInterface.pids[mav].Item1._filt_hz.ToString("f2") + ":DT:0.1:FF:0";
                        streamWriter.WriteLine(writeLine);
                        writeLine = "Pitch:" + "P:" + SwarmInterface.pids[mav].Item2._kp.ToString("f2") + ":I:" + SwarmInterface.pids[mav].Item2._ki.ToString("f2") + ":D:" + SwarmInterface.pids[mav].Item2._kd.ToString("f2") + ":IMAX:" + SwarmInterface.pids[mav].Item2._imax.ToString("f2") + ":FILT:" + SwarmInterface.pids[mav].Item2._filt_hz.ToString("f2") + ":DT:0.1:FF:0";
                        streamWriter.WriteLine(writeLine);
                        writeLine = "Yaw:" + "P:" + SwarmInterface.pids[mav].Item3._kp.ToString("f2") + ":I:" + SwarmInterface.pids[mav].Item3._ki.ToString("f2") + ":D:" + SwarmInterface.pids[mav].Item3._kd.ToString("f2") + ":IMAX:" + SwarmInterface.pids[mav].Item3._imax.ToString("f2") + ":FILT:" + SwarmInterface.pids[mav].Item3._filt_hz.ToString("f2") + ":DT:0.1:FF:0";
                        streamWriter.WriteLine(writeLine);
                        writeLine = "Speed:" + "P:" + SwarmInterface.pids[mav].Item4._kp.ToString("f3") + ":I:" + SwarmInterface.pids[mav].Item4._ki.ToString("f3") + ":D:" + SwarmInterface.pids[mav].Item4._kd.ToString("f3") + ":IMAX:" + SwarmInterface.pids[mav].Item4._imax.ToString("f2") + ":FILT:" + SwarmInterface.pids[mav].Item4._filt_hz.ToString("f2") + ":DT:0.1:FF:0";
                        streamWriter.WriteLine(writeLine);
                        writeLine = "Thrust:" + "P:" + SwarmInterface.pids[mav].Item5._kp.ToString("f3") + ":I:" + SwarmInterface.pids[mav].Item5._ki.ToString("f3") + ":D:" + SwarmInterface.pids[mav].Item5._kd.ToString("f3") + ":IMAX:" + SwarmInterface.pids[mav].Item5._imax.ToString("f2") + ":FILT:" + SwarmInterface.pids[mav].Item5._filt_hz.ToString("f2") + ":DT:0.1:FF:0";
                        streamWriter.WriteLine(writeLine);
                        writeLine = "AdvancedParam:" + "SCALE:" + mav.sawmAdvancedParameters.targettrailerscale.ToString("f2") + ":LDIST:" + mav.sawmAdvancedParameters.targetleaderdis_m.ToString() + ":YDIST:" + mav.sawmAdvancedParameters.yawdistanceboundary_m.ToString() + ":RLOW:" + mav.sawmAdvancedParameters.rolllowdistboundary_m.ToString() + ":RHIGH:" + mav.sawmAdvancedParameters.rollhighdistboundary_m.ToString() +
                            ":RMIN:" + mav.sawmAdvancedParameters.rollmindisttotarget_m.ToString() + ":R2YANGLE:" + mav.sawmAdvancedParameters.yawtorollmaxangle_deg.ToString() + ":MAXDIST:" + mav.sawmAdvancedParameters.maxdisttodspd_m.ToString() + ":DISTINTER:" + mav.sawmAdvancedParameters.distintegseparation_m.ToString() + ":SPDSCALE:" + mav.sawmAdvancedParameters.deltaspeedscale.ToString("f2") + ":THRTRIM:"
                            + mav.sawmAdvancedParameters.thrusttrim.ToString("f2") + ":THRMIN:" + mav.sawmAdvancedParameters.thrustmin.ToString("f2") + ":MINSPD:" + mav.sawmAdvancedParameters.mintargetspeed.ToString() + ":PREDLS:" + mav.sawmAdvancedParameters.predistancetolosespeed.ToString() + ":STALL:" + (mav.sawmAdvancedParameters.stallprotect == true ? 1 : 0).ToString();
                        streamWriter.WriteLine(writeLine);
                        writeLine = port.BaseStream.PortName + " " + mav.sysid + " " + mav.compid + " " + "End";
                        streamWriter.WriteLine(writeLine);
                    }
                }
                writeLine = "FormationControlParamFinished";
                streamWriter.WriteLine(writeLine);
                if (streamWriter != null)
                {
                    streamWriter.Close();
                }
            }
            catch
            {
                if (streamWriter != null)
                {
                    streamWriter.Close();
                }
                MessageBox.Show("参数写入失败");
            }
        }

        private void UdpSendProcess()
        {
            if (DateTime.Now > udpSendnext)
            {
                udpSendnext = DateTime.Now.AddMilliseconds(1000 / udpSendfrequency);
                byte[] senddata = new byte[] { 0x01, 0x00, 0x00, 0x00, 0xD0 };
                udpSend.UdpClient.Send(senddata, senddata.Length, udpSend.remoteIpAndPort);
                if (cam1 != null)
                {
                    using (MemoryStream mem = new MemoryStream())
                    {
                        Bitmap bmp = new Bitmap(userVideo1HudShow.bgimage);
                        bmp.Save(mem, userVideo1HudShow.GetImageCodecInfo(), userVideo1HudShow.GetEncoderParameters());
                        udpSend.UdpClient.Send(mem.ToArray(), (int)mem.Length, udpSend.remoteIpAndPort);
                    }
                }
            }
        }
        private void UdpReceProcess()
        {
            //网络中无可用数据，立刻退出，防止线程阻塞
            if (udpState.UdpClient.Available == 0)
            {
                return;
            }
            byte[] ReceiveBytes = udpState.UdpClient.Receive(ref udpState.remoteIpAndPort);
            if (ReceiveBytes == null)
            {
                return;
            }
            else if (ReceiveBytes.Length == 0)
            {
                return;
            }
            else
            {
                udpState.cnt = ReceiveBytes.Length;
            }

        }

        private void BUT_UDPReceiveConnect(object sender, EventArgs e)
        {
            // 未绑定
            if (udpState.isBind == false)
            {
                try
                {
                    udpState.IP = IPAddress.Parse(textBox_localip.Text);
                    udpState.Port = Convert.ToInt32(textBox_localport.Text);
                    udpState.LocalIPEndPoint = new IPEndPoint(udpState.IP, udpState.Port);
                    udpState.UdpClient = new UdpClient(udpState.Port);
                    //定义IPENDPOINT，装载远程IP地址和端口 
                    udpState.remoteIpAndPort = new IPEndPoint(IPAddress.Any, 0);
                    textBox_localport.Enabled = false;
                    UDP_receive.Text = Strings.Stop;
                    udpState.isBind = true;
                    // 开启定时器
                    timer2.Enabled = true;
                }
                catch
                {
                    MessageBox.Show("链接失败，请检查端口号是否存在或被占用");
                }
            }
            else
            {//已绑定
                udpState.UdpClient.Close();
                textBox_localport.Enabled = true;
                UDP_receive.Text = Strings.Connect;
                udpState.isBind = false;
                //timer2.Enabled = false;
            }
        }

        private void BUT_UDPSendConnect(object sender, EventArgs e)
        {
            if (udpSend.isBind == false)
            {
                try
                {
                    udpSend.IP = IPAddress.Parse(txt_remoteip.Text);
                    udpSend.Port = Convert.ToInt32(txt_remoteport.Text);
                    udpSend.LocalIPEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    //定义IPENDPOINT，装载远程IP地址和端口
                    udpSend.remoteIpAndPort = new IPEndPoint(udpSend.IP, udpSend.Port);
                    udpSend.UdpClient = new UdpClient(udpSend.Port);
                    txt_remoteport.Enabled = false;
                    UDP_send.Text = Strings.Stop;
                    udpSend.isBind = true;
                    // 开启定时器
                    timer2.Enabled = true;
                }
                catch
                {
                    MessageBox.Show("链接失败，请检查端口号是否存在或被占用");
                }
            }
            else
            {
                udpSend.UdpClient.Close();
                txt_remoteport.Enabled = true;
                UDP_send.Text = Strings.Connect;
                udpSend.isBind = false;
            }
        }
        private void CMB_pid_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == CMB_pid.SelectedValue)
                    {
                        updateParam(mav, true);
                    }
                }
            }
        }

        private void BUT_writeparam(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (CHK_lockallmav.Checked)
                    {
                        updateParam(mav, false);
                    }
                    else
                    {
                        if (mav == CMB_pid.SelectedValue)
                        {
                            // 写入参数
                            updateParam(mav, false);
                        }
                    }
                    // 调试全部的高级参数
                    if (checkBox_AllAdvanceParam.Checked)
                    {
                        updateAdvanceParam(mav);
                    }
                }
            }
            if (MainV2.speechEnable && MainV2.speechEngine != null)
            {
                if (CHK_lockallmav.Checked)
                {
                    MainV2.speechEngine.SpeakAsync("全部写入成功");
                }
                else
                {
                    MainV2.speechEngine.SpeakAsync("写入成功");
                }
                    
            }
        }

        private void trackBar_Scroll(object sender, EventArgs e)
        {
            textBox_swarmspace.Text = trackBar_swarmspace.Value.ToString();
        }

        private void BTN_SwarmSpace_Click(object sender, EventArgs e)
        {
            if (textBox_swarmspace.Text == "" || textBox_swarmspace.Text == "-")
                return;
            try
            {
                int.Parse(textBox_swarmspace.Text);
            }
            catch
            {
                MessageBox.Show("请不要输入非数字字符");
                return;
            }
            int x;
            bool result = int.TryParse(textBox_swarmspace.Text, out x);
            if (result)
            {
                if (x > trackBar_swarmspace.Maximum)
                {
                    textBox_swarmspace.Text = trackBar_swarmspace.Maximum.ToString();
                    trackBar_swarmspace.Value = trackBar_swarmspace.Maximum;
                }
                else if (x < trackBar_swarmspace.Minimum)
                {
                    textBox_swarmspace.Text = trackBar_swarmspace.Minimum.ToString();
                    trackBar_swarmspace.Value = trackBar_swarmspace.Minimum;
                }
                else
                {
                    trackBar_swarmspace.Value = int.Parse(textBox_swarmspace.Text);
                }
            }
            else
            {
                MessageBox.Show("请不要输入非数字字符");
            }
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            if (comboBox_formation_select.SelectedItem != null)
            {
                updateFormationShape();
            }
        }

        private void cbB_formation_select_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_formation_select.SelectedItem == null)
            {
                return;
            }
            // 第一步让加载队形的选中项为空
            listBox_formationShape.SelectedItem = null;
            if (comboBox_formation_select.SelectedIndex >=0)
            {
                updateFormationShape();
            }
        }
        private uint getMavCounts()
        {
            uint counts = 0;
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    counts++;
                }
            }
            return counts;
        }
        private void updateFormationShape()
        {
            // 计算飞机总数
            uint totalmavcount = getMavCounts();
            int swarmDistance_m = trackBar_swarmspace.Value;
            if (totalmavcount <= 1)
            {
                totalmavcount = 1;
                MessageBox.Show("小于两架飞机不能组成队形");
                return;
            }
            // 定义一维数组保存所有飞机的偏移量
            Vector3[] totalmavoffsets = new Vector3[totalmavcount];
            for (uint m = 0; m < totalmavcount; m++)
            {
                totalmavoffsets[m] = new Vector3(0, 0, 0);
            }
            uint n = 0;
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    totalmavoffsets[n] = SwarmInterface.getgrid2Offsets(mav);
                    n++;
                }
            }
            //没有找到包含的队形
            if (comboBox_formation_select.Items.Contains(comboBox_formation_select.SelectedItem) == false)
            {
                MessageBox.Show("没有找到队形");
                return;
            }
            uint excludedmavnum = 1;
            uint excludedindex = 0;
            if (checkBox_VLmode.Checked)
            {
                excludedmavnum = 2;
            }
            switch (comboBox_formation_select.SelectedItem.ToString())
            {
                case "大雁形":
                    {
                        uint rightmavcount = (totalmavcount - excludedmavnum) / 2;
                        uint leftmavcount = totalmavcount - excludedmavnum - rightmavcount;
                        for (excludedindex=0; excludedindex< excludedmavnum; excludedindex++)
                        {
                            totalmavoffsets[excludedindex] = new Vector3(0, swarmDistance_m, 0);
                        }
                        // 计算左侧的偏移量
                        for (uint i = 0; i < leftmavcount; i++)
                        {
                            totalmavoffsets[excludedmavnum + i].x = -(i + 1) * swarmDistance_m * Math.Sin(30 * MathHelper.deg2rad);
                            totalmavoffsets[excludedmavnum + i].y = swarmDistance_m - (i + 1) * swarmDistance_m * Math.Cos(30 * MathHelper.deg2rad);
                            //totalmavoffsets[1 + i].z = 0;
                        }
                        // 计算右侧的偏移量
                        for (uint j = 0; j < rightmavcount; j++)
                        {
                            totalmavoffsets[leftmavcount + excludedmavnum + j].x = (j + 1) * swarmDistance_m * Math.Sin(30 * MathHelper.deg2rad);
                            totalmavoffsets[leftmavcount + excludedmavnum + j].y = swarmDistance_m - (j + 1) * swarmDistance_m * Math.Cos(30 * MathHelper.deg2rad);
                        }
                    }
                    break;
                case "竖一字形":
                    {
                        uint halftotalmavcountdist = (totalmavcount - excludedmavnum) / 2;
                        for (excludedindex = 0; excludedindex < excludedmavnum; excludedindex++)
                        {
                            totalmavoffsets[excludedindex].x = 0;
                            totalmavoffsets[excludedindex].y = halftotalmavcountdist * swarmDistance_m;
                        }
                        for (uint i2 = 0; i2 < totalmavcount- excludedmavnum; i2++)
                        {
                            totalmavoffsets[i2 + excludedmavnum].x = 0;
                            totalmavoffsets[i2 + excludedmavnum].y = halftotalmavcountdist * swarmDistance_m - (i2+1) * swarmDistance_m;
                            //totalmavoffsets[i2].z = 0;
                        }
                    }
                    break;
                case "横一字形":
                    {
                        uint rightsidemavcount = (totalmavcount - excludedmavnum) / 2;
                        uint leftsidemavcount = totalmavcount - excludedmavnum - rightsidemavcount;
                        for (excludedindex = 0; excludedindex < excludedmavnum; excludedindex++)
                        {
                            totalmavoffsets[excludedindex] = new Vector3(0, 0, 0); 
                        }
                        // 左侧
                        for (uint i3 = 0; i3 < leftsidemavcount; i3++)
                        {
                            totalmavoffsets[i3 + excludedmavnum].x = -(i3 + 1) * swarmDistance_m;
                            totalmavoffsets[i3 + excludedmavnum].y = 0;
                            //totalmavoffsets[i3 + 1].z = 0;
                        }
                        // 右侧
                        for (uint i3 = 0; i3 < rightsidemavcount; i3++)
                        {
                            totalmavoffsets[i3 + excludedmavnum + leftsidemavcount].x = (i3 + 1) * swarmDistance_m;
                            totalmavoffsets[i3 + excludedmavnum + leftsidemavcount].y = 0;
                            //totalmavoffsets[i3 + 1 + leftsidemavcount].z = 0;
                        }
                    }
                    break;
                case "三角形":
                    {
                        int mavcounts = (int)(totalmavcount - (excludedmavnum - 1));
                        if (mavcounts <= 2)
                        {
                            MessageBox.Show("少于三架飞机不建议组成三角形编队");
                            return;
                        }
                        uint remainder = (uint)mavcounts % 3;
                        float trisidelength, intercept, leftpointx, leftpointy;
                        uint sidemavcount;
                        trisidelength = (uint)((mavcounts + 2) / 3) * swarmDistance_m;
                        // 每一边的飞机数量
                        sidemavcount = ((uint)mavcounts + 2) / 3 - 1;
                        // 算出截距
                        intercept = trisidelength * (float)Math.Sin(30 * MathHelper.deg2rad) / (float)Math.Sin(120 * MathHelper.deg2rad);
                        for (excludedindex = 0; excludedindex < excludedmavnum; excludedindex++)
                        {
                            totalmavoffsets[excludedindex] = new Vector3(0, intercept, 0);
                        }
                        // 计算左下角x,y相对应的坐标
                        leftpointx = -trisidelength * (float)Math.Sin(30 * MathHelper.deg2rad);
                        leftpointy = intercept - trisidelength * (float)Math.Cos(30 * MathHelper.deg2rad);
                        // 左下角的点
                        //totalmavoffsets[1] = new Vector3(leftpointx, leftpointy, 0);
                        totalmavoffsets[excludedmavnum].x = leftpointx;
                        totalmavoffsets[excludedmavnum].y = leftpointy;
                        // 右下角的点
                        //totalmavoffsets[2] = new Vector3(-leftpointx, leftpointy, 0);
                        totalmavoffsets[excludedmavnum+1].x = -leftpointx;
                        totalmavoffsets[excludedmavnum+1].y = leftpointy;
                        uint i4 = 0;
                        switch (remainder)
                        {
                            case 0:
                                // 左边
                                for (i4 = 0; i4 < sidemavcount; i4++)
                                {
                                    totalmavoffsets[3 + (excludedmavnum - 1) + i4].x = -(i4 + 1) * swarmDistance_m * Math.Sin(30 * MathHelper.deg2rad);
                                    totalmavoffsets[3 + (excludedmavnum - 1) + i4].y = intercept - (i4 + 1) * swarmDistance_m * Math.Cos(30 * MathHelper.deg2rad);
                                    //totalmavoffsets[3 + i4].z = 0;
                                }
                                // 右边
                                for (i4 = 0; i4 < sidemavcount; i4++)
                                {
                                    totalmavoffsets[3 + (excludedmavnum - 1) + sidemavcount + i4].x = (i4 + 1) * swarmDistance_m * Math.Sin(30 * MathHelper.deg2rad);
                                    totalmavoffsets[3 + (excludedmavnum - 1) + sidemavcount + i4].y = intercept - (i4 + 1) * swarmDistance_m * Math.Cos(30 * MathHelper.deg2rad);
                                    //totalmavoffsets[3 + sidemavcount + i4].z = 0;
                                }
                                // 下边
                                for (i4 = 0; i4 < sidemavcount; i4++)
                                {
                                    totalmavoffsets[3 + (excludedmavnum - 1) + 2 * sidemavcount + i4].x = leftpointx + (i4 + 1) * swarmDistance_m;
                                    totalmavoffsets[3 + (excludedmavnum - 1) + 2 * sidemavcount + i4].y = leftpointy;
                                    //totalmavoffsets[3 + 2 * sidemavcount + i4].z = 0;
                                }
                                break;
                            case 1:
                                // 左边
                                for (i4 = 0; i4 < sidemavcount - 1; i4++)
                                {
                                    totalmavoffsets[3 + (excludedmavnum - 1) + i4].x = -(i4 + 1) * swarmDistance_m * Math.Sin(30 * MathHelper.deg2rad);
                                    totalmavoffsets[3 + (excludedmavnum - 1) + i4].y = intercept - (i4 + 1) * swarmDistance_m * Math.Cos(30 * MathHelper.deg2rad);
                                    //totalmavoffsets[3 + i4].z = 0;
                                }
                                // 右边
                                for (i4 = 0; i4 < sidemavcount - 1; i4++)
                                {
                                    totalmavoffsets[3 + (excludedmavnum - 1) + sidemavcount - 1 + i4].x = (i4 + 1) * swarmDistance_m * Math.Sin(30 * MathHelper.deg2rad);
                                    totalmavoffsets[3 + (excludedmavnum - 1) + sidemavcount - 1 + i4].y = intercept - (i4 + 1) * swarmDistance_m * Math.Cos(30 * MathHelper.deg2rad);
                                    //totalmavoffsets[3 + sidemavcount - 1 + i4].z = 0;
                                }
                                // 下边
                                for (i4 = 0; i4 < sidemavcount; i4++)
                                {
                                    totalmavoffsets[3 + (excludedmavnum - 1) + 2 * (sidemavcount - 1) + i4].x = leftpointx + (i4 + 1) * swarmDistance_m;
                                    totalmavoffsets[3 + (excludedmavnum - 1) + 2 * (sidemavcount - 1) + i4].y = leftpointy;
                                    //totalmavoffsets[3 + 2 * (sidemavcount - 1) + i4].z = 0;
                                }
                                break;
                            case 2:
                                // 左边
                                for (i4 = 0; i4 < sidemavcount; i4++)
                                {
                                    totalmavoffsets[3 + (excludedmavnum - 1) + i4].x = -(i4 + 1) * swarmDistance_m * Math.Sin(30 * MathHelper.deg2rad);
                                    totalmavoffsets[3 + (excludedmavnum - 1) + i4].y = intercept - (i4 + 1) * swarmDistance_m * Math.Cos(30 * MathHelper.deg2rad);
                                    //totalmavoffsets[3 + i4].z = 0;
                                }
                                // 右边
                                for (i4 = 0; i4 < sidemavcount; i4++)
                                {
                                    totalmavoffsets[3 + (excludedmavnum - 1) + sidemavcount + i4].x = (i4 + 1) * swarmDistance_m * Math.Sin(30 * MathHelper.deg2rad);
                                    totalmavoffsets[3 + (excludedmavnum - 1) + sidemavcount + i4].y = intercept - (i4 + 1) * swarmDistance_m * Math.Cos(30 * MathHelper.deg2rad);
                                    //totalmavoffsets[3 + sidemavcount + i4].z = 0;
                                }
                                // 下边
                                for (i4 = 0; i4 < sidemavcount - 1; i4++)
                                {
                                    totalmavoffsets[3 + (excludedmavnum - 1) + 2 * (sidemavcount) + i4].x = leftpointx + (i4 + 1) * swarmDistance_m;
                                    totalmavoffsets[3 + (excludedmavnum - 1) + 2 * (sidemavcount) + i4].y = leftpointy;
                                    //totalmavoffsets[3 + 2 * (sidemavcount) + i4].z = 0;
                                }
                                break;
                            default:
                                MessageBox.Show("出现未知的数值错误");
                                break;
                        }
                    }
                    break;
                case "亠形":
                    {
                        // 最上面一点的偏移
                        for (excludedindex = 0; excludedindex < excludedmavnum; excludedindex++)
                        {
                            totalmavoffsets[excludedindex] = new Vector3(0, 0, 0);
                        }
                        uint belowmavcounts = totalmavcount - excludedmavnum;
                        float belowsidelengths = (belowmavcounts - 1) * swarmDistance_m;
                        for (uint i5 = 0;i5< belowmavcounts;i5++)
                        {
                            totalmavoffsets[excludedmavnum + i5].x = -belowsidelengths / 2 + i5 * swarmDistance_m;
                            totalmavoffsets[excludedmavnum + i5].y = -swarmDistance_m;
                            //totalmavoffsets[1 + i5].z = 0;
                        }
                    }
                    break;
                case "下亠形":
                    {
                        // 最下面一点的偏移
                        for (excludedindex = 0; excludedindex < excludedmavnum; excludedindex++)
                        {
                            totalmavoffsets[excludedindex] = new Vector3(0, 0, 0);
                        }
                        uint topmavcounts = totalmavcount - excludedmavnum;
                        float topsidelengths = (topmavcounts - 1) * swarmDistance_m;
                        for (uint i6 = 0; i6 < topmavcounts; i6++)
                        {
                            totalmavoffsets[excludedmavnum + i6].x = -topsidelengths / 2 + i6 * swarmDistance_m;
                            totalmavoffsets[excludedmavnum + i6].y = swarmDistance_m;
                            //totalmavoffsets[1 + i6].z = 0;
                        }
                    }
                    break;
                case "横网形":
                    {
                        generateHorizontalTriHole(totalmavoffsets, totalmavcount, swarmDistance_m);
                    }
                    break;
                case "竖网形":
                    {
                        generateVerticalTriHole(totalmavoffsets, totalmavcount, swarmDistance_m);
                    }
                    break;
                case "斜网形":
                    {
                        generateTiltTriHole(totalmavoffsets, totalmavcount, swarmDistance_m);
                    }
                    break;
                default:
                    //MessageBox.Show("没有找到队形");
                    break;
            }
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == SwarmInterface.Leader)
                    {
                        ((Formation)SwarmInterface).setgrid2Offsets(mav, totalmavoffsets[0].x, totalmavoffsets[0].y, totalmavoffsets[0].z);
                    }
                }
            }
            int k = (SwarmInterface.Leader == null) ? 0:1;
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == SwarmInterface.Leader)
                    {
                        continue;
                    }
                    ((Formation)SwarmInterface).setgrid2Offsets(mav, totalmavoffsets[k].x, totalmavoffsets[k].y, totalmavoffsets[k].z);
                    k++;
                }
            }
            grid2updateicons();
        }

        private uint getHexagonLevelDeep(uint mavCount)
        {
            uint level = 0;
            uint sumMav = 0;
            sumMav = getMavCountByHexagon(level);
            while (sumMav < mavCount) {
                level++;
                sumMav += getMavCountByHexagon(level);
            }
            return level;
        }

        private uint getMavCountByHexagon(uint level)
        {
            uint result = 6 + 6 * (level - 1);
            result = Math.Max(1, result);
            return result;
        }

        private void generateHorizontalTriHole(Vector3[] offsets, uint mavCount, int distance)
        {
            // 右边开始逆时针扫描
            uint startIndex = 0;
            if (checkBox_VLmode.Checked)
            {
                startIndex = 1;
            }
            uint tempMavCount = mavCount - startIndex;
            uint level = getHexagonLevelDeep(tempMavCount);
            List<Vector3> tempOffsets = new List<Vector3>();
            for (uint indexLevel = 0; indexLevel <= level; indexLevel++)
            {
                if (indexLevel == 0)
                {
                    Vector3 element = new Vector3(0, 0, 0);
                    tempOffsets.Add(element);
                    continue;
                }
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < indexLevel; j++)
                    {
                        double pearAngle = 60 / indexLevel;
                        Vector3 element = new Vector3(0, 0, 0);
                        double a = distance * indexLevel;
                        double b = j * distance;
                        double distanceArc = Math.Sqrt(a * a + b * b - 2 * a * b * Math.Cos(60 * MathHelper.deg2rad));
                        element.x = distanceArc * Math.Cos((60 * i + j * pearAngle) * MathHelper.deg2rad);
                        element.y = distanceArc * Math.Sin((60 * i + j * pearAngle) * MathHelper.deg2rad);
                        tempOffsets.Add(element);
                    }
                }
            }
            offsets[0] = new Vector3(0, 0, 0);
            for (uint index = startIndex; index < mavCount; index++)
            {
                offsets[index].x = tempOffsets[(int)(index - startIndex)].x;
                offsets[index].y = tempOffsets[(int)(index - startIndex)].y;
                offsets[index].z = tempOffsets[(int)(index - startIndex)].z;
            }
        }

        private void generateVerticalTriHole(Vector3[] offsets, uint mavCount, int distance)
        {
            // 右边开始逆时针扫描
            uint startIndex = 0;
            if (checkBox_VLmode.Checked)
            {
                startIndex = 1;
            }
            uint tempMavCount = mavCount - startIndex;
            uint level = getHexagonLevelDeep(tempMavCount);
            List<Vector3> tempOffsets = new List<Vector3>();
            for (uint indexLevel = 0; indexLevel <= level; indexLevel++)
            {
                if (indexLevel == 0)
                {
                    Vector3 element = new Vector3(0, 0, 0);
                    tempOffsets.Add(element);
                    continue;
                }
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < indexLevel; j++)
                    {
                        double pearAngle = 60 / indexLevel;
                        Vector3 element = new Vector3(0, 0, 0);
                        double a = distance * indexLevel;
                        double b = j * distance;
                        double distanceArc = Math.Sqrt(a * a + b * b - 2 * a * b * Math.Cos(60 * MathHelper.deg2rad));
                        element.x = distanceArc * Math.Cos((60 * i + j * pearAngle) * MathHelper.deg2rad);
                        element.z = distanceArc * Math.Sin((60 * i + j * pearAngle) * MathHelper.deg2rad);
                        tempOffsets.Add(element);
                    }
                }
            }
            offsets[0] = new Vector3(0, 0, 0);
            for (uint index = startIndex; index < mavCount; index++)
            {
                offsets[index].x = tempOffsets[(int)(index - startIndex)].x;
                offsets[index].y = tempOffsets[(int)(index - startIndex)].y;
                offsets[index].z = tempOffsets[(int)(index - startIndex)].z;
            }
        }

        private void generateTiltTriHole(Vector3[] offsets, uint mavCount, int distance)
        {
            // 右边开始逆时针扫描
            uint startIndex = 0;
            if (checkBox_VLmode.Checked)
            {
                startIndex = 1;
            }
            uint tempMavCount = mavCount - startIndex;
            uint level = getHexagonLevelDeep(tempMavCount);
            List<Vector3> tempOffsets = new List<Vector3>();
            for (uint indexLevel = 0; indexLevel <= level; indexLevel++)
            {
                if (indexLevel == 0)
                {
                    Vector3 element = new Vector3(0, 0, 0);
                    tempOffsets.Add(element);
                    continue;
                }
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < indexLevel; j++)
                    {
                        double pearAngle = 60 / indexLevel;
                        Vector3 element = new Vector3(0, 0, 0);
                        double a = distance * indexLevel;
                        double b = j * distance;
                        double distanceArc = Math.Sqrt(a * a + b * b - 2 * a * b * Math.Cos(60 * MathHelper.deg2rad));
                        element.x = distanceArc * Math.Cos((60 * i + j * pearAngle) * MathHelper.deg2rad);
                        element.y = distanceArc * Math.Sin((60 * i + j * pearAngle) * MathHelper.deg2rad);
                        element.z = element.y;
                        tempOffsets.Add(element);
                    }
                }
            }
            offsets[0] = new Vector3(0, 0, 0);
            for (uint index = startIndex; index < mavCount; index++)
            {
                offsets[index].x = tempOffsets[(int)(index - startIndex)].x;
                offsets[index].y = tempOffsets[(int)(index - startIndex)].y;
                offsets[index].z = tempOffsets[(int)(index - startIndex)].z;
            }
        }

        private void BTN_PreviewForamtionTrans(object sender, EventArgs e)
        {
            if (SwarmInterface.Leader == null)
            {
                MessageBox.Show("请按照如下步骤操作：在编队控制界面设置长机->选择已有队形或者加载用户自定义队形->预览队形变换");
                return;
            }
            // 没有可选择的队形
            if (comboBox_formation_select.SelectedItem == null && listBox_formationShape.SelectedItem == null)
            {
                MessageBox.Show("请先选择已有队形或者加载用户自定义队形");
                return;
            }
            int totalmavcount = (int)getMavCounts();
            if (totalmavcount <= 1)
            {
                MessageBox.Show("至少需要两架飞机");
                return;
            }
            // 定义效益矩阵
            int[,] cost_array = new int[totalmavcount - 1, totalmavcount - 1];
            // 计算两个界面直接的偏差
            Vector3 leaderRelativeOffset = new Vector3();
            List<Vector3> followOffsets = new List<Vector3>();
            followOffsets.Clear();
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == SwarmInterface.Leader)
                    {
                        leaderRelativeOffset = new Vector3(SwarmInterface.grid2offsets[mav]);
                    }
                    else
                    {
                        Vector3 otherOffset = new Vector3(SwarmInterface.grid2offsets[mav]);
                        followOffsets.Add(otherOffset);
                    }
                }
            }
            // 将所有偏移量映射到编队控制界面下的偏移
            foreach (var followOffset in followOffsets)
            {
                followOffset.x -= leaderRelativeOffset.x;
                followOffset.y -= leaderRelativeOffset.y;
                followOffset.z -= leaderRelativeOffset.z;
            }
            int row = 0;
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    //FollowoffsetDis offsetDis = new FollowoffsetDis();
                    //List<FollowoffsetDis> followoffsetDis = new List<FollowoffsetDis>();
                    //followoffsetDis.Clear();
                    if (mav == SwarmInterface.Leader)
                    {
                        //grid2.UpdateIcon(mav, 0, 0, 0, true, Color.Red);
                        continue;
                    }
                    foreach (var follow in followOffsets)
                    {
                        PointLatLngAlt target = new PointLatLngAlt(SwarmInterface.Leader.cs.lat, SwarmInterface.Leader.cs.lng, SwarmInterface.Leader.cs.alt);

                        PointLatLngAlt mavTarget = new PointLatLngAlt(GetMavTarget(target, SwarmInterface.Leader.cs.yaw, follow));
                        var dist = mavTarget.GetDistance(mav.cs.Location);
                        cost_array[row, followOffsets.IndexOf(follow)] = (int)(100 * dist);
                        //offsetDis.followoffset = follow;
                        //offsetDis.distance = (int)(100 * dist);
                        //followoffsetDis.Add(offsetDis);
                    }
                    row++;
                    // 找到路径最短相对应的编号
                    //if (followoffsetDis.Count > 0)
                    //{
                    //    FollowoffsetDis fod = followoffsetDis.Where(p => p.distance == (int)followoffsetDis.Min(a => a.distance)).FirstOrDefault();
                    //    grid2.UpdateIcon(mav, (float)fod.followoffset.x, (float)fod.followoffset.y, (float)fod.followoffset.z,true,Color.Red);
                    //    followOffsets.Remove(fod.followoffset);
                    //}
                }
            }
            // 匈牙利算法
            int[] result = HungarianAlgorithm.FindAssignments(cost_array);
            int index = 0;
            ShapeChangeAction action = calculateShapeChangeAction();
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == SwarmInterface.Leader)
                    {
                        grid2.UpdateIcon(mav, 0, 0, 0, true, Color.Red);
                        continue;
                    }
                    if (action == ShapeChangeAction.NONE)
                    {
                        grid2.UpdateIcon(mav, (float)followOffsets[result[index]].x, (float)followOffsets[result[index]].y, (float)followOffsets[result[index]].z, true, Color.Red);
                    }
                    else
                    {
                        if (mavIndex.ContainsKey(mav))
                        {
                            int specialIndex = mavIndex[mav];
                            grid2.UpdateIcon(mav, (float)followOffsets[specialIndex].x, (float)followOffsets[specialIndex].y, (float)followOffsets[specialIndex].z, true, Color.Red);
                        }
                    }
                    index++;
                }
            }

            grid2.ClearOnlyShow();
            //计算实际飞机所在的偏移
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    System.Numerics.Vector2 realoffsetToLeader = GetoffsetbyPoint(SwarmInterface.Leader.cs.Location,mav.cs.Location);
                    System.Numerics.Vector2 unitleaderyaw = new System.Numerics.Vector2((float)Math.Sin(SwarmInterface.Leader.cs.yaw * MathHelper.deg2rad), (float)Math.Cos(SwarmInterface.Leader.cs.yaw * MathHelper.deg2rad));
                    // 点积
                    float y = realoffsetToLeader.X * unitleaderyaw.X+ realoffsetToLeader.Y * unitleaderyaw.Y;
                    // 叉积
                    float x = realoffsetToLeader.X * unitleaderyaw.Y - realoffsetToLeader.Y * unitleaderyaw.X;
                    if (grid2.Vertical)
                    {
                        grid2.showicons.Add(new Grid.icon() { interf = mav, x = x, y = 0, z = y, Name = mav.ToString(), Color = Color.Yellow });
                    }
                    else
                    {
                        grid2.showicons.Add(new Grid.icon() { interf = mav, x = x, y = y, z = 0, Name = mav.ToString(), Color = Color.Yellow });
                    }
                }
            }
            grid2.DrawTransLine = true;
        }
        public System.Numerics.Vector2 GetoffsetbyPoint(PointLatLngAlt base_point, PointLatLngAlt tar_point)
        {
            utmpos basepos = new utmpos(base_point);
            utmpos tarpos = new utmpos(tar_point);
            if (grid2.Vertical)
            {
                return new System.Numerics.Vector2((float)(tarpos.x - basepos.x), (float)(tar_point.Alt - base_point.Alt));
            }
            else
            {
                return new System.Numerics.Vector2((float)(tarpos.x - basepos.x), (float)(tarpos.y - basepos.y));
            }
        }
        //private struct FollowoffsetDis
        //{
        //    public int distance;
        //    public Vector3 followoffset;
        //}
        private void myButton_Transformation_Click(object sender, EventArgs e)
        {
            if (SwarmInterface.Leader == null)
            {
                MessageBox.Show("请先在编队控制界面设置长机，再执行编队变换");
                return;
            }
            // 没有可选择的队形
            if (comboBox_formation_select.SelectedItem == null && listBox_formationShape.SelectedItem == null)
            {
                MessageBox.Show("请先添加一个队形，再执行编队变换");
                return;
            }
            int totalmavcount = (int)getMavCounts();
            if (totalmavcount<=1)
            {
                MessageBox.Show("至少需要两架飞机");
                return;
            }

            ShapeChangeAction action = calculateShapeChangeAction();
            if (action == ShapeChangeAction.NONE)
            {
                hungarianAlgorithmRun();
            }
            else
            {
                specialShapeChangeRun(action);
            }
            updateicons();
            grid2updateicons();
            recordLastShapeInfo();
        }

        private void hungarianAlgorithmRun() {
            int totalmavcount = (int)getMavCounts();
            // 定义效益矩阵
            int[,] cost_array = new int[totalmavcount - 1, totalmavcount - 1];
            // 计算两个界面直接的偏差
            Vector3 leaderRelativeOffset = new Vector3();
            List<Vector3> followOffsets = new List<Vector3>();
            followOffsets.Clear();
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == SwarmInterface.Leader)
                    {
                        leaderRelativeOffset = new Vector3(SwarmInterface.grid2offsets[mav]);
                    }
                    else
                    {
                        Vector3 otherOffset = new Vector3(SwarmInterface.grid2offsets[mav]);
                        followOffsets.Add(otherOffset);
                    }
                }
            }
            // 将所有偏移量映射到编队控制界面下的偏移
            foreach (var followOffset in followOffsets)
            {
                followOffset.x -= leaderRelativeOffset.x;
                followOffset.y -= leaderRelativeOffset.y;
                followOffset.z -= leaderRelativeOffset.z;
            }
            int row = 0;
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    //FollowoffsetDis offsetDis = new FollowoffsetDis();
                    //List<FollowoffsetDis> followoffsetDis = new List<FollowoffsetDis>();
                    //followoffsetDis.Clear();
                    if (mav == SwarmInterface.Leader)
                    {
                        continue;
                    }
                    foreach (var follow in followOffsets)
                    {
                        PointLatLngAlt target = new PointLatLngAlt(SwarmInterface.Leader.cs.lat, SwarmInterface.Leader.cs.lng, SwarmInterface.Leader.cs.alt);
                        PointLatLngAlt mavTarget = new PointLatLngAlt(GetMavTarget(target, SwarmInterface.Leader.cs.yaw, follow));
                        var horizontalDist = mavTarget.GetDistance(mav.cs.Location);
                        var verticalDist = Math.Abs(mavTarget.Alt - target.Alt);
                        var dist = Math.Sqrt(horizontalDist * horizontalDist + verticalDist * verticalDist);
                        cost_array[row, followOffsets.IndexOf(follow)] = (int)(100 * dist);
                    }
                    row++;
                }
            }
            // 匈牙利算法
            int[] result = HungarianAlgorithm.FindAssignments(cost_array);
            int index = 0;
            mavIndex.Clear();
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == SwarmInterface.Leader)
                    {
                        mavIndex[mav] = index < result.Length ? result[index] : 0;
                        continue;
                    }
                    mavIndex[mav] = index < result.Length ? result[index] : 0;
                    SwarmInterface.setOffsets(mav, followOffsets[result[index]].x, followOffsets[result[index]].y, followOffsets[result[index]].z);
                    index++;
                }
            }
        }

        private void specialShapeChangeRun(ShapeChangeAction action) {
            switch (action) {
                case ShapeChangeAction.HORIZONTAL2VERTICAL:
                case ShapeChangeAction.VERTICA2HORIZONTAL:
                    {
                        foreach (var port in MainV2.Comports)
                        {
                            foreach (var mav in port.MAVlist)
                            {
                                if (mav == SwarmInterface.Leader)
                                {
                                    continue;
                                }
                                Vector3 offset = SwarmInterface.getOffsets(mav);
                                SwarmInterface.setOffsets(mav, offset.x, offset.z, offset.y);
                            }
                            
                        }
                    }
                    break;
                case ShapeChangeAction.HORIZONTAL2TILT:
                    {
                        foreach (var port in MainV2.Comports)
                        {
                            foreach (var mav in port.MAVlist)
                            {
                                if (mav == SwarmInterface.Leader)
                                {
                                    continue;
                                }
                                Vector3 offset = SwarmInterface.getOffsets(mav);
                                SwarmInterface.setOffsets(mav, offset.x, offset.y, offset.y);
                            }

                        }
                    }
                    break;
                case ShapeChangeAction.TILT2VERTICAL:
                    {
                        foreach (var port in MainV2.Comports)
                        {
                            foreach (var mav in port.MAVlist)
                            {
                                if (mav == SwarmInterface.Leader)
                                {
                                    continue;
                                }
                                Vector3 offset = SwarmInterface.getOffsets(mav);
                                SwarmInterface.setOffsets(mav, offset.x, 0, offset.z);
                            }

                        }
                    }
                    break;
                case ShapeChangeAction.VERTICAL2TILT:
                    {
                        foreach (var port in MainV2.Comports)
                        {
                            foreach (var mav in port.MAVlist)
                            {
                                if (mav == SwarmInterface.Leader)
                                {
                                    continue;
                                }
                                Vector3 offset = SwarmInterface.getOffsets(mav);
                                SwarmInterface.setOffsets(mav, offset.x, offset.z, offset.z);
                            }

                        }
                    }
                    break;
                case ShapeChangeAction.TILT2HORIZONTAL:
                    {
                        foreach (var port in MainV2.Comports)
                        {
                            foreach (var mav in port.MAVlist)
                            {
                                if (mav == SwarmInterface.Leader)
                                {
                                    continue;
                                }
                                Vector3 offset = SwarmInterface.getOffsets(mav);
                                SwarmInterface.setOffsets(mav, offset.x, offset.y, 0);
                            }

                        }
                    }
                    break;
                case ShapeChangeAction.HORIZONTAL2HORIZONTAL:
                case ShapeChangeAction.VERTICA2VERTICA:
                case ShapeChangeAction.TILT2TILT:
                    {
                        // 计算两个界面直接的偏差
                        Vector3 leaderRelativeOffset = new Vector3();
                        List<Vector3> followOffsets = new List<Vector3>();
                        followOffsets.Clear();
                        foreach (var port in MainV2.Comports)
                        {
                            foreach (var mav in port.MAVlist)
                            {
                                if (mav == SwarmInterface.Leader)
                                {
                                    leaderRelativeOffset = new Vector3(SwarmInterface.grid2offsets[mav]);
                                }
                                else
                                {
                                    Vector3 otherOffset = new Vector3(SwarmInterface.grid2offsets[mav]);
                                    followOffsets.Add(otherOffset);
                                }
                            }
                        }
                        // 将所有偏移量映射到编队控制界面下的偏移
                        foreach (var followOffset in followOffsets)
                        {
                            followOffset.x -= leaderRelativeOffset.x;
                            followOffset.y -= leaderRelativeOffset.y;
                            followOffset.z -= leaderRelativeOffset.z;
                        }
                        foreach (var port in MainV2.Comports)
                        {
                            foreach (var mav in port.MAVlist)
                            {
                                if (mav == SwarmInterface.Leader)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (mavIndex.ContainsKey(mav))
                                    {
                                        int index = mavIndex[mav];
                                        SwarmInterface.setOffsets(mav, followOffsets[index].x, followOffsets[index].y, followOffsets[index].z);
                                    }
                                }
                            }
                        }

                    }
                    break;
                default:
                    break;
            }
        }

        private void recordLastShapeInfo() {
            if (comboBox_formation_select.SelectedItem == null) {
                lastShape = "";
                lastDistance = -1;
                return;
            }
            lastShape = comboBox_formation_select.SelectedItem.ToString();
            lastDistance = trackBar_swarmspace.Value;
        }

        private ShapeChangeAction calculateShapeChangeAction() {
            if (lastShape == "" || lastDistance == -1) {
                return ShapeChangeAction.NONE;
            }
            if (comboBox_formation_select.SelectedItem == null)
            {
                return ShapeChangeAction.NONE;
            }
            string newShape = comboBox_formation_select.SelectedItem.ToString();
            int newDistance = trackBar_swarmspace.Value;
            if (lastShape == "横网形" && newShape == "竖网形") {
                return ShapeChangeAction.HORIZONTAL2VERTICAL;
            }

            if (lastShape == "竖网形" && newShape == "横网形") {
                return ShapeChangeAction.VERTICA2HORIZONTAL;
            }

            if (lastShape == "横网形" && newShape == "斜网形")
            {
                return ShapeChangeAction.HORIZONTAL2TILT;
            }

            if (lastShape == "斜网形" && newShape == "竖网形")
            {
                return ShapeChangeAction.TILT2VERTICAL;
            }

            if (lastShape == "竖网形" && newShape == "斜网形")
            {
                return ShapeChangeAction.VERTICAL2TILT;
            }

            if (lastShape == "斜网形" && newShape == "横网形")
            {
                return ShapeChangeAction.TILT2HORIZONTAL;
            }

            if (lastShape == "横网形" && newShape == "横网形")
            {
                return ShapeChangeAction.HORIZONTAL2HORIZONTAL;
            }

            if (lastShape == "竖网形" && newShape == "竖网形")
            {
                return ShapeChangeAction.VERTICA2VERTICA;
            }

            if (lastShape == "斜网形" && newShape == "斜网形")
            {
                return ShapeChangeAction.TILT2TILT;
            }

            return ShapeChangeAction.NONE;
        }
        
        //convert Wgs84ConversionInfo to utm
        CoordinateTransformationFactory myctfac = new CoordinateTransformationFactory();

        GeographicCoordinateSystem mywgs84 = GeographicCoordinateSystem.WGS84;
        public PointLatLngAlt GetMavTarget(PointLatLngAlt target, double yaw,Vector3 offsettoleader)
        {
            //转换到WSG84_UTM坐标系
            
            int utmzone = (int)((target.Lng - -186.0) / 6.0);

            IProjectedCoordinateSystem utm = ProjectedCoordinateSystem.WGS84_UTM(utmzone,
                target.Lat < 0 ? false : true);

            ICoordinateTransformation trans = myctfac.CreateFromCoordinateSystems(mywgs84, utm);

            double[] pll1 = { target.Lng, target.Lat };

            double[] p1 = trans.MathTransform.Transform(pll1);

            double heading = -yaw;

            double length = offsettoleader.length();

            var x = offsettoleader.x;
            var y = offsettoleader.y;

            // add offsets to utm
            p1[0] += x * Math.Cos(heading * MathHelper.deg2rad) - y * Math.Sin(heading * MathHelper.deg2rad);
            p1[1] += x * Math.Sin(heading * MathHelper.deg2rad) + y * Math.Cos(heading * MathHelper.deg2rad);
            // convert back to wgs84
            IMathTransform inversedTransform = trans.MathTransform.Inverse();
            double[] point = inversedTransform.Transform(p1);

            target.Lat = point[1];
            target.Lng = point[0];
            target.Alt += offsettoleader.z;
            return target;
        }

        private void BUT_openFormationShapeFile(object sender, EventArgs e)
        {
            var formationshapefile = new OpenFileDialog();
            if (formationshapefile.ShowDialog() == DialogResult.OK)
            {
                textBox_filepath.Text = formationshapefile.FileName;
                Open_formationShapeFile(textBox_filepath.Text);
                loadItemToListBox(formationshapelines);
            }
            else
            {
                textBox_filepath.Text = "";
            }
        }
        private void Open_formationShapeFile(string filepath)
        {
            FileStream fs_formationshape = null;
            try
            {
                fs_formationshape = new FileStream(@filepath, FileMode.Open, FileAccess.Read);//只读打开位置文件
            }
            catch
            {
                textBox_filepath.Text = "";
                MessageBox.Show("编队队形文件打开失败！");
                return;
            }
            StreamReader formationShapeStreamReader = new StreamReader(fs_formationshape, Encoding.Default);
            fs_formationshape.Seek(0, SeekOrigin.Begin);
            string line = formationShapeStreamReader.ReadLine();
            if (line != null)
            {
                string[] data = line.Split(':');
                if (data[0].Equals("TotalFormationShapeStart") == false)
                {
                    MessageBox.Show("不是编队队形位置文件！");
                    fs_formationshape.Close();
                    return;
                }
                else
                {
                    formationshapelines.Clear();
                    while (line != null)
                    {
                        line = formationShapeStreamReader.ReadLine();
                        formationshapelines.Add(line);
                        if (line.Equals("TotalFormationShapeEnd"))
                        {
                            break;
                        }
                    }
                }
            }
            if (fs_formationshape != null)
            {
                fs_formationshape.Close();
            }
            if (formationShapeStreamReader !=null)
            {
                formationShapeStreamReader.Close();
            }
        }
        private void loadItemToListBox(List<string> stringList)
        {
            if (stringList.Count <=0)
            {
                return;
            }
            formationShapeList.Clear();
            string[] data = stringList[0].Split(':');
            int shapecounts = int.Parse(data[1]);
            int i = 0;
            while (i< stringList.Count)
            {
                // 判断是否到达了文件的底部
                if (stringList[i].Equals("TotalFormationShapeEnd"))
                {
                    break;
                }
                string[] linedata1 = stringList[i].Split(':');
                // 找到相应的队形
                if (linedata1[0].Equals("ShapeBegin"))
                {
                    Vector3Array mavArrayPos = new Vector3Array((uint)shapecounts);
                    for (int j = 0; j < shapecounts; j++)
                    {
                        string[] linedata2 = stringList[i+1+j].Split(':');
                        mavArrayPos.vector3Array[j].x =float.Parse(linedata2[2]);
                        mavArrayPos.vector3Array[j].y = float.Parse(linedata2[4]);
                        mavArrayPos.vector3Array[j].z = float.Parse(linedata2[6]);
                    }
                    formationShapeList.Add(mavArrayPos);
                }
                i++;
            }
            listBox_formationShape.Items.Clear();
            for (i = 0; i < formationShapeList.Count; i++)
            {
                listBox_formationShape.Items.Add("队形" + (i + 1).ToString());
            }

        }
        private void BUT_CreateForamtionShape(object sender, EventArgs e)
        {
            // 计算飞机总数
            uint totalmavcount = getMavCounts();
            if (SwarmInterface == null)
            {
                return;
            }
            Vector3Array mavArray= new Vector3Array(totalmavcount);
            uint index = 0;
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    mavArray.vector3Array[index].x = SwarmInterface.getgrid2Offsets(mav).x;
                    mavArray.vector3Array[index].y = SwarmInterface.getgrid2Offsets(mav).y;
                    mavArray.vector3Array[index].z = SwarmInterface.getgrid2Offsets(mav).z;
                    index++;
                }
            }
            formationShapeList.Add(mavArray);
            listBox_formationShape.Items.Clear();
            for (uint i = 0;i< formationShapeList.Count;i++)
            {
                listBox_formationShape.Items.Add("队形" + (i + 1).ToString());
            }
        }

        private void FormationShape_SelectedChanged(object sender, EventArgs e)
        {
            if (listBox_formationShape.SelectedItem == null)
            {
                return;
            }
            // 第一步先让已有队形的选中消失，优先选择用户当前的选项。
            comboBox_formation_select.SelectedItem = null;
            int index = listBox_formationShape.SelectedIndex;
            //当选中队形时，更新队形变换中的界面
            if (index >= 0)
            {
                display_formation_shape(index);
            }
        }
        private void display_formation_shape(int index)
        {
            // 计算飞机总数
            uint totalmavcount = getMavCounts();
            if (totalmavcount<=1)
            {
                MessageBox.Show("已连接的飞机至少为2架，请增加飞机连接");
                return;
            }
            try
            {
                int i = 0;
                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        // 如果实际飞机数量比选中中的队形少，则尽可能选择数组前面的
                        if (i < formationShapeList[index].Count)
                        {
                            ((Formation)SwarmInterface).setgrid2Offsets(mav, formationShapeList[index].vector3Array[i].x, formationShapeList[index].vector3Array[i].y, formationShapeList[index].vector3Array[i].z);
                        }
                        else // 实际飞机比选中队形中的飞机还要多，将此飞机偏移设置成零，防止formationShapeList[index].vector3Array数组越界
                        {
                            ((Formation)SwarmInterface).setgrid2Offsets(mav, 0, 0, 0);
                        }
                        i++;
                    }
                }
                
                grid2updateicons();
            }
            catch
            {
                MessageBox.Show("队形与实际飞机匹配失败");
            }
        }
        // 选中对应listbox的队形，右键菜单显示
        private int mouse_selete_listbox_index = -1;
        private void listbox_MouseUp(object sender, MouseEventArgs e)
        {
            mouse_selete_listbox_index = listBox_formationShape.IndexFromPoint(new Point(e.X, e.Y));
            if (mouse_selete_listbox_index != -1)
            {
                if (e.Button == MouseButtons.Right)
                {
                    listBox_formationShape.SelectedIndex = mouse_selete_listbox_index;
                    contextMenuStrip1.Show(this.listBox_formationShape,e.X, e.Y);
                }
            }
        }
        // 选中对应listbox的队形，右键删除属性
        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mouse_selete_listbox_index != -1)
            {
                listBox_formationShape.Items.Remove(listBox_formationShape.Items[mouse_selete_listbox_index]);
                formationShapeList.RemoveAt(mouse_selete_listbox_index);
                listBox_formationShape.Items.Clear();
                for (uint i = 0; i < formationShapeList.Count; i++)
                {
                    listBox_formationShape.Items.Add("队形" + (i + 1).ToString());
                }
            }

        }
        // 修改添加的队形
        private void ChangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mouse_selete_listbox_index != -1)
            {
                uint index = 0;
                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        formationShapeList[mouse_selete_listbox_index].vector3Array[index].x = SwarmInterface.getgrid2Offsets(mav).x;
                        formationShapeList[mouse_selete_listbox_index].vector3Array[index].y = SwarmInterface.getgrid2Offsets(mav).y;
                        formationShapeList[mouse_selete_listbox_index].vector3Array[index].z = SwarmInterface.getgrid2Offsets(mav).z;
                        index++;
                    }
                }
            }
        }

        private void BUT_SaveFormationShapeFile(object sender, EventArgs e)
        {
            var formationshapefile = new SaveFileDialog();
            if (formationshapefile.ShowDialog() == DialogResult.OK)
            {
                textBox_filepath.Text = formationshapefile.FileName;
            }
            if (formationShapeList.Count == 0 || listBox_formationShape.Items.Count == 0)
            {
                MessageBox.Show("请先设计好队形，然后点创建文件再保存");
                return;
            }
            string formationShapeWriteLine;
            try
            {
                uint totalmavcount = getMavCounts();
                StreamWriter formationShapeWriter = new StreamWriter(File.Create(@textBox_filepath.Text));
                formationShapeWriteLine = "TotalFormationShapeStart:byZengXu";
                formationShapeWriter.WriteLine(formationShapeWriteLine);
                formationShapeWriteLine = "TotalMavCounts:" + totalmavcount.ToString();
                formationShapeWriter.WriteLine(formationShapeWriteLine);
                for (int i=0;i< formationShapeList.Count; i++)
                {
                    formationShapeWriteLine = "ShapeBegin:" + (i + 1).ToString();
                    formationShapeWriter.WriteLine(formationShapeWriteLine);
                    for (int j=0;j< formationShapeList[i].Count;j++)
                    {
                        formationShapeWriteLine = (j + 1).ToString() + ":x:" + ((int)formationShapeList[i].vector3Array[j].x).ToString() + ":y:" + ((int)formationShapeList[i].vector3Array[j].y).ToString() + ":z:" + ((int)formationShapeList[i].vector3Array[j].z).ToString();
                        formationShapeWriter.WriteLine(formationShapeWriteLine);
                    }
                    formationShapeWriteLine = "ShapeFinished:" + (i + 1).ToString();
                    formationShapeWriter.WriteLine(formationShapeWriteLine);
                }
                formationShapeWriteLine = "TotalFormationShapeEnd";
                formationShapeWriter.WriteLine(formationShapeWriteLine);
                if (formationShapeWriter != null)
                {
                    formationShapeWriter.Close();
                }
            }
            catch
            {
                MessageBox.Show("保存队形文件失败");
            }
        }

        private void ZedGraphTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Make sure that the curvelist has at least one curve
                if (zg1.GraphPane.CurveList.Count <= 0)
                    return;

                // Get the first CurveItem in the graph
                LineItem curve = zg1.GraphPane.CurveList[0] as LineItem;
                if (curve == null)
                    return;

                // Get the PointPairList
                IPointListEdit list = curve.Points as IPointListEdit;
                // If this is null, it means the reference at curve.Points does not
                // support IPointListEdit, so we won't be able to modify it
                if (list == null)
                    return;

                // Time is measured in seconds
                double time = (Environment.TickCount - tickStart) / 1000.0;

                // Keep the X scale at a rolling 30 second interval, with one
                // major step between the max X value and the end of the axis
                Scale xScale = zg1.GraphPane.XAxis.Scale;
                if (time > xScale.Max - xScale.MajorStep)
                {
                    xScale.Max = time + xScale.MajorStep;
                    xScale.Min = xScale.Max - 10.0;
                }

                // Make sure the Y axis is rescaled to accommodate actual data
                zg1.AxisChange();

                // Force a redraw

                zg1.Invalidate();
            }
            catch
            {
            }
        }
        // 10hz
        private void Timer3_Tick(object sender, EventArgs e)
        {
            updateGraphLine();
            if (gotorearch)
            {
                if (Formationoverallparameters.AvoidanceEnable)
                {
                    SwarmInterface.ObstacleAvoidanceAlgorithm(Formationoverallparameters.VirtualLeaderEnable, false);
                }
                else
                {
                    Reset_Obstacle_ControlVecter();
                }
                CollaborativeSearchRun();
            }
        }
        
        private void CollaborativeSearchRun()
        {
            // 检查目标是否搜寻到的次数
            CheckWasSearchedCounts(searchsimulation);
            // 协同搜索算法
            // 目标搜索
            if (radioButton_targetsearch.Checked)
            {
                foreach (var polygon in searchareaoverlay.Polygons)
                {
                    // 区域搜索编队控制
                    foreach (var port in MainV2.Comports)
                    {
                        foreach (var mav in port.MAVlist)
                        {
                            if (internalformations[polygon].mAVStates.Contains(mav))
                            {
                                if (mav == internalformations[polygon].Leader)
                                {
                                    if (mav.obsvector.z != 0 && mav.obstacleid != 0)
                                    {
                                        // 长机对于本编队的内部飞机不避障，长机避障采取错开高度。
                                        if (!internalformations[polygon].IsInThismavs(mav.obstacleid))
                                        {
                                            port.setNewWPAlt(mav.sysid, mav.compid, new Locationwp { alt = (float)Math.Max(mav.obsvector.z, 30) / CurrentState.multiplieralt });
                                            internalformations[polygon].lastmisssionalt = Math.Max(mav.obsvector.z, 30);
                                        }
                                    }
                                    else
                                    {
                                        // 避障结束之后恢复高度
                                        if (!internalformations[polygon].lastmisssionalt.Equals(internalformations[polygon].misssionalt))
                                        {
                                            port.setNewWPAlt(mav.sysid, mav.compid, new Locationwp { alt = (float)internalformations[polygon].misssionalt / CurrentState.multiplieralt });
                                            internalformations[polygon].lastmisssionalt = internalformations[polygon].misssionalt;
                                        }
                                    }
                                    continue;
                                }
                                PointLatLngAlt leaderpos = new PointLatLngAlt(internalformations[polygon].Leader.cs.lat, internalformations[polygon].Leader.cs.lng, internalformations[polygon].Leader.cs.alt);
                                PointLatLngAlt target = GetMavTarget(leaderpos, internalformations[polygon].Leader.cs.yaw, mav.internaloffset);
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
                                    SwarmInterface.ArduplaneCommand(port, mav, new PointLatLngAlt(target),internalformations[polygon].Leader, mav.internaloffset);
                                }
                                else
                                {
                                    SwarmInterface.ArducopterCommand(port, mav, new PointLatLngAlt(target), internalformations[polygon].Leader);
                                }
                            }
                        }
                    }
                }
            }// 目标追踪
            else if (radioButton_tragettrack.Checked)
            {
                foreach (var polygon in searchareaoverlay.Polygons)
                {
                    foreach (var port in MainV2.Comports)
                    {
                        foreach (var mav in port.MAVlist)
                        {
                            if (internalformations[polygon].mAVStates.Contains(mav))
                            {
                                if (mav == internalformations[polygon].Leader)
                                {
                                    if (mav.obsvector.z != 0 && mav.obstacleid != 0)
                                    {
                                        // 长机对于本编队的内部飞机不避障，长机避障采取错开高度。
                                        if (!internalformations[polygon].IsInThismavs(mav.obstacleid))
                                        {
                                            port.setNewWPAlt(mav.sysid, mav.compid, new Locationwp { alt = (float)Math.Max(mav.obsvector.z, 30) / CurrentState.multiplieralt });
                                            internalformations[polygon].lastmisssionalt = Math.Max(mav.obsvector.z, 30);
                                        }
                                    }
                                    else
                                    {
                                        // 避障结束之后恢复高度
                                        if (!internalformations[polygon].lastmisssionalt.Equals(internalformations[polygon].misssionalt))
                                        {
                                            port.setNewWPAlt(mav.sysid, mav.compid, new Locationwp { alt = (float)internalformations[polygon].misssionalt / CurrentState.multiplieralt });
                                            internalformations[polygon].lastmisssionalt = internalformations[polygon].misssionalt;
                                        }
                                    }
                                    continue;
                                }
                                PointLatLngAlt leaderpos = new PointLatLngAlt(internalformations[polygon].Leader.cs.lat, internalformations[polygon].Leader.cs.lng, internalformations[polygon].Leader.cs.alt);
                                PointLatLngAlt target = GetMavTarget(leaderpos, internalformations[polygon].Leader.cs.yaw, mav.internaloffset);
                                // 本机发现目标
                                if (mav.targetTrackingInfo.newfind)
                                {
                                    PointLatLngAlt newtarget = new PointLatLngAlt(mav.targetTrackingInfo.targets[mav.targetTrackingInfo.targets.Count - 1].Lat, mav.targetTrackingInfo.targets[mav.targetTrackingInfo.targets.Count - 1].Lng, target.Alt);
                                    if (mav.obsvector.x != 0 || mav.obsvector.y != 0 || mav.obsvector.z != 0)
                                    {
                                        newtarget = newtarget.gps_offset(mav.obsvector.y, mav.obsvector.x);
                                        if (mav.obsvector.z != 0)
                                        {
                                            newtarget.Alt = mav.obsvector.z;
                                        }
                                    }
                                    if (mav.cs.firmware == Firmwares.ArduPlane)
                                    {
                                        Locationwp gotohere = new Locationwp();

                                        gotohere.id = (ushort)MAVLink.MAV_CMD.WAYPOINT;
                                        gotohere.alt = (float)newtarget.Alt; // back to m
                                        gotohere.lat = (newtarget.Lat);
                                        gotohere.lng = (newtarget.Lng);
                                        port.setGuidedModeWP(mav.sysid, mav.compid, gotohere);
                                    }
                                    else
                                    {
                                        port.setPositionTargetGlobalInt(mav.sysid, mav.compid, true, false, false, false,
                                            MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT_INT, newtarget.Lat, newtarget.Lng, newtarget.Alt, 0, 0, 0, 0, 0);
                                    }
                                }
                                else
                                {
                                    if (mav.targetTrackingInfo.targets.Count > 0)
                                    {
                                        // 丢失目标后根据运动规律预测跟踪3秒，预测取目标数组内10个数据预测出运动方位
                                        if (DateTime.Now.Subtract(mav.targetTrackingInfo.timestamp).TotalSeconds < 3)
                                        {
                                            if (mav.targetTrackingInfo.targets.Count < 10)
                                            {
                                                PointLatLngAlt startpoint = mav.targetTrackingInfo.targets[0];
                                                PointLatLngAlt endpoint = mav.targetTrackingInfo.targets[mav.targetTrackingInfo.targets.Count - 1];
                                                double bearing = startpoint.GetBearing(endpoint);
                                                PointLatLngAlt newtarget = new PointLatLngAlt(mav.targetTrackingInfo.targets[mav.targetTrackingInfo.targets.Count - 1].Lat, mav.targetTrackingInfo.targets[mav.targetTrackingInfo.targets.Count - 1].Lng, target.Alt);
                                                newtarget = newtarget.newpos(bearing, mav.cs.groundspeed * DateTime.Now.Subtract(mav.targetTrackingInfo.timestamp).TotalSeconds);
                                                if (mav.obsvector.x != 0 || mav.obsvector.y != 0 || mav.obsvector.z != 0)
                                                {
                                                    newtarget = newtarget.gps_offset(mav.obsvector.y, mav.obsvector.x);
                                                    if (mav.obsvector.z != 0)
                                                    {
                                                        newtarget.Alt = mav.obsvector.z;
                                                    }
                                                }
                                                if (mav.cs.firmware == Firmwares.ArduPlane)
                                                {
                                                    Locationwp gotohere = new Locationwp();

                                                    gotohere.id = (ushort)MAVLink.MAV_CMD.WAYPOINT;
                                                    gotohere.alt = (float)newtarget.Alt; // back to m
                                                    gotohere.lat = (newtarget.Lat);
                                                    gotohere.lng = (newtarget.Lng);
                                                    port.setGuidedModeWP(mav.sysid, mav.compid, gotohere);
                                                }
                                                else
                                                {
                                                    port.setPositionTargetGlobalInt(mav.sysid, mav.compid, true, false, false, false,
                                                        MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT_INT, newtarget.Lat, newtarget.Lng, newtarget.Alt, 0, 0, 0, 0, 0);
                                                }
                                            }
                                            else
                                            {
                                                PointLatLngAlt startpoint = mav.targetTrackingInfo.targets[mav.targetTrackingInfo.targets.Count - 10];
                                                PointLatLngAlt endpoint = mav.targetTrackingInfo.targets[mav.targetTrackingInfo.targets.Count - 1];
                                                double bearing = startpoint.GetBearing(endpoint);
                                                PointLatLngAlt newtarget = new PointLatLngAlt(mav.targetTrackingInfo.targets[mav.targetTrackingInfo.targets.Count - 1].Lat, mav.targetTrackingInfo.targets[mav.targetTrackingInfo.targets.Count - 1].Lng, target.Alt);
                                                newtarget = newtarget.newpos(bearing, mav.cs.groundspeed * DateTime.Now.Subtract(mav.targetTrackingInfo.timestamp).TotalSeconds);
                                                if (mav.obsvector.x != 0 || mav.obsvector.y != 0 || mav.obsvector.z != 0)
                                                {
                                                    newtarget = newtarget.gps_offset(mav.obsvector.y, mav.obsvector.x);
                                                    if (mav.obsvector.z != 0)
                                                    {
                                                        newtarget.Alt = mav.obsvector.z;
                                                    }
                                                }
                                                if (mav.cs.firmware == Firmwares.ArduPlane)
                                                {
                                                    Locationwp gotohere = new Locationwp();

                                                    gotohere.id = (ushort)MAVLink.MAV_CMD.WAYPOINT;
                                                    gotohere.alt = (float)newtarget.Alt; // back to m
                                                    gotohere.lat = (newtarget.Lat);
                                                    gotohere.lng = (newtarget.Lng);
                                                    port.setGuidedModeWP(mav.sysid, mav.compid, gotohere);
                                                }
                                                else
                                                {
                                                    port.setPositionTargetGlobalInt(mav.sysid, mav.compid, true, false, false, false,
                                                        MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT_INT, newtarget.Lat, newtarget.Lng, newtarget.Alt, 0, 0, 0, 0, 0);
                                                }
                                            }
                                        }
                                        else
                                        {
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
                                                SwarmInterface.ArduplaneCommand(port, mav, new PointLatLngAlt(target), internalformations[polygon].Leader, mav.internaloffset);
                                            }
                                            else
                                            {
                                                SwarmInterface.ArducopterCommand(port, mav, new PointLatLngAlt(target), internalformations[polygon].Leader);
                                            }
                                        }
                                    }
                                    else
                                    {
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
                                            SwarmInterface.ArduplaneCommand(port, mav, new PointLatLngAlt(target), internalformations[polygon].Leader, mav.internaloffset);
                                        }
                                        else
                                        {
                                            SwarmInterface.ArducopterCommand(port, mav, new PointLatLngAlt(target), internalformations[polygon].Leader);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        int targetWasSearchedCounts = 0;
        double searchRadius = 60;
        List<PointLatLngAlt> targetsFoundListRecord = new List<PointLatLngAlt>();
        private void CheckWasSearchedCounts(bool simution)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (simution)
                    {
                        mav.targetTrackingInfo.newfind = false;
                        foreach (GMapMarkerVirtualTarget marker in virtualtargetoverlay.Markers)
                        {
                            PointLatLngAlt markerpoint = new PointLatLngAlt(marker.Position.Lat, marker.Position.Lng);
                            if (mav.cs.Location.GetDistance(markerpoint) < searchRadius * 2)
                            {
                                targetWasSearchedCounts++;
                                addTargetFounds(markerpoint,targetWasSearchedCounts);
                                mav.targetTrackingInfo.newfind = true;
                                mav.targetTrackingInfo.timestamp = DateTime.Now;
                                if (mav.targetTrackingInfo.targets.Count < mav.targetTrackingInfo.maxrecord)
                                {
                                    mav.targetTrackingInfo.targets.Add(markerpoint);
                                }
                                else
                                {
                                    mav.targetTrackingInfo.targets.RemoveAt(0);
                                    mav.targetTrackingInfo.targets.Add(markerpoint);
                                }
                            }
                        }
                    }
                    else
                    {
                        // 实际搜索根据实际飞机的传感器测量得到目标是否找到
                    }
                }
            }
            if (targetWasSearchedCounts>0)
            {
                lbl_targetfoundcounts.Text = targetWasSearchedCounts.ToString();
            }
        }
        private void addTargetFounds(PointLatLngAlt markerpoint,int tagcount)
        {
            // list最大保存100个数据
            if (targetsFoundListRecord.Count < 100)
            {
                targetsFoundListRecord.Add(markerpoint);
                targetsFound.Items.Add("lat:" + markerpoint.Lat.ToString("f6") + " " + "lng:" + markerpoint.Lng.ToString("f6"));
                addTargetFoundMarker(new PointLatLng(markerpoint.Lat, markerpoint.Lng), tagcount, false);
            }
            else
            {
                targetsFoundListRecord.RemoveAt(0);
                targetsFoundListRecord.Add(markerpoint);
                targetsFound.Items.RemoveAt(0);
                targetsFound.Items.Add("lat:" + markerpoint.Lat.ToString("f6") + " " + "lng:" + markerpoint.Lng.ToString("f6"));
                addTargetFoundMarker(new PointLatLng(markerpoint.Lat, markerpoint.Lng), tagcount, true);
            }
        }
        private void addTargetFoundMarker(PointLatLng targetpoint,int count,bool remove)
        {
            PointLatLng point = new PointLatLng(targetpoint.Lat, targetpoint.Lng);
            GMarkerGoogle m = new GMarkerGoogle(point, GMarkerGoogleType.yellow);
            m.ToolTipMode = MarkerTooltipMode.OnMouseOver;
            m.ToolTipText = point.Lat.ToString("f6")+ " " + point.Lng.ToString("f6");
            m.Tag = count.ToString();
            if (remove)
            {
                targetsfoundoverlay.Markers.RemoveAt(0);
                targetsfoundoverlay.Markers.Add(m);
            }
            else
            {
                targetsfoundoverlay.Markers.Add(m);
            }
            
        }
        private void updateGraphLine()
        {
            double time = (Environment.TickCount - tickStart) / 1000.0;

            if (comboBox_chartshow.SelectedValue == null)
            {
                return;
            }
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == comboBox_chartshow.SelectedValue)
                    {
                        update_Graph(mav, time);
                    }
                }
            }
        }
        // 定义十种颜色，每次点击从这十种颜色中随机产生颜色的曲线
        List<Color> Colorlist = new List<Color> { Color.Red, Color.Blue, Color.Green, Color.Orange,Color.Yellow,Color.Magenta,Color.Purple, Color.LimeGreen, Color.Cyan , Color.Violet };
        // 随机数
        Random rNumber = new Random();
        private void update_Graph(MAVState mavstate,double xtime)
        {
            if (targetalt == null || alt ==null || navpitch ==null || pitch==null || distotarget == null || navthurst==null || navroll==null|| roll==null|| targetyaw==null|| 
                yaw==null|| navbearing==null|| yawerror==null|| disttoleader==null|| targetspeed==null|| groudspeed == null || courceoftarget == null || speedoftarget == null)
            {
                return;
            }
            if (targetalt.Checked)
            {
                list1.Add(xtime, mavstate.SwarmTargetAlt);
                if (list1curve == null)
                {
                    list1.Clear();
                    list1curve = zg1.GraphPane.AddCurve(targetalt.Text, list1, Colorlist[rNumber.Next(0, Colorlist.Count-1)], SymbolType.None);
                }
            }
            else
            {
                if (list1curve != null)
                {
                    zg1.GraphPane.CurveList.Remove(list1curve);
                    list1curve = null;
                }
            }
            if (alt.Checked)
            {
                list2.Add(xtime, mavstate.cs.alt);
                if (list2curve == null)
                {
                    list2.Clear();
                    list2curve = zg1.GraphPane.AddCurve(alt.Text, list2, Colorlist[rNumber.Next(0, Colorlist.Count - 1)], SymbolType.None);
                }
            }
            else
            {
                if (list2curve != null)
                {
                    zg1.GraphPane.CurveList.Remove(list2curve);
                    list2curve = null;
                }
            }
            if (navpitch.Checked)
            {
                list3.Add(xtime, mavstate.SwarmNavPitch);
                if (list3curve == null)
                {
                    list3.Clear();
                    list3curve = zg1.GraphPane.AddCurve(navpitch.Text, list3, Colorlist[rNumber.Next(0, Colorlist.Count - 1)], SymbolType.None);
                }
            }
            else
            {
                if (list3curve != null)
                {
                    zg1.GraphPane.CurveList.Remove(list3curve);
                    list3curve = null;
                }
            }
            if (pitch.Checked)
            {
                list4.Add(xtime, mavstate.cs.pitch);
                if (list4curve == null)
                {
                    list4.Clear();
                    list4curve = zg1.GraphPane.AddCurve(pitch.Text, list4, Colorlist[rNumber.Next(0, Colorlist.Count - 1)], SymbolType.None);
                }
            }
            else
            {
                if (list4curve != null)
                {
                    zg1.GraphPane.CurveList.Remove(list4curve);
                    list4curve = null;
                }
            }
            if (distotarget.Checked)
            {
                list5.Add(xtime, mavstate.SwarmDistToTarg);
                if (list5curve == null)
                {
                    list5.Clear();
                    list5curve = zg1.GraphPane.AddCurve(distotarget.Text, list5, Colorlist[rNumber.Next(0, Colorlist.Count - 1)], SymbolType.None);
                }
            }
            else
            {
                if (list5curve != null)
                {
                    zg1.GraphPane.CurveList.Remove(list5curve);
                    list5curve = null;
                }
            }
            if (navthurst.Checked)
            {
                list6.Add(xtime, mavstate.SwarmNavThurst);
                if (list6curve == null)
                {
                    list6.Clear();
                    list6curve = zg1.GraphPane.AddCurve(navthurst.Text, list6, Colorlist[rNumber.Next(0, Colorlist.Count - 1)], SymbolType.None);
                }
            }
            else
            {
                if (list6curve != null)
                {
                    zg1.GraphPane.CurveList.Remove(list6curve);
                    list6curve = null;
                }
            }
            if (navroll.Checked)
            {
                list7.Add(xtime, mavstate.SwarmNavRoll);
                if (list7curve == null)
                {
                    list7.Clear();
                    list7curve = zg1.GraphPane.AddCurve(navroll.Text, list7, Colorlist[rNumber.Next(0, Colorlist.Count - 1)], SymbolType.None);
                }
            }
            else
            {
                if (list7curve != null)
                {
                    zg1.GraphPane.CurveList.Remove(list7curve);
                    list7curve = null;
                }
            }
            if (roll.Checked)
            {
                list8.Add(xtime, mavstate.cs.roll);
                if (list8curve == null)
                {
                    list8.Clear();
                    list8curve = zg1.GraphPane.AddCurve(roll.Text, list8, Colorlist[rNumber.Next(0, Colorlist.Count - 1)], SymbolType.None);
                }
            }
            else
            {
                if (list8curve != null)
                {
                    zg1.GraphPane.CurveList.Remove(list8curve);
                    list8curve = null;
                }
            }
            if (targetyaw.Checked)
            {
                list9.Add(xtime, mavstate.SwarmTargYaw);
                if (list9curve == null)
                {
                    list9.Clear();
                    list9curve = zg1.GraphPane.AddCurve(targetyaw.Text, list9, Colorlist[rNumber.Next(0, Colorlist.Count - 1)], SymbolType.None);
                }
            }
            else
            {
                if (list9curve != null)
                {
                    zg1.GraphPane.CurveList.Remove(list9curve);
                    list9curve = null;
                }
            }
            if (yaw.Checked)
            {
                list10.Add(xtime, mavstate.cs.yaw);
                if (list10curve == null)
                {
                    list10.Clear();
                    list10curve = zg1.GraphPane.AddCurve(yaw.Text, list10, Colorlist[rNumber.Next(0, Colorlist.Count - 1)], SymbolType.None);
                }
            }
            else
            {
                if (list10curve != null)
                {
                    zg1.GraphPane.CurveList.Remove(list10curve);
                    list10curve = null;
                }
            }
            if (navbearing.Checked)
            {
                list11.Add(xtime, mavstate.SwarmNavBearing);
                if (list11curve == null)
                {
                    list11.Clear();
                    list11curve = zg1.GraphPane.AddCurve(navbearing.Text, list11, Colorlist[rNumber.Next(0, Colorlist.Count - 1)], SymbolType.None);
                }
            }
            else
            {
                if (list11curve != null)
                {
                    zg1.GraphPane.CurveList.Remove(list11curve);
                    list11curve = null;
                }
            }
            if (yawerror.Checked)
            {
                list12.Add(xtime, mavstate.SwarmYawError);
                if (list12curve == null)
                {
                    list12.Clear();
                    list12curve = zg1.GraphPane.AddCurve(yawerror.Text, list12, Colorlist[rNumber.Next(0, Colorlist.Count - 1)], SymbolType.None);
                }
            }
            else
            {
                if (list12curve != null)
                {
                    zg1.GraphPane.CurveList.Remove(list12curve);
                    list12curve = null;
                }
            }
            if (disttoleader.Checked)
            {
                list13.Add(xtime, mavstate.SwarmDistToLeader);
                if (list13curve == null)
                {
                    list13.Clear();
                    list13curve = zg1.GraphPane.AddCurve(disttoleader.Text, list13, Colorlist[rNumber.Next(0, Colorlist.Count - 1)], SymbolType.None);
                }
            }
            else
            {
                if (list13curve != null)
                {
                    zg1.GraphPane.CurveList.Remove(list13curve);
                    list13curve = null;
                }
            }
            if (targetspeed.Checked)
            {
                list14.Add(xtime, mavstate.SwarmNavSpeed);
                if (list14curve == null)
                {
                    list14.Clear();
                    list14curve = zg1.GraphPane.AddCurve(targetspeed.Text, list14, Colorlist[rNumber.Next(0, Colorlist.Count - 1)], SymbolType.None);
                }
            }
            else
            {
                if (list14curve != null)
                {
                    zg1.GraphPane.CurveList.Remove(list14curve);
                    list14curve = null;
                }
            }
            if (groudspeed.Checked)
            {
                list15.Add(xtime, mavstate.cs.groundspeed);
                if (list15curve == null)
                {
                    list15.Clear();
                    list15curve = zg1.GraphPane.AddCurve(groudspeed.Text, list15, Colorlist[rNumber.Next(0, Colorlist.Count - 1)], SymbolType.None);
                }
            }
            else
            {
                if (list15curve != null)
                {
                    zg1.GraphPane.CurveList.Remove(list15curve);
                    list15curve = null;
                }
            }
            if (courceoftarget.Checked)
            {
                list16.Add(xtime, mavstate.CourseOfTarget);
                if (list16curve == null)
                {
                    list16.Clear();
                    list16curve = zg1.GraphPane.AddCurve(courceoftarget.Text, list16, Colorlist[rNumber.Next(0, Colorlist.Count - 1)], SymbolType.None);
                }
            }
            else
            {
                if (list16curve != null)
                {
                    zg1.GraphPane.CurveList.Remove(list16curve);
                    list16curve = null;
                }
            }
            if (speedoftarget.Checked)
            {
                list17.Add(xtime, mavstate.SpeedOfTarget);
                if (list17curve == null)
                {
                    list17.Clear();
                    list17curve = zg1.GraphPane.AddCurve(speedoftarget.Text, list17, Colorlist[rNumber.Next(0, Colorlist.Count - 1)], SymbolType.None);
                }
            }
            else
            {
                if (list17curve != null)
                {
                    zg1.GraphPane.CurveList.Remove(list17curve);
                    list17curve = null;
                }
            }
        }

        private void Tab_SeletedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab.Name.Equals(tabPage3.Name))
            {
                ZedGraphTimer.Enabled = true;
                ZedGraphTimer.Start();
            }
            else
            {
                ZedGraphTimer.Enabled = false;
            }
        }
        void cam1_camimage(Image camimage)
        {
            userVideo1HudShow.bgimage = camimage;
        }
        void cam2_camimage(Image camimage)
        {
            userVideo2HudShow.bgimage = camimage;
        }
        private void CollisionTimesReset_Click(object sender, EventArgs e)
        {
            maybecollisiontimes = 0;
        }
        private void BTN_ArmCommand(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == CMB_pid.SelectedValue)
                    {
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
        }
        private void BTN_DisarmCommand(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == CMB_pid.SelectedValue)
                    {
                        if (mav.cs.armed)
                        {
                            if (CustomMessageBox.Show("是否确定锁定?", "锁定?", MessageBoxButtons.YesNo) != (int)DialogResult.Yes)
                                return;
                        }
                        port.doARM(mav.sysid, mav.compid, false);
                    }
                }
            }
        }
        private void BTN_ManualMode(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == CMB_pid.SelectedValue)
                    {
                        if (mav.cs.firmware != Firmwares.ArduPlane)
                        {
                            MessageBox.Show("该模式为固定翼模式，请检查飞机类型");
                            return;
                        }
                        port.setMode(mav.sysid, mav.compid, "Manual");
                    }
                }
            }
        }

        private void BTN_FBWAMode(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == CMB_pid.SelectedValue)
                    {
                        if (mav.cs.firmware != Firmwares.ArduPlane)
                        {
                            MessageBox.Show("该模式为固定翼模式，请检查飞机类型");
                            return;
                        }
                        port.setMode(mav.sysid, mav.compid, "FBWA");
                    }
                }
            }
        }

        private void BTN_FBWBMode(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == CMB_pid.SelectedValue)
                    {
                        if (mav.cs.firmware != Firmwares.ArduPlane)
                        {
                            MessageBox.Show("该模式为固定翼模式，请检查飞机类型");
                            return;
                        }
                        port.setMode(mav.sysid, mav.compid, "FBWB");
                    }
                }
            }
        }

        private void BTN_AutoMode(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == CMB_pid.SelectedValue)
                    {
                        port.setMode(mav.sysid, mav.compid, "Auto");
                    }
                }
            }
        }

        private void BTN_LoiterMode(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == CMB_pid.SelectedValue)
                    {
                        //if (mav.cs.firmware != Firmwares.ArduPlane)
                        //{
                        //    MessageBox.Show("该模式为固定翼模式，请检查飞机类型");
                        //    return;
                        //}
                        port.setMode(mav.sysid, mav.compid, "Loiter");
                    }
                }
            }
        }

        private void BTN_QLoiterMode(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == CMB_pid.SelectedValue)
                    {
                        if (mav.cs.firmware != Firmwares.ArduPlane)
                        {
                            MessageBox.Show("该模式为固定翼模式，请检查飞机类型");
                            return;
                        }
                        port.setMode(mav.sysid, mav.compid, "QLoiter");
                    }
                }
            }
        }

        private void BTN_RTLMode(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == CMB_pid.SelectedValue)
                    {
                        port.setMode(mav.sysid, mav.compid, "RTL");
                    }
                }
            }
        }

        private void BTN_QRTLMode(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == CMB_pid.SelectedValue)
                    {
                        if (mav.cs.firmware != Firmwares.ArduPlane)
                        {
                            MessageBox.Show("该模式为固定翼模式，请检查飞机类型");
                            return;
                        }
                        port.setMode(mav.sysid, mav.compid, "QRTL");
                    }
                }
            }
        }

        private void BTN_LandMode(object sender, EventArgs e)
        {
            
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == CMB_pid.SelectedValue)
                    {
                        port.setMode(mav.sysid, mav.compid, "Land");
                    }
                }
            }
        }

        private void BTN_TakeoffMode(object sender, EventArgs e)
        {
            string output = "20";
            float takeoffalt = 20;
            if (DialogResult.OK == InputBox.Show("Alt", "请输入起飞高度", ref output))
            {
                try
                {
                    takeoffalt = float.Parse(output);
                }
                catch
                {
                    MessageBox.Show("输入数据类型错误，系统自动设置起飞高度20米");
                }
                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        if (mav == CMB_pid.SelectedValue)
                        {
                            port.doCommand(mav.sysid, mav.compid, MAVLink.MAV_CMD.TAKEOFF, 0, 0, 0, 0, 0, 0, takeoffalt);
                        }
                    }
                }
            }
        }

        private void BTN_GuidedMode(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == CMB_pid.SelectedValue)
                    {
                        port.setMode(mav.sysid, mav.compid, "Guided");
                    }
                }
            }
        }

        private void CMB_video1sources_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.MONO)
                return;

            int hr;
            int count;
            int size;
            object o;
            IBaseFilter capFilter = null;
            ICaptureGraphBuilder2 capGraph = null;
            AMMediaType media = null;
            VideoInfoHeader v;
            VideoStreamConfigCaps c;
            var modes = new List<GCSBitmapInfo>();

            // Get the ICaptureGraphBuilder2
            capGraph = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
            var m_FilterGraph = (IFilterGraph2)new FilterGraph();

            DsDevice[] capDevices;
            capDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            // 增加视频设备
            hr = m_FilterGraph.AddSourceFilterForMoniker(capDevices[CMB_video1sources.SelectedIndex].Mon, null,
                "Video input", out capFilter);
            try
            {
                DsError.ThrowExceptionForHR(hr);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Can not add video source\n" + ex);
                return;
            }

            // 找到流的接口配置
            hr = capGraph.FindInterface(PinCategory.Capture, MediaType.Video, capFilter, typeof(IAMStreamConfig).GUID,
                out o);
            DsError.ThrowExceptionForHR(hr);

            var videoStreamConfig = o as IAMStreamConfig;
            if (videoStreamConfig == null)
            {
                CustomMessageBox.Show("Failed to get IAMStreamConfig");
                return;
            }

            hr = videoStreamConfig.GetNumberOfCapabilities(out count, out size);
            DsError.ThrowExceptionForHR(hr);
            var TaskMemPointer = Marshal.AllocCoTaskMem(size);
            for (var i = 0; i < count; i++)
            {
                var ptr = IntPtr.Zero;

                hr = videoStreamConfig.GetStreamCaps(i, out media, TaskMemPointer);
                v = (VideoInfoHeader)Marshal.PtrToStructure(media.formatPtr, typeof(VideoInfoHeader));
                c = (VideoStreamConfigCaps)Marshal.PtrToStructure(TaskMemPointer, typeof(VideoStreamConfigCaps));
                modes.Add(new GCSBitmapInfo(v.BmiHeader.Width, v.BmiHeader.Height, c.MaxFrameInterval,
                    c.VideoStandard.ToString(), media));
            }
            Marshal.FreeCoTaskMem(TaskMemPointer);
            DsUtils.FreeAMMediaType(media);

            CMB_video1resolutions.DataSource = modes;

            if (Settings.Instance["video1_options"] != "" && CMB_video1sources.Text != "")
            {
                try
                {
                    CMB_video1resolutions.SelectedIndex = Settings.Instance.GetInt32("video1_options");
                }
                catch
                {
                } // ignore bad entries
            }
        }

        private void CMB_video1sources_Click(object sender, EventArgs e)
        {
            if (MainV2.MONO)
                return;
            // the reason why i dont populate this list is because on linux/mac this call will fail.
            var capt = new Capture();

            var devices = WebCamService.Capture.getDevices();

            CMB_video1sources.DataSource = devices;

            capt.Dispose();
        }
        private void CMB_video2sources_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainV2.MONO)
                return;

            int hr;
            int count;
            int size;
            object o;
            IBaseFilter capFilter = null;
            ICaptureGraphBuilder2 capGraph = null;
            AMMediaType media = null;
            VideoInfoHeader v;
            VideoStreamConfigCaps c;
            var modes = new List<GCSBitmapInfo>();

            // Get the ICaptureGraphBuilder2
            capGraph = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
            var m_FilterGraph = (IFilterGraph2)new FilterGraph();

            DsDevice[] capDevices;
            capDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            // Add the video device
            hr = m_FilterGraph.AddSourceFilterForMoniker(capDevices[CMB_video2sources.SelectedIndex].Mon, null,
                "Video input", out capFilter);
            try
            {
                DsError.ThrowExceptionForHR(hr);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Can not add video source\n" + ex);
                return;
            }

            // Find the stream config interface
            hr = capGraph.FindInterface(PinCategory.Capture, MediaType.Video, capFilter, typeof(IAMStreamConfig).GUID,
                out o);
            DsError.ThrowExceptionForHR(hr);

            var videoStreamConfig = o as IAMStreamConfig;
            if (videoStreamConfig == null)
            {
                CustomMessageBox.Show("Failed to get IAMStreamConfig");
                return;
            }

            hr = videoStreamConfig.GetNumberOfCapabilities(out count, out size);
            DsError.ThrowExceptionForHR(hr);
            var TaskMemPointer = Marshal.AllocCoTaskMem(size);
            for (var i = 0; i < count; i++)
            {
                var ptr = IntPtr.Zero;

                hr = videoStreamConfig.GetStreamCaps(i, out media, TaskMemPointer);
                v = (VideoInfoHeader)Marshal.PtrToStructure(media.formatPtr, typeof(VideoInfoHeader));
                c = (VideoStreamConfigCaps)Marshal.PtrToStructure(TaskMemPointer, typeof(VideoStreamConfigCaps));
                modes.Add(new GCSBitmapInfo(v.BmiHeader.Width, v.BmiHeader.Height, c.MaxFrameInterval,
                    c.VideoStandard.ToString(), media));
            }
            Marshal.FreeCoTaskMem(TaskMemPointer);
            DsUtils.FreeAMMediaType(media);

            CMB_video2resolutions.DataSource = modes;

            if (Settings.Instance["video2_options"] != "" && CMB_video2sources.Text != "")
            {
                try
                {
                    CMB_video2resolutions.SelectedIndex = Settings.Instance.GetInt32("video2_options");
                }
                catch
                {
                } // ignore bad entries
            }
        }
        private void CMB_video2sources_Click(object sender, EventArgs e)
        {
            if (MainV2.MONO)
                return;
            // the reason why i dont populate this list is because on linux/mac this call will fail.
            var capt = new Capture();

            var devices = WebCamService.Capture.getDevices();

            CMB_video2sources.DataSource = devices;

            capt.Dispose();
        }

        private void BUT_video1start_Click(object sender, EventArgs e)
        {
            if (MainV2.MONO)
                return;

            // stop first
            BUT_video1stop_Click(sender, e);

            CMB_video1sources_SelectedIndexChanged(sender,e);

            var bmp = (GCSBitmapInfo)CMB_video1resolutions.SelectedItem;

            try
            {
                // 判断是否与第二个选择相同的设备
                if (CMB_video2sources.SelectedItem != null && CMB_video1sources.SelectedIndex == CMB_video2sources.SelectedIndex)
                {
                    if (cam2 != null)
                    {
                        CustomMessageBox.Show("目标设备已经打开请先关闭");
                        return;
                        //BUT_video2stop_Click(sender, e);
                        //System.Threading.Thread.Sleep(500);
                    }
                }
                if (ConfigPlanner.videosources1index != 0&& CMB_video1sources.SelectedIndex == ConfigPlanner.videosources1index)
                {
                    if (ConfigPlannerInstance!=null)
                    {
                        if (MainV2.cam != null)
                        {
                            CustomMessageBox.Show("目标设备已经打开请先关闭");
                            return;
                            //ConfigPlanner.ConfigPlannerInstance.BUT_videostop_Click(sender, e);
                        }
                    }
                }
                cam1 = new Capture(CMB_video1sources.SelectedIndex, bmp.Media);

                cam1.Start();

                Settings.Instance["video1_device"] = CMB_video1sources.SelectedIndex.ToString();

                Settings.Instance["video1_options"] = CMB_video1resolutions.SelectedIndex.ToString();

                BUT_video1start.Enabled = false;
                userVideo1HudShow.shownamestring = "光电载荷1";
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Camera Fail: " + ex.Message);
            }
        }

        public void BUT_video1stop_Click(object sender, EventArgs e)
        {
            BUT_video1start.Enabled = true;
            if (cam1 != null)
            {
                cam1.Dispose();
                cam1 = null;
            }
        }
        bool video1dropout = false;
        bool huddropoutresize1 = false;
        private void video1_DoubleClick(object sender, EventArgs e)
        {
            if (video1dropout)
                return;
            Form dropout1 = new Form();
            dropout1.Size = new Size(userVideo1HudShow.Width, userVideo1HudShow.Height + 20);
            tableLayoutPanel1.Controls.Remove(userVideo1HudShow);
            dropout1.Controls.Add(userVideo1HudShow);
            dropout1.Resize += dropout1_Resize;
            dropout1.FormClosed += dropout1_FormClosed;
            dropout1.Show();
            video1dropout = true;
        }
        void dropout1_Resize(object sender, EventArgs e)
        {
            if (huddropoutresize1)
                return;

            huddropoutresize1 = true;

            int hudh = userVideo1HudShow.Height;
            int formh = ((Form)sender).Height - 30;

            if (((Form)sender).Height < hudh)
            {
                if (((Form)sender).WindowState == FormWindowState.Maximized)
                {
                    Point tl = ((Form)sender).DesktopLocation;
                    ((Form)sender).WindowState = FormWindowState.Normal;
                    ((Form)sender).Location = tl;
                }
                ((Form)sender).Width = (int)(formh * (userVideo1HudShow.SixteenXNine ? 1.777f : 1.333f));
                ((Form)sender).Height = formh + 20;
            }
            userVideo1HudShow.Refresh();
            huddropoutresize1 = false;
        }
        void dropout1_FormClosed(object sender, FormClosedEventArgs e)
        {
            tableLayoutPanel1.Controls.Add(userVideo1HudShow,0,0);
            tableLayoutPanel1.Refresh();
            video1dropout = false;
        }
        bool video2dropout = false;
        bool huddropoutresize2 = false;
        private void video2_DoubleClick(object sender, EventArgs e)
        {
            if (video2dropout)
                return;
            Form dropout2 = new Form();
            dropout2.Size = new Size(userVideo2HudShow.Width, userVideo2HudShow.Height + 20);
            tableLayoutPanel1.Controls.Remove(userVideo2HudShow);
            dropout2.Controls.Add(userVideo2HudShow);
            dropout2.Resize += dropout2_Resize;
            dropout2.FormClosed += dropout2_FormClosed;
            dropout2.Show();
            video2dropout = true;
        }
        void dropout2_Resize(object sender, EventArgs e)
        {
            if (huddropoutresize2)
                return;

            huddropoutresize2 = true;

            int hudh = userVideo2HudShow.Height;
            int formh = ((Form)sender).Height - 30;

            if (((Form)sender).Height < hudh)
            {
                if (((Form)sender).WindowState == FormWindowState.Maximized)
                {
                    Point tl = ((Form)sender).DesktopLocation;
                    ((Form)sender).WindowState = FormWindowState.Normal;
                    ((Form)sender).Location = tl;
                }
                ((Form)sender).Width = (int)(formh * (userVideo2HudShow.SixteenXNine ? 1.777f : 1.333f));
                ((Form)sender).Height = formh + 20;
            }
            userVideo2HudShow.Refresh();
            huddropoutresize2 = false;
        }
        void dropout2_FormClosed(object sender, FormClosedEventArgs e)
        {
            tableLayoutPanel1.Controls.Add(userVideo2HudShow, 1, 0);
            tableLayoutPanel1.Refresh();
            video2dropout = false;
        }
        private void BUT_video2start_Click(object sender, EventArgs e)
        {
            if (MainV2.MONO)
                return;

            // stop first
            BUT_video2stop_Click(sender, e);
            CMB_video2sources_SelectedIndexChanged(sender, e);

            var bmp = (GCSBitmapInfo)CMB_video2resolutions.SelectedItem;

            try
            {
                // 判断是否与第一个选择相同的设备
                if (CMB_video1sources.SelectedItem != null && CMB_video2sources.SelectedIndex == CMB_video1sources.SelectedIndex)
                {
                    if (cam1 != null)
                    {
                        CustomMessageBox.Show("目标设备已经打开请先关闭");
                        return;
                        // 关掉第一个的选择
                        //BUT_video1stop_Click(sender, e);
                        //System.Threading.Thread.Sleep(500);
                    }
                }
                if (ConfigPlanner.videosources1index != 0 && CMB_video2sources.SelectedIndex == ConfigPlanner.videosources1index)
                {
                    if (ConfigPlannerInstance != null)
                    {
                        if (MainV2.cam != null)
                        {
                            CustomMessageBox.Show("目标设备已经打开请先关闭");
                            return;
                            //ConfigPlanner.ConfigPlannerInstance.BUT_videostop_Click(sender, e);
                        }
                    }
                }
                cam2 = new Capture(CMB_video2sources.SelectedIndex, bmp.Media);

                cam2.Start();

                Settings.Instance["video2_device"] = CMB_video2sources.SelectedIndex.ToString();

                Settings.Instance["video2_options"] = CMB_video2resolutions.SelectedIndex.ToString();

                BUT_video2start.Enabled = false;
                userVideo2HudShow.shownamestring = "光电载荷2";
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Camera Fail: " + ex.Message);
            }
        }

        private void SC7Panel_MouseUP(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip2.Show(this.splitContainer7.Panel1, e.X, e.Y);
            }
        }
        VideoDisplayOut videoDisplayOut = null;
        private void VideoDisplayOut_Click(object sender, EventArgs e)
        {
            if (videoDisplayOut == null)
            {
                videoDisplayOut = new VideoDisplayOut();
                videoDisplayOut.AddHud(userVideo1HudShow, userVideo2HudShow);
                videoDisplayOut.FormClosed += videoDisplayOut_FormClosed;
                videoDisplayOut.Show();
            }
        }
        void videoDisplayOut_FormClosed(object sender, FormClosedEventArgs e)
        {
            tableLayoutPanel1.Controls.Add(userVideo1HudShow, 0, 0);
            tableLayoutPanel1.Controls.Add(userVideo2HudShow, 1, 0);
            tableLayoutPanel1.Refresh();
            videoDisplayOut = null;
        }

        public void BUT_video2stop_Click(object sender, EventArgs e)
        {
            BUT_video2start.Enabled = true;
            if (cam2 != null)
            {
                cam2.Dispose();
                cam2 = null;
            }
        }
        AviWriter video1AviWrite = null;
        AviWriter video2AviWrite = null;
        private void BTN_BrowseVideo1PathChange(object sender, EventArgs e)
        {
            var video1file = new FolderBrowserDialog();
            if (video1file.ShowDialog() == DialogResult.OK)
            {
                txt_video1path.Text = video1file.SelectedPath;
                Settings.Instance["video1_pathfile"] = txt_video1path.Text;
            }
            else
            {
                txt_video1path.Text = "";
            }
        }
        private void BTN_Video1WriteBegin(object sender, EventArgs e)
        {
            if (video1AviWrite == null)
            {
                if (txt_video1path.Text.Equals(""))
                {
                    CustomMessageBox.Show("保存路径不能为空");
                    return;
                }
                if (cam1 == null)
                {
                    CustomMessageBox.Show("视屏1未打开，请先打开再录制视频");
                    return;
                }
                try
                {
                    video1AviWrite = new AviWriter();
                    video1AviWrite.avi_start(txt_video1path.Text + Path.DirectorySeparatorChar + "video1_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".avi");
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(Strings.ERROR + " " + ex, Strings.ERROR);
                    video1AviWrite = null;
                    return;
                }
                BTN_video1record.Enabled = false;
            }
            else
            {
                CustomMessageBox.Show("视频流1正在录制...");
            }
        }

        private void BTN_Video1WriteStop(object sender, EventArgs e)
        {
            try
            {
                if (video1AviWrite != null)
                {
                    video1AviWrite.avi_close();
                    BTN_video1record.Enabled = true;
                }
                  
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(Strings.ERROR + " " + ex, Strings.ERROR);
            }

            video1AviWrite = null;
        }

        private void BTN_BrowseVideo2PathChange(object sender, EventArgs e)
        {
            var video2file = new FolderBrowserDialog();
            if (video2file.ShowDialog() == DialogResult.OK)
            {
                txt_video2path.Text = video2file.SelectedPath;
                Settings.Instance["video2_pathfile"] = txt_video2path.Text;
            }
            else
            {
                txt_video2path.Text = "";
            }
        }

        private void BTN_Video2WriteBegin(object sender, EventArgs e)
        {
            if (video2AviWrite == null)
            {
                if (txt_video2path.Text.Equals(""))
                {
                    CustomMessageBox.Show("保存路径不能为空");
                    return;
                }
                if (cam2 == null)
                {
                    CustomMessageBox.Show("视屏2未打开，请先打开再录制视频");
                    return;
                }
                try
                {
                    video2AviWrite = new AviWriter();
                    video2AviWrite.avi_start(txt_video2path.Text + Path.DirectorySeparatorChar + "video2_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".avi");
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(Strings.ERROR + " " + ex, Strings.ERROR);
                    video2AviWrite = null;
                    return;
                }
                BTN_video2record.Enabled = false;
            }
            else
            {
                CustomMessageBox.Show("视频流2正在录制...");
            }
        }

        private void BTN_Video2WriteStop(object sender, EventArgs e)
        {
            try
            {
                if (video2AviWrite != null)
                {
                    video2AviWrite.avi_close();
                    BTN_video2record.Enabled = true;
                }

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(Strings.ERROR + " " + ex, Strings.ERROR);
            }

            video2AviWrite = null;
        }
        float video1imagefrequency = 5;
        uint video1imagenumber = 0;
        DateTime video1imagenextsave = DateTime.Now;
        private void cmb_video1imagefrequency(object sender, EventArgs e)
        {
            try
            {
                video1imagefrequency = float.Parse(comboBox_video1imageHZ.Text.Replace("hz", ""));
            }
            catch
            {
                CustomMessageBox.Show(Strings.InvalidUpdateRate, Strings.ERROR);
            }
        }
        float video2imagefrequency = 5;
        uint video2imagenumber = 0;
        DateTime video2imagenextsave = DateTime.Now;
        private void cmb_video2imagefrequency(object sender, EventArgs e)
        {
            try
            {
                video2imagefrequency = float.Parse(comboBox_video2imageHZ.Text.Replace("hz", ""));
            }
            catch
            {
                CustomMessageBox.Show(Strings.InvalidUpdateRate, Strings.ERROR);
            }
        }

        private void CMB_PortNameClick(object sender, EventArgs e)
        {
            cmb_portname.Items.Clear();
            cmb_portname.Items.AddRange(SerialPort.GetPortNames());
            cmb_portname.Items.Add("TCP");
            cmb_portname.Items.Add("UDP");
            cmb_portname.Items.Add("UDPCl");
        }
        // serialport
        internal static ICommsSerial comPort = new SerialPort();
        internal float extralportsendrate = 20;
        internal static CommandManager commandManager = new CommandManager();
        private void BTN_extralcom_Click(object sender, EventArgs e)
        {
            if (comPort.IsOpen)
            {
                timer4.Enabled = false;
                comPort.Close();
                BTN_extralcom.Text = Strings.Connect;
            }
            else
            {
                try
                {
                    switch (cmb_portname.Text)
                    {
                        case "TCP":
                            comPort = new TcpSerial();
                            cmb_baudrate.SelectedIndex = 0;
                            break;
                        case "UDP":
                            comPort = new UdpSerial();
                            cmb_baudrate.SelectedIndex = 0;
                            break;
                        case "UDPCl":
                            comPort = new UdpSerialConnect();
                            cmb_baudrate.SelectedIndex = 0;
                            break;
                        default:
                            comPort = new SerialPort();
                            comPort.PortName = cmb_portname.Text;
                            break;
                    }
                    
                }
                catch
                {
                    CustomMessageBox.Show(Strings.InvalidPortName);
                    return;
                }

                try
                {
                    comPort.BaudRate = int.Parse(cmb_baudrate.Text);
                }
                catch
                {
                    CustomMessageBox.Show(Strings.InvalidBaudRate);
                    return;
                }

                try
                {
                    extralportsendrate = float.Parse(cmb_sendrate.Text.Replace("hz", ""));
                }
                catch
                {
                    CustomMessageBox.Show(Strings.InvalidUpdateRate, Strings.ERROR);
                    return;
                }

                try
                {
                    comPort.Open();
                }
                catch (Exception ex)
                {
                    CustomMessageBox.Show(Strings.ErrorConnecting + "\n" + ex.ToString(), Strings.ERROR);
                    return;
                }
                commandManager.setPort(comPort);
                timer4.Enabled = true;
                BTN_extralcom.Text = Strings.Stop;
            }
        }

        private void Time4_Click(object sender, EventArgs e)
        {
            if (comPort.IsOpen)
            {
                extralPortRead();
                extralPortWrite();
            }
            else
            {
                if (BTN_extralcom.Text.Equals(Strings.Stop))
                {
                    BTN_extralcom.Text = Strings.Connect;
                }
            }
        }

        DateTime extralSendNext = DateTime.Now;
        int swarmControlState = 0;

        int i = 0;
        //private MAVState[] mavs;
        int j = 0;
        int k = 3;
        private void extralPortWrite()
        {
            if (DateTime.Now < extralSendNext)
            {
                return;
            }
            extralSendNext = DateTime.Now.AddMilliseconds(1000 / extralportsendrate);
            try
            {
                commandManager.SendMPState(SwarmInterface.Leader, swarmControlState, (int)getMavCounts());
                i = 0;
               foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        if ((i >=j) && (i < k))
                        {
                            commandManager.SendSwamState(SwarmInterface.Leader, mav, lastShape, (int)getMavCounts(), trackBar_swarmspace.Value);
                        }
                        i++;
                    }
                }
                j += 3;
                k += 3;
                if (j >= i)
                {
                    j = 0;
                    k = 3;
                }

                //byte[] testbyte = new byte[] { 0x01, 0x00, 0x00, 0x00, 0xD0, 0x08, 0x00, 0x00, 0x00, 0xC2, 0x01, 0x00, 0x23, 0x00, 0x23, 0x00, 0x00, 0x00, 0x00, 0x00 };
                //comPort.Write(testbyte, 0, testbyte.Length);
            }
            catch
            {

            }
        }
        private void extralPortRead()
        {
            try
            {
                if (comPort.BytesToRead == 0)
                {
                    return;
                }
                byte[] a = new byte[comPort.BytesToRead];
                comPort.Read(a, 0, a.Length);
                if (checkBox_rxdebug.Checked && groupbox_rxdebug.Visible && !showpause)
                {
                    rxdebug_datashow(a);
                }
                commandManager.ParseReadData(a);
            }
            catch
            {
            }
        }
        internal bool showpause = false;
        private void rxdebug_datashow(byte[] a)
        {
            //string mystr1 = DateTime.Now.ToString("yyyyMMddHHmmssfff") + " ";
            string mystr2 = Encoding.UTF8.GetString(a);
            string b = "";
            if (radioButton_hexshow.Checked)
            {
                b = ASCIIToHex(mystr2);
            }
            else
            {
                b = mystr2;
            }
            txt_rxdebug.Text += b + "\r\n";
        }
        //16进制字符串转换成ASCII字符串
        private string HexToASCII(string str)
        {
            try
            {
                string[] mystr1 = str.Trim().Split(' ');
                byte[] t = new byte[mystr1.Length];
                for (int i = 0; i < t.Length; i++)
                {
                    t[i] = Convert.ToByte(mystr1[i], 16);
                }
                return Encoding.UTF8.GetString(t);
            }
            catch (Exception ex)
            {
                MessageBox.Show("转换失败！" + ex.Message, "错误提示");
                return str;
            }
        }

        //字符串转换成16进制字符
        private string ASCIIToHex(string str)
        {
            try
            {
                byte[] a = Encoding.UTF8.GetBytes(str.Trim());
                string mystr1 = "";
                for (int i = 0; i < a.Length; i++)
                {
                    mystr1 += a[i].ToString("X2") + " ";
                }
                return mystr1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("转换失败！" + ex.Message, "错误提示");
                return str;
            }
        }
        private void check_rxdebug(object sender, EventArgs e)
        {
            if (checkBox_rxdebug.Checked)
            {
                groupbox_rxdebug.Visible = true;
            }
            else
            {
                groupbox_rxdebug.Visible = false;
            }
        }

        private void BTN_clearrxdebugdata(object sender, EventArgs e)
        {
            txt_rxdebug.Text = "";
        }

        private void BTN_dubugpause(object sender, EventArgs e)
        {
            if (showpause)
            {
                showpause = false;
                mybutton_datashowpause.Text = "暂停";
            }
            else
            {
                showpause = true;
                mybutton_datashowpause.Text = "开始";
            }
        }

        float udpSendfrequency = 20;
        DateTime udpSendnext = DateTime.Now;
        private void CMB_udpSendfrequency(object sender, EventArgs e)
        {
            try
            {
                udpSendfrequency = float.Parse(CMB_UdpSendRate.Text.Replace("hz", ""));
            }
            catch
            {
                CustomMessageBox.Show(Strings.InvalidUpdateRate, Strings.ERROR);
            }
        }
        VoiceIdentification voiceIdentification = null;
        string newRecognizedline = "";
        string oldRecognizedline = "";

        private void BTN_voiceIdentificationstart(object sender, EventArgs e)
        {
            if (voiceIdentification == null)
            {
                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        if (mav.cs.firmware == Firmwares.ArduPlane)
                        {
                            CustomMessageBox.Show("检测到有固定翼机型连入，语音识别功能不针对固定翼");
                            return;
                        }
                    }
                }
                voiceIdentification = new VoiceIdentification();
                voiceIdentification.init();
                voiceIdentification.start();
                lbl_SpeechRecognition.Text = "识别状态:开启";
            }
        }

        private void BTN_voiceIdentificationstop(object sender, EventArgs e)
        {
            if(voiceIdentification !=null)
            {
                voiceIdentification.stop();
                voiceIdentification = null;
                lbl_SpeechRecognition.Text = "识别状态:关闭";
            }
        }
        internal PointLatLng MouseDownStart;
        private void gMap_MouseDown(object sender, MouseEventArgs e)
        {
            MouseDownStart = gMap.FromLocalToLatLng(e.X, e.Y);
        }
        static public Object thisLock = new Object();

        private void gMap_MouseMove(object sender, MouseEventArgs e)
        {
            PointLatLng point = gMap.FromLocalToLatLng(e.X, e.Y);
            if (e.Button == MouseButtons.Left)
            {
                double latdif = MouseDownStart.Lat - point.Lat;
                double lngdif = MouseDownStart.Lng - point.Lng;
                try
                {
                    lock (thisLock)
                    {
                        gMap.Position = new PointLatLng(center.Position.Lat + latdif,
                            center.Position.Lng + lngdif);
                    }
                }
                catch (Exception ex)
                {
                    
                }
            }
        }
        void gMap_OnMapZoomChanged()
        {
            try
            {
                // Exception System.Runtime.InteropServices.SEHException: External component has thrown an exception.
                track_zoom.Value = (float)gMap.Zoom;
                center.Position = gMap.Position;
            }
            catch
            {
            }
        }
        void gMap_OnCurrentPositionChanged(PointLatLng point)
        {
            if (point.Lat > 90)
            {
                point.Lat = 90;
            }
            if (point.Lat < -90)
            {
                point.Lat = -90;
            }
            if (point.Lng > 180)
            {
                point.Lng = 180;
            }
            if (point.Lng < -180)
            {
                point.Lng = -180;
            }
            center.Position = point;
        }
        private void track_zoom_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (gMap.MaxZoom + 1 == (double)track_zoom.Value)
                {
                    gMap.Zoom = track_zoom.Value - .1;
                }
                else
                {
                    gMap.Zoom = track_zoom.Value;
                }
            }
            catch
            {
            }
        }

        private void showStartPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gMap.Position = new PointLatLng(start.Position.Lat, start.Position.Lng);
        }

        private void BTN_searchUpdate_Click(object sender, EventArgs e)
        {
            SearchInfoLoad();
        }
        internal bool searchsimulation = false;
        List<DateTime> nextUpdateHeadingDateTimes = new List<DateTime>();
        private void BTN_searchsimulation_Click(object sender, EventArgs e)
        {
            if (!searchsimulation)
            {
                if (FlightData.instance != null)
                {
                    if (FlightData.instance.searchareaoverlay.Polygons.Count < 1)
                    {
                        CustomMessageBox.Show("请添加搜索区域");
                        return;
                    }
                    if (FlightData.instance.virtualtargetoverlay.Markers.Count < 1)
                    {
                        CustomMessageBox.Show("请添加假想目标");
                        return;
                    }
                }
                else
                {
                    CustomMessageBox.Show("未知错误");
                    return;
                }

                searchsimulation = true;
                for (int i =0;i< virtualtargetoverlay.Markers.Count;i++)
                {
                    DateTime nextUpdateHeading = DateTime.Now.AddSeconds(headingchangeintervl);
                    nextUpdateHeadingDateTimes.Add(nextUpdateHeading);
                }
                BTN_searchsimulation.Text = "模拟结束";
            }
            else
            {
                searchsimulation = false;
                BTN_searchsimulation.Text = "模拟开始";
            }
        }
        // 内部编队  横一字搜索
        public class InternalFormation
        {
            public List<MAVState> mAVStates = new List<MAVState>();
            public List<PointLatLngAlt> grid = new List<PointLatLngAlt>();
            public MAVState Leader = null;
            public double distancebetweenmavs = 0;
            public double misssionalt;
            public double lastmisssionalt;
            // 构造函数
            public InternalFormation()
            {

            }
            public void setDistanceBetweenMavs(double distance)
            {
                distancebetweenmavs = distance;
            }
            // 横一字，从左下开始搜索，所以整体编队向左展开
            public void setInternalOffset()
            {
                if (mAVStates.Count == 0)
                {
                    return;
                }
                distancebetweenmavs = Math.Max(distancebetweenmavs,1);
                List<Vector3> offsets = new List<Vector3>();
                int a = 0;
                foreach (var mav in mAVStates)
                {
                    if (mav == Leader)
                    {
                        continue;
                    }
                    a++;
                    int sign = a % 2 == 0 ? 1 : -1;
                    Vector3 temp = new Vector3(-a * distancebetweenmavs, 0, sign * 20);
                    offsets.Add(temp);
                }
                if (offsets.Count<1)
                {
                    return;
                }
                // 效益矩阵
                int[,] cost_array = new int[offsets.Count, offsets.Count];

                //--------------------------------------------------------//
                int row = 0;
                foreach (var mav in mAVStates)
                {
                    if (mav == Leader)
                    {
                        continue;
                    }
                    foreach (var offset in offsets)
                    {
                        PointLatLngAlt target = new PointLatLngAlt(Leader.cs.lat, Leader.cs.lng, Leader.cs.alt);
                        PointLatLngAlt mavTarget = new PointLatLngAlt(instance.GetMavTarget(target, Leader.cs.yaw, offset));
                        var dist = mavTarget.GetDistance(mav.cs.Location);
                        cost_array[row, offsets.IndexOf(offset)] = (int)(100 * dist);
                    }
                    row++;
                }
                // 匈牙利算法
                int[] result = HungarianAlgorithm.FindAssignments(cost_array);
                //-----------------------------------------------------------------------//
                int index = 0;
                foreach (var mav in mAVStates)
                {
                    if (mav == Leader)
                    {
                        continue;
                    }
                    mav.internaloffset = new Vector3(offsets[result[index]]);
                    index++;
                }
            }
            public void setVirtualLeaderHeight(double height)
            {
                misssionalt = height;
                lastmisssionalt = height;
                if (grid.Count == 0)
                {
                    // bad should not hanppen
                    return;
                }
                foreach (var point in grid)
                {
                    point.Alt = height;
                }
            }
            public void setLeader()
            {
                if (mAVStates.Count == 0)
                {
                    // bad should not hanppen
                    return;
                }
                // 如果包含之前单编队的长机，则将其赋值给这个长机
                if (mAVStates.Contains(instance.SwarmInterface.Leader))
                {
                    Leader = instance.SwarmInterface.Leader;
                }
                else
                {
                    // 取第一个元素
                    Leader = mAVStates[0];
                }
                // 将长机写入生成的航点
                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        if (mav == Leader)
                        {
                            MissionCommandSend(port,mav,grid,-1);
                        }
                    }
                }
            }
            // setWPCurrent(0);
            public void MissionCommandSend(MAVLinkInterface port, MAVState mav, List<PointLatLngAlt> points,int cycles)
            {
                try
                {
                    port.sysidcurrent = mav.sysid;
                    port.compidcurrent = mav.compid;
                    port.giveComport = true;
                    Locationwp home = new Locationwp();
                    home.id = (ushort)MAVLink.MAV_CMD.WAYPOINT;
                    home.lat = mav.cs.HomeLocation.Lat;
                    home.lng = mav.cs.HomeLocation.Lng;
                    home.alt = (float)mav.cs.HomeLocation.Alt / CurrentState.multiplieralt; // use saved home
                    bool use_int = (mav.cs.capabilities & (uint)MAVLink.MAV_PROTOCOL_CAPABILITY.MISSION_INT) > 0;
                    ushort totalwpcountforupload = (ushort)(points.Count + 1 + 1); //+home+do jump
                    port.setWPTotal(totalwpcountforupload); // //+home+do jump
                                                            // upload from wp0
                    int a = 0;
                    try
                    {
                        var homeans = port.setWP(home, (ushort)a, MAVLink.MAV_FRAME.GLOBAL, 0, 1, use_int);
                        if (homeans != MAVLink.MAV_MISSION_RESULT.MAV_MISSION_ACCEPTED)
                        {
                            if (homeans != MAVLink.MAV_MISSION_RESULT.MAV_MISSION_INVALID_SEQUENCE)
                            {
                                CustomMessageBox.Show(Strings.ErrorRejectedByMAV, Strings.ERROR);
                                return;
                            }
                        }
                        a++;
                    }
                    catch (TimeoutException)
                    {
                        use_int = false;
                        // added here to prevent timeout errors
                        port.setWPTotal(totalwpcountforupload);
                        var homeans = port.setWP(home, (ushort)a, MAVLink.MAV_FRAME.GLOBAL, 0, 1, use_int);
                        if (homeans != MAVLink.MAV_MISSION_RESULT.MAV_MISSION_ACCEPTED)
                        {
                            if (homeans != MAVLink.MAV_MISSION_RESULT.MAV_MISSION_INVALID_SEQUENCE)
                            {
                                CustomMessageBox.Show(Strings.ErrorRejectedByMAV, Strings.ERROR);
                                return;
                            }
                        }
                        a++;
                    }
                    MAVLink.MAV_FRAME frame = MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT;
                    List<Locationwp> commands = new List<Locationwp>();
                    foreach (var point in points)
                    {
                        Locationwp temp1 = new Locationwp();
                        temp1.id = (ushort)MAVLink.MAV_CMD.WAYPOINT;
                        temp1.lat = point.Lat;
                        temp1.lng = point.Lng;
                        temp1.alt = (float)point.Alt / CurrentState.multiplieralt;
                        commands.Add(temp1);
                    }
                    Locationwp last = new Locationwp();
                    last.id = (ushort)MAVLink.MAV_CMD.DO_JUMP;
                    last.p1 = 1;
                    last.p2 = cycles;
                    last.lat = mav.cs.HomeLocation.Lat;
                    last.lng = mav.cs.HomeLocation.Lng;
                    last.alt = (float)mav.cs.HomeLocation.Alt / CurrentState.multiplieralt;
                    commands.Add(last);
                    for (a = 1; a <= commands.Count; a++)
                    {
                        var temp = commands[a - 1];
                        // handle current wp upload number
                        int uploadwpno = a;
                        // try send the wp
                        MAVLink.MAV_MISSION_RESULT ans = port.setWP(temp, (ushort)(uploadwpno), frame, 0, 1, use_int);

                        // we timed out while uploading wps/ command wasnt replaced/ command wasnt added
                        if (ans == MAVLink.MAV_MISSION_RESULT.MAV_MISSION_ERROR)
                        {
                            // resend for partial upload
                            port.setWPPartialUpdate((ushort)(uploadwpno), totalwpcountforupload);
                            // reupload this point.
                            ans = port.setWP(temp, (ushort)(uploadwpno), frame, 0, 1, use_int);
                        }

                        if (ans == MAVLink.MAV_MISSION_RESULT.MAV_MISSION_NO_SPACE)
                        {
                            return;
                        }
                        if (ans == MAVLink.MAV_MISSION_RESULT.MAV_MISSION_INVALID)
                        {
                            return;
                        }
                        if (ans == MAVLink.MAV_MISSION_RESULT.MAV_MISSION_INVALID_SEQUENCE)
                        {
                            // invalid sequence can only occur if we failed to see a response from the apm when we sent the request.
                            // or there is io lag and we send 2 mission_items and get 2 responces, one valid, one a ack of the second send

                            // the ans is received via mission_ack, so we dont know for certain what our current request is for. as we may have lost the mission_request

                            // get requested wp no - 1;
                            a = port.getRequestedWPNo() - 1;

                            continue;
                        }
                        if (ans != MAVLink.MAV_MISSION_RESULT.MAV_MISSION_ACCEPTED)
                        {
                            return;
                        }
                    }
                    port.setWPACK();

                    if (!mav.cs.mode.ToLower().Equals("auto"))
                    {
                        port.setMode(mav.sysid, mav.compid, "Auto");
                    }
                    else
                    {
                        // 本身为自动模式，则重新开始任务。
                        port.setWPCurrent(0);
                    }
                }
                catch (Exception ex)
                {
                    port.giveComport = false;
                    throw;
                }
                port.giveComport = false;
            }
            // 内部编队的僚机切到guided模式，长机模式不动，由外部确定
            public void FollowMavsGuidedEnable()
            {
                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        if (mAVStates.Contains(mav))
                        {
                            if (mav == Leader)
                            {
                                continue;
                            }
                            if (!mav.cs.mode.ToLower().Equals("guided"))
                            {
                                port.setMode(mav.sysid, mav.compid, "Guided");
                            }
                        }
                    }
                }
            }
            public bool IsInThismavs(byte id)
            {
                foreach (var mav in mAVStates)
                {
                    if (mav.sysid == id)
                    {
                        return true;
                    }
                }
                return false;
            }
            float location_path_proportion(PointLatLngAlt location, PointLatLngAlt point1, PointLatLngAlt point2)
            {
                System.Numerics.Vector2 vec1 = Avoidance.location_diff(point1, point2);
                System.Numerics.Vector2 vec2 = Avoidance.location_diff(point1, location);
                float dsquared = vec1.X * vec1.X + vec1.Y * vec1.Y;
                if (dsquared < 0.001f)
                {
                    // 两个点距离太近
                    return 1.0f;
                }
                return (vec1.X * vec2.X + vec1.Y* vec2.Y) / dsquared;
            }
        };
        Dictionary<GMapPolygon, InternalFormation> internalformations = new Dictionary<GMapPolygon, InternalFormation>();
        internal bool gotorearch = false;
        private void BTN_search_Click(object sender, EventArgs e)
        {
            if (!gotorearch)
            {
                if (searchareaoverlay.Polygons.Count < 1)
                {
                    CustomMessageBox.Show("请添加搜索区域");
                    return;
                }
                if (getMavCounts()<1)
                {
                    CustomMessageBox.Show("连接飞机数量为零");
                    return;
                }
                List<MAVState> mavs = new List<MAVState>();
                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        if (mav == SwarmInterface.Leader)
                        {
                            if (Formationoverallparameters.VirtualLeaderEnable)
                            {
                                continue;
                            }
                        }
                        mavs.Add(mav);
                    }
                }
                List<List<MAVState>> listArr = new List<List<MAVState>>();
                int tagetsize = mavs.Count;
                int size = searchareaoverlay.Polygons.Count;
                //获取被拆分的数组个数  
                int arrSize = tagetsize % size == 0 ? tagetsize / size : tagetsize / size + 1;
                for (int i = 0; i < size; i++)
                {
                    List<MAVState> sub = new List<MAVState>();
                    //把指定索引数据放入到list中  
                    for (int j = i * arrSize; j <= arrSize * (i + 1) - 1; j++)
                    {
                        if (j <= tagetsize - 1)
                        {
                            sub.Add(mavs[j]);
                        }
                    }
                    listArr.Add(sub);
                }
                double linedistance = arrSize > 1 ? searchRadius * 2 * arrSize : searchRadius * (Math.Max(arrSize - 1, 1));
                createWaypointListFromPolygons(searchaltset, overshot, linedistance);
                internalformations.Clear();
                foreach (var polygon in searchareaoverlay.Polygons)
                {
                    InternalFormation smallformation = new InternalFormation();
                    smallformation.mAVStates = listArr[searchareaoverlay.Polygons.IndexOf(polygon)];
                    smallformation.grid = grids[searchareaoverlay.Polygons.IndexOf(polygon)];
                    smallformation.setDistanceBetweenMavs(searchRadius);
                    smallformation.setVirtualLeaderHeight(searchaltset);
                    smallformation.setLeader();
                    smallformation.FollowMavsGuidedEnable();
                    smallformation.setInternalOffset();
                    internalformations[polygon] = smallformation;
                }
                if (threadrun)
                {
                    BUT_Start_Click(this,null);
                }
                gotorearch = true;
                BTN_search.Text = "停止搜寻";
            }
            else
            {
                routesOverlay.Markers.Clear();
                gotorearch = false;
                BTN_search.Text = "开始搜寻";
            }
        }
        internal double vtspeedms = 5;
        internal double headingchangeintervl = 30;
        private void BTN_virtualTargetParmSet_Click(object sender, EventArgs e)
        {
            vtspeedms = (double)virtualTargetSpeed.Value;
            headingchangeintervl = (double)headingChangeInterval.Value;
        }

        private void targetsFoundListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (targetsFound.SelectedItem == null)
            {
                return;
            }
            int index = targetsFound.SelectedIndex;
            //让地图显示此坐标
            if (index >= 0)
            {
                try
                {
                    PointLatLng showposition = new PointLatLng(targetsFoundListRecord[index].Lat, targetsFoundListRecord[index].Lng);
                    gMap.Position = showposition;
                }
                catch
                {

                }
            }
        }
        double searchaltset = 200;
        double overshot = 300;// 考虑固定翼转弯需要距离去收敛。在搜索区域外部增加一段距离。
        private void BTN_SearchAlt_Click(object sender, EventArgs e)
        {
            searchaltset = (double)numericUpDown_searchalt.Value;
            overshot = (double)numericUpDown_overshot.Value;
            //createWaypointListFromPolygons(searchaltset, overshot,100);
        }

        private void BTN_SetMavOffset(object sender, EventArgs e)
        {
            if (getMavCounts() < 1)
            {
                CustomMessageBox.Show("连接飞机数量为零");
                return;
            }
            if (SwarmInterface.Leader == null)
            {
                CustomMessageBox.Show("请先确定长机");
                return;
            }

            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == comboBox_mavsoffset.SelectedValue)
                    {
                        if (mav == SwarmInterface.Leader)
                        {
                            CustomMessageBox.Show("长机的x,y,z轴偏差为0，且不可改变，请选择其他飞机");
                            return;
                        }
                        else
                        {
                            try
                            {
                                SwarmInterface.setOffsets(mav, (double)numericUpDown_offsetx.Value, (double)numericUpDown_offsety.Value, (double)numericUpDown_offsetz.Value);
                                SwarmInterface.grid2offsets[mav].z = SwarmInterface.getOffsets(mav).z;
                            }
                            catch
                            {
                                // should not be here
                                CustomMessageBox.Show("设置失败，请重新输入");
                            }
                        }

                    }
                }
            }
            updateicons();
            grid2updateicons();
        }

        private void CMB_offsetClick(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == comboBox_mavsoffset.SelectedValue)
                    {
                        numericUpDown_offsetx.Value = (decimal)SwarmInterface.getOffsets(mav).x;
                        numericUpDown_offsety.Value = (decimal)SwarmInterface.getOffsets(mav).y;
                        numericUpDown_offsetz.Value = (decimal)SwarmInterface.getOffsets(mav).z;
                    }
                }
            }
        }
        bool HILmode = false;

        private void BTN_SimStart_Click(object sender, EventArgs e)
        {
            if (SwarmInterface.Leader == null)
            {
                CustomMessageBox.Show("请先设置长机");
                return;
            }
            if (!HILmode)
            {
                HILmode = true;
                BTN_SimStart.Text = "Simulation Link Stop";
                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        if (Formationoverallparameters.VirtualLeaderEnable && mav == SwarmInterface.Leader)
                        {
                            continue;
                        }
                        Hil hil = null ;
                        if (radioButton_xplane.Checked)
                        {
                            hil = new XPlane();
                        }
                        else if (radioButton_matlab.Checked)
                        {
                            hil = new MatLab();
                        }
                        else
                        {
                            hil = new XPlane();
                        }
                        hil.xplane10 = true;
                        try
                        {
                            hil.SetupSockets(HILLinks[mav].getRevPort(), HILLinks[mav].getSendPort(), HILLinks[mav].getIP());
                        }
                        catch
                        {
                            try { HILSims[mav].Shutdown(); }
                            catch { }
                            HILSims[mav] = null;
                            CustomMessageBox.Show("IP:"+ HILLinks[mav].getIP()+"Revport:"+ HILLinks[mav].getRevPort().ToString()+
                                "SendPort:"+ HILLinks[mav].getSendPort().ToString()+"绑定失败");
                        }                 
                        HILSims[mav] = hil;
                    }
                }
                timer5.Enabled = true;
            }
            else
            {
                HILmode = false;
                BTN_SimStart.Text = "Simulation Link Start";
                timer5.Enabled = false;
                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        if (HILSims.ContainsKey(mav))
                        {
                            if (HILSims[mav] != null)
                            {
                                HILSims[mav].Shutdown();
                                HILSims[mav] = null;
                            }
                        }
                    }
                }
            }
        }

        private void REVRoll_CheckedChange(object sender, EventArgs e)
        {
            if (!HILmode)
            {
                return;
            }
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (HILSims.ContainsKey(mav) && HILSims[mav] != null)
                    {
                        HILSims[mav].setRevRoll(checkBox_REVroll.Checked == true ? -1 : 1);
                    }
                }
            }
        }

        private void REVPitch_CheckedChange(object sender, EventArgs e)
        {
            if (!HILmode)
            {
                return;
            }
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (HILSims.ContainsKey(mav)&& HILSims[mav] != null)
                    {
                        HILSims[mav].setRevPitch(checkBox_REVpitch.Checked == true ? -1 : 1);
                    }
                }
            }
        }

        private void REVYaw_CheckedChange(object sender, EventArgs e)
        {
            if (!HILmode)
            {
                return;
            }
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (HILSims.ContainsKey(mav) && HILSims[mav] != null)
                    {
                        HILSims[mav].setRevYaw(checkBox_REVyaw.Checked == true ? -1 : 1);
                    }
                }
            }
        }

        private void timer5_Tick(object sender, EventArgs e)
        {
            if (!HILmode)
            {
                return;
            }
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (HILSims.ContainsKey(mav) && HILSims[mav] != null)
                    {
                        HILSims[mav].GetFromSim();
                        HILSims[mav].SendToAP(port, mav);
                        HILSims[mav].SendToSim();
                    }
                }
            }
        }

        private void Btn_Stop_Click(object sender, EventArgs e)
        {
            threadrun = false;
            BUT_Start.Text = Strings.Start;
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == SwarmInterface.Leader)
                    {
                        continue;
                    }
                    if (mav.cs.mode.ToLower().Equals("guided"))
                    {
                        port.setPositionTargetGlobalInt(mav.sysid, mav.compid, true, false, false, false, MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT_INT,
                                mav.cs.lat, mav.cs.lng, mav.cs.alt, 0, 0, 0, 0, 0);
                        port.doCommand(mav.sysid, mav.compid, MAVLink.MAV_CMD.CONDITION_YAW, SwarmInterface.Leader.cs.yaw,
                        100.0f, 0, 0, 0, 0, 0, false);
                    }
                }
            }
        }

        private void Btn_DoExtralCommand_Click(object sender, EventArgs e)
        {
            DateTime lastReadTime = commandManager.GetLastReadTime();
            // 3秒没有收到数据，显示为0，字体红色
            if (DateTime.Now > lastReadTime.AddSeconds(3))
            {
                CustomMessageBox.Show("没有与指控系统建立链接，无法执行命令");
                return;
            }
            if (SwarmInterface.Leader == null)
            {
                CustomMessageBox.Show("没有设置长机，无法执行命令");
                return;
            }
            PointLatLngAlt intercept = commandManager.GetInterceptPoint();
            double dist = SwarmInterface.Leader.cs.Location.GetDistance(intercept);
            if (dist < 1000)
            {
                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        if (mav == SwarmInterface.Leader && mav.cs.mode.ToLower().Equals("guided"))
                        {
                            port.setPositionTargetGlobalInt(mav.sysid, mav.compid, true, false, false, false, MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT_INT, intercept.Lat, intercept.Lng, intercept.Alt, 0, 0, 0, 0, 0);
                        }
                    }
                }
            }
            else
            {
                if (CustomMessageBox.Show("距离大于1000米，确认执行?", "锁定?", MessageBoxButtons.YesNo) != (int)DialogResult.Yes)
                    return;
                foreach (var port in MainV2.Comports)
                {
                    foreach (var mav in port.MAVlist)
                    {
                        if (mav == SwarmInterface.Leader && mav.cs.mode.ToLower().Equals("guided"))
                        {
                            port.setPositionTargetGlobalInt(mav.sysid, mav.compid, true, false, false, false, MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT_INT, intercept.Lat, intercept.Lng, intercept.Alt, 0, 0, 0, 0, 0);
                        }
                    }
                }
            }
        }

        private void Btn_DoCancelCommandClick(object sender, EventArgs e)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (mav == SwarmInterface.Leader && mav.cs.mode.ToLower().Equals("guided"))
                    {
                        port.setPositionTargetGlobalInt(mav.sysid, mav.compid, true, false, false, false, MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT_INT, mav.cs.lat, mav.cs.lng, mav.cs.alt, 0, 0, 0, 0, 0);
                        port.doCommand(mav.sysid, mav.compid, MAVLink.MAV_CMD.CONDITION_YAW, mav.cs.yaw,100.0f, 0, 0, 0, 0, 0, false);
                    }
                }
            }
        }

        private bool isInMirroringUdpData = false;

        private void btn_mirrorUdpForward_Click(object sender, EventArgs e)
        {
            if (!isInMirroringUdpData)
            {
                startMirrorUdp();
            }
            else
            {
                stopMirrorUdp();
            }
        }

        private void startMirrorUdp()
        {
            btn_mirrorUdpForward.Enabled = false;
            btn_mirrorUdpForward.Text = "连接中";
            List<string> udpPorts = new List<string>();
            if (rb_autoSetMirrorUdp.Checked)
            {
                textBox_mirrorUdpPortInfo.Text = "";
                int begin = 0;
                int interval = 0;
                try
                {
                    begin = Convert.ToInt32(textBox_BeginMirrorUdpPort.Text);
                    interval = Convert.ToInt32(textBox_intervalMirrorUdpPort.Text);
                }
                catch
                {
                    CustomMessageBox.Show("初始端口或者间隔设置不合法");
                    return;
                }
                uint mavcounts = getMavCounts();
                udpPorts.Clear();
                for (uint i = 0; i < mavcounts; i++)
                {
                    int portNumber = begin + (int)i * interval;
                    if (portNumber < 0 || portNumber > 65535)
                    {
                        CustomMessageBox.Show("端口号不合法，必须在0-65535之间");
                        return;
                    }
                    udpPorts.Add(portNumber.ToString());
                    textBox_mirrorUdpPortInfo.Text += portNumber.ToString() + " ";
                }
            }
            if (rb_mannalSetMirrorUdp.Checked)
            {
                udpPorts.Clear();
                string[] items = textBox_mirrorUdpPortInfo.Text.Split(new[] { '\t', ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var iter in items)
                {
                    udpPorts.Add(iter.Trim());
                }
                foreach (var port in udpPorts)
                {
                    int portNumber = -1;
                    try
                    {
                        portNumber = Convert.ToInt32(port);
                    }
                    catch
                    {
                        CustomMessageBox.Show("端口号不合法，必须在0-65535之间,并且用空格隔开");
                        return;
                    }
                    textBox_mirrorUdpPortInfo.Text += portNumber.ToString() + " ";
                }
            }
            int index = 0;
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    try
                    {
                        port.UdpMirrorStream = new UdpSerial();
                    }
                    catch
                    {
                        CustomMessageBox.Show(Strings.InvalidPortName);
                        return;
                    }
                    string destPort = udpPorts[index];
                    try
                    {
                        port.UdpMirrorStream.Start(destPort);
                    }
                    catch
                    {
                        CustomMessageBox.Show("链接错误");
                        return;
                    }
                    index++;
                }
            }

            isInMirroringUdpData = true;
            btn_mirrorUdpForward.Text = "停止";
            btn_mirrorUdpForward.Enabled = true;
        }

        private void stopMirrorUdp()
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (port.UdpMirrorStream == null)
                    {
                        continue;
                    }
                    port.UdpMirrorStream.Close();
                    port.UdpMirrorStream = null;
                }
            }
            isInMirroringUdpData = false;
            btn_mirrorUdpForward.Text = "转发";
            btn_mirrorUdpForward.Enabled = true;
        }

        private void show_sim_data()
        {
            if (!HILmode)
            {
                return;
            }
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    if (HILSims.ContainsKey(mav))
                    {
                        if (HILLinks[mav].getShowSimDataChecked())
                        {
                            lbl_lat.Text = HILSims[mav].sitl_data.latitude.ToString("f6")+"deg";
                            lbl_lng.Text = HILSims[mav].sitl_data.longitude.ToString("f6") + "deg";
                            lbl_alt.Text = HILSims[mav].sitl_data.altitude.ToString("f2")+"m";
                            lbl_vx.Text = HILSims[mav].sitl_data.speedN.ToString("f2")+"m/s";
                            lbl_vy.Text = HILSims[mav].sitl_data.speedE.ToString("f2")+"m/s";
                            lbl_vz.Text = HILSims[mav].sitl_data.speedD.ToString("f2")+"m/s";
                            lbl_gx.Text = HILSims[mav].sitl_data.rollRate.ToString("f2")+"deg/s";
                            lbl_gy.Text = HILSims[mav].sitl_data.pitchRate.ToString("f2") + "deg/s";
                            lbl_gz.Text = HILSims[mav].sitl_data.yawRate.ToString("f2") + "deg/s";
                            lbl_ax.Text = HILSims[mav].sitl_data.xAccel.ToString("f2") + "m/s/s";
                            lbl_ay.Text = HILSims[mav].sitl_data.yAccel.ToString("f2") + "m/s/s";
                            lbl_az.Text = HILSims[mav].sitl_data.zAccel.ToString("f2") + "m/s/s";
                            lbl_pitch.Text = HILSims[mav].sitl_data.pitchDeg.ToString("f2") + "deg";
                            lbl_roll.Text = HILSims[mav].sitl_data.rollDeg.ToString("f2") + "deg";
                            lbl_yaw.Text = HILSims[mav].sitl_data.yawDeg.ToString("f2") + "deg";
                            lbl_heading.Text = HILSims[mav].sitl_data.heading.ToString("f2") + "deg";
                            lbl_servo1.Text = HILSims[mav].roll_out.ToString("f2");
                            lbl_servo2.Text = HILSims[mav].pitch_out.ToString("f2");
                            lbl_servo3.Text = HILSims[mav].pitch_out.ToString("f2");
                            lbl_servo4.Text = HILSims[mav].throttle_out.ToString("f2");
                            return;
                        }
                    }
                }
            }
        }
        public void voice_identification_show(string str)
        {
            newRecognizedline = str;
        }

        private void setUdpMirrorTransferEnable(bool enable)
        {
            foreach (var port in MainV2.Comports)
            {
                foreach (var mav in port.MAVlist)
                {
                    port.UdpMirrorStreamWrite = enable;
                }
            }
        }
    }
}