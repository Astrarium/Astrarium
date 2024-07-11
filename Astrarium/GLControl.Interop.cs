using System;
using System.Runtime.InteropServices;

namespace Astrarium
{
    public partial class GLControl
    {
        private const string GDI32 = "gdi32";
        private const string OPENGL32 = "opengl32";
        private const string USER32 = "user32";

        private const int WCONTEXT_MAJOR_VERSION_ARB = 0x2091;
        private const int WCONTEXT_MINOR_VERSION_ARB = 0x2092;

        private const int WCONTEXT_PROFILE_MASK_ARB = 0x9126;
        private const int WCONTEXT_CORE_PROFILE_BIT_ARB = 0x00000001;
        private const int WCONTEXT_FLAGS_ARB = 0x2094;
        private const int WCONTEXT_FORWARD_COMPATIBLE_BIT_ARB = 0x00000002;
        private const int WCONTEXT_COMPATIBILITY_PROFILE_BIT_ARB = 0x00000002;

        [DllImport(USER32, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport(GDI32, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern bool SwapBuffers(IntPtr dc);

        [DllImport(GDI32, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern int ChoosePixelFormat(IntPtr hdc, [In] ref PixelFormatDescriptor ppfd);

        [DllImport(GDI32, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern bool SetPixelFormat(IntPtr hdc, int iPixelFormat, ref PixelFormatDescriptor ppfd);

        [DllImport(OPENGL32, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr wglCreateContext(IntPtr hDC);

        [DllImport(OPENGL32, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hrc);

        [DllImport(OPENGL32, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern bool wglDeleteContext(IntPtr hrc);

        [DllImport(OPENGL32)]
        private static extern IntPtr wglGetProcAddress(string name);

        [DllImport(OPENGL32)]
        private static extern void glViewport(int x, int y, int width, int height);

        [DllImport(OPENGL32, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr glGetString(uint name);

        private delegate IntPtr wglCreateContextAttribsARB(IntPtr hDC, IntPtr hShareContext, int[] attribs);

        [StructLayout(LayoutKind.Sequential)]
        private struct PixelFormatDescriptor
        {
            public void Init()
            {
                nSize = (ushort)Marshal.SizeOf(typeof(PixelFormatDescriptor));
                nVersion = 1;
                dwFlags = PFD_FLAGS.PFD_DRAW_TO_WINDOW | PFD_FLAGS.PFD_SUPPORT_OPENGL | PFD_FLAGS.PFD_DOUBLEBUFFER;
                iPixelType = PFD_PIXEL_TYPE.PFD_TYPE_RGBA;
                cColorBits = 24;
                cRedBits = cRedShift = cGreenBits = cGreenShift = cBlueBits = cBlueShift = 0;
                cAlphaBits = cAlphaShift = 0;
                cAccumBits = cAccumRedBits = cAccumGreenBits = cAccumBlueBits = cAccumAlphaBits = 0;
                cDepthBits = 32;
                cStencilBits = cAuxBuffers = 0;
                iLayerType = PFD_LAYER_TYPES.PFD_MAIN_PLANE;
                bReserved = 0;
                dwLayerMask = dwVisibleMask = dwDamageMask = 0;
            }
            ushort nSize;
            ushort nVersion;
            PFD_FLAGS dwFlags;
            PFD_PIXEL_TYPE iPixelType;
            byte cColorBits;
            byte cRedBits;
            byte cRedShift;
            byte cGreenBits;
            byte cGreenShift;
            byte cBlueBits;
            byte cBlueShift;
            byte cAlphaBits;
            byte cAlphaShift;
            byte cAccumBits;
            byte cAccumRedBits;
            byte cAccumGreenBits;
            byte cAccumBlueBits;
            byte cAccumAlphaBits;
            byte cDepthBits;
            byte cStencilBits;
            byte cAuxBuffers;
            PFD_LAYER_TYPES iLayerType;
            byte bReserved;
            uint dwLayerMask;
            uint dwVisibleMask;
            uint dwDamageMask;
        }

        [Flags]
        private enum PFD_FLAGS : uint
        {
            PFD_DOUBLEBUFFER = 0x00000001,
            PFD_STEREO = 0x00000002,
            PFD_DRAW_TO_WINDOW = 0x00000004,
            PFD_DRAW_TO_BITMAP = 0x00000008,
            PFD_SUPPORT_GDI = 0x00000010,
            PFD_SUPPORT_OPENGL = 0x00000020,
            PFD_GENERIC_FORMAT = 0x00000040,
            PFD_NEED_PALETTE = 0x00000080,
            PFD_NEED_SYSTEM_PALETTE = 0x00000100,
            PFD_SWAP_EXCHANGE = 0x00000200,
            PFD_SWAP_COPY = 0x00000400,
            PFD_SWAP_LAYER_BUFFERS = 0x00000800,
            PFD_GENERIC_ACCELERATED = 0x00001000,
            PFD_SUPPORT_DIRECTDRAW = 0x00002000,
            PFD_DIRECT3D_ACCELERATED = 0x00004000,
            PFD_SUPPORT_COMPOSITION = 0x00008000,
            PFD_DEPTH_DONTCARE = 0x20000000,
            PFD_DOUBLEBUFFER_DONTCARE = 0x40000000,
            PFD_STEREO_DONTCARE = 0x80000000
        }

        private enum PFD_LAYER_TYPES : byte
        {
            PFD_MAIN_PLANE = 0,
            PFD_OVERLAY_PLANE = 1,
            PFD_UNDERLAY_PLANE = 255
        }

        private enum PFD_PIXEL_TYPE : byte
        {
            PFD_TYPE_RGBA = 0,
            PFD_TYPE_COLORINDEX = 1
        }
    }
}
