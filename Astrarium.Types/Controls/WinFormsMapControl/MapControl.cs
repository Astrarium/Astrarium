using Astrarium.Types;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    /// <summary>
    /// Map control for displaying online and offline maps.
    /// </summary>
    [DesignerCategory("code")]
    public partial class MapControl : Control
    {
        /// <summary>
        /// Tile size, in pixels.
        /// </summary>
        private const int TILE_SIZE = 256;

        /// <summary>
        /// Last known mouse position.
        /// </summary>
        private Drawing.Point _LastMouse = new Drawing.Point();

        /// <summary>
        /// Flag indicating the map is dragging by mouse
        /// </summary>
        private bool _IsDragging = false;

        /// <summary>
        /// Cache used to store tile images in memory.
        /// </summary>
        private ConcurrentBag<Tile> _Cache = new ConcurrentBag<Tile>();

        /// <summary>
        /// Pool of tiles to be requested from the server.
        /// </summary>
        private ConcurrentBag<Tile> _RequestPool = new ConcurrentBag<Tile>();

        /// <summary>
        /// Worker thread to process tile requests to the server.
        /// </summary>
        private Thread _Worker = null;

        /// <summary>
        /// Event handle to stop/resume requests processing.
        /// </summary>
        private EventWaitHandle _WorkerWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        /// <summary>
        /// String format to draw text aligned to center.
        /// </summary>
        private readonly StringFormat _AlignCenterStringFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        /// <summary>
        /// Gets size of map in tiles.
        /// </summary>
        private int FullMapSizeInTiles => 1 << ZoomLevel;

        /// <summary>
        /// Gets maps size in pixels.
        /// </summary>
        private int FullMapSizeInPixels => FullMapSizeInTiles * TILE_SIZE;

        /// <summary>
        /// Backing field for <see cref="ZoomLevel"/> property.
        /// </summary>
        private int _ZoomLevel = 0;

        /// <summary>
        /// Map zoom level.
        /// </summary>
        [Description("Map zoom level"), Category("Behavior")]
        public int ZoomLevel
        {
            get => _ZoomLevel;
            set
            {
                if (value < 0 || value > 19)
                    throw new ArgumentException($"{value} is an incorrect value for {nameof(ZoomLevel)} property. Value should be in range from 0 to 19.");

                SetZoomLevel(value, new Drawing.Point(Width / 2, Height / 2));
                CenterChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Backing field for <see cref="OverlayOpacity"/> property.
        /// </summary>
        private float _OverlayOpacity = 0.5f;

        /// <summary>
        /// Map zoom level.
        /// </summary>
        [Description("Overlay opacity"), Category("Behavior")]
        public float OverlayOpacity
        {
            get => _OverlayOpacity;
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentException($"{value} is an incorrect value for {nameof(OverlayOpacity)} property. Value should be in range from 0 to 1.");

                _OverlayOpacity = value;
                OverlayOpacityChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        /// <summary>
        /// Backing field for <see cref="CacheFolder"/> property.
        /// </summary>
        private string _CacheFolder = null;

        /// <summary>
        /// Path to tile cache folder. Should be set if tile server supports file system caching.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CacheFolder
        {
            get => _CacheFolder;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException($"{nameof(CacheFolder)} property value should not be empty.");

                if (value.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                    throw new ArgumentException($"{value} is an incorrect value for {nameof(CacheFolder)} property.");

                _CacheFolder = value;
            }
        }

        /// <summary>
        /// Backing field for <see cref="MinZoomLevel"/> property.
        /// </summary>
        private int _MinZoomLevel = 0;

        /// <summary>
        /// Gets or sets minimal zoom level.
        /// </summary>
        [Description("Minimal zoom level"), Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int MinZoomLevel
        {
            get => _MinZoomLevel;
            set
            {
                if (value < 0 || value > 19)
                    throw new ArgumentException($"{value} is an incorrect value for {nameof(MinZoomLevel)} property. Value should be in range from 0 to 19.");

                _MinZoomLevel = value;

                if (ZoomLevel < _MinZoomLevel)
                    ZoomLevel = _MinZoomLevel;
                
                Invalidate();
            }
        }

        /// <summary>
        /// Backing field for <see cref="MaxZoomLevel"/> property.
        /// </summary>
        private int _MaxZoomLevel = 19;

        /// <summary>
        /// Gets or sets maximal zoom level.
        /// </summary>
        [Description("Maximal zoom level"), Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int MaxZoomLevel
        {
            get => _MaxZoomLevel;
            set
            {
                if (value < 0 || value > 19)
                    throw new ArgumentException($"{value} is an incorrect value for {nameof(MaxZoomLevel)} property. Value should be in range from 0 to 19.");

                _MaxZoomLevel = value;
                Invalidate();
            }
        }


        private ObservableCollection<Layer> _Layers = new ObservableCollection<Layer>();

        /// <summary>
        /// Gets or sets collection of layers to be displayed on the map.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObservableCollection<Layer> Layers
        {
            get => _Layers;
            set
            {
                if (value != _Layers)
                {
                    _Layers.CollectionChanged -= OverlayItemsChanged;
                    _Layers = value;
                    if (value != null)
                    {
                        _Layers.CollectionChanged += OverlayItemsChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets tile server instance used to obtain map tiles.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ITileServer TileServer
        {
            get => Layers[0].TileServer;
            set
            {
                var center = Center;

                Layers[0].TileServer = value;

                if (value != null)
                {
                    _Cache = new ConcurrentBag<Tile>();

                    Center = center;

                    if (TileServer.AttributionText != null)
                    {
                        OnSizeChanged(new EventArgs());
                    }

                    if (ZoomLevel > TileServer.MaxZoomLevel)
                        ZoomLevel = TileServer.MaxZoomLevel;

                    if (ZoomLevel < TileServer.MinZoomLevel)
                        ZoomLevel = TileServer.MinZoomLevel;

                    MaxZoomLevel = TileServer.MaxZoomLevel;
                    MinZoomLevel = TileServer.MinZoomLevel;
                }

                Invalidate();
                TileServerChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets tile server instance used to obtain overlay tiles.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ITileServer OverlayTileServer
        {
            get => Layers[1].TileServer;
            set
            {
                Layers[1].TileServer = value;

                if (value != null)
                {
                    _Cache = new ConcurrentBag<Tile>();

                    if (Layers[1].TileServer.AttributionText != null)
                    {
                        OnSizeChanged(new EventArgs());
                    }

                    if (ZoomLevel > OverlayTileServer.MaxZoomLevel)
                        ZoomLevel = OverlayTileServer.MaxZoomLevel;

                    if (ZoomLevel < OverlayTileServer.MinZoomLevel)
                        ZoomLevel = OverlayTileServer.MinZoomLevel;

                    MaxZoomLevel = OverlayTileServer.MaxZoomLevel;
                    MinZoomLevel = OverlayTileServer.MinZoomLevel;
                }

                Invalidate();
                OverlayTileServerChanged?.Invoke(this, EventArgs.Empty);
            }
        }


        /// <summary>
        /// Gets or sets geographical coordinates of the map center.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GeoPoint Center
        {
            get
            {
                if (Layers[0].TileServer != null)
                {
                    float x = NormalizeTileNumber(-(float)(Layers[0].OffsetX - Width / 2) / TILE_SIZE);
                    float y = -(float)(Layers[0].OffsetY - Height / 2) / TILE_SIZE;
                    return Layers[0].TileServer.Projection.TileToWorldPos(x, y, ZoomLevel);
                }
                else
                {
                    return new GeoPoint();
                }
            }
            set
            {
                if (Layers[0].TileServer != null)
                {
                    foreach (var layer in Layers.Where(n => n.TileServer != null))
                    {
                        var center = layer.TileServer.Projection.WorldToTilePos(value, ZoomLevel);
                        layer.OffsetX = -(int)(center.X * TILE_SIZE) + Width / 2;
                        layer.OffsetY = -(int)(center.Y * TILE_SIZE) + Height / 2;
                        layer.OffsetX = layer.OffsetX % FullMapSizeInPixels;
                    }

                    Invalidate();
                    CenterChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets geographical coordinates of the current position of mouse.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GeoPoint Mouse
        {
            get
            {
                float x = NormalizeTileNumber(-(float)(Layers[0].OffsetX - _LastMouse.X) / TILE_SIZE);
                float y = -(float)(Layers[0].OffsetY - _LastMouse.Y) / TILE_SIZE;
                return Layers[0].TileServer.Projection.TileToWorldPos(x, y, ZoomLevel);
            }
        }

        /// <summary>
        /// Backing field for <see cref="Markers"/> property.
        /// </summary>
        private ObservableCollection<Marker> _Markers = new ObservableCollection<Marker>();

        /// <summary>
        /// Gets or sets collection of markers to be displayed on the map.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObservableCollection<Marker> Markers
        {
            get => _Markers;
            set
            {
                if (value != _Markers)
                {
                    _Markers.CollectionChanged -= OverlayItemsChanged;
                    _Markers = value;
                    if (value != null)
                    {
                        _Markers.CollectionChanged += OverlayItemsChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Backing field for <see cref="Tracks"/> property.
        /// </summary>
        private ObservableCollection<Track> _Tracks = new ObservableCollection<Track>();

        /// <summary>
        /// Gets collection of tracks to be displayed on the map.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObservableCollection<Track> Tracks
        {
            get => _Tracks;
            set
            {
                if (value != _Tracks)
                {
                    _Tracks.CollectionChanged -= OverlayItemsChanged;
                    _Tracks = value;
                    if (value != null)
                    {
                        _Tracks.CollectionChanged += OverlayItemsChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Backing field for <see cref="Polygons"/> property.
        /// </summary>
        private ObservableCollection<Polygon> _Polygons = new ObservableCollection<Polygon>();

        /// <summary>
        /// Gets collection of polygons to be displayed on the map.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObservableCollection<Polygon> Polygons
        {
            get => _Polygons;
            set
            {
                if (value != _Polygons)
                {
                    _Polygons.CollectionChanged -= OverlayItemsChanged;
                    _Polygons = value;
                    if (value != null)
                    {
                        _Polygons.CollectionChanged += OverlayItemsChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Backing field for <see cref="FitToBounds"/> property.
        /// </summary>
        private bool _FitToBounds = true;

        /// <summary>
        /// Gets or sets value indicating should the map fit to vertical bounds of the control or not.
        /// </summary>
        [Description("Value indicating should the map fit to vertical bounds of the control or not."), Category("Behavior")]
        public bool FitToBounds
        {
            get => _FitToBounds;
            set
            {
                _FitToBounds = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Flag indicating the map is in the update state, 
        /// and painting events should be suppressed.
        /// </summary>
        private bool _IsUpdating = false;

        /// <summary>
        /// Maintains the map control performance by preventing the control from drawing 
        /// until the <see cref="EndUpdate"/> method is called.
        /// </summary>
        public void BeginUpdate()
        {
            _IsUpdating = true;
        }

        /// <summary>
        /// Resumes painting the map control after painting is suspended 
        /// by the <see cref="BeginUpdate"/> method.
        /// </summary>
        public void EndUpdate()
        {
            _IsUpdating = false;
            Invalidate();
        }       

        /// <summary>
        /// Gets or sets color used to draw error messages.
        /// </summary>
        [Description("Color used to draw error messages."), Category("Appearance")]
        public Color ErrorColor { get; set; } = Color.Red;

        /// <summary>
        /// Gets or sets text to be displayed instead of tile when it is being downloaded.
        /// </summary>
        [Description("Text to be displayed instead of tile when it is being downloaded."), Category("Appearance")]
        public string ThumbnailText { get; set; } = "Downloading...";

        /// <summary>
        /// Gets or sets color of the text to be displayed instead of tile when it is being downloaded.
        /// </summary>
        [Description("Color of the tile thumbnail text."), Category("Appearance")]
        public Color ThumbnailForeColor { get; set; } = Color.FromArgb(0xB0, 0xB0, 0xB0);

        /// <summary>
        /// Gets or sets backgound of the thumbnail to be displayed when a tile is being downloaded.
        /// </summary>
        [Description("Color of the tile thumbnail background."), Category("Appearance")]
        public Color ThumbnailBackColor { get; set; } = Color.FromArgb(0xE0, 0xE0, 0xE0);

        /// <summary>
        /// Gets or sets flag indicating show thumbnails while downloading tile images or not.
        /// </summary>
        [Description("Show thumbnails while downloading tile images."), Category("Behavior")]
        public bool ShowThumbnails { get; set; } = true;

        /// <summary>
        /// Gets or sets the foreground color of the map.
        /// </summary>
        [Description("Gets or sets the foreground color of the map."), Category("Appearance")]
        public override Color ForeColor
        {
            get => base.ForeColor;
            set
            {
                base.ForeColor = value;
            }
        }

        /// <summary>
        /// Image attributes to be applied to a tile image when drawing.
        /// </summary>
        [Description("Image attributes to be applied to a tile image when drawing."), Category("Appearance")]
        public ImageAttributes TileImageAttributes { get; set; } = null;

        /// <summary>
        /// Raised when marker is drawn on the map.
        /// </summary>
        public event EventHandler<DrawMarkerEventArgs> DrawMarker;

        /// <summary>
        /// Raised when track is drawn on the map.
        /// </summary>
        public event EventHandler<DrawTrackEventArgs> DrawTrack;

        /// <summary>
        /// Raised when polygon is drawn on the map.
        /// </summary>
        public event EventHandler<DrawPolygonEventArgs> DrawPolygon;

        /// <summary>
        /// Raised when <see cref="Center"/> property value is changed.
        /// </summary>
        public event EventHandler<EventArgs> CenterChanged;

        /// <summary>
        /// Raised when <see cref="OverlayOpacity"/> property value is changed.
        /// </summary>
        public event EventHandler<EventArgs> OverlayOpacityChanged;

        /// <summary>
        /// Raised when <see cref="Mouse"/> property value is changed.
        /// </summary>
        public event EventHandler<EventArgs> MouseChanged;

        /// <summary>
        /// Raised when <see cref="ZoomLevel"/> property value is changed.
        /// </summary>
        public event EventHandler<EventArgs> ZoomLevelChaged;

        /// <summary>
        /// Raised when <see cref="TileServer"/> property value is changed.
        /// </summary>
        public event EventHandler<EventArgs> TileServerChanged;

        /// <summary>
        /// Raised when <see cref="OverlayTileServer"/> property value is changed.
        /// </summary>
        public event EventHandler<EventArgs> OverlayTileServerChanged;

        /// <summary>
        /// Creates new <see cref="MapControl"/> control.
        /// </summary>
        public MapControl()
        {
            InitializeComponent();
            DoubleBuffered = true;
            Cursor = Cursors.Cross;
            Layers.Add(new Layer() { ZIndex = 0 });
            Layers.Add(new Layer() { ZIndex = 1 });
        }

        public static ICollection<ITileServer> CreateTileServers(string userAgent)
        {
            return new ITileServer[]
            {
                new OfflineTileServer(),
                new OpenStreetMapTileServer(userAgent),
                //new StamenTerrainTileServer(),
                new OpenTopoMapServer(userAgent),
                new WikimapiaTileServer(userAgent),
                new EsriSatelliteMapsTileServer(userAgent),
                //new DoubleGisTileServer(userAgent),
                new GoogleMapsSatelliteTileServer(userAgent),
                new GoogleMapsRoadmapTileServer(userAgent),
                new GoogleMapsHybridTileServer(userAgent),
                new BingMapsAerialTileServer(),
                new BingMapsRoadsTileServer(),
                new BingMapsHybridTileServer(),
                //new YandexRoadMapsTileServer(userAgent),
                //new YandexSatelliteMapsTileServer(userAgent)
            };
        }

        public static ICollection<ITileServer> CreateOverlayServers(string userAgent)
        {
            return new ITileServer[]
            {
                new LightPollutionWA2015TileServer(userAgent),
                new LightPollutionVIIRS2021TileServer(userAgent),
                new LightPollutionVIIRS2020TileServer(userAgent),
                new LightPollutionVIIRS2019TileServer(userAgent),
                new LightPollutionVIIRS2018TileServer(userAgent),
                new LightPollutionVIIRS2017TileServer(userAgent),
                new LightPollutionVIIRS2016TileServer(userAgent),
                new LightPollutionVIIRS2015TileServer(userAgent),
                new LightPollutionVIIRS2014TileServer(userAgent),
                new LightPollutionVIIRS2013TileServer(userAgent),
                new LightPollutionVIIRS2012TileServer(userAgent),
            };
        }

        /// <summary>
        /// Raised when overlay items (markers, tracks, polygons) changed.
        /// </summary>
        private void OverlayItemsChanged(object sender, Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Invalidate();
        }

        /// <summary>
        /// Handles clicks on LinkLabel links
        /// </summary>
        private void _LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString());
        }

        /// <summary>
        /// Does painting of the map.
        /// </summary>
        /// <param name="pe">Paint event args.</param>
        protected override void OnPaint(PaintEventArgs pe)
        {
            bool drawContent = true;

            if (!DesignMode)
            {
                if (CacheFolder == null && TileServer is IFileCacheTileServer)
                {
                    drawContent = false;
                    DrawErrorString(pe.Graphics, $"{nameof(CacheFolder)} property value is not set.\nIt should be specified if you are using tile server which supports file system cache.");
                }
                else if (TileServer == null)
                {
                    drawContent = false;
                    DrawErrorString(pe.Graphics, $"{nameof(TileServer)} property value is not set.\nPlease specify tile server instance to obtain map images before using the map control.");
                }
            }
            // design mode
            else
            {
                drawContent = false;
                DrawErrorString(pe.Graphics, $"Map is unavailable in design mode.");
            }

            if (drawContent)
            {
                try
                {
                    pe.Graphics.SmoothingMode = SmoothingMode.None;
                    DrawTiles(pe.Graphics);
                    pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    if (!_IsUpdating)
                    {
                        DrawPolygons(pe.Graphics);
                        DrawTracks(pe.Graphics);
                        DrawMarkers(pe.Graphics);
                    }

                    var gs = pe.Graphics.Save();
                    pe.Graphics.SmoothingMode = SmoothingMode.None;
                    if (_Layers[0].OffsetY > 0)
                    {
                        pe.Graphics.FillRectangle(new SolidBrush(BackColor), 0, -1, Width, Layers[0].OffsetY + 1);
                    }
                    if (Layers[0].OffsetY + FullMapSizeInPixels < Height)
                    {
                        pe.Graphics.FillRectangle(new SolidBrush(BackColor), 0, Layers[0].OffsetY + FullMapSizeInPixels, Width, Layers[0].OffsetY + FullMapSizeInPixels);
                    }
                    pe.Graphics.Restore(gs);

                }
                catch (Exception ex)
                {
                    if (!(ex is InvalidOperationException))
                    {
                        DrawErrorString(pe.Graphics, $"Drawing error:\n{ex}");
                    }
                }
            }

            _WorkerWaitHandle.Reset();

            base.OnPaint(pe);
        }

        /// <summary>
        /// Called when control size is changed.
        /// </summary>
        /// <param name="e">Event args.</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            AdjustMapBounds();
            Invalidate();
            //CenterChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when mouse is down.
        /// </summary>
        /// <param name="e">Event args.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _IsDragging = true;
                _LastMouse.X = e.X;
                _LastMouse.Y = e.Y;
            }
            base.OnMouseDown(e);
        }

        /// <summary>
        /// Called when mouse is moving.
        /// </summary>
        /// <param name="e">Event args.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_IsDragging)
            {
                Layers[0].OffsetX += e.X - _LastMouse.X;
                Layers[0].OffsetY += e.Y - _LastMouse.Y;

                Layers[0].OffsetX = Layers[0].OffsetX % FullMapSizeInPixels;

                if (Layers[0].OffsetY < -FullMapSizeInPixels)
                    Layers[0].OffsetY = -FullMapSizeInPixels;

                if (Layers[0].OffsetY > Height)
                    Layers[0].OffsetY = Height;

                AdjustMapBounds();
                Invalidate();
                CenterChanged?.Invoke(this, EventArgs.Empty);
            }

            _LastMouse.X = e.X;
            _LastMouse.Y = e.Y;

            MouseChanged?.Invoke(this, EventArgs.Empty);

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _IsDragging = false;
            base.OnMouseUp(e);
        }

        /// <summary>
        /// Called when mouse is wheeling.
        /// </summary>
        /// <param name="e">Event args.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int z = ZoomLevel;

            if (e.Delta >= 120)
                z = ZoomLevel + 1;
            else if (e.Delta <= -120)
                z = ZoomLevel - 1;

            SetZoomLevel(z, new Drawing.Point(e.X, e.Y));
            AdjustMapBounds();
            base.OnMouseWheel(e);
            CenterChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Adjusts map bounds if required.
        /// </summary>
        private void AdjustMapBounds()
        {
            if (FitToBounds)
            {
                if (FullMapSizeInPixels > Height)
                {
                    if (Layers[0].OffsetY > 0) Layers[0].OffsetY = 0;
                    if (Layers[0].OffsetY + FullMapSizeInPixels < Height) Layers[0].OffsetY = Height - FullMapSizeInPixels;
                }
                else
                {
                    if (Layers[0].OffsetY > Height - FullMapSizeInPixels) Layers[0].OffsetY = Height - FullMapSizeInPixels;
                    if (Layers[0].OffsetY < 0) Layers[0].OffsetY = 0;
                }
            }
        }

        /// <summary>
        /// Normalizes tile number to fit value from 0 to FullMapSizeInTiles.
        /// </summary>
        /// <param name="n">Tile number, with fractions.</param>
        /// <returns>Tile number in range from 0 to FullMapSizeInTiles.</returns>
        private float NormalizeTileNumber(float n)
        {
            int size = FullMapSizeInTiles;
            return (n %= size) >= 0 ? n : (n + size);
        }

        /// <summary>
        /// Draws error string on the map.
        /// </summary>
        /// <param name="gr">Graphics instance to draw on.</param>
        /// <param name="text">Error string to be drawn.</param>
        private void DrawErrorString(Graphics gr, string text)
        {
            gr.DrawString(text, Font, new SolidBrush(ErrorColor), new Drawing.Point(Width / 2, Height / 2), _AlignCenterStringFormat);
        }

        private IEnumerable<ITileServer> TileServers
        {
            get
            {
                yield return TileServer;
                if (OverlayTileServer != null) yield return OverlayTileServer;
            }
        }

        /// <summary>
        /// Draws map tiles.
        /// </summary>
        /// <param name="g">Graphics instance to draw on.</param>
        private void DrawTiles(Graphics g)
        {
            // count of visible tiles (vertically and horizontally)
            int tilesByWidth = (int)Math.Ceiling((float)Width / TILE_SIZE);
            int tilesByHeight = (int)Math.Ceiling((float)Height / TILE_SIZE);

            // flush used flag for all memory-cached tiles
            foreach (var c in _Cache)
            {
                c.Used = false;
            }

            foreach (var layer in Layers.OrderBy(n => n.ZIndex).Where(n => n.TileServer != null))
            {
                ITileServer tileServer = layer.TileServer;

                bool isOverlay = tileServer is IOverlayTileServer;

                // indices of first visible tile
                int fromX = (int)Math.Floor(-(float)layer.OffsetX / TILE_SIZE);
                int fromY = (int)Math.Floor(-(float)layer.OffsetY / TILE_SIZE);

                // indices of last visible tile
                int toX = fromX + tilesByWidth;
                int toY = fromY + tilesByHeight;

                for (int x = fromX; x <= toX; x++)
                {
                    for (int y = fromY; y <= toY; y++)
                    {
                        int x_ = (int)NormalizeTileNumber(x);
                        if (y >= 0 && y < FullMapSizeInTiles)
                        {
                            Tile tile = GetTile(tileServer, x_, y, ZoomLevel);

                            // tile for current zoom and position found
                            if (tile != null)
                            {
                                if (tile.Image != null)
                                {
                                    tile.Used = true;
                                    DrawTile(g, x, y, tile.Image, layer, isOverlay);
                                }
                                else
                                {
                                    tile.Used = true;
                                    if (!isOverlay)
                                    {
                                        DrawThumbnail(g, layer, x, y, tile.ErrorMessage, true);
                                    }
                                }
                            }
                            // tile not found, do some magic
                            else
                            {
                                // draw thumbnail first
                                if (!isOverlay)
                                {
                                    DrawThumbnail(g, layer, x, y, ThumbnailText, false);
                                }

                                // try to find out tile with less zoom level, and draw scaled part of that tile

                                int z = 1;
                                while (ZoomLevel - z >= 0)
                                {
                                    // fraction of a tile to be drawn (1/2, 1/4 and etc.)
                                    int f = 1 << z;

                                    //  try to get tile with less zoom level from cache
                                    tile = GetTile(tileServer, x_ / f, y / f, ZoomLevel - z, fromCacheOnly: true);

                                    // if tile found, draw part of it
                                    if (tile != null && tile.Image != null)
                                    {
                                        tile.Used = true;
                                        if (!isOverlay)
                                        {
                                            DrawTilePart(g, x, y, x_ % f, y % f, f, tile.Image, layer, isOverlay);
                                        }
                                        break;
                                    }

                                    // move up to less zoom level
                                    z++;
                                }
                            }
                        }
                    }
                }
            }

            // Dispose images that were not used 
            _Cache.Where(c => !c.Used).ToList().ForEach(c => c.Image?.Dispose());

            // Update cache, leave only used images
            _Cache = new ConcurrentBag<Tile>(_Cache.Where(c => c.Used));
        }

        /// <summary>
        /// Draws markers on the map
        /// </summary>
        /// <param name="gr">Graphics instance to draw on.</param>
        private void DrawMarkers(Graphics gr)
        {
            foreach (Marker m in Markers)
            {
                var p = Project(m.Point);
                Draw(gr, () =>
                {
                    var eventArgs = new DrawMarkerEventArgs()
                    {
                        Graphics = gr,
                        Marker = m,
                        Point = p
                    };

                    DrawMarker?.Invoke(this, eventArgs);

                    if (gr.IsVisible(p))
                    {
                        if (!eventArgs.Handled)
                        {
                            if (m.Style.MarkerBrush != null)
                            {
                                gr.FillEllipse(m.Style.MarkerBrush, p.X - m.Style.MarkerWidth / 2, p.Y - m.Style.MarkerWidth / 2, m.Style.MarkerWidth, m.Style.MarkerWidth);
                            }
                            if (m.Style.MarkerPen != null)
                            {
                                gr.DrawEllipse(m.Style.MarkerPen, p.X - m.Style.MarkerWidth / 2, p.Y - m.Style.MarkerWidth / 2, m.Style.MarkerWidth, m.Style.MarkerWidth);
                            }
                            if (m.Style.LabelFont != null && m.Style.LabelBrush != null && m.Style.LabelFormat != null)
                            {
                                if ((m.MinZoomToDisplayLabel == 0 || ZoomLevel >= m.MinZoomToDisplayLabel) &&
                                    (m.MaxZoomToDisplayLabel == 0 || ZoomLevel <= m.MaxZoomToDisplayLabel))
                                {
                                    gr.DrawString(m.Label, m.Style.LabelFont, m.Style.LabelBrush, new PointF(p.X + m.Style.MarkerWidth * 0.35f, p.Y + m.Style.MarkerWidth * 0.35f), m.Style.LabelFormat);
                                }
                            }
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Draws tracks on the map
        /// </summary>
        /// <param name="gr">Graphics instance to draw on.</param>
        private void DrawTracks(Graphics gr)
        {
            foreach (var track in Tracks)
            {
                PointF[] points = new PointF[track.Count];

                for (int i = 0; i < track.Count; i++)
                {
                    GeoPoint g = track.ElementAt(i);
                    PointF p = Project(g);
                    if (i > 0)
                    {
                        p = points[i - 1].Nearest(p, new PointF(p.X - FullMapSizeInPixels, p.Y), new PointF(p.X + FullMapSizeInPixels, p.Y));
                    }
                    points[i] = p;
                }

                var eventArgs = new DrawTrackEventArgs()
                {
                    Graphics = gr,
                    Track = track,
                    Points = points
                };

                DrawTrack?.Invoke(this, eventArgs);
                if (!eventArgs.Handled)
                {
                    //if (ZoomLevel < 3)
                    //{
                        if (track.Style.Pen != null && points.Length > 1)
                        {
                            Draw(gr, () => gr.DrawLines(track.Style.Pen, points));
                        }
                    //}
                    //else
                    //{
                    //    Draw(gr, () => gr.DrawPolyline(track.Style.Pen, points));
                    //}
                }
            }
        }

        /// <summary>
        /// Draws polygons on the map
        /// </summary>
        /// <param name="gr">Graphics instance to draw on.</param>
        private void DrawPolygons(Graphics gr)
        {
            foreach (var polygon in Polygons)
            {
                PointF p0 = PointF.Empty;
                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.StartFigure();
                    for (int i = 0; i < polygon.Count; i++)
                    {
                        GeoPoint g = polygon.ElementAt(i);
                        PointF p = Project(g);
                        if (i > 0)
                        {
                            p = p0.Nearest(p, new PointF(p.X - FullMapSizeInPixels, p.Y), new PointF(p.X + FullMapSizeInPixels, p.Y));
                            gp.AddLine(p0, p);
                        }

                        p0 = p;
                    }

                    gp.CloseFigure();
                    gp.Flatten();

                    var eventArgs = new DrawPolygonEventArgs()
                    {
                        Graphics = gr,
                        Polygon = polygon,
                        Path = gp
                    };

                    DrawPolygon?.Invoke(this, eventArgs);
                    if (!eventArgs.Handled)
                    {
                        if (ZoomLevel < 3)
                        {
                            Draw(gr, () =>
                            {
                                if (polygon.Style.Brush != null)
                                {
                                    gr.FillPath(polygon.Style.Brush, gp);
                                }
                                if (polygon.Style.Pen != null)
                                {
                                    gr.DrawPath(polygon.Style.Pen, gp);
                                }
                            });
                        }
                        else
                        {
                            Draw(gr, () => gr.DrawGraphicsPath(gp, polygon.Style.Brush, polygon.Style.Pen));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws part of a tile.
        /// This method is needed to draw portion of a tile with highest zoom level if a tile with smallest zoom is not ready yet.
        /// </summary>
        /// <param name="gr">Graphics instance to draw on.</param>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index of the tile.</param>
        /// <param name="xRemainder">X-index of a tile portion to be drawn.</param>
        /// <param name="yRemainder">Y-index of a tile portion to be drawn.</param>
        /// <param name="frac">Portion of a tile to be drawn, 2 means 1/2, 4 means 1/4 etc.</param>
        /// <param name="image">Full tile image.</param>
        private void DrawTilePart(Graphics gr, int x, int y, int xRemainder, int yRemainder, int frac, Image image, Layer layer, bool isOverlay)
        {
            // coordinates of a tile on the map
            var p = new Drawing.Point();
            p.X = layer.OffsetX + x * TILE_SIZE;
            p.Y = layer.OffsetY + y * TILE_SIZE;

            // Calc source portion of the tile
            Rectangle srcRect = new Rectangle(TILE_SIZE / frac * xRemainder, TILE_SIZE / frac * yRemainder, TILE_SIZE / frac, TILE_SIZE / frac);

            // Destination rectangle
            Rectangle destRect = new Rectangle(p.X - frac, p.Y - frac, TILE_SIZE + 2 * frac, TILE_SIZE + 2 * frac);

            var state = gr.Save();
            gr.SmoothingMode = SmoothingMode.HighSpeed;
            gr.InterpolationMode = InterpolationMode.NearestNeighbor;
            gr.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            gr.CompositingQuality = CompositingQuality.HighSpeed;

            gr.DrawImage(image, destRect, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, GraphicsUnit.Pixel, GetImageAttributes(isOverlay));

            gr.Restore(state);
        }

        public ImageAttributes GetImageAttributes(bool isOverlay)
        {
            if (isOverlay)
            {
                var attrs = (ImageAttributes)TileImageAttributes?.Clone() ?? new ImageAttributes();

                // create a color matrix object  
                ColorMatrix matrix = new ColorMatrix();

                // set the opacity  
                matrix.Matrix33 = OverlayOpacity;

                // set the color( opacity) of the image  
                attrs.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                return attrs;
            }
            else
            {
                return TileImageAttributes;
            }
        }

        /// <summary>
        /// Draws a tile on the map.
        /// </summary>
        /// <param name="gr">Graphics instance to draw on.</param>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index of the tile.</param>
        /// <param name="image">Tile image.</param>
        private void DrawTile(Graphics gr, int x, int y, Image image, Layer layer, bool isOverlay)
        {
            var p = new Drawing.Point();
            p.X = layer.OffsetX + x * TILE_SIZE;
            p.Y = layer.OffsetY + y * TILE_SIZE;

            gr.DrawImage(image, new Rectangle(p, new Drawing.Size(TILE_SIZE, TILE_SIZE)), 0, 0, TILE_SIZE, TILE_SIZE, GraphicsUnit.Pixel, GetImageAttributes(isOverlay));
        }

        /// <summary>
        /// Draws thumbnail frame and text instead of a tile.
        /// </summary>
        /// <param name="gr">Graphics instance to draw on.</param>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index of the tile.</param>
        /// <param name="message">Message to be displayed instead of the tile.</param>
        /// <param name="isError">Flag indicating the message should be displayed with error color.</param>
        private void DrawThumbnail(Graphics gr, Layer layer, int x, int y, string message, bool isError)
        {
            if (ShowThumbnails || isError)
            {
                var p = new Drawing.Point();
                p.X = layer.OffsetX + x * TILE_SIZE;
                p.Y = layer.OffsetY + y * TILE_SIZE;
                Rectangle rectangle = new Rectangle(p.X, p.Y, TILE_SIZE, TILE_SIZE);
                gr.FillRectangle(new SolidBrush(ThumbnailBackColor), rectangle);
                gr.DrawRectangle(new Pen(ThumbnailForeColor) { DashStyle = DashStyle.Dot }, rectangle);
                gr.DrawString(message, Font, new SolidBrush(isError ? ErrorColor : ThumbnailForeColor), rectangle, _AlignCenterStringFormat);
            }
        }

        /// <summary>
        /// Does the draw action.
        /// The method is needed for repeating drawing because map is infinite in longitude.
        /// </summary>
        /// <param name="gr">Graphics instance to draw on.</param>
        /// <param name="draw">Draw action to perform several times for all visible width of the map.</param>
        private void Draw(Graphics gr, Action draw)
        {
            int count = (int)Math.Ceiling((double)Width / FullMapSizeInPixels) + 1;
            for (int i = -count; i < count; i++)
            {
                var state = gr.Save();
                gr.TranslateTransform(i * FullMapSizeInPixels, 0);
                draw();
                gr.Restore(state);
            }
        }

        /// <summary>
        /// Sets zoom level with specifying central point to zoom in/out.
        /// </summary>
        /// <param name="z">Zoom level to be set.</param>
        /// <param name="p">Central point to zoom in/out.</param>
        private void SetZoomLevel(int z, Drawing.Point p)
        {
            int max = TileServer != null ? Math.Min(MaxZoomLevel, TileServer.MaxZoomLevel) : MaxZoomLevel;
            int min = TileServer != null ? Math.Max(MinZoomLevel, TileServer.MinZoomLevel) : MinZoomLevel;

            if (OverlayTileServer != null)
                max = Math.Min(max, OverlayTileServer.MaxZoomLevel);

            if (OverlayTileServer != null)
                min = Math.Max(min, OverlayTileServer.MinZoomLevel);

            if (z < min) z = min;
            if (z > max) z = max;

            if (z != ZoomLevel)
            {
                double factor = Math.Pow(2, z - ZoomLevel);

                _ZoomLevel = z;

                foreach (var layer in Layers)
                {
                    layer.OffsetX = (int)((layer.OffsetX - p.X) * factor) + p.X;
                    layer.OffsetY = (int)((layer.OffsetY - p.Y) * factor) + p.Y;

                    layer.OffsetX = layer.OffsetX % FullMapSizeInPixels;
                }
                Invalidate();

                ZoomLevelChaged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets tile image by X and Y indices and zoom level.
        /// </summary>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index of the tile.</param>
        /// <param name="z">Zoom level.</param>
        /// <param name="fromCacheOnly">Flag indicating the tile can be fetched from cache only (server request is not allowed).</param>
        /// <returns><see cref="Tile"/> instance.</returns>
        private Tile GetTile(ITileServer tileServer, int x, int y, int z, bool fromCacheOnly = false)
        {
            try
            {
                Tile tile;

                // try to get tile from memory cache
                tile = _Cache.FirstOrDefault(t => t.X == x && t.Y == y && t.Z == z && t.TileServer == tileServer.GetType().Name);
                if (tile != null)
                {
                    return tile;
                }

                // try to get tile from file system
                if (tileServer is IFileCacheTileServer fcTileServer)
                {
                    string localPath = Path.Combine(CacheFolder, tileServer.GetType().Name, $"{z}", $"{x}", $"{y}.tile");
                    if (File.Exists(localPath))
                    {
                        var fileInfo = new FileInfo(localPath);
                        if (fileInfo.Length > 0 && fileInfo.LastWriteTime + fcTileServer.TileExpirationPeriod >= DateTime.Now)
                        {
                            Image image = Image.FromFile(localPath);
                            tile = new Tile(image, x, y, z, tileServer.GetType().Name);
                            _Cache.Add(tile);
                            return tile;
                        }
                    }
                }

                // get tile from the server
                if (!fromCacheOnly)
                {
                    RequestTile(tileServer, x, y, z);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Does a tile request to the tile server
        /// </summary>
        /// <param name="x">X-index of the tile to be requested.</param>
        /// <param name="y">Y-index of the tile to be requested.</param>
        /// <param name="z">Zoom level</param>
        private void RequestTile(ITileServer tileServer, int x, int y, int z)
        {
            // Intialize worker, if not yet initialized
            if (_Worker == null)
            {
                _Worker = new Thread(new ThreadStart(ProcessRequests));
                _Worker.IsBackground = true;
                _Worker.Start();
            }

            // Check the tile is already requested
            if (!_RequestPool.Any(t => t.X == x && t.Y == y && t.Z == z && t.TileServer == tileServer.GetType().Name))
            {
                _RequestPool.Add(new Tile(x, y, z, tileServer.GetType().Name));
                _WorkerWaitHandle.Set();
            }
        }

        /// <summary>
        /// Processes tile request for the specified tile server
        /// </summary>
        /// <param name="tileServer">Tile server to request the tile</param>
        /// <param name="tile">Tile to be requested</param>
        private void ProcessRequest(Layer layer, Tile tile)
        {
            try
            {
                // indices of first visible tile
                int fromX = (int)Math.Floor(-(float)layer.OffsetX / TILE_SIZE);
                int fromY = (int)Math.Floor(-(float)layer.OffsetY / TILE_SIZE);

                // count of visible tiles (vertically and horizontally)
                int tilesByWidth = (int)Math.Ceiling((float)Width / TILE_SIZE);
                int tilesByHeight = (int)Math.Ceiling((float)Height / TILE_SIZE);

                // indices of last visible tile
                int toX = fromX + tilesByWidth;
                int toY = fromY + tilesByHeight;

                // request tiles matching the current zoom level, server name and position
                if (tile.TileServer == layer.TileServer.GetType().Name &&
                    tile.Z == ZoomLevel &&
                    tile.X >= fromX && tile.X <= toX &&
                    tile.Y >= fromY && tile.Y <= toY)
                {
                    tile.Image = layer.TileServer.GetTile(tile.X, tile.Y, tile.Z);
                    tile.Used = true;
                }
            }
            catch (Exception ex)
            {
                // keep error text to be displayed instead of the tile
                tile.ErrorMessage = ex.Message;
            }
            finally
            {
                // if we have obtained image from the server, save it in file system (if server supports file system cache)
                if (layer.TileServer is IFileCacheTileServer && tile.Image != null)
                {
                    string localPath = Path.Combine(CacheFolder, tile.TileServer, $"{tile.Z}", $"{tile.X}", $"{tile.Y}.tile");
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(localPath));
                        tile.Image.Save(localPath);
                    }
                    catch (Exception ex)
                    {
                        Log.Debug($"Unable to save tile image {localPath}. Reason: {ex.Message}");
                    }
                }

                // add tile to the memory cache
                if (tile.Image != null || tile.ErrorMessage != null)
                {
                    _Cache.Add(tile);
                }

                // remove the tile from requests pool
                _RequestPool.TryTake(out tile);
            }
        }

        /// <summary>
        /// Background worker function. 
        /// Processes tiles requests if requests pool is not empty, 
        /// than stops execution until the pool gets a new image request.
        /// Breaks execution on disposing.
        /// </summary>
        private void ProcessRequests()
        {
            while (!IsDisposed)
            {
                if (_RequestPool.TryPeek(out Tile tile))
                {
                    foreach (var layer in Layers.Where(n => n.TileServer != null))
                    {
                        ProcessRequest(layer, tile);
                    }
                    Invalidate();
                }
                else
                {
                    _WorkerWaitHandle.WaitOne();
                }
            };
        }

        /// <summary>
        /// Gets projection of geographical coordinates onto the map.
        /// </summary>
        /// <param name="g">Point with geographical coordinates.</param>
        /// <returns><see cref="PointF"/> object representing projection of the specified geographical coordinates on the map.</returns>
        private PointF Project(GeoPoint g)
        {
            var p = TileServer.Projection.WorldToTilePos(g, ZoomLevel);
            return new PointF(p.X * TILE_SIZE + Layers[0].OffsetX, p.Y * TILE_SIZE + Layers[0].OffsetY);
        }

        /// <summary>
        /// Clears map cache. If <paramref name="allTileServers"/> flag is set to true, cache for all tile servers will be cleared.
        /// If no, only current tile server cache will be cleared.
        /// </summary>
        /// <param name="allTileServers">If flag is set to true, cache for all tile servers will be cleared.</param>
        public void ClearCache(bool allTileServers = false)
        {
            _Cache = new ConcurrentBag<Tile>();
            _RequestPool = new ConcurrentBag<Tile>();

            if (TileServer != null)
            {
                string cacheFolder = allTileServers ? CacheFolder : Path.Combine(CacheFolder, TileServer.GetType().Name);
                if (Directory.Exists(cacheFolder))
                {
                    if (allTileServers)
                    {
                        var subdirs = Directory.EnumerateDirectories(cacheFolder);
                        foreach (string dir in subdirs)
                        {
                            try
                            {
                                Directory.Delete(dir, true);
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        try
                        {
                            Directory.Delete(cacheFolder, true);
                        }
                        catch { }
                    }
                }
            }
            Invalidate();
        }

        /// <summary>
        /// Removes all markers, tracks and polygons from the map.
        /// </summary>
        public void ClearAll()
        {
            Markers.Clear();
            Tracks.Clear();
            Polygons.Clear();
            Invalidate();
        }
    }
}
