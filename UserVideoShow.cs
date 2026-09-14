using MissionPlanner.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MissionPlanner
{
    public partial class UserVideoShow: Form
    {
        //this.panel1.Controls.Add(this.hud1);
        public static UserVideoShow instance;
        //public static HUD userhud;
        //public UserVideoHudShow UserVideoHud = new UserVideoHudShow();
        public static Image camimage = null;
        public UserVideoShow()
        {
            InitializeComponent();
            instance = this;
            //userhud = hud1;
            //this.panel1.Controls.Add(UserVideoHud);
            new System.Threading.Thread(mainloop) { IsBackground = true }.Start();
        }
        void update_camimage(Image _image)
        {
            userVideoHudShow1.bgimage = _image;
        }
        void mainloop()
        {
            while (!this.IsDisposed)
            {
                if (camimage != null)
                {
                    update_camimage(camimage);
                }
                // 5HZ
                System.Threading.Thread.Sleep(200);
            }
        }
    }
}
