using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Astrarium
{
    [DesignerCategory("Code")]
    public partial class GLControl : UserControl
    {
        /// <summary>
        /// Subscribers must imlpement drawing logic in Render event handler
        /// </summary>
        public event EventHandler Render;

        public event EventHandler Initialized;

        public GLControl() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.DoubleBuffer, false);
            DoubleBuffered = false;
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        /// <summary>
        /// Device context handle.
        /// </summary>
        private IntPtr hDC;

        /// <summary>
        /// Render context handle.
        /// </summary>
        private IntPtr hRC;

        /// <summary>
        /// Reset event to manage rendering cycle.
        /// </summary>
        private AutoResetEvent renderResetEvent = new AutoResetEvent(true);

        /// <summary>
        /// Flag indicating the rendering will be done on the main app thread.
        /// If flag is on, continuous rendering mode is unsupported.
        /// Default value is false.
        /// </summary>
        protected virtual bool RenderOnMainThread { get; } = false;

        /// <summary>
        /// There are 2 drawing modes available.
        /// If continuous rendering mode is turned on, the control renders itself automatically, i.e. rendering cycle is an inifinite loop.
        /// If continuous rendering mode is turned off, you need to call Refresh() / Invalidate() each time the control should be repainted.
        /// </summary>
        protected virtual bool RenderContinuous { get; } = false;

        /// <summary>
        /// Flag indicating specific OpenGL version should be used.
        /// If set to true, you must specify <see cref="MajorOpenGLVersion" /> and <see cref="MinorOpenGLVersion"/> values.
        /// Default value is false.
        /// </summary>
        protected virtual bool UseSpecificOpenGLVersion { get; } = false;

        /// <summary>
        /// Specifies major OpenGL verion to be requested.
        /// </summary>
        protected virtual uint MajorOpenGLVersion { get; } = 0;

        /// <summary>
        /// Specifies minor OpenGL verion to be requested.
        /// </summary>
        protected virtual uint MinorOpenGLVersion { get; } = 0;

        /// <inheritdoc />
        protected override void OnHandleCreated(EventArgs e)
        {
            hDC = GetDC(Handle);

            if (RenderContinuous && RenderOnMainThread)
                throw new Exception("Continuous rendering is incompatible with rendering on main thread.");

            if (RenderOnMainThread)
                Initialize();
            else
                new Thread(RenderLoop) { Name = "OpenGL Render Loop", IsBackground = true }.Start();
        }

        /// <summary>
        /// Delay in continuous rendering mode
        /// </summary>
        public int RenderContinuousDelay { get; set; } = 0;

        private void RenderLoop()
        {
            Initialize();

            while (true)
            {
                OnRender();

                if (RenderContinuous)
                    renderResetEvent.WaitOne(RenderContinuousDelay);
                else
                    renderResetEvent.WaitOne();
            }
        }

        private void OnRender()
        {
            glViewport(0, 0, Width, Height);
            Render?.Invoke(this, EventArgs.Empty);
            SwapBuffers(hDC);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            if (hRC != IntPtr.Zero)
            {
                wglDeleteContext(hRC);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (hRC != IntPtr.Zero)
            {
                if (RenderOnMainThread)
                {
                    OnRender();
                }
                else if (!RenderContinuous)
                {
                    renderResetEvent.Set();
                }
            }
        }

        private void Initialize()
        {
            var pfd = new PixelFormatDescriptor();
            pfd.Init();

            // Match an appropriate pixel format 
            int iPixelformat;
            if ((iPixelformat = ChoosePixelFormat(hDC, ref pfd)) == 0)
            {
                throw new Exception("Error on choosing pixel format.");
            }

            // Sets the pixel format
            if (SetPixelFormat(hDC, iPixelformat, ref pfd) == false)
            {
                throw new Exception("Error on setting pixel format.");
            }
            
            // Need to request context with specific OpenGL version
            if (UseSpecificOpenGLVersion)
            {
                if (MajorOpenGLVersion == 0)
                    throw new Exception("You must specify valid MajorOpenGLVersion.");

                // Create the dummy render context
                var hrc = wglCreateContext(hDC);
                if (hrc == IntPtr.Zero)
                {
                    throw new Exception("Error on creating dummy render context.");
                }

                // Make it current
                if (wglMakeCurrent(hDC, hrc) == false)
                {
                    throw new Exception("Error on setting dummy context as current.");
                }

                // Obtain extension function for creating context with attributes
                IntPtr wglCreateContextAttribsARBPtr = wglGetProcAddress(nameof(wglCreateContextAttribsARB));
                wglCreateContextAttribsARB createContextAttribs = (wglCreateContextAttribsARB)Marshal.GetDelegateForFunctionPointer(wglCreateContextAttribsARBPtr, typeof(wglCreateContextAttribsARB));

                // Set specific OpenGL version
                int[] attributes =
                {
                    WCONTEXT_MAJOR_VERSION_ARB, (int)MajorOpenGLVersion,
                    WCONTEXT_MINOR_VERSION_ARB, (int)MinorOpenGLVersion,

                    // Uncomment this for forward compatibility mode
                    // WCONTEXT_FLAGS_ARB, WCONTEXT_FORWARD_COMPATIBLE_BIT_ARB,
                    // Uncomment this for Compatibility profile
                    WCONTEXT_PROFILE_MASK_ARB, WCONTEXT_COMPATIBILITY_PROFILE_BIT_ARB,
                    // We are using Core profile here
                    WCONTEXT_PROFILE_MASK_ARB, WCONTEXT_CORE_PROFILE_BIT_ARB,
                };

                // Create render context with attributes
                hRC = createContextAttribs(hDC, IntPtr.Zero, attributes);

                // Delete dummy instance
                if (wglDeleteContext(hrc) == false)
                {
                    throw new Exception("Unable delete dummy context.");
                }
            }
            else
            {
                // Create the render context
                hRC = wglCreateContext(hDC);
                if (hRC == IntPtr.Zero)
                {
                    throw new Exception("Error on creating dummy render context.");
                }
            }

            // Make context current
            if (wglMakeCurrent(hDC, hRC) == false)
            {
                throw new Exception("Error on setting context as current.");
            }

            // Get version to make sure it's ok
            OpenGLVersion = Marshal.PtrToStringAnsi(glGetString(0x1F02));

            if (string.IsNullOrEmpty(OpenGLVersion))
            {
                throw new Exception("Unable to get OpenGL version. Seems it's not loaded correctly.");
            }

            Invoke((MethodInvoker)delegate
            {
                Initialized?.Invoke(this, EventArgs.Empty);
            });
        }

        /// <summary>
        /// Gets OpenGL version
        /// </summary>
        public string OpenGLVersion { get; private set; }
    }
}
