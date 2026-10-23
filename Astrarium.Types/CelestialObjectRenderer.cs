using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Astrarium.Types
{
    /// <summary>
    /// Contains methods for rendering celestial objects with caching capabilities.
    /// </summary>
    /// <typeparam name="T">Type of celestial object</typeparam>
    public abstract class CelestialObjectRenderer<T> : BaseRenderer
    {
        /// <summary>
        /// Map instance
        /// </summary>
        private readonly ISkyMap map;

        #region Caching

        /// <summary>
        /// Immutable snapshot of cached objects.
        /// Never mutated after publication; replaced atomically by the writer.
        /// </summary>
        private IEnumerable<T> cache = Enumerable.Empty<T>();

        /// <summary>
        /// Hash info
        /// </summary>
        private readonly Hash hash = new Hash();

        /// <summary>
        /// Sync locker
        /// </summary>
        private readonly object locker = new object();

        /// <summary>
        /// Flag indicating the request is not completed.
        /// </summary>
        private bool isRequested = false;

        #endregion Caching

        /// <summary>
        /// Creates new instance of the renderer.
        /// </summary>
        /// <param name="map">Map instance</param>
        protected CelestialObjectRenderer(ISkyMap map)
        {
            this.map = map;
        }

        /// <summary>
        /// Should be implemented in derived class.
        /// Returns enumerable of celestial objects matching the criteria; without cache.
        /// </summary>
        /// <param name="context">SkyContext instance</param>
        /// <param name="eq0">Coordinates of a center of vision</param>
        /// <param name="angle">Current FOV angle, in degrees</param>
        /// <param name="magFilter">Filter function by magnitude</param>
        /// <returns>Enumerable of celestial objects to be drawn</returns>
        protected abstract IEnumerable<T> GetCelestialObjectsWithoutCache(SkyContext context, CrdsEquatorial eq0, double angle, Func<float, bool> magFilter);

        /// <summary>
        /// Returns enumerable of celestial objects matching the criteria; with cache.
        /// </summary>
        /// <param name="context">SkyContext instance</param>
        /// <param name="eq0">Coordinates of a center of vision</param>
        /// <param name="angle">Current FOV angle, in degrees</param>
        /// <param name="magFilter">Filter function by magnitude</param>
        /// <returns>Enumerable of celestial objects to be drawn</returns>
        protected IEnumerable<T> GetCelestialObjectsWithCache(SkyContext context, CrdsEquatorial eq0, double angle, Func<float, bool> magFilter)
        {
            float magLimit = map.Projection.MagLimit;

            bool needRequest;
            lock (locker)
            {
                needRequest = !isRequested && hash.HasDifference(eq0, angle, magLimit);
                if (needRequest)
                {
                    hash.Take(eq0, angle, magLimit);
                    isRequested = true;
                }
            }

            if (needRequest)
            {
                _ = Task.Run(() =>
                {
                    try
                    {
                        var snapshot = GetCelestialObjectsWithoutCache(context, eq0, angle, magFilter).ToList();

                        lock (locker)
                        {
                            cache = snapshot;
                        }

                        map.Invalidate();
                    }
                    finally
                    {
                        lock (locker) isRequested = false;
                    }
                });
            }

            return cache;
        }

        /// <summary>
        /// Gets star color by its spectral class.
        /// </summary>
        /// <param name="spClass">Spectral class</param>
        /// <returns>Star color</returns>
        protected virtual Color GetStarColor(char spClass)
        {
            Color starColor;
            
            switch (spClass)
            {
                case 'O':
                case 'W':
                    starColor = Color.LightBlue;
                    break;
                case 'B':
                    starColor = Color.LightCyan;
                    break;
                case 'A':
                    starColor = Color.White;
                    break;
                case 'F':
                    starColor = Color.LightYellow;
                    break;
                case 'G':
                    starColor = Color.Yellow;
                    break;
                case 'K':
                    starColor = Color.Orange;
                    break;
                case 'M':
                    starColor = Color.OrangeRed;
                    break;
                default:
                    starColor = Color.White;
                    break;
            }

            return starColor;
        }

        private class Hash
        {
            /// <summary>
            /// FOV in degrees
            /// </summary>
            private double fov;

            /// <summary>
            /// Current center of view
            /// </summary>
            private CrdsEquatorial center = null;

            /// <summary>
            /// Current magnitude limit
            /// </summary>
            private float magLimit;

            /// <summary>
            /// Returns true if hash has significant difference with provided params
            /// </summary>
            /// <param name="center">Current center of view</param>
            /// <param name="fov">FOV in degrees</param>
            /// <param name="magLimit">Magnitude limit</param>
            /// <returns>True if current hash has difference</returns>
            public bool HasDifference(CrdsEquatorial center, double fov, float magLimit)
            {
                if (this.center == null) return true;
                if (this.fov != fov) return true;
                if (this.magLimit != magLimit) return true;
                return Angle.Separation(this.center, center) > fov * 0.1;
            }

            /// <summary>
            /// Takes the hash from provided params
            /// </summary>
            /// <param name="center">Current center of view</param>
            /// <param name="fov">FOV in degrees</param>
            /// <param name="magLimit">Magnitude limit</param>
            public void Take(CrdsEquatorial center, double fov, float magLimit)
            {
                this.center = new CrdsEquatorial(center);
                this.fov = fov;
                this.magLimit = magLimit;
            }
        }
    }
}
