using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MissionPlanner.Swarm
{
    public partial class VideoDisplayOut: Form
    {
        private TableLayoutPanel tableLayoutPanel1;
        //public static VideoDisplayOut instance = null;

        public VideoDisplayOut()
        {
            InitializeComponent();
            //instance = this;
        }
        public void AddHud(UserVideoHudShow hub1, UserVideoHudShow hub2)
        {
            this.tableLayoutPanel1.Controls.Add(hub1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(hub2, 1, 0);
        }
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1216, 494);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // VideoDisplayOut
            // 
            this.ClientSize = new System.Drawing.Size(1216, 494);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "VideoDisplayOut";
            this.ResumeLayout(false);

        }
    }
}
