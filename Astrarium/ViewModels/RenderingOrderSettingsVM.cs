using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.ViewModels
{
    public class RenderingOrderSettingsVM : ViewModelBase
    {
        /// <summary>
        /// Settings instance
        /// </summary>
        public ISettings Settings { get; private set; }

        /// <summary>
        /// Collection of renderers
        /// </summary>
        public RenderingOrder Renderers { get; private set; }

        public RenderingOrderSettingsVM(ISettings settings)
        {
            Settings = settings;
            Renderers = settings.Get<RenderingOrder>("RenderingOrder");
        }

        
    }
}
