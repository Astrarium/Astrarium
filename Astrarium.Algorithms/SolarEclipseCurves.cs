using System.Collections.Generic;

namespace Astrarium.Algorithms
{
    public class SolarEclipseCurves
    {
        public List<CrdsGeographical> UmbraPath { get; } = new List<CrdsGeographical>();
        public List<CrdsGeographical> UmbraNorthernLimit { get; } = new List<CrdsGeographical>();
        public List<CrdsGeographical> UmbraSouthernLimit { get; } = new List<CrdsGeographical>();
        public List<CrdsGeographical> RiseSetCurves { get; } = new List<CrdsGeographical>();
        public List<CrdsGeographical> PenumbraNorthernLimit { get; } = new List<CrdsGeographical>();
        public List<CrdsGeographical> PenumbraSouthernLimit { get; } = new List<CrdsGeographical>();
    }
}
