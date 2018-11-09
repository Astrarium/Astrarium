using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
    public interface IProjection
    {
        PointF Project(CrdsHorizontal hor);
        CrdsHorizontal Invert(PointF point);
    }
}
