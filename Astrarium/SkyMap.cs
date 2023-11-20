using Astrarium.Algorithms;
using Astrarium.Projections;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium
{
    public class SkyMap : ISkyMap
    {
        public event Action<double> FovChanged;

        /// <summary>
        /// Stopwatch to measure rendering time
        /// </summary>
        private Stopwatch renderStopWatch = new Stopwatch();

        /// <summary>
        /// Collection of bounding rectangles of labels displayed on the map
        /// </summary>
        private ICollection<RectangleF> labels = new List<RectangleF>();

        /// <summary>
        /// Collection of renderers
        /// </summary>
        private readonly RenderersCollection renderers = new RenderersCollection();

        private bool dateTimeSync = false;
        public bool TimeSync
        {
            get => dateTimeSync;
            set
            {
                dateTimeSync = value;

                if (value)
                {
                    timeSyncEvent.Set();
                    timeSyncResetEvent.Set();
                }
                else
                {
                    timeSyncEvent.Set();
                    timeSyncResetEvent.Reset();
                }
            }
        }

        public float DaylightFactor { get; set; }
        public bool Antialias { get; set; } = true;

        private CelestialObject selectedObject;
        public CelestialObject SelectedObject
        {
            get
            {
                return selectedObject;
            }
            set
            {
                if (value != selectedObject)
                {
                    selectedObject = value;
                    SelectedObjectChanged?.Invoke(selectedObject);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedObject)));
                }
            }
        }

        /// <summary>
        /// Locked Object. If it set, map moving is denied and it always centered on this body. 
        /// </summary>
        public CelestialObject LockedObject { get; set; }

        /// <summary>
        /// Backing field for <see cref="MousePosition"/> property.
        /// </summary>
        private CrdsEquatorial mousePosition = new CrdsEquatorial(0, 0);

        /// <summary>
        /// Last result of needRedraw flag
        /// </summary>
        private bool lastNeedRedraw = false;

        /// <summary>
        /// Gets or sets current coordinates of mouse, converted to Horizontal coordinates on the map.
        /// Setting new value can raise redraw of map
        /// </summary>
        public CrdsEquatorial MousePosition
        {
            get { return mousePosition; }
            set
            {
                mousePosition = value;
                bool needRedraw = renderers.Any(r => r.OnMouseMove(mousePosition, MouseButton));
                if (MouseButton == MouseButton.None && (needRedraw || lastNeedRedraw))
                {
                    lastNeedRedraw = needRedraw;
                    Invalidate();
                }
            }
        }

        private PointF mouseCoordinates;

        public PointF MouseCoordinates
        {
            get => mouseCoordinates;
            set
            {
                mouseCoordinates = value;
                bool needRedraw = renderers.Any(r => r.OnMouseMove(this, mouseCoordinates, MouseButton));
                if (MouseButton == MouseButton.None && (needRedraw || lastNeedRedraw))
                {
                    lastNeedRedraw = needRedraw;
                    Invalidate();
                }
            }
        }

        public MouseButton MouseButton { get; set; }

        /// <summary>
        /// Occurs when selected celestial object is changed
        /// </summary>
        public event Action<CelestialObject> SelectedObjectChanged;

        public Projection Projection { get; private set; }

        public event Action OnInvalidate;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Collection of celestial objects drawn on the map
        /// </summary>
        private ICollection<Tuple<CelestialObject, PointF, float>> celestialObjects = new List<Tuple<CelestialObject, PointF, float>>();

        /// <summary>
        /// Application settings
        /// </summary>
        private ISettings settings = null;

        /// <summary>
        /// Texture manager instance
        /// </summary>
        private ITextureManager textureManager = null;

        public SkyMap(ITextureManager textureManager, ISettings settings)
        {
            this.textureManager = textureManager;
            this.settings = settings;
        }

        /// <summary>
        /// Sky context instance used for rendering purposes
        /// </summary>
        private SkyContext context;

        public void SetProjection(Type type)
        {
            var fov = Projection?.Fov ?? 90;
            var vision = Projection?.CenterHorizontal ?? new CrdsHorizontal(0, 0);
            int w = Projection?.ScreenWidth ?? 1;
            int h = Projection?.ScreenHeight ?? 1;
            var mode = Projection?.ViewMode ?? ProjectionViewType.Horizontal;

            Projection = (Projection)Activator.CreateInstance(type, context);
            Projection.Fov = fov;
            Projection.SetVision(vision);
            Projection.FlipVertical = !settings.Get("IsInverted");
            Projection.FlipHorizontal = settings.Get("IsMirrored");
            Projection.ViewMode = mode;
            Projection.SetScreenSize(w, h);
            FovChanged?.Invoke(fov);
            Invalidate();
        }

        public void Initialize(SkyContext skyContext, ICollection<BaseRenderer> renderers)
        {
            context = new SkyContext(skyContext.JulianDay, skyContext.GeoLocation);

            // Keep current context synchronized with global instance
            skyContext.ContextChanged += () => context.Set(skyContext.JulianDay, skyContext.GeoLocation);

            SetProjection(typeof(StereographicProjection));

            this.renderers.AddRange(renderers);
            this.renderers.ForEach(r => r.Initialize());

            Schema = settings.Get<ColorSchema>("Schema");

            // get saved rendering orders
            RenderingOrder renderingOrder = settings.Get<RenderingOrder>("RenderingOrder");

            // sort renderers according saved orders
            this.renderers.Sort(renderingOrder.Select(r => r.RendererTypeName));

            // build rendering order based on existing renderers
            renderingOrder = new RenderingOrder(this.renderers.Select(r => new RenderingOrderItem(r)));

            // save actual rendering order
            settings.Set("RenderingOrder", renderingOrder);

            settings.SettingValueChanged += (name, value) =>
            {
                // redraw if rendering order changed
                if (name == "RenderingOrder")
                {
                    this.renderers.Sort(settings.Get<RenderingOrder>("RenderingOrder").Select(r => r.RendererTypeName));
                    Invalidate();
                }

                if (name == "Schema")
                {
                    Schema = settings.Get<ColorSchema>("Schema");
                    Invalidate();
                }

                if (name == "IsMirrored")
                {
                    Projection.FlipHorizontal = settings.Get("IsMirrored");
                    Invalidate();
                }

                if (name == "IsInverted")
                {
                    Projection.FlipVertical = !settings.Get("IsInverted");
                    Invalidate();
                }
            };

            new Thread(TimeSyncWorker) { IsBackground = true }.Start();
        }

        private ManualResetEvent timeSyncResetEvent = new ManualResetEvent(false);
        private ManualResetEvent timeSyncEvent = new ManualResetEvent(false);

        private void TimeSyncWorker()
        {
            while (true)
            {
                timeSyncEvent.WaitOne();
                timeSyncResetEvent.WaitOne();
                double rate = Math.Min(5000, Math.Max(100, Projection.Fov * 100));
                context.JulianDay = new Date(DateTime.Now).ToJulianEphemerisDay();
                Invalidate();
                Thread.Sleep((int)rate);
            }
        }

        public ColorSchema Schema { get; private set; } = ColorSchema.Night;

        private long rendersCount = 0;
        private long meanRenderTime = 0;

        public void Render()
        {
            renderStopWatch.Restart();

            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.StencilBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, Projection.ScreenWidth,
                        0, Projection.ScreenHeight, -1, 1);

            textureManager.Cleanup();
            celestialObjects.Clear();
            labels.Clear();

            for (int i = 0; i < renderers.Count(); i++)
            {
                try
                {
                    renderers.ElementAt(i).Render(this);
                }
                catch (Exception ex)
                {
                    Log.Error($"Rendering error: {ex}");
                }
            }

            DrawSelectedObject();

            GL.PopMatrix();


            renderStopWatch.Stop();

            rendersCount++;

            // Calculate mean time of rendering with Cumulative Moving Average formula
            meanRenderTime = (renderStopWatch.ElapsedMilliseconds + rendersCount * meanRenderTime) / (rendersCount + 1);

            //Debug.WriteLine("Mean render time: " + meanRenderTime);
        }

        private void DrawSelectedObject()
        {
            if (SelectedObject != null && celestialObjects.Any())
            {
                var bodyAndPosition = celestialObjects.FirstOrDefault(x => x.Item1.Equals(SelectedObject));

                if (bodyAndPosition != null)
                {
                    PointF pos = bodyAndPosition.Item2;
                    CelestialObject body = bodyAndPosition.Item1;

                    double sd = (body is SizeableCelestialObject) ? (body as SizeableCelestialObject).Semidiameter : 0;

                    float mag = (body is IMagnitudeObject) ? (body as IMagnitudeObject).Magnitude : Projection.MagLimit;

                    double diskSize = Projection.GetDiskSize(sd, 10);
                    double pointSize = Projection.GetPointSize(double.IsNaN(mag) ? Projection.MagLimit : mag);

                    double size = Math.Max(diskSize, pointSize);

                    Vec2 p = new Vec2(pos.X, pos.Y);

                    Primitives.DrawEllipse(p, Pens.Red, (size + 8) / 2);
                }
            }
        }

        public void Invalidate()
        {
            if (!renderStopWatch.IsRunning)
            {
                OnInvalidate?.Invoke();
            }
        }

        public CelestialObject FindObject(PointF point)
        {
            /*
            var hor = Projection.Invert(point);

            foreach (var body in drawnObjects.OrderBy(c => Angle.Separation(hor, c.Horizontal)))
            {
                double sd = (body is SizeableCelestialObject) ?
                    (body as SizeableCelestialObject).Semidiameter : 0;

                double size = Math.Max(10, sd / 3600.0 / ViewAngle * Width);

                PointF p = Projection.Project(body.Horizontal);

                if (mapContext.DistanceBetweenPoints(p, point) <= size / 2)
                {
                    return body;
                }
            }
            */

            foreach (var x in celestialObjects.OrderBy(c => (point.X - c.Item2.X) * (point.X - c.Item2.X) + (Projection.ScreenHeight - point.Y - c.Item2.Y) * (Projection.ScreenHeight - point.Y - c.Item2.Y)))
            {
                double sd = (x.Item1 is SizeableCelestialObject) ?
                    (x.Item1 as SizeableCelestialObject).Semidiameter : 0;

                double size = Projection.GetDiskSize(sd, 10);

                if (Math.Sqrt((x.Item2.X - point.X) * (x.Item2.X - point.X) + (Projection.ScreenHeight - x.Item2.Y - point.Y) * (Projection.ScreenHeight - x.Item2.Y - point.Y)) < size / 2)
                {
                    return x.Item1;
                }
            }

            return null;
        }

        public void GoToObject(CelestialObject body, TimeSpan animationDuration)
        {
            double sd = (body is SizeableCelestialObject) ?
                        (body as SizeableCelestialObject).Semidiameter / 3600 : 0;

            double viewAngleTarget = sd == 0 ? 1 : Math.Max(sd * 10, 1 / 1024.0);

            GoToPoint(body.Equatorial, animationDuration, viewAngleTarget);
        }

        public void GoToObject(CelestialObject body, double viewAngleTarget)
        {
            GoToPoint(body.Equatorial, TimeSpan.Zero, viewAngleTarget);
        }

        public void GoToObject(CelestialObject body, TimeSpan animationDuration, double viewAngleTarget)
        {
            GoToPoint(body.Equatorial, animationDuration, viewAngleTarget);
        }

        public void GoToPoint(CrdsEquatorial eq, TimeSpan animationDuration)
        {
            GoToPoint(eq, animationDuration, Math.Min(Projection.Fov, 90));
        }

        public void GoToPoint(CrdsEquatorial eq, double viewAngleTarget)
        {
            GoToPoint(eq, TimeSpan.Zero, viewAngleTarget);
        }

        public void GoToPoint(CrdsEquatorial eq, TimeSpan animationDuration, double viewAngleTarget)
        {
            if (viewAngleTarget == 0)
            {
                viewAngleTarget = Projection.Fov;
            }

            if (animationDuration.Equals(TimeSpan.Zero))
            {
                Projection.SetVision(eq);
                Projection.Fov = viewAngleTarget;
                FovChanged?.Invoke(viewAngleTarget);
                Invalidate();
            }
            else
            {
                CrdsEquatorial centerOriginal = new CrdsEquatorial(Projection.CenterEquatorial);
                double ad = Angle.Separation(eq, centerOriginal);

                // TODO: calculate steps by more suitable formula
                double steps = animationDuration.TotalMilliseconds;
                
                double[] x = new double[] { 0, steps / 2, steps };
                double[] y = (ad < Projection.Fov) ?
                    // linear zooming if body is already on the screen:
                    new double[] { Projection.Fov, (Projection.Fov + viewAngleTarget) / 2, viewAngleTarget } :
                    // parabolic zooming with jumping to 90 degrees view angle at the middle of path:
                    new double[] { Projection.Fov, 90, viewAngleTarget };

                Task.Run(() =>
                {
                    for (int i = 0; i <= steps; i++)
                    {
                        Projection.SetVision(Angle.Intermediate(centerOriginal, eq, i / steps));
                        Projection.Fov = Math.Min(90, Interpolation.Lagrange(x, y, i));
                        Thread.Sleep(1);
                        Invalidate();                        
                        FovChanged?.Invoke(Projection.Fov);
                    }
                });
            }
        }

        public void AddDrawnObject(PointF p, CelestialObject obj, float size)
        {
            celestialObjects.Add(new Tuple<CelestialObject, PointF, float>(obj, p, size));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textRenderer"></param>
        /// <param name="label"></param>
        /// <param name="font"></param>
        /// <param name="brush"></param>
        /// <param name="p"></param>
        /// <param name="size">Object size, in pixels</param>
        public void DrawObjectLabel(TextRenderer textRenderer, string label, Font font, Brush brush, PointF p, float size) 
        {
            SizeF b = System.Windows.Forms.TextRenderer.MeasureText(label, font);

            float s = size > 5 ? (size / 2.8284f + 2) : 1;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    float dx = x == 0 ? s : -s - b.Width;
                    float dy = y == 0 ? -s : s + b.Height;
                    RectangleF r = new RectangleF(p.X + dx, p.Y + dy, b.Width, b.Height);
                    if (!labels.Any(l => l.IntersectsWith(r)))
                    {
                        textRenderer.DrawString(label, font, brush, r.Location);
                        labels.Add(r);
                        return;
                    }
                }
            }
        }
    }
}
