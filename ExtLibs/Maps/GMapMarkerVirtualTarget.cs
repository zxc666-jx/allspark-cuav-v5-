using System;
using System.Drawing;
using System.IO;
using GMap.NET;
using GMap.NET.WindowsForms;
using MissionPlanner.Utilities;

namespace MissionPlanner.Maps
{
    [Serializable]
    public class GMapMarkerVirtualTarget : GMapMarker
    {
        static System.Drawing.Size SizeSt = new System.Drawing.Size(52,134);

        public float heading = 0;
        Bitmap tankbitmap;
        public GMapMarkerVirtualTarget(PointLatLng p, float heading)
            : base(p)
        {
            tankbitmap = BytesToBitmap(global::MissionPlanner.Maps.Resources.mytank);
            SizeSt= new System.Drawing.Size(tankbitmap.Width, tankbitmap.Height);
            this.heading = heading;
            Size = SizeSt;
        }

        //byte[] 转换 Bitmap
        public static Bitmap BytesToBitmap(byte[] Bytes)
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(Bytes);
                return new Bitmap((Image)new Bitmap(stream));
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            finally
            {
                stream.Close();
            }
        }
        public override void OnRender(Graphics g)
        {
            var temp = g.Transform;
            g.TranslateTransform(LocalPosition.X, LocalPosition.Y);

            g.RotateTransform(-Overlay.Control.Bearing);

            //int length = 500;
            //// anti NaN
            //try
            //{
            //    g.DrawLine(new Pen(Color.Red, 2), 0.0f, 0.0f, (float)Math.Cos((heading - 90) * MathHelper.deg2rad) * length,
            //        (float)Math.Sin((heading - 90) * MathHelper.deg2rad) * length);
            //}
            //catch
            //{
            //}
            try
            {
                g.RotateTransform(heading);
            }
            catch
            {
            }
            g.DrawImageUnscaled(tankbitmap,
                tankbitmap.Width / -2,
                tankbitmap.Height / -2);

            g.Transform = temp;
        }
    }
}
