using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MissionPlanner.Swarm
{
    public partial class SimUDPLink: UserControl
    {
        public SimUDPLink()
        {
            InitializeComponent();
        }
        public void setConnectMav(string name)
        {
            txt_connectmav.Text = name;
        }
        public bool getShowSimDataChecked()
        {
            return checkBox_showdata.Checked;
        }
        public string getConnectMav()
        {
            return txt_connectmav.Text;
        }
        public string getIP()
        {
            return textBox_ip.Text;
        }
        public int getRevPort()
        {
            return int.Parse(textBox_revport.Text);
        }
        public int getSendPort()
        {
            return int.Parse(textBox_sendport.Text);
        }
    }
}
