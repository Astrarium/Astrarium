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
    public class FontPicker : Control
    {
        public FontPicker()
        {

        }

        public Font SelectedFont
        {
            get
            {
                return (Font)GetValue(SelectedFontProperty);
            }
            set
            {
                SetValue(SelectedFontProperty, value);
            }
        }

        public readonly static DependencyProperty SelectedFontProperty = DependencyProperty.Register(
            nameof(SelectedFont), typeof(Font), typeof(FontPicker), new FrameworkPropertyMetadata(System.Drawing.SystemFonts.DefaultFont)
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                AffectsRender = true
            });
        
        Button _Button;
        
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _Button = Template.FindName("Button", this) as Button;
            _Button.Click += Button_Click;           
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new WF.FontDialog()
            {
                Font = SelectedFont
            };
            if (WF.DialogResult.OK == dialog.ShowDialog())
            {
                SelectedFont = dialog.Font;
            }
        }
    }
}
