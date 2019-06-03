using ADK;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Planetarium.Calculators
{
    public interface ITycho2Catalog
    {
        ICollection<Tycho2Star> GetStarsAtCircle(CrdsEquatorial eq, double angle, double years, float magLimit);
        CrdsHorizontal GetCoordinates(SkyContext context, Tycho2Star star);
    }

    public class Tycho2Calculator : BaseCalc, ICelestialObjectCalc<Tycho2Star>, ITycho2Catalog
    {
        /// <summary>
        /// Represents a single record from Tycho2 index file.
        /// </summary>
        private class Tycho2Region
        {
            public long FirstStarId { get; set; }
            public long LastStarId { get; set; }
            public float RAmin { get; set; }
            public float RAmax { get; set; }
            public float DECmin { get; set; }
            public float DECmax { get; set; }
        }

        private static List<Tycho2Region> _Index = new List<Tycho2Region>();
        private static BinaryReader _Catalog;

        /// <summary>
        /// Length of catalog record
        /// </summary>
        private const int CATALOG_RECORD_LEN = 33;

        /// <summary>
        /// Width of segment of celestial sphere (in degrees) that's defined by Tycho2Region structure  
        /// </summary>
        private const double SEGMENT_WIDTH = 3.75;

        public override void Initialize()
        {
            // TODO take from settings
            string folder = "D:\\Tycho2";

            try
            {
                string indexFile = Path.Combine(folder, "tycho2.idx");
                string catalogFile = Path.Combine(folder, "tycho2.dat");

                // Read Tycho2 index file and load it into memory.

                StreamReader sr = new StreamReader(indexFile);

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string[] parced = line.Split(';');
                    Tycho2Region region = new Tycho2Region();
                    region.FirstStarId = Convert.ToInt64(parced[0].Trim());
                    region.LastStarId = Convert.ToInt64(parced[1].Trim());
                    region.RAmin = Convert.ToSingle(parced[2].Trim(), CultureInfo.InvariantCulture);
                    region.RAmax = Convert.ToSingle(parced[3].Trim(), CultureInfo.InvariantCulture);
                    region.DECmin = Convert.ToSingle(parced[4].Trim(), CultureInfo.InvariantCulture);
                    region.DECmax = Convert.ToSingle(parced[5].Trim(), CultureInfo.InvariantCulture);

                    _Index.Add(region);
                }

                sr.Close();

                // Open Tycho2 catalog file
                _Catalog = new BinaryReader(File.Open(catalogFile, FileMode.Open));
            }
            catch (Exception ex)
            {

            }
        }

        public ICollection<Tycho2Star> GetStarsAtCircle(CrdsEquatorial eq, double angle, double years, float magLimit)
        {
            double ang = angle + SEGMENT_WIDTH / 2.0;

            var regions = _Index.Where(reg => Angle.Separation(eq, new CrdsEquatorial((reg.RAmax + reg.RAmin) / 2.0, (reg.DECmax + reg.DECmin) / 2.0)) <= ang);

            List<Tycho2Star> stars = new List<Tycho2Star>();

            foreach (Tycho2Region region in regions)
            {
                stars.AddRange(GetStarsInRegion(region, eq, ang, years, magLimit));
            }

            return stars;
        }

        public CrdsHorizontal GetCoordinates(SkyContext context, Tycho2Star star)
        {
            var pe = Precession.ElementsFK5(Date.EPOCH_J2000, context.JulianDay);

            CrdsEquatorial eq = Precession.GetEquatorialCoordinates(star.Equatorial0, pe);

            eq = eq +
                Nutation.NutationEffect(eq, context.NutationElements, context.Epsilon) +
                Aberration.AberrationEffect(eq, context.AberrationElements, context.Epsilon);

            CrdsHorizontal hor = eq.ToHorizontal(context.GeoLocation, context.SiderealTime);

            return hor;
        }

        private ICollection<Tycho2Star> GetStarsInRegion(Tycho2Region region, CrdsEquatorial eq, double angle, double years, float magLimit)
        {
            _Catalog.BaseStream.Seek(CATALOG_RECORD_LEN * (region.FirstStarId - 1), SeekOrigin.Begin);

            int count = (int)(region.LastStarId - region.FirstStarId);
            List<Tycho2Star> stars = new List<Tycho2Star>();

            byte[] buffer = _Catalog.ReadBytes(CATALOG_RECORD_LEN * count);

            for (int i = 0; i < count; i++)
            {
                Tycho2Star star = ParseStarData(buffer, i * CATALOG_RECORD_LEN, eq, angle, years, magLimit);
                if (star != null)
                {
                    stars.Add(star);
                }
            }

            return stars;
        }

        /// <summary>
        /// Reads data from catalog file as <see cref="Tycho2Star" /> instance. 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="starId"></param>
        /// <param name="eqCenter"></param>
        /// <param name="angle"></param>
        /// <remarks>
        /// Data in file saved in form:
        /// 
        /// Tyc1  = 2 bytes = 
        /// Tyc2  = 2
        /// Tyc3  = 1
        /// ra    = 8
        /// dec   = 8
        /// pmRA  = 4
        /// pmDec = 4
        /// mag   = 4
        /// </remarks>
        private Tycho2Star ParseStarData(byte[] buffer, int offset, CrdsEquatorial eqCenter, double angle, double years, float magLimit)
        {
            float mag = BitConverter.ToSingle(buffer, offset + 29);
            if (mag <= magLimit)
            {
                // Star coordinates at epoch J2000.0 
                var eq0 = new CrdsEquatorial(
                    BitConverter.ToDouble(buffer, offset + 5),
                    BitConverter.ToDouble(buffer, offset + 13));

                // Take into account proper motion
                var pm = new CrdsEquatorial(
                    BitConverter.ToSingle(buffer, offset + 21) * years / 3600000.0,
                    BitConverter.ToSingle(buffer, offset + 25) * years / 3600000.0);

                eq0 += pm;

                if (Angle.Separation(eq0, eqCenter) <= angle)
                {
                    Tycho2Star star = new Tycho2Star();
                    star.Equatorial0 = eq0;
                    star.Tyc1 = BitConverter.ToInt16(buffer, offset);
                    star.Tyc2 = BitConverter.ToInt16(buffer, offset + 2);
                    star.Tyc3 = (char)buffer[offset + 4];
                    star.Magnitude = mag;
                    return star;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public override void Calculate(SkyContext context)
        {
            
        }

        public void ConfigureEphemeris(EphemerisConfig<Tycho2Star> config)
        {
            // TODO
        }

        public CelestialObjectInfo GetInfo(SkyContext context, Tycho2Star body)
        {
            throw new NotImplementedException();
        }

        public ICollection<SearchResultItem> Search(string searchString, int maxCount = 50)
        {
            List<SearchResultItem> items = new List<SearchResultItem>();

            Regex regex = new Regex("tyc\\s*(\\d+)(\\s*-\\s*|\\s+)(\\d+)(\\s*-\\s*|\\s+)(\\d+)");

            if (regex.IsMatch(searchString.ToLowerInvariant()))
            {
                var match = regex.Match(searchString.ToLowerInvariant());

                int tyc1 = int.Parse(match.Groups[1].Value);
                int tyc2 = int.Parse(match.Groups[3].Value);
                string tyc3 = match.Groups[5].Value;

                if (tyc1 > 0 && tyc1 <= 9537)
                {
                    Tycho2Region region = _Index[tyc1 - 1];
                    //List<Tycho2Star> stars = GetStarsInRegion(region, tyc2, tyc3);
                }
            }

            return items;
        }

        public string GetName(Tycho2Star star)
        {
            return star.ToString();
        }
    }
}
