using System;
using System.Windows;
using System.Windows.Input;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using WF = System.Windows.Forms;
using Astrarium.Types.Themes;
using Astrarium.Types;
using System.Linq;
using Astrarium.Algorithms;
using System.Windows.Forms.Integration;
using Astrarium.Types.Controls;

namespace Astrarium
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MouseEventsInterceptableWindow
    {
        private const uint WM_EXIT_SIZE_MOVE = 0x232;
        private const uint SWP_SHOWWINDOW = 0x0040;

        public static readonly DependencyProperty MouseEquatorialPositionProperty = DependencyProperty.RegisterAttached(
            "MouseEquatorialPosition", typeof(CrdsEquatorial), typeof(MainWindow), new PropertyMetadata(null));

        public static void SetMouseEquatorialPosition(DependencyObject target, CrdsEquatorial value)
        {
            target.SetValue(MouseEquatorialPositionProperty, value);
        }

        public static CrdsEquatorial GetMouseEquatorialPosition(DependencyObject target)
        {
            return (CrdsEquatorial)target.GetValue(MouseEquatorialPositionProperty);
        }

        public static readonly DependencyProperty MouseHorizontalPositionProperty = DependencyProperty.RegisterAttached(
            "MouseHorizontalPosition", typeof(CrdsHorizontal), typeof(MainWindow), new PropertyMetadata(null));

        public static void SetMouseHorizontalPosition(DependencyObject target, CrdsHorizontal value)
        {
            target.SetValue(MouseHorizontalPositionProperty, value);
        }

        public static CrdsHorizontal GetMouseHorizontalPosition(DependencyObject target)
        {
            return (CrdsHorizontal)target.GetValue(MouseHorizontalPositionProperty);
        }

        public static readonly DependencyProperty MousePositionConstellationProperty = DependencyProperty.RegisterAttached(
            "MousePositionConstellation", typeof(string), typeof(MainWindow), new PropertyMetadata(null));

        public static void SetMousePositionConstellation(DependencyObject target, string value)
        {
            target.SetValue(MousePositionConstellationProperty, value);
        }

        public static string GetMousePositionConstellation(DependencyObject target)
        {
            return (string)target.GetValue(MousePositionConstellationProperty);
        }

        public static readonly DependencyProperty FPSProperty = DependencyProperty.RegisterAttached(
            "FPS", typeof(string), typeof(MainWindow), new PropertyMetadata(null));

        public static void SetFPS(DependencyObject target, string value)
        {
            target.SetValue(FPSProperty, value);
        }

        public static string GetFPS(DependencyObject target)
        {
            return (string)target.GetValue(FPSProperty);
        }

        public static readonly DependencyProperty MapKeyDownProperty = DependencyProperty.RegisterAttached(
            "MapKeyDown", typeof(Command<KeyEventArgs>), typeof(MainWindow));

        public static void SetMapKeyDown(DependencyObject target, Command<KeyEventArgs> value)
        {
            target.SetValue(MapKeyDownProperty, value);
        }

        public static Command<KeyEventArgs> GetMapKeyDown(DependencyObject target)
        {
            return (Command<KeyEventArgs>)target.GetValue(MapKeyDownProperty);
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

        public static readonly DependencyProperty WindowSizeProperty = DependencyProperty.RegisterAttached(
           "WindowSize", typeof(System.Drawing.Size), typeof(MainWindow));

        public static void SetWindowSize(DependencyObject target, System.Drawing.Size value)
        {
            target.SetValue(WindowSizeProperty, value);
        }

        public static System.Drawing.Size GetWindowSize(DependencyObject target)
        {
            return (System.Drawing.Size)target.GetValue(WindowSizeProperty);
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
                    window.SaveState();
                    if (window.WindowState != WindowState.Normal)
                    {
                        window.Left = bounds.Left;
                        window.Top = bounds.Top;
                        window.Width = bounds.Width;
                        window.Height = bounds.Height;
                    }

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
                window.MainWindow_StateChanged(window, EventArgs.Empty);
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

        private Rectangle SavedBounds;
        private WindowState SavedState;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int left, int top, int width, int height, uint flags);

        public static WF.Screen CurrentScreen(Window window)
        {
            return WF.Screen.FromPoint(new System.Drawing.Point((int)window.Left, (int)window.Top));
        }

        private readonly SkyView skyView;
        private readonly SkyMap map;

        protected override WindowsFormsHost Host => MapHost;

        public MainWindow(ISettings settings, SkyMap map)
        {
            InitializeComponent();

            this.map = map;
            this.StateChanged += MainWindow_StateChanged;

            skyView = new SkyView();
            skyView.Paint += SkyView_Paint;
            skyView.Resize += SkyView_Resize;
            skyView.MouseDown += SkyView_MouseDown;
            skyView.MouseUp += SkyView_MouseUp;
            skyView.MouseMove += SkyView_MouseMove;
            skyView.MouseWheel += SkyView_MouseWheel;
            skyView.MouseClick += SkyView_MouseClick;
            skyView.MouseDoubleClick += SkyView_MouseDoubleClick;
            map.OnInvalidate += skyView.Invalidate;

            settings.SettingValueChanged += (s, v) => skyView.Invalidate();

            Host.Child = skyView;

            Host.Loaded += (o, e) => WF.Application.AddMessageFilter(this);
            Host.KeyDown += (o, e) => GetMapKeyDown(this)?.Execute(e);

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void SkyView_MouseDoubleClick(object sender, WF.MouseEventArgs e)
        {
            GetMapDoubleClick(this)?.Execute(new PointF(e.X, e.Y));
        }

        private void SkyView_MouseWheel(object sender, WF.MouseEventArgs e)
        {
            map.Projection.Fov *= Math.Pow(1.1, -e.Delta / 120);
            map.Invalidate();
        }

        private void SkyView_MouseDown(object sender, WF.MouseEventArgs e)
        {
            map.MouseButton = e.Button == WF.MouseButtons.Left ? Types.MouseButton.Left : Types.MouseButton.None;
            map.RaiseMouseDown();
        }

        private void SkyView_MouseUp(object sender, WF.MouseEventArgs e)
        {
            pOld = PointF.Empty;
            map.MouseButton = e.Button == WF.MouseButtons.Left ? Types.MouseButton.Left : Types.MouseButton.None;
            map.RaiseMouseUp();
        }

        private PointF pOld = new PointF();

        private void SkyView_MouseMove(object sender, WF.MouseEventArgs e)
        {
            long tooltipTag = skyToolTip.Tag as long? ?? 0;

            var p = new PointF(e.X, map.Projection.ScreenHeight - e.Y);

            map.MouseButton = e.Button == WF.MouseButtons.Left ? Types.MouseButton.Left : Types.MouseButton.None;
            map.MouseScreenCoordinates = p;
            var eq = map.Projection.WithoutRefraction(map.MouseEquatorialCoordinates);
            var hor = map.MouseHorizontalCoordinates;

            map.RaiseMouseMove();

            SetMouseEquatorialPosition(this, eq);
            SetMouseHorizontalPosition(this, hor);
            SetMousePositionConstellation(this, Constellations.FindConstellation(eq, map.Projection.Context.JulianDay));

            if (e.Button == WF.MouseButtons.Left)
            {
                if (pOld != PointF.Empty)
                {
                    map.Move(new Vec2(pOld.X, pOld.Y), new Vec2(p.X, p.Y));
                }

                pOld = new PointF(p.X, p.Y);
            }

            if ((WF.Control.ModifierKeys & WF.Keys.Control) != 0)
            {
                var body = map.FindObject(p);
                if (body != null)
                {
                    if (body is IMagnitudeObject mo)
                    {
                        skyToolTip.Content = $"{body.Names.First()} {Formatters.Magnitude.Format(mo.Magnitude)}";
                    }
                    else
                    {
                        skyToolTip.Content = body.Names.First();
                    }
                    skyToolTip.PlacementRectangle = new Rect(e.X, e.Y, 0, 0);
                    skyToolTip.IsOpen = true;
                    skyToolTip.Tag = tooltipTag + 1;
                }
            }

            if (skyToolTip.Tag as long? <= tooltipTag)
            {
                skyToolTip.IsOpen = false;
            }
        }

        private void SkyView_Resize(object sender, EventArgs e)
        {
            map.Projection.SetScreenSize(skyView.Width, skyView.Height);
        }

        private System.Diagnostics.Stopwatch renderStopWatch = new System.Diagnostics.Stopwatch();

        private void SkyView_Paint(object sender, WF.PaintEventArgs e)
        {
            renderStopWatch.Restart();
            map.Render();
            renderStopWatch.Stop();
            int fps = (int)(1000f / renderStopWatch.ElapsedMilliseconds);
            SetFPS(this, $"FPS = {fps}");
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CursorsHelper.SetSystemCursors();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized || GetFullScreen(this))
            {
                Host.Margin = new Thickness(-1);
            }
            else
            {
                Host.Margin = new Thickness(5, 0, 5, 0);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var helper = new WindowInteropHelper(this);
            if (helper.Handle != null)
            {
                var source = HwndSource.FromHwnd(helper.Handle);
                if (source != null)
                    source.AddHook(HwndMessageHook);
            }

            var size = GetWindowSize(this);
            if (size != System.Drawing.Size.Empty)
            {
                var bounds = CurrentScreen(this).Bounds;

                if (size.Width > bounds.Width)
                    size.Width = bounds.Width;

                if (size.Height > bounds.Height)
                    size.Height = bounds.Height;

                Width = size.Width;
                Height = size.Height;

                var left = (bounds.Width - size.Width) / 2;
                var top = (bounds.Height - size.Height) / 2;

                if (left < 0)
                    left = 0;

                if (top < 0)
                    top = 0;

                Left = left;
                Top = top;
            }
        }

        private IntPtr HwndMessageHook(IntPtr wnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((uint)msg)
            {
                case WM_EXIT_SIZE_MOVE:
                    SetWindowSize(this, new System.Drawing.Size((int)RenderSize.Width, (int)RenderSize.Height));
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }

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
                GetMapRightClick(this)?.Execute(map.MouseScreenCoordinates);
                // setting placement target is needed to update context menu colors:
                // see https://github.com/MahApps/MahApps.Metro/issues/2244
                Host.ContextMenu.PlacementTarget = Host;
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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Environment.Exit(0);
        }
    }
}
