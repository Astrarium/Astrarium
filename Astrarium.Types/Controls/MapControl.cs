using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;

namespace Astrarium.Types.Controls
{
    public class MapControl : WindowsFormsHost
    {
        public static readonly DependencyProperty CacheFolderProperty =
            DependencyProperty.Register(nameof(CacheFolder), typeof(string), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public static readonly DependencyProperty TileServerProperty =
            DependencyProperty.Register(nameof(TileServer), typeof(ITileServer), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

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

        public static readonly DependencyProperty ErrorColorProperty =
            DependencyProperty.Register(nameof(ErrorColor), typeof(Color), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public static readonly DependencyProperty MarkersProperty =
            DependencyProperty.Register(nameof(Markers), typeof(ICollection<Marker>), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public static readonly DependencyProperty TracksProperty =
            DependencyProperty.Register(nameof(Tracks), typeof(ICollection<Track>), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public static readonly DependencyProperty PolygonsProperty =
            DependencyProperty.Register(nameof(Polygons), typeof(ICollection<Polygon>), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged) });

        public static readonly DependencyProperty OnClickProperty =
            DependencyProperty.Register(nameof(OnClick), typeof(ICommand), typeof(MapControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });


        public ITileServer TileServer
        {
            get => (ITileServer)GetValue(TileServerProperty);
            set => SetValue(TileServerProperty, value);
        }

        public string CacheFolder
        {
            get => (string)GetValue(CacheFolderProperty);
            set => SetValue(CacheFolderProperty, value);
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

        public Color ErrorColor
        {
            get => (Color)GetValue(ErrorColorProperty);
            set => SetValue(ErrorColorProperty, value);
        }

        public ICollection<Marker> Markers
        {
            get => (ICollection<Marker>)GetValue(MarkersProperty);
            set => SetValue(MarkersProperty, value);
        }

        public ICollection<Track> Tracks
        {
            get => (ICollection<Track>)GetValue(TracksProperty);
            set => SetValue(TracksProperty, value);
        }

        public ICollection<Polygon> Polygons
        {
            get => (ICollection<Polygon>)GetValue(PolygonsProperty);
            set => SetValue(PolygonsProperty, value);
        }

        public ICommand OnClick
        {
            get => (ICommand)GetValue(OnClickProperty);
            set => SetValue(OnClickProperty, value);
        }

        private static void DependencyPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            typeof(System.Windows.Forms.MapControl).GetProperty(e.Property.Name).SetValue((sender as MapControl).mapControl, e.NewValue);
        }

        private System.Windows.Forms.MapControl mapControl = new System.Windows.Forms.MapControl();

        public MapControl()
        {
            mapControl.MouseLeave += (s, e) => Focus();
            mapControl.MouseEnter += (s, e) => mapControl.Focus();
            mapControl.MouseDown += (s, e) => mapControl.Focus();
            mapControl.MouseUp += (s, e) => mapControl.Focus();
            mapControl.MouseDoubleClick += (s, e) => { if (e.Button == MouseButtons.Left) OnClick?.Execute(null); };

            mapControl.TileServerChanged += (s, e) => TileServer = mapControl.TileServer;
            mapControl.CenterChanged += (s, e) => Center = mapControl.Center;
            mapControl.MouseChanged += (s, e) => Mouse = mapControl.Mouse;
            mapControl.BackColorChanged += (s, e) => BackColor = mapControl.BackColor;
            mapControl.ForeColorChanged += (s, e) => ForeColor = mapControl.ForeColor;

            Child = mapControl;
            mapControl.Focus();
        }
    }
}
