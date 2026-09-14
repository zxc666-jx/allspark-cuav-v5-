namespace MissionPlanner.Swarm
{
    partial class FormationControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormationControl));
            this.CMB_mavs = new System.Windows.Forms.ComboBox();
            this.bindingSource_1 = new System.Windows.Forms.BindingSource(this.components);
            this.BUT_Start = new MissionPlanner.Controls.MyButton();
            this.BUT_leader = new MissionPlanner.Controls.MyButton();
            this.BUT_Land = new MissionPlanner.Controls.MyButton();
            this.BUT_Takeoff = new MissionPlanner.Controls.MyButton();
            this.BUT_Disarm = new MissionPlanner.Controls.MyButton();
            this.BUT_Arm = new MissionPlanner.Controls.MyButton();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox24 = new System.Windows.Forms.GroupBox();
            this.numericUpDown_offsety = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_offsetx = new System.Windows.Forms.NumericUpDown();
            this.myButton26 = new MissionPlanner.Controls.MyButton();
            this.comboBox_mavsoffset = new System.Windows.Forms.ComboBox();
            this.bindingSource_4 = new System.Windows.Forms.BindingSource(this.components);
            this.numericUpDown_offsetz = new System.Windows.Forms.NumericUpDown();
            this.grid1 = new MissionPlanner.Swarm.Grid();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox14 = new System.Windows.Forms.GroupBox();
            this.myButton16 = new MissionPlanner.Controls.MyButton();
            this.myButton15 = new MissionPlanner.Controls.MyButton();
            this.myButton9 = new MissionPlanner.Controls.MyButton();
            this.myButton4 = new MissionPlanner.Controls.MyButton();
            this.myButton11 = new MissionPlanner.Controls.MyButton();
            this.myButton12 = new MissionPlanner.Controls.MyButton();
            this.myButton14 = new MissionPlanner.Controls.MyButton();
            this.myButton5 = new MissionPlanner.Controls.MyButton();
            this.myButton13 = new MissionPlanner.Controls.MyButton();
            this.myButton6 = new MissionPlanner.Controls.MyButton();
            this.myButton10 = new MissionPlanner.Controls.MyButton();
            this.myButton7 = new MissionPlanner.Controls.MyButton();
            this.myButton8 = new MissionPlanner.Controls.MyButton();
            this.label56 = new System.Windows.Forms.Label();
            this.checkBox_AllAdvanceParam = new System.Windows.Forms.CheckBox();
            this.CMB_pid = new System.Windows.Forms.ComboBox();
            this.bindingSource_2 = new System.Windows.Forms.BindingSource(this.components);
            this.CHK_lockallmav = new System.Windows.Forms.CheckBox();
            this.panel_PID_param = new System.Windows.Forms.Panel();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.virtualleaderenable = new System.Windows.Forms.NumericUpDown();
            this.label57 = new System.Windows.Forms.Label();
            this.min_approach_times = new System.Windows.Forms.NumericUpDown();
            this.label55 = new System.Windows.Forms.Label();
            this.fail_action = new System.Windows.Forms.NumericUpDown();
            this.label85 = new System.Windows.Forms.Label();
            this.avoidance_enable = new System.Windows.Forms.NumericUpDown();
            this.label54 = new System.Windows.Forms.Label();
            this.avoidance_horizontal_m = new System.Windows.Forms.NumericUpDown();
            this.avoidance_vertical_m = new System.Windows.Forms.NumericUpDown();
            this.label53 = new System.Windows.Forms.Label();
            this.label52 = new System.Windows.Forms.Label();
            this.fail_alt_min_m = new System.Windows.Forms.NumericUpDown();
            this.label51 = new System.Windows.Forms.Label();
            this.warn_distance_z_m = new System.Windows.Forms.NumericUpDown();
            this.label50 = new System.Windows.Forms.Label();
            this.warn_distance_xy_m = new System.Windows.Forms.NumericUpDown();
            this.fail_distance_xy_m = new System.Windows.Forms.NumericUpDown();
            this.fail_distance_z_m = new System.Windows.Forms.NumericUpDown();
            this.warn_time_horizon_s = new System.Windows.Forms.NumericUpDown();
            this.label49 = new System.Windows.Forms.Label();
            this.label48 = new System.Windows.Forms.Label();
            this.label47 = new System.Windows.Forms.Label();
            this.label46 = new System.Windows.Forms.Label();
            this.fail_time_horizon_s = new System.Windows.Forms.NumericUpDown();
            this.label45 = new System.Windows.Forms.Label();
            this.label44 = new System.Windows.Forms.Label();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.AP_oil_power = new System.Windows.Forms.NumericUpDown();
            this.label101 = new System.Windows.Forms.Label();
            this.copter_takeoff_m = new System.Windows.Forms.NumericUpDown();
            this.label43 = new System.Windows.Forms.Label();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.speed_FILT = new System.Windows.Forms.NumericUpDown();
            this.speed_IMAX = new System.Windows.Forms.NumericUpDown();
            this.speed_D = new System.Windows.Forms.NumericUpDown();
            this.speed_I = new System.Windows.Forms.NumericUpDown();
            this.speed_P = new System.Windows.Forms.NumericUpDown();
            this.label35 = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.label37 = new System.Windows.Forms.Label();
            this.label38 = new System.Windows.Forms.Label();
            this.label39 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.stall_protect = new System.Windows.Forms.NumericUpDown();
            this.label108 = new System.Windows.Forms.Label();
            this.pre_distance_to_lose_speed = new System.Windows.Forms.NumericUpDown();
            this.label100 = new System.Windows.Forms.Label();
            this.min_target_speed = new System.Windows.Forms.NumericUpDown();
            this.label99 = new System.Windows.Forms.Label();
            this.thrust_min = new System.Windows.Forms.NumericUpDown();
            this.label98 = new System.Windows.Forms.Label();
            this.thrust_trim = new System.Windows.Forms.NumericUpDown();
            this.label97 = new System.Windows.Forms.Label();
            this.delta_speed_scale = new System.Windows.Forms.NumericUpDown();
            this.dist_integral_separation_m = new System.Windows.Forms.NumericUpDown();
            this.label42 = new System.Windows.Forms.Label();
            this.label41 = new System.Windows.Forms.Label();
            this.max_dist_to_delta_speed_m = new System.Windows.Forms.NumericUpDown();
            this.label40 = new System.Windows.Forms.Label();
            this.yaw_to_roll_max_angle_deg = new System.Windows.Forms.NumericUpDown();
            this.label26 = new System.Windows.Forms.Label();
            this.roll_min_disttotarget_m = new System.Windows.Forms.NumericUpDown();
            this.label25 = new System.Windows.Forms.Label();
            this.roll_high_dist_boundary_m = new System.Windows.Forms.NumericUpDown();
            this.label24 = new System.Windows.Forms.Label();
            this.roll_low_dist_boundary_m = new System.Windows.Forms.NumericUpDown();
            this.label23 = new System.Windows.Forms.Label();
            this.yaw_distance_boundary_m = new System.Windows.Forms.NumericUpDown();
            this.label22 = new System.Windows.Forms.Label();
            this.target_leader_dist_m = new System.Windows.Forms.NumericUpDown();
            this.label21 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.target_trailer_scale = new System.Windows.Forms.NumericUpDown();
            this.label19 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.BUT_writePIDS = new MissionPlanner.Controls.MyButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.thrust_FILT = new System.Windows.Forms.NumericUpDown();
            this.thrust_IMAX = new System.Windows.Forms.NumericUpDown();
            this.thrust_D = new System.Windows.Forms.NumericUpDown();
            this.thrust_I = new System.Windows.Forms.NumericUpDown();
            this.thrust_P = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.yaw_FILT = new System.Windows.Forms.NumericUpDown();
            this.yaw_IMAX = new System.Windows.Forms.NumericUpDown();
            this.yaw_D = new System.Windows.Forms.NumericUpDown();
            this.yaw_I = new System.Windows.Forms.NumericUpDown();
            this.yaw_P = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.pitch_FILT = new System.Windows.Forms.NumericUpDown();
            this.pitch_IMAX = new System.Windows.Forms.NumericUpDown();
            this.pitch_D = new System.Windows.Forms.NumericUpDown();
            this.pitch_I = new System.Windows.Forms.NumericUpDown();
            this.pitch_P = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox25 = new System.Windows.Forms.GroupBox();
            this.roll_FILT = new System.Windows.Forms.NumericUpDown();
            this.roll_IMAX = new System.Windows.Forms.NumericUpDown();
            this.roll_D = new System.Windows.Forms.NumericUpDown();
            this.roll_I = new System.Windows.Forms.NumericUpDown();
            this.roll_P = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label88 = new System.Windows.Forms.Label();
            this.label90 = new System.Windows.Forms.Label();
            this.label91 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.splitContainer6 = new System.Windows.Forms.SplitContainer();
            this.comboBox_chartshow = new System.Windows.Forms.ComboBox();
            this.variable_check = new System.Windows.Forms.FlowLayoutPanel();
            this.zg1 = new ZedGraph.ZedGraphControl();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.groupBox13 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label_maybecollisiontimes = new System.Windows.Forms.Label();
            this.label83 = new System.Windows.Forms.Label();
            this.label79 = new System.Windows.Forms.Label();
            this.label_totalavoidancecraftid = new System.Windows.Forms.Label();
            this.label82 = new System.Windows.Forms.Label();
            this.label_avoidancecraftcounts = new System.Windows.Forms.Label();
            this.groupBox12 = new System.Windows.Forms.GroupBox();
            this.label_avoidancetargetalt2 = new System.Windows.Forms.Label();
            this.label_avoidancecraftid2 = new System.Windows.Forms.Label();
            this.label_obstacleid2 = new System.Windows.Forms.Label();
            this.label_craftdisttoobstacle2 = new System.Windows.Forms.Label();
            this.label_avoidanceeastoffset2 = new System.Windows.Forms.Label();
            this.label_avoidancenorthoffset2 = new System.Windows.Forms.Label();
            this.label73 = new System.Windows.Forms.Label();
            this.label74 = new System.Windows.Forms.Label();
            this.label75 = new System.Windows.Forms.Label();
            this.label76 = new System.Windows.Forms.Label();
            this.label77 = new System.Windows.Forms.Label();
            this.label78 = new System.Windows.Forms.Label();
            this.groupBox11 = new System.Windows.Forms.GroupBox();
            this.label_avoidancetargetalt1 = new System.Windows.Forms.Label();
            this.label_avoidancecraftid1 = new System.Windows.Forms.Label();
            this.label_obstacleid1 = new System.Windows.Forms.Label();
            this.label_craftdisttoobstacle1 = new System.Windows.Forms.Label();
            this.label_avoidanceeastoffset1 = new System.Windows.Forms.Label();
            this.label_avoidancenorthoffset1 = new System.Windows.Forms.Label();
            this.label61 = new System.Windows.Forms.Label();
            this.label62 = new System.Windows.Forms.Label();
            this.label63 = new System.Windows.Forms.Label();
            this.label64 = new System.Windows.Forms.Label();
            this.label65 = new System.Windows.Forms.Label();
            this.label66 = new System.Windows.Forms.Label();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.label_lowaltcount = new System.Windows.Forms.Label();
            this.label_armcount = new System.Windows.Forms.Label();
            this.label_heathlinkcount = new System.Windows.Forms.Label();
            this.label_guidedcount = new System.Windows.Forms.Label();
            this.label_connectcount = new System.Windows.Forms.Label();
            this.label_lowspeedcount = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.label33 = new System.Windows.Forms.Label();
            this.label32 = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.listBox_formationShape = new System.Windows.Forms.ListBox();
            this.myButton3 = new MissionPlanner.Controls.MyButton();
            this.label28 = new System.Windows.Forms.Label();
            this.comboBox_formation_select = new System.Windows.Forms.ComboBox();
            this.myButton2 = new MissionPlanner.Controls.MyButton();
            this.myButton1 = new MissionPlanner.Controls.MyButton();
            this.textBox_filepath = new System.Windows.Forms.TextBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.checkBox_VLmode = new System.Windows.Forms.CheckBox();
            this.myButton18 = new MissionPlanner.Controls.MyButton();
            this.myButton_Transformation = new MissionPlanner.Controls.MyButton();
            this.label27 = new System.Windows.Forms.Label();
            this.myButton_swarmspace = new MissionPlanner.Controls.MyButton();
            this.textBox_swarmspace = new System.Windows.Forms.TextBox();
            this.trackBar_swarmspace = new System.Windows.Forms.TrackBar();
            this.grid2 = new MissionPlanner.Swarm.Grid();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.splitContainer7 = new System.Windows.Forms.SplitContainer();
            this.groupBox16 = new System.Windows.Forms.GroupBox();
            this.CMB_video2resolutions = new System.Windows.Forms.ComboBox();
            this.BUT_video2stop = new MissionPlanner.Controls.MyButton();
            this.BUT_video2start = new MissionPlanner.Controls.MyButton();
            this.CMB_video2sources = new System.Windows.Forms.ComboBox();
            this.label59 = new System.Windows.Forms.Label();
            this.label60 = new System.Windows.Forms.Label();
            this.groupBox15 = new System.Windows.Forms.GroupBox();
            this.CMB_video1resolutions = new System.Windows.Forms.ComboBox();
            this.BUT_video1stop = new MissionPlanner.Controls.MyButton();
            this.BUT_video1start = new MissionPlanner.Controls.MyButton();
            this.CMB_video1sources = new System.Windows.Forms.ComboBox();
            this.label58 = new System.Windows.Forms.Label();
            this.label92 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.userVideo1HudShow = new MissionPlanner.UserVideoHudShow();
            this.userVideo2HudShow = new MissionPlanner.UserVideoHudShow();
            this.tabPage7 = new System.Windows.Forms.TabPage();
            this.splitContainer8 = new System.Windows.Forms.SplitContainer();
            this.myButton17 = new MissionPlanner.Controls.MyButton();
            this.BTN_Show3dMap = new MissionPlanner.Controls.MyButton();
            this.CMB_3DMAP = new System.Windows.Forms.ComboBox();
            this.label67 = new System.Windows.Forms.Label();
            this.tabPage_voiceIdentification = new System.Windows.Forms.TabPage();
            this.groupBox22 = new System.Windows.Forms.GroupBox();
            this.label87 = new System.Windows.Forms.Label();
            this.lbl_fourthgrammer = new System.Windows.Forms.Label();
            this.lbl_thirdgrammer = new System.Windows.Forms.Label();
            this.lbl_secondgrammer = new System.Windows.Forms.Label();
            this.lbl_firstgrammer = new System.Windows.Forms.Label();
            this.lbl_SpeechRecognition = new System.Windows.Forms.Label();
            this.groupBox21 = new System.Windows.Forms.GroupBox();
            this.txt_Identificateresult = new System.Windows.Forms.TextBox();
            this.myButton25 = new MissionPlanner.Controls.MyButton();
            this.myButton23 = new MissionPlanner.Controls.MyButton();
            this.tabPage_targetserach = new System.Windows.Forms.TabPage();
            this.splitContainer9 = new System.Windows.Forms.SplitContainer();
            this.splitContainer10 = new System.Windows.Forms.SplitContainer();
            this.numericUpDown_overshot = new System.Windows.Forms.NumericUpDown();
            this.label96 = new System.Windows.Forms.Label();
            this.BTN_SearchAlt = new MissionPlanner.Controls.MyButton();
            this.numericUpDown_searchalt = new System.Windows.Forms.NumericUpDown();
            this.label95 = new System.Windows.Forms.Label();
            this.groupBox23 = new System.Windows.Forms.GroupBox();
            this.BTN_virtualTargetParmSet = new MissionPlanner.Controls.MyButton();
            this.headingChangeInterval = new System.Windows.Forms.NumericUpDown();
            this.label93 = new System.Windows.Forms.Label();
            this.speed = new System.Windows.Forms.Label();
            this.virtualTargetSpeed = new System.Windows.Forms.NumericUpDown();
            this.radioButton_targetsearch = new System.Windows.Forms.RadioButton();
            this.radioButton_tragettrack = new System.Windows.Forms.RadioButton();
            this.BTN_searchsimulation = new MissionPlanner.Controls.MyButton();
            this.BTN_searchUpdate = new MissionPlanner.Controls.MyButton();
            this.BTN_search = new MissionPlanner.Controls.MyButton();
            this.lbl_targetfoundcounts = new System.Windows.Forms.Label();
            this.label94 = new System.Windows.Forms.Label();
            this.lbl_serachareacounts = new System.Windows.Forms.Label();
            this.label89 = new System.Windows.Forms.Label();
            this.targetsFound = new System.Windows.Forms.ListBox();
            this.gMap = new MissionPlanner.Controls.myGMAP();
            this.GmapcontextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showStartPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.track_zoom = new MissionPlanner.Controls.MyTrackBar();
            this.tabPage_HIL = new System.Windows.Forms.TabPage();
            this.splitContainer11 = new System.Windows.Forms.SplitContainer();
            this.checkBox_REVthr = new System.Windows.Forms.CheckBox();
            this.checkBox_REVyaw = new System.Windows.Forms.CheckBox();
            this.checkBox_REVpitch = new System.Windows.Forms.CheckBox();
            this.checkBox_REVroll = new System.Windows.Forms.CheckBox();
            this.radioButton_jsbsim = new System.Windows.Forms.RadioButton();
            this.radioButton_aersimrc = new System.Windows.Forms.RadioButton();
            this.radioButton_flight = new System.Windows.Forms.RadioButton();
            this.radioButton_matlab = new System.Windows.Forms.RadioButton();
            this.radioButton_xplane = new System.Windows.Forms.RadioButton();
            this.BTN_SimStart = new MissionPlanner.Controls.MyButton();
            this.splitContainer12 = new System.Windows.Forms.SplitContainer();
            this.flowLayoutPanel_udplinks = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox29 = new System.Windows.Forms.GroupBox();
            this.lbl_servo4 = new System.Windows.Forms.Label();
            this.lbl_servo3 = new System.Windows.Forms.Label();
            this.lbl_servo2 = new System.Windows.Forms.Label();
            this.lbl_servo1 = new System.Windows.Forms.Label();
            this.label138 = new System.Windows.Forms.Label();
            this.label139 = new System.Windows.Forms.Label();
            this.label140 = new System.Windows.Forms.Label();
            this.label141 = new System.Windows.Forms.Label();
            this.groupBox28 = new System.Windows.Forms.GroupBox();
            this.lbl_heading = new System.Windows.Forms.Label();
            this.lbl_yaw = new System.Windows.Forms.Label();
            this.lbl_roll = new System.Windows.Forms.Label();
            this.lbl_pitch = new System.Windows.Forms.Label();
            this.label134 = new System.Windows.Forms.Label();
            this.label135 = new System.Windows.Forms.Label();
            this.label136 = new System.Windows.Forms.Label();
            this.label137 = new System.Windows.Forms.Label();
            this.groupBox27 = new System.Windows.Forms.GroupBox();
            this.lbl_az = new System.Windows.Forms.Label();
            this.lbl_ay = new System.Windows.Forms.Label();
            this.lbl_ax = new System.Windows.Forms.Label();
            this.lbl_gz = new System.Windows.Forms.Label();
            this.lbl_gy = new System.Windows.Forms.Label();
            this.lbl_gx = new System.Windows.Forms.Label();
            this.label120 = new System.Windows.Forms.Label();
            this.label121 = new System.Windows.Forms.Label();
            this.label122 = new System.Windows.Forms.Label();
            this.label123 = new System.Windows.Forms.Label();
            this.label124 = new System.Windows.Forms.Label();
            this.label125 = new System.Windows.Forms.Label();
            this.groupBox26 = new System.Windows.Forms.GroupBox();
            this.lbl_vz = new System.Windows.Forms.Label();
            this.lbl_vy = new System.Windows.Forms.Label();
            this.lbl_vx = new System.Windows.Forms.Label();
            this.lbl_alt = new System.Windows.Forms.Label();
            this.lbl_lng = new System.Windows.Forms.Label();
            this.lbl_lat = new System.Windows.Forms.Label();
            this.label107 = new System.Windows.Forms.Label();
            this.label106 = new System.Windows.Forms.Label();
            this.label105 = new System.Windows.Forms.Label();
            this.label104 = new System.Windows.Forms.Label();
            this.label103 = new System.Windows.Forms.Label();
            this.label102 = new System.Windows.Forms.Label();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.groupBox30 = new System.Windows.Forms.GroupBox();
            this.textBox_mirrorUdpPortInfo = new System.Windows.Forms.TextBox();
            this.rb_mannalSetMirrorUdp = new System.Windows.Forms.RadioButton();
            this.textBox_intervalMirrorUdpPort = new System.Windows.Forms.TextBox();
            this.label110 = new System.Windows.Forms.Label();
            this.textBox_BeginMirrorUdpPort = new System.Windows.Forms.TextBox();
            this.label109 = new System.Windows.Forms.Label();
            this.btn_mirrorUdpForward = new MissionPlanner.Controls.MyButton();
            this.rb_autoSetMirrorUdp = new System.Windows.Forms.RadioButton();
            this.groupBox20 = new System.Windows.Forms.GroupBox();
            this.CMB_UdpSendRate = new System.Windows.Forms.ComboBox();
            this.label84 = new System.Windows.Forms.Label();
            this.UDP_send = new MissionPlanner.Controls.MyButton();
            this.txt_remoteip = new System.Windows.Forms.TextBox();
            this.txt_remoteport = new System.Windows.Forms.TextBox();
            this.label86 = new System.Windows.Forms.Label();
            this.groupbox_rxdebug = new System.Windows.Forms.GroupBox();
            this.mybutton_datashowpause = new MissionPlanner.Controls.MyButton();
            this.myButton20 = new MissionPlanner.Controls.MyButton();
            this.radioButton_hexshow = new System.Windows.Forms.RadioButton();
            this.radioButton_charshow = new System.Windows.Forms.RadioButton();
            this.txt_rxdebug = new System.Windows.Forms.TextBox();
            this.groupBox19 = new System.Windows.Forms.GroupBox();
            this.checkBox_rxdebug = new System.Windows.Forms.CheckBox();
            this.cmb_sendrate = new System.Windows.Forms.ComboBox();
            this.cmb_baudrate = new System.Windows.Forms.ComboBox();
            this.cmb_portname = new System.Windows.Forms.ComboBox();
            this.BTN_extralcom = new MissionPlanner.Controls.MyButton();
            this.label81 = new System.Windows.Forms.Label();
            this.label80 = new System.Windows.Forms.Label();
            this.label72 = new System.Windows.Forms.Label();
            this.groupBox18 = new System.Windows.Forms.GroupBox();
            this.comboBox_video2imageHZ = new System.Windows.Forms.ComboBox();
            this.label71 = new System.Windows.Forms.Label();
            this.checkBox_video2imagesave = new System.Windows.Forms.CheckBox();
            this.label_video2state = new System.Windows.Forms.Label();
            this.myButton22 = new MissionPlanner.Controls.MyButton();
            this.BTN_video2record = new MissionPlanner.Controls.MyButton();
            this.myButton24 = new MissionPlanner.Controls.MyButton();
            this.txt_video2path = new System.Windows.Forms.TextBox();
            this.label69 = new System.Windows.Forms.Label();
            this.groupBox17 = new System.Windows.Forms.GroupBox();
            this.label70 = new System.Windows.Forms.Label();
            this.comboBox_video1imageHZ = new System.Windows.Forms.ComboBox();
            this.checkBox_video1imagesave = new System.Windows.Forms.CheckBox();
            this.label_video1state = new System.Windows.Forms.Label();
            this.myButton21 = new MissionPlanner.Controls.MyButton();
            this.BTN_video1record = new MissionPlanner.Controls.MyButton();
            this.myButton19 = new MissionPlanner.Controls.MyButton();
            this.txt_video1path = new System.Windows.Forms.TextBox();
            this.label68 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label_localip = new System.Windows.Forms.Label();
            this.UDP_receive = new MissionPlanner.Controls.MyButton();
            this.textBox_localip = new System.Windows.Forms.TextBox();
            this.textBox_localport = new System.Windows.Forms.TextBox();
            this.label_localport = new System.Windows.Forms.Label();
            this.BUT_Updatepos = new MissionPlanner.Controls.MyButton();
            this.PNL_status = new System.Windows.Forms.FlowLayoutPanel();
            this.timer1_status = new System.Windows.Forms.Timer(this.components);
            this.but_guided = new MissionPlanner.Controls.MyButton();
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.myButton_RTL = new MissionPlanner.Controls.MyButton();
            this.BUT_Auto = new MissionPlanner.Controls.MyButton();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ChangeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ZedGraphTimer = new System.Windows.Forms.Timer(this.components);
            this.timer3 = new System.Windows.Forms.Timer(this.components);
            this.bindingSource_3 = new System.Windows.Forms.BindingSource(this.components);
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_DispalyOut = new System.Windows.Forms.ToolStripMenuItem();
            this.timer4 = new System.Windows.Forms.Timer(this.components);
            this.timer5 = new System.Windows.Forms.Timer(this.components);
            this.Btn_Stop = new MissionPlanner.Controls.MyButton();
            this.distBwtLeadToIntercept = new System.Windows.Forms.Label();
            this.Btn_DoExtralCommand = new MissionPlanner.Controls.MyButton();
            this.Btn_DoCancelCommand = new MissionPlanner.Controls.MyButton();
            this.heightToIntercept = new System.Windows.Forms.Label();
            this.label111 = new System.Windows.Forms.Label();
            this.udp_transfer_enable = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource_1)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox24.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_offsety)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_offsetx)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource_4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_offsetz)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox14.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource_2)).BeginInit();
            this.panel_PID_param.SuspendLayout();
            this.groupBox10.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.virtualleaderenable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.min_approach_times)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fail_action)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.avoidance_enable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.avoidance_horizontal_m)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.avoidance_vertical_m)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fail_alt_min_m)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.warn_distance_z_m)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.warn_distance_xy_m)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fail_distance_xy_m)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fail_distance_z_m)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.warn_time_horizon_s)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fail_time_horizon_s)).BeginInit();
            this.groupBox9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AP_oil_power)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.copter_takeoff_m)).BeginInit();
            this.groupBox8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.speed_FILT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.speed_IMAX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.speed_D)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.speed_I)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.speed_P)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.stall_protect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pre_distance_to_lose_speed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.min_target_speed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.thrust_min)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.thrust_trim)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.delta_speed_scale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dist_integral_separation_m)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.max_dist_to_delta_speed_m)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yaw_to_roll_max_angle_deg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.roll_min_disttotarget_m)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.roll_high_dist_boundary_m)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.roll_low_dist_boundary_m)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yaw_distance_boundary_m)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.target_leader_dist_m)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.target_trailer_scale)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.thrust_FILT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.thrust_IMAX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.thrust_D)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.thrust_I)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.thrust_P)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yaw_FILT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yaw_IMAX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yaw_D)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yaw_I)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yaw_P)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pitch_FILT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pitch_IMAX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pitch_D)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pitch_I)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pitch_P)).BeginInit();
            this.groupBox25.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.roll_FILT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.roll_IMAX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.roll_D)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.roll_I)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.roll_P)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).BeginInit();
            this.splitContainer6.Panel1.SuspendLayout();
            this.splitContainer6.Panel2.SuspendLayout();
            this.splitContainer6.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox13.SuspendLayout();
            this.groupBox12.SuspendLayout();
            this.groupBox11.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_swarmspace)).BeginInit();
            this.tabPage6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer7)).BeginInit();
            this.splitContainer7.Panel1.SuspendLayout();
            this.splitContainer7.Panel2.SuspendLayout();
            this.splitContainer7.SuspendLayout();
            this.groupBox16.SuspendLayout();
            this.groupBox15.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabPage7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer8)).BeginInit();
            this.splitContainer8.Panel1.SuspendLayout();
            this.splitContainer8.SuspendLayout();
            this.tabPage_voiceIdentification.SuspendLayout();
            this.groupBox22.SuspendLayout();
            this.groupBox21.SuspendLayout();
            this.tabPage_targetserach.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer9)).BeginInit();
            this.splitContainer9.Panel1.SuspendLayout();
            this.splitContainer9.Panel2.SuspendLayout();
            this.splitContainer9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer10)).BeginInit();
            this.splitContainer10.Panel1.SuspendLayout();
            this.splitContainer10.Panel2.SuspendLayout();
            this.splitContainer10.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_overshot)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_searchalt)).BeginInit();
            this.groupBox23.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headingChangeInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.virtualTargetSpeed)).BeginInit();
            this.GmapcontextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.track_zoom)).BeginInit();
            this.tabPage_HIL.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer11)).BeginInit();
            this.splitContainer11.Panel1.SuspendLayout();
            this.splitContainer11.Panel2.SuspendLayout();
            this.splitContainer11.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer12)).BeginInit();
            this.splitContainer12.Panel1.SuspendLayout();
            this.splitContainer12.Panel2.SuspendLayout();
            this.splitContainer12.SuspendLayout();
            this.groupBox29.SuspendLayout();
            this.groupBox28.SuspendLayout();
            this.groupBox27.SuspendLayout();
            this.groupBox26.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.groupBox30.SuspendLayout();
            this.groupBox20.SuspendLayout();
            this.groupbox_rxdebug.SuspendLayout();
            this.groupBox19.SuspendLayout();
            this.groupBox18.SuspendLayout();
            this.groupBox17.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource_3)).BeginInit();
            this.contextMenuStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udp_transfer_enable)).BeginInit();
            this.SuspendLayout();
            // 
            // CMB_mavs
            // 
            this.CMB_mavs.DataSource = this.bindingSource_1;
            this.CMB_mavs.FormattingEnabled = true;
            this.CMB_mavs.Location = new System.Drawing.Point(423, 11);
            this.CMB_mavs.Name = "CMB_mavs";
            this.CMB_mavs.Size = new System.Drawing.Size(121, 20);
            this.CMB_mavs.TabIndex = 4;
            this.CMB_mavs.SelectedIndexChanged += new System.EventHandler(this.CMB_mavs_SelectedIndexChanged);
            // 
            // BUT_Start
            // 
            this.BUT_Start.Enabled = false;
            this.BUT_Start.Location = new System.Drawing.Point(955, 12);
            this.BUT_Start.Name = "BUT_Start";
            this.BUT_Start.Size = new System.Drawing.Size(75, 23);
            this.BUT_Start.TabIndex = 6;
            this.BUT_Start.Text = "开始";
            this.BUT_Start.UseVisualStyleBackColor = true;
            this.BUT_Start.Click += new System.EventHandler(this.BUT_Start_Click);
            // 
            // BUT_leader
            // 
            this.BUT_leader.Location = new System.Drawing.Point(550, 12);
            this.BUT_leader.Name = "BUT_leader";
            this.BUT_leader.Size = new System.Drawing.Size(75, 23);
            this.BUT_leader.TabIndex = 5;
            this.BUT_leader.Text = "Set Leader";
            this.BUT_leader.UseVisualStyleBackColor = true;
            this.BUT_leader.Click += new System.EventHandler(this.BUT_leader_Click);
            // 
            // BUT_Land
            // 
            this.BUT_Land.Location = new System.Drawing.Point(255, 12);
            this.BUT_Land.Name = "BUT_Land";
            this.BUT_Land.Size = new System.Drawing.Size(75, 23);
            this.BUT_Land.TabIndex = 3;
            this.BUT_Land.Text = "Land ";
            this.BUT_Land.UseVisualStyleBackColor = true;
            this.BUT_Land.Click += new System.EventHandler(this.BUT_Land_Click);
            // 
            // BUT_Takeoff
            // 
            this.BUT_Takeoff.Location = new System.Drawing.Point(174, 12);
            this.BUT_Takeoff.Name = "BUT_Takeoff";
            this.BUT_Takeoff.Size = new System.Drawing.Size(75, 23);
            this.BUT_Takeoff.TabIndex = 2;
            this.BUT_Takeoff.Text = "Takeoff";
            this.BUT_Takeoff.UseVisualStyleBackColor = true;
            this.BUT_Takeoff.Click += new System.EventHandler(this.BUT_Takeoff_Click);
            // 
            // BUT_Disarm
            // 
            this.BUT_Disarm.Location = new System.Drawing.Point(93, 12);
            this.BUT_Disarm.Name = "BUT_Disarm";
            this.BUT_Disarm.Size = new System.Drawing.Size(75, 23);
            this.BUT_Disarm.TabIndex = 1;
            this.BUT_Disarm.Text = "Disarm";
            this.BUT_Disarm.UseVisualStyleBackColor = true;
            this.BUT_Disarm.Click += new System.EventHandler(this.BUT_Disarm_Click);
            // 
            // BUT_Arm
            // 
            this.BUT_Arm.Location = new System.Drawing.Point(12, 12);
            this.BUT_Arm.Name = "BUT_Arm";
            this.BUT_Arm.Size = new System.Drawing.Size(75, 23);
            this.BUT_Arm.TabIndex = 0;
            this.BUT_Arm.Text = "Arm ";
            this.BUT_Arm.UseVisualStyleBackColor = true;
            this.BUT_Arm.Click += new System.EventHandler(this.BUT_Arm_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage6);
            this.tabControl1.Controls.Add(this.tabPage7);
            this.tabControl1.Controls.Add(this.tabPage_voiceIdentification);
            this.tabControl1.Controls.Add(this.tabPage_targetserach);
            this.tabControl1.Controls.Add(this.tabPage_HIL);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Location = new System.Drawing.Point(8, 61);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1242, 680);
            this.tabControl1.TabIndex = 9;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.Tab_SeletedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox24);
            this.tabPage1.Controls.Add(this.grid1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1234, 654);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "编队控制";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox24
            // 
            this.groupBox24.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox24.Controls.Add(this.numericUpDown_offsety);
            this.groupBox24.Controls.Add(this.numericUpDown_offsetx);
            this.groupBox24.Controls.Add(this.myButton26);
            this.groupBox24.Controls.Add(this.comboBox_mavsoffset);
            this.groupBox24.Controls.Add(this.numericUpDown_offsetz);
            this.groupBox24.Location = new System.Drawing.Point(1065, 23);
            this.groupBox24.Name = "groupBox24";
            this.groupBox24.Size = new System.Drawing.Size(163, 83);
            this.groupBox24.TabIndex = 79;
            this.groupBox24.TabStop = false;
            this.groupBox24.Text = "x,y,z轴偏差";
            // 
            // numericUpDown_offsety
            // 
            this.numericUpDown_offsety.Location = new System.Drawing.Point(58, 53);
            this.numericUpDown_offsety.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.numericUpDown_offsety.Minimum = new decimal(new int[] {
            300,
            0,
            0,
            -2147483648});
            this.numericUpDown_offsety.Name = "numericUpDown_offsety";
            this.numericUpDown_offsety.Size = new System.Drawing.Size(46, 21);
            this.numericUpDown_offsety.TabIndex = 82;
            // 
            // numericUpDown_offsetx
            // 
            this.numericUpDown_offsetx.Location = new System.Drawing.Point(6, 53);
            this.numericUpDown_offsetx.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.numericUpDown_offsetx.Minimum = new decimal(new int[] {
            300,
            0,
            0,
            -2147483648});
            this.numericUpDown_offsetx.Name = "numericUpDown_offsetx";
            this.numericUpDown_offsetx.Size = new System.Drawing.Size(46, 21);
            this.numericUpDown_offsetx.TabIndex = 81;
            // 
            // myButton26
            // 
            this.myButton26.Location = new System.Drawing.Point(108, 24);
            this.myButton26.Name = "myButton26";
            this.myButton26.Size = new System.Drawing.Size(55, 23);
            this.myButton26.TabIndex = 80;
            this.myButton26.Text = "输入";
            this.myButton26.UseVisualStyleBackColor = true;
            this.myButton26.Click += new System.EventHandler(this.BTN_SetMavOffset);
            // 
            // comboBox_mavsoffset
            // 
            this.comboBox_mavsoffset.DataSource = this.bindingSource_4;
            this.comboBox_mavsoffset.FormattingEnabled = true;
            this.comboBox_mavsoffset.Location = new System.Drawing.Point(0, 24);
            this.comboBox_mavsoffset.Name = "comboBox_mavsoffset";
            this.comboBox_mavsoffset.Size = new System.Drawing.Size(103, 20);
            this.comboBox_mavsoffset.TabIndex = 9;
            this.comboBox_mavsoffset.SelectedIndexChanged += new System.EventHandler(this.CMB_offsetClick);
            // 
            // numericUpDown_offsetz
            // 
            this.numericUpDown_offsetz.Location = new System.Drawing.Point(110, 53);
            this.numericUpDown_offsetz.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.numericUpDown_offsetz.Minimum = new decimal(new int[] {
            300,
            0,
            0,
            -2147483648});
            this.numericUpDown_offsetz.Name = "numericUpDown_offsetz";
            this.numericUpDown_offsetz.Size = new System.Drawing.Size(46, 21);
            this.numericUpDown_offsetz.TabIndex = 78;
            // 
            // grid1
            // 
            this.grid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid1.Location = new System.Drawing.Point(3, 3);
            this.grid1.Name = "grid1";
            this.grid1.Size = new System.Drawing.Size(1228, 648);
            this.grid1.TabIndex = 8;
            this.grid1.Vertical = false;
            this.grid1.UpdateOffsets += new MissionPlanner.Swarm.Grid.UpdateOffsetsEvent(this.grid1_UpdateOffsets);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.panel1);
            this.tabPage2.Controls.Add(this.panel_PID_param);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(1234, 654);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "扩展调参";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox14);
            this.panel1.Controls.Add(this.label56);
            this.panel1.Controls.Add(this.checkBox_AllAdvanceParam);
            this.panel1.Controls.Add(this.CMB_pid);
            this.panel1.Controls.Add(this.CHK_lockallmav);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(152, 645);
            this.panel1.TabIndex = 1;
            // 
            // groupBox14
            // 
            this.groupBox14.Controls.Add(this.myButton16);
            this.groupBox14.Controls.Add(this.myButton15);
            this.groupBox14.Controls.Add(this.myButton9);
            this.groupBox14.Controls.Add(this.myButton4);
            this.groupBox14.Controls.Add(this.myButton11);
            this.groupBox14.Controls.Add(this.myButton12);
            this.groupBox14.Controls.Add(this.myButton14);
            this.groupBox14.Controls.Add(this.myButton5);
            this.groupBox14.Controls.Add(this.myButton13);
            this.groupBox14.Controls.Add(this.myButton6);
            this.groupBox14.Controls.Add(this.myButton10);
            this.groupBox14.Controls.Add(this.myButton7);
            this.groupBox14.Controls.Add(this.myButton8);
            this.groupBox14.Location = new System.Drawing.Point(4, 114);
            this.groupBox14.Name = "groupBox14";
            this.groupBox14.Size = new System.Drawing.Size(149, 217);
            this.groupBox14.TabIndex = 76;
            this.groupBox14.TabStop = false;
            // 
            // myButton16
            // 
            this.myButton16.Location = new System.Drawing.Point(75, 10);
            this.myButton16.Name = "myButton16";
            this.myButton16.Size = new System.Drawing.Size(75, 23);
            this.myButton16.TabIndex = 20;
            this.myButton16.Text = "Disarm";
            this.myButton16.UseVisualStyleBackColor = true;
            this.myButton16.Click += new System.EventHandler(this.BTN_DisarmCommand);
            // 
            // myButton15
            // 
            this.myButton15.Location = new System.Drawing.Point(-1, 10);
            this.myButton15.Name = "myButton15";
            this.myButton15.Size = new System.Drawing.Size(75, 23);
            this.myButton15.TabIndex = 19;
            this.myButton15.Text = "Arm";
            this.myButton15.UseVisualStyleBackColor = true;
            this.myButton15.Click += new System.EventHandler(this.BTN_ArmCommand);
            // 
            // myButton9
            // 
            this.myButton9.Location = new System.Drawing.Point(-1, 184);
            this.myButton9.Name = "myButton9";
            this.myButton9.Size = new System.Drawing.Size(75, 23);
            this.myButton9.TabIndex = 13;
            this.myButton9.Text = "GUIDED";
            this.myButton9.UseVisualStyleBackColor = true;
            this.myButton9.Click += new System.EventHandler(this.BTN_GuidedMode);
            // 
            // myButton4
            // 
            this.myButton4.Location = new System.Drawing.Point(-1, 39);
            this.myButton4.Name = "myButton4";
            this.myButton4.Size = new System.Drawing.Size(75, 23);
            this.myButton4.TabIndex = 8;
            this.myButton4.Text = "MANUAL";
            this.myButton4.UseVisualStyleBackColor = true;
            this.myButton4.Click += new System.EventHandler(this.BTN_ManualMode);
            // 
            // myButton11
            // 
            this.myButton11.Location = new System.Drawing.Point(75, 155);
            this.myButton11.Name = "myButton11";
            this.myButton11.Size = new System.Drawing.Size(75, 23);
            this.myButton11.TabIndex = 15;
            this.myButton11.Text = "Takeoff";
            this.myButton11.UseVisualStyleBackColor = true;
            this.myButton11.Click += new System.EventHandler(this.BTN_TakeoffMode);
            // 
            // myButton12
            // 
            this.myButton12.Location = new System.Drawing.Point(-1, 155);
            this.myButton12.Name = "myButton12";
            this.myButton12.Size = new System.Drawing.Size(75, 23);
            this.myButton12.TabIndex = 16;
            this.myButton12.Text = "Land";
            this.myButton12.UseVisualStyleBackColor = true;
            this.myButton12.Click += new System.EventHandler(this.BTN_LandMode);
            // 
            // myButton14
            // 
            this.myButton14.Location = new System.Drawing.Point(75, 126);
            this.myButton14.Name = "myButton14";
            this.myButton14.Size = new System.Drawing.Size(75, 23);
            this.myButton14.TabIndex = 18;
            this.myButton14.Text = "QRTL";
            this.myButton14.UseVisualStyleBackColor = true;
            this.myButton14.Click += new System.EventHandler(this.BTN_QRTLMode);
            // 
            // myButton5
            // 
            this.myButton5.Location = new System.Drawing.Point(75, 39);
            this.myButton5.Name = "myButton5";
            this.myButton5.Size = new System.Drawing.Size(75, 23);
            this.myButton5.TabIndex = 9;
            this.myButton5.Text = "FBWA";
            this.myButton5.UseVisualStyleBackColor = true;
            this.myButton5.Click += new System.EventHandler(this.BTN_FBWAMode);
            // 
            // myButton13
            // 
            this.myButton13.Location = new System.Drawing.Point(75, 97);
            this.myButton13.Name = "myButton13";
            this.myButton13.Size = new System.Drawing.Size(75, 23);
            this.myButton13.TabIndex = 17;
            this.myButton13.Text = "QLOITER";
            this.myButton13.UseVisualStyleBackColor = true;
            this.myButton13.Click += new System.EventHandler(this.BTN_QLoiterMode);
            // 
            // myButton6
            // 
            this.myButton6.Location = new System.Drawing.Point(-1, 68);
            this.myButton6.Name = "myButton6";
            this.myButton6.Size = new System.Drawing.Size(75, 23);
            this.myButton6.TabIndex = 10;
            this.myButton6.Text = "FBWB";
            this.myButton6.UseVisualStyleBackColor = true;
            this.myButton6.Click += new System.EventHandler(this.BTN_FBWBMode);
            // 
            // myButton10
            // 
            this.myButton10.Location = new System.Drawing.Point(-1, 126);
            this.myButton10.Name = "myButton10";
            this.myButton10.Size = new System.Drawing.Size(75, 23);
            this.myButton10.TabIndex = 14;
            this.myButton10.Text = "RTL";
            this.myButton10.UseVisualStyleBackColor = true;
            this.myButton10.Click += new System.EventHandler(this.BTN_RTLMode);
            // 
            // myButton7
            // 
            this.myButton7.Location = new System.Drawing.Point(75, 68);
            this.myButton7.Name = "myButton7";
            this.myButton7.Size = new System.Drawing.Size(75, 23);
            this.myButton7.TabIndex = 11;
            this.myButton7.Text = "AUTO";
            this.myButton7.UseVisualStyleBackColor = true;
            this.myButton7.Click += new System.EventHandler(this.BTN_AutoMode);
            // 
            // myButton8
            // 
            this.myButton8.Location = new System.Drawing.Point(-1, 97);
            this.myButton8.Name = "myButton8";
            this.myButton8.Size = new System.Drawing.Size(75, 23);
            this.myButton8.TabIndex = 12;
            this.myButton8.Text = "LOITER";
            this.myButton8.UseVisualStyleBackColor = true;
            this.myButton8.Click += new System.EventHandler(this.BTN_LoiterMode);
            // 
            // label56
            // 
            this.label56.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label56.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label56.Location = new System.Drawing.Point(3, 88);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(150, 23);
            this.label56.TabIndex = 75;
            this.label56.Text = "设置单机模式:";
            // 
            // checkBox_AllAdvanceParam
            // 
            this.checkBox_AllAdvanceParam.AutoSize = true;
            this.checkBox_AllAdvanceParam.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.checkBox_AllAdvanceParam.Location = new System.Drawing.Point(3, 24);
            this.checkBox_AllAdvanceParam.Name = "checkBox_AllAdvanceParam";
            this.checkBox_AllAdvanceParam.Size = new System.Drawing.Size(102, 16);
            this.checkBox_AllAdvanceParam.TabIndex = 7;
            this.checkBox_AllAdvanceParam.Text = "高级参数(all)";
            this.checkBox_AllAdvanceParam.UseVisualStyleBackColor = true;
            // 
            // CMB_pid
            // 
            this.CMB_pid.DataSource = this.bindingSource_2;
            this.CMB_pid.FormattingEnabled = true;
            this.CMB_pid.Location = new System.Drawing.Point(0, 49);
            this.CMB_pid.Name = "CMB_pid";
            this.CMB_pid.Size = new System.Drawing.Size(149, 20);
            this.CMB_pid.TabIndex = 6;
            this.CMB_pid.SelectedIndexChanged += new System.EventHandler(this.CMB_pid_SelectedIndexChanged);
            // 
            // CHK_lockallmav
            // 
            this.CHK_lockallmav.AutoSize = true;
            this.CHK_lockallmav.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CHK_lockallmav.Location = new System.Drawing.Point(3, 0);
            this.CHK_lockallmav.Name = "CHK_lockallmav";
            this.CHK_lockallmav.Size = new System.Drawing.Size(96, 16);
            this.CHK_lockallmav.TabIndex = 5;
            this.CHK_lockallmav.Text = "调参全部飞机";
            this.CHK_lockallmav.UseVisualStyleBackColor = true;
            // 
            // panel_PID_param
            // 
            this.panel_PID_param.Controls.Add(this.groupBox10);
            this.panel_PID_param.Controls.Add(this.label44);
            this.panel_PID_param.Controls.Add(this.groupBox9);
            this.panel_PID_param.Controls.Add(this.groupBox8);
            this.panel_PID_param.Controls.Add(this.groupBox4);
            this.panel_PID_param.Controls.Add(this.label19);
            this.panel_PID_param.Controls.Add(this.label18);
            this.panel_PID_param.Controls.Add(this.BUT_writePIDS);
            this.panel_PID_param.Controls.Add(this.groupBox3);
            this.panel_PID_param.Controls.Add(this.groupBox2);
            this.panel_PID_param.Controls.Add(this.groupBox1);
            this.panel_PID_param.Controls.Add(this.groupBox25);
            this.panel_PID_param.Location = new System.Drawing.Point(158, 3);
            this.panel_PID_param.Name = "panel_PID_param";
            this.panel_PID_param.Size = new System.Drawing.Size(1073, 645);
            this.panel_PID_param.TabIndex = 0;
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.virtualleaderenable);
            this.groupBox10.Controls.Add(this.label57);
            this.groupBox10.Controls.Add(this.min_approach_times);
            this.groupBox10.Controls.Add(this.label55);
            this.groupBox10.Controls.Add(this.fail_action);
            this.groupBox10.Controls.Add(this.label85);
            this.groupBox10.Controls.Add(this.avoidance_enable);
            this.groupBox10.Controls.Add(this.label54);
            this.groupBox10.Controls.Add(this.avoidance_horizontal_m);
            this.groupBox10.Controls.Add(this.avoidance_vertical_m);
            this.groupBox10.Controls.Add(this.label53);
            this.groupBox10.Controls.Add(this.label52);
            this.groupBox10.Controls.Add(this.fail_alt_min_m);
            this.groupBox10.Controls.Add(this.label51);
            this.groupBox10.Controls.Add(this.warn_distance_z_m);
            this.groupBox10.Controls.Add(this.label50);
            this.groupBox10.Controls.Add(this.warn_distance_xy_m);
            this.groupBox10.Controls.Add(this.fail_distance_xy_m);
            this.groupBox10.Controls.Add(this.fail_distance_z_m);
            this.groupBox10.Controls.Add(this.warn_time_horizon_s);
            this.groupBox10.Controls.Add(this.label49);
            this.groupBox10.Controls.Add(this.label48);
            this.groupBox10.Controls.Add(this.label47);
            this.groupBox10.Controls.Add(this.label46);
            this.groupBox10.Controls.Add(this.fail_time_horizon_s);
            this.groupBox10.Controls.Add(this.label45);
            this.groupBox10.Location = new System.Drawing.Point(248, 432);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(588, 210);
            this.groupBox10.TabIndex = 96;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Avoidance";
            // 
            // virtualleaderenable
            // 
            this.virtualleaderenable.Location = new System.Drawing.Point(473, 146);
            this.virtualleaderenable.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.virtualleaderenable.Name = "virtualleaderenable";
            this.virtualleaderenable.Size = new System.Drawing.Size(73, 21);
            this.virtualleaderenable.TabIndex = 114;
            this.virtualleaderenable.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label57
            // 
            this.label57.AutoSize = true;
            this.label57.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label57.Location = new System.Drawing.Point(280, 150);
            this.label57.Name = "label57";
            this.label57.Size = new System.Drawing.Size(131, 12);
            this.label57.TabIndex = 113;
            this.label57.Text = "VIRTUAL_LEADER_ENABLE";
            // 
            // min_approach_times
            // 
            this.min_approach_times.DecimalPlaces = 1;
            this.min_approach_times.Location = new System.Drawing.Point(473, 123);
            this.min_approach_times.Name = "min_approach_times";
            this.min_approach_times.Size = new System.Drawing.Size(73, 21);
            this.min_approach_times.TabIndex = 112;
            this.min_approach_times.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label55
            // 
            this.label55.AutoSize = true;
            this.label55.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label55.Location = new System.Drawing.Point(279, 125);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(119, 12);
            this.label55.TabIndex = 111;
            this.label55.Text = "MIN_APPROACH_TIME_S";
            // 
            // fail_action
            // 
            this.fail_action.Location = new System.Drawing.Point(473, 73);
            this.fail_action.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.fail_action.Name = "fail_action";
            this.fail_action.Size = new System.Drawing.Size(73, 21);
            this.fail_action.TabIndex = 110;
            this.fail_action.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // label85
            // 
            this.label85.AutoSize = true;
            this.label85.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label85.Location = new System.Drawing.Point(280, 75);
            this.label85.Name = "label85";
            this.label85.Size = new System.Drawing.Size(71, 12);
            this.label85.TabIndex = 109;
            this.label85.Text = "FAIL_ACTION";
            // 
            // avoidance_enable
            // 
            this.avoidance_enable.Location = new System.Drawing.Point(473, 98);
            this.avoidance_enable.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.avoidance_enable.Name = "avoidance_enable";
            this.avoidance_enable.Size = new System.Drawing.Size(73, 21);
            this.avoidance_enable.TabIndex = 108;
            // 
            // label54
            // 
            this.label54.AutoSize = true;
            this.label54.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label54.Location = new System.Drawing.Point(280, 100);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(101, 12);
            this.label54.TabIndex = 107;
            this.label54.Text = "AVOIDANCE_ENABLE";
            // 
            // avoidance_horizontal_m
            // 
            this.avoidance_horizontal_m.Location = new System.Drawing.Point(473, 50);
            this.avoidance_horizontal_m.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.avoidance_horizontal_m.Name = "avoidance_horizontal_m";
            this.avoidance_horizontal_m.Size = new System.Drawing.Size(73, 21);
            this.avoidance_horizontal_m.TabIndex = 106;
            this.avoidance_horizontal_m.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // avoidance_vertical_m
            // 
            this.avoidance_vertical_m.Location = new System.Drawing.Point(473, 27);
            this.avoidance_vertical_m.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.avoidance_vertical_m.Name = "avoidance_vertical_m";
            this.avoidance_vertical_m.Size = new System.Drawing.Size(73, 21);
            this.avoidance_vertical_m.TabIndex = 105;
            this.avoidance_vertical_m.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // label53
            // 
            this.label53.AutoSize = true;
            this.label53.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label53.Location = new System.Drawing.Point(280, 52);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(125, 12);
            this.label53.TabIndex = 104;
            this.label53.Text = "AVOIDANCE_HORIZONTAL";
            // 
            // label52
            // 
            this.label52.AutoSize = true;
            this.label52.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label52.Location = new System.Drawing.Point(280, 27);
            this.label52.Name = "label52";
            this.label52.Size = new System.Drawing.Size(113, 12);
            this.label52.TabIndex = 103;
            this.label52.Text = "AVOIDANCE_VERTICAL";
            // 
            // fail_alt_min_m
            // 
            this.fail_alt_min_m.Location = new System.Drawing.Point(175, 173);
            this.fail_alt_min_m.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.fail_alt_min_m.Name = "fail_alt_min_m";
            this.fail_alt_min_m.Size = new System.Drawing.Size(73, 21);
            this.fail_alt_min_m.TabIndex = 102;
            this.fail_alt_min_m.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label51.Location = new System.Drawing.Point(6, 179);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(77, 12);
            this.label51.TabIndex = 101;
            this.label51.Text = "FAIL_ALT_MIN";
            // 
            // warn_distance_z_m
            // 
            this.warn_distance_z_m.Location = new System.Drawing.Point(175, 148);
            this.warn_distance_z_m.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.warn_distance_z_m.Name = "warn_distance_z_m";
            this.warn_distance_z_m.Size = new System.Drawing.Size(73, 21);
            this.warn_distance_z_m.TabIndex = 100;
            this.warn_distance_z_m.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label50.Location = new System.Drawing.Point(6, 153);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(95, 12);
            this.label50.TabIndex = 99;
            this.label50.Text = "WARN_DISTANCE_Z";
            // 
            // warn_distance_xy_m
            // 
            this.warn_distance_xy_m.Location = new System.Drawing.Point(175, 123);
            this.warn_distance_xy_m.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.warn_distance_xy_m.Name = "warn_distance_xy_m";
            this.warn_distance_xy_m.Size = new System.Drawing.Size(73, 21);
            this.warn_distance_xy_m.TabIndex = 98;
            this.warn_distance_xy_m.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            // 
            // fail_distance_xy_m
            // 
            this.fail_distance_xy_m.Location = new System.Drawing.Point(175, 73);
            this.fail_distance_xy_m.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.fail_distance_xy_m.Name = "fail_distance_xy_m";
            this.fail_distance_xy_m.Size = new System.Drawing.Size(73, 21);
            this.fail_distance_xy_m.TabIndex = 97;
            this.fail_distance_xy_m.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            // 
            // fail_distance_z_m
            // 
            this.fail_distance_z_m.Location = new System.Drawing.Point(175, 98);
            this.fail_distance_z_m.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.fail_distance_z_m.Name = "fail_distance_z_m";
            this.fail_distance_z_m.Size = new System.Drawing.Size(73, 21);
            this.fail_distance_z_m.TabIndex = 96;
            this.fail_distance_z_m.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // warn_time_horizon_s
            // 
            this.warn_time_horizon_s.DecimalPlaces = 1;
            this.warn_time_horizon_s.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.warn_time_horizon_s.Location = new System.Drawing.Point(175, 48);
            this.warn_time_horizon_s.Name = "warn_time_horizon_s";
            this.warn_time_horizon_s.Size = new System.Drawing.Size(73, 21);
            this.warn_time_horizon_s.TabIndex = 95;
            this.warn_time_horizon_s.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label49.Location = new System.Drawing.Point(6, 75);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(101, 12);
            this.label49.TabIndex = 94;
            this.label49.Text = "FAIL_DISTANCE_XY";
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label48.Location = new System.Drawing.Point(6, 127);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(101, 12);
            this.label48.TabIndex = 93;
            this.label48.Text = "WARN_DISTANCE_XY";
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label47.Location = new System.Drawing.Point(6, 101);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(95, 12);
            this.label47.TabIndex = 92;
            this.label47.Text = "FAIL_DISTANCE_Z";
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label46.Location = new System.Drawing.Point(6, 49);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(107, 12);
            this.label46.TabIndex = 91;
            this.label46.Text = "WARN_TIME_HORIZON";
            // 
            // fail_time_horizon_s
            // 
            this.fail_time_horizon_s.DecimalPlaces = 1;
            this.fail_time_horizon_s.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.fail_time_horizon_s.Location = new System.Drawing.Point(175, 23);
            this.fail_time_horizon_s.Name = "fail_time_horizon_s";
            this.fail_time_horizon_s.Size = new System.Drawing.Size(73, 21);
            this.fail_time_horizon_s.TabIndex = 90;
            this.fail_time_horizon_s.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label45.Location = new System.Drawing.Point(6, 23);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(107, 12);
            this.label45.TabIndex = 7;
            this.label45.Text = "FAIL_TIME_HORIZON";
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label44.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label44.Location = new System.Drawing.Point(7, 404);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(94, 20);
            this.label44.TabIndex = 95;
            this.label44.Text = "编队参数：";
            // 
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.udp_transfer_enable);
            this.groupBox9.Controls.Add(this.label111);
            this.groupBox9.Controls.Add(this.AP_oil_power);
            this.groupBox9.Controls.Add(this.label101);
            this.groupBox9.Controls.Add(this.copter_takeoff_m);
            this.groupBox9.Controls.Add(this.label43);
            this.groupBox9.Location = new System.Drawing.Point(12, 432);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(207, 210);
            this.groupBox9.TabIndex = 94;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "for swarm";
            // 
            // AP_oil_power
            // 
            this.AP_oil_power.Location = new System.Drawing.Point(147, 44);
            this.AP_oil_power.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.AP_oil_power.Name = "AP_oil_power";
            this.AP_oil_power.Size = new System.Drawing.Size(54, 21);
            this.AP_oil_power.TabIndex = 92;
            // 
            // label101
            // 
            this.label101.AutoSize = true;
            this.label101.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label101.Location = new System.Drawing.Point(6, 49);
            this.label101.Name = "label101";
            this.label101.Size = new System.Drawing.Size(77, 12);
            this.label101.TabIndex = 91;
            this.label101.Text = "AP_OIL_POWER";
            // 
            // copter_takeoff_m
            // 
            this.copter_takeoff_m.Location = new System.Drawing.Point(147, 20);
            this.copter_takeoff_m.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.copter_takeoff_m.Name = "copter_takeoff_m";
            this.copter_takeoff_m.Size = new System.Drawing.Size(54, 21);
            this.copter_takeoff_m.TabIndex = 90;
            this.copter_takeoff_m.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label43.Location = new System.Drawing.Point(6, 25);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(101, 12);
            this.label43.TabIndex = 7;
            this.label43.Text = "AC_TAKEOFF_ALT_M";
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.speed_FILT);
            this.groupBox8.Controls.Add(this.speed_IMAX);
            this.groupBox8.Controls.Add(this.speed_D);
            this.groupBox8.Controls.Add(this.speed_I);
            this.groupBox8.Controls.Add(this.speed_P);
            this.groupBox8.Controls.Add(this.label35);
            this.groupBox8.Controls.Add(this.label36);
            this.groupBox8.Controls.Add(this.label37);
            this.groupBox8.Controls.Add(this.label38);
            this.groupBox8.Controls.Add(this.label39);
            this.groupBox8.Location = new System.Drawing.Point(666, 48);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(170, 136);
            this.groupBox8.TabIndex = 77;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Dist To Speed";
            // 
            // speed_FILT
            // 
            this.speed_FILT.DecimalPlaces = 2;
            this.speed_FILT.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.speed_FILT.Location = new System.Drawing.Point(80, 110);
            this.speed_FILT.Name = "speed_FILT";
            this.speed_FILT.Size = new System.Drawing.Size(84, 21);
            this.speed_FILT.TabIndex = 13;
            // 
            // speed_IMAX
            // 
            this.speed_IMAX.DecimalPlaces = 2;
            this.speed_IMAX.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.speed_IMAX.Location = new System.Drawing.Point(80, 86);
            this.speed_IMAX.Name = "speed_IMAX";
            this.speed_IMAX.Size = new System.Drawing.Size(84, 21);
            this.speed_IMAX.TabIndex = 12;
            // 
            // speed_D
            // 
            this.speed_D.DecimalPlaces = 3;
            this.speed_D.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.speed_D.Location = new System.Drawing.Point(80, 61);
            this.speed_D.Name = "speed_D";
            this.speed_D.Size = new System.Drawing.Size(84, 21);
            this.speed_D.TabIndex = 11;
            // 
            // speed_I
            // 
            this.speed_I.DecimalPlaces = 3;
            this.speed_I.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.speed_I.Location = new System.Drawing.Point(80, 38);
            this.speed_I.Name = "speed_I";
            this.speed_I.Size = new System.Drawing.Size(84, 21);
            this.speed_I.TabIndex = 10;
            // 
            // speed_P
            // 
            this.speed_P.DecimalPlaces = 3;
            this.speed_P.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.speed_P.Location = new System.Drawing.Point(80, 16);
            this.speed_P.Name = "speed_P";
            this.speed_P.Size = new System.Drawing.Size(84, 21);
            this.speed_P.TabIndex = 9;
            // 
            // label35
            // 
            this.label35.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label35.Location = new System.Drawing.Point(6, 112);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(68, 13);
            this.label35.TabIndex = 8;
            this.label35.Text = "FILT";
            // 
            // label36
            // 
            this.label36.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label36.Location = new System.Drawing.Point(6, 63);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(10, 13);
            this.label36.TabIndex = 4;
            this.label36.Text = "D";
            // 
            // label37
            // 
            this.label37.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label37.Location = new System.Drawing.Point(6, 86);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(68, 13);
            this.label37.TabIndex = 6;
            this.label37.Text = "IMAX";
            // 
            // label38
            // 
            this.label38.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label38.Location = new System.Drawing.Point(6, 40);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(10, 13);
            this.label38.TabIndex = 2;
            this.label38.Text = "I";
            // 
            // label39
            // 
            this.label39.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label39.Location = new System.Drawing.Point(6, 16);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(14, 13);
            this.label39.TabIndex = 0;
            this.label39.Text = "P";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.stall_protect);
            this.groupBox4.Controls.Add(this.label108);
            this.groupBox4.Controls.Add(this.pre_distance_to_lose_speed);
            this.groupBox4.Controls.Add(this.label100);
            this.groupBox4.Controls.Add(this.min_target_speed);
            this.groupBox4.Controls.Add(this.label99);
            this.groupBox4.Controls.Add(this.thrust_min);
            this.groupBox4.Controls.Add(this.label98);
            this.groupBox4.Controls.Add(this.thrust_trim);
            this.groupBox4.Controls.Add(this.label97);
            this.groupBox4.Controls.Add(this.delta_speed_scale);
            this.groupBox4.Controls.Add(this.dist_integral_separation_m);
            this.groupBox4.Controls.Add(this.label42);
            this.groupBox4.Controls.Add(this.label41);
            this.groupBox4.Controls.Add(this.max_dist_to_delta_speed_m);
            this.groupBox4.Controls.Add(this.label40);
            this.groupBox4.Controls.Add(this.yaw_to_roll_max_angle_deg);
            this.groupBox4.Controls.Add(this.label26);
            this.groupBox4.Controls.Add(this.roll_min_disttotarget_m);
            this.groupBox4.Controls.Add(this.label25);
            this.groupBox4.Controls.Add(this.roll_high_dist_boundary_m);
            this.groupBox4.Controls.Add(this.label24);
            this.groupBox4.Controls.Add(this.roll_low_dist_boundary_m);
            this.groupBox4.Controls.Add(this.label23);
            this.groupBox4.Controls.Add(this.yaw_distance_boundary_m);
            this.groupBox4.Controls.Add(this.label22);
            this.groupBox4.Controls.Add(this.target_leader_dist_m);
            this.groupBox4.Controls.Add(this.label21);
            this.groupBox4.Controls.Add(this.label20);
            this.groupBox4.Controls.Add(this.target_trailer_scale);
            this.groupBox4.Location = new System.Drawing.Point(8, 208);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(1062, 193);
            this.groupBox4.TabIndex = 76;
            this.groupBox4.TabStop = false;
            // 
            // stall_protect
            // 
            this.stall_protect.Location = new System.Drawing.Point(845, 25);
            this.stall_protect.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.stall_protect.Name = "stall_protect";
            this.stall_protect.Size = new System.Drawing.Size(84, 21);
            this.stall_protect.TabIndex = 103;
            // 
            // label108
            // 
            this.label108.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label108.Location = new System.Drawing.Point(664, 26);
            this.label108.Name = "label108";
            this.label108.Size = new System.Drawing.Size(122, 23);
            this.label108.TabIndex = 102;
            this.label108.Text = "STALL_PROTECT";
            // 
            // pre_distance_to_lose_speed
            // 
            this.pre_distance_to_lose_speed.Location = new System.Drawing.Point(559, 154);
            this.pre_distance_to_lose_speed.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.pre_distance_to_lose_speed.Name = "pre_distance_to_lose_speed";
            this.pre_distance_to_lose_speed.Size = new System.Drawing.Size(84, 21);
            this.pre_distance_to_lose_speed.TabIndex = 101;
            this.pre_distance_to_lose_speed.Value = new decimal(new int[] {
            130,
            0,
            0,
            0});
            // 
            // label100
            // 
            this.label100.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label100.Location = new System.Drawing.Point(329, 158);
            this.label100.Name = "label100";
            this.label100.Size = new System.Drawing.Size(215, 23);
            this.label100.TabIndex = 100;
            this.label100.Text = "PRE_DISTANCE_LOSE_SPEED";
            // 
            // min_target_speed
            // 
            this.min_target_speed.Location = new System.Drawing.Point(559, 133);
            this.min_target_speed.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.min_target_speed.Name = "min_target_speed";
            this.min_target_speed.Size = new System.Drawing.Size(84, 21);
            this.min_target_speed.TabIndex = 99;
            this.min_target_speed.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            // 
            // label99
            // 
            this.label99.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label99.Location = new System.Drawing.Point(329, 135);
            this.label99.Name = "label99";
            this.label99.Size = new System.Drawing.Size(175, 23);
            this.label99.TabIndex = 98;
            this.label99.Text = "MIN_TARGET_SPEED";
            // 
            // thrust_min
            // 
            this.thrust_min.DecimalPlaces = 2;
            this.thrust_min.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.thrust_min.Location = new System.Drawing.Point(559, 111);
            this.thrust_min.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.thrust_min.Name = "thrust_min";
            this.thrust_min.Size = new System.Drawing.Size(84, 21);
            this.thrust_min.TabIndex = 97;
            this.thrust_min.Value = new decimal(new int[] {
            8,
            0,
            0,
            131072});
            // 
            // label98
            // 
            this.label98.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label98.Location = new System.Drawing.Point(329, 112);
            this.label98.Name = "label98";
            this.label98.Size = new System.Drawing.Size(175, 23);
            this.label98.TabIndex = 96;
            this.label98.Text = "THRUST_MIN";
            // 
            // thrust_trim
            // 
            this.thrust_trim.DecimalPlaces = 2;
            this.thrust_trim.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.thrust_trim.Location = new System.Drawing.Point(559, 90);
            this.thrust_trim.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.thrust_trim.Name = "thrust_trim";
            this.thrust_trim.Size = new System.Drawing.Size(84, 21);
            this.thrust_trim.TabIndex = 95;
            this.thrust_trim.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            // 
            // label97
            // 
            this.label97.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label97.Location = new System.Drawing.Point(329, 90);
            this.label97.Name = "label97";
            this.label97.Size = new System.Drawing.Size(175, 23);
            this.label97.TabIndex = 94;
            this.label97.Text = "THRUST_TRIM";
            // 
            // delta_speed_scale
            // 
            this.delta_speed_scale.DecimalPlaces = 2;
            this.delta_speed_scale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.delta_speed_scale.Location = new System.Drawing.Point(559, 67);
            this.delta_speed_scale.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.delta_speed_scale.Name = "delta_speed_scale";
            this.delta_speed_scale.Size = new System.Drawing.Size(84, 21);
            this.delta_speed_scale.TabIndex = 93;
            this.delta_speed_scale.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            // 
            // dist_integral_separation_m
            // 
            this.dist_integral_separation_m.Location = new System.Drawing.Point(559, 47);
            this.dist_integral_separation_m.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.dist_integral_separation_m.Name = "dist_integral_separation_m";
            this.dist_integral_separation_m.Size = new System.Drawing.Size(84, 21);
            this.dist_integral_separation_m.TabIndex = 92;
            this.dist_integral_separation_m.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            // 
            // label42
            // 
            this.label42.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label42.Location = new System.Drawing.Point(329, 69);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(175, 23);
            this.label42.TabIndex = 91;
            this.label42.Text = "DELTA_SPEED_SCALE";
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label41.Location = new System.Drawing.Point(329, 49);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(161, 12);
            this.label41.TabIndex = 90;
            this.label41.Text = "DIST_INTEGRAL_SEPARATION_M";
            // 
            // max_dist_to_delta_speed_m
            // 
            this.max_dist_to_delta_speed_m.Location = new System.Drawing.Point(559, 26);
            this.max_dist_to_delta_speed_m.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.max_dist_to_delta_speed_m.Name = "max_dist_to_delta_speed_m";
            this.max_dist_to_delta_speed_m.Size = new System.Drawing.Size(84, 21);
            this.max_dist_to_delta_speed_m.TabIndex = 89;
            this.max_dist_to_delta_speed_m.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label40.Location = new System.Drawing.Point(329, 28);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(149, 12);
            this.label40.TabIndex = 88;
            this.label40.Text = "MAX_DISTTO_DELTA_SPEED_M";
            // 
            // yaw_to_roll_max_angle_deg
            // 
            this.yaw_to_roll_max_angle_deg.Increment = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.yaw_to_roll_max_angle_deg.Location = new System.Drawing.Point(224, 161);
            this.yaw_to_roll_max_angle_deg.Maximum = new decimal(new int[] {
            65,
            0,
            0,
            0});
            this.yaw_to_roll_max_angle_deg.Name = "yaw_to_roll_max_angle_deg";
            this.yaw_to_roll_max_angle_deg.Size = new System.Drawing.Size(84, 21);
            this.yaw_to_roll_max_angle_deg.TabIndex = 87;
            this.yaw_to_roll_max_angle_deg.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // label26
            // 
            this.label26.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label26.Location = new System.Drawing.Point(6, 163);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(212, 23);
            this.label26.TabIndex = 86;
            this.label26.Text = "YAW_TO_ROLL_MAX_ANGLE_DEG";
            // 
            // roll_min_disttotarget_m
            // 
            this.roll_min_disttotarget_m.Location = new System.Drawing.Point(224, 138);
            this.roll_min_disttotarget_m.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.roll_min_disttotarget_m.Name = "roll_min_disttotarget_m";
            this.roll_min_disttotarget_m.Size = new System.Drawing.Size(84, 21);
            this.roll_min_disttotarget_m.TabIndex = 85;
            this.roll_min_disttotarget_m.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            // 
            // label25
            // 
            this.label25.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label25.Location = new System.Drawing.Point(6, 140);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(193, 23);
            this.label25.TabIndex = 84;
            this.label25.Text = "ROLL_MIN_DISTTOTARGET_M";
            // 
            // roll_high_dist_boundary_m
            // 
            this.roll_high_dist_boundary_m.Increment = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.roll_high_dist_boundary_m.Location = new System.Drawing.Point(224, 113);
            this.roll_high_dist_boundary_m.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.roll_high_dist_boundary_m.Name = "roll_high_dist_boundary_m";
            this.roll_high_dist_boundary_m.Size = new System.Drawing.Size(84, 21);
            this.roll_high_dist_boundary_m.TabIndex = 83;
            this.roll_high_dist_boundary_m.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label24
            // 
            this.label24.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label24.Location = new System.Drawing.Point(6, 115);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(212, 23);
            this.label24.TabIndex = 82;
            this.label24.Text = "ROLL_HIGH_DIST_BOUNDARY_M";
            // 
            // roll_low_dist_boundary_m
            // 
            this.roll_low_dist_boundary_m.Location = new System.Drawing.Point(224, 90);
            this.roll_low_dist_boundary_m.Name = "roll_low_dist_boundary_m";
            this.roll_low_dist_boundary_m.Size = new System.Drawing.Size(84, 21);
            this.roll_low_dist_boundary_m.TabIndex = 81;
            this.roll_low_dist_boundary_m.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // label23
            // 
            this.label23.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label23.Location = new System.Drawing.Point(6, 92);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(212, 23);
            this.label23.TabIndex = 80;
            this.label23.Text = "ROLL_LOW_DIST_BOUNDARY_M";
            // 
            // yaw_distance_boundary_m
            // 
            this.yaw_distance_boundary_m.Location = new System.Drawing.Point(224, 67);
            this.yaw_distance_boundary_m.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.yaw_distance_boundary_m.Name = "yaw_distance_boundary_m";
            this.yaw_distance_boundary_m.Size = new System.Drawing.Size(84, 21);
            this.yaw_distance_boundary_m.TabIndex = 79;
            this.yaw_distance_boundary_m.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label22
            // 
            this.label22.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label22.Location = new System.Drawing.Point(6, 72);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(193, 23);
            this.label22.TabIndex = 78;
            this.label22.Text = "YAW_DISTANCE_BOUNDARY_M";
            // 
            // target_leader_dist_m
            // 
            this.target_leader_dist_m.Location = new System.Drawing.Point(224, 44);
            this.target_leader_dist_m.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.target_leader_dist_m.Name = "target_leader_dist_m";
            this.target_leader_dist_m.Size = new System.Drawing.Size(84, 21);
            this.target_leader_dist_m.TabIndex = 77;
            this.target_leader_dist_m.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            // 
            // label21
            // 
            this.label21.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label21.Location = new System.Drawing.Point(6, 49);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(175, 23);
            this.label21.TabIndex = 76;
            this.label21.Text = "TARGET_LEADER_DIST_M";
            // 
            // label20
            // 
            this.label20.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label20.Location = new System.Drawing.Point(6, 26);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(175, 23);
            this.label20.TabIndex = 74;
            this.label20.Text = "TARGET_TRAILER_SCALE";
            // 
            // target_trailer_scale
            // 
            this.target_trailer_scale.DecimalPlaces = 2;
            this.target_trailer_scale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.target_trailer_scale.Location = new System.Drawing.Point(224, 26);
            this.target_trailer_scale.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.target_trailer_scale.Name = "target_trailer_scale";
            this.target_trailer_scale.Size = new System.Drawing.Size(84, 21);
            this.target_trailer_scale.TabIndex = 75;
            this.target_trailer_scale.Value = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label19.Location = new System.Drawing.Point(3, 13);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(94, 20);
            this.label19.TabIndex = 73;
            this.label19.Text = "基本参数：";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label18.Location = new System.Drawing.Point(7, 187);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(94, 20);
            this.label18.TabIndex = 72;
            this.label18.Text = "高级参数：";
            // 
            // BUT_writePIDS
            // 
            this.BUT_writePIDS.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.BUT_writePIDS.Location = new System.Drawing.Point(962, 623);
            this.BUT_writePIDS.Name = "BUT_writePIDS";
            this.BUT_writePIDS.Size = new System.Drawing.Size(103, 19);
            this.BUT_writePIDS.TabIndex = 23;
            this.BUT_writePIDS.Text = "Write Params";
            this.BUT_writePIDS.UseVisualStyleBackColor = true;
            this.BUT_writePIDS.Click += new System.EventHandler(this.BUT_writeparam);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.thrust_FILT);
            this.groupBox3.Controls.Add(this.thrust_IMAX);
            this.groupBox3.Controls.Add(this.thrust_D);
            this.groupBox3.Controls.Add(this.thrust_I);
            this.groupBox3.Controls.Add(this.thrust_P);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Controls.Add(this.label14);
            this.groupBox3.Controls.Add(this.label15);
            this.groupBox3.Controls.Add(this.label16);
            this.groupBox3.Location = new System.Drawing.Point(882, 48);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(170, 136);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "SpdError To thrust";
            // 
            // thrust_FILT
            // 
            this.thrust_FILT.DecimalPlaces = 2;
            this.thrust_FILT.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.thrust_FILT.Location = new System.Drawing.Point(80, 110);
            this.thrust_FILT.Name = "thrust_FILT";
            this.thrust_FILT.Size = new System.Drawing.Size(84, 21);
            this.thrust_FILT.TabIndex = 13;
            // 
            // thrust_IMAX
            // 
            this.thrust_IMAX.DecimalPlaces = 2;
            this.thrust_IMAX.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.thrust_IMAX.Location = new System.Drawing.Point(80, 86);
            this.thrust_IMAX.Name = "thrust_IMAX";
            this.thrust_IMAX.Size = new System.Drawing.Size(84, 21);
            this.thrust_IMAX.TabIndex = 12;
            // 
            // thrust_D
            // 
            this.thrust_D.DecimalPlaces = 3;
            this.thrust_D.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.thrust_D.Location = new System.Drawing.Point(80, 61);
            this.thrust_D.Name = "thrust_D";
            this.thrust_D.Size = new System.Drawing.Size(84, 21);
            this.thrust_D.TabIndex = 11;
            // 
            // thrust_I
            // 
            this.thrust_I.DecimalPlaces = 3;
            this.thrust_I.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.thrust_I.Location = new System.Drawing.Point(80, 38);
            this.thrust_I.Name = "thrust_I";
            this.thrust_I.Size = new System.Drawing.Size(84, 21);
            this.thrust_I.TabIndex = 10;
            // 
            // thrust_P
            // 
            this.thrust_P.DecimalPlaces = 3;
            this.thrust_P.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.thrust_P.Location = new System.Drawing.Point(80, 16);
            this.thrust_P.Name = "thrust_P";
            this.thrust_P.Size = new System.Drawing.Size(84, 21);
            this.thrust_P.TabIndex = 9;
            // 
            // label11
            // 
            this.label11.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label11.Location = new System.Drawing.Point(6, 112);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(68, 13);
            this.label11.TabIndex = 8;
            this.label11.Text = "FILT";
            // 
            // label13
            // 
            this.label13.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label13.Location = new System.Drawing.Point(6, 63);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(10, 13);
            this.label13.TabIndex = 4;
            this.label13.Text = "D";
            // 
            // label14
            // 
            this.label14.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label14.Location = new System.Drawing.Point(6, 86);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(68, 13);
            this.label14.TabIndex = 6;
            this.label14.Text = "IMAX";
            // 
            // label15
            // 
            this.label15.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label15.Location = new System.Drawing.Point(6, 40);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(10, 13);
            this.label15.TabIndex = 2;
            this.label15.Text = "I";
            // 
            // label16
            // 
            this.label16.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label16.Location = new System.Drawing.Point(6, 16);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(14, 13);
            this.label16.TabIndex = 0;
            this.label16.Text = "P";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.yaw_FILT);
            this.groupBox2.Controls.Add(this.yaw_IMAX);
            this.groupBox2.Controls.Add(this.yaw_D);
            this.groupBox2.Controls.Add(this.yaw_I);
            this.groupBox2.Controls.Add(this.yaw_P);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Location = new System.Drawing.Point(451, 48);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(170, 136);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "YawError";
            // 
            // yaw_FILT
            // 
            this.yaw_FILT.DecimalPlaces = 2;
            this.yaw_FILT.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.yaw_FILT.Location = new System.Drawing.Point(80, 110);
            this.yaw_FILT.Name = "yaw_FILT";
            this.yaw_FILT.Size = new System.Drawing.Size(84, 21);
            this.yaw_FILT.TabIndex = 13;
            // 
            // yaw_IMAX
            // 
            this.yaw_IMAX.DecimalPlaces = 2;
            this.yaw_IMAX.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.yaw_IMAX.Location = new System.Drawing.Point(80, 86);
            this.yaw_IMAX.Name = "yaw_IMAX";
            this.yaw_IMAX.Size = new System.Drawing.Size(84, 21);
            this.yaw_IMAX.TabIndex = 12;
            // 
            // yaw_D
            // 
            this.yaw_D.DecimalPlaces = 2;
            this.yaw_D.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.yaw_D.Location = new System.Drawing.Point(80, 61);
            this.yaw_D.Name = "yaw_D";
            this.yaw_D.Size = new System.Drawing.Size(84, 21);
            this.yaw_D.TabIndex = 11;
            // 
            // yaw_I
            // 
            this.yaw_I.DecimalPlaces = 2;
            this.yaw_I.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.yaw_I.Location = new System.Drawing.Point(80, 38);
            this.yaw_I.Name = "yaw_I";
            this.yaw_I.Size = new System.Drawing.Size(84, 21);
            this.yaw_I.TabIndex = 10;
            // 
            // yaw_P
            // 
            this.yaw_P.DecimalPlaces = 2;
            this.yaw_P.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.yaw_P.Location = new System.Drawing.Point(80, 16);
            this.yaw_P.Name = "yaw_P";
            this.yaw_P.Size = new System.Drawing.Size(84, 21);
            this.yaw_P.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label6.Location = new System.Drawing.Point(6, 112);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(68, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "FILT";
            // 
            // label7
            // 
            this.label7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label7.Location = new System.Drawing.Point(6, 63);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(10, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "D";
            // 
            // label8
            // 
            this.label8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label8.Location = new System.Drawing.Point(6, 86);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(68, 13);
            this.label8.TabIndex = 6;
            this.label8.Text = "IMAX";
            // 
            // label9
            // 
            this.label9.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label9.Location = new System.Drawing.Point(6, 40);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(10, 13);
            this.label9.TabIndex = 2;
            this.label9.Text = "I";
            // 
            // label10
            // 
            this.label10.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label10.Location = new System.Drawing.Point(6, 16);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(14, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "P";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.pitch_FILT);
            this.groupBox1.Controls.Add(this.pitch_IMAX);
            this.groupBox1.Controls.Add(this.pitch_D);
            this.groupBox1.Controls.Add(this.pitch_I);
            this.groupBox1.Controls.Add(this.pitch_P);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Location = new System.Drawing.Point(223, 48);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(170, 136);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "AltError To Pitch";
            // 
            // pitch_FILT
            // 
            this.pitch_FILT.DecimalPlaces = 2;
            this.pitch_FILT.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.pitch_FILT.Location = new System.Drawing.Point(80, 110);
            this.pitch_FILT.Name = "pitch_FILT";
            this.pitch_FILT.Size = new System.Drawing.Size(84, 21);
            this.pitch_FILT.TabIndex = 13;
            // 
            // pitch_IMAX
            // 
            this.pitch_IMAX.DecimalPlaces = 2;
            this.pitch_IMAX.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.pitch_IMAX.Location = new System.Drawing.Point(80, 86);
            this.pitch_IMAX.Name = "pitch_IMAX";
            this.pitch_IMAX.Size = new System.Drawing.Size(84, 21);
            this.pitch_IMAX.TabIndex = 12;
            // 
            // pitch_D
            // 
            this.pitch_D.DecimalPlaces = 2;
            this.pitch_D.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.pitch_D.Location = new System.Drawing.Point(80, 61);
            this.pitch_D.Name = "pitch_D";
            this.pitch_D.Size = new System.Drawing.Size(84, 21);
            this.pitch_D.TabIndex = 11;
            // 
            // pitch_I
            // 
            this.pitch_I.DecimalPlaces = 2;
            this.pitch_I.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.pitch_I.Location = new System.Drawing.Point(80, 38);
            this.pitch_I.Name = "pitch_I";
            this.pitch_I.Size = new System.Drawing.Size(84, 21);
            this.pitch_I.TabIndex = 10;
            // 
            // pitch_P
            // 
            this.pitch_P.DecimalPlaces = 2;
            this.pitch_P.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.pitch_P.Location = new System.Drawing.Point(80, 16);
            this.pitch_P.Name = "pitch_P";
            this.pitch_P.Size = new System.Drawing.Size(84, 21);
            this.pitch_P.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(6, 112);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "FILT";
            // 
            // label2
            // 
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(6, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(10, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "D";
            // 
            // label3
            // 
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(6, 86);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "IMAX";
            // 
            // label4
            // 
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(6, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(10, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "I";
            // 
            // label5
            // 
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(6, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(14, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "P";
            // 
            // groupBox25
            // 
            this.groupBox25.Controls.Add(this.roll_FILT);
            this.groupBox25.Controls.Add(this.roll_IMAX);
            this.groupBox25.Controls.Add(this.roll_D);
            this.groupBox25.Controls.Add(this.roll_I);
            this.groupBox25.Controls.Add(this.roll_P);
            this.groupBox25.Controls.Add(this.label12);
            this.groupBox25.Controls.Add(this.label17);
            this.groupBox25.Controls.Add(this.label88);
            this.groupBox25.Controls.Add(this.label90);
            this.groupBox25.Controls.Add(this.label91);
            this.groupBox25.Location = new System.Drawing.Point(3, 48);
            this.groupBox25.Name = "groupBox25";
            this.groupBox25.Size = new System.Drawing.Size(170, 136);
            this.groupBox25.TabIndex = 6;
            this.groupBox25.TabStop = false;
            this.groupBox25.Text = "TurnError To Roll";
            // 
            // roll_FILT
            // 
            this.roll_FILT.DecimalPlaces = 2;
            this.roll_FILT.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.roll_FILT.Location = new System.Drawing.Point(81, 110);
            this.roll_FILT.Name = "roll_FILT";
            this.roll_FILT.Size = new System.Drawing.Size(84, 21);
            this.roll_FILT.TabIndex = 13;
            // 
            // roll_IMAX
            // 
            this.roll_IMAX.DecimalPlaces = 2;
            this.roll_IMAX.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.roll_IMAX.Location = new System.Drawing.Point(80, 86);
            this.roll_IMAX.Name = "roll_IMAX";
            this.roll_IMAX.Size = new System.Drawing.Size(84, 21);
            this.roll_IMAX.TabIndex = 12;
            // 
            // roll_D
            // 
            this.roll_D.DecimalPlaces = 2;
            this.roll_D.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.roll_D.Location = new System.Drawing.Point(80, 61);
            this.roll_D.Name = "roll_D";
            this.roll_D.Size = new System.Drawing.Size(84, 21);
            this.roll_D.TabIndex = 11;
            // 
            // roll_I
            // 
            this.roll_I.DecimalPlaces = 2;
            this.roll_I.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.roll_I.Location = new System.Drawing.Point(80, 38);
            this.roll_I.Name = "roll_I";
            this.roll_I.Size = new System.Drawing.Size(84, 21);
            this.roll_I.TabIndex = 10;
            // 
            // roll_P
            // 
            this.roll_P.DecimalPlaces = 2;
            this.roll_P.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.roll_P.Location = new System.Drawing.Point(80, 16);
            this.roll_P.Name = "roll_P";
            this.roll_P.Size = new System.Drawing.Size(84, 21);
            this.roll_P.TabIndex = 9;
            // 
            // label12
            // 
            this.label12.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label12.Location = new System.Drawing.Point(6, 112);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(68, 13);
            this.label12.TabIndex = 8;
            this.label12.Text = "FILT";
            // 
            // label17
            // 
            this.label17.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label17.Location = new System.Drawing.Point(6, 63);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(10, 13);
            this.label17.TabIndex = 4;
            this.label17.Text = "D";
            // 
            // label88
            // 
            this.label88.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label88.Location = new System.Drawing.Point(6, 86);
            this.label88.Name = "label88";
            this.label88.Size = new System.Drawing.Size(68, 13);
            this.label88.TabIndex = 6;
            this.label88.Text = "IMAX";
            // 
            // label90
            // 
            this.label90.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label90.Location = new System.Drawing.Point(6, 40);
            this.label90.Name = "label90";
            this.label90.Size = new System.Drawing.Size(10, 13);
            this.label90.TabIndex = 2;
            this.label90.Text = "I";
            // 
            // label91
            // 
            this.label91.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label91.Location = new System.Drawing.Point(6, 16);
            this.label91.Name = "label91";
            this.label91.Size = new System.Drawing.Size(14, 13);
            this.label91.TabIndex = 0;
            this.label91.Text = "P";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.splitContainer4);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(1234, 654);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "参数监视";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.splitContainer5);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.groupBox6);
            this.splitContainer4.Size = new System.Drawing.Size(1234, 654);
            this.splitContainer4.SplitterDistance = 410;
            this.splitContainer4.TabIndex = 0;
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.splitContainer6);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.zg1);
            this.splitContainer5.Size = new System.Drawing.Size(1234, 410);
            this.splitContainer5.SplitterDistance = 160;
            this.splitContainer5.TabIndex = 0;
            // 
            // splitContainer6
            // 
            this.splitContainer6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer6.Location = new System.Drawing.Point(0, 0);
            this.splitContainer6.Name = "splitContainer6";
            this.splitContainer6.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer6.Panel1
            // 
            this.splitContainer6.Panel1.Controls.Add(this.comboBox_chartshow);
            // 
            // splitContainer6.Panel2
            // 
            this.splitContainer6.Panel2.Controls.Add(this.variable_check);
            this.splitContainer6.Size = new System.Drawing.Size(160, 410);
            this.splitContainer6.SplitterDistance = 34;
            this.splitContainer6.TabIndex = 0;
            // 
            // comboBox_chartshow
            // 
            this.comboBox_chartshow.DataSource = this.bindingSource_2;
            this.comboBox_chartshow.FormattingEnabled = true;
            this.comboBox_chartshow.Location = new System.Drawing.Point(6, 3);
            this.comboBox_chartshow.Name = "comboBox_chartshow";
            this.comboBox_chartshow.Size = new System.Drawing.Size(118, 20);
            this.comboBox_chartshow.TabIndex = 7;
            // 
            // variable_check
            // 
            this.variable_check.AutoScroll = true;
            this.variable_check.Dock = System.Windows.Forms.DockStyle.Fill;
            this.variable_check.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.variable_check.Location = new System.Drawing.Point(0, 0);
            this.variable_check.Name = "variable_check";
            this.variable_check.Size = new System.Drawing.Size(160, 372);
            this.variable_check.TabIndex = 0;
            this.variable_check.WrapContents = false;
            // 
            // zg1
            // 
            this.zg1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zg1.Location = new System.Drawing.Point(0, 0);
            this.zg1.Name = "zg1";
            this.zg1.ScrollGrace = 0D;
            this.zg1.ScrollMaxX = 0D;
            this.zg1.ScrollMaxY = 0D;
            this.zg1.ScrollMaxY2 = 0D;
            this.zg1.ScrollMinX = 0D;
            this.zg1.ScrollMinY = 0D;
            this.zg1.ScrollMinY2 = 0D;
            this.zg1.Size = new System.Drawing.Size(1070, 410);
            this.zg1.TabIndex = 0;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.groupBox13);
            this.groupBox6.Controls.Add(this.groupBox12);
            this.groupBox6.Controls.Add(this.groupBox11);
            this.groupBox6.Controls.Add(this.groupBox7);
            this.groupBox6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox6.Location = new System.Drawing.Point(0, 0);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(1234, 240);
            this.groupBox6.TabIndex = 0;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "其他参数";
            // 
            // groupBox13
            // 
            this.groupBox13.Controls.Add(this.button1);
            this.groupBox13.Controls.Add(this.label_maybecollisiontimes);
            this.groupBox13.Controls.Add(this.label83);
            this.groupBox13.Controls.Add(this.label79);
            this.groupBox13.Controls.Add(this.label_totalavoidancecraftid);
            this.groupBox13.Controls.Add(this.label82);
            this.groupBox13.Controls.Add(this.label_avoidancecraftcounts);
            this.groupBox13.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox13.Location = new System.Drawing.Point(995, 15);
            this.groupBox13.Name = "groupBox13";
            this.groupBox13.Size = new System.Drawing.Size(233, 215);
            this.groupBox13.TabIndex = 10;
            this.groupBox13.TabStop = false;
            this.groupBox13.Text = "avoidance detect";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(152, 125);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 26);
            this.button1.TabIndex = 12;
            this.button1.Text = "清零";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.CollisionTimesReset_Click);
            // 
            // label_maybecollisiontimes
            // 
            this.label_maybecollisiontimes.BackColor = System.Drawing.Color.Transparent;
            this.label_maybecollisiontimes.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_maybecollisiontimes.ForeColor = System.Drawing.Color.Red;
            this.label_maybecollisiontimes.Location = new System.Drawing.Point(16, 125);
            this.label_maybecollisiontimes.Name = "label_maybecollisiontimes";
            this.label_maybecollisiontimes.Size = new System.Drawing.Size(131, 26);
            this.label_maybecollisiontimes.TabIndex = 11;
            this.label_maybecollisiontimes.Tag = "custom";
            this.label_maybecollisiontimes.Text = "0";
            this.label_maybecollisiontimes.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label83
            // 
            this.label83.AutoSize = true;
            this.label83.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label83.Location = new System.Drawing.Point(7, 102);
            this.label83.Name = "label83";
            this.label83.Size = new System.Drawing.Size(112, 15);
            this.label83.TabIndex = 10;
            this.label83.Text = "疑似避障次数:";
            // 
            // label79
            // 
            this.label79.AutoSize = true;
            this.label79.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label79.Location = new System.Drawing.Point(7, 33);
            this.label79.Name = "label79";
            this.label79.Size = new System.Drawing.Size(112, 15);
            this.label79.TabIndex = 3;
            this.label79.Text = "避障飞机数量:";
            // 
            // label_totalavoidancecraftid
            // 
            this.label_totalavoidancecraftid.BackColor = System.Drawing.Color.Transparent;
            this.label_totalavoidancecraftid.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_totalavoidancecraftid.ForeColor = System.Drawing.Color.Red;
            this.label_totalavoidancecraftid.Location = new System.Drawing.Point(7, 77);
            this.label_totalavoidancecraftid.Name = "label_totalavoidancecraftid";
            this.label_totalavoidancecraftid.Size = new System.Drawing.Size(226, 19);
            this.label_totalavoidancecraftid.TabIndex = 8;
            this.label_totalavoidancecraftid.Tag = "custom";
            this.label_totalavoidancecraftid.Text = "0";
            this.label_totalavoidancecraftid.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label82
            // 
            this.label82.AutoSize = true;
            this.label82.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label82.Location = new System.Drawing.Point(7, 55);
            this.label82.Name = "label82";
            this.label82.Size = new System.Drawing.Size(162, 15);
            this.label82.TabIndex = 9;
            this.label82.Text = "避障飞机ID汇总如下:";
            // 
            // label_avoidancecraftcounts
            // 
            this.label_avoidancecraftcounts.BackColor = System.Drawing.Color.Transparent;
            this.label_avoidancecraftcounts.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_avoidancecraftcounts.ForeColor = System.Drawing.Color.Red;
            this.label_avoidancecraftcounts.Location = new System.Drawing.Point(163, 33);
            this.label_avoidancecraftcounts.Name = "label_avoidancecraftcounts";
            this.label_avoidancecraftcounts.Size = new System.Drawing.Size(64, 19);
            this.label_avoidancecraftcounts.TabIndex = 7;
            this.label_avoidancecraftcounts.Tag = "custom";
            this.label_avoidancecraftcounts.Text = "0";
            // 
            // groupBox12
            // 
            this.groupBox12.Controls.Add(this.label_avoidancetargetalt2);
            this.groupBox12.Controls.Add(this.label_avoidancecraftid2);
            this.groupBox12.Controls.Add(this.label_obstacleid2);
            this.groupBox12.Controls.Add(this.label_craftdisttoobstacle2);
            this.groupBox12.Controls.Add(this.label_avoidanceeastoffset2);
            this.groupBox12.Controls.Add(this.label_avoidancenorthoffset2);
            this.groupBox12.Controls.Add(this.label73);
            this.groupBox12.Controls.Add(this.label74);
            this.groupBox12.Controls.Add(this.label75);
            this.groupBox12.Controls.Add(this.label76);
            this.groupBox12.Controls.Add(this.label77);
            this.groupBox12.Controls.Add(this.label78);
            this.groupBox12.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox12.ForeColor = System.Drawing.SystemColors.ControlText;
            this.groupBox12.Location = new System.Drawing.Point(689, 15);
            this.groupBox12.Name = "groupBox12";
            this.groupBox12.Size = new System.Drawing.Size(284, 215);
            this.groupBox12.TabIndex = 2;
            this.groupBox12.TabStop = false;
            this.groupBox12.Text = "avoidance state2";
            // 
            // label_avoidancetargetalt2
            // 
            this.label_avoidancetargetalt2.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_avoidancetargetalt2.ForeColor = System.Drawing.Color.Red;
            this.label_avoidancetargetalt2.Location = new System.Drawing.Point(214, 143);
            this.label_avoidancetargetalt2.Name = "label_avoidancetargetalt2";
            this.label_avoidancetargetalt2.Size = new System.Drawing.Size(64, 19);
            this.label_avoidancetargetalt2.TabIndex = 11;
            this.label_avoidancetargetalt2.Tag = "custom";
            this.label_avoidancetargetalt2.Text = "0";
            // 
            // label_avoidancecraftid2
            // 
            this.label_avoidancecraftid2.BackColor = System.Drawing.Color.Transparent;
            this.label_avoidancecraftid2.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_avoidancecraftid2.ForeColor = System.Drawing.Color.Red;
            this.label_avoidancecraftid2.Location = new System.Drawing.Point(214, 33);
            this.label_avoidancecraftid2.Name = "label_avoidancecraftid2";
            this.label_avoidancecraftid2.Size = new System.Drawing.Size(64, 19);
            this.label_avoidancecraftid2.TabIndex = 6;
            this.label_avoidancecraftid2.Tag = "custom";
            this.label_avoidancecraftid2.Text = "0";
            // 
            // label_obstacleid2
            // 
            this.label_obstacleid2.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_obstacleid2.ForeColor = System.Drawing.Color.Red;
            this.label_obstacleid2.Location = new System.Drawing.Point(214, 55);
            this.label_obstacleid2.Name = "label_obstacleid2";
            this.label_obstacleid2.Size = new System.Drawing.Size(64, 19);
            this.label_obstacleid2.TabIndex = 8;
            this.label_obstacleid2.Tag = "custom";
            this.label_obstacleid2.Text = "0";
            // 
            // label_craftdisttoobstacle2
            // 
            this.label_craftdisttoobstacle2.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_craftdisttoobstacle2.ForeColor = System.Drawing.Color.Red;
            this.label_craftdisttoobstacle2.Location = new System.Drawing.Point(214, 77);
            this.label_craftdisttoobstacle2.Name = "label_craftdisttoobstacle2";
            this.label_craftdisttoobstacle2.Size = new System.Drawing.Size(64, 19);
            this.label_craftdisttoobstacle2.TabIndex = 7;
            this.label_craftdisttoobstacle2.Tag = "custom";
            this.label_craftdisttoobstacle2.Text = "∞";
            // 
            // label_avoidanceeastoffset2
            // 
            this.label_avoidanceeastoffset2.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_avoidanceeastoffset2.ForeColor = System.Drawing.Color.Red;
            this.label_avoidanceeastoffset2.Location = new System.Drawing.Point(214, 99);
            this.label_avoidanceeastoffset2.Name = "label_avoidanceeastoffset2";
            this.label_avoidanceeastoffset2.Size = new System.Drawing.Size(64, 19);
            this.label_avoidanceeastoffset2.TabIndex = 9;
            this.label_avoidanceeastoffset2.Tag = "custom";
            this.label_avoidanceeastoffset2.Text = "0";
            // 
            // label_avoidancenorthoffset2
            // 
            this.label_avoidancenorthoffset2.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_avoidancenorthoffset2.ForeColor = System.Drawing.Color.Red;
            this.label_avoidancenorthoffset2.Location = new System.Drawing.Point(214, 121);
            this.label_avoidancenorthoffset2.Name = "label_avoidancenorthoffset2";
            this.label_avoidancenorthoffset2.Size = new System.Drawing.Size(64, 19);
            this.label_avoidancenorthoffset2.TabIndex = 10;
            this.label_avoidancenorthoffset2.Tag = "custom";
            this.label_avoidancenorthoffset2.Text = "0";
            // 
            // label73
            // 
            this.label73.AutoSize = true;
            this.label73.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label73.Location = new System.Drawing.Point(6, 148);
            this.label73.Name = "label73";
            this.label73.Size = new System.Drawing.Size(112, 15);
            this.label73.TabIndex = 5;
            this.label73.Text = "避障期望高度:";
            // 
            // label74
            // 
            this.label74.AutoSize = true;
            this.label74.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label74.Location = new System.Drawing.Point(6, 125);
            this.label74.Name = "label74";
            this.label74.Size = new System.Drawing.Size(112, 15);
            this.label74.TabIndex = 4;
            this.label74.Text = "避障北向输出:";
            // 
            // label75
            // 
            this.label75.AutoSize = true;
            this.label75.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label75.Location = new System.Drawing.Point(6, 102);
            this.label75.Name = "label75";
            this.label75.Size = new System.Drawing.Size(112, 15);
            this.label75.TabIndex = 3;
            this.label75.Text = "避障东向输出:";
            // 
            // label76
            // 
            this.label76.AutoSize = true;
            this.label76.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label76.Location = new System.Drawing.Point(6, 79);
            this.label76.Name = "label76";
            this.label76.Size = new System.Drawing.Size(112, 15);
            this.label76.TabIndex = 2;
            this.label76.Text = "与障碍物距离:";
            // 
            // label77
            // 
            this.label77.AutoSize = true;
            this.label77.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label77.Location = new System.Drawing.Point(6, 56);
            this.label77.Name = "label77";
            this.label77.Size = new System.Drawing.Size(82, 15);
            this.label77.TabIndex = 1;
            this.label77.Text = "障碍物ID:";
            // 
            // label78
            // 
            this.label78.AutoSize = true;
            this.label78.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label78.Location = new System.Drawing.Point(6, 33);
            this.label78.Name = "label78";
            this.label78.Size = new System.Drawing.Size(66, 15);
            this.label78.TabIndex = 0;
            this.label78.Text = "飞机ID:";
            // 
            // groupBox11
            // 
            this.groupBox11.Controls.Add(this.label_avoidancetargetalt1);
            this.groupBox11.Controls.Add(this.label_avoidancecraftid1);
            this.groupBox11.Controls.Add(this.label_obstacleid1);
            this.groupBox11.Controls.Add(this.label_craftdisttoobstacle1);
            this.groupBox11.Controls.Add(this.label_avoidanceeastoffset1);
            this.groupBox11.Controls.Add(this.label_avoidancenorthoffset1);
            this.groupBox11.Controls.Add(this.label61);
            this.groupBox11.Controls.Add(this.label62);
            this.groupBox11.Controls.Add(this.label63);
            this.groupBox11.Controls.Add(this.label64);
            this.groupBox11.Controls.Add(this.label65);
            this.groupBox11.Controls.Add(this.label66);
            this.groupBox11.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox11.ForeColor = System.Drawing.SystemColors.ControlText;
            this.groupBox11.Location = new System.Drawing.Point(388, 15);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Size = new System.Drawing.Size(284, 215);
            this.groupBox11.TabIndex = 1;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = "avoidance state1";
            // 
            // label_avoidancetargetalt1
            // 
            this.label_avoidancetargetalt1.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_avoidancetargetalt1.ForeColor = System.Drawing.Color.Red;
            this.label_avoidancetargetalt1.Location = new System.Drawing.Point(214, 143);
            this.label_avoidancetargetalt1.Name = "label_avoidancetargetalt1";
            this.label_avoidancetargetalt1.Size = new System.Drawing.Size(64, 19);
            this.label_avoidancetargetalt1.TabIndex = 11;
            this.label_avoidancetargetalt1.Tag = "custom";
            this.label_avoidancetargetalt1.Text = "0";
            // 
            // label_avoidancecraftid1
            // 
            this.label_avoidancecraftid1.BackColor = System.Drawing.Color.Transparent;
            this.label_avoidancecraftid1.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_avoidancecraftid1.ForeColor = System.Drawing.Color.Red;
            this.label_avoidancecraftid1.Location = new System.Drawing.Point(214, 33);
            this.label_avoidancecraftid1.Name = "label_avoidancecraftid1";
            this.label_avoidancecraftid1.Size = new System.Drawing.Size(64, 19);
            this.label_avoidancecraftid1.TabIndex = 6;
            this.label_avoidancecraftid1.Tag = "custom";
            this.label_avoidancecraftid1.Text = "0";
            // 
            // label_obstacleid1
            // 
            this.label_obstacleid1.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_obstacleid1.ForeColor = System.Drawing.Color.Red;
            this.label_obstacleid1.Location = new System.Drawing.Point(214, 55);
            this.label_obstacleid1.Name = "label_obstacleid1";
            this.label_obstacleid1.Size = new System.Drawing.Size(64, 19);
            this.label_obstacleid1.TabIndex = 8;
            this.label_obstacleid1.Tag = "custom";
            this.label_obstacleid1.Text = "0";
            // 
            // label_craftdisttoobstacle1
            // 
            this.label_craftdisttoobstacle1.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_craftdisttoobstacle1.ForeColor = System.Drawing.Color.Red;
            this.label_craftdisttoobstacle1.Location = new System.Drawing.Point(214, 77);
            this.label_craftdisttoobstacle1.Name = "label_craftdisttoobstacle1";
            this.label_craftdisttoobstacle1.Size = new System.Drawing.Size(64, 19);
            this.label_craftdisttoobstacle1.TabIndex = 7;
            this.label_craftdisttoobstacle1.Tag = "custom";
            this.label_craftdisttoobstacle1.Text = "∞";
            // 
            // label_avoidanceeastoffset1
            // 
            this.label_avoidanceeastoffset1.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_avoidanceeastoffset1.ForeColor = System.Drawing.Color.Red;
            this.label_avoidanceeastoffset1.Location = new System.Drawing.Point(214, 99);
            this.label_avoidanceeastoffset1.Name = "label_avoidanceeastoffset1";
            this.label_avoidanceeastoffset1.Size = new System.Drawing.Size(64, 19);
            this.label_avoidanceeastoffset1.TabIndex = 9;
            this.label_avoidanceeastoffset1.Tag = "custom";
            this.label_avoidanceeastoffset1.Text = "0";
            // 
            // label_avoidancenorthoffset1
            // 
            this.label_avoidancenorthoffset1.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_avoidancenorthoffset1.ForeColor = System.Drawing.Color.Red;
            this.label_avoidancenorthoffset1.Location = new System.Drawing.Point(214, 121);
            this.label_avoidancenorthoffset1.Name = "label_avoidancenorthoffset1";
            this.label_avoidancenorthoffset1.Size = new System.Drawing.Size(64, 19);
            this.label_avoidancenorthoffset1.TabIndex = 10;
            this.label_avoidancenorthoffset1.Tag = "custom";
            this.label_avoidancenorthoffset1.Text = "0";
            // 
            // label61
            // 
            this.label61.AutoSize = true;
            this.label61.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label61.Location = new System.Drawing.Point(6, 148);
            this.label61.Name = "label61";
            this.label61.Size = new System.Drawing.Size(112, 15);
            this.label61.TabIndex = 5;
            this.label61.Text = "避障期望高度:";
            // 
            // label62
            // 
            this.label62.AutoSize = true;
            this.label62.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label62.Location = new System.Drawing.Point(6, 125);
            this.label62.Name = "label62";
            this.label62.Size = new System.Drawing.Size(112, 15);
            this.label62.TabIndex = 4;
            this.label62.Text = "避障北向输出:";
            // 
            // label63
            // 
            this.label63.AutoSize = true;
            this.label63.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label63.Location = new System.Drawing.Point(6, 102);
            this.label63.Name = "label63";
            this.label63.Size = new System.Drawing.Size(112, 15);
            this.label63.TabIndex = 3;
            this.label63.Text = "避障东向输出:";
            // 
            // label64
            // 
            this.label64.AutoSize = true;
            this.label64.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label64.Location = new System.Drawing.Point(6, 79);
            this.label64.Name = "label64";
            this.label64.Size = new System.Drawing.Size(112, 15);
            this.label64.TabIndex = 2;
            this.label64.Text = "与障碍物距离:";
            // 
            // label65
            // 
            this.label65.AutoSize = true;
            this.label65.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label65.Location = new System.Drawing.Point(6, 56);
            this.label65.Name = "label65";
            this.label65.Size = new System.Drawing.Size(82, 15);
            this.label65.TabIndex = 1;
            this.label65.Text = "障碍物ID:";
            // 
            // label66
            // 
            this.label66.AutoSize = true;
            this.label66.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label66.Location = new System.Drawing.Point(6, 33);
            this.label66.Name = "label66";
            this.label66.Size = new System.Drawing.Size(66, 15);
            this.label66.TabIndex = 0;
            this.label66.Text = "飞机ID:";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.label_lowaltcount);
            this.groupBox7.Controls.Add(this.label_armcount);
            this.groupBox7.Controls.Add(this.label_heathlinkcount);
            this.groupBox7.Controls.Add(this.label_guidedcount);
            this.groupBox7.Controls.Add(this.label_connectcount);
            this.groupBox7.Controls.Add(this.label_lowspeedcount);
            this.groupBox7.Controls.Add(this.label34);
            this.groupBox7.Controls.Add(this.label33);
            this.groupBox7.Controls.Add(this.label32);
            this.groupBox7.Controls.Add(this.label31);
            this.groupBox7.Controls.Add(this.label30);
            this.groupBox7.Controls.Add(this.label29);
            this.groupBox7.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.groupBox7.Location = new System.Drawing.Point(81, 15);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(284, 215);
            this.groupBox7.TabIndex = 0;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "formation state";
            // 
            // label_lowaltcount
            // 
            this.label_lowaltcount.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_lowaltcount.ForeColor = System.Drawing.Color.Red;
            this.label_lowaltcount.Location = new System.Drawing.Point(214, 143);
            this.label_lowaltcount.Name = "label_lowaltcount";
            this.label_lowaltcount.Size = new System.Drawing.Size(64, 19);
            this.label_lowaltcount.TabIndex = 11;
            this.label_lowaltcount.Tag = "custom";
            this.label_lowaltcount.Text = "0";
            // 
            // label_armcount
            // 
            this.label_armcount.BackColor = System.Drawing.Color.Transparent;
            this.label_armcount.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_armcount.ForeColor = System.Drawing.Color.Red;
            this.label_armcount.Location = new System.Drawing.Point(214, 33);
            this.label_armcount.Name = "label_armcount";
            this.label_armcount.Size = new System.Drawing.Size(64, 19);
            this.label_armcount.TabIndex = 6;
            this.label_armcount.Tag = "custom";
            this.label_armcount.Text = "0";
            // 
            // label_heathlinkcount
            // 
            this.label_heathlinkcount.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_heathlinkcount.ForeColor = System.Drawing.Color.Red;
            this.label_heathlinkcount.Location = new System.Drawing.Point(214, 55);
            this.label_heathlinkcount.Name = "label_heathlinkcount";
            this.label_heathlinkcount.Size = new System.Drawing.Size(64, 19);
            this.label_heathlinkcount.TabIndex = 8;
            this.label_heathlinkcount.Tag = "custom";
            this.label_heathlinkcount.Text = "0";
            // 
            // label_guidedcount
            // 
            this.label_guidedcount.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_guidedcount.ForeColor = System.Drawing.Color.Red;
            this.label_guidedcount.Location = new System.Drawing.Point(214, 77);
            this.label_guidedcount.Name = "label_guidedcount";
            this.label_guidedcount.Size = new System.Drawing.Size(64, 19);
            this.label_guidedcount.TabIndex = 7;
            this.label_guidedcount.Tag = "custom";
            this.label_guidedcount.Text = "0";
            // 
            // label_connectcount
            // 
            this.label_connectcount.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_connectcount.ForeColor = System.Drawing.Color.Red;
            this.label_connectcount.Location = new System.Drawing.Point(214, 99);
            this.label_connectcount.Name = "label_connectcount";
            this.label_connectcount.Size = new System.Drawing.Size(64, 19);
            this.label_connectcount.TabIndex = 9;
            this.label_connectcount.Tag = "custom";
            this.label_connectcount.Text = "0";
            // 
            // label_lowspeedcount
            // 
            this.label_lowspeedcount.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_lowspeedcount.ForeColor = System.Drawing.Color.Red;
            this.label_lowspeedcount.Location = new System.Drawing.Point(214, 121);
            this.label_lowspeedcount.Name = "label_lowspeedcount";
            this.label_lowspeedcount.Size = new System.Drawing.Size(64, 19);
            this.label_lowspeedcount.TabIndex = 10;
            this.label_lowspeedcount.Tag = "custom";
            this.label_lowspeedcount.Text = "0";
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label34.Location = new System.Drawing.Point(6, 148);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(128, 15);
            this.label34.TabIndex = 5;
            this.label34.Text = "低高度飞机数量:";
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label33.Location = new System.Drawing.Point(6, 125);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(112, 15);
            this.label33.TabIndex = 4;
            this.label33.Text = "低速飞机数量:";
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label32.Location = new System.Drawing.Point(6, 102);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(112, 15);
            this.label32.TabIndex = 3;
            this.label32.Text = "连接飞机数量:";
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label31.Location = new System.Drawing.Point(6, 79);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(134, 15);
            this.label31.TabIndex = 2;
            this.label31.Text = "guided飞机数量:";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label30.Location = new System.Drawing.Point(6, 56);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(144, 15);
            this.label30.TabIndex = 1;
            this.label30.Text = "良好通信飞机数量:";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label29.Location = new System.Drawing.Point(6, 33);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(112, 15);
            this.label29.TabIndex = 0;
            this.label29.Text = "解锁飞机数量:";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.splitContainer1);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(1234, 654);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "队形变换";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(6, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1228, 651);
            this.splitContainer1.SplitterDistance = 195;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.listBox_formationShape);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.myButton3);
            this.splitContainer3.Panel2.Controls.Add(this.label28);
            this.splitContainer3.Panel2.Controls.Add(this.comboBox_formation_select);
            this.splitContainer3.Panel2.Controls.Add(this.myButton2);
            this.splitContainer3.Panel2.Controls.Add(this.myButton1);
            this.splitContainer3.Panel2.Controls.Add(this.textBox_filepath);
            this.splitContainer3.Size = new System.Drawing.Size(195, 651);
            this.splitContainer3.SplitterDistance = 430;
            this.splitContainer3.TabIndex = 0;
            // 
            // listBox_formationShape
            // 
            this.listBox_formationShape.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox_formationShape.FormattingEnabled = true;
            this.listBox_formationShape.ItemHeight = 12;
            this.listBox_formationShape.Location = new System.Drawing.Point(0, 0);
            this.listBox_formationShape.Name = "listBox_formationShape";
            this.listBox_formationShape.Size = new System.Drawing.Size(195, 430);
            this.listBox_formationShape.TabIndex = 0;
            this.listBox_formationShape.SelectedIndexChanged += new System.EventHandler(this.FormationShape_SelectedChanged);
            this.listBox_formationShape.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listbox_MouseUp);
            // 
            // myButton3
            // 
            this.myButton3.Location = new System.Drawing.Point(106, 34);
            this.myButton3.Name = "myButton3";
            this.myButton3.Size = new System.Drawing.Size(88, 23);
            this.myButton3.TabIndex = 9;
            this.myButton3.Text = "保存队形";
            this.myButton3.UseVisualStyleBackColor = true;
            this.myButton3.Click += new System.EventHandler(this.BUT_SaveFormationShapeFile);
            // 
            // label28
            // 
            this.label28.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label28.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label28.Location = new System.Drawing.Point(107, 89);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(91, 22);
            this.label28.TabIndex = 8;
            this.label28.Text = "选择已有队形";
            // 
            // comboBox_formation_select
            // 
            this.comboBox_formation_select.FormattingEnabled = true;
            this.comboBox_formation_select.Items.AddRange(new object[] {
            "大雁形",
            "竖一字形",
            "横一字形",
            "三角形",
            "亠形",
            "下亠形",
            "横网形",
            "竖网形",
            "斜网形"});
            this.comboBox_formation_select.Location = new System.Drawing.Point(4, 86);
            this.comboBox_formation_select.Name = "comboBox_formation_select";
            this.comboBox_formation_select.Size = new System.Drawing.Size(97, 20);
            this.comboBox_formation_select.TabIndex = 3;
            this.comboBox_formation_select.SelectedIndexChanged += new System.EventHandler(this.cbB_formation_select_SelectedIndexChanged);
            // 
            // myButton2
            // 
            this.myButton2.Location = new System.Drawing.Point(107, 63);
            this.myButton2.Name = "myButton2";
            this.myButton2.Size = new System.Drawing.Size(88, 23);
            this.myButton2.TabIndex = 2;
            this.myButton2.Text = "创建队形";
            this.myButton2.UseVisualStyleBackColor = true;
            this.myButton2.Click += new System.EventHandler(this.BUT_CreateForamtionShape);
            // 
            // myButton1
            // 
            this.myButton1.Location = new System.Drawing.Point(4, 34);
            this.myButton1.Name = "myButton1";
            this.myButton1.Size = new System.Drawing.Size(88, 23);
            this.myButton1.TabIndex = 1;
            this.myButton1.Text = "加载队形";
            this.myButton1.UseVisualStyleBackColor = true;
            this.myButton1.Click += new System.EventHandler(this.BUT_openFormationShapeFile);
            // 
            // textBox_filepath
            // 
            this.textBox_filepath.Location = new System.Drawing.Point(1, 3);
            this.textBox_filepath.Name = "textBox_filepath";
            this.textBox_filepath.Size = new System.Drawing.Size(193, 21);
            this.textBox_filepath.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.checkBox_VLmode);
            this.splitContainer2.Panel1.Controls.Add(this.myButton18);
            this.splitContainer2.Panel1.Controls.Add(this.myButton_Transformation);
            this.splitContainer2.Panel1.Controls.Add(this.label27);
            this.splitContainer2.Panel1.Controls.Add(this.myButton_swarmspace);
            this.splitContainer2.Panel1.Controls.Add(this.textBox_swarmspace);
            this.splitContainer2.Panel1.Controls.Add(this.trackBar_swarmspace);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.grid2);
            this.splitContainer2.Size = new System.Drawing.Size(1029, 651);
            this.splitContainer2.SplitterDistance = 70;
            this.splitContainer2.TabIndex = 0;
            // 
            // checkBox_VLmode
            // 
            this.checkBox_VLmode.AutoSize = true;
            this.checkBox_VLmode.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.checkBox_VLmode.Location = new System.Drawing.Point(594, 28);
            this.checkBox_VLmode.Name = "checkBox_VLmode";
            this.checkBox_VLmode.Size = new System.Drawing.Size(66, 16);
            this.checkBox_VLmode.TabIndex = 10;
            this.checkBox_VLmode.Text = "VL_MODE";
            this.checkBox_VLmode.UseVisualStyleBackColor = true;
            // 
            // myButton18
            // 
            this.myButton18.Location = new System.Drawing.Point(685, 20);
            this.myButton18.Name = "myButton18";
            this.myButton18.Size = new System.Drawing.Size(142, 35);
            this.myButton18.TabIndex = 9;
            this.myButton18.Text = "预览队形变换";
            this.myButton18.UseVisualStyleBackColor = true;
            this.myButton18.Click += new System.EventHandler(this.BTN_PreviewForamtionTrans);
            // 
            // myButton_Transformation
            // 
            this.myButton_Transformation.Location = new System.Drawing.Point(854, 20);
            this.myButton_Transformation.Name = "myButton_Transformation";
            this.myButton_Transformation.Size = new System.Drawing.Size(142, 35);
            this.myButton_Transformation.TabIndex = 8;
            this.myButton_Transformation.Text = "执行队形变换";
            this.myButton_Transformation.UseVisualStyleBackColor = true;
            this.myButton_Transformation.Click += new System.EventHandler(this.myButton_Transformation_Click);
            // 
            // label27
            // 
            this.label27.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label27.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label27.Location = new System.Drawing.Point(0, 3);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(85, 22);
            this.label27.TabIndex = 7;
            this.label27.Text = "编队间距:";
            // 
            // myButton_swarmspace
            // 
            this.myButton_swarmspace.Location = new System.Drawing.Point(3, 31);
            this.myButton_swarmspace.Name = "myButton_swarmspace";
            this.myButton_swarmspace.Size = new System.Drawing.Size(123, 23);
            this.myButton_swarmspace.TabIndex = 2;
            this.myButton_swarmspace.Text = "输入";
            this.myButton_swarmspace.UseVisualStyleBackColor = true;
            this.myButton_swarmspace.Click += new System.EventHandler(this.BTN_SwarmSpace_Click);
            // 
            // textBox_swarmspace
            // 
            this.textBox_swarmspace.Location = new System.Drawing.Point(91, 3);
            this.textBox_swarmspace.Name = "textBox_swarmspace";
            this.textBox_swarmspace.Size = new System.Drawing.Size(35, 21);
            this.textBox_swarmspace.TabIndex = 1;
            this.textBox_swarmspace.Text = "60";
            // 
            // trackBar_swarmspace
            // 
            this.trackBar_swarmspace.LargeChange = 50;
            this.trackBar_swarmspace.Location = new System.Drawing.Point(144, 3);
            this.trackBar_swarmspace.Maximum = 200;
            this.trackBar_swarmspace.Minimum = 1;
            this.trackBar_swarmspace.Name = "trackBar_swarmspace";
            this.trackBar_swarmspace.Size = new System.Drawing.Size(422, 45);
            this.trackBar_swarmspace.TabIndex = 0;
            this.trackBar_swarmspace.Value = 60;
            this.trackBar_swarmspace.Scroll += new System.EventHandler(this.trackBar_Scroll);
            this.trackBar_swarmspace.ValueChanged += new System.EventHandler(this.trackBar_ValueChanged);
            // 
            // grid2
            // 
            this.grid2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid2.Location = new System.Drawing.Point(0, 0);
            this.grid2.Name = "grid2";
            this.grid2.Size = new System.Drawing.Size(1029, 577);
            this.grid2.TabIndex = 0;
            this.grid2.Vertical = false;
            this.grid2.UpdateOffsets += new MissionPlanner.Swarm.Grid.UpdateOffsetsEvent(this.grid2_UpdateOffsets);
            // 
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.splitContainer7);
            this.tabPage6.Location = new System.Drawing.Point(4, 22);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Size = new System.Drawing.Size(1234, 654);
            this.tabPage6.TabIndex = 5;
            this.tabPage6.Text = "视频显示";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // splitContainer7
            // 
            this.splitContainer7.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer7.Location = new System.Drawing.Point(0, 0);
            this.splitContainer7.Name = "splitContainer7";
            this.splitContainer7.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer7.Panel1
            // 
            this.splitContainer7.Panel1.Controls.Add(this.groupBox16);
            this.splitContainer7.Panel1.Controls.Add(this.groupBox15);
            this.splitContainer7.Panel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SC7Panel_MouseUP);
            // 
            // splitContainer7.Panel2
            // 
            this.splitContainer7.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer7.Size = new System.Drawing.Size(1234, 654);
            this.splitContainer7.SplitterDistance = 104;
            this.splitContainer7.TabIndex = 0;
            // 
            // groupBox16
            // 
            this.groupBox16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox16.Controls.Add(this.CMB_video2resolutions);
            this.groupBox16.Controls.Add(this.BUT_video2stop);
            this.groupBox16.Controls.Add(this.BUT_video2start);
            this.groupBox16.Controls.Add(this.CMB_video2sources);
            this.groupBox16.Controls.Add(this.label59);
            this.groupBox16.Controls.Add(this.label60);
            this.groupBox16.Location = new System.Drawing.Point(663, 3);
            this.groupBox16.Name = "groupBox16";
            this.groupBox16.Size = new System.Drawing.Size(545, 99);
            this.groupBox16.TabIndex = 1;
            this.groupBox16.TabStop = false;
            this.groupBox16.Text = "Video2";
            // 
            // CMB_video2resolutions
            // 
            this.CMB_video2resolutions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CMB_video2resolutions.FormattingEnabled = true;
            this.CMB_video2resolutions.Location = new System.Drawing.Point(114, 55);
            this.CMB_video2resolutions.Name = "CMB_video2resolutions";
            this.CMB_video2resolutions.Size = new System.Drawing.Size(408, 20);
            this.CMB_video2resolutions.TabIndex = 91;
            // 
            // BUT_video2stop
            // 
            this.BUT_video2stop.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.BUT_video2stop.Location = new System.Drawing.Point(447, 17);
            this.BUT_video2stop.Name = "BUT_video2stop";
            this.BUT_video2stop.Size = new System.Drawing.Size(75, 23);
            this.BUT_video2stop.TabIndex = 90;
            this.BUT_video2stop.Text = "Stop";
            this.BUT_video2stop.UseVisualStyleBackColor = true;
            this.BUT_video2stop.Click += new System.EventHandler(this.BUT_video2stop_Click);
            // 
            // BUT_video2start
            // 
            this.BUT_video2start.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.BUT_video2start.Location = new System.Drawing.Point(366, 17);
            this.BUT_video2start.Name = "BUT_video2start";
            this.BUT_video2start.Size = new System.Drawing.Size(75, 23);
            this.BUT_video2start.TabIndex = 89;
            this.BUT_video2start.Text = "Start";
            this.BUT_video2start.UseVisualStyleBackColor = true;
            this.BUT_video2start.Click += new System.EventHandler(this.BUT_video2start_Click);
            // 
            // CMB_video2sources
            // 
            this.CMB_video2sources.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CMB_video2sources.FormattingEnabled = true;
            this.CMB_video2sources.Location = new System.Drawing.Point(115, 17);
            this.CMB_video2sources.Name = "CMB_video2sources";
            this.CMB_video2sources.Size = new System.Drawing.Size(245, 20);
            this.CMB_video2sources.TabIndex = 88;
            this.CMB_video2sources.SelectedIndexChanged += new System.EventHandler(this.CMB_video2sources_SelectedIndexChanged);
            this.CMB_video2sources.Click += new System.EventHandler(this.CMB_video2sources_Click);
            // 
            // label59
            // 
            this.label59.AutoSize = true;
            this.label59.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label59.Location = new System.Drawing.Point(6, 59);
            this.label59.Name = "label59";
            this.label59.Size = new System.Drawing.Size(77, 12);
            this.label59.TabIndex = 87;
            this.label59.Text = "Video Format";
            // 
            // label60
            // 
            this.label60.AutoSize = true;
            this.label60.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label60.Location = new System.Drawing.Point(6, 21);
            this.label60.Name = "label60";
            this.label60.Size = new System.Drawing.Size(77, 12);
            this.label60.TabIndex = 75;
            this.label60.Text = "Video Device";
            // 
            // groupBox15
            // 
            this.groupBox15.Controls.Add(this.CMB_video1resolutions);
            this.groupBox15.Controls.Add(this.BUT_video1stop);
            this.groupBox15.Controls.Add(this.BUT_video1start);
            this.groupBox15.Controls.Add(this.CMB_video1sources);
            this.groupBox15.Controls.Add(this.label58);
            this.groupBox15.Controls.Add(this.label92);
            this.groupBox15.Location = new System.Drawing.Point(22, 3);
            this.groupBox15.Name = "groupBox15";
            this.groupBox15.Size = new System.Drawing.Size(545, 99);
            this.groupBox15.TabIndex = 0;
            this.groupBox15.TabStop = false;
            this.groupBox15.Text = "Video1";
            // 
            // CMB_video1resolutions
            // 
            this.CMB_video1resolutions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CMB_video1resolutions.FormattingEnabled = true;
            this.CMB_video1resolutions.Location = new System.Drawing.Point(114, 55);
            this.CMB_video1resolutions.Name = "CMB_video1resolutions";
            this.CMB_video1resolutions.Size = new System.Drawing.Size(408, 20);
            this.CMB_video1resolutions.TabIndex = 91;
            // 
            // BUT_video1stop
            // 
            this.BUT_video1stop.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.BUT_video1stop.Location = new System.Drawing.Point(447, 17);
            this.BUT_video1stop.Name = "BUT_video1stop";
            this.BUT_video1stop.Size = new System.Drawing.Size(75, 23);
            this.BUT_video1stop.TabIndex = 90;
            this.BUT_video1stop.Text = "Stop";
            this.BUT_video1stop.UseVisualStyleBackColor = true;
            this.BUT_video1stop.Click += new System.EventHandler(this.BUT_video1stop_Click);
            // 
            // BUT_video1start
            // 
            this.BUT_video1start.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.BUT_video1start.Location = new System.Drawing.Point(366, 17);
            this.BUT_video1start.Name = "BUT_video1start";
            this.BUT_video1start.Size = new System.Drawing.Size(75, 23);
            this.BUT_video1start.TabIndex = 89;
            this.BUT_video1start.Text = "Start";
            this.BUT_video1start.UseVisualStyleBackColor = true;
            this.BUT_video1start.Click += new System.EventHandler(this.BUT_video1start_Click);
            // 
            // CMB_video1sources
            // 
            this.CMB_video1sources.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CMB_video1sources.FormattingEnabled = true;
            this.CMB_video1sources.Location = new System.Drawing.Point(115, 17);
            this.CMB_video1sources.Name = "CMB_video1sources";
            this.CMB_video1sources.Size = new System.Drawing.Size(245, 20);
            this.CMB_video1sources.TabIndex = 88;
            this.CMB_video1sources.SelectedIndexChanged += new System.EventHandler(this.CMB_video1sources_SelectedIndexChanged);
            this.CMB_video1sources.Click += new System.EventHandler(this.CMB_video1sources_Click);
            // 
            // label58
            // 
            this.label58.AutoSize = true;
            this.label58.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label58.Location = new System.Drawing.Point(6, 59);
            this.label58.Name = "label58";
            this.label58.Size = new System.Drawing.Size(77, 12);
            this.label58.TabIndex = 87;
            this.label58.Text = "Video Format";
            // 
            // label92
            // 
            this.label92.AutoSize = true;
            this.label92.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label92.Location = new System.Drawing.Point(6, 21);
            this.label92.Name = "label92";
            this.label92.Size = new System.Drawing.Size(77, 12);
            this.label92.TabIndex = 75;
            this.label92.Text = "Video Device";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.userVideo1HudShow, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.userVideo2HudShow, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1234, 546);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // userVideo1HudShow
            // 
            this.userVideo1HudShow.BackColor = System.Drawing.Color.Black;
            this.userVideo1HudShow.bgimage = null;
            this.userVideo1HudShow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userVideo1HudShow.hudcolor = System.Drawing.Color.White;
            this.userVideo1HudShow.Location = new System.Drawing.Point(4, 3);
            this.userVideo1HudShow.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.userVideo1HudShow.Name = "userVideo1HudShow";
            this.userVideo1HudShow.Size = new System.Drawing.Size(609, 540);
            this.userVideo1HudShow.streamjpg = ((System.IO.MemoryStream)(resources.GetObject("userVideo1HudShow.streamjpg")));
            this.userVideo1HudShow.TabIndex = 0;
            this.userVideo1HudShow.VSync = false;
            this.userVideo1HudShow.DoubleClick += new System.EventHandler(this.video1_DoubleClick);
            // 
            // userVideo2HudShow
            // 
            this.userVideo2HudShow.BackColor = System.Drawing.Color.Black;
            this.userVideo2HudShow.bgimage = null;
            this.userVideo2HudShow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userVideo2HudShow.hudcolor = System.Drawing.Color.White;
            this.userVideo2HudShow.Location = new System.Drawing.Point(621, 3);
            this.userVideo2HudShow.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.userVideo2HudShow.Name = "userVideo2HudShow";
            this.userVideo2HudShow.Size = new System.Drawing.Size(609, 540);
            this.userVideo2HudShow.streamjpg = ((System.IO.MemoryStream)(resources.GetObject("userVideo2HudShow.streamjpg")));
            this.userVideo2HudShow.TabIndex = 1;
            this.userVideo2HudShow.VSync = false;
            this.userVideo2HudShow.DoubleClick += new System.EventHandler(this.video2_DoubleClick);
            // 
            // tabPage7
            // 
            this.tabPage7.Controls.Add(this.splitContainer8);
            this.tabPage7.Location = new System.Drawing.Point(4, 22);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Size = new System.Drawing.Size(1234, 654);
            this.tabPage7.TabIndex = 6;
            this.tabPage7.Text = "3D地形";
            this.tabPage7.UseVisualStyleBackColor = true;
            // 
            // splitContainer8
            // 
            this.splitContainer8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer8.Location = new System.Drawing.Point(0, 0);
            this.splitContainer8.Name = "splitContainer8";
            // 
            // splitContainer8.Panel1
            // 
            this.splitContainer8.Panel1.Controls.Add(this.myButton17);
            this.splitContainer8.Panel1.Controls.Add(this.BTN_Show3dMap);
            this.splitContainer8.Panel1.Controls.Add(this.CMB_3DMAP);
            this.splitContainer8.Panel1.Controls.Add(this.label67);
            this.splitContainer8.Size = new System.Drawing.Size(1234, 654);
            this.splitContainer8.SplitterDistance = 200;
            this.splitContainer8.TabIndex = 0;
            // 
            // myButton17
            // 
            this.myButton17.Location = new System.Drawing.Point(81, 67);
            this.myButton17.Name = "myButton17";
            this.myButton17.Size = new System.Drawing.Size(65, 23);
            this.myButton17.TabIndex = 79;
            this.myButton17.Text = "关闭";
            this.myButton17.UseVisualStyleBackColor = true;
            this.myButton17.Click += new System.EventHandler(this.BTN_3DmapClose);
            // 
            // BTN_Show3dMap
            // 
            this.BTN_Show3dMap.Location = new System.Drawing.Point(10, 67);
            this.BTN_Show3dMap.Name = "BTN_Show3dMap";
            this.BTN_Show3dMap.Size = new System.Drawing.Size(65, 23);
            this.BTN_Show3dMap.TabIndex = 78;
            this.BTN_Show3dMap.Text = "显示";
            this.BTN_Show3dMap.UseVisualStyleBackColor = true;
            this.BTN_Show3dMap.Click += new System.EventHandler(this.BTN_Show3dMap_Click);
            // 
            // CMB_3DMAP
            // 
            this.CMB_3DMAP.DataSource = this.bindingSource_2;
            this.CMB_3DMAP.FormattingEnabled = true;
            this.CMB_3DMAP.Location = new System.Drawing.Point(10, 38);
            this.CMB_3DMAP.Name = "CMB_3DMAP";
            this.CMB_3DMAP.Size = new System.Drawing.Size(118, 20);
            this.CMB_3DMAP.TabIndex = 77;
            // 
            // label67
            // 
            this.label67.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label67.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label67.Location = new System.Drawing.Point(6, 12);
            this.label67.Name = "label67";
            this.label67.Size = new System.Drawing.Size(150, 23);
            this.label67.TabIndex = 76;
            this.label67.Text = "选择对应编号:";
            // 
            // tabPage_voiceIdentification
            // 
            this.tabPage_voiceIdentification.Controls.Add(this.groupBox22);
            this.tabPage_voiceIdentification.Controls.Add(this.lbl_SpeechRecognition);
            this.tabPage_voiceIdentification.Controls.Add(this.groupBox21);
            this.tabPage_voiceIdentification.Controls.Add(this.myButton25);
            this.tabPage_voiceIdentification.Controls.Add(this.myButton23);
            this.tabPage_voiceIdentification.Location = new System.Drawing.Point(4, 22);
            this.tabPage_voiceIdentification.Name = "tabPage_voiceIdentification";
            this.tabPage_voiceIdentification.Size = new System.Drawing.Size(1234, 654);
            this.tabPage_voiceIdentification.TabIndex = 7;
            this.tabPage_voiceIdentification.Text = "语音识别";
            this.tabPage_voiceIdentification.UseVisualStyleBackColor = true;
            // 
            // groupBox22
            // 
            this.groupBox22.Controls.Add(this.label87);
            this.groupBox22.Controls.Add(this.lbl_fourthgrammer);
            this.groupBox22.Controls.Add(this.lbl_thirdgrammer);
            this.groupBox22.Controls.Add(this.lbl_secondgrammer);
            this.groupBox22.Controls.Add(this.lbl_firstgrammer);
            this.groupBox22.Location = new System.Drawing.Point(456, 3);
            this.groupBox22.Name = "groupBox22";
            this.groupBox22.Size = new System.Drawing.Size(775, 176);
            this.groupBox22.TabIndex = 82;
            this.groupBox22.TabStop = false;
            this.groupBox22.Text = "语法规则";
            // 
            // label87
            // 
            this.label87.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label87.Location = new System.Drawing.Point(6, 134);
            this.label87.Name = "label87";
            this.label87.Size = new System.Drawing.Size(763, 22);
            this.label87.TabIndex = 86;
            this.label87.Text = "示例:1号机左飞50厘米,1号机起飞";
            // 
            // lbl_fourthgrammer
            // 
            this.lbl_fourthgrammer.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_fourthgrammer.Location = new System.Drawing.Point(6, 109);
            this.lbl_fourthgrammer.Name = "lbl_fourthgrammer";
            this.lbl_fourthgrammer.Size = new System.Drawing.Size(763, 22);
            this.lbl_fourthgrammer.TabIndex = 85;
            this.lbl_fourthgrammer.Text = "四级语意(可缺省):最小值=1度;步长=1度;最大值=30度";
            // 
            // lbl_thirdgrammer
            // 
            this.lbl_thirdgrammer.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_thirdgrammer.Location = new System.Drawing.Point(6, 83);
            this.lbl_thirdgrammer.Name = "lbl_thirdgrammer";
            this.lbl_thirdgrammer.Size = new System.Drawing.Size(763, 22);
            this.lbl_thirdgrammer.TabIndex = 84;
            this.lbl_thirdgrammer.Text = "三级语意(可缺省):最小值=10厘米;步长=10厘米;最大值=500厘米";
            // 
            // lbl_secondgrammer
            // 
            this.lbl_secondgrammer.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_secondgrammer.Location = new System.Drawing.Point(6, 56);
            this.lbl_secondgrammer.Name = "lbl_secondgrammer";
            this.lbl_secondgrammer.Size = new System.Drawing.Size(763, 22);
            this.lbl_secondgrammer.TabIndex = 83;
            this.lbl_secondgrammer.Text = "二级语意:解锁 起飞 降落 定点 升高 下降 前进 后退 左飞 右飞 左偏航 右偏航";
            // 
            // lbl_firstgrammer
            // 
            this.lbl_firstgrammer.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_firstgrammer.Location = new System.Drawing.Point(6, 30);
            this.lbl_firstgrammer.Name = "lbl_firstgrammer";
            this.lbl_firstgrammer.Size = new System.Drawing.Size(763, 22);
            this.lbl_firstgrammer.TabIndex = 82;
            this.lbl_firstgrammer.Text = "一级语意:1号机";
            // 
            // lbl_SpeechRecognition
            // 
            this.lbl_SpeechRecognition.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_SpeechRecognition.Location = new System.Drawing.Point(226, 20);
            this.lbl_SpeechRecognition.Name = "lbl_SpeechRecognition";
            this.lbl_SpeechRecognition.Size = new System.Drawing.Size(111, 22);
            this.lbl_SpeechRecognition.TabIndex = 81;
            this.lbl_SpeechRecognition.Text = "识别状态:关闭";
            // 
            // groupBox21
            // 
            this.groupBox21.Controls.Add(this.txt_Identificateresult);
            this.groupBox21.Location = new System.Drawing.Point(3, 57);
            this.groupBox21.Name = "groupBox21";
            this.groupBox21.Size = new System.Drawing.Size(447, 122);
            this.groupBox21.TabIndex = 80;
            this.groupBox21.TabStop = false;
            this.groupBox21.Text = "识别信息";
            // 
            // txt_Identificateresult
            // 
            this.txt_Identificateresult.Location = new System.Drawing.Point(6, 17);
            this.txt_Identificateresult.Multiline = true;
            this.txt_Identificateresult.Name = "txt_Identificateresult";
            this.txt_Identificateresult.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txt_Identificateresult.Size = new System.Drawing.Size(432, 88);
            this.txt_Identificateresult.TabIndex = 0;
            // 
            // myButton25
            // 
            this.myButton25.Location = new System.Drawing.Point(124, 16);
            this.myButton25.Name = "myButton25";
            this.myButton25.Size = new System.Drawing.Size(75, 23);
            this.myButton25.TabIndex = 2;
            this.myButton25.Text = "关闭";
            this.myButton25.UseVisualStyleBackColor = true;
            this.myButton25.Click += new System.EventHandler(this.BTN_voiceIdentificationstop);
            // 
            // myButton23
            // 
            this.myButton23.Location = new System.Drawing.Point(18, 16);
            this.myButton23.Name = "myButton23";
            this.myButton23.Size = new System.Drawing.Size(75, 23);
            this.myButton23.TabIndex = 1;
            this.myButton23.Text = "开启";
            this.myButton23.UseVisualStyleBackColor = true;
            this.myButton23.Click += new System.EventHandler(this.BTN_voiceIdentificationstart);
            // 
            // tabPage_targetserach
            // 
            this.tabPage_targetserach.Controls.Add(this.splitContainer9);
            this.tabPage_targetserach.Location = new System.Drawing.Point(4, 22);
            this.tabPage_targetserach.Name = "tabPage_targetserach";
            this.tabPage_targetserach.Size = new System.Drawing.Size(1234, 654);
            this.tabPage_targetserach.TabIndex = 8;
            this.tabPage_targetserach.Text = "目标搜索";
            this.tabPage_targetserach.UseVisualStyleBackColor = true;
            // 
            // splitContainer9
            // 
            this.splitContainer9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer9.Location = new System.Drawing.Point(0, 0);
            this.splitContainer9.Name = "splitContainer9";
            // 
            // splitContainer9.Panel1
            // 
            this.splitContainer9.Panel1.Controls.Add(this.splitContainer10);
            // 
            // splitContainer9.Panel2
            // 
            this.splitContainer9.Panel2.Controls.Add(this.gMap);
            this.splitContainer9.Panel2.Controls.Add(this.track_zoom);
            this.splitContainer9.Size = new System.Drawing.Size(1234, 654);
            this.splitContainer9.SplitterDistance = 250;
            this.splitContainer9.TabIndex = 0;
            // 
            // splitContainer10
            // 
            this.splitContainer10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer10.Location = new System.Drawing.Point(0, 0);
            this.splitContainer10.Name = "splitContainer10";
            this.splitContainer10.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer10.Panel1
            // 
            this.splitContainer10.Panel1.Controls.Add(this.numericUpDown_overshot);
            this.splitContainer10.Panel1.Controls.Add(this.label96);
            this.splitContainer10.Panel1.Controls.Add(this.BTN_SearchAlt);
            this.splitContainer10.Panel1.Controls.Add(this.numericUpDown_searchalt);
            this.splitContainer10.Panel1.Controls.Add(this.label95);
            this.splitContainer10.Panel1.Controls.Add(this.groupBox23);
            this.splitContainer10.Panel1.Controls.Add(this.radioButton_targetsearch);
            this.splitContainer10.Panel1.Controls.Add(this.radioButton_tragettrack);
            this.splitContainer10.Panel1.Controls.Add(this.BTN_searchsimulation);
            this.splitContainer10.Panel1.Controls.Add(this.BTN_searchUpdate);
            this.splitContainer10.Panel1.Controls.Add(this.BTN_search);
            this.splitContainer10.Panel1.Controls.Add(this.lbl_targetfoundcounts);
            this.splitContainer10.Panel1.Controls.Add(this.label94);
            this.splitContainer10.Panel1.Controls.Add(this.lbl_serachareacounts);
            this.splitContainer10.Panel1.Controls.Add(this.label89);
            // 
            // splitContainer10.Panel2
            // 
            this.splitContainer10.Panel2.Controls.Add(this.targetsFound);
            this.splitContainer10.Size = new System.Drawing.Size(250, 654);
            this.splitContainer10.SplitterDistance = 302;
            this.splitContainer10.TabIndex = 0;
            // 
            // numericUpDown_overshot
            // 
            this.numericUpDown_overshot.Location = new System.Drawing.Point(81, 61);
            this.numericUpDown_overshot.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDown_overshot.Name = "numericUpDown_overshot";
            this.numericUpDown_overshot.Size = new System.Drawing.Size(58, 21);
            this.numericUpDown_overshot.TabIndex = 85;
            this.numericUpDown_overshot.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // label96
            // 
            this.label96.AutoSize = true;
            this.label96.Location = new System.Drawing.Point(6, 66);
            this.label96.Name = "label96";
            this.label96.Size = new System.Drawing.Size(59, 12);
            this.label96.TabIndex = 84;
            this.label96.Text = "过冲距离:";
            // 
            // BTN_SearchAlt
            // 
            this.BTN_SearchAlt.Location = new System.Drawing.Point(145, 85);
            this.BTN_SearchAlt.Name = "BTN_SearchAlt";
            this.BTN_SearchAlt.Size = new System.Drawing.Size(75, 23);
            this.BTN_SearchAlt.TabIndex = 83;
            this.BTN_SearchAlt.Text = "设置";
            this.BTN_SearchAlt.UseVisualStyleBackColor = true;
            this.BTN_SearchAlt.Click += new System.EventHandler(this.BTN_SearchAlt_Click);
            // 
            // numericUpDown_searchalt
            // 
            this.numericUpDown_searchalt.Location = new System.Drawing.Point(81, 84);
            this.numericUpDown_searchalt.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDown_searchalt.Name = "numericUpDown_searchalt";
            this.numericUpDown_searchalt.Size = new System.Drawing.Size(58, 21);
            this.numericUpDown_searchalt.TabIndex = 82;
            this.numericUpDown_searchalt.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // label95
            // 
            this.label95.AutoSize = true;
            this.label95.Location = new System.Drawing.Point(6, 89);
            this.label95.Name = "label95";
            this.label95.Size = new System.Drawing.Size(59, 12);
            this.label95.TabIndex = 10;
            this.label95.Text = "搜索高度:";
            // 
            // groupBox23
            // 
            this.groupBox23.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox23.Controls.Add(this.BTN_virtualTargetParmSet);
            this.groupBox23.Controls.Add(this.headingChangeInterval);
            this.groupBox23.Controls.Add(this.label93);
            this.groupBox23.Controls.Add(this.speed);
            this.groupBox23.Controls.Add(this.virtualTargetSpeed);
            this.groupBox23.Location = new System.Drawing.Point(3, 122);
            this.groupBox23.Name = "groupBox23";
            this.groupBox23.Size = new System.Drawing.Size(244, 103);
            this.groupBox23.TabIndex = 9;
            this.groupBox23.TabStop = false;
            this.groupBox23.Text = "假想目标模拟参数设置";
            // 
            // BTN_virtualTargetParmSet
            // 
            this.BTN_virtualTargetParmSet.Location = new System.Drawing.Point(140, 76);
            this.BTN_virtualTargetParmSet.Name = "BTN_virtualTargetParmSet";
            this.BTN_virtualTargetParmSet.Size = new System.Drawing.Size(75, 23);
            this.BTN_virtualTargetParmSet.TabIndex = 82;
            this.BTN_virtualTargetParmSet.Text = "设置";
            this.BTN_virtualTargetParmSet.UseVisualStyleBackColor = true;
            this.BTN_virtualTargetParmSet.Click += new System.EventHandler(this.BTN_virtualTargetParmSet_Click);
            // 
            // headingChangeInterval
            // 
            this.headingChangeInterval.Location = new System.Drawing.Point(140, 45);
            this.headingChangeInterval.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.headingChangeInterval.Name = "headingChangeInterval";
            this.headingChangeInterval.Size = new System.Drawing.Size(58, 21);
            this.headingChangeInterval.TabIndex = 81;
            this.headingChangeInterval.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // label93
            // 
            this.label93.AutoSize = true;
            this.label93.Location = new System.Drawing.Point(12, 50);
            this.label93.Name = "label93";
            this.label93.Size = new System.Drawing.Size(83, 12);
            this.label93.TabIndex = 80;
            this.label93.Text = "变航向时间(s)";
            // 
            // speed
            // 
            this.speed.AutoSize = true;
            this.speed.Location = new System.Drawing.Point(12, 25);
            this.speed.Name = "speed";
            this.speed.Size = new System.Drawing.Size(83, 12);
            this.speed.TabIndex = 79;
            this.speed.Text = "运动速度(m/s)";
            // 
            // virtualTargetSpeed
            // 
            this.virtualTargetSpeed.Location = new System.Drawing.Point(140, 20);
            this.virtualTargetSpeed.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.virtualTargetSpeed.Name = "virtualTargetSpeed";
            this.virtualTargetSpeed.Size = new System.Drawing.Size(58, 21);
            this.virtualTargetSpeed.TabIndex = 78;
            this.virtualTargetSpeed.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // radioButton_targetsearch
            // 
            this.radioButton_targetsearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButton_targetsearch.AutoSize = true;
            this.radioButton_targetsearch.Checked = true;
            this.radioButton_targetsearch.Location = new System.Drawing.Point(97, 243);
            this.radioButton_targetsearch.Name = "radioButton_targetsearch";
            this.radioButton_targetsearch.Size = new System.Drawing.Size(71, 16);
            this.radioButton_targetsearch.TabIndex = 8;
            this.radioButton_targetsearch.TabStop = true;
            this.radioButton_targetsearch.Text = "目标搜索";
            this.radioButton_targetsearch.UseVisualStyleBackColor = true;
            // 
            // radioButton_tragettrack
            // 
            this.radioButton_tragettrack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButton_tragettrack.AutoSize = true;
            this.radioButton_tragettrack.Location = new System.Drawing.Point(3, 243);
            this.radioButton_tragettrack.Name = "radioButton_tragettrack";
            this.radioButton_tragettrack.Size = new System.Drawing.Size(71, 16);
            this.radioButton_tragettrack.TabIndex = 7;
            this.radioButton_tragettrack.Text = "目标追踪";
            this.radioButton_tragettrack.UseVisualStyleBackColor = true;
            // 
            // BTN_searchsimulation
            // 
            this.BTN_searchsimulation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BTN_searchsimulation.Location = new System.Drawing.Point(84, 276);
            this.BTN_searchsimulation.Name = "BTN_searchsimulation";
            this.BTN_searchsimulation.Size = new System.Drawing.Size(75, 23);
            this.BTN_searchsimulation.TabIndex = 6;
            this.BTN_searchsimulation.Text = "模拟开始";
            this.BTN_searchsimulation.UseVisualStyleBackColor = true;
            this.BTN_searchsimulation.Click += new System.EventHandler(this.BTN_searchsimulation_Click);
            // 
            // BTN_searchUpdate
            // 
            this.BTN_searchUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BTN_searchUpdate.Location = new System.Drawing.Point(3, 276);
            this.BTN_searchUpdate.Name = "BTN_searchUpdate";
            this.BTN_searchUpdate.Size = new System.Drawing.Size(75, 23);
            this.BTN_searchUpdate.TabIndex = 5;
            this.BTN_searchUpdate.Text = "刷新";
            this.BTN_searchUpdate.UseVisualStyleBackColor = true;
            this.BTN_searchUpdate.Click += new System.EventHandler(this.BTN_searchUpdate_Click);
            // 
            // BTN_search
            // 
            this.BTN_search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BTN_search.Location = new System.Drawing.Point(169, 276);
            this.BTN_search.Name = "BTN_search";
            this.BTN_search.Size = new System.Drawing.Size(75, 23);
            this.BTN_search.TabIndex = 4;
            this.BTN_search.Text = "开始搜寻";
            this.BTN_search.UseVisualStyleBackColor = true;
            this.BTN_search.Click += new System.EventHandler(this.BTN_search_Click);
            // 
            // lbl_targetfoundcounts
            // 
            this.lbl_targetfoundcounts.AutoSize = true;
            this.lbl_targetfoundcounts.Location = new System.Drawing.Point(166, 38);
            this.lbl_targetfoundcounts.Name = "lbl_targetfoundcounts";
            this.lbl_targetfoundcounts.Size = new System.Drawing.Size(11, 12);
            this.lbl_targetfoundcounts.TabIndex = 3;
            this.lbl_targetfoundcounts.Text = "0";
            // 
            // label94
            // 
            this.label94.AutoSize = true;
            this.label94.Location = new System.Drawing.Point(6, 38);
            this.label94.Name = "label94";
            this.label94.Size = new System.Drawing.Size(83, 12);
            this.label94.TabIndex = 2;
            this.label94.Text = "找到目标次数:";
            // 
            // lbl_serachareacounts
            // 
            this.lbl_serachareacounts.AutoSize = true;
            this.lbl_serachareacounts.Location = new System.Drawing.Point(166, 10);
            this.lbl_serachareacounts.Name = "lbl_serachareacounts";
            this.lbl_serachareacounts.Size = new System.Drawing.Size(11, 12);
            this.lbl_serachareacounts.TabIndex = 1;
            this.lbl_serachareacounts.Text = "0";
            // 
            // label89
            // 
            this.label89.AutoSize = true;
            this.label89.Location = new System.Drawing.Point(6, 10);
            this.label89.Name = "label89";
            this.label89.Size = new System.Drawing.Size(83, 12);
            this.label89.TabIndex = 0;
            this.label89.Text = "搜索区域个数:";
            // 
            // targetsFound
            // 
            this.targetsFound.Dock = System.Windows.Forms.DockStyle.Fill;
            this.targetsFound.FormattingEnabled = true;
            this.targetsFound.ItemHeight = 12;
            this.targetsFound.Location = new System.Drawing.Point(0, 0);
            this.targetsFound.Name = "targetsFound";
            this.targetsFound.Size = new System.Drawing.Size(250, 348);
            this.targetsFound.TabIndex = 0;
            this.targetsFound.SelectedIndexChanged += new System.EventHandler(this.targetsFoundListBox_SelectedIndexChanged);
            // 
            // gMap
            // 
            this.gMap.BackColor = System.Drawing.Color.Black;
            this.gMap.Bearing = 0F;
            this.gMap.CanDragMap = true;
            this.gMap.ContextMenuStrip = this.GmapcontextMenuStrip;
            this.gMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gMap.EmptyTileColor = System.Drawing.Color.Gray;
            this.gMap.GrayScaleMode = false;
            this.gMap.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
            this.gMap.HoldInvalidation = false;
            this.gMap.LevelsKeepInMemmory = 5;
            this.gMap.Location = new System.Drawing.Point(0, 0);
            this.gMap.Margin = new System.Windows.Forms.Padding(0);
            this.gMap.MarkersEnabled = true;
            this.gMap.MaxZoom = 24;
            this.gMap.MinZoom = 0;
            this.gMap.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            this.gMap.Name = "gMap";
            this.gMap.NegativeMode = false;
            this.gMap.PolygonsEnabled = true;
            this.gMap.RetryLoadTile = 0;
            this.gMap.RoutesEnabled = false;
            this.gMap.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Fractional;
            this.gMap.SelectedAreaFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(65)))), ((int)(((byte)(105)))), ((int)(((byte)(225)))));
            this.gMap.ShowTileGridLines = false;
            this.gMap.Size = new System.Drawing.Size(935, 654);
            this.gMap.TabIndex = 74;
            this.gMap.Zoom = 3D;
            this.gMap.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gMap_MouseDown);
            this.gMap.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gMap_MouseMove);
            // 
            // GmapcontextMenuStrip
            // 
            this.GmapcontextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.GmapcontextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showStartPositionToolStripMenuItem});
            this.GmapcontextMenuStrip.Name = "GmapcontextMenuStrip";
            this.GmapcontextMenuStrip.Size = new System.Drawing.Size(149, 26);
            // 
            // showStartPositionToolStripMenuItem
            // 
            this.showStartPositionToolStripMenuItem.Name = "showStartPositionToolStripMenuItem";
            this.showStartPositionToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.showStartPositionToolStripMenuItem.Text = "显示起始位置";
            this.showStartPositionToolStripMenuItem.Click += new System.EventHandler(this.showStartPositionToolStripMenuItem_Click);
            // 
            // track_zoom
            // 
            this.track_zoom.Dock = System.Windows.Forms.DockStyle.Right;
            this.track_zoom.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.track_zoom.LargeChange = 1F;
            this.track_zoom.Location = new System.Drawing.Point(935, 0);
            this.track_zoom.Maximum = 24F;
            this.track_zoom.Minimum = 1F;
            this.track_zoom.Name = "track_zoom";
            this.track_zoom.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.track_zoom.Size = new System.Drawing.Size(45, 654);
            this.track_zoom.SmallChange = 1F;
            this.track_zoom.TabIndex = 73;
            this.track_zoom.TickFrequency = 1F;
            this.track_zoom.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.track_zoom.Value = 1F;
            this.track_zoom.Scroll += new System.EventHandler(this.track_zoom_Scroll);
            // 
            // tabPage_HIL
            // 
            this.tabPage_HIL.Controls.Add(this.splitContainer11);
            this.tabPage_HIL.Location = new System.Drawing.Point(4, 22);
            this.tabPage_HIL.Name = "tabPage_HIL";
            this.tabPage_HIL.Size = new System.Drawing.Size(1234, 654);
            this.tabPage_HIL.TabIndex = 9;
            this.tabPage_HIL.Text = "HIL仿真";
            this.tabPage_HIL.UseVisualStyleBackColor = true;
            // 
            // splitContainer11
            // 
            this.splitContainer11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer11.Location = new System.Drawing.Point(0, 0);
            this.splitContainer11.Name = "splitContainer11";
            this.splitContainer11.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer11.Panel1
            // 
            this.splitContainer11.Panel1.Controls.Add(this.checkBox_REVthr);
            this.splitContainer11.Panel1.Controls.Add(this.checkBox_REVyaw);
            this.splitContainer11.Panel1.Controls.Add(this.checkBox_REVpitch);
            this.splitContainer11.Panel1.Controls.Add(this.checkBox_REVroll);
            this.splitContainer11.Panel1.Controls.Add(this.radioButton_jsbsim);
            this.splitContainer11.Panel1.Controls.Add(this.radioButton_aersimrc);
            this.splitContainer11.Panel1.Controls.Add(this.radioButton_flight);
            this.splitContainer11.Panel1.Controls.Add(this.radioButton_matlab);
            this.splitContainer11.Panel1.Controls.Add(this.radioButton_xplane);
            this.splitContainer11.Panel1.Controls.Add(this.BTN_SimStart);
            // 
            // splitContainer11.Panel2
            // 
            this.splitContainer11.Panel2.Controls.Add(this.splitContainer12);
            this.splitContainer11.Size = new System.Drawing.Size(1234, 654);
            this.splitContainer11.SplitterDistance = 98;
            this.splitContainer11.TabIndex = 0;
            // 
            // checkBox_REVthr
            // 
            this.checkBox_REVthr.AutoSize = true;
            this.checkBox_REVthr.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.checkBox_REVthr.Location = new System.Drawing.Point(726, 64);
            this.checkBox_REVthr.Name = "checkBox_REVthr";
            this.checkBox_REVthr.Size = new System.Drawing.Size(90, 16);
            this.checkBox_REVthr.TabIndex = 16;
            this.checkBox_REVthr.Text = "Reverse Thr";
            this.checkBox_REVthr.UseVisualStyleBackColor = true;
            // 
            // checkBox_REVyaw
            // 
            this.checkBox_REVyaw.AutoSize = true;
            this.checkBox_REVyaw.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.checkBox_REVyaw.Location = new System.Drawing.Point(606, 64);
            this.checkBox_REVyaw.Name = "checkBox_REVyaw";
            this.checkBox_REVyaw.Size = new System.Drawing.Size(90, 16);
            this.checkBox_REVyaw.TabIndex = 15;
            this.checkBox_REVyaw.Text = "Reverse Yaw";
            this.checkBox_REVyaw.UseVisualStyleBackColor = true;
            this.checkBox_REVyaw.CheckedChanged += new System.EventHandler(this.REVYaw_CheckedChange);
            // 
            // checkBox_REVpitch
            // 
            this.checkBox_REVpitch.AutoSize = true;
            this.checkBox_REVpitch.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.checkBox_REVpitch.Location = new System.Drawing.Point(470, 64);
            this.checkBox_REVpitch.Name = "checkBox_REVpitch";
            this.checkBox_REVpitch.Size = new System.Drawing.Size(102, 16);
            this.checkBox_REVpitch.TabIndex = 14;
            this.checkBox_REVpitch.Text = "Reverse Pitch";
            this.checkBox_REVpitch.UseVisualStyleBackColor = true;
            this.checkBox_REVpitch.CheckedChanged += new System.EventHandler(this.REVPitch_CheckedChange);
            // 
            // checkBox_REVroll
            // 
            this.checkBox_REVroll.AutoSize = true;
            this.checkBox_REVroll.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.checkBox_REVroll.Location = new System.Drawing.Point(342, 64);
            this.checkBox_REVroll.Name = "checkBox_REVroll";
            this.checkBox_REVroll.Size = new System.Drawing.Size(96, 16);
            this.checkBox_REVroll.TabIndex = 13;
            this.checkBox_REVroll.Text = "Reverse Roll";
            this.checkBox_REVroll.UseVisualStyleBackColor = true;
            this.checkBox_REVroll.CheckedChanged += new System.EventHandler(this.REVRoll_CheckedChange);
            // 
            // radioButton_jsbsim
            // 
            this.radioButton_jsbsim.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButton_jsbsim.AutoSize = true;
            this.radioButton_jsbsim.Location = new System.Drawing.Point(754, 21);
            this.radioButton_jsbsim.Name = "radioButton_jsbsim";
            this.radioButton_jsbsim.Size = new System.Drawing.Size(59, 16);
            this.radioButton_jsbsim.TabIndex = 12;
            this.radioButton_jsbsim.Text = "JSBSim";
            this.radioButton_jsbsim.UseVisualStyleBackColor = true;
            // 
            // radioButton_aersimrc
            // 
            this.radioButton_aersimrc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButton_aersimrc.AutoSize = true;
            this.radioButton_aersimrc.Location = new System.Drawing.Point(643, 21);
            this.radioButton_aersimrc.Name = "radioButton_aersimrc";
            this.radioButton_aersimrc.Size = new System.Drawing.Size(77, 16);
            this.radioButton_aersimrc.TabIndex = 11;
            this.radioButton_aersimrc.Text = "AeroSimRC";
            this.radioButton_aersimrc.UseVisualStyleBackColor = true;
            // 
            // radioButton_flight
            // 
            this.radioButton_flight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButton_flight.AutoSize = true;
            this.radioButton_flight.Location = new System.Drawing.Point(524, 21);
            this.radioButton_flight.Name = "radioButton_flight";
            this.radioButton_flight.Size = new System.Drawing.Size(83, 16);
            this.radioButton_flight.TabIndex = 10;
            this.radioButton_flight.Text = "FlightGear";
            this.radioButton_flight.UseVisualStyleBackColor = true;
            // 
            // radioButton_matlab
            // 
            this.radioButton_matlab.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButton_matlab.AutoSize = true;
            this.radioButton_matlab.Location = new System.Drawing.Point(437, 21);
            this.radioButton_matlab.Name = "radioButton_matlab";
            this.radioButton_matlab.Size = new System.Drawing.Size(59, 16);
            this.radioButton_matlab.TabIndex = 9;
            this.radioButton_matlab.Text = "MATLAB";
            this.radioButton_matlab.UseVisualStyleBackColor = true;
            // 
            // radioButton_xplane
            // 
            this.radioButton_xplane.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButton_xplane.AutoSize = true;
            this.radioButton_xplane.Checked = true;
            this.radioButton_xplane.Location = new System.Drawing.Point(342, 21);
            this.radioButton_xplane.Name = "radioButton_xplane";
            this.radioButton_xplane.Size = new System.Drawing.Size(65, 16);
            this.radioButton_xplane.TabIndex = 8;
            this.radioButton_xplane.TabStop = true;
            this.radioButton_xplane.Text = "X-plane";
            this.radioButton_xplane.UseVisualStyleBackColor = true;
            // 
            // BTN_SimStart
            // 
            this.BTN_SimStart.Location = new System.Drawing.Point(18, 28);
            this.BTN_SimStart.Name = "BTN_SimStart";
            this.BTN_SimStart.Size = new System.Drawing.Size(186, 42);
            this.BTN_SimStart.TabIndex = 1;
            this.BTN_SimStart.Text = "Simulation Link Start";
            this.BTN_SimStart.UseVisualStyleBackColor = true;
            this.BTN_SimStart.Click += new System.EventHandler(this.BTN_SimStart_Click);
            // 
            // splitContainer12
            // 
            this.splitContainer12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer12.Location = new System.Drawing.Point(0, 0);
            this.splitContainer12.Name = "splitContainer12";
            // 
            // splitContainer12.Panel1
            // 
            this.splitContainer12.Panel1.Controls.Add(this.flowLayoutPanel_udplinks);
            // 
            // splitContainer12.Panel2
            // 
            this.splitContainer12.Panel2.Controls.Add(this.groupBox29);
            this.splitContainer12.Panel2.Controls.Add(this.groupBox28);
            this.splitContainer12.Panel2.Controls.Add(this.groupBox27);
            this.splitContainer12.Panel2.Controls.Add(this.groupBox26);
            this.splitContainer12.Size = new System.Drawing.Size(1234, 552);
            this.splitContainer12.SplitterDistance = 642;
            this.splitContainer12.TabIndex = 0;
            // 
            // flowLayoutPanel_udplinks
            // 
            this.flowLayoutPanel_udplinks.AutoScroll = true;
            this.flowLayoutPanel_udplinks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel_udplinks.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel_udplinks.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel_udplinks.Name = "flowLayoutPanel_udplinks";
            this.flowLayoutPanel_udplinks.Size = new System.Drawing.Size(642, 552);
            this.flowLayoutPanel_udplinks.TabIndex = 1;
            this.flowLayoutPanel_udplinks.WrapContents = false;
            // 
            // groupBox29
            // 
            this.groupBox29.Controls.Add(this.lbl_servo4);
            this.groupBox29.Controls.Add(this.lbl_servo3);
            this.groupBox29.Controls.Add(this.lbl_servo2);
            this.groupBox29.Controls.Add(this.lbl_servo1);
            this.groupBox29.Controls.Add(this.label138);
            this.groupBox29.Controls.Add(this.label139);
            this.groupBox29.Controls.Add(this.label140);
            this.groupBox29.Controls.Add(this.label141);
            this.groupBox29.Location = new System.Drawing.Point(301, 228);
            this.groupBox29.Name = "groupBox29";
            this.groupBox29.Size = new System.Drawing.Size(227, 142);
            this.groupBox29.TabIndex = 3;
            this.groupBox29.TabStop = false;
            this.groupBox29.Text = "Plane Output";
            // 
            // lbl_servo4
            // 
            this.lbl_servo4.Location = new System.Drawing.Point(116, 113);
            this.lbl_servo4.Name = "lbl_servo4";
            this.lbl_servo4.Size = new System.Drawing.Size(85, 15);
            this.lbl_servo4.TabIndex = 9;
            this.lbl_servo4.Text = "0";
            this.lbl_servo4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_servo3
            // 
            this.lbl_servo3.Location = new System.Drawing.Point(116, 89);
            this.lbl_servo3.Name = "lbl_servo3";
            this.lbl_servo3.Size = new System.Drawing.Size(85, 15);
            this.lbl_servo3.TabIndex = 8;
            this.lbl_servo3.Text = "0";
            this.lbl_servo3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_servo2
            // 
            this.lbl_servo2.Location = new System.Drawing.Point(116, 63);
            this.lbl_servo2.Name = "lbl_servo2";
            this.lbl_servo2.Size = new System.Drawing.Size(85, 15);
            this.lbl_servo2.TabIndex = 7;
            this.lbl_servo2.Text = "0";
            this.lbl_servo2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_servo1
            // 
            this.lbl_servo1.Location = new System.Drawing.Point(116, 37);
            this.lbl_servo1.Name = "lbl_servo1";
            this.lbl_servo1.Size = new System.Drawing.Size(85, 15);
            this.lbl_servo1.TabIndex = 6;
            this.lbl_servo1.Text = "0";
            this.lbl_servo1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label138
            // 
            this.label138.AutoSize = true;
            this.label138.Location = new System.Drawing.Point(12, 113);
            this.label138.Name = "label138";
            this.label138.Size = new System.Drawing.Size(47, 12);
            this.label138.TabIndex = 3;
            this.label138.Text = "Servo4:";
            // 
            // label139
            // 
            this.label139.AutoSize = true;
            this.label139.Location = new System.Drawing.Point(12, 89);
            this.label139.Name = "label139";
            this.label139.Size = new System.Drawing.Size(47, 12);
            this.label139.TabIndex = 2;
            this.label139.Text = "Servo3:";
            // 
            // label140
            // 
            this.label140.AutoSize = true;
            this.label140.Location = new System.Drawing.Point(12, 63);
            this.label140.Name = "label140";
            this.label140.Size = new System.Drawing.Size(47, 12);
            this.label140.TabIndex = 1;
            this.label140.Text = "Servo2:";
            // 
            // label141
            // 
            this.label141.AutoSize = true;
            this.label141.Location = new System.Drawing.Point(12, 37);
            this.label141.Name = "label141";
            this.label141.Size = new System.Drawing.Size(47, 12);
            this.label141.TabIndex = 0;
            this.label141.Text = "Servo1:";
            // 
            // groupBox28
            // 
            this.groupBox28.Controls.Add(this.lbl_heading);
            this.groupBox28.Controls.Add(this.lbl_yaw);
            this.groupBox28.Controls.Add(this.lbl_roll);
            this.groupBox28.Controls.Add(this.lbl_pitch);
            this.groupBox28.Controls.Add(this.label134);
            this.groupBox28.Controls.Add(this.label135);
            this.groupBox28.Controls.Add(this.label136);
            this.groupBox28.Controls.Add(this.label137);
            this.groupBox28.Location = new System.Drawing.Point(14, 228);
            this.groupBox28.Name = "groupBox28";
            this.groupBox28.Size = new System.Drawing.Size(227, 142);
            this.groupBox28.TabIndex = 2;
            this.groupBox28.TabStop = false;
            this.groupBox28.Text = "Plane AHRS";
            // 
            // lbl_heading
            // 
            this.lbl_heading.Location = new System.Drawing.Point(116, 113);
            this.lbl_heading.Name = "lbl_heading";
            this.lbl_heading.Size = new System.Drawing.Size(85, 15);
            this.lbl_heading.TabIndex = 9;
            this.lbl_heading.Text = "0";
            this.lbl_heading.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_yaw
            // 
            this.lbl_yaw.Location = new System.Drawing.Point(116, 89);
            this.lbl_yaw.Name = "lbl_yaw";
            this.lbl_yaw.Size = new System.Drawing.Size(85, 15);
            this.lbl_yaw.TabIndex = 8;
            this.lbl_yaw.Text = "0";
            this.lbl_yaw.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_roll
            // 
            this.lbl_roll.Location = new System.Drawing.Point(116, 63);
            this.lbl_roll.Name = "lbl_roll";
            this.lbl_roll.Size = new System.Drawing.Size(85, 15);
            this.lbl_roll.TabIndex = 7;
            this.lbl_roll.Text = "0";
            this.lbl_roll.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_pitch
            // 
            this.lbl_pitch.Location = new System.Drawing.Point(116, 37);
            this.lbl_pitch.Name = "lbl_pitch";
            this.lbl_pitch.Size = new System.Drawing.Size(85, 15);
            this.lbl_pitch.TabIndex = 6;
            this.lbl_pitch.Text = "0";
            this.lbl_pitch.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label134
            // 
            this.label134.AutoSize = true;
            this.label134.Location = new System.Drawing.Point(12, 113);
            this.label134.Name = "label134";
            this.label134.Size = new System.Drawing.Size(53, 12);
            this.label134.TabIndex = 3;
            this.label134.Text = "Heading:";
            // 
            // label135
            // 
            this.label135.AutoSize = true;
            this.label135.Location = new System.Drawing.Point(12, 89);
            this.label135.Name = "label135";
            this.label135.Size = new System.Drawing.Size(29, 12);
            this.label135.TabIndex = 2;
            this.label135.Text = "Yaw:";
            // 
            // label136
            // 
            this.label136.AutoSize = true;
            this.label136.Location = new System.Drawing.Point(12, 63);
            this.label136.Name = "label136";
            this.label136.Size = new System.Drawing.Size(35, 12);
            this.label136.TabIndex = 1;
            this.label136.Text = "Roll:";
            // 
            // label137
            // 
            this.label137.AutoSize = true;
            this.label137.Location = new System.Drawing.Point(12, 37);
            this.label137.Name = "label137";
            this.label137.Size = new System.Drawing.Size(41, 12);
            this.label137.TabIndex = 0;
            this.label137.Text = "Pitch:";
            // 
            // groupBox27
            // 
            this.groupBox27.Controls.Add(this.lbl_az);
            this.groupBox27.Controls.Add(this.lbl_ay);
            this.groupBox27.Controls.Add(this.lbl_ax);
            this.groupBox27.Controls.Add(this.lbl_gz);
            this.groupBox27.Controls.Add(this.lbl_gy);
            this.groupBox27.Controls.Add(this.lbl_gx);
            this.groupBox27.Controls.Add(this.label120);
            this.groupBox27.Controls.Add(this.label121);
            this.groupBox27.Controls.Add(this.label122);
            this.groupBox27.Controls.Add(this.label123);
            this.groupBox27.Controls.Add(this.label124);
            this.groupBox27.Controls.Add(this.label125);
            this.groupBox27.Location = new System.Drawing.Point(301, 3);
            this.groupBox27.Name = "groupBox27";
            this.groupBox27.Size = new System.Drawing.Size(227, 195);
            this.groupBox27.TabIndex = 1;
            this.groupBox27.TabStop = false;
            this.groupBox27.Text = "Plane IMU";
            // 
            // lbl_az
            // 
            this.lbl_az.Location = new System.Drawing.Point(116, 168);
            this.lbl_az.Name = "lbl_az";
            this.lbl_az.Size = new System.Drawing.Size(85, 15);
            this.lbl_az.TabIndex = 11;
            this.lbl_az.Text = "0";
            this.lbl_az.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_ay
            // 
            this.lbl_ay.Location = new System.Drawing.Point(116, 140);
            this.lbl_ay.Name = "lbl_ay";
            this.lbl_ay.Size = new System.Drawing.Size(85, 15);
            this.lbl_ay.TabIndex = 10;
            this.lbl_ay.Text = "0";
            this.lbl_ay.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_ax
            // 
            this.lbl_ax.Location = new System.Drawing.Point(116, 113);
            this.lbl_ax.Name = "lbl_ax";
            this.lbl_ax.Size = new System.Drawing.Size(85, 15);
            this.lbl_ax.TabIndex = 9;
            this.lbl_ax.Text = "0";
            this.lbl_ax.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_gz
            // 
            this.lbl_gz.Location = new System.Drawing.Point(116, 89);
            this.lbl_gz.Name = "lbl_gz";
            this.lbl_gz.Size = new System.Drawing.Size(85, 15);
            this.lbl_gz.TabIndex = 8;
            this.lbl_gz.Text = "0";
            this.lbl_gz.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_gy
            // 
            this.lbl_gy.Location = new System.Drawing.Point(116, 63);
            this.lbl_gy.Name = "lbl_gy";
            this.lbl_gy.Size = new System.Drawing.Size(85, 15);
            this.lbl_gy.TabIndex = 7;
            this.lbl_gy.Text = "0";
            this.lbl_gy.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_gx
            // 
            this.lbl_gx.Location = new System.Drawing.Point(116, 37);
            this.lbl_gx.Name = "lbl_gx";
            this.lbl_gx.Size = new System.Drawing.Size(85, 15);
            this.lbl_gx.TabIndex = 6;
            this.lbl_gx.Text = "0";
            this.lbl_gx.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label120
            // 
            this.label120.AutoSize = true;
            this.label120.Location = new System.Drawing.Point(12, 168);
            this.label120.Name = "label120";
            this.label120.Size = new System.Drawing.Size(23, 12);
            this.label120.TabIndex = 5;
            this.label120.Text = "Az:";
            // 
            // label121
            // 
            this.label121.AutoSize = true;
            this.label121.Location = new System.Drawing.Point(12, 140);
            this.label121.Name = "label121";
            this.label121.Size = new System.Drawing.Size(23, 12);
            this.label121.TabIndex = 4;
            this.label121.Text = "Ay:";
            // 
            // label122
            // 
            this.label122.AutoSize = true;
            this.label122.Location = new System.Drawing.Point(12, 113);
            this.label122.Name = "label122";
            this.label122.Size = new System.Drawing.Size(23, 12);
            this.label122.TabIndex = 3;
            this.label122.Text = "Ax:";
            // 
            // label123
            // 
            this.label123.AutoSize = true;
            this.label123.Location = new System.Drawing.Point(12, 89);
            this.label123.Name = "label123";
            this.label123.Size = new System.Drawing.Size(23, 12);
            this.label123.TabIndex = 2;
            this.label123.Text = "Gz:";
            // 
            // label124
            // 
            this.label124.AutoSize = true;
            this.label124.Location = new System.Drawing.Point(12, 63);
            this.label124.Name = "label124";
            this.label124.Size = new System.Drawing.Size(23, 12);
            this.label124.TabIndex = 1;
            this.label124.Text = "Gy:";
            // 
            // label125
            // 
            this.label125.AutoSize = true;
            this.label125.Location = new System.Drawing.Point(12, 37);
            this.label125.Name = "label125";
            this.label125.Size = new System.Drawing.Size(23, 12);
            this.label125.TabIndex = 0;
            this.label125.Text = "Gx:";
            // 
            // groupBox26
            // 
            this.groupBox26.Controls.Add(this.lbl_vz);
            this.groupBox26.Controls.Add(this.lbl_vy);
            this.groupBox26.Controls.Add(this.lbl_vx);
            this.groupBox26.Controls.Add(this.lbl_alt);
            this.groupBox26.Controls.Add(this.lbl_lng);
            this.groupBox26.Controls.Add(this.lbl_lat);
            this.groupBox26.Controls.Add(this.label107);
            this.groupBox26.Controls.Add(this.label106);
            this.groupBox26.Controls.Add(this.label105);
            this.groupBox26.Controls.Add(this.label104);
            this.groupBox26.Controls.Add(this.label103);
            this.groupBox26.Controls.Add(this.label102);
            this.groupBox26.Location = new System.Drawing.Point(14, 3);
            this.groupBox26.Name = "groupBox26";
            this.groupBox26.Size = new System.Drawing.Size(227, 195);
            this.groupBox26.TabIndex = 0;
            this.groupBox26.TabStop = false;
            this.groupBox26.Text = "Plane GPS";
            // 
            // lbl_vz
            // 
            this.lbl_vz.Location = new System.Drawing.Point(116, 168);
            this.lbl_vz.Name = "lbl_vz";
            this.lbl_vz.Size = new System.Drawing.Size(85, 15);
            this.lbl_vz.TabIndex = 11;
            this.lbl_vz.Text = "0";
            this.lbl_vz.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_vy
            // 
            this.lbl_vy.Location = new System.Drawing.Point(116, 140);
            this.lbl_vy.Name = "lbl_vy";
            this.lbl_vy.Size = new System.Drawing.Size(85, 15);
            this.lbl_vy.TabIndex = 10;
            this.lbl_vy.Text = "0";
            this.lbl_vy.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_vx
            // 
            this.lbl_vx.Location = new System.Drawing.Point(116, 113);
            this.lbl_vx.Name = "lbl_vx";
            this.lbl_vx.Size = new System.Drawing.Size(85, 15);
            this.lbl_vx.TabIndex = 9;
            this.lbl_vx.Text = "0";
            this.lbl_vx.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_alt
            // 
            this.lbl_alt.Location = new System.Drawing.Point(116, 89);
            this.lbl_alt.Name = "lbl_alt";
            this.lbl_alt.Size = new System.Drawing.Size(85, 15);
            this.lbl_alt.TabIndex = 8;
            this.lbl_alt.Text = "0";
            this.lbl_alt.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_lng
            // 
            this.lbl_lng.Location = new System.Drawing.Point(101, 63);
            this.lbl_lng.Name = "lbl_lng";
            this.lbl_lng.Size = new System.Drawing.Size(116, 15);
            this.lbl_lng.TabIndex = 7;
            this.lbl_lng.Text = "0";
            this.lbl_lng.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_lat
            // 
            this.lbl_lat.Location = new System.Drawing.Point(101, 37);
            this.lbl_lat.Name = "lbl_lat";
            this.lbl_lat.Size = new System.Drawing.Size(116, 15);
            this.lbl_lat.TabIndex = 6;
            this.lbl_lat.Text = "0";
            this.lbl_lat.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label107
            // 
            this.label107.AutoSize = true;
            this.label107.Location = new System.Drawing.Point(12, 168);
            this.label107.Name = "label107";
            this.label107.Size = new System.Drawing.Size(23, 12);
            this.label107.TabIndex = 5;
            this.label107.Text = "Vz:";
            // 
            // label106
            // 
            this.label106.AutoSize = true;
            this.label106.Location = new System.Drawing.Point(12, 140);
            this.label106.Name = "label106";
            this.label106.Size = new System.Drawing.Size(23, 12);
            this.label106.TabIndex = 4;
            this.label106.Text = "Vy:";
            // 
            // label105
            // 
            this.label105.AutoSize = true;
            this.label105.Location = new System.Drawing.Point(12, 113);
            this.label105.Name = "label105";
            this.label105.Size = new System.Drawing.Size(23, 12);
            this.label105.TabIndex = 3;
            this.label105.Text = "Vx:";
            // 
            // label104
            // 
            this.label104.AutoSize = true;
            this.label104.Location = new System.Drawing.Point(12, 89);
            this.label104.Name = "label104";
            this.label104.Size = new System.Drawing.Size(59, 12);
            this.label104.TabIndex = 2;
            this.label104.Text = "Altitude:";
            // 
            // label103
            // 
            this.label103.AutoSize = true;
            this.label103.Location = new System.Drawing.Point(12, 63);
            this.label103.Name = "label103";
            this.label103.Size = new System.Drawing.Size(65, 12);
            this.label103.TabIndex = 1;
            this.label103.Text = "Longitude:";
            // 
            // label102
            // 
            this.label102.AutoSize = true;
            this.label102.Location = new System.Drawing.Point(12, 37);
            this.label102.Name = "label102";
            this.label102.Size = new System.Drawing.Size(59, 12);
            this.label102.TabIndex = 0;
            this.label102.Text = "Latitude:";
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.groupBox30);
            this.tabPage5.Controls.Add(this.groupBox20);
            this.tabPage5.Controls.Add(this.groupbox_rxdebug);
            this.tabPage5.Controls.Add(this.groupBox19);
            this.tabPage5.Controls.Add(this.groupBox18);
            this.tabPage5.Controls.Add(this.groupBox17);
            this.tabPage5.Controls.Add(this.groupBox5);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(1234, 654);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "系统配置";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // groupBox30
            // 
            this.groupBox30.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox30.Controls.Add(this.textBox_mirrorUdpPortInfo);
            this.groupBox30.Controls.Add(this.rb_mannalSetMirrorUdp);
            this.groupBox30.Controls.Add(this.textBox_intervalMirrorUdpPort);
            this.groupBox30.Controls.Add(this.label110);
            this.groupBox30.Controls.Add(this.textBox_BeginMirrorUdpPort);
            this.groupBox30.Controls.Add(this.label109);
            this.groupBox30.Controls.Add(this.btn_mirrorUdpForward);
            this.groupBox30.Controls.Add(this.rb_autoSetMirrorUdp);
            this.groupBox30.Location = new System.Drawing.Point(538, 429);
            this.groupBox30.Name = "groupBox30";
            this.groupBox30.Size = new System.Drawing.Size(687, 167);
            this.groupBox30.TabIndex = 81;
            this.groupBox30.TabStop = false;
            this.groupBox30.Text = "UDP镜像转发设置";
            // 
            // textBox_mirrorUdpPortInfo
            // 
            this.textBox_mirrorUdpPortInfo.Location = new System.Drawing.Point(6, 41);
            this.textBox_mirrorUdpPortInfo.Multiline = true;
            this.textBox_mirrorUdpPortInfo.Name = "textBox_mirrorUdpPortInfo";
            this.textBox_mirrorUdpPortInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_mirrorUdpPortInfo.Size = new System.Drawing.Size(675, 119);
            this.textBox_mirrorUdpPortInfo.TabIndex = 46;
            // 
            // rb_mannalSetMirrorUdp
            // 
            this.rb_mannalSetMirrorUdp.AutoSize = true;
            this.rb_mannalSetMirrorUdp.Location = new System.Drawing.Point(18, 19);
            this.rb_mannalSetMirrorUdp.Name = "rb_mannalSetMirrorUdp";
            this.rb_mannalSetMirrorUdp.Size = new System.Drawing.Size(95, 16);
            this.rb_mannalSetMirrorUdp.TabIndex = 45;
            this.rb_mannalSetMirrorUdp.Text = "手动设置端口";
            this.rb_mannalSetMirrorUdp.UseVisualStyleBackColor = true;
            // 
            // textBox_intervalMirrorUdpPort
            // 
            this.textBox_intervalMirrorUdpPort.BackColor = System.Drawing.SystemColors.Control;
            this.textBox_intervalMirrorUdpPort.Font = new System.Drawing.Font("宋体", 9F);
            this.textBox_intervalMirrorUdpPort.Location = new System.Drawing.Point(405, 17);
            this.textBox_intervalMirrorUdpPort.Name = "textBox_intervalMirrorUdpPort";
            this.textBox_intervalMirrorUdpPort.Size = new System.Drawing.Size(48, 21);
            this.textBox_intervalMirrorUdpPort.TabIndex = 44;
            this.textBox_intervalMirrorUdpPort.Text = "1";
            // 
            // label110
            // 
            this.label110.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label110.Location = new System.Drawing.Point(336, 17);
            this.label110.Name = "label110";
            this.label110.Size = new System.Drawing.Size(63, 21);
            this.label110.TabIndex = 43;
            this.label110.Text = "递增间隔:";
            // 
            // textBox_BeginMirrorUdpPort
            // 
            this.textBox_BeginMirrorUdpPort.BackColor = System.Drawing.SystemColors.Control;
            this.textBox_BeginMirrorUdpPort.Font = new System.Drawing.Font("宋体", 9F);
            this.textBox_BeginMirrorUdpPort.Location = new System.Drawing.Point(282, 17);
            this.textBox_BeginMirrorUdpPort.Name = "textBox_BeginMirrorUdpPort";
            this.textBox_BeginMirrorUdpPort.Size = new System.Drawing.Size(48, 21);
            this.textBox_BeginMirrorUdpPort.TabIndex = 42;
            this.textBox_BeginMirrorUdpPort.Text = "100";
            // 
            // label109
            // 
            this.label109.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label109.Location = new System.Drawing.Point(224, 17);
            this.label109.Name = "label109";
            this.label109.Size = new System.Drawing.Size(63, 21);
            this.label109.TabIndex = 41;
            this.label109.Text = "初始端口:";
            // 
            // btn_mirrorUdpForward
            // 
            this.btn_mirrorUdpForward.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_mirrorUdpForward.Location = new System.Drawing.Point(606, 16);
            this.btn_mirrorUdpForward.Name = "btn_mirrorUdpForward";
            this.btn_mirrorUdpForward.Size = new System.Drawing.Size(75, 22);
            this.btn_mirrorUdpForward.TabIndex = 40;
            this.btn_mirrorUdpForward.Text = "转发";
            this.btn_mirrorUdpForward.UseVisualStyleBackColor = true;
            this.btn_mirrorUdpForward.Click += new System.EventHandler(this.btn_mirrorUdpForward_Click);
            // 
            // rb_autoSetMirrorUdp
            // 
            this.rb_autoSetMirrorUdp.AutoSize = true;
            this.rb_autoSetMirrorUdp.Checked = true;
            this.rb_autoSetMirrorUdp.Location = new System.Drawing.Point(116, 19);
            this.rb_autoSetMirrorUdp.Name = "rb_autoSetMirrorUdp";
            this.rb_autoSetMirrorUdp.Size = new System.Drawing.Size(95, 16);
            this.rb_autoSetMirrorUdp.TabIndex = 3;
            this.rb_autoSetMirrorUdp.TabStop = true;
            this.rb_autoSetMirrorUdp.Text = "自动设置端口";
            this.rb_autoSetMirrorUdp.UseVisualStyleBackColor = true;
            // 
            // groupBox20
            // 
            this.groupBox20.Controls.Add(this.CMB_UdpSendRate);
            this.groupBox20.Controls.Add(this.label84);
            this.groupBox20.Controls.Add(this.UDP_send);
            this.groupBox20.Controls.Add(this.txt_remoteip);
            this.groupBox20.Controls.Add(this.txt_remoteport);
            this.groupBox20.Controls.Add(this.label86);
            this.groupBox20.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox20.Location = new System.Drawing.Point(12, 526);
            this.groupBox20.Name = "groupBox20";
            this.groupBox20.Size = new System.Drawing.Size(520, 70);
            this.groupBox20.TabIndex = 80;
            this.groupBox20.TabStop = false;
            this.groupBox20.Text = "UDP发送端口设置";
            // 
            // CMB_UdpSendRate
            // 
            this.CMB_UdpSendRate.FormattingEnabled = true;
            this.CMB_UdpSendRate.Items.AddRange(new object[] {
            "20hz",
            "10hz",
            "5hz",
            "3hz",
            "2hz",
            "1hz",
            "0.5hz",
            "0.2hz",
            "0.1hz"});
            this.CMB_UdpSendRate.Location = new System.Drawing.Point(350, 34);
            this.CMB_UdpSendRate.Name = "CMB_UdpSendRate";
            this.CMB_UdpSendRate.Size = new System.Drawing.Size(80, 20);
            this.CMB_UdpSendRate.TabIndex = 44;
            this.CMB_UdpSendRate.SelectedIndexChanged += new System.EventHandler(this.CMB_udpSendfrequency);
            // 
            // label84
            // 
            this.label84.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label84.Location = new System.Drawing.Point(6, 34);
            this.label84.Name = "label84";
            this.label84.Size = new System.Drawing.Size(89, 22);
            this.label84.TabIndex = 35;
            this.label84.Text = "Remote IP:";
            // 
            // UDP_send
            // 
            this.UDP_send.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.UDP_send.Location = new System.Drawing.Point(442, 34);
            this.UDP_send.Name = "UDP_send";
            this.UDP_send.Size = new System.Drawing.Size(75, 22);
            this.UDP_send.TabIndex = 39;
            this.UDP_send.Text = "Connect";
            this.UDP_send.UseVisualStyleBackColor = true;
            this.UDP_send.Click += new System.EventHandler(this.BUT_UDPSendConnect);
            // 
            // txt_remoteip
            // 
            this.txt_remoteip.BackColor = System.Drawing.SystemColors.Control;
            this.txt_remoteip.Font = new System.Drawing.Font("宋体", 9F);
            this.txt_remoteip.Location = new System.Drawing.Point(95, 33);
            this.txt_remoteip.Name = "txt_remoteip";
            this.txt_remoteip.Size = new System.Drawing.Size(89, 21);
            this.txt_remoteip.TabIndex = 36;
            this.txt_remoteip.Text = "127.0.0.1";
            // 
            // txt_remoteport
            // 
            this.txt_remoteport.BackColor = System.Drawing.SystemColors.Control;
            this.txt_remoteport.Font = new System.Drawing.Font("宋体", 9F);
            this.txt_remoteport.Location = new System.Drawing.Point(282, 33);
            this.txt_remoteport.Name = "txt_remoteport";
            this.txt_remoteport.Size = new System.Drawing.Size(62, 21);
            this.txt_remoteport.TabIndex = 38;
            this.txt_remoteport.Text = "14551";
            // 
            // label86
            // 
            this.label86.Font = new System.Drawing.Font("宋体", 9F);
            this.label86.Location = new System.Drawing.Point(184, 34);
            this.label86.Name = "label86";
            this.label86.Size = new System.Drawing.Size(116, 22);
            this.label86.TabIndex = 37;
            this.label86.Text = "Remote Port:";
            // 
            // groupbox_rxdebug
            // 
            this.groupbox_rxdebug.Controls.Add(this.mybutton_datashowpause);
            this.groupbox_rxdebug.Controls.Add(this.myButton20);
            this.groupbox_rxdebug.Controls.Add(this.radioButton_hexshow);
            this.groupbox_rxdebug.Controls.Add(this.radioButton_charshow);
            this.groupbox_rxdebug.Controls.Add(this.txt_rxdebug);
            this.groupbox_rxdebug.Location = new System.Drawing.Point(538, 3);
            this.groupbox_rxdebug.Name = "groupbox_rxdebug";
            this.groupbox_rxdebug.Size = new System.Drawing.Size(693, 408);
            this.groupbox_rxdebug.TabIndex = 79;
            this.groupbox_rxdebug.TabStop = false;
            this.groupbox_rxdebug.Text = "接收区";
            this.groupbox_rxdebug.Visible = false;
            // 
            // mybutton_datashowpause
            // 
            this.mybutton_datashowpause.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.mybutton_datashowpause.Location = new System.Drawing.Point(387, 352);
            this.mybutton_datashowpause.Name = "mybutton_datashowpause";
            this.mybutton_datashowpause.Size = new System.Drawing.Size(75, 22);
            this.mybutton_datashowpause.TabIndex = 41;
            this.mybutton_datashowpause.Text = "暂停";
            this.mybutton_datashowpause.UseVisualStyleBackColor = true;
            this.mybutton_datashowpause.Click += new System.EventHandler(this.BTN_dubugpause);
            // 
            // myButton20
            // 
            this.myButton20.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.myButton20.Location = new System.Drawing.Point(282, 352);
            this.myButton20.Name = "myButton20";
            this.myButton20.Size = new System.Drawing.Size(75, 22);
            this.myButton20.TabIndex = 40;
            this.myButton20.Text = "清除";
            this.myButton20.UseVisualStyleBackColor = true;
            this.myButton20.Click += new System.EventHandler(this.BTN_clearrxdebugdata);
            // 
            // radioButton_hexshow
            // 
            this.radioButton_hexshow.AutoSize = true;
            this.radioButton_hexshow.Checked = true;
            this.radioButton_hexshow.Location = new System.Drawing.Point(148, 352);
            this.radioButton_hexshow.Name = "radioButton_hexshow";
            this.radioButton_hexshow.Size = new System.Drawing.Size(65, 16);
            this.radioButton_hexshow.TabIndex = 2;
            this.radioButton_hexshow.TabStop = true;
            this.radioButton_hexshow.Text = "HEX显示";
            this.radioButton_hexshow.UseVisualStyleBackColor = true;
            // 
            // radioButton_charshow
            // 
            this.radioButton_charshow.AutoSize = true;
            this.radioButton_charshow.Location = new System.Drawing.Point(18, 352);
            this.radioButton_charshow.Name = "radioButton_charshow";
            this.radioButton_charshow.Size = new System.Drawing.Size(71, 16);
            this.radioButton_charshow.TabIndex = 1;
            this.radioButton_charshow.Text = "字符显示";
            this.radioButton_charshow.UseVisualStyleBackColor = true;
            // 
            // txt_rxdebug
            // 
            this.txt_rxdebug.Location = new System.Drawing.Point(6, 17);
            this.txt_rxdebug.Multiline = true;
            this.txt_rxdebug.Name = "txt_rxdebug";
            this.txt_rxdebug.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txt_rxdebug.Size = new System.Drawing.Size(681, 329);
            this.txt_rxdebug.TabIndex = 0;
            // 
            // groupBox19
            // 
            this.groupBox19.Controls.Add(this.checkBox_rxdebug);
            this.groupBox19.Controls.Add(this.cmb_sendrate);
            this.groupBox19.Controls.Add(this.cmb_baudrate);
            this.groupBox19.Controls.Add(this.cmb_portname);
            this.groupBox19.Controls.Add(this.BTN_extralcom);
            this.groupBox19.Controls.Add(this.label81);
            this.groupBox19.Controls.Add(this.label80);
            this.groupBox19.Controls.Add(this.label72);
            this.groupBox19.Location = new System.Drawing.Point(3, 301);
            this.groupBox19.Name = "groupBox19";
            this.groupBox19.Size = new System.Drawing.Size(529, 110);
            this.groupBox19.TabIndex = 78;
            this.groupBox19.TabStop = false;
            this.groupBox19.Text = "外接扩展端口";
            // 
            // checkBox_rxdebug
            // 
            this.checkBox_rxdebug.AutoSize = true;
            this.checkBox_rxdebug.Location = new System.Drawing.Point(451, 49);
            this.checkBox_rxdebug.Name = "checkBox_rxdebug";
            this.checkBox_rxdebug.Size = new System.Drawing.Size(48, 16);
            this.checkBox_rxdebug.TabIndex = 44;
            this.checkBox_rxdebug.Text = "调试";
            this.checkBox_rxdebug.UseVisualStyleBackColor = true;
            this.checkBox_rxdebug.CheckedChanged += new System.EventHandler(this.check_rxdebug);
            // 
            // cmb_sendrate
            // 
            this.cmb_sendrate.FormattingEnabled = true;
            this.cmb_sendrate.Items.AddRange(new object[] {
            "20hz",
            "10hz",
            "5hz",
            "3hz",
            "2hz",
            "1hz",
            "0.5hz",
            "0.2hz",
            "0.1hz"});
            this.cmb_sendrate.Location = new System.Drawing.Point(238, 49);
            this.cmb_sendrate.Name = "cmb_sendrate";
            this.cmb_sendrate.Size = new System.Drawing.Size(80, 20);
            this.cmb_sendrate.TabIndex = 43;
            // 
            // cmb_baudrate
            // 
            this.cmb_baudrate.FormattingEnabled = true;
            this.cmb_baudrate.Items.AddRange(new object[] {
            "115200",
            "57600",
            "38400",
            "9600"});
            this.cmb_baudrate.Location = new System.Drawing.Point(120, 49);
            this.cmb_baudrate.Name = "cmb_baudrate";
            this.cmb_baudrate.Size = new System.Drawing.Size(80, 20);
            this.cmb_baudrate.TabIndex = 42;
            // 
            // cmb_portname
            // 
            this.cmb_portname.FormattingEnabled = true;
            this.cmb_portname.Location = new System.Drawing.Point(9, 49);
            this.cmb_portname.Name = "cmb_portname";
            this.cmb_portname.Size = new System.Drawing.Size(80, 20);
            this.cmb_portname.TabIndex = 41;
            this.cmb_portname.Click += new System.EventHandler(this.CMB_PortNameClick);
            // 
            // BTN_extralcom
            // 
            this.BTN_extralcom.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BTN_extralcom.Location = new System.Drawing.Point(370, 49);
            this.BTN_extralcom.Name = "BTN_extralcom";
            this.BTN_extralcom.Size = new System.Drawing.Size(75, 22);
            this.BTN_extralcom.TabIndex = 40;
            this.BTN_extralcom.Text = "Connect";
            this.BTN_extralcom.UseVisualStyleBackColor = true;
            this.BTN_extralcom.Click += new System.EventHandler(this.BTN_extralcom_Click);
            // 
            // label81
            // 
            this.label81.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label81.Location = new System.Drawing.Point(235, 21);
            this.label81.Name = "label81";
            this.label81.Size = new System.Drawing.Size(83, 22);
            this.label81.TabIndex = 38;
            this.label81.Text = "Send Rate";
            // 
            // label80
            // 
            this.label80.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label80.Location = new System.Drawing.Point(117, 21);
            this.label80.Name = "label80";
            this.label80.Size = new System.Drawing.Size(83, 22);
            this.label80.TabIndex = 37;
            this.label80.Text = "Baud Rate";
            // 
            // label72
            // 
            this.label72.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label72.Location = new System.Drawing.Point(6, 21);
            this.label72.Name = "label72";
            this.label72.Size = new System.Drawing.Size(83, 22);
            this.label72.TabIndex = 36;
            this.label72.Text = "Port Name";
            // 
            // groupBox18
            // 
            this.groupBox18.Controls.Add(this.comboBox_video2imageHZ);
            this.groupBox18.Controls.Add(this.label71);
            this.groupBox18.Controls.Add(this.checkBox_video2imagesave);
            this.groupBox18.Controls.Add(this.label_video2state);
            this.groupBox18.Controls.Add(this.myButton22);
            this.groupBox18.Controls.Add(this.BTN_video2record);
            this.groupBox18.Controls.Add(this.myButton24);
            this.groupBox18.Controls.Add(this.txt_video2path);
            this.groupBox18.Controls.Add(this.label69);
            this.groupBox18.Location = new System.Drawing.Point(3, 149);
            this.groupBox18.Name = "groupBox18";
            this.groupBox18.Size = new System.Drawing.Size(529, 114);
            this.groupBox18.TabIndex = 77;
            this.groupBox18.TabStop = false;
            this.groupBox18.Text = "视频流2保存设置";
            // 
            // comboBox_video2imageHZ
            // 
            this.comboBox_video2imageHZ.FormattingEnabled = true;
            this.comboBox_video2imageHZ.Items.AddRange(new object[] {
            "5hz",
            "2hz",
            "1hz",
            "0.5hz",
            "0.2hz",
            "0.1hz"});
            this.comboBox_video2imageHZ.Location = new System.Drawing.Point(243, 88);
            this.comboBox_video2imageHZ.Name = "comboBox_video2imageHZ";
            this.comboBox_video2imageHZ.Size = new System.Drawing.Size(110, 20);
            this.comboBox_video2imageHZ.TabIndex = 48;
            this.comboBox_video2imageHZ.SelectedIndexChanged += new System.EventHandler(this.cmb_video2imagefrequency);
            // 
            // label71
            // 
            this.label71.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label71.Location = new System.Drawing.Point(187, 89);
            this.label71.Name = "label71";
            this.label71.Size = new System.Drawing.Size(47, 22);
            this.label71.TabIndex = 47;
            this.label71.Text = "频率:";
            // 
            // checkBox_video2imagesave
            // 
            this.checkBox_video2imagesave.AutoSize = true;
            this.checkBox_video2imagesave.Location = new System.Drawing.Point(53, 89);
            this.checkBox_video2imagesave.Name = "checkBox_video2imagesave";
            this.checkBox_video2imagesave.Size = new System.Drawing.Size(96, 16);
            this.checkBox_video2imagesave.TabIndex = 45;
            this.checkBox_video2imagesave.Text = "图片形式保存";
            this.checkBox_video2imagesave.UseVisualStyleBackColor = true;
            // 
            // label_video2state
            // 
            this.label_video2state.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_video2state.Location = new System.Drawing.Point(372, 53);
            this.label_video2state.Name = "label_video2state";
            this.label_video2state.Size = new System.Drawing.Size(109, 22);
            this.label_video2state.TabIndex = 44;
            this.label_video2state.Text = "状态:未录制";
            // 
            // myButton22
            // 
            this.myButton22.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.myButton22.Location = new System.Drawing.Point(181, 49);
            this.myButton22.Name = "myButton22";
            this.myButton22.Size = new System.Drawing.Size(94, 22);
            this.myButton22.TabIndex = 42;
            this.myButton22.Text = "停止录制";
            this.myButton22.UseVisualStyleBackColor = true;
            this.myButton22.Click += new System.EventHandler(this.BTN_Video2WriteStop);
            // 
            // BTN_video2record
            // 
            this.BTN_video2record.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BTN_video2record.Location = new System.Drawing.Point(53, 49);
            this.BTN_video2record.Name = "BTN_video2record";
            this.BTN_video2record.Size = new System.Drawing.Size(94, 22);
            this.BTN_video2record.TabIndex = 41;
            this.BTN_video2record.Text = "开始录制";
            this.BTN_video2record.UseVisualStyleBackColor = true;
            this.BTN_video2record.Click += new System.EventHandler(this.BTN_Video2WriteBegin);
            // 
            // myButton24
            // 
            this.myButton24.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.myButton24.Location = new System.Drawing.Point(406, 19);
            this.myButton24.Name = "myButton24";
            this.myButton24.Size = new System.Drawing.Size(75, 22);
            this.myButton24.TabIndex = 40;
            this.myButton24.Text = "浏览";
            this.myButton24.UseVisualStyleBackColor = true;
            this.myButton24.Click += new System.EventHandler(this.BTN_BrowseVideo2PathChange);
            // 
            // txt_video2path
            // 
            this.txt_video2path.Location = new System.Drawing.Point(53, 18);
            this.txt_video2path.Name = "txt_video2path";
            this.txt_video2path.Size = new System.Drawing.Size(318, 21);
            this.txt_video2path.TabIndex = 37;
            // 
            // label69
            // 
            this.label69.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label69.Location = new System.Drawing.Point(6, 19);
            this.label69.Name = "label69";
            this.label69.Size = new System.Drawing.Size(48, 22);
            this.label69.TabIndex = 36;
            this.label69.Text = "路径:";
            // 
            // groupBox17
            // 
            this.groupBox17.Controls.Add(this.label70);
            this.groupBox17.Controls.Add(this.comboBox_video1imageHZ);
            this.groupBox17.Controls.Add(this.checkBox_video1imagesave);
            this.groupBox17.Controls.Add(this.label_video1state);
            this.groupBox17.Controls.Add(this.myButton21);
            this.groupBox17.Controls.Add(this.BTN_video1record);
            this.groupBox17.Controls.Add(this.myButton19);
            this.groupBox17.Controls.Add(this.txt_video1path);
            this.groupBox17.Controls.Add(this.label68);
            this.groupBox17.Location = new System.Drawing.Point(3, 3);
            this.groupBox17.Name = "groupBox17";
            this.groupBox17.Size = new System.Drawing.Size(529, 111);
            this.groupBox17.TabIndex = 76;
            this.groupBox17.TabStop = false;
            this.groupBox17.Text = "视频流1保存设置";
            // 
            // label70
            // 
            this.label70.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label70.Location = new System.Drawing.Point(190, 83);
            this.label70.Name = "label70";
            this.label70.Size = new System.Drawing.Size(47, 22);
            this.label70.TabIndex = 46;
            this.label70.Text = "频率:";
            // 
            // comboBox_video1imageHZ
            // 
            this.comboBox_video1imageHZ.FormattingEnabled = true;
            this.comboBox_video1imageHZ.Items.AddRange(new object[] {
            "5hz",
            "2hz",
            "1hz",
            "0.5hz",
            "0.2hz",
            "0.1hz"});
            this.comboBox_video1imageHZ.Location = new System.Drawing.Point(243, 82);
            this.comboBox_video1imageHZ.Name = "comboBox_video1imageHZ";
            this.comboBox_video1imageHZ.Size = new System.Drawing.Size(110, 20);
            this.comboBox_video1imageHZ.TabIndex = 45;
            this.comboBox_video1imageHZ.SelectedIndexChanged += new System.EventHandler(this.cmb_video1imagefrequency);
            // 
            // checkBox_video1imagesave
            // 
            this.checkBox_video1imagesave.AutoSize = true;
            this.checkBox_video1imagesave.Location = new System.Drawing.Point(53, 85);
            this.checkBox_video1imagesave.Name = "checkBox_video1imagesave";
            this.checkBox_video1imagesave.Size = new System.Drawing.Size(96, 16);
            this.checkBox_video1imagesave.TabIndex = 44;
            this.checkBox_video1imagesave.Text = "图片形式保存";
            this.checkBox_video1imagesave.UseVisualStyleBackColor = true;
            // 
            // label_video1state
            // 
            this.label_video1state.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_video1state.Location = new System.Drawing.Point(372, 48);
            this.label_video1state.Name = "label_video1state";
            this.label_video1state.Size = new System.Drawing.Size(109, 22);
            this.label_video1state.TabIndex = 43;
            this.label_video1state.Text = "状态:未录制";
            // 
            // myButton21
            // 
            this.myButton21.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.myButton21.Location = new System.Drawing.Point(181, 48);
            this.myButton21.Name = "myButton21";
            this.myButton21.Size = new System.Drawing.Size(94, 22);
            this.myButton21.TabIndex = 42;
            this.myButton21.Text = "停止录制";
            this.myButton21.UseVisualStyleBackColor = true;
            this.myButton21.Click += new System.EventHandler(this.BTN_Video1WriteStop);
            // 
            // BTN_video1record
            // 
            this.BTN_video1record.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BTN_video1record.Location = new System.Drawing.Point(53, 48);
            this.BTN_video1record.Name = "BTN_video1record";
            this.BTN_video1record.Size = new System.Drawing.Size(94, 22);
            this.BTN_video1record.TabIndex = 41;
            this.BTN_video1record.Text = "开始录制";
            this.BTN_video1record.UseVisualStyleBackColor = true;
            this.BTN_video1record.Click += new System.EventHandler(this.BTN_Video1WriteBegin);
            // 
            // myButton19
            // 
            this.myButton19.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.myButton19.Location = new System.Drawing.Point(406, 17);
            this.myButton19.Name = "myButton19";
            this.myButton19.Size = new System.Drawing.Size(75, 22);
            this.myButton19.TabIndex = 40;
            this.myButton19.Text = "浏览";
            this.myButton19.UseVisualStyleBackColor = true;
            this.myButton19.Click += new System.EventHandler(this.BTN_BrowseVideo1PathChange);
            // 
            // txt_video1path
            // 
            this.txt_video1path.Location = new System.Drawing.Point(53, 16);
            this.txt_video1path.Name = "txt_video1path";
            this.txt_video1path.Size = new System.Drawing.Size(318, 21);
            this.txt_video1path.TabIndex = 37;
            // 
            // label68
            // 
            this.label68.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label68.Location = new System.Drawing.Point(6, 17);
            this.label68.Name = "label68";
            this.label68.Size = new System.Drawing.Size(48, 22);
            this.label68.TabIndex = 36;
            this.label68.Text = "路径:";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label_localip);
            this.groupBox5.Controls.Add(this.UDP_receive);
            this.groupBox5.Controls.Add(this.textBox_localip);
            this.groupBox5.Controls.Add(this.textBox_localport);
            this.groupBox5.Controls.Add(this.label_localport);
            this.groupBox5.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox5.Location = new System.Drawing.Point(12, 429);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(520, 70);
            this.groupBox5.TabIndex = 75;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "UDP接收端口设置";
            // 
            // label_localip
            // 
            this.label_localip.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_localip.Location = new System.Drawing.Point(6, 34);
            this.label_localip.Name = "label_localip";
            this.label_localip.Size = new System.Drawing.Size(83, 22);
            this.label_localip.TabIndex = 35;
            this.label_localip.Text = "Local IP:";
            // 
            // UDP_receive
            // 
            this.UDP_receive.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.UDP_receive.Location = new System.Drawing.Point(392, 34);
            this.UDP_receive.Name = "UDP_receive";
            this.UDP_receive.Size = new System.Drawing.Size(75, 22);
            this.UDP_receive.TabIndex = 39;
            this.UDP_receive.Text = "Connect";
            this.UDP_receive.UseVisualStyleBackColor = true;
            this.UDP_receive.Click += new System.EventHandler(this.BUT_UDPReceiveConnect);
            // 
            // textBox_localip
            // 
            this.textBox_localip.BackColor = System.Drawing.SystemColors.Control;
            this.textBox_localip.Font = new System.Drawing.Font("宋体", 9F);
            this.textBox_localip.Location = new System.Drawing.Point(95, 34);
            this.textBox_localip.Name = "textBox_localip";
            this.textBox_localip.Size = new System.Drawing.Size(89, 21);
            this.textBox_localip.TabIndex = 36;
            this.textBox_localip.Text = "127.0.0.1";
            // 
            // textBox_localport
            // 
            this.textBox_localport.BackColor = System.Drawing.SystemColors.Control;
            this.textBox_localport.Font = new System.Drawing.Font("宋体", 9F);
            this.textBox_localport.Location = new System.Drawing.Point(282, 34);
            this.textBox_localport.Name = "textBox_localport";
            this.textBox_localport.Size = new System.Drawing.Size(89, 21);
            this.textBox_localport.TabIndex = 38;
            this.textBox_localport.Text = "14550";
            // 
            // label_localport
            // 
            this.label_localport.Font = new System.Drawing.Font("宋体", 9F);
            this.label_localport.Location = new System.Drawing.Point(190, 34);
            this.label_localport.Name = "label_localport";
            this.label_localport.Size = new System.Drawing.Size(97, 22);
            this.label_localport.TabIndex = 37;
            this.label_localport.Text = "Local Port:";
            // 
            // BUT_Updatepos
            // 
            this.BUT_Updatepos.Enabled = false;
            this.BUT_Updatepos.Location = new System.Drawing.Point(793, 12);
            this.BUT_Updatepos.Name = "BUT_Updatepos";
            this.BUT_Updatepos.Size = new System.Drawing.Size(75, 23);
            this.BUT_Updatepos.TabIndex = 10;
            this.BUT_Updatepos.Text = "Update Pos";
            this.BUT_Updatepos.UseVisualStyleBackColor = true;
            this.BUT_Updatepos.Click += new System.EventHandler(this.BUT_Updatepos_Click);
            // 
            // PNL_status
            // 
            this.PNL_status.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PNL_status.AutoScroll = true;
            this.PNL_status.Location = new System.Drawing.Point(1256, 61);
            this.PNL_status.Name = "PNL_status";
            this.PNL_status.Size = new System.Drawing.Size(147, 680);
            this.PNL_status.TabIndex = 11;
            // 
            // timer1_status
            // 
            this.timer1_status.Enabled = true;
            this.timer1_status.Interval = 200;
            this.timer1_status.Tick += new System.EventHandler(this.timer1_status_Tick);
            // 
            // but_guided
            // 
            this.but_guided.Location = new System.Drawing.Point(631, 12);
            this.but_guided.Name = "but_guided";
            this.but_guided.Size = new System.Drawing.Size(75, 23);
            this.but_guided.TabIndex = 12;
            this.but_guided.Text = "Guided Mode";
            this.but_guided.UseVisualStyleBackColor = true;
            this.but_guided.Click += new System.EventHandler(this.but_guided_Click);
            // 
            // timer2
            // 
            this.timer2.Interval = 50;
            this.timer2.Tick += new System.EventHandler(this.Time2_Tick);
            // 
            // myButton_RTL
            // 
            this.myButton_RTL.Location = new System.Drawing.Point(712, 12);
            this.myButton_RTL.Name = "myButton_RTL";
            this.myButton_RTL.Size = new System.Drawing.Size(75, 23);
            this.myButton_RTL.TabIndex = 40;
            this.myButton_RTL.Text = "RTL";
            this.myButton_RTL.UseVisualStyleBackColor = true;
            this.myButton_RTL.Click += new System.EventHandler(this.BTN_RTL);
            // 
            // BUT_Auto
            // 
            this.BUT_Auto.Location = new System.Drawing.Point(336, 12);
            this.BUT_Auto.Name = "BUT_Auto";
            this.BUT_Auto.Size = new System.Drawing.Size(75, 23);
            this.BUT_Auto.TabIndex = 41;
            this.BUT_Auto.Text = "Auto";
            this.BUT_Auto.UseVisualStyleBackColor = true;
            this.BUT_Auto.Click += new System.EventHandler(this.BUT_Auto_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeToolStripMenuItem,
            this.ChangeToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(101, 48);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.removeToolStripMenuItem.Text = "删除";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // ChangeToolStripMenuItem
            // 
            this.ChangeToolStripMenuItem.Name = "ChangeToolStripMenuItem";
            this.ChangeToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.ChangeToolStripMenuItem.Text = "修改";
            this.ChangeToolStripMenuItem.Click += new System.EventHandler(this.ChangeToolStripMenuItem_Click);
            // 
            // ZedGraphTimer
            // 
            this.ZedGraphTimer.Tick += new System.EventHandler(this.ZedGraphTimer_Tick);
            // 
            // timer3
            // 
            this.timer3.Tick += new System.EventHandler(this.Timer3_Tick);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_DispalyOut});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(149, 26);
            // 
            // toolStripMenuItem_DispalyOut
            // 
            this.toolStripMenuItem_DispalyOut.Name = "toolStripMenuItem_DispalyOut";
            this.toolStripMenuItem_DispalyOut.Size = new System.Drawing.Size(148, 22);
            this.toolStripMenuItem_DispalyOut.Text = "视频独立显示";
            this.toolStripMenuItem_DispalyOut.Click += new System.EventHandler(this.VideoDisplayOut_Click);
            // 
            // timer4
            // 
            this.timer4.Interval = 50;
            this.timer4.Tick += new System.EventHandler(this.Time4_Click);
            // 
            // timer5
            // 
            this.timer5.Interval = 20;
            this.timer5.Tick += new System.EventHandler(this.timer5_Tick);
            // 
            // Btn_Stop
            // 
            this.Btn_Stop.Enabled = false;
            this.Btn_Stop.Location = new System.Drawing.Point(874, 12);
            this.Btn_Stop.Name = "Btn_Stop";
            this.Btn_Stop.Size = new System.Drawing.Size(75, 23);
            this.Btn_Stop.TabIndex = 42;
            this.Btn_Stop.Text = "急停";
            this.Btn_Stop.UseVisualStyleBackColor = true;
            this.Btn_Stop.Click += new System.EventHandler(this.Btn_Stop_Click);
            // 
            // distBwtLeadToIntercept
            // 
            this.distBwtLeadToIntercept.BackColor = System.Drawing.Color.Transparent;
            this.distBwtLeadToIntercept.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.distBwtLeadToIntercept.ForeColor = System.Drawing.Color.Black;
            this.distBwtLeadToIntercept.Location = new System.Drawing.Point(1036, 14);
            this.distBwtLeadToIntercept.Name = "distBwtLeadToIntercept";
            this.distBwtLeadToIntercept.Size = new System.Drawing.Size(117, 19);
            this.distBwtLeadToIntercept.TabIndex = 43;
            this.distBwtLeadToIntercept.Tag = "custom";
            this.distBwtLeadToIntercept.Text = "距离:0.00米";
            // 
            // Btn_DoExtralCommand
            // 
            this.Btn_DoExtralCommand.Location = new System.Drawing.Point(1165, 12);
            this.Btn_DoExtralCommand.Name = "Btn_DoExtralCommand";
            this.Btn_DoExtralCommand.Size = new System.Drawing.Size(48, 23);
            this.Btn_DoExtralCommand.TabIndex = 44;
            this.Btn_DoExtralCommand.Text = "执行";
            this.Btn_DoExtralCommand.UseVisualStyleBackColor = true;
            this.Btn_DoExtralCommand.Click += new System.EventHandler(this.Btn_DoExtralCommand_Click);
            // 
            // Btn_DoCancelCommand
            // 
            this.Btn_DoCancelCommand.Location = new System.Drawing.Point(1219, 12);
            this.Btn_DoCancelCommand.Name = "Btn_DoCancelCommand";
            this.Btn_DoCancelCommand.Size = new System.Drawing.Size(45, 23);
            this.Btn_DoCancelCommand.TabIndex = 45;
            this.Btn_DoCancelCommand.Text = "中止";
            this.Btn_DoCancelCommand.UseVisualStyleBackColor = true;
            this.Btn_DoCancelCommand.Click += new System.EventHandler(this.Btn_DoCancelCommandClick);
            // 
            // heightToIntercept
            // 
            this.heightToIntercept.BackColor = System.Drawing.Color.Transparent;
            this.heightToIntercept.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.heightToIntercept.ForeColor = System.Drawing.Color.Black;
            this.heightToIntercept.Location = new System.Drawing.Point(1036, 39);
            this.heightToIntercept.Name = "heightToIntercept";
            this.heightToIntercept.Size = new System.Drawing.Size(117, 19);
            this.heightToIntercept.TabIndex = 46;
            this.heightToIntercept.Tag = "custom";
            this.heightToIntercept.Text = "高度差:0.00米";
            // 
            // label111
            // 
            this.label111.AutoSize = true;
            this.label111.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label111.Location = new System.Drawing.Point(6, 73);
            this.label111.Name = "label111";
            this.label111.Size = new System.Drawing.Size(89, 12);
            this.label111.TabIndex = 93;
            this.label111.Text = "UDP_TRANSFER_E";
            // 
            // udp_transfer_enable
            // 
            this.udp_transfer_enable.Location = new System.Drawing.Point(147, 69);
            this.udp_transfer_enable.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udp_transfer_enable.Name = "udp_transfer_enable";
            this.udp_transfer_enable.Size = new System.Drawing.Size(54, 21);
            this.udp_transfer_enable.TabIndex = 94;
            // 
            // FormationControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1404, 753);
            this.Controls.Add(this.heightToIntercept);
            this.Controls.Add(this.Btn_DoCancelCommand);
            this.Controls.Add(this.Btn_DoExtralCommand);
            this.Controls.Add(this.distBwtLeadToIntercept);
            this.Controls.Add(this.Btn_Stop);
            this.Controls.Add(this.BUT_Auto);
            this.Controls.Add(this.myButton_RTL);
            this.Controls.Add(this.but_guided);
            this.Controls.Add(this.PNL_status);
            this.Controls.Add(this.BUT_Updatepos);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.BUT_Start);
            this.Controls.Add(this.BUT_leader);
            this.Controls.Add(this.CMB_mavs);
            this.Controls.Add(this.BUT_Land);
            this.Controls.Add(this.BUT_Takeoff);
            this.Controls.Add(this.BUT_Disarm);
            this.Controls.Add(this.BUT_Arm);
            this.Name = "FormationControl";
            this.Text = "Swarm Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Control_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource_1)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox24.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_offsety)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_offsetx)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource_4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_offsetz)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox14.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource_2)).EndInit();
            this.panel_PID_param.ResumeLayout(false);
            this.panel_PID_param.PerformLayout();
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.virtualleaderenable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.min_approach_times)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fail_action)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.avoidance_enable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.avoidance_horizontal_m)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.avoidance_vertical_m)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fail_alt_min_m)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.warn_distance_z_m)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.warn_distance_xy_m)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fail_distance_xy_m)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fail_distance_z_m)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.warn_time_horizon_s)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fail_time_horizon_s)).EndInit();
            this.groupBox9.ResumeLayout(false);
            this.groupBox9.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AP_oil_power)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.copter_takeoff_m)).EndInit();
            this.groupBox8.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.speed_FILT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.speed_IMAX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.speed_D)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.speed_I)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.speed_P)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.stall_protect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pre_distance_to_lose_speed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.min_target_speed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.thrust_min)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.thrust_trim)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.delta_speed_scale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dist_integral_separation_m)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.max_dist_to_delta_speed_m)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yaw_to_roll_max_angle_deg)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.roll_min_disttotarget_m)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.roll_high_dist_boundary_m)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.roll_low_dist_boundary_m)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yaw_distance_boundary_m)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.target_leader_dist_m)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.target_trailer_scale)).EndInit();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.thrust_FILT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.thrust_IMAX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.thrust_D)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.thrust_I)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.thrust_P)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.yaw_FILT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yaw_IMAX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yaw_D)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yaw_I)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yaw_P)).EndInit();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pitch_FILT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pitch_IMAX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pitch_D)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pitch_I)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pitch_P)).EndInit();
            this.groupBox25.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.roll_FILT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.roll_IMAX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.roll_D)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.roll_I)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.roll_P)).EndInit();
            this.tabPage3.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
            this.splitContainer5.ResumeLayout(false);
            this.splitContainer6.Panel1.ResumeLayout(false);
            this.splitContainer6.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).EndInit();
            this.splitContainer6.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox13.ResumeLayout(false);
            this.groupBox13.PerformLayout();
            this.groupBox12.ResumeLayout(false);
            this.groupBox12.PerformLayout();
            this.groupBox11.ResumeLayout(false);
            this.groupBox11.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_swarmspace)).EndInit();
            this.tabPage6.ResumeLayout(false);
            this.splitContainer7.Panel1.ResumeLayout(false);
            this.splitContainer7.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer7)).EndInit();
            this.splitContainer7.ResumeLayout(false);
            this.groupBox16.ResumeLayout(false);
            this.groupBox16.PerformLayout();
            this.groupBox15.ResumeLayout(false);
            this.groupBox15.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tabPage7.ResumeLayout(false);
            this.splitContainer8.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer8)).EndInit();
            this.splitContainer8.ResumeLayout(false);
            this.tabPage_voiceIdentification.ResumeLayout(false);
            this.groupBox22.ResumeLayout(false);
            this.groupBox21.ResumeLayout(false);
            this.groupBox21.PerformLayout();
            this.tabPage_targetserach.ResumeLayout(false);
            this.splitContainer9.Panel1.ResumeLayout(false);
            this.splitContainer9.Panel2.ResumeLayout(false);
            this.splitContainer9.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer9)).EndInit();
            this.splitContainer9.ResumeLayout(false);
            this.splitContainer10.Panel1.ResumeLayout(false);
            this.splitContainer10.Panel1.PerformLayout();
            this.splitContainer10.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer10)).EndInit();
            this.splitContainer10.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_overshot)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_searchalt)).EndInit();
            this.groupBox23.ResumeLayout(false);
            this.groupBox23.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.headingChangeInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.virtualTargetSpeed)).EndInit();
            this.GmapcontextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.track_zoom)).EndInit();
            this.tabPage_HIL.ResumeLayout(false);
            this.splitContainer11.Panel1.ResumeLayout(false);
            this.splitContainer11.Panel1.PerformLayout();
            this.splitContainer11.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer11)).EndInit();
            this.splitContainer11.ResumeLayout(false);
            this.splitContainer12.Panel1.ResumeLayout(false);
            this.splitContainer12.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer12)).EndInit();
            this.splitContainer12.ResumeLayout(false);
            this.groupBox29.ResumeLayout(false);
            this.groupBox29.PerformLayout();
            this.groupBox28.ResumeLayout(false);
            this.groupBox28.PerformLayout();
            this.groupBox27.ResumeLayout(false);
            this.groupBox27.PerformLayout();
            this.groupBox26.ResumeLayout(false);
            this.groupBox26.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.groupBox30.ResumeLayout(false);
            this.groupBox30.PerformLayout();
            this.groupBox20.ResumeLayout(false);
            this.groupBox20.PerformLayout();
            this.groupbox_rxdebug.ResumeLayout(false);
            this.groupbox_rxdebug.PerformLayout();
            this.groupBox19.ResumeLayout(false);
            this.groupBox19.PerformLayout();
            this.groupBox18.ResumeLayout(false);
            this.groupBox18.PerformLayout();
            this.groupBox17.ResumeLayout(false);
            this.groupBox17.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource_3)).EndInit();
            this.contextMenuStrip2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.udp_transfer_enable)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.MyButton BUT_Arm;
        private Controls.MyButton BUT_Disarm;
        private Controls.MyButton BUT_Takeoff;
        private Controls.MyButton BUT_Land;
        private System.Windows.Forms.ComboBox CMB_mavs;
        private Controls.MyButton BUT_leader;
        private Controls.MyButton BUT_Start;
        private Grid grid1;
        private System.Windows.Forms.BindingSource bindingSource_1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private Controls.MyButton BUT_Updatepos;
        private System.Windows.Forms.FlowLayoutPanel PNL_status;
        private System.Windows.Forms.Timer timer1_status;
        private Controls.MyButton but_guided;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Label label_localip;
        private System.Windows.Forms.TextBox textBox_localip;
        private System.Windows.Forms.Label label_localport;
        private System.Windows.Forms.TextBox textBox_localport;
        private Controls.MyButton UDP_receive;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Panel panel_PID_param;
        private System.Windows.Forms.GroupBox groupBox25;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label88;
        private System.Windows.Forms.Label label90;
        private System.Windows.Forms.Label label91;
        private System.Windows.Forms.NumericUpDown roll_P;
        private System.Windows.Forms.NumericUpDown roll_D;
        private System.Windows.Forms.NumericUpDown roll_I;
        private System.Windows.Forms.NumericUpDown roll_FILT;
        private System.Windows.Forms.NumericUpDown roll_IMAX;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown pitch_FILT;
        private System.Windows.Forms.NumericUpDown pitch_IMAX;
        private System.Windows.Forms.NumericUpDown pitch_D;
        private System.Windows.Forms.NumericUpDown pitch_I;
        private System.Windows.Forms.NumericUpDown pitch_P;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.NumericUpDown thrust_FILT;
        private System.Windows.Forms.NumericUpDown thrust_IMAX;
        private System.Windows.Forms.NumericUpDown thrust_D;
        private System.Windows.Forms.NumericUpDown thrust_I;
        private System.Windows.Forms.NumericUpDown thrust_P;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown yaw_FILT;
        private System.Windows.Forms.NumericUpDown yaw_IMAX;
        private System.Windows.Forms.NumericUpDown yaw_D;
        private System.Windows.Forms.NumericUpDown yaw_I;
        private System.Windows.Forms.NumericUpDown yaw_P;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private Controls.MyButton BUT_writePIDS;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox CMB_pid;
        private System.Windows.Forms.CheckBox CHK_lockallmav;
        private System.Windows.Forms.BindingSource bindingSource_2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private Controls.MyButton myButton_RTL;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.NumericUpDown target_trailer_scale;
        private System.Windows.Forms.NumericUpDown yaw_to_roll_max_angle_deg;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.NumericUpDown roll_min_disttotarget_m;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.NumericUpDown roll_high_dist_boundary_m;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.NumericUpDown roll_low_dist_boundary_m;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.NumericUpDown yaw_distance_boundary_m;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.NumericUpDown target_leader_dist_m;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private Grid grid2;
        private Controls.MyButton myButton_swarmspace;
        private System.Windows.Forms.TextBox textBox_swarmspace;
        private System.Windows.Forms.TrackBar trackBar_swarmspace;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.ListBox listBox_formationShape;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.ComboBox comboBox_formation_select;
        private Controls.MyButton myButton2;
        private Controls.MyButton myButton1;
        private System.Windows.Forms.TextBox textBox_filepath;
        private Controls.MyButton myButton_Transformation;
        private Controls.MyButton BUT_Auto;
        private System.Windows.Forms.CheckBox checkBox_AllAdvanceParam;
        private Controls.MyButton myButton3;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ChangeToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.ComboBox comboBox_chartshow;
        private System.Windows.Forms.SplitContainer splitContainer6;
        private System.Windows.Forms.FlowLayoutPanel variable_check;
        private ZedGraph.ZedGraphControl zg1;
        private System.Windows.Forms.Timer ZedGraphTimer;
        private System.Windows.Forms.Timer timer3;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Label label_lowaltcount;
        private System.Windows.Forms.Label label_lowspeedcount;
        private System.Windows.Forms.Label label_connectcount;
        private System.Windows.Forms.Label label_heathlinkcount;
        private System.Windows.Forms.Label label_guidedcount;
        private System.Windows.Forms.Label label_armcount;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.NumericUpDown speed_FILT;
        private System.Windows.Forms.NumericUpDown speed_IMAX;
        private System.Windows.Forms.NumericUpDown speed_D;
        private System.Windows.Forms.NumericUpDown speed_I;
        private System.Windows.Forms.NumericUpDown speed_P;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.Label label38;
        private System.Windows.Forms.Label label39;
        private System.Windows.Forms.NumericUpDown delta_speed_scale;
        private System.Windows.Forms.NumericUpDown dist_integral_separation_m;
        private System.Windows.Forms.Label label42;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.NumericUpDown max_dist_to_delta_speed_m;
        private System.Windows.Forms.Label label40;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.NumericUpDown copter_takeoff_m;
        private System.Windows.Forms.Label label43;
        private System.Windows.Forms.Label label44;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.NumericUpDown fail_time_horizon_s;
        private System.Windows.Forms.Label label45;
        private System.Windows.Forms.NumericUpDown warn_distance_xy_m;
        private System.Windows.Forms.NumericUpDown fail_distance_xy_m;
        private System.Windows.Forms.NumericUpDown fail_distance_z_m;
        private System.Windows.Forms.NumericUpDown warn_time_horizon_s;
        private System.Windows.Forms.Label label49;
        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.Label label47;
        private System.Windows.Forms.Label label46;
        private System.Windows.Forms.Label label51;
        private System.Windows.Forms.NumericUpDown warn_distance_z_m;
        private System.Windows.Forms.Label label50;
        private System.Windows.Forms.NumericUpDown fail_alt_min_m;
        private System.Windows.Forms.Label label53;
        private System.Windows.Forms.Label label52;
        private System.Windows.Forms.NumericUpDown avoidance_horizontal_m;
        private System.Windows.Forms.NumericUpDown avoidance_vertical_m;
        private System.Windows.Forms.NumericUpDown avoidance_enable;
        private System.Windows.Forms.Label label54;
        private System.Windows.Forms.GroupBox groupBox13;
        private System.Windows.Forms.Label label_maybecollisiontimes;
        private System.Windows.Forms.Label label83;
        private System.Windows.Forms.Label label79;
        private System.Windows.Forms.Label label_totalavoidancecraftid;
        private System.Windows.Forms.Label label82;
        private System.Windows.Forms.Label label_avoidancecraftcounts;
        private System.Windows.Forms.GroupBox groupBox12;
        private System.Windows.Forms.Label label_avoidancetargetalt2;
        private System.Windows.Forms.Label label_avoidancecraftid2;
        private System.Windows.Forms.Label label_obstacleid2;
        private System.Windows.Forms.Label label_craftdisttoobstacle2;
        private System.Windows.Forms.Label label_avoidanceeastoffset2;
        private System.Windows.Forms.Label label_avoidancenorthoffset2;
        private System.Windows.Forms.Label label73;
        private System.Windows.Forms.Label label74;
        private System.Windows.Forms.Label label75;
        private System.Windows.Forms.Label label76;
        private System.Windows.Forms.Label label77;
        private System.Windows.Forms.Label label78;
        private System.Windows.Forms.GroupBox groupBox11;
        private System.Windows.Forms.Label label_avoidancetargetalt1;
        private System.Windows.Forms.Label label_avoidancecraftid1;
        private System.Windows.Forms.Label label_obstacleid1;
        private System.Windows.Forms.Label label_craftdisttoobstacle1;
        private System.Windows.Forms.Label label_avoidanceeastoffset1;
        private System.Windows.Forms.Label label_avoidancenorthoffset1;
        private System.Windows.Forms.Label label61;
        private System.Windows.Forms.Label label62;
        private System.Windows.Forms.Label label63;
        private System.Windows.Forms.Label label64;
        private System.Windows.Forms.Label label65;
        private System.Windows.Forms.Label label66;
        private System.Windows.Forms.NumericUpDown fail_action;
        private System.Windows.Forms.Label label85;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label55;
        private System.Windows.Forms.NumericUpDown min_approach_times;
        private Controls.MyButton myButton9;
        private Controls.MyButton myButton8;
        private Controls.MyButton myButton7;
        private Controls.MyButton myButton6;
        private Controls.MyButton myButton5;
        private Controls.MyButton myButton4;
        private Controls.MyButton myButton14;
        private Controls.MyButton myButton13;
        private Controls.MyButton myButton12;
        private Controls.MyButton myButton11;
        private Controls.MyButton myButton10;
        private System.Windows.Forms.Label label56;
        private System.Windows.Forms.GroupBox groupBox14;
        private System.Windows.Forms.NumericUpDown virtualleaderenable;
        private System.Windows.Forms.Label label57;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.SplitContainer splitContainer7;
        private System.Windows.Forms.GroupBox groupBox15;
        private System.Windows.Forms.Label label92;
        private System.Windows.Forms.Label label58;
        private Controls.MyButton BUT_video1start;
        private Controls.MyButton BUT_video1stop;
        private System.Windows.Forms.GroupBox groupBox16;
        private System.Windows.Forms.ComboBox CMB_video2resolutions;
        private Controls.MyButton BUT_video2stop;
        private Controls.MyButton BUT_video2start;
        private System.Windows.Forms.Label label59;
        private System.Windows.Forms.Label label60;
        private System.Windows.Forms.ComboBox CMB_video1resolutions;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private UserVideoHudShow userVideo1HudShow;
        private UserVideoHudShow userVideo2HudShow;
        public System.Windows.Forms.ComboBox CMB_video1sources;
        public System.Windows.Forms.ComboBox CMB_video2sources;
        private Controls.MyButton myButton16;
        private Controls.MyButton myButton15;
        private System.Windows.Forms.TabPage tabPage7;
        private System.Windows.Forms.SplitContainer splitContainer8;
        private Controls.MyButton BTN_Show3dMap;
        private System.Windows.Forms.ComboBox CMB_3DMAP;
        private System.Windows.Forms.Label label67;
        private System.Windows.Forms.BindingSource bindingSource_3;
        private Controls.MyButton myButton17;
        private Controls.MyButton myButton18;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_DispalyOut;
        private System.Windows.Forms.CheckBox checkBox_VLmode;
        private System.Windows.Forms.GroupBox groupBox18;
        private Controls.MyButton myButton22;
        private Controls.MyButton BTN_video2record;
        private Controls.MyButton myButton24;
        private System.Windows.Forms.TextBox txt_video2path;
        private System.Windows.Forms.Label label69;
        private System.Windows.Forms.GroupBox groupBox17;
        private Controls.MyButton myButton21;
        private Controls.MyButton BTN_video1record;
        private Controls.MyButton myButton19;
        private System.Windows.Forms.TextBox txt_video1path;
        private System.Windows.Forms.Label label68;
        private System.Windows.Forms.Label label_video2state;
        private System.Windows.Forms.Label label_video1state;
        private System.Windows.Forms.ComboBox comboBox_video2imageHZ;
        private System.Windows.Forms.Label label71;
        private System.Windows.Forms.CheckBox checkBox_video2imagesave;
        private System.Windows.Forms.Label label70;
        private System.Windows.Forms.ComboBox comboBox_video1imageHZ;
        private System.Windows.Forms.CheckBox checkBox_video1imagesave;
        private System.Windows.Forms.GroupBox groupBox19;
        private System.Windows.Forms.Label label80;
        private System.Windows.Forms.Label label72;
        private System.Windows.Forms.ComboBox cmb_sendrate;
        private System.Windows.Forms.ComboBox cmb_baudrate;
        private System.Windows.Forms.ComboBox cmb_portname;
        private Controls.MyButton BTN_extralcom;
        private System.Windows.Forms.Label label81;
        private System.Windows.Forms.Timer timer4;
        private System.Windows.Forms.GroupBox groupbox_rxdebug;
        private Controls.MyButton mybutton_datashowpause;
        private Controls.MyButton myButton20;
        private System.Windows.Forms.RadioButton radioButton_hexshow;
        private System.Windows.Forms.RadioButton radioButton_charshow;
        private System.Windows.Forms.TextBox txt_rxdebug;
        private System.Windows.Forms.CheckBox checkBox_rxdebug;
        private System.Windows.Forms.GroupBox groupBox20;
        private System.Windows.Forms.Label label84;
        private Controls.MyButton UDP_send;
        private System.Windows.Forms.TextBox txt_remoteip;
        private System.Windows.Forms.TextBox txt_remoteport;
        private System.Windows.Forms.Label label86;
        private System.Windows.Forms.ComboBox CMB_UdpSendRate;
        private System.Windows.Forms.TabPage tabPage_voiceIdentification;
        private System.Windows.Forms.TabPage tabPage_targetserach;
        private System.Windows.Forms.GroupBox groupBox21;
        private System.Windows.Forms.TextBox txt_Identificateresult;
        private Controls.MyButton myButton25;
        private Controls.MyButton myButton23;
        private System.Windows.Forms.Label lbl_SpeechRecognition;
        private System.Windows.Forms.GroupBox groupBox22;
        private System.Windows.Forms.Label lbl_fourthgrammer;
        private System.Windows.Forms.Label lbl_thirdgrammer;
        private System.Windows.Forms.Label lbl_secondgrammer;
        private System.Windows.Forms.Label lbl_firstgrammer;
        private System.Windows.Forms.Label label87;
        private System.Windows.Forms.SplitContainer splitContainer9;
        private System.Windows.Forms.SplitContainer splitContainer10;
        private System.Windows.Forms.ListBox targetsFound;
        private Controls.MyTrackBar track_zoom;
        private Controls.MyButton BTN_search;
        private System.Windows.Forms.Label lbl_targetfoundcounts;
        private System.Windows.Forms.Label label94;
        private System.Windows.Forms.Label lbl_serachareacounts;
        private System.Windows.Forms.Label label89;
        public Controls.myGMAP gMap;
        private Controls.MyButton BTN_searchUpdate;
        private System.Windows.Forms.ContextMenuStrip GmapcontextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem showStartPositionToolStripMenuItem;
        private System.Windows.Forms.RadioButton radioButton_targetsearch;
        private System.Windows.Forms.RadioButton radioButton_tragettrack;
        private Controls.MyButton BTN_searchsimulation;
        private System.Windows.Forms.GroupBox groupBox23;
        private Controls.MyButton BTN_virtualTargetParmSet;
        private System.Windows.Forms.NumericUpDown headingChangeInterval;
        private System.Windows.Forms.Label label93;
        private System.Windows.Forms.Label speed;
        private System.Windows.Forms.NumericUpDown virtualTargetSpeed;
        private Controls.MyButton BTN_SearchAlt;
        private System.Windows.Forms.NumericUpDown numericUpDown_searchalt;
        private System.Windows.Forms.Label label95;
        private System.Windows.Forms.NumericUpDown numericUpDown_overshot;
        private System.Windows.Forms.Label label96;
        private System.Windows.Forms.NumericUpDown thrust_trim;
        private System.Windows.Forms.Label label97;
        private System.Windows.Forms.NumericUpDown thrust_min;
        private System.Windows.Forms.Label label98;
        private System.Windows.Forms.NumericUpDown min_target_speed;
        private System.Windows.Forms.Label label99;
        private System.Windows.Forms.NumericUpDown pre_distance_to_lose_speed;
        private System.Windows.Forms.Label label100;
        private System.Windows.Forms.ComboBox comboBox_mavsoffset;
        private System.Windows.Forms.GroupBox groupBox24;
        private Controls.MyButton myButton26;
        private System.Windows.Forms.NumericUpDown numericUpDown_offsetz;
        private System.Windows.Forms.BindingSource bindingSource_4;
        private System.Windows.Forms.NumericUpDown numericUpDown_offsety;
        private System.Windows.Forms.NumericUpDown numericUpDown_offsetx;
        private System.Windows.Forms.NumericUpDown AP_oil_power;
        private System.Windows.Forms.Label label101;
        private System.Windows.Forms.TabPage tabPage_HIL;
        private System.Windows.Forms.SplitContainer splitContainer11;
        private System.Windows.Forms.RadioButton radioButton_jsbsim;
        private System.Windows.Forms.RadioButton radioButton_aersimrc;
        private System.Windows.Forms.RadioButton radioButton_flight;
        private System.Windows.Forms.RadioButton radioButton_matlab;
        private System.Windows.Forms.RadioButton radioButton_xplane;
        private Controls.MyButton BTN_SimStart;
        private System.Windows.Forms.SplitContainer splitContainer12;
        private System.Windows.Forms.CheckBox checkBox_REVthr;
        private System.Windows.Forms.CheckBox checkBox_REVyaw;
        private System.Windows.Forms.CheckBox checkBox_REVpitch;
        private System.Windows.Forms.CheckBox checkBox_REVroll;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_udplinks;
        private System.Windows.Forms.GroupBox groupBox26;
        private System.Windows.Forms.Label label104;
        private System.Windows.Forms.Label label103;
        private System.Windows.Forms.Label label102;
        private System.Windows.Forms.Label label107;
        private System.Windows.Forms.Label label106;
        private System.Windows.Forms.Label label105;
        private System.Windows.Forms.GroupBox groupBox27;
        private System.Windows.Forms.Label lbl_az;
        private System.Windows.Forms.Label lbl_ay;
        private System.Windows.Forms.Label lbl_ax;
        private System.Windows.Forms.Label lbl_gz;
        private System.Windows.Forms.Label lbl_gy;
        private System.Windows.Forms.Label lbl_gx;
        private System.Windows.Forms.Label label120;
        private System.Windows.Forms.Label label121;
        private System.Windows.Forms.Label label122;
        private System.Windows.Forms.Label label123;
        private System.Windows.Forms.Label label124;
        private System.Windows.Forms.Label label125;
        private System.Windows.Forms.Label lbl_vz;
        private System.Windows.Forms.Label lbl_vy;
        private System.Windows.Forms.Label lbl_vx;
        private System.Windows.Forms.Label lbl_alt;
        private System.Windows.Forms.Label lbl_lng;
        private System.Windows.Forms.Label lbl_lat;
        private System.Windows.Forms.GroupBox groupBox29;
        private System.Windows.Forms.Label lbl_servo4;
        private System.Windows.Forms.Label lbl_servo3;
        private System.Windows.Forms.Label lbl_servo2;
        private System.Windows.Forms.Label lbl_servo1;
        private System.Windows.Forms.Label label138;
        private System.Windows.Forms.Label label139;
        private System.Windows.Forms.Label label140;
        private System.Windows.Forms.Label label141;
        private System.Windows.Forms.GroupBox groupBox28;
        private System.Windows.Forms.Label lbl_heading;
        private System.Windows.Forms.Label lbl_yaw;
        private System.Windows.Forms.Label lbl_roll;
        private System.Windows.Forms.Label lbl_pitch;
        private System.Windows.Forms.Label label134;
        private System.Windows.Forms.Label label135;
        private System.Windows.Forms.Label label136;
        private System.Windows.Forms.Label label137;
        private System.Windows.Forms.Timer timer5;
        private System.Windows.Forms.NumericUpDown stall_protect;
        private System.Windows.Forms.Label label108;
        private Controls.MyButton Btn_Stop;
        private System.Windows.Forms.Label distBwtLeadToIntercept;
        private Controls.MyButton Btn_DoExtralCommand;
        private Controls.MyButton Btn_DoCancelCommand;
        private System.Windows.Forms.Label heightToIntercept;
        private System.Windows.Forms.GroupBox groupBox30;
        private System.Windows.Forms.Label label109;
        private Controls.MyButton btn_mirrorUdpForward;
        private System.Windows.Forms.RadioButton rb_autoSetMirrorUdp;
        private System.Windows.Forms.TextBox textBox_mirrorUdpPortInfo;
        private System.Windows.Forms.RadioButton rb_mannalSetMirrorUdp;
        private System.Windows.Forms.TextBox textBox_intervalMirrorUdpPort;
        private System.Windows.Forms.Label label110;
        private System.Windows.Forms.TextBox textBox_BeginMirrorUdpPort;
        private System.Windows.Forms.NumericUpDown udp_transfer_enable;
        private System.Windows.Forms.Label label111;
    }
}