namespace MissionPlanner.Swarm
{
    partial class SimUDPLink
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.lbl_udpip = new System.Windows.Forms.Label();
            this.textBox_sendport = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_revport = new System.Windows.Forms.TextBox();
            this.lbl_udpport = new System.Windows.Forms.Label();
            this.textBox_ip = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_connectmav = new System.Windows.Forms.TextBox();
            this.checkBox_showdata = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lbl_udpip
            // 
            this.lbl_udpip.AutoSize = true;
            this.lbl_udpip.Location = new System.Drawing.Point(3, 4);
            this.lbl_udpip.Name = "lbl_udpip";
            this.lbl_udpip.Size = new System.Drawing.Size(38, 15);
            this.lbl_udpip.TabIndex = 44;
            this.lbl_udpip.Text = "IP：";
            // 
            // textBox_sendport
            // 
            this.textBox_sendport.BackColor = System.Drawing.SystemColors.Control;
            this.textBox_sendport.Font = new System.Drawing.Font("宋体", 9F);
            this.textBox_sendport.Location = new System.Drawing.Point(401, 4);
            this.textBox_sendport.Name = "textBox_sendport";
            this.textBox_sendport.Size = new System.Drawing.Size(54, 25);
            this.textBox_sendport.TabIndex = 53;
            this.textBox_sendport.Text = "49000";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(304, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 15);
            this.label2.TabIndex = 52;
            this.label2.Text = "SEND PORT：";
            // 
            // textBox_revport
            // 
            this.textBox_revport.BackColor = System.Drawing.SystemColors.Control;
            this.textBox_revport.Font = new System.Drawing.Font("宋体", 9F);
            this.textBox_revport.Location = new System.Drawing.Point(248, 4);
            this.textBox_revport.Name = "textBox_revport";
            this.textBox_revport.Size = new System.Drawing.Size(54, 25);
            this.textBox_revport.TabIndex = 51;
            this.textBox_revport.Text = "49001";
            // 
            // lbl_udpport
            // 
            this.lbl_udpport.AutoSize = true;
            this.lbl_udpport.Location = new System.Drawing.Point(163, 4);
            this.lbl_udpport.Name = "lbl_udpport";
            this.lbl_udpport.Size = new System.Drawing.Size(86, 15);
            this.lbl_udpport.TabIndex = 50;
            this.lbl_udpport.Text = "REV PORT：";
            // 
            // textBox_ip
            // 
            this.textBox_ip.BackColor = System.Drawing.SystemColors.Control;
            this.textBox_ip.Font = new System.Drawing.Font("宋体", 9F);
            this.textBox_ip.Location = new System.Drawing.Point(29, 3);
            this.textBox_ip.Name = "textBox_ip";
            this.textBox_ip.Size = new System.Drawing.Size(126, 25);
            this.textBox_ip.TabIndex = 49;
            this.textBox_ip.Text = "127.0.0.1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(463, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 15);
            this.label1.TabIndex = 54;
            this.label1.Text = "MAV：";
            // 
            // txt_connectmav
            // 
            this.txt_connectmav.BackColor = System.Drawing.SystemColors.Control;
            this.txt_connectmav.Font = new System.Drawing.Font("宋体", 9F);
            this.txt_connectmav.Location = new System.Drawing.Point(513, 4);
            this.txt_connectmav.Name = "txt_connectmav";
            this.txt_connectmav.Size = new System.Drawing.Size(99, 25);
            this.txt_connectmav.TabIndex = 55;
            this.txt_connectmav.Text = "TCP5760 1 1";
            // 
            // checkBox_showdata
            // 
            this.checkBox_showdata.AutoSize = true;
            this.checkBox_showdata.Location = new System.Drawing.Point(627, 4);
            this.checkBox_showdata.Name = "checkBox_showdata";
            this.checkBox_showdata.Size = new System.Drawing.Size(93, 19);
            this.checkBox_showdata.TabIndex = 56;
            this.checkBox_showdata.Text = "ShowData";
            this.checkBox_showdata.UseVisualStyleBackColor = true;
            // 
            // SimUDPLink
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checkBox_showdata);
            this.Controls.Add(this.txt_connectmav);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_sendport);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_revport);
            this.Controls.Add(this.lbl_udpport);
            this.Controls.Add(this.textBox_ip);
            this.Controls.Add(this.lbl_udpip);
            this.Name = "SimUDPLink";
            this.Size = new System.Drawing.Size(730, 50);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_udpip;
        private System.Windows.Forms.TextBox textBox_sendport;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_revport;
        private System.Windows.Forms.Label lbl_udpport;
        private System.Windows.Forms.TextBox textBox_ip;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_connectmav;
        private System.Windows.Forms.CheckBox checkBox_showdata;
    }
}
