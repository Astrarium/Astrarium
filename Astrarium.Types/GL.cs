using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace Astrarium.Types
{
    public static class GL
    {
        #region Interop

        private const string OPENGL32 = "opengl32";

        public const int GL_VENDOR = 0x1F00;
        public const int GL_RENDERER = 0x1F01;
        public const int GL_VERSION = 0x1F02;
        public const int GL_EXTENSIONS = 0x1F03;

        [DllImport(OPENGL32)]
        private static extern void glEnable(uint cap);

        [DllImport(OPENGL32)]
        private static extern void glDisable(uint cap);

        [DllImport(OPENGL32)]
        private static extern void glClear(int mask);

        [DllImport(OPENGL32)]
        private static extern void glViewport(int x, int y, int width, int height);

        [DllImport(OPENGL32)]
        private static extern void glCallList(uint list);

        [DllImport(OPENGL32)]
        private static extern IntPtr wglGetProcAddress(string function);

        [DllImport(OPENGL32)]
        private static extern void glBindTexture(uint target, uint texture);

        [DllImport(OPENGL32)]
        private static extern void glBlendFunc(uint sfactor, uint dfactor);

        [DllImport(OPENGL32)]
        private static extern void glClearStencil(int value);

        [DllImport(OPENGL32)]
        private static extern void glClearColor(float r, float g, float b, float alpha);

        [DllImport(OPENGL32)]
        private static extern void glGenTextures(int n, uint[] textures);

        [DllImport(OPENGL32)]
        private static extern void glOrtho(double left, double right, double bottom, double top, double zNear, double zFar);

        [DllImport(OPENGL32)]
        private static extern void glPointSize(float size);

        [DllImport(OPENGL32)]
        private static extern void glPopMatrix();

        [DllImport(OPENGL32)]
        private static extern void glRotated(double angle, double x, double y, double z);

        [DllImport(OPENGL32)]
        private static extern void glMatrixMode(uint mode);

        [DllImport(OPENGL32)]
        private static extern void glNormal3d(double nx, double ny, double nz);

        [DllImport(OPENGL32)]
        private static extern void glPushMatrix();

        [DllImport(OPENGL32)]
        private static extern void glShadeModel(uint mode);

        [DllImport(OPENGL32)]
        private static extern void glStencilFunc(uint func, int ref_notkeword, uint mask);

        [DllImport(OPENGL32)]
        private static extern void glStencilMask(uint mask);

        [DllImport(OPENGL32)]
        private static extern void glStencilOp(uint fail, uint zfail, uint zpass);

        [DllImport(OPENGL32)]
        private static extern void glTexCoord2d(double s, double t);

        [DllImport(OPENGL32)]
        private static extern void glTexImage2D(uint target, int level, uint internalformat, int width, int height, int border, uint format, uint type, IntPtr pixels);

        [DllImport(OPENGL32)]
        private static extern void glTexParameteri(uint target, uint pname, int param);

        [DllImport(OPENGL32)]
        private static extern void glTranslated(double x, double y, double z);

        [DllImport(OPENGL32)]
        private static extern void glLoadIdentity();

        [DllImport(OPENGL32)]
        private static extern void glMaterialfv(uint face, uint pname, float[] params_notkeyword);

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
        private static extern void glCullFace(uint mode);

        [DllImport(OPENGL32)]
        private static extern void glVertex2d(double x, double y);

        [DllImport(OPENGL32)]
        private static extern void glVertex3d(double x, double y, double z);

        [DllImport(OPENGL32)]
        private static extern void glBegin(int mode);

        [DllImport(OPENGL32)]
        private static extern void glEnd();

        [DllImport(OPENGL32)]
        private static extern void glFlush();

        [DllImport(OPENGL32)]
        private static extern void glDepthMask(byte flag);

        [DllImport(OPENGL32)]
        private static extern void glDepthFunc(int val);

        [DllImport(OPENGL32)]
        private static extern void glDeleteTextures(int n, uint[] textures);

        [DllImport(OPENGL32)]
        private static extern void glHint(uint target, uint mode);

        [DllImport(OPENGL32)]
        private static extern void glLightfv(uint light, uint pname, float[] params_notkeyword);

        [DllImport(OPENGL32)]
        private static extern void glLineStipple(int factor, ushort pattern);

        [DllImport(OPENGL32)]
        private static extern void glLineWidth(float width);

        [DllImport(OPENGL32, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr glGetString(uint name);

        public static string GetString(uint name)
        {
            return Marshal.PtrToStringAnsi(glGetString(name));
        }

        #endregion Interop

        public static void Enable(EnableCap enableCap)
        {
            glEnable((uint)enableCap);
        }

        public static void Disable(EnableCap enableCap)
        {
            glDisable((uint)enableCap);
        }

        public static void Begin(PrimitiveType mode)
        {
            glBegin((int)mode);
        }

        public static void End()
        {
            glEnd();
        }

        public static void Flush()
        {
            glFlush();
        }

        public static void BindTexture(TextureTarget target, int id)
        {
            glBindTexture((uint)target, (uint)id);
        }

        public static void BlendFunc(BlendingFactor sfactor, BlendingFactor dfactor)
        {
            glBlendFunc((uint)sfactor, (uint)dfactor);
        }

        public static void Clear(ClearBufferMask mask)
        {
            glClear((int)mask);
        }

        public static void ClearColor(Color color)
        {
            glClearColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }

        public static void ClearStencil(int value)
        {
            glClearStencil(value);
        }

        public static void Color3(Color color)
        {
            glColor3f(color.R / 255f, color.G / 255f, color.B / 255f);
        }

        public static void Color4(Color color)
        {
            glColor4f(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }

        public static void ColorMask(bool r, bool g, bool b, bool a)
        {
            glColorMask(r ? (byte)1 : (byte)0, g ? (byte)1 : (byte)0, b ? (byte)1 : (byte)0, a ? (byte)1 : (byte)0);
        }

        public static void CullFace(CullFaceMode mode)
        {
            glCullFace((uint)mode);
        }

        public static void DeleteTexture(int id)
        {
            glDeleteTextures(1, new uint[] { (uint)id });
        }

        public static void DeleteTextures(int n, int[] ids)
        {
            glDeleteTextures(n, ids.Select(x => (uint)x).ToArray());
        }

        public static void DepthMask(bool flag)
        {
            glDepthMask(flag ? (byte)1 : (byte)0);
        }

        public static void Hint(HintTarget target, HintMode mode)
        {
            glHint((uint)target, (uint)mode);
        }

        public static void Light(LightName light, LightParameter pname, float[] @params)
        {
            glLightfv((uint)light, (uint)pname, @params);
        }

        public static void LineStipple(int factor, ushort pattern)
        {
            glLineStipple(factor, pattern);
        }

        public static void LineWidth(float width)
        {
            glLineWidth(width);
        }

        public static void LoadIdentity()
        {
            glLoadIdentity();
        }

        public static void Material(MaterialFace face, MaterialParameter pname, float[] @params)
        {
            glMaterialfv((uint)face, (uint)pname, @params);
        }

        public static void MatrixMode(MatrixMode mode)
        {
            glMatrixMode((uint)mode);
        }

        public static void Normal3(double nx, double ny, double nz)
        {
            glNormal3d(nx, ny, nz);
        }

        public static void Ortho(double left, double right, double bottom, double top, double zNear, double zFar)
        {
            glOrtho(left, right, bottom, top, zNear, zFar);
        }

        public static void PointSize(float size)
        {
            glPointSize(size);
        }

        public static void PopMatrix()
        {
            glPopMatrix();
        }

        public static void PushMatrix()
        {
            glPushMatrix();
        }

        public static void Rotate(double angle, double x, double y, double z)
        {
            glRotated(angle, x, y, z);
        }

        public static void ShadeModel(ShadingModel model)
        {
            glShadeModel((uint)model);
        }

        public static void StencilFunc(StencilFunction func, int @ref, uint mask)
        {
            glStencilFunc((uint)func, @ref, mask);
        }

        public static void StencilMask(uint mask)
        {
            glStencilMask(mask);
        }

        public static void StencilOp(StencilOp fail, StencilOp zfail, StencilOp zpass)
        {
            glStencilOp((uint)fail, (uint)zfail, (uint)zpass);
        }

        public static void TexCoord2(double s, double t)
        {
            glTexCoord2d(s, t);
        }

        public static void TexImage2D(TextureTarget target, int level, PixelInternalFormat internalformat, int width, int height, int border, PixelFormat format, PixelType type, IntPtr pixels)
        {
            glTexImage2D((uint)target, level, (uint)internalformat, width, height, border, (uint)format, (uint)type, pixels);
        }

        public static void TexParameter(TextureTarget target, TextureParameterName pname, int param)
        {
            glTexParameteri((uint)target, (uint)pname, param);
        }

        public static void Translate(double x, double y, double z)
        {
            glTranslated(x, y, z);
        }

        public static void Vertex2(double x, double y)
        {
            glVertex2d(x, y);
        }

        public static void Vertex3(double x, double y, double z)
        {
            glVertex3d(x, y, z);
        }

        public static void Viewport(int x, int y, int width, int height)
        {
            glViewport(x, y, width, height);
        }

        public static int GenTexture()
        {
            uint[] names = new uint[1];
            glGenTextures(1, names);
            return (int)names[0];
        }
    }

    #region Enums

    public enum TextureWrapMode
    {
        //
        // Summary:
        //     Original was GL_CLAMP = 0x2900
        Clamp = 10496,
        //
        // Summary:
        //     Original was GL_REPEAT = 0x2901
        Repeat = 10497,
        //
        // Summary:
        //     Original was GL_CLAMP_TO_BORDER = 0x812D
        ClampToBorder = 33069,
        //
        // Summary:
        //     Original was GL_CLAMP_TO_BORDER_ARB = 0x812D
        ClampToBorderArb = 33069,
        //
        // Summary:
        //     Original was GL_CLAMP_TO_BORDER_NV = 0x812D
        ClampToBorderNv = 33069,
        //
        // Summary:
        //     Original was GL_CLAMP_TO_BORDER_SGIS = 0x812D
        ClampToBorderSgis = 33069,
        //
        // Summary:
        //     Original was GL_CLAMP_TO_EDGE = 0x812F
        ClampToEdge = 33071,
        //
        // Summary:
        //     Original was GL_CLAMP_TO_EDGE_SGIS = 0x812F
        ClampToEdgeSgis = 33071,
        //
        // Summary:
        //     Original was GL_MIRRORED_REPEAT = 0x8370
        MirroredRepeat = 33648
    }

    public enum TextureMinFilter
    {
        //
        // Summary:
        //     Original was GL_NEAREST = 0x2600
        Nearest = 9728,
        //
        // Summary:
        //     Original was GL_LINEAR = 0x2601
        Linear = 9729,
        //
        // Summary:
        //     Original was GL_NEAREST_MIPMAP_NEAREST = 0x2700
        NearestMipmapNearest = 9984,
        //
        // Summary:
        //     Original was GL_LINEAR_MIPMAP_NEAREST = 0x2701
        LinearMipmapNearest = 9985,
        //
        // Summary:
        //     Original was GL_NEAREST_MIPMAP_LINEAR = 0x2702
        NearestMipmapLinear = 9986,
        //
        // Summary:
        //     Original was GL_LINEAR_MIPMAP_LINEAR = 0x2703
        LinearMipmapLinear = 9987,
        //
        // Summary:
        //     Original was GL_FILTER4_SGIS = 0x8146
        Filter4Sgis = 33094,
        //
        // Summary:
        //     Original was GL_LINEAR_CLIPMAP_LINEAR_SGIX = 0x8170
        LinearClipmapLinearSgix = 33136,
        //
        // Summary:
        //     Original was GL_PIXEL_TEX_GEN_Q_CEILING_SGIX = 0x8184
        PixelTexGenQCeilingSgix = 33156,
        //
        // Summary:
        //     Original was GL_PIXEL_TEX_GEN_Q_ROUND_SGIX = 0x8185
        PixelTexGenQRoundSgix = 33157,
        //
        // Summary:
        //     Original was GL_PIXEL_TEX_GEN_Q_FLOOR_SGIX = 0x8186
        PixelTexGenQFloorSgix = 33158,
        //
        // Summary:
        //     Original was GL_NEAREST_CLIPMAP_NEAREST_SGIX = 0x844D
        NearestClipmapNearestSgix = 33869,
        //
        // Summary:
        //     Original was GL_NEAREST_CLIPMAP_LINEAR_SGIX = 0x844E
        NearestClipmapLinearSgix = 33870,
        //
        // Summary:
        //     Original was GL_LINEAR_CLIPMAP_NEAREST_SGIX = 0x844F
        LinearClipmapNearestSgix = 33871
    }

    public enum TextureMagFilter
    {
        //
        // Summary:
        //     Original was GL_NEAREST = 0x2600
        Nearest = 9728,
        //
        // Summary:
        //     Original was GL_LINEAR = 0x2601
        Linear = 9729,
        //
        // Summary:
        //     Original was GL_LINEAR_DETAIL_SGIS = 0x8097
        LinearDetailSgis = 32919,
        //
        // Summary:
        //     Original was GL_LINEAR_DETAIL_ALPHA_SGIS = 0x8098
        LinearDetailAlphaSgis = 32920,
        //
        // Summary:
        //     Original was GL_LINEAR_DETAIL_COLOR_SGIS = 0x8099
        LinearDetailColorSgis = 32921,
        //
        // Summary:
        //     Original was GL_LINEAR_SHARPEN_SGIS = 0x80AD
        LinearSharpenSgis = 32941,
        //
        // Summary:
        //     Original was GL_LINEAR_SHARPEN_ALPHA_SGIS = 0x80AE
        LinearSharpenAlphaSgis = 32942,
        //
        // Summary:
        //     Original was GL_LINEAR_SHARPEN_COLOR_SGIS = 0x80AF
        LinearSharpenColorSgis = 32943,
        //
        // Summary:
        //     Original was GL_FILTER4_SGIS = 0x8146
        Filter4Sgis = 33094,
        //
        // Summary:
        //     Original was GL_PIXEL_TEX_GEN_Q_CEILING_SGIX = 0x8184
        PixelTexGenQCeilingSgix = 33156,
        //
        // Summary:
        //     Original was GL_PIXEL_TEX_GEN_Q_ROUND_SGIX = 0x8185
        PixelTexGenQRoundSgix = 33157,
        //
        // Summary:
        //     Original was GL_PIXEL_TEX_GEN_Q_FLOOR_SGIX = 0x8186
        PixelTexGenQFloorSgix = 33158
    }

    public enum PixelFormat
    {
        //
        // Summary:
        //     Original was GL_UNSIGNED_SHORT = 0x1403
        UnsignedShort = 5123,
        //
        // Summary:
        //     Original was GL_UNSIGNED_INT = 0x1405
        UnsignedInt = 5125,
        //
        // Summary:
        //     Original was GL_COLOR_INDEX = 0x1900
        ColorIndex = 6400,
        //
        // Summary:
        //     Original was GL_STENCIL_INDEX = 0x1901
        StencilIndex = 6401,
        //
        // Summary:
        //     Original was GL_DEPTH_COMPONENT = 0x1902
        DepthComponent = 6402,
        //
        // Summary:
        //     Original was GL_RED = 0x1903
        Red = 6403,
        //
        // Summary:
        //     Original was GL_RED_EXT = 0x1903
        RedExt = 6403,
        //
        // Summary:
        //     Original was GL_GREEN = 0x1904
        Green = 6404,
        //
        // Summary:
        //     Original was GL_BLUE = 0x1905
        Blue = 6405,
        //
        // Summary:
        //     Original was GL_ALPHA = 0x1906
        Alpha = 6406,
        //
        // Summary:
        //     Original was GL_RGB = 0x1907
        Rgb = 6407,
        //
        // Summary:
        //     Original was GL_RGBA = 0x1908
        Rgba = 6408,
        //
        // Summary:
        //     Original was GL_LUMINANCE = 0x1909
        Luminance = 6409,
        //
        // Summary:
        //     Original was GL_LUMINANCE_ALPHA = 0x190A
        LuminanceAlpha = 6410,
        //
        // Summary:
        //     Original was GL_ABGR_EXT = 0x8000
        AbgrExt = 32768,
        //
        // Summary:
        //     Original was GL_CMYK_EXT = 0x800C
        CmykExt = 32780,
        //
        // Summary:
        //     Original was GL_CMYKA_EXT = 0x800D
        CmykaExt = 32781,
        //
        // Summary:
        //     Original was GL_BGR = 0x80E0
        Bgr = 32992,
        //
        // Summary:
        //     Original was GL_BGRA = 0x80E1
        Bgra = 32993,
        //
        // Summary:
        //     Original was GL_YCRCB_422_SGIX = 0x81BB
        Ycrcb422Sgix = 33211,
        //
        // Summary:
        //     Original was GL_YCRCB_444_SGIX = 0x81BC
        Ycrcb444Sgix = 33212,
        //
        // Summary:
        //     Original was GL_RG = 0x8227
        Rg = 33319,
        //
        // Summary:
        //     Original was GL_RG_INTEGER = 0x8228
        RgInteger = 33320,
        //
        // Summary:
        //     Original was GL_R5_G6_B5_ICC_SGIX = 0x8466
        R5G6B5IccSgix = 33894,
        //
        // Summary:
        //     Original was GL_R5_G6_B5_A8_ICC_SGIX = 0x8467
        R5G6B5A8IccSgix = 33895,
        //
        // Summary:
        //     Original was GL_ALPHA16_ICC_SGIX = 0x8468
        Alpha16IccSgix = 33896,
        //
        // Summary:
        //     Original was GL_LUMINANCE16_ICC_SGIX = 0x8469
        Luminance16IccSgix = 33897,
        //
        // Summary:
        //     Original was GL_LUMINANCE16_ALPHA8_ICC_SGIX = 0x846B
        Luminance16Alpha8IccSgix = 33899,
        //
        // Summary:
        //     Original was GL_DEPTH_STENCIL = 0x84F9
        DepthStencil = 34041,
        //
        // Summary:
        //     Original was GL_RED_INTEGER = 0x8D94
        RedInteger = 36244,
        //
        // Summary:
        //     Original was GL_GREEN_INTEGER = 0x8D95
        GreenInteger = 36245,
        //
        // Summary:
        //     Original was GL_BLUE_INTEGER = 0x8D96
        BlueInteger = 36246,
        //
        // Summary:
        //     Original was GL_ALPHA_INTEGER = 0x8D97
        AlphaInteger = 36247,
        //
        // Summary:
        //     Original was GL_RGB_INTEGER = 0x8D98
        RgbInteger = 36248,
        //
        // Summary:
        //     Original was GL_RGBA_INTEGER = 0x8D99
        RgbaInteger = 36249,
        //
        // Summary:
        //     Original was GL_BGR_INTEGER = 0x8D9A
        BgrInteger = 36250,
        //
        // Summary:
        //     Original was GL_BGRA_INTEGER = 0x8D9B
        BgraInteger = 36251
    }

    public enum PixelType
    {
        //
        // Summary:
        //     Original was GL_BYTE = 0x1400
        Byte = 5120,
        //
        // Summary:
        //     Original was GL_UNSIGNED_BYTE = 0x1401
        UnsignedByte = 5121,
        //
        // Summary:
        //     Original was GL_SHORT = 0x1402
        Short = 5122,
        //
        // Summary:
        //     Original was GL_UNSIGNED_SHORT = 0x1403
        UnsignedShort = 5123,
        //
        // Summary:
        //     Original was GL_INT = 0x1404
        Int = 5124,
        //
        // Summary:
        //     Original was GL_UNSIGNED_INT = 0x1405
        UnsignedInt = 5125,
        //
        // Summary:
        //     Original was GL_FLOAT = 0x1406
        Float = 5126,
        //
        // Summary:
        //     Original was GL_HALF_FLOAT = 0x140B
        HalfFloat = 5131,
        //
        // Summary:
        //     Original was GL_BITMAP = 0x1A00
        Bitmap = 6656,
        //
        // Summary:
        //     Original was GL_UNSIGNED_BYTE_3_3_2 = 0x8032
        UnsignedByte332 = 32818,
        //
        // Summary:
        //     Original was GL_UNSIGNED_BYTE_3_3_2_EXT = 0x8032
        UnsignedByte332Ext = 32818,
        //
        // Summary:
        //     Original was GL_UNSIGNED_SHORT_4_4_4_4 = 0x8033
        UnsignedShort4444 = 32819,
        //
        // Summary:
        //     Original was GL_UNSIGNED_SHORT_4_4_4_4_EXT = 0x8033
        UnsignedShort4444Ext = 32819,
        //
        // Summary:
        //     Original was GL_UNSIGNED_SHORT_5_5_5_1 = 0x8034
        UnsignedShort5551 = 32820,
        //
        // Summary:
        //     Original was GL_UNSIGNED_SHORT_5_5_5_1_EXT = 0x8034
        UnsignedShort5551Ext = 32820,
        //
        // Summary:
        //     Original was GL_UNSIGNED_INT_8_8_8_8 = 0x8035
        UnsignedInt8888 = 32821,
        //
        // Summary:
        //     Original was GL_UNSIGNED_INT_8_8_8_8_EXT = 0x8035
        UnsignedInt8888Ext = 32821,
        //
        // Summary:
        //     Original was GL_UNSIGNED_INT_10_10_10_2 = 0x8036
        UnsignedInt1010102 = 32822,
        //
        // Summary:
        //     Original was GL_UNSIGNED_INT_10_10_10_2_EXT = 0x8036
        UnsignedInt1010102Ext = 32822,
        //
        // Summary:
        //     Original was GL_UNSIGNED_BYTE_2_3_3_REVERSED = 0x8362
        UnsignedByte233Reversed = 33634,
        //
        // Summary:
        //     Original was GL_UNSIGNED_SHORT_5_6_5 = 0x8363
        UnsignedShort565 = 33635,
        //
        // Summary:
        //     Original was GL_UNSIGNED_SHORT_5_6_5_REVERSED = 0x8364
        UnsignedShort565Reversed = 33636,
        //
        // Summary:
        //     Original was GL_UNSIGNED_SHORT_4_4_4_4_REVERSED = 0x8365
        UnsignedShort4444Reversed = 33637,
        //
        // Summary:
        //     Original was GL_UNSIGNED_SHORT_1_5_5_5_REVERSED = 0x8366
        UnsignedShort1555Reversed = 33638,
        //
        // Summary:
        //     Original was GL_UNSIGNED_INT_8_8_8_8_REVERSED = 0x8367
        UnsignedInt8888Reversed = 33639,
        //
        // Summary:
        //     Original was GL_UNSIGNED_INT_2_10_10_10_REVERSED = 0x8368
        UnsignedInt2101010Reversed = 33640,
        //
        // Summary:
        //     Original was GL_UNSIGNED_INT_24_8 = 0x84FA
        UnsignedInt248 = 34042,
        //
        // Summary:
        //     Original was GL_UNSIGNED_INT_10F_11F_11F_REV = 0x8C3B
        UnsignedInt10F11F11FRev = 35899,
        //
        // Summary:
        //     Original was GL_UNSIGNED_INT_5_9_9_9_REV = 0x8C3E
        UnsignedInt5999Rev = 35902,
        //
        // Summary:
        //     Original was GL_FLOAT_32_UNSIGNED_INT_24_8_REV = 0x8DAD
        Float32UnsignedInt248Rev = 36269
    }

    public enum PixelInternalFormat
    {
        //
        // Summary:
        //     Original was GL_ONE = 1
        One = 1,
        //
        // Summary:
        //     Original was GL_TWO = 2
        Two = 2,
        //
        // Summary:
        //     Original was GL_THREE = 3
        Three = 3,
        //
        // Summary:
        //     Original was GL_FOUR = 4
        Four = 4,
        //
        // Summary:
        //     Original was GL_DEPTH_COMPONENT = 0x1902
        DepthComponent = 6402,
        //
        // Summary:
        //     Original was GL_ALPHA = 0x1906
        Alpha = 6406,
        //
        // Summary:
        //     Original was GL_RGB = 0x1907
        Rgb = 6407,
        //
        // Summary:
        //     Original was GL_RGBA = 0x1908
        Rgba = 6408,
        //
        // Summary:
        //     Original was GL_LUMINANCE = 0x1909
        Luminance = 6409,
        //
        // Summary:
        //     Original was GL_LUMINANCE_ALPHA = 0x190A
        LuminanceAlpha = 6410,
        //
        // Summary:
        //     Original was GL_R3_G3_B2 = 0x2A10
        R3G3B2 = 10768,
        //
        // Summary:
        //     Original was GL_ALPHA4 = 0x803B
        Alpha4 = 32827,
        //
        // Summary:
        //     Original was GL_ALPHA8 = 0x803C
        Alpha8 = 32828,
        //
        // Summary:
        //     Original was GL_ALPHA12 = 0x803D
        Alpha12 = 32829,
        //
        // Summary:
        //     Original was GL_ALPHA16 = 0x803E
        Alpha16 = 32830,
        //
        // Summary:
        //     Original was GL_LUMINANCE4 = 0x803F
        Luminance4 = 32831,
        //
        // Summary:
        //     Original was GL_LUMINANCE8 = 0x8040
        Luminance8 = 32832,
        //
        // Summary:
        //     Original was GL_LUMINANCE12 = 0x8041
        Luminance12 = 32833,
        //
        // Summary:
        //     Original was GL_LUMINANCE16 = 0x8042
        Luminance16 = 32834,
        //
        // Summary:
        //     Original was GL_LUMINANCE4_ALPHA4 = 0x8043
        Luminance4Alpha4 = 32835,
        //
        // Summary:
        //     Original was GL_LUMINANCE6_ALPHA2 = 0x8044
        Luminance6Alpha2 = 32836,
        //
        // Summary:
        //     Original was GL_LUMINANCE8_ALPHA8 = 0x8045
        Luminance8Alpha8 = 32837,
        //
        // Summary:
        //     Original was GL_LUMINANCE12_ALPHA4 = 0x8046
        Luminance12Alpha4 = 32838,
        //
        // Summary:
        //     Original was GL_LUMINANCE12_ALPHA12 = 0x8047
        Luminance12Alpha12 = 32839,
        //
        // Summary:
        //     Original was GL_LUMINANCE16_ALPHA16 = 0x8048
        Luminance16Alpha16 = 32840,
        //
        // Summary:
        //     Original was GL_INTENSITY = 0x8049
        Intensity = 32841,
        //
        // Summary:
        //     Original was GL_INTENSITY4 = 0x804A
        Intensity4 = 32842,
        //
        // Summary:
        //     Original was GL_INTENSITY8 = 0x804B
        Intensity8 = 32843,
        //
        // Summary:
        //     Original was GL_INTENSITY12 = 0x804C
        Intensity12 = 32844,
        //
        // Summary:
        //     Original was GL_INTENSITY16 = 0x804D
        Intensity16 = 32845,
        //
        // Summary:
        //     Original was GL_RGB2_EXT = 0x804E
        Rgb2Ext = 32846,
        //
        // Summary:
        //     Original was GL_RGB4 = 0x804F
        Rgb4 = 32847,
        //
        // Summary:
        //     Original was GL_RGB5 = 0x8050
        Rgb5 = 32848,
        //
        // Summary:
        //     Original was GL_RGB8 = 0x8051
        Rgb8 = 32849,
        //
        // Summary:
        //     Original was GL_RGB10 = 0x8052
        Rgb10 = 32850,
        //
        // Summary:
        //     Original was GL_RGB12 = 0x8053
        Rgb12 = 32851,
        //
        // Summary:
        //     Original was GL_RGB16 = 0x8054
        Rgb16 = 32852,
        //
        // Summary:
        //     Original was GL_RGBA2 = 0x8055
        Rgba2 = 32853,
        //
        // Summary:
        //     Original was GL_RGBA4 = 0x8056
        Rgba4 = 32854,
        //
        // Summary:
        //     Original was GL_RGB5_A1 = 0x8057
        Rgb5A1 = 32855,
        //
        // Summary:
        //     Original was GL_RGBA8 = 0x8058
        Rgba8 = 32856,
        //
        // Summary:
        //     Original was GL_RGB10_A2 = 0x8059
        Rgb10A2 = 32857,
        //
        // Summary:
        //     Original was GL_RGBA12 = 0x805A
        Rgba12 = 32858,
        //
        // Summary:
        //     Original was GL_RGBA16 = 0x805B
        Rgba16 = 32859,
        //
        // Summary:
        //     Original was GL_DUAL_ALPHA4_SGIS = 0x8110
        DualAlpha4Sgis = 33040,
        //
        // Summary:
        //     Original was GL_DUAL_ALPHA8_SGIS = 0x8111
        DualAlpha8Sgis = 33041,
        //
        // Summary:
        //     Original was GL_DUAL_ALPHA12_SGIS = 0x8112
        DualAlpha12Sgis = 33042,
        //
        // Summary:
        //     Original was GL_DUAL_ALPHA16_SGIS = 0x8113
        DualAlpha16Sgis = 33043,
        //
        // Summary:
        //     Original was GL_DUAL_LUMINANCE4_SGIS = 0x8114
        DualLuminance4Sgis = 33044,
        //
        // Summary:
        //     Original was GL_DUAL_LUMINANCE8_SGIS = 0x8115
        DualLuminance8Sgis = 33045,
        //
        // Summary:
        //     Original was GL_DUAL_LUMINANCE12_SGIS = 0x8116
        DualLuminance12Sgis = 33046,
        //
        // Summary:
        //     Original was GL_DUAL_LUMINANCE16_SGIS = 0x8117
        DualLuminance16Sgis = 33047,
        //
        // Summary:
        //     Original was GL_DUAL_INTENSITY4_SGIS = 0x8118
        DualIntensity4Sgis = 33048,
        //
        // Summary:
        //     Original was GL_DUAL_INTENSITY8_SGIS = 0x8119
        DualIntensity8Sgis = 33049,
        //
        // Summary:
        //     Original was GL_DUAL_INTENSITY12_SGIS = 0x811A
        DualIntensity12Sgis = 33050,
        //
        // Summary:
        //     Original was GL_DUAL_INTENSITY16_SGIS = 0x811B
        DualIntensity16Sgis = 33051,
        //
        // Summary:
        //     Original was GL_DUAL_LUMINANCE_ALPHA4_SGIS = 0x811C
        DualLuminanceAlpha4Sgis = 33052,
        //
        // Summary:
        //     Original was GL_DUAL_LUMINANCE_ALPHA8_SGIS = 0x811D
        DualLuminanceAlpha8Sgis = 33053,
        //
        // Summary:
        //     Original was GL_QUAD_ALPHA4_SGIS = 0x811E
        QuadAlpha4Sgis = 33054,
        //
        // Summary:
        //     Original was GL_QUAD_ALPHA8_SGIS = 0x811F
        QuadAlpha8Sgis = 33055,
        //
        // Summary:
        //     Original was GL_QUAD_LUMINANCE4_SGIS = 0x8120
        QuadLuminance4Sgis = 33056,
        //
        // Summary:
        //     Original was GL_QUAD_LUMINANCE8_SGIS = 0x8121
        QuadLuminance8Sgis = 33057,
        //
        // Summary:
        //     Original was GL_QUAD_INTENSITY4_SGIS = 0x8122
        QuadIntensity4Sgis = 33058,
        //
        // Summary:
        //     Original was GL_QUAD_INTENSITY8_SGIS = 0x8123
        QuadIntensity8Sgis = 33059,
        //
        // Summary:
        //     Original was GL_DEPTH_COMPONENT16 = 0x81a5
        DepthComponent16 = 33189,
        //
        // Summary:
        //     Original was GL_DEPTH_COMPONENT16_SGIX = 0x81A5
        DepthComponent16Sgix = 33189,
        //
        // Summary:
        //     Original was GL_DEPTH_COMPONENT24 = 0x81a6
        DepthComponent24 = 33190,
        //
        // Summary:
        //     Original was GL_DEPTH_COMPONENT24_SGIX = 0x81A6
        DepthComponent24Sgix = 33190,
        //
        // Summary:
        //     Original was GL_DEPTH_COMPONENT32 = 0x81a7
        DepthComponent32 = 33191,
        //
        // Summary:
        //     Original was GL_DEPTH_COMPONENT32_SGIX = 0x81A7
        DepthComponent32Sgix = 33191,
        //
        // Summary:
        //     Original was GL_COMPRESSED_RED = 0x8225
        CompressedRed = 33317,
        //
        // Summary:
        //     Original was GL_COMPRESSED_RG = 0x8226
        CompressedRg = 33318,
        //
        // Summary:
        //     Original was GL_R8 = 0x8229
        R8 = 33321,
        //
        // Summary:
        //     Original was GL_R16 = 0x822A
        R16 = 33322,
        //
        // Summary:
        //     Original was GL_RG8 = 0x822B
        Rg8 = 33323,
        //
        // Summary:
        //     Original was GL_RG16 = 0x822C
        Rg16 = 33324,
        //
        // Summary:
        //     Original was GL_R16F = 0x822D
        R16f = 33325,
        //
        // Summary:
        //     Original was GL_R32F = 0x822E
        R32f = 33326,
        //
        // Summary:
        //     Original was GL_RG16F = 0x822F
        Rg16f = 33327,
        //
        // Summary:
        //     Original was GL_RG32F = 0x8230
        Rg32f = 33328,
        //
        // Summary:
        //     Original was GL_R8I = 0x8231
        R8i = 33329,
        //
        // Summary:
        //     Original was GL_R8UI = 0x8232
        R8ui = 33330,
        //
        // Summary:
        //     Original was GL_R16I = 0x8233
        R16i = 33331,
        //
        // Summary:
        //     Original was GL_R16UI = 0x8234
        R16ui = 33332,
        //
        // Summary:
        //     Original was GL_R32I = 0x8235
        R32i = 33333,
        //
        // Summary:
        //     Original was GL_R32UI = 0x8236
        R32ui = 33334,
        //
        // Summary:
        //     Original was GL_RG8I = 0x8237
        Rg8i = 33335,
        //
        // Summary:
        //     Original was GL_RG8UI = 0x8238
        Rg8ui = 33336,
        //
        // Summary:
        //     Original was GL_RG16I = 0x8239
        Rg16i = 33337,
        //
        // Summary:
        //     Original was GL_RG16UI = 0x823A
        Rg16ui = 33338,
        //
        // Summary:
        //     Original was GL_RG32I = 0x823B
        Rg32i = 33339,
        //
        // Summary:
        //     Original was GL_RG32UI = 0x823C
        Rg32ui = 33340,
        //
        // Summary:
        //     Original was GL_COMPRESSED_RGB_S3TC_DXT1_EXT = 0x83F0
        CompressedRgbS3tcDxt1Ext = 33776,
        //
        // Summary:
        //     Original was GL_COMPRESSED_RGBA_S3TC_DXT1_EXT = 0x83F1
        CompressedRgbaS3tcDxt1Ext = 33777,
        //
        // Summary:
        //     Original was GL_COMPRESSED_RGBA_S3TC_DXT3_EXT = 0x83F2
        CompressedRgbaS3tcDxt3Ext = 33778,
        //
        // Summary:
        //     Original was GL_COMPRESSED_RGBA_S3TC_DXT5_EXT = 0x83F3
        CompressedRgbaS3tcDxt5Ext = 33779,
        //
        // Summary:
        //     Original was GL_RGB_ICC_SGIX = 0x8460
        RgbIccSgix = 33888,
        //
        // Summary:
        //     Original was GL_RGBA_ICC_SGIX = 0x8461
        RgbaIccSgix = 33889,
        //
        // Summary:
        //     Original was GL_ALPHA_ICC_SGIX = 0x8462
        AlphaIccSgix = 33890,
        //
        // Summary:
        //     Original was GL_LUMINANCE_ICC_SGIX = 0x8463
        LuminanceIccSgix = 33891,
        //
        // Summary:
        //     Original was GL_INTENSITY_ICC_SGIX = 0x8464
        IntensityIccSgix = 33892,
        //
        // Summary:
        //     Original was GL_LUMINANCE_ALPHA_ICC_SGIX = 0x8465
        LuminanceAlphaIccSgix = 33893,
        //
        // Summary:
        //     Original was GL_R5_G6_B5_ICC_SGIX = 0x8466
        R5G6B5IccSgix = 33894,
        //
        // Summary:
        //     Original was GL_R5_G6_B5_A8_ICC_SGIX = 0x8467
        R5G6B5A8IccSgix = 33895,
        //
        // Summary:
        //     Original was GL_ALPHA16_ICC_SGIX = 0x8468
        Alpha16IccSgix = 33896,
        //
        // Summary:
        //     Original was GL_LUMINANCE16_ICC_SGIX = 0x8469
        Luminance16IccSgix = 33897,
        //
        // Summary:
        //     Original was GL_INTENSITY16_ICC_SGIX = 0x846A
        Intensity16IccSgix = 33898,
        //
        // Summary:
        //     Original was GL_LUMINANCE16_ALPHA8_ICC_SGIX = 0x846B
        Luminance16Alpha8IccSgix = 33899,
        //
        // Summary:
        //     Original was GL_COMPRESSED_ALPHA = 0x84E9
        CompressedAlpha = 34025,
        //
        // Summary:
        //     Original was GL_COMPRESSED_LUMINANCE = 0x84EA
        CompressedLuminance = 34026,
        //
        // Summary:
        //     Original was GL_COMPRESSED_LUMINANCE_ALPHA = 0x84EB
        CompressedLuminanceAlpha = 34027,
        //
        // Summary:
        //     Original was GL_COMPRESSED_INTENSITY = 0x84EC
        CompressedIntensity = 34028,
        //
        // Summary:
        //     Original was GL_COMPRESSED_RGB = 0x84ED
        CompressedRgb = 34029,
        //
        // Summary:
        //     Original was GL_COMPRESSED_RGBA = 0x84EE
        CompressedRgba = 34030,
        //
        // Summary:
        //     Original was GL_DEPTH_STENCIL = 0x84F9
        DepthStencil = 34041,
        //
        // Summary:
        //     Original was GL_RGBA32F = 0x8814
        Rgba32f = 34836,
        //
        // Summary:
        //     Original was GL_RGB32F = 0x8815
        Rgb32f = 34837,
        //
        // Summary:
        //     Original was GL_RGBA16F = 0x881A
        Rgba16f = 34842,
        //
        // Summary:
        //     Original was GL_RGB16F = 0x881B
        Rgb16f = 34843,
        //
        // Summary:
        //     Original was GL_DEPTH24_STENCIL8 = 0x88F0
        Depth24Stencil8 = 35056,
        //
        // Summary:
        //     Original was GL_R11F_G11F_B10F = 0x8C3A
        R11fG11fB10f = 35898,
        //
        // Summary:
        //     Original was GL_RGB9_E5 = 0x8C3D
        Rgb9E5 = 35901,
        //
        // Summary:
        //     Original was GL_SRGB = 0x8C40
        Srgb = 35904,
        //
        // Summary:
        //     Original was GL_SRGB8 = 0x8C41
        Srgb8 = 35905,
        //
        // Summary:
        //     Original was GL_SRGB_ALPHA = 0x8C42
        SrgbAlpha = 35906,
        //
        // Summary:
        //     Original was GL_SRGB8_ALPHA8 = 0x8C43
        Srgb8Alpha8 = 35907,
        //
        // Summary:
        //     Original was GL_SLUMINANCE_ALPHA = 0x8C44
        SluminanceAlpha = 35908,
        //
        // Summary:
        //     Original was GL_SLUMINANCE8_ALPHA8 = 0x8C45
        Sluminance8Alpha8 = 35909,
        //
        // Summary:
        //     Original was GL_SLUMINANCE = 0x8C46
        Sluminance = 35910,
        //
        // Summary:
        //     Original was GL_SLUMINANCE8 = 0x8C47
        Sluminance8 = 35911,
        //
        // Summary:
        //     Original was GL_COMPRESSED_SRGB = 0x8C48
        CompressedSrgb = 35912,
        //
        // Summary:
        //     Original was GL_COMPRESSED_SRGB_ALPHA = 0x8C49
        CompressedSrgbAlpha = 35913,
        //
        // Summary:
        //     Original was GL_COMPRESSED_SLUMINANCE = 0x8C4A
        CompressedSluminance = 35914,
        //
        // Summary:
        //     Original was GL_COMPRESSED_SLUMINANCE_ALPHA = 0x8C4B
        CompressedSluminanceAlpha = 35915,
        //
        // Summary:
        //     Original was GL_COMPRESSED_SRGB_S3TC_DXT1_EXT = 0x8C4C
        CompressedSrgbS3tcDxt1Ext = 35916,
        //
        // Summary:
        //     Original was GL_COMPRESSED_SRGB_ALPHA_S3TC_DXT1_EXT = 0x8C4D
        CompressedSrgbAlphaS3tcDxt1Ext = 35917,
        //
        // Summary:
        //     Original was GL_COMPRESSED_SRGB_ALPHA_S3TC_DXT3_EXT = 0x8C4E
        CompressedSrgbAlphaS3tcDxt3Ext = 35918,
        //
        // Summary:
        //     Original was GL_COMPRESSED_SRGB_ALPHA_S3TC_DXT5_EXT = 0x8C4F
        CompressedSrgbAlphaS3tcDxt5Ext = 35919,
        //
        // Summary:
        //     Original was GL_DEPTH_COMPONENT32F = 0x8CAC
        DepthComponent32f = 36012,
        //
        // Summary:
        //     Original was GL_DEPTH32F_STENCIL8 = 0x8CAD
        Depth32fStencil8 = 36013,
        //
        // Summary:
        //     Original was GL_RGBA32UI = 0x8D70
        Rgba32ui = 36208,
        //
        // Summary:
        //     Original was GL_RGB32UI = 0x8D71
        Rgb32ui = 36209,
        //
        // Summary:
        //     Original was GL_RGBA16UI = 0x8D76
        Rgba16ui = 36214,
        //
        // Summary:
        //     Original was GL_RGB16UI = 0x8D77
        Rgb16ui = 36215,
        //
        // Summary:
        //     Original was GL_RGBA8UI = 0x8D7C
        Rgba8ui = 36220,
        //
        // Summary:
        //     Original was GL_RGB8UI = 0x8D7D
        Rgb8ui = 36221,
        //
        // Summary:
        //     Original was GL_RGBA32I = 0x8D82
        Rgba32i = 36226,
        //
        // Summary:
        //     Original was GL_RGB32I = 0x8D83
        Rgb32i = 36227,
        //
        // Summary:
        //     Original was GL_RGBA16I = 0x8D88
        Rgba16i = 36232,
        //
        // Summary:
        //     Original was GL_RGB16I = 0x8D89
        Rgb16i = 36233,
        //
        // Summary:
        //     Original was GL_RGBA8I = 0x8D8E
        Rgba8i = 36238,
        //
        // Summary:
        //     Original was GL_RGB8I = 0x8D8F
        Rgb8i = 36239,
        //
        // Summary:
        //     Original was GL_FLOAT_32_UNSIGNED_INT_24_8_REV = 0x8DAD
        Float32UnsignedInt248Rev = 36269,
        //
        // Summary:
        //     Original was GL_COMPRESSED_RED_RGTC1 = 0x8DBB
        CompressedRedRgtc1 = 36283,
        //
        // Summary:
        //     Original was GL_COMPRESSED_SIGNED_RED_RGTC1 = 0x8DBC
        CompressedSignedRedRgtc1 = 36284,
        //
        // Summary:
        //     Original was GL_COMPRESSED_RG_RGTC2 = 0x8DBD
        CompressedRgRgtc2 = 36285,
        //
        // Summary:
        //     Original was GL_COMPRESSED_SIGNED_RG_RGTC2 = 0x8DBE
        CompressedSignedRgRgtc2 = 36286,
        //
        // Summary:
        //     Original was GL_COMPRESSED_RGBA_BPTC_UNORM = 0x8E8C
        CompressedRgbaBptcUnorm = 36492,
        //
        // Summary:
        //     Original was GL_COMPRESSED_SRGB_ALPHA_BPTC_UNORM = 0x8E8D
        CompressedSrgbAlphaBptcUnorm = 36493,
        //
        // Summary:
        //     Original was GL_COMPRESSED_RGB_BPTC_SIGNED_FLOAT = 0x8E8E
        CompressedRgbBptcSignedFloat = 36494,
        //
        // Summary:
        //     Original was GL_COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT = 0x8E8F
        CompressedRgbBptcUnsignedFloat = 36495,
        //
        // Summary:
        //     Original was GL_R8_SNORM = 0x8F94
        R8Snorm = 36756,
        //
        // Summary:
        //     Original was GL_RG8_SNORM = 0x8F95
        Rg8Snorm = 36757,
        //
        // Summary:
        //     Original was GL_RGB8_SNORM = 0x8F96
        Rgb8Snorm = 36758,
        //
        // Summary:
        //     Original was GL_RGBA8_SNORM = 0x8F97
        Rgba8Snorm = 36759,
        //
        // Summary:
        //     Original was GL_R16_SNORM = 0x8F98
        R16Snorm = 36760,
        //
        // Summary:
        //     Original was GL_RG16_SNORM = 0x8F99
        Rg16Snorm = 36761,
        //
        // Summary:
        //     Original was GL_RGB16_SNORM = 0x8F9A
        Rgb16Snorm = 36762,
        //
        // Summary:
        //     Original was GL_RGBA16_SNORM = 0x8F9B
        Rgba16Snorm = 36763,
        //
        // Summary:
        //     Original was GL_RGB10_A2UI = 0x906F
        Rgb10A2ui = 36975
    }

    public enum StencilOp
    {
        //
        // Summary:
        //     Original was GL_ZERO = 0
        Zero = 0,
        //
        // Summary:
        //     Original was GL_INVERT = 0x150A
        Invert = 5386,
        //
        // Summary:
        //     Original was GL_KEEP = 0x1E00
        Keep = 7680,
        //
        // Summary:
        //     Original was GL_REPLACE = 0x1E01
        Replace = 7681,
        //
        // Summary:
        //     Original was GL_INCR = 0x1E02
        Incr = 7682,
        //
        // Summary:
        //     Original was GL_DECR = 0x1E03
        Decr = 7683,
        //
        // Summary:
        //     Original was GL_INCR_WRAP = 0x8507
        IncrWrap = 34055,
        //
        // Summary:
        //     Original was GL_DECR_WRAP = 0x8508
        DecrWrap = 34056
    }

    public enum StencilFunction
    {
        //
        // Summary:
        //     Original was GL_NEVER = 0x0200
        Never = 512,
        //
        // Summary:
        //     Original was GL_LESS = 0x0201
        Less = 513,
        //
        // Summary:
        //     Original was GL_EQUAL = 0x0202
        Equal = 514,
        //
        // Summary:
        //     Original was GL_LEQUAL = 0x0203
        Lequal = 515,
        //
        // Summary:
        //     Original was GL_GREATER = 0x0204
        Greater = 516,
        //
        // Summary:
        //     Original was GL_NOTEQUAL = 0x0205
        Notequal = 517,
        //
        // Summary:
        //     Original was GL_GEQUAL = 0x0206
        Gequal = 518,
        //
        // Summary:
        //     Original was GL_ALWAYS = 0x0207
        Always = 519
    }

    public enum ShadingModel
    {
        //
        // Summary:
        //     Original was GL_FLAT = 0x1D00
        Flat = 7424,
        //
        // Summary:
        //     Original was GL_SMOOTH = 0x1D01
        Smooth = 7425
    }

    public enum MatrixMode
    {
        //
        // Summary:
        //     Original was GL_MODELVIEW = 0x1700
        Modelview = 5888,
        //
        // Summary:
        //     Original was GL_MODELVIEW0_EXT = 0x1700
        Modelview0Ext = 5888,
        //
        // Summary:
        //     Original was GL_PROJECTION = 0x1701
        Projection = 5889,
        //
        // Summary:
        //     Original was GL_TEXTURE = 0x1702
        Texture = 5890,
        //
        // Summary:
        //     Original was GL_COLOR = 0x1800
        Color = 6144
    }

    public enum MaterialParameter
    {
        //
        // Summary:
        //     Original was GL_AMBIENT = 0x1200
        Ambient = 4608,
        //
        // Summary:
        //     Original was GL_DIFFUSE = 0x1201
        Diffuse = 4609,
        //
        // Summary:
        //     Original was GL_SPECULAR = 0x1202
        Specular = 4610,
        //
        // Summary:
        //     Original was GL_EMISSION = 0x1600
        Emission = 5632,
        //
        // Summary:
        //     Original was GL_SHININESS = 0x1601
        Shininess = 5633,
        //
        // Summary:
        //     Original was GL_AMBIENT_AND_DIFFUSE = 0x1602
        AmbientAndDiffuse = 5634,
        //
        // Summary:
        //     Original was GL_COLOR_INDEXES = 0x1603
        ColorIndexes = 5635
    }

    public enum MaterialFace
    {
        //
        // Summary:
        //     Original was GL_FRONT = 0x0404
        Front = 1028,
        //
        // Summary:
        //     Original was GL_BACK = 0x0405
        Back = 1029,
        //
        // Summary:
        //     Original was GL_FRONT_AND_BACK = 0x0408
        FrontAndBack = 1032
    }

    public enum LightParameter
    {
        //
        // Summary:
        //     Original was GL_AMBIENT = 0x1200
        Ambient = 4608,
        //
        // Summary:
        //     Original was GL_DIFFUSE = 0x1201
        Diffuse = 4609,
        //
        // Summary:
        //     Original was GL_SPECULAR = 0x1202
        Specular = 4610,
        //
        // Summary:
        //     Original was GL_POSITION = 0x1203
        Position = 4611,
        //
        // Summary:
        //     Original was GL_SPOT_DIRECTION = 0x1204
        SpotDirection = 4612,
        //
        // Summary:
        //     Original was GL_SPOT_EXPONENT = 0x1205
        SpotExponent = 4613,
        //
        // Summary:
        //     Original was GL_SPOT_CUTOFF = 0x1206
        SpotCutoff = 4614,
        //
        // Summary:
        //     Original was GL_CONSTANT_ATTENUATION = 0x1207
        ConstantAttenuation = 4615,
        //
        // Summary:
        //     Original was GL_LINEAR_ATTENUATION = 0x1208
        LinearAttenuation = 4616,
        //
        // Summary:
        //     Original was GL_QUADRATIC_ATTENUATION = 0x1209
        QuadraticAttenuation = 4617
    }

    public enum LightName : uint
    {
        //
        // Summary:
        //     Original was GL_LIGHT0 = 0x4000
        Light0 = 16384,
        //
        // Summary:
        //     Original was GL_LIGHT1 = 0x4001
        Light1 = 16385,
        //
        // Summary:
        //     Original was GL_LIGHT2 = 0x4002
        Light2 = 16386,
        //
        // Summary:
        //     Original was GL_LIGHT3 = 0x4003
        Light3 = 16387,
        //
        // Summary:
        //     Original was GL_LIGHT4 = 0x4004
        Light4 = 16388,
        //
        // Summary:
        //     Original was GL_LIGHT5 = 0x4005
        Light5 = 16389,
        //
        // Summary:
        //     Original was GL_LIGHT6 = 0x4006
        Light6 = 16390,
        //
        // Summary:
        //     Original was GL_LIGHT7 = 0x4007
        Light7 = 16391,
        //
        // Summary:
        //     Original was GL_FRAGMENT_LIGHT0_SGIX = 0x840C
        FragmentLight0Sgix = 33804,
        //
        // Summary:
        //     Original was GL_FRAGMENT_LIGHT1_SGIX = 0x840D
        FragmentLight1Sgix = 33805,
        //
        // Summary:
        //     Original was GL_FRAGMENT_LIGHT2_SGIX = 0x840E
        FragmentLight2Sgix = 33806,
        //
        // Summary:
        //     Original was GL_FRAGMENT_LIGHT3_SGIX = 0x840F
        FragmentLight3Sgix = 33807,
        //
        // Summary:
        //     Original was GL_FRAGMENT_LIGHT4_SGIX = 0x8410
        FragmentLight4Sgix = 33808,
        //
        // Summary:
        //     Original was GL_FRAGMENT_LIGHT5_SGIX = 0x8411
        FragmentLight5Sgix = 33809,
        //
        // Summary:
        //     Original was GL_FRAGMENT_LIGHT6_SGIX = 0x8412
        FragmentLight6Sgix = 33810,
        //
        // Summary:
        //     Original was GL_FRAGMENT_LIGHT7_SGIX = 0x8413
        FragmentLight7Sgix = 33811
    }

    public enum HintTarget : uint
    {
        //
        // Summary:
        //     Original was GL_PERSPECTIVE_CORRECTION_HINT = 0x0C50
        PerspectiveCorrectionHint = 3152,
        //
        // Summary:
        //     Original was GL_POINT_SMOOTH_HINT = 0x0C51
        PointSmoothHint = 3153,
        //
        // Summary:
        //     Original was GL_LINE_SMOOTH_HINT = 0x0C52
        LineSmoothHint = 3154,
        //
        // Summary:
        //     Original was GL_POLYGON_SMOOTH_HINT = 0x0C53
        PolygonSmoothHint = 3155,
        //
        // Summary:
        //     Original was GL_FOG_HINT = 0x0C54
        FogHint = 3156,
        //
        // Summary:
        //     Original was GL_PACK_CMYK_HINT_EXT = 0x800E
        PackCmykHintExt = 32782,
        //
        // Summary:
        //     Original was GL_UNPACK_CMYK_HINT_EXT = 0x800F
        UnpackCmykHintExt = 32783,
        //
        // Summary:
        //     Original was GL_PHONG_HINT_WIN = 0x80EB
        PhongHintWin = 33003,
        //
        // Summary:
        //     Original was GL_CLIP_VOLUME_CLIPPING_HINT_EXT = 0x80F0
        ClipVolumeClippingHintExt = 33008,
        //
        // Summary:
        //     Original was GL_TEXTURE_MULTI_BUFFER_HINT_SGIX = 0x812E
        TextureMultiBufferHintSgix = 33070,
        //
        // Summary:
        //     Original was GL_GENERATE_MIPMAP_HINT = 0x8192
        GenerateMipmapHint = 33170,
        //
        // Summary:
        //     Original was GL_GENERATE_MIPMAP_HINT_SGIS = 0x8192
        GenerateMipmapHintSgis = 33170,
        //
        // Summary:
        //     Original was GL_PROGRAM_BINARY_RETRIEVABLE_HINT = 0x8257
        ProgramBinaryRetrievableHint = 33367,
        //
        // Summary:
        //     Original was GL_CONVOLUTION_HINT_SGIX = 0x8316
        ConvolutionHintSgix = 33558,
        //
        // Summary:
        //     Original was GL_SCALEBIAS_HINT_SGIX = 0x8322
        ScalebiasHintSgix = 33570,
        //
        // Summary:
        //     Original was GL_LINE_QUALITY_HINT_SGIX = 0x835B
        LineQualityHintSgix = 33627,
        //
        // Summary:
        //     Original was GL_VERTEX_PRECLIP_SGIX = 0x83EE
        VertexPreclipSgix = 33774,
        //
        // Summary:
        //     Original was GL_VERTEX_PRECLIP_HINT_SGIX = 0x83EF
        VertexPreclipHintSgix = 33775,
        //
        // Summary:
        //     Original was GL_TEXTURE_COMPRESSION_HINT = 0x84EF
        TextureCompressionHint = 34031,
        //
        // Summary:
        //     Original was GL_TEXTURE_COMPRESSION_HINT_ARB = 0x84EF
        TextureCompressionHintArb = 34031,
        //
        // Summary:
        //     Original was GL_VERTEX_ARRAY_STORAGE_HINT_APPLE = 0x851F
        VertexArrayStorageHintApple = 34079,
        //
        // Summary:
        //     Original was GL_MULTISAMPLE_FILTER_HINT_NV = 0x8534
        MultisampleFilterHintNv = 34100,
        //
        // Summary:
        //     Original was GL_TRANSFORM_HINT_APPLE = 0x85B1
        TransformHintApple = 34225,
        //
        // Summary:
        //     Original was GL_TEXTURE_STORAGE_HINT_APPLE = 0x85BC
        TextureStorageHintApple = 34236,
        //
        // Summary:
        //     Original was GL_FRAGMENT_SHADER_DERIVATIVE_HINT = 0x8B8B
        FragmentShaderDerivativeHint = 35723,
        //
        // Summary:
        //     Original was GL_FRAGMENT_SHADER_DERIVATIVE_HINT_ARB = 0x8B8B
        FragmentShaderDerivativeHintArb = 35723,
        //
        // Summary:
        //     Original was GL_FRAGMENT_SHADER_DERIVATIVE_HINT_OES = 0x8B8B
        FragmentShaderDerivativeHintOes = 35723,
        //
        // Summary:
        //     Original was GL_BINNING_CONTROL_HINT_QCOM = 0x8FB0
        BinningControlHintQcom = 36784,
        //
        // Summary:
        //     Original was GL_PREFER_DOUBLEBUFFER_HINT_PGI = 0x1A1F8
        PreferDoublebufferHintPgi = 107000,
        //
        // Summary:
        //     Original was GL_CONSERVE_MEMORY_HINT_PGI = 0x1A1FD
        ConserveMemoryHintPgi = 107005,
        //
        // Summary:
        //     Original was GL_RECLAIM_MEMORY_HINT_PGI = 0x1A1FE
        ReclaimMemoryHintPgi = 107006,
        //
        // Summary:
        //     Original was GL_NATIVE_GRAPHICS_BEGIN_HINT_PGI = 0x1A203
        NativeGraphicsBeginHintPgi = 107011,
        //
        // Summary:
        //     Original was GL_NATIVE_GRAPHICS_END_HINT_PGI = 0x1A204
        NativeGraphicsEndHintPgi = 107012,
        //
        // Summary:
        //     Original was GL_ALWAYS_FAST_HINT_PGI = 0x1A20C
        AlwaysFastHintPgi = 107020,
        //
        // Summary:
        //     Original was GL_ALWAYS_SOFT_HINT_PGI = 0x1A20D
        AlwaysSoftHintPgi = 107021,
        //
        // Summary:
        //     Original was GL_ALLOW_DRAW_OBJ_HINT_PGI = 0x1A20E
        AllowDrawObjHintPgi = 107022,
        //
        // Summary:
        //     Original was GL_ALLOW_DRAW_WIN_HINT_PGI = 0x1A20F
        AllowDrawWinHintPgi = 107023,
        //
        // Summary:
        //     Original was GL_ALLOW_DRAW_FRG_HINT_PGI = 0x1A210
        AllowDrawFrgHintPgi = 107024,
        //
        // Summary:
        //     Original was GL_ALLOW_DRAW_MEM_HINT_PGI = 0x1A211
        AllowDrawMemHintPgi = 107025,
        //
        // Summary:
        //     Original was GL_STRICT_DEPTHFUNC_HINT_PGI = 0x1A216
        StrictDepthfuncHintPgi = 107030,
        //
        // Summary:
        //     Original was GL_STRICT_LIGHTING_HINT_PGI = 0x1A217
        StrictLightingHintPgi = 107031,
        //
        // Summary:
        //     Original was GL_STRICT_SCISSOR_HINT_PGI = 0x1A218
        StrictScissorHintPgi = 107032,
        //
        // Summary:
        //     Original was GL_FULL_STIPPLE_HINT_PGI = 0x1A219
        FullStippleHintPgi = 107033,
        //
        // Summary:
        //     Original was GL_CLIP_NEAR_HINT_PGI = 0x1A220
        ClipNearHintPgi = 107040,
        //
        // Summary:
        //     Original was GL_CLIP_FAR_HINT_PGI = 0x1A221
        ClipFarHintPgi = 107041,
        //
        // Summary:
        //     Original was GL_WIDE_LINE_HINT_PGI = 0x1A222
        WideLineHintPgi = 107042,
        //
        // Summary:
        //     Original was GL_BACK_NORMALS_HINT_PGI = 0x1A223
        BackNormalsHintPgi = 107043,
        //
        // Summary:
        //     Original was GL_VERTEX_DATA_HINT_PGI = 0x1A22A
        VertexDataHintPgi = 107050,
        //
        // Summary:
        //     Original was GL_VERTEX_CONSISTENT_HINT_PGI = 0x1A22B
        VertexConsistentHintPgi = 107051,
        //
        // Summary:
        //     Original was GL_MATERIAL_SIDE_HINT_PGI = 0x1A22C
        MaterialSideHintPgi = 107052,
        //
        // Summary:
        //     Original was GL_MAX_VERTEX_HINT_PGI = 0x1A22D
        MaxVertexHintPgi = 107053
    }

    public enum HintMode : uint
    {
        //
        // Summary:
        //     Original was GL_DONT_CARE = 0x1100
        DontCare = 4352,
        //
        // Summary:
        //     Original was GL_FASTEST = 0x1101
        Fastest = 4353,
        //
        // Summary:
        //     Original was GL_NICEST = 0x1102
        Nicest = 4354
    }

    public enum CullFaceMode : uint
    {
        //
        // Summary:
        //     Original was GL_FRONT = 0x0404
        Front = 1028,
        //
        // Summary:
        //     Original was GL_BACK = 0x0405
        Back = 1029,
        //
        // Summary:
        //     Original was GL_FRONT_AND_BACK = 0x0408
        FrontAndBack = 1032
    }

    public enum ClearBufferMask
    {
        //
        // Summary:
        //     Original was GL_NONE = 0
        None = 0,
        //
        // Summary:
        //     Original was GL_DEPTH_BUFFER_BIT = 0x00000100
        DepthBufferBit = 256,
        //
        // Summary:
        //     Original was GL_ACCUM_BUFFER_BIT = 0x00000200
        AccumBufferBit = 512,
        //
        // Summary:
        //     Original was GL_STENCIL_BUFFER_BIT = 0x00000400
        StencilBufferBit = 1024,
        //
        // Summary:
        //     Original was GL_COLOR_BUFFER_BIT = 0x00004000
        ColorBufferBit = 16384,
        //
        // Summary:
        //     Original was GL_COVERAGE_BUFFER_BIT_NV = 0x00008000
        CoverageBufferBitNv = 32768
    }

    public enum BlendingFactor : uint
    {
        //
        // Summary:
        //     Original was GL_ZERO = 0
        Zero = 0,
        //
        // Summary:
        //     Original was GL_ONE = 1
        One = 1,
        //
        // Summary:
        //     Original was GL_SRC_COLOR = 0x0300
        SrcColor = 768,
        //
        // Summary:
        //     Original was GL_ONE_MINUS_SRC_COLOR = 0x0301
        OneMinusSrcColor = 769,
        //
        // Summary:
        //     Original was GL_SRC_ALPHA = 0x0302
        SrcAlpha = 770,
        //
        // Summary:
        //     Original was GL_ONE_MINUS_SRC_ALPHA = 0x0303
        OneMinusSrcAlpha = 771,
        //
        // Summary:
        //     Original was GL_DST_ALPHA = 0x0304
        DstAlpha = 772,
        //
        // Summary:
        //     Original was GL_ONE_MINUS_DST_ALPHA = 0x0305
        OneMinusDstAlpha = 773,
        //
        // Summary:
        //     Original was GL_DST_COLOR = 0x0306
        DstColor = 774,
        //
        // Summary:
        //     Original was GL_ONE_MINUS_DST_COLOR = 0x0307
        OneMinusDstColor = 775,
        //
        // Summary:
        //     Original was GL_SRC_ALPHA_SATURATE = 0x0308
        SrcAlphaSaturate = 776,
        //
        // Summary:
        //     Original was GL_CONSTANT_COLOR = 0x8001
        ConstantColor = 32769,
        //
        // Summary:
        //     Original was GL_ONE_MINUS_CONSTANT_COLOR = 0x8002
        OneMinusConstantColor = 32770,
        //
        // Summary:
        //     Original was GL_CONSTANT_ALPHA = 0x8003
        ConstantAlpha = 32771,
        //
        // Summary:
        //     Original was GL_ONE_MINUS_CONSTANT_ALPHA = 0x8004
        OneMinusConstantAlpha = 32772,
        //
        // Summary:
        //     Original was GL_SRC1_ALPHA = 0x8589
        Src1Alpha = 34185,
        //
        // Summary:
        //     Original was GL_SRC1_COLOR = 0x88F9
        Src1Color = 35065
    }

    //
    // Summary:
    //     Used in GL.Amd.MultiDrawArraysIndirect, GL.Amd.MultiDrawElementsIndirect and
    //     51 other functions
    public enum PrimitiveType
    {
        //
        // Summary:
        //     Original was GL_POINTS = 0x0000
        Points = 0,
        //
        // Summary:
        //     Original was GL_LINES = 0x0001
        Lines = 1,
        //
        // Summary:
        //     Original was GL_LINE_LOOP = 0x0002
        LineLoop = 2,
        //
        // Summary:
        //     Original was GL_LINE_STRIP = 0x0003
        LineStrip = 3,
        //
        // Summary:
        //     Original was GL_TRIANGLES = 0x0004
        Triangles = 4,
        //
        // Summary:
        //     Original was GL_TRIANGLE_STRIP = 0x0005
        TriangleStrip = 5,
        //
        // Summary:
        //     Original was GL_TRIANGLE_FAN = 0x0006
        TriangleFan = 6,
        //
        // Summary:
        //     Original was GL_QUADS = 0x0007
        Quads = 7,
        //
        // Summary:
        //     Original was GL_QUADS_EXT = 0x0007
        QuadsExt = 7,
        //
        // Summary:
        //     Original was GL_QUAD_STRIP = 0x0008
        QuadStrip = 8,
        //
        // Summary:
        //     Original was GL_POLYGON = 0x0009
        Polygon = 9,
        //
        // Summary:
        //     Original was GL_LINES_ADJACENCY = 0x000A
        LinesAdjacency = 10,
        //
        // Summary:
        //     Original was GL_LINES_ADJACENCY_ARB = 0x000A
        LinesAdjacencyArb = 10,
        //
        // Summary:
        //     Original was GL_LINES_ADJACENCY_EXT = 0x000A
        LinesAdjacencyExt = 10,
        //
        // Summary:
        //     Original was GL_LINE_STRIP_ADJACENCY = 0x000B
        LineStripAdjacency = 11,
        //
        // Summary:
        //     Original was GL_LINE_STRIP_ADJACENCY_ARB = 0x000B
        LineStripAdjacencyArb = 11,
        //
        // Summary:
        //     Original was GL_LINE_STRIP_ADJACENCY_EXT = 0x000B
        LineStripAdjacencyExt = 11,
        //
        // Summary:
        //     Original was GL_TRIANGLES_ADJACENCY = 0x000C
        TrianglesAdjacency = 12,
        //
        // Summary:
        //     Original was GL_TRIANGLES_ADJACENCY_ARB = 0x000C
        TrianglesAdjacencyArb = 12,
        //
        // Summary:
        //     Original was GL_TRIANGLES_ADJACENCY_EXT = 0x000C
        TrianglesAdjacencyExt = 12,
        //
        // Summary:
        //     Original was GL_TRIANGLE_STRIP_ADJACENCY = 0x000D
        TriangleStripAdjacency = 13,
        //
        // Summary:
        //     Original was GL_TRIANGLE_STRIP_ADJACENCY_ARB = 0x000D
        TriangleStripAdjacencyArb = 13,
        //
        // Summary:
        //     Original was GL_TRIANGLE_STRIP_ADJACENCY_EXT = 0x000D
        TriangleStripAdjacencyExt = 13,
        //
        // Summary:
        //     Original was GL_PATCHES = 0x000E
        Patches = 14,
        //
        // Summary:
        //     Original was GL_PATCHES_EXT = 0x000E
        PatchesExt = 14
    }



    public enum EnableCap : uint
    {
        //
        // Summary:
        //     Original was GL_POINT_SMOOTH = 0x0B10
        PointSmooth = 2832,
        //
        // Summary:
        //     Original was GL_LINE_SMOOTH = 0x0B20
        LineSmooth = 2848,
        //
        // Summary:
        //     Original was GL_LINE_STIPPLE = 0x0B24
        LineStipple = 2852,
        //
        // Summary:
        //     Original was GL_POLYGON_SMOOTH = 0x0B41
        PolygonSmooth = 2881,
        //
        // Summary:
        //     Original was GL_POLYGON_STIPPLE = 0x0B42
        PolygonStipple = 2882,
        //
        // Summary:
        //     Original was GL_CULL_FACE = 0x0B44
        CullFace = 2884,
        //
        // Summary:
        //     Original was GL_LIGHTING = 0x0B50
        Lighting = 2896,
        //
        // Summary:
        //     Original was GL_COLOR_MATERIAL = 0x0B57
        ColorMaterial = 2903,
        //
        // Summary:
        //     Original was GL_FOG = 0x0B60
        Fog = 2912,
        //
        // Summary:
        //     Original was GL_DEPTH_TEST = 0x0B71
        DepthTest = 2929,
        //
        // Summary:
        //     Original was GL_STENCIL_TEST = 0x0B90
        StencilTest = 2960,
        //
        // Summary:
        //     Original was GL_NORMALIZE = 0x0BA1
        Normalize = 2977,
        //
        // Summary:
        //     Original was GL_ALPHA_TEST = 0x0BC0
        AlphaTest = 3008,
        //
        // Summary:
        //     Original was GL_DITHER = 0x0BD0
        Dither = 3024,
        //
        // Summary:
        //     Original was GL_BLEND = 0x0BE2
        Blend = 3042,
        //
        // Summary:
        //     Original was GL_INDEX_LOGIC_OP = 0x0BF1
        IndexLogicOp = 3057,
        //
        // Summary:
        //     Original was GL_COLOR_LOGIC_OP = 0x0BF2
        ColorLogicOp = 3058,
        //
        // Summary:
        //     Original was GL_SCISSOR_TEST = 0x0C11
        ScissorTest = 3089,
        //
        // Summary:
        //     Original was GL_TEXTURE_GEN_S = 0x0C60
        TextureGenS = 3168,
        //
        // Summary:
        //     Original was GL_TEXTURE_GEN_T = 0x0C61
        TextureGenT = 3169,
        //
        // Summary:
        //     Original was GL_TEXTURE_GEN_R = 0x0C62
        TextureGenR = 3170,
        //
        // Summary:
        //     Original was GL_TEXTURE_GEN_Q = 0x0C63
        TextureGenQ = 3171,
        //
        // Summary:
        //     Original was GL_AUTO_NORMAL = 0x0D80
        AutoNormal = 3456,
        //
        // Summary:
        //     Original was GL_MAP1_COLOR_4 = 0x0D90
        Map1Color4 = 3472,
        //
        // Summary:
        //     Original was GL_MAP1_INDEX = 0x0D91
        Map1Index = 3473,
        //
        // Summary:
        //     Original was GL_MAP1_NORMAL = 0x0D92
        Map1Normal = 3474,
        //
        // Summary:
        //     Original was GL_MAP1_TEXTURE_COORD_1 = 0x0D93
        Map1TextureCoord1 = 3475,
        //
        // Summary:
        //     Original was GL_MAP1_TEXTURE_COORD_2 = 0x0D94
        Map1TextureCoord2 = 3476,
        //
        // Summary:
        //     Original was GL_MAP1_TEXTURE_COORD_3 = 0x0D95
        Map1TextureCoord3 = 3477,
        //
        // Summary:
        //     Original was GL_MAP1_TEXTURE_COORD_4 = 0x0D96
        Map1TextureCoord4 = 3478,
        //
        // Summary:
        //     Original was GL_MAP1_VERTEX_3 = 0x0D97
        Map1Vertex3 = 3479,
        //
        // Summary:
        //     Original was GL_MAP1_VERTEX_4 = 0x0D98
        Map1Vertex4 = 3480,
        //
        // Summary:
        //     Original was GL_MAP2_COLOR_4 = 0x0DB0
        Map2Color4 = 3504,
        //
        // Summary:
        //     Original was GL_MAP2_INDEX = 0x0DB1
        Map2Index = 3505,
        //
        // Summary:
        //     Original was GL_MAP2_NORMAL = 0x0DB2
        Map2Normal = 3506,
        //
        // Summary:
        //     Original was GL_MAP2_TEXTURE_COORD_1 = 0x0DB3
        Map2TextureCoord1 = 3507,
        //
        // Summary:
        //     Original was GL_MAP2_TEXTURE_COORD_2 = 0x0DB4
        Map2TextureCoord2 = 3508,
        //
        // Summary:
        //     Original was GL_MAP2_TEXTURE_COORD_3 = 0x0DB5
        Map2TextureCoord3 = 3509,
        //
        // Summary:
        //     Original was GL_MAP2_TEXTURE_COORD_4 = 0x0DB6
        Map2TextureCoord4 = 3510,
        //
        // Summary:
        //     Original was GL_MAP2_VERTEX_3 = 0x0DB7
        Map2Vertex3 = 3511,
        //
        // Summary:
        //     Original was GL_MAP2_VERTEX_4 = 0x0DB8
        Map2Vertex4 = 3512,
        //
        // Summary:
        //     Original was GL_TEXTURE_1D = 0x0DE0
        Texture1D = 3552,
        //
        // Summary:
        //     Original was GL_TEXTURE_2D = 0x0DE1
        Texture2D = 3553,
        //
        // Summary:
        //     Original was GL_POLYGON_OFFSET_POINT = 0x2A01
        PolygonOffsetPoint = 10753,
        //
        // Summary:
        //     Original was GL_POLYGON_OFFSET_LINE = 0x2A02
        PolygonOffsetLine = 10754,
        //
        // Summary:
        //     Original was GL_CLIP_DISTANCE0 = 0x3000
        ClipDistance0 = 12288,
        //
        // Summary:
        //     Original was GL_CLIP_PLANE0 = 0x3000
        ClipPlane0 = 12288,
        //
        // Summary:
        //     Original was GL_CLIP_DISTANCE1 = 0x3001
        ClipDistance1 = 12289,
        //
        // Summary:
        //     Original was GL_CLIP_PLANE1 = 0x3001
        ClipPlane1 = 12289,
        //
        // Summary:
        //     Original was GL_CLIP_DISTANCE2 = 0x3002
        ClipDistance2 = 12290,
        //
        // Summary:
        //     Original was GL_CLIP_PLANE2 = 0x3002
        ClipPlane2 = 12290,
        //
        // Summary:
        //     Original was GL_CLIP_DISTANCE3 = 0x3003
        ClipDistance3 = 12291,
        //
        // Summary:
        //     Original was GL_CLIP_PLANE3 = 0x3003
        ClipPlane3 = 12291,
        //
        // Summary:
        //     Original was GL_CLIP_DISTANCE4 = 0x3004
        ClipDistance4 = 12292,
        //
        // Summary:
        //     Original was GL_CLIP_PLANE4 = 0x3004
        ClipPlane4 = 12292,
        //
        // Summary:
        //     Original was GL_CLIP_DISTANCE5 = 0x3005
        ClipDistance5 = 12293,
        //
        // Summary:
        //     Original was GL_CLIP_PLANE5 = 0x3005
        ClipPlane5 = 12293,
        //
        // Summary:
        //     Original was GL_CLIP_DISTANCE6 = 0x3006
        ClipDistance6 = 12294,
        //
        // Summary:
        //     Original was GL_CLIP_DISTANCE7 = 0x3007
        ClipDistance7 = 12295,
        //
        // Summary:
        //     Original was GL_LIGHT0 = 0x4000
        Light0 = 16384,
        //
        // Summary:
        //     Original was GL_LIGHT1 = 0x4001
        Light1 = 16385,
        //
        // Summary:
        //     Original was GL_LIGHT2 = 0x4002
        Light2 = 16386,
        //
        // Summary:
        //     Original was GL_LIGHT3 = 0x4003
        Light3 = 16387,
        //
        // Summary:
        //     Original was GL_LIGHT4 = 0x4004
        Light4 = 16388,
        //
        // Summary:
        //     Original was GL_LIGHT5 = 0x4005
        Light5 = 16389,
        //
        // Summary:
        //     Original was GL_LIGHT6 = 0x4006
        Light6 = 16390,
        //
        // Summary:
        //     Original was GL_LIGHT7 = 0x4007
        Light7 = 16391,
        //
        // Summary:
        //     Original was GL_CONVOLUTION_1D = 0x8010
        Convolution1D = 32784,
        //
        // Summary:
        //     Original was GL_CONVOLUTION_1D_EXT = 0x8010
        Convolution1DExt = 32784,
        //
        // Summary:
        //     Original was GL_CONVOLUTION_2D = 0x8011
        Convolution2D = 32785,
        //
        // Summary:
        //     Original was GL_CONVOLUTION_2D_EXT = 0x8011
        Convolution2DExt = 32785,
        //
        // Summary:
        //     Original was GL_SEPARABLE_2D = 0x8012
        Separable2D = 32786,
        //
        // Summary:
        //     Original was GL_SEPARABLE_2D_EXT = 0x8012
        Separable2DExt = 32786,
        //
        // Summary:
        //     Original was GL_HISTOGRAM = 0x8024
        Histogram = 32804,
        //
        // Summary:
        //     Original was GL_HISTOGRAM_EXT = 0x8024
        HistogramExt = 32804,
        //
        // Summary:
        //     Original was GL_MINMAX_EXT = 0x802E
        MinmaxExt = 32814,
        //
        // Summary:
        //     Original was GL_POLYGON_OFFSET_FILL = 0x8037
        PolygonOffsetFill = 32823,
        //
        // Summary:
        //     Original was GL_RESCALE_NORMAL = 0x803A
        RescaleNormal = 32826,
        //
        // Summary:
        //     Original was GL_RESCALE_NORMAL_EXT = 0x803A
        RescaleNormalExt = 32826,
        //
        // Summary:
        //     Original was GL_TEXTURE_3D_EXT = 0x806F
        Texture3DExt = 32879,
        //
        // Summary:
        //     Original was GL_VERTEX_ARRAY = 0x8074
        VertexArray = 32884,
        //
        // Summary:
        //     Original was GL_NORMAL_ARRAY = 0x8075
        NormalArray = 32885,
        //
        // Summary:
        //     Original was GL_COLOR_ARRAY = 0x8076
        ColorArray = 32886,
        //
        // Summary:
        //     Original was GL_INDEX_ARRAY = 0x8077
        IndexArray = 32887,
        //
        // Summary:
        //     Original was GL_TEXTURE_COORD_ARRAY = 0x8078
        TextureCoordArray = 32888,
        //
        // Summary:
        //     Original was GL_EDGE_FLAG_ARRAY = 0x8079
        EdgeFlagArray = 32889,
        //
        // Summary:
        //     Original was GL_INTERLACE_SGIX = 0x8094
        InterlaceSgix = 32916,
        //
        // Summary:
        //     Original was GL_MULTISAMPLE = 0x809D
        Multisample = 32925,
        //
        // Summary:
        //     Original was GL_MULTISAMPLE_SGIS = 0x809D
        MultisampleSgis = 32925,
        //
        // Summary:
        //     Original was GL_SAMPLE_ALPHA_TO_COVERAGE = 0x809E
        SampleAlphaToCoverage = 32926,
        //
        // Summary:
        //     Original was GL_SAMPLE_ALPHA_TO_MASK_SGIS = 0x809E
        SampleAlphaToMaskSgis = 32926,
        //
        // Summary:
        //     Original was GL_SAMPLE_ALPHA_TO_ONE = 0x809F
        SampleAlphaToOne = 32927,
        //
        // Summary:
        //     Original was GL_SAMPLE_ALPHA_TO_ONE_SGIS = 0x809F
        SampleAlphaToOneSgis = 32927,
        //
        // Summary:
        //     Original was GL_SAMPLE_COVERAGE = 0x80A0
        SampleCoverage = 32928,
        //
        // Summary:
        //     Original was GL_SAMPLE_MASK_SGIS = 0x80A0
        SampleMaskSgis = 32928,
        //
        // Summary:
        //     Original was GL_TEXTURE_COLOR_TABLE_SGI = 0x80BC
        TextureColorTableSgi = 32956,
        //
        // Summary:
        //     Original was GL_COLOR_TABLE = 0x80D0
        ColorTable = 32976,
        //
        // Summary:
        //     Original was GL_COLOR_TABLE_SGI = 0x80D0
        ColorTableSgi = 32976,
        //
        // Summary:
        //     Original was GL_POST_CONVOLUTION_COLOR_TABLE = 0x80D1
        PostConvolutionColorTable = 32977,
        //
        // Summary:
        //     Original was GL_POST_CONVOLUTION_COLOR_TABLE_SGI = 0x80D1
        PostConvolutionColorTableSgi = 32977,
        //
        // Summary:
        //     Original was GL_POST_COLOR_MATRIX_COLOR_TABLE = 0x80D2
        PostColorMatrixColorTable = 32978,
        //
        // Summary:
        //     Original was GL_POST_COLOR_MATRIX_COLOR_TABLE_SGI = 0x80D2
        PostColorMatrixColorTableSgi = 32978,
        //
        // Summary:
        //     Original was GL_TEXTURE_4D_SGIS = 0x8134
        Texture4DSgis = 33076,
        //
        // Summary:
        //     Original was GL_PIXEL_TEX_GEN_SGIX = 0x8139
        PixelTexGenSgix = 33081,
        //
        // Summary:
        //     Original was GL_SPRITE_SGIX = 0x8148
        SpriteSgix = 33096,
        //
        // Summary:
        //     Original was GL_REFERENCE_PLANE_SGIX = 0x817D
        ReferencePlaneSgix = 33149,
        //
        // Summary:
        //     Original was GL_IR_INSTRUMENT1_SGIX = 0x817F
        IrInstrument1Sgix = 33151,
        //
        // Summary:
        //     Original was GL_CALLIGRAPHIC_FRAGMENT_SGIX = 0x8183
        CalligraphicFragmentSgix = 33155,
        //
        // Summary:
        //     Original was GL_FRAMEZOOM_SGIX = 0x818B
        FramezoomSgix = 33163,
        //
        // Summary:
        //     Original was GL_FOG_OFFSET_SGIX = 0x8198
        FogOffsetSgix = 33176,
        //
        // Summary:
        //     Original was GL_SHARED_TEXTURE_PALETTE_EXT = 0x81FB
        SharedTexturePaletteExt = 33275,
        //
        // Summary:
        //     Original was GL_DEBUG_OUTPUT_SYNCHRONOUS = 0x8242
        DebugOutputSynchronous = 33346,
        //
        // Summary:
        //     Original was GL_ASYNC_HISTOGRAM_SGIX = 0x832C
        AsyncHistogramSgix = 33580,
        //
        // Summary:
        //     Original was GL_PIXEL_TEXTURE_SGIS = 0x8353
        PixelTextureSgis = 33619,
        //
        // Summary:
        //     Original was GL_ASYNC_TEX_IMAGE_SGIX = 0x835C
        AsyncTexImageSgix = 33628,
        //
        // Summary:
        //     Original was GL_ASYNC_DRAW_PIXELS_SGIX = 0x835D
        AsyncDrawPixelsSgix = 33629,
        //
        // Summary:
        //     Original was GL_ASYNC_READ_PIXELS_SGIX = 0x835E
        AsyncReadPixelsSgix = 33630,
        //
        // Summary:
        //     Original was GL_FRAGMENT_LIGHTING_SGIX = 0x8400
        FragmentLightingSgix = 33792,
        //
        // Summary:
        //     Original was GL_FRAGMENT_COLOR_MATERIAL_SGIX = 0x8401
        FragmentColorMaterialSgix = 33793,
        //
        // Summary:
        //     Original was GL_FRAGMENT_LIGHT0_SGIX = 0x840C
        FragmentLight0Sgix = 33804,
        //
        // Summary:
        //     Original was GL_FRAGMENT_LIGHT1_SGIX = 0x840D
        FragmentLight1Sgix = 33805,
        //
        // Summary:
        //     Original was GL_FRAGMENT_LIGHT2_SGIX = 0x840E
        FragmentLight2Sgix = 33806,
        //
        // Summary:
        //     Original was GL_FRAGMENT_LIGHT3_SGIX = 0x840F
        FragmentLight3Sgix = 33807,
        //
        // Summary:
        //     Original was GL_FRAGMENT_LIGHT4_SGIX = 0x8410
        FragmentLight4Sgix = 33808,
        //
        // Summary:
        //     Original was GL_FRAGMENT_LIGHT5_SGIX = 0x8411
        FragmentLight5Sgix = 33809,
        //
        // Summary:
        //     Original was GL_FRAGMENT_LIGHT6_SGIX = 0x8412
        FragmentLight6Sgix = 33810,
        //
        // Summary:
        //     Original was GL_FRAGMENT_LIGHT7_SGIX = 0x8413
        FragmentLight7Sgix = 33811,
        //
        // Summary:
        //     Original was GL_FOG_COORD_ARRAY = 0x8457
        FogCoordArray = 33879,
        //
        // Summary:
        //     Original was GL_COLOR_SUM = 0x8458
        ColorSum = 33880,
        //
        // Summary:
        //     Original was GL_SECONDARY_COLOR_ARRAY = 0x845E
        SecondaryColorArray = 33886,
        //
        // Summary:
        //     Original was GL_TEXTURE_RECTANGLE = 0x84F5
        TextureRectangle = 34037,
        //
        // Summary:
        //     Original was GL_TEXTURE_CUBE_MAP = 0x8513
        TextureCubeMap = 34067,
        //
        // Summary:
        //     Original was GL_PROGRAM_POINT_SIZE = 0x8642
        ProgramPointSize = 34370,
        //
        // Summary:
        //     Original was GL_VERTEX_PROGRAM_POINT_SIZE = 0x8642
        VertexProgramPointSize = 34370,
        //
        // Summary:
        //     Original was GL_VERTEX_PROGRAM_TWO_SIDE = 0x8643
        VertexProgramTwoSide = 34371,
        //
        // Summary:
        //     Original was GL_DEPTH_CLAMP = 0x864F
        DepthClamp = 34383,
        //
        // Summary:
        //     Original was GL_TEXTURE_CUBE_MAP_SEAMLESS = 0x884F
        TextureCubeMapSeamless = 34895,
        //
        // Summary:
        //     Original was GL_POINT_SPRITE = 0x8861
        PointSprite = 34913,
        //
        // Summary:
        //     Original was GL_SAMPLE_SHADING = 0x8C36
        SampleShading = 35894,
        //
        // Summary:
        //     Original was GL_RASTERIZER_DISCARD = 0x8C89
        RasterizerDiscard = 35977,
        //
        // Summary:
        //     Original was GL_PRIMITIVE_RESTART_FIXED_INDEX = 0x8D69
        PrimitiveRestartFixedIndex = 36201,
        //
        // Summary:
        //     Original was GL_FRAMEBUFFER_SRGB = 0x8DB9
        FramebufferSrgb = 36281,
        //
        // Summary:
        //     Original was GL_SAMPLE_MASK = 0x8E51
        SampleMask = 36433,
        //
        // Summary:
        //     Original was GL_PRIMITIVE_RESTART = 0x8F9D
        PrimitiveRestart = 36765,
        //
        // Summary:
        //     Original was GL_DEBUG_OUTPUT = 0x92E0
        DebugOutput = 37600
    }

    public enum TextureParameterName
    {
        //
        // Summary:
        //     Original was GL_TEXTURE_WIDTH = 0x1000
        TextureWidth = 4096,
        //
        // Summary:
        //     Original was GL_TEXTURE_HEIGHT = 0x1001
        TextureHeight = 4097,
        //
        // Summary:
        //     Original was GL_TEXTURE_COMPONENTS = 0x1003
        TextureComponents = 4099,
        //
        // Summary:
        //     Original was GL_TEXTURE_INTERNAL_FORMAT = 0x1003
        TextureInternalFormat = 4099,
        //
        // Summary:
        //     Original was GL_TEXTURE_BORDER_COLOR = 0x1004
        TextureBorderColor = 4100,
        //
        // Summary:
        //     Original was GL_TEXTURE_BORDER_COLOR_NV = 0x1004
        TextureBorderColorNv = 4100,
        //
        // Summary:
        //     Original was GL_TEXTURE_BORDER = 0x1005
        TextureBorder = 4101,
        //
        // Summary:
        //     Original was GL_TEXTURE_MAG_FILTER = 0x2800
        TextureMagFilter = 10240,
        //
        // Summary:
        //     Original was GL_TEXTURE_MIN_FILTER = 0x2801
        TextureMinFilter = 10241,
        //
        // Summary:
        //     Original was GL_TEXTURE_WRAP_S = 0x2802
        TextureWrapS = 10242,
        //
        // Summary:
        //     Original was GL_TEXTURE_WRAP_T = 0x2803
        TextureWrapT = 10243,
        //
        // Summary:
        //     Original was GL_TEXTURE_RED_SIZE = 0x805C
        TextureRedSize = 32860,
        //
        // Summary:
        //     Original was GL_TEXTURE_GREEN_SIZE = 0x805D
        TextureGreenSize = 32861,
        //
        // Summary:
        //     Original was GL_TEXTURE_BLUE_SIZE = 0x805E
        TextureBlueSize = 32862,
        //
        // Summary:
        //     Original was GL_TEXTURE_ALPHA_SIZE = 0x805F
        TextureAlphaSize = 32863,
        //
        // Summary:
        //     Original was GL_TEXTURE_LUMINANCE_SIZE = 0x8060
        TextureLuminanceSize = 32864,
        //
        // Summary:
        //     Original was GL_TEXTURE_INTENSITY_SIZE = 0x8061
        TextureIntensitySize = 32865,
        //
        // Summary:
        //     Original was GL_TEXTURE_PRIORITY = 0x8066
        TexturePriority = 32870,
        //
        // Summary:
        //     Original was GL_TEXTURE_PRIORITY_EXT = 0x8066
        TexturePriorityExt = 32870,
        //
        // Summary:
        //     Original was GL_TEXTURE_RESIDENT = 0x8067
        TextureResident = 32871,
        //
        // Summary:
        //     Original was GL_TEXTURE_DEPTH = 0x8071
        TextureDepth = 32881,
        //
        // Summary:
        //     Original was GL_TEXTURE_DEPTH_EXT = 0x8071
        TextureDepthExt = 32881,
        //
        // Summary:
        //     Original was GL_TEXTURE_WRAP_R = 0x8072
        TextureWrapR = 32882,
        //
        // Summary:
        //     Original was GL_TEXTURE_WRAP_R_EXT = 0x8072
        TextureWrapRExt = 32882,
        //
        // Summary:
        //     Original was GL_TEXTURE_WRAP_R_OES = 0x8072
        TextureWrapROes = 32882,
        //
        // Summary:
        //     Original was GL_DETAIL_TEXTURE_LEVEL_SGIS = 0x809A
        DetailTextureLevelSgis = 32922,
        //
        // Summary:
        //     Original was GL_DETAIL_TEXTURE_MODE_SGIS = 0x809B
        DetailTextureModeSgis = 32923,
        //
        // Summary:
        //     Original was GL_DETAIL_TEXTURE_FUNC_POINTS_SGIS = 0x809C
        DetailTextureFuncPointsSgis = 32924,
        //
        // Summary:
        //     Original was GL_SHARPEN_TEXTURE_FUNC_POINTS_SGIS = 0x80B0
        SharpenTextureFuncPointsSgis = 32944,
        //
        // Summary:
        //     Original was GL_SHADOW_AMBIENT_SGIX = 0x80BF
        ShadowAmbientSgix = 32959,
        //
        // Summary:
        //     Original was GL_TEXTURE_COMPARE_FAIL_VALUE = 0x80BF
        TextureCompareFailValue = 32959,
        //
        // Summary:
        //     Original was GL_DUAL_TEXTURE_SELECT_SGIS = 0x8124
        DualTextureSelectSgis = 33060,
        //
        // Summary:
        //     Original was GL_QUAD_TEXTURE_SELECT_SGIS = 0x8125
        QuadTextureSelectSgis = 33061,
        //
        // Summary:
        //     Original was GL_CLAMP_TO_BORDER = 0x812D
        ClampToBorder = 33069,
        //
        // Summary:
        //     Original was GL_CLAMP_TO_EDGE = 0x812F
        ClampToEdge = 33071,
        //
        // Summary:
        //     Original was GL_TEXTURE_4DSIZE_SGIS = 0x8136
        Texture4DsizeSgis = 33078,
        //
        // Summary:
        //     Original was GL_TEXTURE_WRAP_Q_SGIS = 0x8137
        TextureWrapQSgis = 33079,
        //
        // Summary:
        //     Original was GL_TEXTURE_MIN_LOD = 0x813A
        TextureMinLod = 33082,
        //
        // Summary:
        //     Original was GL_TEXTURE_MIN_LOD_SGIS = 0x813A
        TextureMinLodSgis = 33082,
        //
        // Summary:
        //     Original was GL_TEXTURE_MAX_LOD = 0x813B
        TextureMaxLod = 33083,
        //
        // Summary:
        //     Original was GL_TEXTURE_MAX_LOD_SGIS = 0x813B
        TextureMaxLodSgis = 33083,
        //
        // Summary:
        //     Original was GL_TEXTURE_BASE_LEVEL = 0x813C
        TextureBaseLevel = 33084,
        //
        // Summary:
        //     Original was GL_TEXTURE_BASE_LEVEL_SGIS = 0x813C
        TextureBaseLevelSgis = 33084,
        //
        // Summary:
        //     Original was GL_TEXTURE_MAX_LEVEL = 0x813D
        TextureMaxLevel = 33085,
        //
        // Summary:
        //     Original was GL_TEXTURE_MAX_LEVEL_SGIS = 0x813D
        TextureMaxLevelSgis = 33085,
        //
        // Summary:
        //     Original was GL_TEXTURE_FILTER4_SIZE_SGIS = 0x8147
        TextureFilter4SizeSgis = 33095,
        //
        // Summary:
        //     Original was GL_TEXTURE_CLIPMAP_CENTER_SGIX = 0x8171
        TextureClipmapCenterSgix = 33137,
        //
        // Summary:
        //     Original was GL_TEXTURE_CLIPMAP_FRAME_SGIX = 0x8172
        TextureClipmapFrameSgix = 33138,
        //
        // Summary:
        //     Original was GL_TEXTURE_CLIPMAP_OFFSET_SGIX = 0x8173
        TextureClipmapOffsetSgix = 33139,
        //
        // Summary:
        //     Original was GL_TEXTURE_CLIPMAP_VIRTUAL_DEPTH_SGIX = 0x8174
        TextureClipmapVirtualDepthSgix = 33140,
        //
        // Summary:
        //     Original was GL_TEXTURE_CLIPMAP_LOD_OFFSET_SGIX = 0x8175
        TextureClipmapLodOffsetSgix = 33141,
        //
        // Summary:
        //     Original was GL_TEXTURE_CLIPMAP_DEPTH_SGIX = 0x8176
        TextureClipmapDepthSgix = 33142,
        //
        // Summary:
        //     Original was GL_POST_TEXTURE_FILTER_BIAS_SGIX = 0x8179
        PostTextureFilterBiasSgix = 33145,
        //
        // Summary:
        //     Original was GL_POST_TEXTURE_FILTER_SCALE_SGIX = 0x817A
        PostTextureFilterScaleSgix = 33146,
        //
        // Summary:
        //     Original was GL_TEXTURE_LOD_BIAS_S_SGIX = 0x818E
        TextureLodBiasSSgix = 33166,
        //
        // Summary:
        //     Original was GL_TEXTURE_LOD_BIAS_T_SGIX = 0x818F
        TextureLodBiasTSgix = 33167,
        //
        // Summary:
        //     Original was GL_TEXTURE_LOD_BIAS_R_SGIX = 0x8190
        TextureLodBiasRSgix = 33168,
        //
        // Summary:
        //     Original was GL_GENERATE_MIPMAP = 0x8191
        GenerateMipmap = 33169,
        //
        // Summary:
        //     Original was GL_GENERATE_MIPMAP_SGIS = 0x8191
        GenerateMipmapSgis = 33169,
        //
        // Summary:
        //     Original was GL_TEXTURE_COMPARE_SGIX = 0x819A
        TextureCompareSgix = 33178,
        //
        // Summary:
        //     Original was GL_TEXTURE_COMPARE_OPERATOR_SGIX = 0x819B
        TextureCompareOperatorSgix = 33179,
        //
        // Summary:
        //     Original was GL_TEXTURE_LEQUAL_R_SGIX = 0x819C
        TextureLequalRSgix = 33180,
        //
        // Summary:
        //     Original was GL_TEXTURE_GEQUAL_R_SGIX = 0x819D
        TextureGequalRSgix = 33181,
        //
        // Summary:
        //     Original was GL_TEXTURE_MAX_CLAMP_S_SGIX = 0x8369
        TextureMaxClampSSgix = 33641,
        //
        // Summary:
        //     Original was GL_TEXTURE_MAX_CLAMP_T_SGIX = 0x836A
        TextureMaxClampTSgix = 33642,
        //
        // Summary:
        //     Original was GL_TEXTURE_MAX_CLAMP_R_SGIX = 0x836B
        TextureMaxClampRSgix = 33643,
        //
        // Summary:
        //     Original was GL_TEXTURE_MAX_ANISOTROPY = 0x84FE
        TextureMaxAnisotropy = 34046,
        //
        // Summary:
        //     Original was GL_TEXTURE_LOD_BIAS = 0x8501
        TextureLodBias = 34049,
        //
        // Summary:
        //     Original was GL_DEPTH_TEXTURE_MODE = 0x884B
        DepthTextureMode = 34891,
        //
        // Summary:
        //     Original was GL_TEXTURE_COMPARE_MODE = 0x884C
        TextureCompareMode = 34892,
        //
        // Summary:
        //     Original was GL_TEXTURE_COMPARE_FUNC = 0x884D
        TextureCompareFunc = 34893,
        //
        // Summary:
        //     Original was GL_TEXTURE_SWIZZLE_R = 0x8E42
        TextureSwizzleR = 36418,
        //
        // Summary:
        //     Original was GL_TEXTURE_SWIZZLE_G = 0x8E43
        TextureSwizzleG = 36419,
        //
        // Summary:
        //     Original was GL_TEXTURE_SWIZZLE_B = 0x8E44
        TextureSwizzleB = 36420,
        //
        // Summary:
        //     Original was GL_TEXTURE_SWIZZLE_A = 0x8E45
        TextureSwizzleA = 36421,
        //
        // Summary:
        //     Original was GL_TEXTURE_SWIZZLE_RGBA = 0x8E46
        TextureSwizzleRgba = 36422,
        //
        // Summary:
        //     Original was GL_DEPTH_STENCIL_TEXTURE_MODE = 0x90EA
        DepthStencilTextureMode = 37098,
        //
        // Summary:
        //     Original was GL_TEXTURE_TILING_EXT = 0x9580
        TextureTilingExt = 38272
    }

    public enum TextureTarget : uint
    {
        //
        // Summary:
        //     Original was GL_TEXTURE_1D = 0x0DE0
        Texture1D = 3552,
        //
        // Summary:
        //     Original was GL_TEXTURE_2D = 0x0DE1
        Texture2D = 3553,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_1D = 0x8063
        ProxyTexture1D = 32867,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_1D_EXT = 0x8063
        ProxyTexture1DExt = 32867,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_2D = 0x8064
        ProxyTexture2D = 32868,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_2D_EXT = 0x8064
        ProxyTexture2DExt = 32868,
        //
        // Summary:
        //     Original was GL_TEXTURE_3D = 0x806F
        Texture3D = 32879,
        //
        // Summary:
        //     Original was GL_TEXTURE_3D_EXT = 0x806F
        Texture3DExt = 32879,
        //
        // Summary:
        //     Original was GL_TEXTURE_3D_OES = 0x806F
        Texture3DOes = 32879,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_3D = 0x8070
        ProxyTexture3D = 32880,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_3D_EXT = 0x8070
        ProxyTexture3DExt = 32880,
        //
        // Summary:
        //     Original was GL_DETAIL_TEXTURE_2D_SGIS = 0x8095
        DetailTexture2DSgis = 32917,
        //
        // Summary:
        //     Original was GL_TEXTURE_4D_SGIS = 0x8134
        Texture4DSgis = 33076,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_4D_SGIS = 0x8135
        ProxyTexture4DSgis = 33077,
        //
        // Summary:
        //     Original was GL_TEXTURE_RECTANGLE = 0x84F5
        TextureRectangle = 34037,
        //
        // Summary:
        //     Original was GL_TEXTURE_RECTANGLE_ARB = 0x84F5
        TextureRectangleArb = 34037,
        //
        // Summary:
        //     Original was GL_TEXTURE_RECTANGLE_NV = 0x84F5
        TextureRectangleNv = 34037,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_RECTANGLE = 0x84F7
        ProxyTextureRectangle = 34039,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_RECTANGLE_ARB = 0x84F7
        ProxyTextureRectangleArb = 34039,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_RECTANGLE_NV = 0x84F7
        ProxyTextureRectangleNv = 34039,
        //
        // Summary:
        //     Original was GL_TEXTURE_CUBE_MAP = 0x8513
        TextureCubeMap = 34067,
        //
        // Summary:
        //     Original was GL_TEXTURE_BINDING_CUBE_MAP = 0x8514
        TextureBindingCubeMap = 34068,
        //
        // Summary:
        //     Original was GL_TEXTURE_CUBE_MAP_POSITIVE_X = 0x8515
        TextureCubeMapPositiveX = 34069,
        //
        // Summary:
        //     Original was GL_TEXTURE_CUBE_MAP_NEGATIVE_X = 0x8516
        TextureCubeMapNegativeX = 34070,
        //
        // Summary:
        //     Original was GL_TEXTURE_CUBE_MAP_POSITIVE_Y = 0x8517
        TextureCubeMapPositiveY = 34071,
        //
        // Summary:
        //     Original was GL_TEXTURE_CUBE_MAP_NEGATIVE_Y = 0x8518
        TextureCubeMapNegativeY = 34072,
        //
        // Summary:
        //     Original was GL_TEXTURE_CUBE_MAP_POSITIVE_Z = 0x8519
        TextureCubeMapPositiveZ = 34073,
        //
        // Summary:
        //     Original was GL_TEXTURE_CUBE_MAP_NEGATIVE_Z = 0x851A
        TextureCubeMapNegativeZ = 34074,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_CUBE_MAP = 0x851B
        ProxyTextureCubeMap = 34075,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_CUBE_MAP_ARB = 0x851B
        ProxyTextureCubeMapArb = 34075,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_CUBE_MAP_EXT = 0x851B
        ProxyTextureCubeMapExt = 34075,
        //
        // Summary:
        //     Original was GL_TEXTURE_1D_ARRAY = 0x8C18
        Texture1DArray = 35864,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_1D_ARRAY = 0x8C19
        ProxyTexture1DArray = 35865,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_1D_ARRAY_EXT = 0x8C19
        ProxyTexture1DArrayExt = 35865,
        //
        // Summary:
        //     Original was GL_TEXTURE_2D_ARRAY = 0x8C1A
        Texture2DArray = 35866,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_2D_ARRAY = 0x8C1B
        ProxyTexture2DArray = 35867,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_2D_ARRAY_EXT = 0x8C1B
        ProxyTexture2DArrayExt = 35867,
        //
        // Summary:
        //     Original was GL_TEXTURE_BUFFER = 0x8C2A
        TextureBuffer = 35882,
        //
        // Summary:
        //     Original was GL_TEXTURE_CUBE_MAP_ARRAY = 0x9009
        TextureCubeMapArray = 36873,
        //
        // Summary:
        //     Original was GL_TEXTURE_CUBE_MAP_ARRAY_ARB = 0x9009
        TextureCubeMapArrayArb = 36873,
        //
        // Summary:
        //     Original was GL_TEXTURE_CUBE_MAP_ARRAY_EXT = 0x9009
        TextureCubeMapArrayExt = 36873,
        //
        // Summary:
        //     Original was GL_TEXTURE_CUBE_MAP_ARRAY_OES = 0x9009
        TextureCubeMapArrayOes = 36873,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_CUBE_MAP_ARRAY = 0x900B
        ProxyTextureCubeMapArray = 36875,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_CUBE_MAP_ARRAY_ARB = 0x900B
        ProxyTextureCubeMapArrayArb = 36875,
        //
        // Summary:
        //     Original was GL_TEXTURE_2D_MULTISAMPLE = 0x9100
        Texture2DMultisample = 37120,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_2D_MULTISAMPLE = 0x9101
        ProxyTexture2DMultisample = 37121,
        //
        // Summary:
        //     Original was GL_TEXTURE_2D_MULTISAMPLE_ARRAY = 0x9102
        Texture2DMultisampleArray = 37122,
        //
        // Summary:
        //     Original was GL_PROXY_TEXTURE_2D_MULTISAMPLE_ARRAY = 0x9103
        ProxyTexture2DMultisampleArray = 37123
    }

    #endregion Enums
}

