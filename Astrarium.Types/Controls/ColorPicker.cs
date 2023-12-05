using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WF = System.Windows.Forms;

namespace Astrarium.Types.Controls
{
    public class ColorPicker : Control
    {
        public ColorPicker()
        {

        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            Color? color = ViewManager.ShowColorDialog(Caption, SelectedColor);
            if (color != null)
            {
                SelectedColor = color.Value;
            }
        }

        public Color SelectedColor
        { 
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public readonly static DependencyProperty SelectedColorProperty = DependencyProperty.Register(
            nameof(SelectedColor), typeof(Color), typeof(ColorPicker), new FrameworkPropertyMetadata(Color.Black)
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                AffectsRender = true
            });

        public string Caption
        {
            get => (string)GetValue(CaptionProperty);
            set => SetValue(CaptionProperty, value);
        }

        public readonly static DependencyProperty CaptionProperty = DependencyProperty.Register(nameof(Caption), typeof(string), typeof(ColorPicker), new UIPropertyMetadata(null));
    }
}
