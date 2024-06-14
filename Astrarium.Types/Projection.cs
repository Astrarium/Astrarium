using Astrarium.Algorithms;
using System;
using System.Linq;

namespace Astrarium.Types
{
    /// <summary>
    /// Base class for map projections
    /// </summary>
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

        /// <summary>
        /// Backing field for <see cref="FlipHorizontal"/>
        /// </summary>
        private bool flipHorizontal = false;

        /// <summary>
        /// Flag indicating flipping image horizontally
        /// </summary>
        public bool FlipHorizontal
        {
            get => flipHorizontal;
            set
            {
                flipHorizontal = value;
                UpdateMatrices();
            }
        }

        /// <summary>
        /// Backing field for <see cref="FlipVertical"/>
        /// </summary>
        private bool flipVertical = false;

        /// <summary>
        /// Flag indicating flipping image vertically
        /// </summary>
        public bool FlipVertical
        {
            get => flipVertical;
            set
            {
                flipVertical = value;
                UpdateMatrices();
            }
        }

        /// <summary>
        /// Projection matrix
        /// </summary>
        protected Mat4 MatProjection { get; private set; } = new Mat4();

        #region Vision matrices

        /// <summary>
        /// Matrix for Equatorial projection
        /// </summary>
        private Mat4 MatEquatorialToVision = new Mat4();

        /// <summary>
        /// Matrix for Horizontal projection
        /// </summary>
        private Mat4 MatHorizontalToVision = new Mat4();

        /// <summary>
        /// Invsered matrix for Equatorial projection
        /// </summary>
        private Mat4 MatEquatorialToVisionInverse = new Mat4();

        /// <summary>
        /// Invsered matrix for Horizontal projection
        /// </summary>
        private Mat4 MatHorizontalToVisionInverse = new Mat4();

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
        private Vec3 VecHorizontalVision;

        /// <summary>
        /// Vision vector in equatorial coordinates
        /// </summary>
        private Vec3 VecEquatorialVision;

        /// <summary>
        /// Backing field for <see cref="ViewMode"/>
        /// </summary>
        private ProjectionViewType viewMode = ProjectionViewType.Horizontal;
        
        /// <summary>
        /// View mode: equatorial (NCP up) or horizontal (Zenith up)
        /// </summary>
        public ProjectionViewType ViewMode
        {
            get => viewMode;
            set
            {
                viewMode = value;
                UpdateMatrices();
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
            return (float)Math.Max(minSize, (float)(Math.Min(ScreenWidth, ScreenHeight) / Fov * (2 * semidiameter / 3600)));
        }

        // log fit {90,6},{45,7},{8,9},{1,12},{0.25,17}
        public float MagLimit => Math.Min(float.MaxValue /* TODO: add option to set by user */, (float)(-1.73494 * Math.Log(0.000462398 * Fov)));

        /// <summary>
        /// Gets pixel size of point object (like star), depending on object magnitude.
        /// </summary>
        /// <param name="mag">Magnitude of object</param>
        /// <param name="maxDrawingSize">Maximal drawing size of a point, set 0 as no limit.</param>
        /// <returns>Returns pixel size of point object (like star), depending on object magnitude.</returns>
        public float GetPointSize(float mag, float maxDrawingSize = 0)
        {
            float mag0 = MagLimit;

            float size;

            if (mag > mag0)
                size = 0;
            else
                size = mag0 - mag;

            if (maxDrawingSize > 0)
                return Math.Min(size, maxDrawingSize);
            else
                return size;
        }

        /// <summary>
        /// Gets body axis rotation angle respect to screen coordinates 
        /// </summary>
        /// <param name="eq">Equatorial coordinates of body</param>
        /// <param name="posAngle">Position angle, in degrees</param>
        /// <returns>Axis rotation angle, in degrees</returns>
        public double GetAxisRotation(CrdsEquatorial eq, double posAngle)
        {
            Vec2 p = Project(eq + new CrdsEquatorial(0, 1));
            Vec2 p0 = Project(eq);
            if (p == null || p0 == null) return 0;
            return (FlipVertical ? 1 : -1) * (90 + (FlipHorizontal ? 1 : -1) * posAngle) + Angle.ToDegrees(Math.Atan2(p.Y - p0.Y, p.X - p0.X));
        }

        /// <summary>
        /// Gets body axis rotation angle respect to screen coordinates 
        /// </summary>
        /// <param name="h">Horizontal coordinates of the body</param>
        /// <param name="posAngle">Position angle, in degrees</param>
        /// <returns>Axis rotation angle, in degrees</returns>
        public double GetAxisRotation(CrdsHorizontal h, double posAngle)
        {
            Vec2 p = Project(h + new CrdsHorizontal(0, 1));
            Vec2 p0 = Project(h);
            if (p == null || p0 == null) return 0;
            return (FlipVertical ? 1 : -1) * (90 + (FlipHorizontal ? 1 : -1) * posAngle) + Angle.ToDegrees(Math.Atan2(p.Y - p0.Y, p.X - p0.X));
        }

        /// <summary>
        /// Gets terminator rotation angle respect to screen coordinates 
        /// </summary>
        /// <param name="ecl">Ecliptical coordinats of the body</param>
        /// <returns>Rotation angle, in degrees</returns>
        public double GetPhaseRotation(CrdsEcliptical ecl)
        {
            Vec2 p = Project((ecl + new CrdsEcliptical(0, 1)).ToEquatorial(Context.Epsilon));
            Vec2 p0 = Project(ecl.ToEquatorial(Context.Epsilon));
            if (p == null || p0 == null) return 0;
            return 90 - Angle.ToDegrees(Math.Atan2(p.Y - p0.Y, p.X - p0.X));
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

        protected Projection(SkyContext context)
        {
            Context = context;
            Context.ContextChanged += UpdateMatrices;

            ScreenWidth = 1024;
            ScreenHeight = 768;

            fov = 90;
        }

        /// <summary>
        /// Updates projection matrices
        /// </summary>
        private void UpdateMatrices()
        {
            UpdateProjectionMatrix();
            UpdateTransformationMatrices();

            // keep the horizontal position the same and recalculate the eq. position
            VecEquatorialVision = MatHorizontalToEquatorial * VecHorizontalVision;

            UpdateVisionMatrices();
        }

        /// <summary>
        /// Sets screen size in pixels
        /// </summary>
        /// <param name="width">Screen width, in pixels</param>
        /// <param name="height">Screen height, in pixels</param>
        public void SetScreenSize(int width, int height)
        {
            ScreenWidth = width;
            ScreenHeight = height;

            UpdateScreenScalingFactor();
            UpdateProjectionMatrix();
        }

        /// <summary>
        /// Gets screen width in pixels
        /// </summary>
        public int ScreenWidth { get; private set; }

        /// <summary>
        /// Gets screen height in pixels
        /// </summary>
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
            var eqR = WithRefraction(eq);
            Vec3 v = SphericalToCartesian(Angle.ToRadians(eqR.Alpha), Angle.ToRadians(eqR.Delta));
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

        protected abstract Vec2 Project(Vec3 v, Mat4 mat);

        protected abstract Vec3 Unproject(Vec2 s, Mat4 m);

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
            double longitude = 0;
            double latitude = 0;

            if (ViewMode == ProjectionViewType.Horizontal)
                CartesianToSpherical(ref longitude, ref latitude, VecHorizontalVision);
            else
                CartesianToSpherical(ref longitude, ref latitude, VecEquatorialVision);

            // moving in longitude (left/right)
            if (deltaLon != 0) longitude -= deltaLon;

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
                    VecHorizontalVision = SphericalToCartesian(longitude, latitude);
                    VecEquatorialVision = MatHorizontalToEquatorial * VecHorizontalVision;
                }
                else
                {
                    VecEquatorialVision = SphericalToCartesian(longitude, latitude);
                    VecHorizontalVision = MatEquatorialToHorizontal * VecEquatorialVision;
                }
            }

