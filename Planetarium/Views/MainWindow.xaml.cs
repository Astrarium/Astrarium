using ADK.Demo;
using ADK.Demo.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WF = System.Windows.Forms;
using System.Windows.Input;
using ADK.Demo.Config;
using Planetarium.ViewModels;
using System.Drawing;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Planetarium.Themes;
using System.Windows.Forms;
using System.Windows.Shell;

namespace Planetarium
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty MousePositionProperty = DependencyProperty.RegisterAttached(
            "MousePosition", typeof(PointF), typeof(MainWindow), new PropertyMetadata(PointF.Empty));

        public static void SetMousePosition(DependencyObject target, PointF value)
        {
            target.SetValue(MousePositionProperty, value);
        }

        public static PointF GetMousePosition(DependencyObject target)
        {
            return (PointF)target.GetValue(MousePositionProperty);
        }

        public static readonly DependencyProperty MapKeyDownProperty = DependencyProperty.RegisterAttached(
            "MapKeyDown", typeof(Command<Key>), typeof(MainWindow));

        public static void SetMapKeyDown(DependencyObject target, Command<Key> value)
        {
            target.SetValue(MapKeyDownProperty, value);
        }

        public static Command<Key> GetMapKeyDown(DependencyObject target)
        {
            return (Command<Key>)target.GetValue(MapKeyDownProperty);
        }

        public static readonly DependencyProperty MapZoomProperty = DependencyProperty.RegisterAttached(
            "MapZoom", typeof(Command<int>), typeof(MainWindow));

        public static void SetMapZoom(DependencyObject target, Command<int> value)
        {
            target.SetValue(MapZoomProperty, value);
        }

        public static Command<int> GetMapZoom(DependencyObject target)
        {
            return (Command<int>)target.GetValue(MapZoomProperty);
        }

        public static readonly DependencyProperty MapDoubleClickProperty = DependencyProperty.RegisterAttached(
            "MapDoubleClick", typeof(Command<PointF>), typeof(MainWindow));

        public static void SetMapDoubleClick(DependencyObject target, Command<PointF> value)
        {
            target.SetValue(MapDoubleClickProperty, value);
        }

        public static Command<PointF> GetMapDoubleClick(DependencyObject target)
        {
            return (Command<PointF>)target.GetValue(MapDoubleClickProperty);
        }

        public static readonly DependencyProperty FullScreenProperty = DependencyProperty.RegisterAttached(
            "FullScreen", typeof(bool), typeof(MainWindow), new FrameworkPropertyMetadata(false, (o, e) =>
            {
                MainWindow window = (MainWindow)o;
                bool value = (bool)e.NewValue;
                var handle = new WindowInteropHelper(window).Handle;
                
                uint flags = SWP_SHOWWINDOW;
                if (value)
                {
                    var bounds = CurrentScreen(window).Bounds;
                    if (window.WindowState != WindowState.Normal)
                    {
                        window.Left = bounds.Left;
                        window.Top = bounds.Top;
                        window.Width = bounds.Width;
                        window.Height = bounds.Height;                        
                    }

                    window.SaveState();

                    WindowProperties.SetIsFullScreen(window, true);
                    window.WindowState = WindowState.Normal;
                    SetWindowPos(handle, HWND_TOPMOST, bounds.Left, bounds.Top, bounds.Width, bounds.Height, flags);
                }
                else
                {
                    var bounds = window.SavedBounds;
                    window.RestoreState();

                    WindowProperties.SetIsFullScreen(window, false);
                    SetWindowPos(handle, HWND_NOTOPMOST, bounds.Left, bounds.Top, bounds.Width, bounds.Height, flags);
                }
            }));

        public static void SetFullScreen(DependencyObject target, bool value)
        {
            target.SetValue(FullScreenProperty, value);
        }

        public static bool GetFullScreen(DependencyObject target)
        {
            return (bool)target.GetValue(FullScreenProperty);
        }

        public static readonly DependencyProperty MapRightClickProperty = DependencyProperty.RegisterAttached(
            "MapRightClick", typeof(Command<PointF>), typeof(MainWindow));

        public static void SetMapRightClick(DependencyObject target, Command<PointF> value)
        {
            target.SetValue(MapRightClickProperty, value);
        }

        public static Command<PointF> GetMapRightClick(DependencyObject target)
        {
            return (Command<PointF>)target.GetValue(MapRightClickProperty);
        }

        private static readonly IntPtr HWND_TOPMOST = (IntPtr)(-1);
        private static readonly IntPtr HWND_NOTOPMOST = (IntPtr)(-2);
        private const uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int left, int top, int width, int height, uint flags);

        public static Screen CurrentScreen(Window window)
        {
            return Screen.FromPoint(new System.Drawing.Point((int)window.Left, (int)window.Top));
        }

        public MainWindow(ISkyMap map)
        {
            InitializeComponent();

            var skyView = new SkyView();
            skyView.SkyMap = map;
            skyView.MouseDoubleClick += (o, e) => GetMapDoubleClick(this)?.Execute(new PointF((e as WF.MouseEventArgs).X, (e as WF.MouseEventArgs).Y));
            skyView.MouseClick += SkyView_MouseClick;
            skyView.MouseMove += (o, e) => { skyView.Focus(); SetMousePosition(this, new PointF(e.X, e.Y)); };
            skyView.MouseWheel += (o, e) => GetMapZoom(this)?.Execute(e.Delta);
            Host.KeyDown += (o, e) => GetMapKeyDown(this)?.Execute(e.Key);
            Host.Child = skyView;
        }

        private Rectangle SavedBounds;
        private WindowState SavedState;

        private void SaveState()
        {
            SavedState = WindowState;
            SavedBounds = new Rectangle((int)Left, (int)Top, (int)Width, (int)Height);
        }

        private void RestoreState()
        {
            Left = SavedBounds.Left;
            Top = SavedBounds.Top;
            Width = SavedBounds.Width;
            Height = SavedBounds.Height;
            WindowState = SavedState;
        }

        private void SkyView_MouseClick(object sender, WF.MouseEventArgs e)
        {
            if (e.Button == WF.MouseButtons.Right && e.Clicks == 1)
            {
                GetMapRightClick(this)?.Execute(new PointF(e.X, e.Y));
                Host.ContextMenu.IsOpen = true;
            }
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (Host.ContextMenu.DataContext == null)
            {
                Host.ContextMenu.DataContext = DataContext;
            }
        }
    }
}
