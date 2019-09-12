using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WF = System.Windows.Forms;

namespace Planetarium.Controls
{
    public class ColorPicker : Control
    {
        public ColorPicker()
        {

        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            WF.ColorDialog dialog = new WF.ColorDialog()
            {
                Color = SelectedColor
            };

            if (WF.DialogResult.OK == dialog.ShowDialog())
            {
                SelectedColor = dialog.Color;
            }
        }

        public System.Drawing.Color SelectedColor
        {
            get
            {
                return (System.Drawing.Color)GetValue(SelectedColorProperty);
            }
            set
            {
                SetValue(SelectedColorProperty, value);
            }
        }

        public readonly static DependencyProperty SelectedColorProperty = DependencyProperty.Register(
            nameof(SelectedColor), typeof(System.Drawing.Color), typeof(ColorPicker), new FrameworkPropertyMetadata(System.Drawing.Color.Transparent)
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                AffectsRender = true
            });

        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        public readonly static DependencyProperty CaptionProperty = DependencyProperty.Register(nameof(Caption), typeof(string), typeof(ColorPicker), new UIPropertyMetadata(null));
    }
}
