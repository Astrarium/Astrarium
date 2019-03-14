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
                int width = (int)SystemParameters.PrimaryScreenWidth;
                int height = (int)SystemParameters.PrimaryScreenHeight;
                uint flags = SWP_SHOWWINDOW;
                if (value)
                {
                    if (window.WindowState != WindowState.Normal)
                    {
                        window.Left = 0;
                        window.Top = 0;
                        window.Width = width;
                        window.Height = height;                        
                    }

                    window.SaveState();
                    window.WindowState = WindowState.Normal;
                    SetWindowPos(handle, HWND_TOPMOST, 0, 0, width, height, flags);
                }
                else
                {
                    window.RestoreState();
                    SetWindowPos(handle, HWND_NOTOPMOST, window.LastSize.Left, window.LastSize.Top, window.LastSize.Width, window.LastSize.Height, flags);
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

        static readonly IntPtr HWND_TOPMOST = (IntPtr)(-1);
        static readonly IntPtr HWND_NOTOPMOST = (IntPtr)(-2);
        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_SHOWWINDOW = 0x0040;
        const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        private Rectangle LastSize;

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

        private WindowState LastState;

        private void SaveState()
        {
            LastState = WindowState;
            LastSize = new Rectangle((int)Left, (int)Top, (int)Width, (int)Height);
        }

        private void RestoreState()
        {
            Left = LastSize.Left;
            Top = LastSize.Top;
            Width = LastSize.Width;
            Height = LastSize.Height;
            WindowState = LastState;
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
