using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Types
{
    public abstract class Projection
    {
        /// <summary>
        /// Gets maximal field of view suported by the projection, in degrees.
        /// </summary>
        public abstract double MaxFov { get; }

        /// <summary>
        /// Screen scaling factor
        /// </summary>
        protected double ScreenScalingFactor { get; private set; }

        private bool flipHorizontal = false;
        public bool FlipHorizontal
        {
            get => flipHorizontal;
            set
            {
                flipHorizontal = value;
                SkyContextChanged();
            }
        }

        private bool flipVertical = false;
        public bool FlipVertical
        {
            get => flipVertical;
            set
            {
                flipVertical = value;
                SkyContextChanged();
            }
        }

        /// <summary>
        /// Projection matrix
        /// </summary>
        public Mat4 MatProjection { get; private set; } = new Mat4();

        #region Vision matrices

        /// <summary>
        /// Matrix for Equatorial projection
        /// </summary>
        public Mat4 MatEquatorialToVision { get; private set; } = new Mat4();

        /// <summary>
        /// Matrix for Horizontal projection
        /// </summary>
        public Mat4 MatHorizontalToVision { get; private set; } = new Mat4();

        /// <summary>
        /// Invsered matrix for Equatorial projection
        /// </summary>
        private Mat4 MatEquatorialToVisionInverse { get; set; } = new Mat4();

        /// <summary>
        /// Invsered matrix for Horizontal projection
        /// </summary>
        private Mat4 MatHorizontalToVisionInverse { get; set; } = new Mat4();

        #endregion Vision matrices

        #region Transformation matrices

        /// <summary>
        /// Transformation matrix from Horizontal to Equatorial coordinates
        /// </summary>
        private Mat4 MatHorizontalToEquatorial = new Mat4();

        /// <summary>
        /// Transformation matrix from Equatorial to Horizontal coordinates
        /// </summary>
        private Mat4 MatEquatorialToHorizontal = new Mat4();

        #endregion Transformation matrices

        /// <summary>
        /// Vision vector in horizontal coordinates
        /// </summary>
        public Vec3 VecHorizontalVision { get; private set; }

        /// <summary>
        /// Vision vector in equatorial coordinates
        /// </summary>
        public Vec3 VecEquatorialVision { get; private set; }

        private ProjectionViewType viewMode = ProjectionViewType.Horizontal;
        public ProjectionViewType ViewMode
        {
            get => viewMode;
            set
            {
                viewMode = value;
                SkyContextChanged();
            }
        }

        /// <summary>
        /// Sky context, i.e. observer location and date/time instant.
        /// </summary>
        public SkyContext Context { get; private set; }

        /// <summary>
        /// Center of the vision in equatorial coordinates
        /// </summary>
        public CrdsEquatorial CenterEquatorial { get; private set; } = new CrdsEquatorial();

        /// <summary>
        /// Center of the vision in horizontal coordinates
        /// </summary>
        public CrdsHorizontal CenterHorizontal { get; private set; } = new CrdsHorizontal();

        /// <summary>
        /// Gets size of a disk (circle) representing a celestial body or another extended object on the sky map, in pixels
        /// </summary>
        /// <param name="semidiameter">Semidiameter of a body or object, in seconds of arc.</param>
        /// <returns>Size (diameter) of a disk in screen pixels</returns>
        public float GetDiskSize(double semidiameter, double minSize = 0)
        {
            // TODO: check it!
            return (float)Math.Max(minSize, (float)(Math.Min(ScreenWidth, ScreenHeight) / Fov * (2 * semidiameter / 3600)));
        }

        // log fit {90,6},{45,7},{8,9},{1,12},{0.25,17}
        public float MagLimit => Math.Min(float.MaxValue /* TODO: add option to set by user */, (float)(-1.73494 * Math.Log(0.000462398 * Fov)));

        public float GetPointSize(float mag)
        {
            float mag0 = MagLimit;

            float size;

            if (mag > mag0)
                size = 0;
            else
                size = mag0 - mag;

            return size;
        }

        /// <summary>
        /// Checks the point is indide screen bounds
        /// </summary>
        /// <param name="p">Point to check</param>
        /// <returns>True if point is inside screen bounds, false otherwise.</returns>
        public bool IsInsideScreen(Vec2 p)
        {
            return p != null &&
                p.X >= 0 && p.X <= ScreenWidth &&
                p.Y >= 0 && p.Y <= ScreenHeight;
        }

        public Projection(SkyContext context)
        {
            Context = context;
            Context.ContextChanged += SkyContextChanged;

            ScreenWidth = 1024;
            ScreenHeight = 768;

            fov = 90;
        }

        public static Projection Create<TProjection>(SkyContext context) where TProjection : Projection
        {
            return (Projection)Activator.CreateInstance(typeof(TProjection), context);
        }

        private void SkyContextChanged()
        {
            UpdateTransformationMatrices();

            // keep the horizontal position the same and recalculate the eq. position
            VecEquatorialVision = MatHorizontalToEquatorial * VecHorizontalVision;

            UpdateVisionMatrices();
        }

        public void SetScreenSize(int width, int height)
        {
            ScreenWidth = width;
            ScreenHeight = height;

            UpdateScreenScalingFactor();
            UpdateProjectionMatrix();
        }

        public int ScreenWidth { get; private set; }

        public int ScreenHeight { get; private set; }

        private void UpdateScreenScalingFactor()
        {
            ScreenScalingFactor = 1.0 / Fov * 180.0 / Math.PI * Math.Min(ScreenWidth, ScreenHeight);
        }

        protected virtual void UpdateProjectionMatrix()
        {
            MatProjection.Set(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            );
        }

        /// <summary>
        /// Backing field for the field of view, in degrees.
        /// </summary>
        private double fov = 90;

        /// <summary>
        /// Gets or sets current Field Of View, in degrees
        /// </summary>
        public double Fov
        {
            get => fov;
            set
            {
                const double minFov = 0.001;

                if (value > MaxFov) value = MaxFov;
                if (value < minFov) value = minFov;
                fov = value;

                UpdateScreenScalingFactor();
                UpdateProjectionMatrix();

                FovChanged?.Invoke(fov);
            }
        }

        public event Action<double> FovChanged;

        public Vec2 Project(CrdsEquatorial eq)
        {
            Vec3 v = SphericalToCartesian(Angle.ToRadians(eq.Alpha), Angle.ToRadians(eq.Delta));
            return Project(v, MatEquatorialToVision);
        }

        public Vec2 Project(CrdsHorizontal h)
        {
            Vec3 v = SphericalToCartesian(Angle.ToRadians(-h.Azimuth), Angle.ToRadians(h.Altitude));
            return Project(v, MatHorizontalToVision);
        }

        public CrdsHorizontal UnprojectHorizontal(double x, double y)
        {
            Vec3 v = Unproject(new Vec2(x, y), MatHorizontalToVisionInverse);
            if (v == null) return null;
            double azi = 0, alt = 0;
            CartesianToSpherical(ref azi, ref alt, v);
            return new CrdsHorizontal(Angle.To360(Angle.ToDegrees(-azi)), Angle.ToDegrees(alt));
        }

        public CrdsEquatorial UnprojectEquatorial(double x, double y)
        {
            Vec3 v = Unproject(new Vec2(x, y), MatEquatorialToVisionInverse);
            if (v == null) return null;
            double ra = 0, dec = 0;
            CartesianToSpherical(ref ra, ref dec, v);
            return new CrdsEquatorial(Angle.To360(Angle.ToDegrees(ra)), Angle.ToDegrees(dec));
        }

        public abstract Vec2 Project(Vec3 v, Mat4 mat);

        public abstract Vec3 Unproject(Vec2 s, Mat4 m);

        public void SetVision(CrdsHorizontal hor)
        {
            var v = SphericalToCartesian(Angle.ToRadians(-hor.Azimuth), Angle.ToRadians(hor.Altitude));
            if (ViewMode == ProjectionViewType.Horizontal)
            {
                VecHorizontalVision = v;
                VecEquatorialVision = MatHorizontalToEquatorial * VecHorizontalVision;
            }
            else
            {
                VecEquatorialVision = MatHorizontalToEquatorial * v;
                VecHorizontalVision = MatEquatorialToHorizontal * VecEquatorialVision;
            }
            UpdateVisionMatrices();
        }

        public void SetVision(CrdsEquatorial eq)
        {
            var v = SphericalToCartesian(Angle.ToRadians(eq.Alpha), Angle.ToRadians(eq.Delta));
            if (ViewMode == ProjectionViewType.Horizontal)
            {
                VecHorizontalVision = MatEquatorialToHorizontal * v;
                VecEquatorialVision = MatHorizontalToEquatorial * VecHorizontalVision;
            }
            else
            {
                VecEquatorialVision = v;
                VecHorizontalVision = MatEquatorialToHorizontal * VecEquatorialVision;
            }
            UpdateVisionMatrices();
        }

        private void UpdateTransformationMatrices()
        {
            // calculate transformation from horizontal to equatorial coordinates for given instant 
            MatHorizontalToEquatorial =
                Mat4.ZRotation(Angle.ToRadians(Context.SiderealTime - Context.GeoLocation.Longitude)) *
                Mat4.YRotation(Angle.ToRadians(90 - Context.GeoLocation.Latitude));

            // inverse transformation is a transposed matrix
            // TODO: may be .Inverse() ?
            MatEquatorialToHorizontal = MatHorizontalToEquatorial.Transpose();
        }

        /// <summary>
        /// Moves view from old 2D position (pos1) to new 2D position (pos2).
        /// Used while dragging map by mouse.
        /// </summary>
        /// <param name="screenPosOld">Old mouse position on the screen</param>
        /// <param name="screenPosNew">New mouse position on the screen</param>
        public void Move(Vec2 screenPosOld, Vec2 screenPosNew)
        {
            double deltaLon, deltaLat;

            if (ViewMode == ProjectionViewType.Horizontal)
            {
                var p0 = UnprojectHorizontal(screenPosOld[0], screenPosOld[1]);
                var p1 = UnprojectHorizontal(screenPosNew[0], screenPosNew[1]);

                if (p0 == null || p1 == null)
                {
                    deltaLat = 0;
                    deltaLon = 0;
                }
                else
                {

                    deltaLon = Angle.ToRadians(p0.Azimuth - p1.Azimuth);
                    deltaLat = Angle.ToRadians(p0.Altitude - p1.Altitude);
                }
            }
            else
            {
                var p0 = UnprojectEquatorial(screenPosOld[0], screenPosOld[1]);
                var p1 = UnprojectEquatorial(screenPosNew[0], screenPosNew[1]);
                deltaLon = Angle.ToRadians(p1.Alpha - p0.Alpha);
                deltaLat = Angle.ToRadians(p0.Delta - p1.Delta);
            }

            Move(deltaLon, deltaLat);
        }

        private void Move(double deltaLon, double deltaLat)
        {
            double longtiude = 0;
            double latitude = 0;

            if (ViewMode == ProjectionViewType.Horizontal)
                CartesianToSpherical(ref longtiude, ref latitude, VecHorizontalVision);
            else
                CartesianToSpherical(ref longtiude, ref latitude, VecEquatorialVision);

            // moving in longitude (left/right)
            if (deltaLon != 0) longtiude -= deltaLon;

            // moving in latitude (up/down)
            if (deltaLat != 0)
            {
                if (latitude + deltaLat <= Math.PI / 2 && latitude + deltaLat >= -Math.PI / 2) latitude += deltaLat;
                if (latitude + deltaLat > Math.PI / 2) latitude = Math.PI / 2;
                if (latitude + deltaLat < -Math.PI / 2) latitude = -Math.PI / 2;
            }

            if (deltaLon != 0 || deltaLat != 0)
            {
                if (ViewMode == ProjectionViewType.Horizontal)
                {
                    VecHorizontalVision = SphericalToCartesian(longtiude, latitude);
                    VecEquatorialVision = MatHorizontalToEquatorial * VecHorizontalVision;
                }
                else
                {
                    VecEquatorialVision = SphericalToCartesian(longtiude, latitude);
                    VecHorizontalVision = MatEquatorialToHorizontal * VecEquatorialVision;
                }
            }

            UpdateVisionMatrices();
        }

        public void UpdateVisionMatrices()
        {
            double raCenter = 0, decCenter = 0;
            CartesianToSpherical(ref raCenter, ref decCenter, VecEquatorialVision);
            CenterEquatorial.Alpha = Angle.To360(Angle.ToDegrees(raCenter));
            CenterEquatorial.Delta = Angle.ToDegrees(decCenter);

            double aziCenter = 0, altCenter = 0;
            CartesianToSpherical(ref aziCenter, ref altCenter, VecHorizontalVision);
            CenterHorizontal.Azimuth = Angle.To360(Angle.ToDegrees(-aziCenter));
            CenterHorizontal.Altitude = Angle.ToDegrees(altCenter);

            Vec3 f = ViewMode == ProjectionViewType.Horizontal ? VecHorizontalVision : VecEquatorialVision;

            f.Normalize();

            Vec3 s = new Vec3(f[1], -f[0], 0.0);

            if (ViewMode == ProjectionViewType.Equatorial)
            {
                f = new Vec3(VecHorizontalVision[0], VecHorizontalVision[1], VecHorizontalVision[2]);
                f.Normalize();
                s = MatEquatorialToHorizontal * s;
            }

            Vec3 u = s ^ f;
            s.Normalize();
            u.Normalize();

            MatHorizontalToVision = new Mat4(
                s[0], u[0], -f[0], 0,
                s[1], u[1], -f[1], 0,
                s[2], u[2], -f[2], 0,
                0, 0, 0, 1
            );

            MatEquatorialToVision = MatHorizontalToVision * MatEquatorialToHorizontal;

            MatHorizontalToVisionInverse = (MatProjection * MatHorizontalToVision).Inverse();
            MatEquatorialToVisionInverse = (MatProjection * MatEquatorialToVision).Inverse();
        }

        public static Vec3 SphericalToCartesian(double lng, double lat)
        {
            double cosLat = Math.Cos(lat);
            return new Vec3(
                Math.Cos(lng) * cosLat,
                Math.Sin(lng) * cosLat,
                Math.Sin(lat));
        }

        public static void CartesianToSpherical(ref double lng, ref double lat, Vec3 v)
        {
            double r = v.Length;
            if (r != 0)
            {
                lat = Math.Asin(v[2] / r);
            }
            lng = Math.Atan2(v[1], v[0]);
        }
    }
}
