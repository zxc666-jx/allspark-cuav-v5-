using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MissionPlanner.Swarm
{
    public partial class Status : UserControl
    {
        public Label Armed
        {
            get { return this.lbl_armed; }
        }

        public Label GPS
        {
            get { return this.lbl_gps; }
        }

        public Label Mode
        {
            get { return this.lbl_mode; }
        }

        public Label MAV
        {
            get { return this.lbl_mav; }
        }

        //public Label Guided
        //{
        //    get { return this.lbl_guided; }
        //}

        //public Label Location1
        //{
        //    get { return this.lbl_loc; }
        //}
        public Label Alt
        {
            get { return this.lbl_alt; }
        }

        public Label Gndspd
        {
            get { return this.lbl_gndspd; }
        }

        public Label Airspd
        {
            get { return this.lbl_airspd; }
        }

        public Label Link
        {
            get { return this.lbl_link; }
        }
        public Status()
        {
            InitializeComponent();
        }
    }
}