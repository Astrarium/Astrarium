using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Astrarium.Types
{
    public class ObjectInfoExtension
    {
        public Func<CelestialObject, FrameworkElement> ViewProvider { get; private set; }
        public string Title { get; private set; }
        public ObjectInfoExtension(string title, Func<CelestialObject, FrameworkElement> viewProvider)
        {
            Title = title;
            ViewProvider = viewProvider;
        }
    }
}
