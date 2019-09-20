using ADK;
using Planetarium.Objects;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.MilkyWay
{
    public interface IMilkyWayProvider
    {
        List<List<CelestialPoint>> MilkyWay { get; }
    }

    /// <summary>
    /// Calculates coordinates of Milky Way outline points to be rendered on map
    /// </summary>
    public class MilkyWayCalc : BaseCalc, IMilkyWayProvider
    {
        /// <summary>
        /// Outline points
        /// </summary>
        public List<List<CelestialPoint>> MilkyWay { get; private set; } = new List<List<CelestialPoint>>();

        public override void Calculate(SkyContext context)
        {
            var p = Precession.ElementsFK5(Date.EPOCH_J2000, context.JulianDay);

            foreach (var block in MilkyWay)
            {
                foreach (var bp in block)
                {
                    // Equatorial coordinates for the mean equinox and epoch of the target date
                    var eq = Precession.GetEquatorialCoordinates(bp.Equatorial0, p);

                    // Apparent horizontal coordinates
                    bp.Horizontal = eq.ToHorizontal(context.GeoLocation, context.SiderealTime);
                }
            }
        }

        public override void Initialize()
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data/MilkyWay.dat");

            List<CelestialPoint> block = null;
            using (var sr = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                int fragment = -1;
                while (sr.BaseStream.Position != sr.BaseStream.Length)
                {
                    int f = sr.ReadChar();
                    if (f != fragment)
                    {
                        fragment = f;
                        block = new List<CelestialPoint>();
                        MilkyWay.Add(block);
                    }

                    block.Add(new CelestialPoint()
                    {
                        Equatorial0 = new CrdsEquatorial(sr.ReadSingle(), sr.ReadSingle())
                    });
                }
            }
        }
    }
}
