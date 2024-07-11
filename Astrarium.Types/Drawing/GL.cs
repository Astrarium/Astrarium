using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;

namespace Astrarium.Types
{
    public static class GL
    {
        private static TextureManager textureManager = new TextureManager();

        private const string OPENGL32 = "opengl32";

        public const int BLEND = 0x0BE2;
        public const int POINT_SMOOTH = 0x0B10;
        public const int LINE_SMOOTH = 0x0B20;
        public const int CULL_FACE = 0x0B44;
        public const int LINE_STIPPLE = 0x0B24;
        public const int TEXTURE_2D = 0x0DE1;
        public const int STENCIL_TEST = 0x0B90;
        public const int LIGHT0 = 0x4000;
        public const int LIGHTING = 0x0B50;
        public const int NICEST = 0x1102;
        public const int POINT_SMOOTH_HINT = 0x0C51;
        public const int LINE_SMOOTH_HINT = 0x0C52;
        public const int AMBIENT = 0x1200;
        public const int DIFFUSE = 0x1201;
        public const int SPECULAR = 0x1202;
        public const int EMISSION = 0x1600;
        public const int SHININESS = 0x1601;
        public const int ZERO = 0;
        public const int ONE = 1;
        public const int ONE_MINUS_SRC_COLOR = 0x0301;
        public const int SRC_ALPHA = 0x0302;
        public const int ONE_MINUS_SRC_ALPHA = 0x0303;
        public const int FRONT = 0x0404;
        public const int BACK = 0x0405;
        public const int STENCIL_BUFFER_BIT = 0x00000400;
        public const int COLOR_BUFFER_BIT = 0x00004000;
        public const int PROJECTION = 0x1701;
        public const int RGB = 0x1907;
        public const int RGBA = 0x1908;
        public const int BGRA = 0x80E1;
        public const int BGR = 0x80E0;
        public const int UNSIGNED_BYTE = 0x1401;
        public const int POINTS = 0x0000;
        public const int LINES = 0x0001;
        public const int LINE_LOOP = 0x0002;
        public const int LINE_STRIP = 0x0003;
        public const int TRIANGLE_STRIP = 0x0005;
        public const int TRIANGLE_FAN = 0x0006;
        public const int QUADS = 0x0007;
        public const int QUAD_STRIP = 0x0008;
        public const int SMOOTH = 0x1D01;
        public const int ALWAYS = 0x0207;
        public const int EQUAL = 0x0202;
        public const int NOTEQUAL = 0x0205;
        public const int KEEP = 0x1E00;
        public const int REPLACE = 0x1E01;
        public const int TEXTURE_MAG_FILTER = 0x2800;
        public const int TEXTURE_MIN_FILTER = 0x2801;
        public const int TEXTURE_WRAP_S = 0x2802;
        public const int TEXTURE_WRAP_T = 0x2803;
        public const int LINEAR = 0x2601;
        public const int MIRRORED_REPEAT = 0x8370;

        [DllImport(OPENGL32)]
        private static extern void glClearColor(float r, float g, float b, float alpha);

        [DllImport(OPENGL32)]
        private static extern void glGenTextures(int n, uint[] textures);

        [DllImport(OPENGL32)]
        private static extern void glFinish();

        [DllImport(OPENGL32)]
        private static extern void glReadPixels(int x, int y, int width, int height, uint format, uint type, IntPtr pixels);

        [DllImport(OPENGL32)]
        private static extern void glColor3f(float r, float g, float b);

        [DllImport(OPENGL32)]
        private static extern void glColor4f(float r, float g, float b, float a);

        [DllImport(OPENGL32)]
        private static extern void glColorMask(byte red, byte green, byte blue, byte alpha);

        [DllImport(OPENGL32)]
        private static extern void glFlush();

        [DllImport(OPENGL32)]
        private static extern void glDepthMask(byte flag);

        [DllImport(OPENGL32, EntryPoint = "glEnable")]
        public static extern void Enable(int cap);

        [DllImport(OPENGL32, EntryPoint = "glDisable")]
        public static extern void Disable(int cap);

        [DllImport(OPENGL32, EntryPoint = "glClear")]
        public static extern void Clear(int mask);

        [DllImport(OPENGL32, EntryPoint = "glBindTexture")]
        public static extern void BindTexture(int target, int texture);

        [DllImport(OPENGL32, EntryPoint = "glBlendFunc")]
        public static extern void BlendFunc(int sfactor, int dfactor);

