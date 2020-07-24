using System.Collections.Generic;
using System.Linq;

namespace Astrarium.Algorithms
{
    public class SolarEclipseCurves
    {
        public List<CrdsGeographical> UmbraPath { get; } = new List<CrdsGeographical>();
        public List<CrdsGeographical> UmbraNorthernLimit { get; } = new List<CrdsGeographical>();
        public List<CrdsGeographical> UmbraSouthernLimit { get; } = new List<CrdsGeographical>();
        public Curve RiseCurve { get; } = new Curve();
        public Curve SetCurve { get; } = new Curve();


        public List<CrdsGeographical> PenumbraNorthernLimit { get; } = new List<CrdsGeographical>();
        public List<CrdsGeographical> PenumbraSouthernLimit { get; } = new List<CrdsGeographical>();
    
        public class Curve : List<CrdsGeographical>
        {
            public new void Add(CrdsGeographical g)
            {
                var g1 = this.FirstOrDefault();
                var g2 = this.LastOrDefault();

                if (g1 != null && g2 != null)
                {
                    var d1 = Angle.Separation(g, g1);
                    var d2 = Angle.Separation(g, g2);

                    if (d1 <= d2)
                        Insert(0, g);
                    else
                        base.Add(g);
                }
                else
                    base.Add(g);
            }

            public void AddMany(IEnumerable<CrdsGeographical> gg)
            {
                foreach (var g in gg)
                {
                    this.Add(g);
                }
            }
        }
    }
}
