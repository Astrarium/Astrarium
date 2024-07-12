using Astrarium.Algorithms;
using Astrarium.Projections;
using Astrarium.Types;
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
        // TODO: remove if not used
        public event Action<double> FovChanged;

        public event Action ContextChanged;

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

        private bool timeSync = false;
        public bool TimeSync
        {
            get => timeSync;
            set
            {
                timeSync = value;
                timeSyncWaitEvent.Set();

                if (value)
                {
                    timeSyncResetEvent.Set();
                }
                else
                {
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

        private CelestialObject lockedObject;
        
        /// <summary>
        /// Locked Object 
        /// </summary>
        public CelestialObject LockedObject
        {
            get => lockedObject;
            set
            {
                if (lockedObject != value)
                {
                    lockedObject = value;
                    if (lockedObject != null)
                    {
                        var eq = lockedObject.Equatorial;
                        lockedObjectShiftAlpha = Projection.CenterEquatorial.Alpha - eq.Alpha;
                        lockedObjectShiftDelta = Projection.CenterEquatorial.Delta - eq.Delta;
                    }
                    LockedObjectChanged?.Invoke(lockedObject);
                }
            }
        }

        public void Move(Vec2 pOld, Vec2 pNew)
        {
            Projection.Move(pOld, pNew);

            if (LockedObject != null)
            {
                var eq = LockedObject.Equatorial;

                var sep = Angle.Separation(Projection.CenterEquatorial, eq);
                if (sep < 90)
                {
                    lockedObjectShiftAlpha = Projection.CenterEquatorial.Alpha - eq.Alpha;
                    lockedObjectShiftDelta = Projection.CenterEquatorial.Delta - eq.Delta;
                }
                else
                {
                    LockedObject = null;
                }
            }

            Invalidate();
        }

        /// <summary>
        /// Shift between screen center and locked object by Right Ascention, in degrees
        /// </summary>
        private double lockedObjectShiftAlpha;

        /// <summary>
        /// Shift between screen center and locked object by Declination, in degrees
        /// </summary
        private double lockedObjectShiftDelta;

        /// <inheritdoc/>
        public CrdsEquatorial MouseEquatorialCoordinates => Projection.UnprojectEquatorial(MouseScreenCoordinates.X, MouseScreenCoordinates.Y);

        /// <inheritdoc/>
        public CrdsHorizontal MouseHorizontalCoordinates => Projection.UnprojectHorizontal(MouseScreenCoordinates.X, MouseScreenCoordinates.Y);

        /// <inheritdoc/>
        public PointF MouseScreenCoordinates { get; set; }

        /// <summary>
        /// Propagates MouseMove event to renderers
        /// </summary>
        public void RaiseMouseMove()
        {
            renderers.ForEach(r => r.OnMouseMove(this, MouseButton));
        }

        /// <summary>
        /// Propagates MouseDown event to renderers
        /// </summary>
        public void RaiseMouseDown()
        {
            renderers.ForEach(r => r.OnMouseDown(this, MouseButton));
        }

        /// <summary>
        /// Propagates MouseUp event to renderers
        /// </summary>
        public void RaiseMouseUp()
        {
            renderers.ForEach(r => r.OnMouseUp(this, MouseButton));
        }

        /// <summary>
        /// Gets or sets current mouse button
        /// </summary>
        public MouseButton MouseButton { get; set; }

        // <inheritdoc />
        public event Action<CelestialObject> SelectedObjectChanged;

        // <inheritdoc />
        public event Action<CelestialObject> LockedObjectChanged;

        // <inheritdoc />
        public Projection Projection { get; private set; }

        public event Action OnInvalidate;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Collection of celestial objects drawn on the map
        /// </summary>
        private ICollection<CelestialObject> celestialObjects = new List<CelestialObject>();

        /// <summary>
        /// Application settings
        /// </summary>
        private ISettings settings = null;

        public SkyMap(ISettings settings)
        {
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
            if (Projection != null)
            {
                Projection.Context.ContextChanged -= Projection_ContextChanged;
                Projection.FovChanged -= Projection_FovChanged;
            }

            Projection = (Projection)Activator.CreateInstance(type, context);
            Projection.Fov = fov;
            Projection.SetVision(vision);
            Projection.FlipVertical = settings.Get("FlipVertical");
            Projection.FlipHorizontal = settings.Get("FlipHorizontal");
            Projection.UseRefraction = settings.Get("Refraction");
            Projection.RefractionPressure = (double)settings.Get("RefractionPressure", 1010m);
            Projection.RefractionTemperature = (double)settings.Get("RefractionTemperature", 10m);
            Projection.ViewMode = settings.Get("ViewMode", ProjectionViewType.Horizontal);
            Projection.SetScreenSize(w, h);
            Projection.FovChanged += Projection_FovChanged;
            Projection.Context.ContextChanged += Projection_ContextChanged;
            Projection_FovChanged(Projection.Fov);
            Invalidate();
        }

        private void Projection_ContextChanged()
        {
            ContextChanged?.Invoke();
        }

        private void Projection_FovChanged(double fov)
        {
            FovChanged?.Invoke(fov);
        }

        public void Initialize(SkyContext skyContext, ICollection<BaseRenderer> renderers)
        {
            context = new SkyContext(skyContext.JulianDay, skyContext.GeoLocation);

            // Keep current context synchronized with global instance
            skyContext.ContextChanged += () =>
            {
                context.Set(skyContext.JulianDay, skyContext.GeoLocation);

                if (LockedObject != null)
                {
                    Projection.SetVision(new CrdsEquatorial(
                        LockedObject.Equatorial.Alpha + lockedObjectShiftAlpha,
                        LockedObject.Equatorial.Delta + lockedObjectShiftDelta));
                }
            };

            var projectionTypeName = settings.Get("Projection", nameof(StereographicProjection));

            var projectionType = System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Projection)) && !t.IsAbstract).FirstOrDefault(t => t.Name == projectionTypeName) ?? typeof(StereographicProjection);

            SetProjection(projectionType);

            this.renderers.AddRange(renderers);
            this.renderers.ForEach(r => r.Initialize());

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

                if (name == "NightMode")
                {
                    Invalidate();
                }

                if (name == "ViewMode")
                {
                    Projection.ViewMode = settings.Get("ViewMode", ProjectionViewType.Horizontal);
                }

                if (name == "FlipHorizontal")
                {
                    Projection.FlipHorizontal = settings.Get("FlipHorizontal");
                    Invalidate();
                }

                if (name == "FlipVertical")
                {
                    Projection.FlipVertical = settings.Get("FlipVertical");
                    Invalidate();
                }

                if (name == "Refraction")
                {
                    Projection.UseRefraction = settings.Get("Refraction");
                    Invalidate();
                }

                if (name == "RefractionPressure")
                {
                    Projection.RefractionPressure = (double)settings.Get("RefractionPressure", 1010m);
                    Invalidate();
                }

                if (name == "RefractionTemperature")
                {
                    Projection.RefractionTemperature = (double)settings.Get("RefractionTemperature", 10m);
                    Invalidate();
                }
            };

            new Thread(TimeSyncWorker) { IsBackground = true }.Start();
        }

        private ManualResetEvent timeSyncResetEvent = new ManualResetEvent(false);
        private AutoResetEvent timeSyncWaitEvent = new AutoResetEvent(false);

        private void TimeSyncWorker()
        {
            while (true)
            {
                timeSyncResetEvent.WaitOne();
                double rate = Math.Min(100, Math.Max(100, Projection.Fov * 100));
                context.JulianDay = new Date(DateTime.Now).ToJulianEphemerisDay();

                if (LockedObject != null)
                {
                    Projection.SetVision(new CrdsEquatorial(
                        LockedObject.Equatorial.Alpha + lockedObjectShiftAlpha,
                        LockedObject.Equatorial.Delta + lockedObjectShiftDelta));
                }

                Invalidate();
                timeSyncWaitEvent.WaitOne((int)rate);
            }
        }

        public void Render()
        {
            GL.ClearColor(Color.Black);
            GL.Clear(GL.COLOR_BUFFER_BIT);
            GL.Clear(GL.STENCIL_BUFFER_BIT);

            GL.MatrixMode(GL.PROJECTION);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, Projection.ScreenWidth, 0, Projection.ScreenHeight, -1, 1);

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
            GL.Flush();
        }

        private void DrawSelectedObject()
        {
            if (SelectedObject != null && celestialObjects.Any() && !SelectedObject.Equals(LockedObject))
            {
                var body = celestialObjects.FirstOrDefault(x => x.Equals(SelectedObject));
                if (body != null)
                {
                    DrawObjectOutline(body, Color.Red);
                }
            }

            if (LockedObject != null && celestialObjects.Any())
            {
                var body = celestialObjects.FirstOrDefault(x => x.Equals(LockedObject));
                if (body != null)
                {
                    DrawObjectOutline(body, Color.LightGreen);
                }
            }
        }

        private void DrawObjectOutline(CelestialObject body, Color color)
        {
            bool isNightMode = settings.Get("NightMode");
            var prj = Projection;
            Vec2 p = prj.Project(body.Equatorial);
            var clr = color.Tint(isNightMode);
            Pen pen = new Pen(clr);

            float mag = (body is IMagnitudeObject) ? (body as IMagnitudeObject).Magnitude : Projection.MagLimit;
            float sd = (body is SizeableCelestialObject) ? (body as SizeableCelestialObject).Semidiameter : 0;
            double diskSize = Projection.GetDiskSize(sd, 0);
            double pointSize = Math.Max(16, Projection.GetPointSize(double.IsNaN(mag) ? Projection.MagLimit : mag));

            if (diskSize > pointSize && body is SizeableCelestialObject sizeableBody)
            {
                // has complex shape
                if (sizeableBody.Shape != null && sizeableBody.Shape.Any())
                {
                    double epoch = sizeableBody.ShapeEpoch.GetValueOrDefault(prj.Context.JulianDay);
                    var pe = Precession.ElementsFK5(epoch, prj.Context.JulianDay);

                    GL.Enable(GL.BLEND);
                    GL.Enable(GL.LINE_SMOOTH);
                    GL.Hint(GL.LINE_SMOOTH_HINT, GL.NICEST);
                    GL.Color4(clr);
                    GL.Begin(GL.LINE_LOOP);

                    foreach (var sp in sizeableBody.Shape)
                    {
                        Vec2 op = prj.Project(Precession.GetEquatorialCoordinates(sp, pe));
                        if (op != null)
                        {
                            GL.Vertex2(op.X, op.Y);
                        }
                    }

                    GL.End();
                }
                // no complex shape
                else
                {
                    float lgSd = sizeableBody.LargeSemidiameter.GetValueOrDefault(sd);
                    float smSd = sizeableBody.SmallSemidiameter.GetValueOrDefault(sd);

                    // non-circular object
                    if (lgSd != smSd)
                    {
                        float posAngle = sizeableBody.PositionAngle.GetValueOrDefault(0);
                        float rx = prj.GetDiskSize(lgSd) / 2 + 4;
                        float ry = prj.GetDiskSize(smSd) / 2 + 4;
                        double rot = prj.GetAxisRotation(body.Equatorial, 90 + posAngle);
                        GL.DrawEllipse(p, pen, rx, ry, rot);
                    }
                    // circular object
                    else
                    {
                        GL.DrawEllipse(p, pen, (diskSize + 8) / 2);
                    }
                }
            }
            else
            {
                GL.DrawEllipse(p, pen, (pointSize + 8) / 2);
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
            foreach (var x in celestialObjects.OrderBy(c => Projection.Project(c.Equatorial).Distance(point)))
            {
                var p = Projection.Project(x.Equatorial);
                float sd = (x is SizeableCelestialObject) ? (x as SizeableCelestialObject).Semidiameter : 0;
                float size = Projection.GetDiskSize(sd, 10);
                if (p.Distance(point) < size / 2)
                {
                    return x;
                }
            }

            return null;
        }

        public void GoToObject(CelestialObject body, TimeSpan animationDuration)
        {
            float sd = (body is SizeableCelestialObject) ?
                       (body as SizeableCelestialObject).Semidiameter / 3600 : 0;

            double viewAngleTarget = sd == 0 ? 1 / 1024.0 : Math.Max(sd * 10, 1 / 1024.0);

            var eq = Projection.WithRefraction(body.Equatorial);
            GoToPoint(eq, animationDuration, viewAngleTarget);
        }

        public void GoToObject(CelestialObject body, double viewAngleTarget)
        {
            var eq = Projection.WithRefraction(body.Equatorial);
            GoToPoint(eq, TimeSpan.Zero, viewAngleTarget);
        }

        public void GoToObject(CelestialObject body, TimeSpan animationDuration, double viewAngleTarget)
        {
            var eq = Projection.WithRefraction(body.Equatorial);
            GoToPoint(eq, animationDuration, viewAngleTarget);
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

            LockedObject = null;

            if (animationDuration.Equals(TimeSpan.Zero))
            {
                Projection.SetVision(eq);
                Projection.Fov = viewAngleTarget;
                Invalidate();
            }
            else
            {
                CrdsEquatorial centerOriginal = new CrdsEquatorial(Projection.CenterEquatorial);
                double ad = Angle.Separation(eq, centerOriginal);

                double duration = animationDuration.TotalMilliseconds;

                double[] x = new double[] { 0, duration / 2, duration };
                double[] y = (ad < Projection.Fov) ?
                    // linear zooming if body is already on the screen:
                    new double[] { Projection.Fov, (Projection.Fov + viewAngleTarget) / 2, viewAngleTarget } :
                    // parabolic zooming with jumping to 90 degrees view angle at the middle of path:
                    new double[] { Projection.Fov, 90, viewAngleTarget };

                Task.Run(() =>
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    double fraction;
                    do
                    {
                        fraction = Math.Min(1, sw.ElapsedMilliseconds / duration);
                        Projection.SetVision(Angle.Intermediate(centerOriginal, eq, fraction));
                        Projection.Fov = Math.Min(90, Interpolation.Lagrange(x, y, Math.Min(duration, sw.ElapsedMilliseconds)));
                        Invalidate();
                    }
                    while (fraction < 1);

                    sw.Stop();
                });
            }
        }

        public void AddDrawnObject(PointF p, CelestialObject obj)
        {
            celestialObjects.Add(obj);
        }

        /// <inheritdoc />
        public void DrawObjectLabel(string label, Font font, Brush brush, PointF p, float size)
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
                        GL.DrawString(label, font, brush, r.Location);
                        labels.Add(r);
                        return;
                    }
                }
            }
        }
    }
}
