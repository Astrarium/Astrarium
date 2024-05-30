using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using WF = System.Windows.Forms;

namespace Astrarium.Types.Controls
{
    public abstract class MouseEventsInterceptableWindow : Window, WF.IMessageFilter
    {
        [DllImport("user32.dll"), SuppressUnmanagedCodeSecurity]
        private static extern IntPtr WindowFromPoint(System.Drawing.Point point);

        private const uint MK_LBUTTON = 0x0001;
        private const uint MK_MBUTTON = 0x0010;
        private const uint MK_RBUTTON = 0x0002;
        private const uint MK_XBUTTON1 = 0x0020;
        private const uint MK_XBUTTON2 = 0x0040;
        private const uint WM_MOUSEWHEEL = 0x020A;

        private MethodInfo onMouseWheelMethod = typeof(WF.Control).GetMethod("OnMouseWheel", BindingFlags.Instance | BindingFlags.NonPublic);

        protected abstract WindowsFormsHost Host { get; }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Host.Loaded += (o, e1) => WF.Application.AddMessageFilter(this);
            Host.Unloaded += (o, e1) => WF.Application.RemoveMessageFilter(this);
        }

        private System.Drawing.Point LocationFromLParam(IntPtr lParam)
        {
            short x = (short)((((long)lParam) >> 0) & 0xffff);
            short y = (short)((((long)lParam) >> 16) & 0xffff);
            return new System.Drawing.Point(x, y);
        }

        private WF.MouseButtons MouseButtonsFromWParam(IntPtr wParam)
        {
            int buttonFlags = (int)((((long)wParam) >> 0) & 0xffff);
            var buttons = WF.MouseButtons.None;
            if (buttonFlags != 0)
            {
                if ((buttonFlags & MK_LBUTTON) == MK_LBUTTON)
                {
                    buttons |= WF.MouseButtons.Left;
                }
                if ((buttonFlags & MK_MBUTTON) == MK_MBUTTON)
                {
                    buttons |= WF.MouseButtons.Middle;
                }
                if ((buttonFlags & MK_RBUTTON) == MK_RBUTTON)
                {
                    buttons |= WF.MouseButtons.Right;
                }
                if ((buttonFlags & MK_XBUTTON1) == MK_XBUTTON1)
                {
                    buttons |= WF.MouseButtons.XButton1;
                }
                if ((buttonFlags & MK_XBUTTON2) == MK_XBUTTON2)
                {
                    buttons |= WF.MouseButtons.XButton2;
                }
            }
            return buttons;
        }

        private bool ConsiderRedirect(WindowsFormsHost host)
        {
            var control = host.Child;
            return control != null &&
                  !control.IsDisposed &&
                   control.IsHandleCreated &&
                   control.Visible &&
                  !control.Focused;
        }

        private int DeltaFromWParam(IntPtr wParam)
        {
            return (short)((((long)wParam) >> 16) & 0xffff);
        }

        bool WF.IMessageFilter.PreFilterMessage(ref WF.Message m)
        {
            if (m.Msg == WM_MOUSEWHEEL)
            {
                var location = LocationFromLParam(m.LParam);
                var hwnd = WindowFromPoint(location);
                {
                    if (ConsiderRedirect(Host))
                    {
                        if (hwnd == Host.Child.Handle)
                        {
                            var delta = DeltaFromWParam(m.WParam);

                            // raise event for WPF control
                            {
                                var mouse = InputManager.Current.PrimaryMouseDevice;
                                var args = new MouseWheelEventArgs(mouse, Environment.TickCount, delta);
                                args.RoutedEvent = MouseWheelEvent;
                                Host.RaiseEvent(args);
                            }

                            // raise event for winforms control
                            {
                                var buttons = MouseButtonsFromWParam(m.WParam);
                                var point = Host.PointFromScreen(new Point(location.X, location.Y));      
                                var args = new WF.MouseEventArgs(buttons, 0, (int)point.X, (int)point.Y, delta);
                                onMouseWheelMethod.Invoke(Host.Child, new object[] { args });
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
