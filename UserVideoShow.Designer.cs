namespace MissionPlanner
{
    partial class UserVideoShow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserVideoShow));
            this.userVideoHudShow1 = new MissionPlanner.UserVideoHudShow();
            this.SuspendLayout();
            // 
            // userVideoHudShow1
            // 
            this.userVideoHudShow1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.userVideoHudShow1.BackColor = System.Drawing.Color.Black;
            this.userVideoHudShow1.bgimage = null;
            this.userVideoHudShow1.hudcolor = System.Drawing.Color.White;
            this.userVideoHudShow1.Location = new System.Drawing.Point(1, 3);
            this.userVideoHudShow1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.userVideoHudShow1.Name = "userVideoHudShow1";
            this.userVideoHudShow1.Size = new System.Drawing.Size(813, 521);
            this.userVideoHudShow1.streamjpg = ((System.IO.MemoryStream)(resources.GetObject("userVideoHudShow1.streamjpg")));
            this.userVideoHudShow1.TabIndex = 0;
            this.userVideoHudShow1.VSync = false;
            // 
            // UserVideoShow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(814, 513);
            this.Controls.Add(this.userVideoHudShow1);
            this.Name = "UserVideoShow";
            this.Text = "UserVideoShow";
            this.ResumeLayout(false);

        }

        #endregion

        private UserVideoHudShow userVideoHudShow1;
    }
}