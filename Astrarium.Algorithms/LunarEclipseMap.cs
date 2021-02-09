using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Algorithms
{
    public class LunarEclipseMap
    {
        public IList<CrdsGeographical> P1 { get; set; } = new CrdsGeographical[0];
        public IList<CrdsGeographical> U1 { get; set; } = new CrdsGeographical[0];
        public IList<CrdsGeographical> U2 { get; set; } = new CrdsGeographical[0];
        public IList<CrdsGeographical> U3 { get; set; } = new CrdsGeographical[0];
        public IList<CrdsGeographical> U4 { get; set; } = new CrdsGeographical[0];
        public IList<CrdsGeographical> P4 { get; set; } = new CrdsGeographical[0];
    }
}
