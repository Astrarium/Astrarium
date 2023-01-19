using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;

namespace Astrarium.Types.Controls
{
    public class MapControl : WindowsFormsHost
    {
        public static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.Register(nameof(IsUpdating), typeof(bool), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = false, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(IsUpdatingPropertyChanged) });

        public static readonly DependencyProperty CacheFolderProperty =
            DependencyProperty.Register(nameof(CacheFolder), typeof(string), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public static readonly DependencyProperty OverlayOpacityProperty =
            DependencyProperty.Register(nameof(OverlayOpacity), typeof(float), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged), DefaultValue = 0.5f });

        public static readonly DependencyProperty TileServerProperty =
            DependencyProperty.Register(nameof(TileServer), typeof(ITileServer), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public static readonly DependencyProperty OverlayTileServerProperty =
           DependencyProperty.Register(nameof(OverlayTileServer), typeof(ITileServer), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register(nameof(Center), typeof(GeoPoint), typeof(MapControl), new FrameworkPropertyMetadata(new GeoPoint(0, 0)) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public static readonly DependencyProperty MouseProperty =
            DependencyProperty.Register(nameof(Mouse), typeof(GeoPoint), typeof(MapControl), new FrameworkPropertyMetadata(new GeoPoint(0, 0)) { BindsTwoWayByDefault = false, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty BackColorProperty =
            DependencyProperty.Register(nameof(BackColor), typeof(Color), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public static readonly DependencyProperty ForeColorProperty =
            DependencyProperty.Register(nameof(ForeColor), typeof(Color), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public static readonly DependencyProperty ThumbnailBackColorProperty =
            DependencyProperty.Register(nameof(ThumbnailBackColor), typeof(Color), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public static readonly DependencyProperty ThumbnailForeColorProperty =
            DependencyProperty.Register(nameof(ThumbnailForeColor), typeof(Color), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public static readonly DependencyProperty ThumbnailTextProperty =
            DependencyProperty.Register(nameof(ThumbnailText), typeof(string), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public static readonly DependencyProperty IsMouseOverMapProperty =
            DependencyProperty.Register(nameof(IsMouseOverMap), typeof(bool), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty ErrorColorProperty =
            DependencyProperty.Register(nameof(ErrorColor), typeof(Color), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public static readonly DependencyProperty TileImageAttributesProperty =
            DependencyProperty.Register(nameof(TileImageAttributes), typeof(ImageAttributes), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public static readonly DependencyProperty MarkersProperty =
            DependencyProperty.Register(nameof(Markers), typeof(ObservableCollection<Marker>), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public static readonly DependencyProperty TracksProperty =
            DependencyProperty.Register(nameof(Tracks), typeof(ObservableCollection<Track>), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public static readonly DependencyProperty PolygonsProperty =
            DependencyProperty.Register(nameof(Polygons), typeof(ObservableCollection<Polygon>), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public static readonly DependencyProperty OnMarkerDrawProperty =
            DependencyProperty.Register(nameof(OnMarkerDraw), typeof(Command<DrawMarkerEventArgs>), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty OnDoubleClickProperty =
            DependencyProperty.Register(nameof(OnDoubleClick), typeof(ICommand), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty OnRightClickProperty =
            DependencyProperty.Register(nameof(OnRightClick), typeof(ICommand), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty MinZoomLevelProperty =
           DependencyProperty.Register(nameof(MinZoomLevel), typeof(int), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = false, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty MaxZoomLevelProperty =
           DependencyProperty.Register(nameof(MaxZoomLevel), typeof(int), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = false, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register(nameof(ZoomLevel), typeof(int), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public bool IsUpdating
        {
            get => (bool)GetValue(IsUpdatingProperty);
            set => SetValue(IsUpdatingProperty, value);
        }

        public ITileServer TileServer
        {
            get => (ITileServer)GetValue(TileServerProperty);
            set => SetValue(TileServerProperty, value);
        }

        public ITileServer OverlayTileServer
        {
            get => (ITileServer)GetValue(OverlayTileServerProperty);
            set => SetValue(OverlayTileServerProperty, value);
        }

        public string CacheFolder
        {
            get => (string)GetValue(CacheFolderProperty);
            set => SetValue(CacheFolderProperty, value);
        }

        public float OverlayOpacity
        {
            get => (float)GetValue(OverlayOpacityProperty);
            set => SetValue(OverlayOpacityProperty, value);
        }

        public GeoPoint Center
        {
            get => (GeoPoint)GetValue(CenterProperty);
            set => SetValue(CenterProperty, value);
        }

        public GeoPoint Mouse
        {
            get => (GeoPoint)GetValue(MouseProperty);
            set => SetValue(MouseProperty, value);
        }

        public Color BackColor
        {
            get => (Color)GetValue(BackColorProperty);
            set => SetValue(BackColorProperty, value);
        }

        public Color ForeColor
        {
            get => (Color)GetValue(ForeColorProperty);
            set => SetValue(ForeColorProperty, value);
        }

        public Color ThumbnailBackColor
        {
            get => (Color)GetValue(ThumbnailBackColorProperty);
            set => SetValue(ThumbnailBackColorProperty, value);
        }

        public Color ThumbnailForeColor
        {
            get => (Color)GetValue(ThumbnailForeColorProperty);
            set => SetValue(ThumbnailForeColorProperty, value);
        }

        public string ThumbnailText
        {
            get => (string)GetValue(ThumbnailTextProperty);
            set => SetValue(ThumbnailTextProperty, value);
        }

        public bool IsMouseOverMap
        {
            get => (bool)GetValue(IsMouseOverMapProperty);
            set => SetValue(IsMouseOverMapProperty, value);
        }

        public int MinZoomLevel
        {
            get => (int)GetValue(MinZoomLevelProperty);
            set => SetValue(MinZoomLevelProperty, value);
        }

        public int MaxZoomLevel
        {
            get => (int)GetValue(MaxZoomLevelProperty);
            set => SetValue(MaxZoomLevelProperty, value);
        }

        public int ZoomLevel
        {
            get => (int)GetValue(ZoomLevelProperty);
            set => SetValue(ZoomLevelProperty, value);
        }

        public Color ErrorColor
        {
            get => (Color)GetValue(ErrorColorProperty);
            set => SetValue(ErrorColorProperty, value);
        }

        public ImageAttributes TileImageAttributes
        {
            get => (ImageAttributes)GetValue(TileImageAttributesProperty);
            set => SetValue(TileImageAttributesProperty, value);
        }

        public ObservableCollection<Marker> Markers
        {
            get => (ObservableCollection<Marker>)GetValue(MarkersProperty);
            set => SetValue(MarkersProperty, value);
        }

        public ObservableCollection<Track> Tracks
        {
            get => (ObservableCollection<Track>)GetValue(TracksProperty);
            set => SetValue(TracksProperty, value);
        }

        public ObservableCollection<Polygon> Polygons
        {
            get => (ObservableCollection<Polygon>)GetValue(PolygonsProperty);
            set => SetValue(PolygonsProperty, value);
        }

        public ICommand OnDoubleClick
        {
            get => (ICommand)GetValue(OnDoubleClickProperty);
            set => SetValue(OnDoubleClickProperty, value);
        }

        public Command<DrawMarkerEventArgs> OnMarkerDraw
        {
            get => (Command<DrawMarkerEventArgs>)GetValue(OnMarkerDrawProperty);
            set => SetValue(OnMarkerDrawProperty, value);
        }

        public ICommand OnRightClick
        {
            get => (ICommand)GetValue(OnRightClickProperty);
            set => SetValue(OnRightClickProperty, value);
        }

        private static void DependencyPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            typeof(System.Windows.Forms.MapControl).GetProperty(e.Property.Name).SetValue((sender as MapControl).mapControl, e.NewValue);
            (sender as MapControl).mapControl.Invalidate();
        }

        private static void IsUpdatingPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool isUpdating = (bool)e.NewValue;
            var mapControl = (sender as MapControl).mapControl;
            if (isUpdating)
                mapControl.BeginUpdate();
            else
                mapControl.EndUpdate();
        }

        private System.Windows.Forms.MapControl mapControl = new System.Windows.Forms.MapControl();

        public MapControl()
        {
            mapControl.MouseEnter += (s, e) => IsMouseOverMap = true;
            mapControl.MouseLeave += (s, e) => { mapControl.Enabled = false; mapControl.Enabled = true; IsMouseOverMap = false; };
            mapControl.MouseEnter += (s, e) => mapControl.Focus();
            mapControl.MouseDown += (s, e) => mapControl.Focus();
            mapControl.MouseUp += (s, e) => mapControl.Focus();
            mapControl.MouseDoubleClick += (s, e) => { if (e.Button == MouseButtons.Left) OnDoubleClick?.Execute(null); };
            mapControl.MouseClick += (s, e) => {
                if (e.Button == MouseButtons.Right)
                {
                    OnRightClick?.Execute(null);
                    if (ContextMenu != null)
                        ContextMenu.IsOpen = true;
                }
            };

            mapControl.DrawMarker += (s, e) => { OnMarkerDraw?.Execute(e); };

            mapControl.TileServerChanged += (s, e) =>
            {
                TileServer = mapControl.TileServer;
                MinZoomLevel = mapControl.MinZoomLevel;
                MaxZoomLevel = mapControl.MaxZoomLevel;
                MapControl_Resize(mapControl, EventArgs.Empty);
            };

            mapControl.OverlayTileServerChanged += (s, e) =>
            {
                OverlayTileServer = mapControl.OverlayTileServer;
                MinZoomLevel = mapControl.MinZoomLevel;
                MaxZoomLevel = mapControl.MaxZoomLevel;
                MapControl_Resize(mapControl, EventArgs.Empty);
            };

            mapControl.CenterChanged += (s, e) => Center = mapControl.Center;
            mapControl.MouseChanged += (s, e) => Mouse = mapControl.Mouse;
            mapControl.BackColorChanged += (s, e) => BackColor = mapControl.BackColor;
            mapControl.ForeColorChanged += (s, e) => ForeColor = mapControl.ForeColor;
            mapControl.ZoomLevelChaged += (s, e) => ZoomLevel = mapControl.ZoomLevel;

            mapControl.Dock = DockStyle.Fill;
            mapControl.Resize += MapControl_Resize;
            Child = mapControl;
            mapControl.Focus();
        }

        private void MapControl_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawLine(Pens.Red, 0, 0, 100, 100);
        }

        private void MapControl_Resize(object sender, System.EventArgs e)
        {
            mapControl.MinZoomLevel = mapControl.Height / 256;
            MinZoomLevel = mapControl.MinZoomLevel;
            MaxZoomLevel = mapControl.MaxZoomLevel;
        }
    }
}
