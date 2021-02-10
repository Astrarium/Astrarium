using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Algorithms
{
    public class LunarEclipseMap
    {
        public IList<CrdsGeographical> PenumbralBegin { get; set; } = new CrdsGeographical[0];
        public IList<CrdsGeographical> PartialBegin { get; set; } = new CrdsGeographical[0];
        public IList<CrdsGeographical> TotalBegin { get; set; } = new CrdsGeographical[0];
        public IList<CrdsGeographical> TotalEnd { get; set; } = new CrdsGeographical[0];
        public IList<CrdsGeographical> PartialEnd { get; set; } = new CrdsGeographical[0];
        public IList<CrdsGeographical> PenumbralEnd { get; set; } = new CrdsGeographical[0];
    }
}