            UpdateVisionMatrices();
        }

        private void UpdateVisionMatrices()
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

        private Vec3 SphericalToCartesian(double lng, double lat)
        {
            double cosLat = Math.Cos(lat);
            return new Vec3(
                Math.Cos(lng) * cosLat,
                Math.Sin(lng) * cosLat,
                Math.Sin(lat));
        }

        private void CartesianToSpherical(ref double lng, ref double lat, Vec3 v)
        {
            double r = v.Length;
            if (r != 0)
            {
                lat = Math.Asin(v[2] / r);
            }
            lng = Math.Atan2(v[1], v[0]);
        }

        /// <summary>
        /// Flag indicating atmospheric refraction corrections is applied
        /// </summary>
        public bool UseRefraction { get; set; }

        /// <summary>
        /// Air temperature, °C, used for refraction corrections
        /// </summary>
        public double RefractionTemperature { get; set; } = 10;

        /// <summary>
        /// Atmospheric pressure, in hPa (mbar), used for refraction corrections
        /// </summary>
        public double RefractionPressure { get; set; } = 1010;

        /// <summary>
        /// Adds atmospheric refraction correction, if required, and returns visible equatorial coordinates.
        /// </summary>
        /// <param name="eq">True (geometric) coordinates, without refraction effect.</param>
        /// <returns>Visible equatorial coordinates.</returns>
        public CrdsEquatorial WithRefraction(CrdsEquatorial eq)
        {
            if (UseRefraction)
            {
                CrdsHorizontal hor = eq.ToHorizontal(Context.GeoLocation, Context.SiderealTime);
                hor.Altitude += Refraction.CorrectionForVisibleCoordinates(hor.Altitude, RefractionPressure, RefractionTemperature);
                return hor.ToEquatorial(Context.GeoLocation, Context.SiderealTime);
            }
            else
            {
                return eq;
            }
        }

        /// <summary>
        /// Withdraws atmospheric refraction correction, if applied, and returns true (geometric) equatorial coordinates.
        /// </summary>
        /// <param name="eq">Visible equatorial coordinates, with refraction effect (if applied).</param>
        /// <returns>True (geometric) equatorial coordinates.</returns>
        public CrdsEquatorial WithoutRefraction(CrdsEquatorial eq)
        {
            if (UseRefraction)
            {
                CrdsHorizontal hor = eq.ToHorizontal(Context.GeoLocation, Context.SiderealTime);
                hor.Altitude -= Refraction.CorrectionForTrueCoordinates(hor.Altitude, RefractionPressure, RefractionTemperature);
                return hor.ToEquatorial(Context.GeoLocation, Context.SiderealTime);
            }
            else
            {
                return eq;
            }
        }
    }
}
