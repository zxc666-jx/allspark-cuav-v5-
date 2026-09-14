using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Collections;
using System.Threading;
using System.Drawing.Drawing2D;
using log4net;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using SvgNet.SvgGdi;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace MissionPlanner
{
    public partial class UserVideoHudShow: GLControl
    {
        public string shownamestring = "光电载荷";
        public string showtimestring = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        private object paintlock = new object();
        private object streamlock = new object();

        private MemoryStream _streamjpg = new MemoryStream();

        //[System.ComponentModel.Browsable(false)]
        public MemoryStream streamjpg
        {
            get
            {
                lock (streamlock)
                {
                    return _streamjpg;
                }
            }
            set
            {
                lock (streamlock)
                {
                    _streamjpg = value;
                }
            }
        }
        /// <summary>
        /// this is to reduce cpu usage
        /// </summary>
        public bool streamjpgenable = false;

        public bool HoldInvalidation = false;

        private class character
        {
            public GraphicsPath pth;
            public Bitmap bitmap;
            public int gltextureid;
            public int width;
            public int size;
        }
        private Dictionary<int, character> charDict = new Dictionary<int, character>();

        public int huddrawtime = 0;

        [DefaultValue(true)] public bool opengl { get; set; }

        [Browsable(false)] public bool npotSupported { get; private set; }
        public Hashtable CustomItems = new Hashtable();

        [System.ComponentModel.Browsable(true), System.ComponentModel.Category("Values")]
        public Color hudcolor
        {
            get { return this._whitePen.Color; }
            set
            {
                _hudcolor = value;
                this._whitePen = new Pen(value, 2);
            }
        }
        private bool started = false;
        private Color _hudcolor = Color.White;
        private Pen _whitePen = new Pen(Color.White, 2);
        private readonly SolidBrush _whiteBrush = new SolidBrush(Color.White);

        private static readonly SolidBrush SolidBrush = new SolidBrush(Color.FromArgb(0x55, 0xff, 0xff, 0xff));

        private static readonly SolidBrush SlightlyTransparentWhiteBrush =
            new SolidBrush(Color.FromArgb(220, 255, 255, 255));

        private static readonly SolidBrush AltGroundBrush = new SolidBrush(Color.FromArgb(100, Color.BurlyWood));

        private readonly object _bgimagelock = new object();

        public Image bgimage
        {
            set
            {
                lock (this._bgimagelock)
                {
                    try
                    {
                        _bgimage = (Image)value;
                    }
                    catch
                    {
                        _bgimage = null;
                    }

                    this.Invalidate();
                }
            }
            get { return _bgimage; }
        }

        private Image _bgimage;

        public Bitmap objBitmap = new Bitmap(1024, 1024, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        private UserVideoHudShow graphicsObject;
        private IGraphics graphicsObjectGDIP;
        private DateTime textureResetDateTime = DateTime.Now;
        public UserVideoHudShow()
        {
            InitializeComponent();
            opengl = true;

            eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 50L);

            objBitmap.MakeTransparent();

            graphicsObject = this;
            graphicsObjectGDIP = new GdiGraphics(Graphics.FromImage(objBitmap));
        }
        public override void Refresh()
        {
            if (!ThisReallyVisible())
            {
                //  return;
            }

            //base.Refresh();
            using (Graphics gg = this.CreateGraphics())
            {
                OnPaint(new PaintEventArgs(gg, this.ClientRectangle));
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            if (opengl && !DesignMode)
            {
                try
                {

                    OpenTK.Graphics.GraphicsMode test = this.GraphicsMode;

                    int[] viewPort = new int[4];
                    GL.GetInteger(GetPName.Viewport, viewPort);
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadIdentity();
                    GL.Ortho(0, Width, Height, 0, -1, 1);
                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.LoadIdentity();

                    GL.PushAttrib(AttribMask.DepthBufferBit);
                    GL.Disable(EnableCap.DepthTest);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                    GL.Enable(EnableCap.Blend);

                    string versionString = GL.GetString(StringName.Version);
                    string majorString = versionString.Split(' ')[0];
                    var v = new Version(majorString);
                    npotSupported = v.Major >= 2;
                }
                catch (Exception ex)
                {
                }

                try
                {
                    GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

                    GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
                    GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
                    GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

                    GL.Hint(HintTarget.TextureCompressionHint, HintMode.Nicest);
                }
                catch (Exception ex)
                {
                }

                try
                {
                    GL.Enable(EnableCap.LineSmooth);
                    GL.Enable(EnableCap.PointSmooth);
                    GL.Disable(EnableCap.PolygonSmooth);

                }
                catch (Exception ex)
                {
                }
            }

            started = true;
        }
        /// <summary>
        /// this is to fix a mono off screen drawing issue
        /// </summary>
        /// <returns></returns>
        public bool ThisReallyVisible()
        {
            //Control ctl = Control.FromHandle(this.Handle);
            return this.Visible;
        }
        DateTime lastinvalidate = DateTime.MinValue;
        public bool SixteenXNine = false;
        /// <summary>
        /// Override to prevent offscreen drawing the control - mono mac
        /// </summary>
        public new void Invalidate()
        {
            if (HoldInvalidation)
                return;

            if (!ThisReallyVisible())
            {
                //  return;
            }

            lastinvalidate = DateTime.Now;

            base.Invalidate();
        }
        bool inOnPaint = false;
        protected override void OnPaint(PaintEventArgs e)
        {
            if (!started)
                return;

            if (this.DesignMode)
            {
                e.Graphics.Clear(this.BackColor);
                e.Graphics.Flush();
                opengl = false;
                doPaint();
                e.Graphics.DrawImageUnscaled(objBitmap, 0, 0);
                opengl = true;
                return;
            }

            // force texture reset
            if (textureResetDateTime.Hour != DateTime.Now.Hour)
            {
                textureResetDateTime = DateTime.Now;
                doResize();
            }

            lock (this)
            {

                if (inOnPaint)
                {
                    return;
                }

                inOnPaint = true;

            }
            try
            {

                if (opengl)
                {
                    // make this gl window and thread current
                    //if (!Context.IsCurrent || DateTime.Now.Second % 5 == 0)
                    MakeCurrent();

                    GL.Clear(ClearBufferMask.ColorBufferBit);

                }

                doPaint();

                if (!opengl)
                {
                    e.Graphics.DrawImageUnscaled(objBitmap, 0, 0);

                    //File.WriteAllText("hud.svg", graphicsObjectGDIP.WriteSVGString());
                }
                else if (opengl)
                {
                    this.SwapBuffers();

                    // free from this thread
                    Context.MakeCurrent(null);
                }

            }
            catch (Exception ex)
            {
            }

            lock (this)
            {
                inOnPaint = false;
            }
        }
        public ImageCodecInfo GetImageCodecInfo()
        {
            return ici;
        }
        public EncoderParameters GetEncoderParameters()
        {
            return eps;
        }
        private readonly Pen _blackPen = new Pen(Color.Black, 2);
        private readonly Pen _greenPen = new Pen(Color.Green, 2);
        private readonly Pen _redPen = new Pen(Color.Red, 2);
        private static ImageCodecInfo ici = GetImageCodec("image/jpeg");
        private static EncoderParameters eps = new EncoderParameters(1);
        Rectangle nameshow = new Rectangle();
        Rectangle nosignal = new Rectangle();
        Rectangle timeshow = new Rectangle();
        void doPaint()
        {
            try
            {
                if (graphicsObjectGDIP == null || !opengl &&
                    (objBitmap.Width != this.Width || objBitmap.Height != this.Height))
                {
                    objBitmap = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    objBitmap.MakeTransparent();
                    graphicsObjectGDIP = new GdiGraphics(Graphics.FromImage(objBitmap));

                    graphicsObjectGDIP.SmoothingMode = SmoothingMode.HighSpeed;
                    graphicsObjectGDIP.InterpolationMode = InterpolationMode.NearestNeighbor;
                    graphicsObjectGDIP.CompositingMode = CompositingMode.SourceOver;
                    graphicsObjectGDIP.CompositingQuality = CompositingQuality.HighSpeed;
                    graphicsObjectGDIP.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                    graphicsObjectGDIP.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
                }

                graphicsObjectGDIP.InterpolationMode = InterpolationMode.Bilinear;

                try
                {
                    graphicsObject.Clear(Color.Transparent);
                }
                catch
                {
                    // this is the first posible opengl call
                    // in vmware fusion on mac, this fails, so switch back to legacy
                    opengl = false;
                }

                if (_bgimage != null)
                {
                    lock (this._bgimagelock)
                        lock (_bgimage)
                        {
                            try
                            {
                                graphicsObject.DrawImage(_bgimage, 0, 0, this.Width, this.Height);
                            }
                            catch (Exception ex)
                            {
                                _bgimage = null;
                            }
                        }
                }

                this._whiteBrush.Color = this._whitePen.Color;

                if (DesignMode)
                {
                    return;
                }
                int centerPointx = this.Width / 2;
                int centerPointy = this.Height / 2;
                int scalex = this.Width / 8;
                int scaley = this.Height / 8;
                int lenthscalex = this.Width / 20;
                int lenthscaley = this.Height/20;
                if (_bgimage != null)
                {
                    // 绘制聚焦框
                    // 左上角
                    graphicsObject.DrawLine(this._whitePen, centerPointx - scalex, centerPointy - scaley, centerPointx - scalex + lenthscalex, centerPointy - scaley);
                    graphicsObject.DrawLine(this._whitePen, centerPointx - scalex, centerPointy - scaley, centerPointx - scalex, centerPointy - scaley + lenthscaley);
                    // 右上角
                    graphicsObject.DrawLine(this._whitePen, centerPointx + scalex, centerPointy - scaley, centerPointx + scalex - lenthscalex, centerPointy - scaley);
                    graphicsObject.DrawLine(this._whitePen, centerPointx + scalex, centerPointy - scaley, centerPointx + scalex, centerPointy - scaley + lenthscaley);
                    // 左下角
                    graphicsObject.DrawLine(this._whitePen, centerPointx - scalex, centerPointy + scaley, centerPointx - scalex + lenthscalex, centerPointy + scaley);
                    graphicsObject.DrawLine(this._whitePen, centerPointx - scalex, centerPointy + scaley, centerPointx - scalex, centerPointy + scaley - lenthscaley);
                    // 右下角
                    graphicsObject.DrawLine(this._whitePen, centerPointx + scalex, centerPointy + scaley, centerPointx + scalex - lenthscalex, centerPointy + scaley);
                    graphicsObject.DrawLine(this._whitePen, centerPointx + scalex, centerPointy + scaley, centerPointx + scalex, centerPointy + scaley - lenthscaley);
                }

                int fontsize = this.Height / 30; // = 10
                int fontoffset = fontsize - 10;
                nameshow = new Rectangle(0 + this.Width / 40, 0 + this.Height / 40, 40, fontsize * 2);
                drawstring(shownamestring, new Font("Arial", fontsize + 2), fontsize + 2, (SolidBrush)Brushes.Red, nameshow.X, nameshow.Y);
                timeshow = new Rectangle(this.Width - this.Width / 2, 0 + this.Height / 40, 40, fontsize * 2);
                drawstring(showtimestring, new Font("Arial", fontsize + 2), fontsize + 2, (SolidBrush)Brushes.Red, timeshow.X, timeshow.Y);
                if (_bgimage == null)
                {
                    nosignal = new Rectangle(this.Width / 2 - fontsize * 6, this.Height / 2 - fontsize, 40, fontsize * 2);
                    drawstring("No Signal", new Font("Arial", fontsize + 15), fontsize + 15, (SolidBrush)Brushes.Red, nosignal.X, nosignal.Y);
                }
                lock (streamlock)
                {
                    if (streamjpgenable || streamjpg == null) // init image and only update when needed
                    {
                        if (opengl)
                        {
                            objBitmap = GrabScreenshot();
                        }

                        streamjpg = new MemoryStream();
                        objBitmap.Save(streamjpg, ici, eps);
                        //objBitmap.Save(streamjpg,ImageFormat.Bmp);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// pen for drawstring
        /// </summary>
        private readonly Pen _p = new Pen(Color.FromArgb(0x26, 0x27, 0x28), 2f);

        void drawstring(string text, Font font, float fontsize, SolidBrush brush, float x, float y)
        {
            if (!opengl)
            {
                drawstringGDI(text, font, fontsize, brush, x, y);
                return;
            }

            if (text == null || text == "")
                return;
            /*
            OpenTK.Graphics.Begin(); 
            GL.PushMatrix(); 
            GL.Translate(x, y, 0);
            printer.Print(text, font, c); 
            GL.PopMatrix(); printer.End();
            */

            float maxy = 1;

            foreach (char cha in text)
            {
                int charno = (int)cha;

                int charid = charno ^ (int)(fontsize * 1000) ^ brush.Color.ToArgb();

                if (!charDict.ContainsKey(charid))
                {
                    charDict[charid] = new character()
                    {
                        bitmap = new Bitmap(128, 128, System.Drawing.Imaging.PixelFormat.Format32bppArgb),
                        size = (int)fontsize
                    };

                    charDict[charid].bitmap.MakeTransparent(Color.Transparent);

                    //charbitmaptexid

                    float maxx = this.Width / 150; // for space


                    // create bitmap
                    using (var gfx = Graphics.FromImage(charDict[charid].bitmap))
                    {
                        var pth = new GraphicsPath();

                        if (text != null)
                            pth.AddString(cha + "", font.FontFamily, 0, fontsize + 5, new Point((int)0, (int)0),
                                StringFormat.GenericTypographic);

                        charDict[charid].pth = pth;

                        gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                        gfx.DrawPath(this._p, pth);

                        //Draw the face

                        gfx.FillPath(brush, pth);


                        if (pth.PointCount > 0)
                        {
                            foreach (PointF pnt in pth.PathPoints)
                            {
                                if (pnt.X > maxx)
                                    maxx = pnt.X;

                                if (pnt.Y > maxy)
                                    maxy = pnt.Y;
                            }
                        }
                    }

                    charDict[charid].width = (int)(maxx + 2);

                    //charbitmaps[charid] = charbitmaps[charid].Clone(new RectangleF(0, 0, maxx + 2, maxy + 2), charbitmaps[charid].PixelFormat);

                    //charbitmaps[charno * (int)fontsize].Save(charno + " " + (int)fontsize + ".png");

                    // create texture
                    int textureId;
                    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode,
                        (float)TextureEnvModeCombine.Replace); //Important, or wrong color on some computers

                    Bitmap bitmap = charDict[charid].bitmap;
                    GL.GenTextures(1, out textureId);
                    GL.BindTexture(TextureTarget.Texture2D, textureId);

                    BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                        (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                        (int)TextureMagFilter.Linear);

                    //    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
                    //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
                    GL.Flush();
                    bitmap.UnlockBits(data);

                    charDict[charid].gltextureid = textureId;
                }

                float scale = 1.0f;

                // dont draw spaces
                if (cha != ' ')
                {
                    /*
                    TranslateTransform(x, y);
                    DrawPath(this._p, charDict[charid].pth);

                    //Draw the face

                    FillPath(brush, charDict[charid].pth);

                    TranslateTransform(-x, -y);
                    */
                    //GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                    GL.Enable(EnableCap.Texture2D);
                    GL.BindTexture(TextureTarget.Texture2D, charDict[charid].gltextureid);

                    GL.Begin(PrimitiveType.TriangleFan);
                    GL.TexCoord2(0, 0);
                    GL.Vertex2(x, y);
                    GL.TexCoord2(1, 0);
                    GL.Vertex2(x + charDict[charid].bitmap.Width * scale, y);
                    GL.TexCoord2(1, 1);
                    GL.Vertex2(x + charDict[charid].bitmap.Width * scale, y + charDict[charid].bitmap.Height * scale);
                    GL.TexCoord2(0, 1);
                    GL.Vertex2(x + 0, y + charDict[charid].bitmap.Height * scale);
                    GL.End();

                    //GL.Disable(EnableCap.Blend);
                    GL.Disable(EnableCap.Texture2D);
                }

                x += charDict[charid].width * scale;
            }
        }
        void drawstringGDI(string text, Font font, float fontsize, SolidBrush brush, float x, float y)
        {
            if (text == null || text == "")
                return;

            float maxy = 0;

            foreach (char cha in text)
            {
                int charno = (int)cha;

                int charid = charno ^ (int)(fontsize * 1000) ^ brush.Color.ToArgb();

                if (!charDict.ContainsKey(charid))
                {
                    charDict[charid] = new character()
                    {
                        bitmap = new Bitmap(128, 128, System.Drawing.Imaging.PixelFormat.Format32bppArgb),
                        size = (int)fontsize
                    };

                    charDict[charid].bitmap.MakeTransparent(Color.Transparent);

                    //charbitmaptexid

                    float maxx = this.Width / 150; // for space


                    // create bitmap
                    using (var gfx = Graphics.FromImage(charDict[charid].bitmap))
                    {
                        var pth = new GraphicsPath();

                        if (text != null)
                            pth.AddString(cha + "", font.FontFamily, 0, fontsize + 5, new Point((int)0, (int)0),
                                StringFormat.GenericTypographic);

                        charDict[charid].pth = pth;

                        gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                        gfx.DrawPath(this._p, pth);

                        //Draw the face

                        gfx.FillPath(brush, pth);


                        if (pth.PointCount > 0)
                        {
                            foreach (PointF pnt in pth.PathPoints)
                            {
                                if (pnt.X > maxx)
                                    maxx = pnt.X;

                                if (pnt.Y > maxy)
                                    maxy = pnt.Y;
                            }
                        }
                    }

                    charDict[charid].width = (int)(maxx + 2);
                }

                // draw it

                float scale = 1.0f;
                // dont draw spaces
                if (cha != ' ')
                {
                    DrawImage(charDict[charid].bitmap, (int)x, (int)y, charDict[charid].bitmap.Width, charDict[charid].bitmap.Height, charDict[charid].gltextureid);
                    /*
                    graphicsObjectGDIP.TranslateTransform(x,y);
                    graphicsObjectGDIP.DrawPath(this._p, charDict[charid].pth);

                    //Draw the face

                    graphicsObjectGDIP.FillPath(brush, charDict[charid].pth);

                    graphicsObjectGDIP.TranslateTransform(-x, -y);
                    */
                }
                else
                {

                }

                x += charDict[charid].width * scale;
            }

        }
        void Clear(Color color)
        {
            if (opengl)
            {
                GL.ClearColor(color);

            }
            else
            {
                graphicsObjectGDIP.Clear(color);
            }
        }
        private character[] _texture = new character[2];
        public void DrawImage(Image img, int x, int y, int width, int height, int textureno = 0)
        {
            if (opengl)
            {
                if (img == null)
                    return;

                if (_texture[textureno] == null)
                    _texture[textureno] = new character();

                // If the image is already a bitmap and we support NPOT textures then simply use it.
                if (npotSupported && img is Bitmap)
                {
                    _texture[textureno].bitmap = (Bitmap)img;
                }
                else
                {
                    // Otherwise we have to resize img to be POT.
                    _texture[textureno].bitmap = ResizeImage(img, 512, 512);
                }

                // generate the texture
                if (_texture[textureno].gltextureid == 0)
                {
                    GL.GenTextures(1, out _texture[textureno].gltextureid);
                }

                GL.BindTexture(TextureTarget.Texture2D, _texture[textureno].gltextureid);

                BitmapData data = _texture[textureno].bitmap.LockBits(
                    new Rectangle(0, 0, _texture[textureno].bitmap.Width, _texture[textureno].bitmap.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                // create the texture type/dimensions
                if (_texture[textureno].width != _texture[textureno].bitmap.Width)
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                    _texture[textureno].width = data.Width;
                }
                else
                {
                    GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, data.Width, data.Height,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                }

                _texture[textureno].bitmap.UnlockBits(data);

                bool polySmoothEnabled = GL.IsEnabled(EnableCap.PolygonSmooth);
                if (polySmoothEnabled)
                    GL.Disable(EnableCap.PolygonSmooth);

                GL.Enable(EnableCap.Texture2D);

                GL.BindTexture(TextureTarget.Texture2D, _texture[textureno].gltextureid);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                    (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                    (int)TextureWrapMode.ClampToEdge);

                GL.Begin(PrimitiveType.TriangleStrip);

                GL.TexCoord2(0.0f, 0.0f);
                GL.Vertex2(x, y);
                GL.TexCoord2(0.0f, 1.0f);
                GL.Vertex2(x, y + height);
                GL.TexCoord2(1.0f, 0.0f);
                GL.Vertex2(x + width, y);
                GL.TexCoord2(1.0f, 1.0f);
                GL.Vertex2(x + width, y + height);

                GL.End();

                GL.Disable(EnableCap.Texture2D);

                if (polySmoothEnabled)
                    GL.Enable(EnableCap.PolygonSmooth);
            }
            else
            {
                graphicsObjectGDIP.DrawImage(img, x, y, width, height);
            }
        }
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.ClearOutputChannelColorProfile();
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
        public void DrawLine(Pen penn, double x1, double y1, double x2, double y2)
        {

            if (opengl)
            {
                GL.Color4(penn.Color);
                GL.LineWidth(penn.Width);

                GL.Begin(PrimitiveType.Lines);
                GL.Vertex2(x1, y1);
                GL.Vertex2(x2, y2);
                GL.End();
            }
            else
            {
                graphicsObjectGDIP.DrawLine(penn, (float)x1, (float)y1, (float)x2, (float)y2);
            }
        }
        public void ResetTransform()
        {
            if (opengl)
            {
                GL.LoadIdentity();
            }
            else
            {
                graphicsObjectGDIP.ResetTransform();
            }
        }
        // Returns a System.Drawing.Bitmap with the contents of the current framebuffer
        public new Bitmap GrabScreenshot()
        {
            if (OpenTK.Graphics.GraphicsContext.CurrentContext == null)
                throw new OpenTK.Graphics.GraphicsContextMissingException();

            Bitmap bmp = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            System.Drawing.Imaging.BitmapData data =
                bmp.LockBits(this.ClientRectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.ReadPixels(0, 0, this.ClientSize.Width, this.ClientSize.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgr,
                PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bmp;
        }
        static ImageCodecInfo GetImageCodec(string mimetype)
        {
            foreach (ImageCodecInfo ici in ImageCodecInfo.GetImageEncoders())
            {
                if (ici.MimeType == mimetype) return ici;
            }

            return null;
        }
        public void doResize()
        {
            OnResize(EventArgs.Empty);
        }

        protected override void OnResize(EventArgs e)
        {
            if (DesignMode || !IsHandleCreated || !started)
                return;

            base.OnResize(e);

            if (SixteenXNine)
            {
                int ht = (int)(this.Width / 1.777f);
                if (ht >= this.Height + 5 || ht <= this.Height - 5)
                {
                    this.Height = ht;
                    return;
                }
            }
            else
            {
                // 4x3
                int ht = (int)(this.Width / 1.333f);
                if (ht >= this.Height + 5 || ht <= this.Height - 5)
                {
                    this.Height = ht;
                    return;
                }
            }

            graphicsObjectGDIP = new GdiGraphics(Graphics.FromImage(objBitmap));

            try
            {
                foreach (character texid in charDict.Values)
                {
                    try
                    {
                        texid.bitmap.Dispose();
                    }
                    catch
                    {
                    }
                }

                if (opengl)
                {
                    foreach (character texid in _texture)
                    {
                        if (texid != null && texid.gltextureid != 0)
                            GL.DeleteTexture(texid.gltextureid);
                    }

                    this._texture = new character[_texture.Length];

                    foreach (character texid in charDict.Values)
                    {
                        if (texid.gltextureid != 0)
                            GL.DeleteTexture(texid.gltextureid);
                    }
                }

                charDict.Clear();
            }
            catch
            {
            }

            try
            {
                if (opengl)
                {
                    MakeCurrent();

                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadIdentity();
                    GL.Ortho(0, Width, Height, 0, -1, 1);
                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.LoadIdentity();

                    GL.Viewport(0, 0, Width, Height);
                }
            }
            catch
            {
            }

            Refresh();
        }
    }
}
