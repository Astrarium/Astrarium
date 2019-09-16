using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Planetarium.Types
{
    public class ToolbarButton
    {
        public string ImageKey { get; private set; }
        public object BindableObject { get; private set; }
        public string BindablePropertyName { get; private set; }
        public string ButtonTooltip { get; private set; }
        public string Group { get; private set;}

        public ToolbarButton(string buttonTooltip, string imageKey, object bindableObject, string bindablePropertyName, string group)
        {
            ImageKey = imageKey;
            BindableObject = bindableObject;
            BindablePropertyName = bindablePropertyName;
            ButtonTooltip = buttonTooltip;
            Group = group;
        }
    }
}