        [DllImport(OPENGL32, EntryPoint = "glClearStencil")]
        public static extern void ClearStencil(int value);

        [DllImport(OPENGL32, EntryPoint = "glOrtho")]
        public static extern void Ortho(double left, double right, double bottom, double top, double zNear, double zFar);

        [DllImport(OPENGL32, EntryPoint = "glPointSize")]
        public static extern void PointSize(float size);

        [DllImport(OPENGL32, EntryPoint = "glPopMatrix")]
        public static extern void PopMatrix();

        [DllImport(OPENGL32, EntryPoint = "glRotated")]
        public static extern void Rotate(double angle, double x, double y, double z);

        [DllImport(OPENGL32, EntryPoint = "glMatrixMode")]
        public static extern void MatrixMode(int mode);

        [DllImport(OPENGL32, EntryPoint = "glNormal3d")]
        public static extern void Normal3(double nx, double ny, double nz);

        [DllImport(OPENGL32, EntryPoint = "glPushMatrix")]
        public static extern void PushMatrix();

        [DllImport(OPENGL32, EntryPoint = "glStencilFunc")]
        public static extern void StencilFunc(int func, int ref_notkeword, uint mask);

        [DllImport(OPENGL32, EntryPoint = "glStencilMask")]
        public static extern void StencilMask(uint mask);

        [DllImport(OPENGL32, EntryPoint = "glStencilOp")]
        public static extern void StencilOp(int fail, int zfail, int zpass);

        [DllImport(OPENGL32, EntryPoint = "glTexCoord2d")]
        public static extern void TexCoord2(double s, double t);

        [DllImport(OPENGL32, EntryPoint = "glTexImage2D")]
        public static extern void TexImage2D(int target, int level, int internalformat, int width, int height, int border, int format, int type, IntPtr pixels);

        [DllImport(OPENGL32, EntryPoint = "glTexParameteri")]
        public static extern void TexParameter(int target, int pname, int param);

        [DllImport(OPENGL32, EntryPoint = "glTranslated")]
        public static extern void Translate(double x, double y, double z);

        [DllImport(OPENGL32, EntryPoint = "glLoadIdentity")]
        public static extern void LoadIdentity();

        [DllImport(OPENGL32, EntryPoint = "glMaterialfv")]
        public static extern void Material(int face, int pname, float[] params_notkeyword);

        [DllImport(OPENGL32, EntryPoint = "glCullFace")]
        public static extern void CullFace(int mode);

        [DllImport(OPENGL32, EntryPoint = "glVertex2d")]
        public static extern void Vertex2(double x, double y);

        [DllImport(OPENGL32, EntryPoint = "glVertex3d")]
        public static extern void Vertex3(double x, double y, double z);

        [DllImport(OPENGL32, EntryPoint = "glBegin")]
        public static extern void Begin(int mode);

        [DllImport(OPENGL32, EntryPoint = "glEnd")]
        public static extern void End();

        [DllImport(OPENGL32, EntryPoint = "glDeleteTextures")]
        public static extern void DeleteTextures(int n, int[] textures);

        [DllImport(OPENGL32, EntryPoint = "glHint")]
        public static extern void Hint(int target, int mode);

        [DllImport(OPENGL32, EntryPoint = "glLightfv")]
        public static extern void Light(int light, int pname, float[] params_notkeyword);

        [DllImport(OPENGL32, EntryPoint = "glLineStipple")]
        public static extern void LineStipple(int factor, ushort pattern);

        [DllImport(OPENGL32, EntryPoint = "glLineWidth")]
        public static extern void LineWidth(float width);

        [DllImport(OPENGL32, EntryPoint = "glShadeModel")]
        public static extern void ShadeModel(int model);

        public static void ClearColor(Color color) => glClearColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

        public static void Color3(Color color) => glColor3f(color.R / 255f, color.G / 255f, color.B / 255f);

        public static void Color4(Color color) => glColor4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

        public static void ColorMask(bool r, bool g, bool b, bool a) => glColorMask(r ? (byte)1 : (byte)0, g ? (byte)1 : (byte)0, b ? (byte)1 : (byte)0, a ? (byte)1 : (byte)0);

        public static void DeleteTexture(int id) => DeleteTextures(1, new int[] { id });

        public static void DepthMask(bool flag) => glDepthMask(flag ? (byte)1 : (byte)0);

        public static int GenTexture()
        {
            uint[] names = new uint[1];
            glGenTextures(1, names);
            return (int)names[0];
        }

