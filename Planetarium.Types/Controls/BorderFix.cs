using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Planetarium.Controls
{
    public sealed class BorderFix : ContentControl
    {
        static BorderFix()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BorderFix), new FrameworkPropertyMetadata(typeof(BorderFix)));
        }
    }
}
