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
        public Font SelectedFont
        {
            get => (Font)GetValue(SelectedFontProperty);
            set => SetValue(SelectedFontProperty, value);
        }

        public readonly static DependencyProperty SelectedFontProperty = DependencyProperty.Register(
            nameof(SelectedFont), typeof(Font), typeof(FontPicker), new FrameworkPropertyMetadata(System.Drawing.SystemFonts.DefaultFont)
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                AffectsRender = true
            });

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            (Template.FindName("Container", this) as UIElement).PreviewMouseDown += OnMouseDown;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
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
}