        public static void DrawLine(PointF p1, PointF p2, Pen pen)
        {
            Enable(BLEND);
            BlendFunc(SRC_ALPHA, ONE_MINUS_SRC_ALPHA);
            Enable(LINE_SMOOTH);
            Hint(LINE_SMOOTH_HINT, NICEST);

            if (pen.DashStyle != DashStyle.Solid)
            {
                Enable(LINE_STIPPLE);
                switch (pen.DashStyle)
                {
                    case DashStyle.Dash:
                        LineStipple(3, 0xAAAA);
                        break;
                    case DashStyle.Dot:
                        LineStipple(1, 0xAAAA);
                        break;
                    default:
                        break;
                }
            }

            LineWidth(pen.Width);
            Color3(pen.Color);

            Begin(LINES);

            Vertex2(p1.X, p1.Y);
            Vertex2(p2.X, p2.Y);

            End();

            if (pen.DashStyle != DashStyle.Solid)
            {
                Disable(LINE_STIPPLE);
            }
        }

        public static void DrawString(string text, Font font, Brush brush, PointF point, StringAlignment horizontalAlign = StringAlignment.Near, StringAlignment verticalAlign = StringAlignment.Near, bool antiAlias = false)
        {
            var formatFlags = System.Windows.Forms.TextFormatFlags.Default;

            if (horizontalAlign == StringAlignment.Center)
            {
                formatFlags |= System.Windows.Forms.TextFormatFlags.HorizontalCenter;
            }
            if (verticalAlign == StringAlignment.Center)
            {
                formatFlags |= System.Windows.Forms.TextFormatFlags.VerticalCenter;
            }

            var size = System.Windows.Forms.TextRenderer.MeasureText(text, font, Size.Empty, formatFlags);

            using (var textRenderer = new TextRenderer(size.Width, size.Height))
            {
                float x = point.X;
                float y = point.Y;
                if (formatFlags.HasFlag(System.Windows.Forms.TextFormatFlags.HorizontalCenter))
                {
                    x -= size.Width / 2;
                }
                if (formatFlags.HasFlag(System.Windows.Forms.TextFormatFlags.VerticalCenter))
                {
                    y += size.Height / 2;
                }

                textRenderer.DrawString(text, font, brush, new Vec2(x, y), antiAlias);
            }
        }

        public static void DrawEllipse(PointF center, Pen pen, double radius)
        {
            DrawEllipse(center, pen, radius, radius, 0);
        }

        public static void DrawEllipse(PointF center, Pen pen, double rx, double ry, double rotationDegrees)
        {
            Enable(BLEND);
            BlendFunc(SRC_ALPHA, ONE_MINUS_SRC_ALPHA);
            Enable(LINE_SMOOTH);
            Hint(LINE_SMOOTH_HINT, NICEST);

            LineWidth(pen.Width);
            Color3(pen.Color);

            if (pen.DashStyle != DashStyle.Solid)
            {
                Enable(LINE_STIPPLE);
                switch (pen.DashStyle)
                {
                    case DashStyle.Dash:
                        LineStipple(3, 0xAAAA);
                        break;
                    case DashStyle.Dot:
                        LineStipple(1, 0xAAAA);
                        break;
                    default:
                        break;
                }
            }

            Begin(LINE_STRIP);

            int count = 64;

            double rot = rotationDegrees / 180.0 * Math.PI;
            double cosRot = Math.Cos(rot);
            double sinRot = Math.Sin(rot);

            for (int i = 0; i <= count; i++)
            {
                double t = i / (double)count * (2 * Math.PI);
                double cost = Math.Cos(t);
                double sint = Math.Sin(t);

                double x = center.X + rx * cost * cosRot - ry * sint * sinRot;
                double y = center.Y + ry * sint * cosRot + rx * cost * sinRot;
                Vertex2(x, y);
            }

            End();
            LineWidth(1);
            Disable(LINE_STIPPLE);
        }

        public static Color Tint(this Color color, bool nightMode)
        {
            if (nightMode)
            {
                byte r = new byte[] { color.R, color.G, color.B }.Max();
                return Color.FromArgb(color.A, r, 0, 0);
            }
            else
            {
                return color;
            }
        }

        public static int GetTexture(string path, string fallbackPath = null, bool permanent = false, Action readyCallback = null)
        {
            return textureManager.GetTexture(path, fallbackPath, permanent, readyCallback);
        }

        public static void RemoveTexture(string path)
        {
            textureManager.RemoveTexture(path);
        }

        public static void Flush()
        {
            glFlush();
            textureManager.Cleanup();
        }
    }
}