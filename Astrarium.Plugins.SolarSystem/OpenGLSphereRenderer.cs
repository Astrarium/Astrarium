using Astrarium.Types;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace Astrarium.Plugins.SolarSystem
{
    internal class OpenGLSphereRenderer : WpfSphereRenderer
    {
        private const string USER32 = "user32";
        private const string GDI32 = "gdi32";
        private const string OPENGL32 = "opengl32";
        private const string GLU32 = "glu32";
        private const string GLFW = "glfw";

        private const int GL_TRUE = 1;

        private const int GL_DEPTH_TEST = 0x0B71;
        private const int GL_LIGHTING = 0x0B50;
        private const int GL_LEQUAL = 0x0203;
        private const int GL_LINEAR = 0x2601;

        private const int GL_COMPILE = 0x1300;
        private const int GL_TEXTURE_MIN_FILTER = 0x2801;
        private const int GL_TEXTURE_MAG_FILTER = 0x2800;

        private const int GL_COLOR_ATTACHMENT0 = 0x8ce0;
        private const int GL_RENDERBUFFER = 0x8d41;
        private const int GL_FRAMEBUFFER = 0x8d40;
        private const int GL_COLOR_BUFFER_BIT = 0x4000;
        private const int GL_DEPTH_BUFFER_BIT = 0x100;

        private const int GL_PROJECTION = 0x1701;
        private const int GL_UNSIGNED_BYTE = 0x1401;
        private const int GL_TEXTURE_2D = 0xDE1;
        private const int GL_RGBA = 0x1908;

        private const int GLFW_CONTEXT_VERSION_MAJOR = 0x00022002;
        private const int GLFW_CONTEXT_VERSION_MINOR = 0x00022003;
        private const int GLFW_VISIBLE = 0x00020004;
        private const int GLFW_FALSE = 0;

        [StructLayout(LayoutKind.Sequential)]
        private struct PixelFormatDescriptor
        {
            public ushort Size;
            public ushort Version;
            public uint Flags;
            public byte PixelType;
            public byte ColorBits;
            public byte RedBits;
            public byte RedShift;
            public byte GreenBits;
            public byte GreenShift;
            public byte BlueBits;
            public byte BlueShift;
            public byte AlphaBits;
            public byte AlphaShift;
            public byte AccumBits;
            public byte AccumRedBits;
            public byte AccumGreenBits;
            public byte AccumBlueBits;
            public byte AccumAlphaBits;
            public byte DepthBits;
            public byte StencilBits;
            public byte AuxBuffers;
            public byte LayerType;
            private byte Reserved;
            public uint LayerMask;
            public uint VisibleMask;
            public uint DamageMask;

            public static PixelFormatDescriptor Build()
            {
                var pfd = new PixelFormatDescriptor
                {
                    Size = (ushort)Marshal.SizeOf(typeof(PixelFormatDescriptor)),
                    Version = 1
                };
                return pfd;
            }
        }

        [DllImport(USER32, CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport(GDI32)]
        private static extern int ChoosePixelFormat(IntPtr hdc, ref PixelFormatDescriptor ppfd);

        [DllImport(GDI32)]
        private static extern int SetPixelFormat(IntPtr hdc, int format, ref PixelFormatDescriptor ppfd);

        [DllImport(OPENGL32)]
        private static extern int wglCreateContext(IntPtr hdc);

        [DllImport(OPENGL32)]
        private static extern int wglDeleteContext(int hglrc);

        [DllImport(OPENGL32)]
        private static extern int wglMakeCurrent(IntPtr hdc, int hglrc);

        [DllImport(OPENGL32, EntryPoint = "glTexImage2D")]
        private static extern void glTexImage2D(uint target, int level, uint internalformat, int width, int height, int border, uint format, uint type, IntPtr pixels);

        [DllImport(OPENGL32, EntryPoint = "glTexParameteri")]
        private static extern void glTexParameteri(uint target, uint name, int param);

        [DllImport(OPENGL32, EntryPoint = "glEnable")]
        private static extern void glEnable(uint cap);

        [DllImport(OPENGL32, EntryPoint = "glDisable")]
        private static extern void glDisable(uint cap);

        [DllImport(OPENGL32, EntryPoint = "glClear")]
        private static extern void glClear(int mask);

        [DllImport(OPENGL32, EntryPoint = "glViewport")]
        private static extern void glViewport(int x, int y, int width, int height);

        [DllImport(OPENGL32, EntryPoint = "glGenLists")]
        private static extern uint glGenLists(int range);

        [DllImport(OPENGL32)]
        private static extern void glNewList(uint list, int mode);

        [DllImport(OPENGL32)]
        private static extern void glEndList();

        [DllImport(OPENGL32)]
        private static extern void glCallList(uint list);

        [DllImport(OPENGL32, EntryPoint = "wglGetProcAddress")]
        private static extern IntPtr wglGetProcAddress(string function);

        [DllImport(OPENGL32, EntryPoint = "glBindTexture")]
        private static extern void glBindTexture(uint target, uint texture);

        [DllImport(OPENGL32, EntryPoint = "glClearColor")]
        private static extern void glClearColor(float r, float g, float b, float alpha);

        [DllImport(OPENGL32, EntryPoint = "glGenTextures")]
        private static extern void glGenTextures(int n, ref uint textures);

        [DllImport(OPENGL32, EntryPoint = "glOrtho")]
        private static extern void glOrtho(double left, double right, double bottom, double top, double zNear, double zFar);

        [DllImport(OPENGL32, EntryPoint = "glRotated")]
        private static extern void glRotated(double angle, double x, double y, double z);

        [DllImport(OPENGL32, EntryPoint = "glMatrixMode")]
        private static extern void glMatrixMode(uint mode);

        [DllImport(OPENGL32, EntryPoint = "glLoadIdentity")]
        private static extern void glLoadIdentity();

        [DllImport(OPENGL32, EntryPoint = "glFinish")]
        private static extern void glFinish();

        [DllImport(OPENGL32, EntryPoint = "glReadPixels")]
        private static extern void glReadPixels(int x, int y, int width, int height, uint format, uint type, IntPtr pixels);

        [DllImport(OPENGL32, EntryPoint = "glEnd")]
        private static extern void glEnd();

        [DllImport(OPENGL32, EntryPoint = "glDepthMask")]
        private static extern void glDepthMask(int flag);

        [DllImport(OPENGL32, EntryPoint = "glDepthFunc")]
        private static extern void glDepthFunc(int val);

        [DllImport(OPENGL32, EntryPoint = "glDeleteTextures")]
        private static extern void glDeleteTextures(int n, ref uint textures);

        [DllImport(GLU32)]
        private static extern IntPtr gluNewQuadric();

        [DllImport(GLU32)]
        private static extern void gluQuadricTexture(IntPtr quadric, int texture);

        [DllImport(GLU32)]
        private static extern void gluSphere(IntPtr quadric, double radius, int slices, int stacks);

        [DllImport(GLU32)]
        private static extern void gluDeleteQuadric(IntPtr quadric);

        [DllImport(GLFW)]
        private static extern bool glfwInit();

        [DllImport(GLFW)]
        private static extern IntPtr glfwCreateWindow(int width, int height, string title, IntPtr monitor, IntPtr share);

        [DllImport(GLFW)]
        private static extern void glfwMakeContextCurrent(IntPtr window);

        [DllImport(GLFW)]
        private static extern void glfwWindowHint(int hint, int value);

        [DllImport(GLFW)]
        private static extern void glfwDestroyWindow(IntPtr window);

        private delegate void glGenFramebuffers(int n, ref uint buffers);
        private delegate void glGenRenderbuffers(int n, ref uint buffer);
        private delegate void glBindFramebuffer(uint target, uint buffer);
        private delegate void glRenderbufferStorage(uint target, int internalFormat, int width, int height);
        private delegate void glBindRenderbuffer(uint target, uint buffer);
        private delegate void glFramebufferRenderbuffer(uint target, uint attachment, uint renderbuffertarget, uint renderbuffer);
        private delegate void glDeleteFramebuffers(int size, ref uint buffer);
        private delegate void glDeleteRenderbuffers(int size, ref uint buffer);

        private glGenFramebuffers GenFramebuffers;
        private glBindFramebuffer BindFramebuffer;
        private glGenRenderbuffers GenRenderbuffers;
        private glRenderbufferStorage RenderbufferStorage;
        private glBindRenderbuffer BindRenderbuffer;
        private glFramebufferRenderbuffer FramebufferRenderbuffer;
        private glDeleteFramebuffers DeleteFramebuffers;
        private glDeleteRenderbuffers DeleteRenderbuffers;

        private void LoadFunctionPointers()
        {
            GenFramebuffers = GetMethod<glGenFramebuffers>();
            GenRenderbuffers = GetMethod<glGenRenderbuffers>();
            BindFramebuffer = GetMethod<glBindFramebuffer>();
            RenderbufferStorage = GetMethod<glRenderbufferStorage>();
            BindRenderbuffer = GetMethod<glBindRenderbuffer>();
            FramebufferRenderbuffer = GetMethod<glFramebufferRenderbuffer>();
            DeleteFramebuffers = GetMethod<glDeleteFramebuffers>();
            DeleteRenderbuffers = GetMethod<glDeleteRenderbuffers>();
        }

        private static T GetMethod<T>()
        {
            var funcPtr = wglGetProcAddress(typeof(T).Name);
            if (funcPtr == IntPtr.Zero)
            {
                throw new Exception($"Unable to load function pointer: {typeof(T).Name}");
            }
            return Marshal.GetDelegateForFunctionPointer<T>(funcPtr);
        }

        public OpenGLSphereRenderer()
        {
            glfwInit();
            IntPtr window = CreateWindow();
            LoadFunctionPointers();
            glfwDestroyWindow(window);
        }

        private IntPtr CreateWindow()
        {
            glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
            glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 0);
            glfwWindowHint(GLFW_VISIBLE, GLFW_FALSE);
            IntPtr window = glfwCreateWindow(1, 1, "", IntPtr.Zero, IntPtr.Zero);
            glfwMakeContextCurrent(window);
            return window;
        }

        [HandleProcessCorruptedStateExceptions]
        public override Image Render(RendererOptions options)
        {
            IntPtr window = IntPtr.Zero;
            try
            {
                Image result;

                window = CreateWindow();

                int size = (int)options.OutputImageSize;

                uint fbo = 0;
                uint rbo = 0;
                GenFramebuffers(1, ref fbo);
                BindFramebuffer(GL_FRAMEBUFFER, fbo);

                GenRenderbuffers(1, ref rbo);
                BindRenderbuffer(GL_RENDERBUFFER, rbo);
                RenderbufferStorage(GL_RENDERBUFFER, GL_RGBA, size, size);
                FramebufferRenderbuffer(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_RENDERBUFFER, rbo);

                using (Bitmap sourceBitmap = CreateTextureBitmap(options))
                {
                    BitmapData data;
                    Rectangle rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
                    data = sourceBitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                    glClearColor(0f, 0f, 0f, 0.0f);
                    glViewport(0, 0, size, size);
                    glMatrixMode(GL_PROJECTION);
                    glLoadIdentity();
                    glOrtho(-1, 1, -1, 1, -1, 0);

                    glRotated(-90 - options.LatitudeShift, 1, 0, 0);
                    glRotated(-options.LongutudeShift, 0, 0, 1);

                    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

                    glDisable(GL_LIGHTING);
                    glDepthMask(GL_TRUE);
                    glEnable(GL_DEPTH_TEST);
                    glDepthFunc(GL_LEQUAL);

                    IntPtr sphere = gluNewQuadric();

                    uint texture = 0;
                    glEnable(GL_TEXTURE_2D);

                    glGenTextures(1, ref texture);
                    glBindTexture(GL_TEXTURE_2D, texture);
                    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, data.Width, data.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, data.Scan0);
                    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
                    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
                    gluQuadricTexture(sphere, GL_TRUE);

                    uint sphereId = glGenLists(1);
                    glNewList(sphereId, GL_COMPILE);
                    gluSphere(sphere, 1.0, 64, 64);
                    glEndList();
                    glCallList(sphereId);
                    glFinish();

                    gluDeleteQuadric(sphere);
                    glDisable(GL_TEXTURE_2D);
                    glDeleteTextures(1, ref texture);
                    glBindTexture(GL_TEXTURE_2D, 0);

                    sourceBitmap.UnlockBits(data);

                    result = GraphicsContextToBitmap(size);
                }

                BindFramebuffer(GL_FRAMEBUFFER, 0);
                BindRenderbuffer(GL_RENDERBUFFER, 0);

                // deinit
                DeleteFramebuffers(1, ref fbo);
                DeleteRenderbuffers(1, ref rbo);

                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"Error on creating texture with OpenGLSphereRenderer: {ex} Use WPFSphereRenderer instead.");
                return base.Render(options);
            }
            finally
            {
                if (window != IntPtr.Zero)
                {
                    glfwDestroyWindow(window);
                }
            }
        }

        private static Bitmap GraphicsContextToBitmap(int size)
        {
            Bitmap bitmap = new Bitmap(size, size);
            Rectangle rect = new Rectangle(0, 0, size, size);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            glReadPixels(0, 0, size, size, GL_RGBA, GL_UNSIGNED_BYTE, data.Scan0);
            bitmap.UnlockBits(data);
            return bitmap;
        }
    }
}
