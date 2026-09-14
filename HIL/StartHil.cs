using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MissionPlanner.HIL
{
    public partial class StartHil: Form
    {
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox_sendport = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_revport = new System.Windows.Forms.TextBox();
            this.lbl_udpport = new System.Windows.Forms.Label();
            this.textBox_ip = new System.Windows.Forms.TextBox();
            this.lbl_udpip = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.BTN_StartHIL = new MissionPlanner.Controls.MyButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBox_sendport);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBox_revport);
            this.groupBox1.Controls.Add(this.lbl_udpport);
            this.groupBox1.Controls.Add(this.textBox_ip);
            this.groupBox1.Controls.Add(this.lbl_udpip);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.BTN_StartHIL);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(478, 200);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "start HIL";
            // 
            // textBox_sendport
            // 
            this.textBox_sendport.BackColor = System.Drawing.SystemColors.Control;
            this.textBox_sendport.Font = new System.Drawing.Font("宋体", 9F);
            this.textBox_sendport.Location = new System.Drawing.Point(357, 55);
            this.textBox_sendport.Name = "textBox_sendport";
            this.textBox_sendport.Size = new System.Drawing.Size(54, 25);
            this.textBox_sendport.TabIndex = 48;
            this.textBox_sendport.Text = "49000";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(270, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 15);
            this.label2.TabIndex = 47;
            this.label2.Text = "SEND PORT：";
            // 
            // textBox_revport
            // 
            this.textBox_revport.BackColor = System.Drawing.SystemColors.Control;
            this.textBox_revport.Font = new System.Drawing.Font("宋体", 9F);
            this.textBox_revport.Location = new System.Drawing.Point(210, 55);
            this.textBox_revport.Name = "textBox_revport";
            this.textBox_revport.Size = new System.Drawing.Size(54, 25);
            this.textBox_revport.TabIndex = 46;
            this.textBox_revport.Text = "49001";
            // 
            // lbl_udpport
            // 
            this.lbl_udpport.AutoSize = true;
            this.lbl_udpport.Location = new System.Drawing.Point(135, 58);
            this.lbl_udpport.Name = "lbl_udpport";
            this.lbl_udpport.Size = new System.Drawing.Size(86, 15);
            this.lbl_udpport.TabIndex = 45;
            this.lbl_udpport.Text = "REV PORT：";
            // 
            // textBox_ip
            // 
            this.textBox_ip.BackColor = System.Drawing.SystemColors.Control;
            this.textBox_ip.Font = new System.Drawing.Font("宋体", 9F);
            this.textBox_ip.Location = new System.Drawing.Point(40, 55);
            this.textBox_ip.Name = "textBox_ip";
            this.textBox_ip.Size = new System.Drawing.Size(89, 25);
            this.textBox_ip.TabIndex = 44;
            this.textBox_ip.Text = "127.0.0.1";
            // 
            // lbl_udpip
            // 
            this.lbl_udpip.AutoSize = true;
            this.lbl_udpip.Location = new System.Drawing.Point(6, 58);
            this.lbl_udpip.Name = "lbl_udpip";
            this.lbl_udpip.Size = new System.Drawing.Size(38, 15);
            this.lbl_udpip.TabIndex = 43;
            this.lbl_udpip.Text = "IP：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 15);
            this.label1.TabIndex = 42;
            this.label1.Text = "UDP端口设置";
            // 
            // BTN_StartHIL
            // 
            this.BTN_StartHIL.Font = new System.Drawing.Font("宋体", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BTN_StartHIL.Location = new System.Drawing.Point(388, 154);
            this.BTN_StartHIL.Name = "BTN_StartHIL";
            this.BTN_StartHIL.Size = new System.Drawing.Size(75, 22);
            this.BTN_StartHIL.TabIndex = 41;
            this.BTN_StartHIL.Text = "开始";
            this.BTN_StartHIL.UseVisualStyleBackColor = true;
            this.BTN_StartHIL.Click += new System.EventHandler(this.BTN_StartHIL_Click);
            // 
            // StartHil
            // 
            this.ClientSize = new System.Drawing.Size(495, 216);
            this.Controls.Add(this.groupBox1);
            this.Name = "StartHil";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StartHILClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        private GroupBox groupBox1;
        private Controls.MyButton BTN_StartHIL;
        public static StartHil instance = null;
        public StartHil()
        {
            InitializeComponent();
            instance = this;
        }
        bool threadrun = false;
        Hil xPlane = null;
        void mainloop()
        {
            threadrun = true;
            if (xPlane == null)
            {
                xPlane = new XPlane();
                xPlane.xplane10 = true;
                xPlane.SetupSockets(int.Parse(textBox_revport.Text), int.Parse(textBox_sendport.Text), textBox_ip.Text);
            }
            while (!this.IsDisposed && threadrun)
            {
                //xPlane.TestSendToAP();
                xPlane.GetFromSim();
                xPlane.SendToAP(MainV2.comPort, MainV2.comPort.MAV);
                xPlane.SendToSim();
                // 50HZ
                System.Threading.Thread.Sleep(20);
            }
        }

        private void BTN_StartHIL_Click(object sender, EventArgs e)
        {
            if (threadrun == true)
            {
                threadrun = false;
                BTN_StartHIL.Text = Strings.Start;
                return;
            }
            if (xPlane != null)
            {
                xPlane.Shutdown();
                xPlane = null;
            }
            new System.Threading.Thread(mainloop) { IsBackground = true }.Start();
            BTN_StartHIL.Text = Strings.Stop;
        }

        private void StartHILClosing(object sender, FormClosingEventArgs e)
        {
            if (threadrun)
            {
                BTN_StartHIL_Click(this, null);
            }
            if (xPlane!=null)
            {
                xPlane.Shutdown();
                xPlane = null;
            }
        }
    }
}
