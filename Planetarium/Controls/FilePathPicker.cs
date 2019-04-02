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
    public class FilePathPicker : Control
    {
        public FilePathPicker()
        {

        }

        public string SelectedPath
        {
            get
            {
                return (string)GetValue(SelectedPathProperty);
            }
            set
            {
                SetValue(SelectedPathProperty, value);
            }
        }

        public readonly static DependencyProperty SelectedPathProperty = DependencyProperty.Register(
            nameof(SelectedPath), typeof(string), typeof(FilePathPicker), new FrameworkPropertyMetadata(string.Empty)
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

        public readonly static DependencyProperty CaptionProperty = DependencyProperty.Register(nameof(Caption), typeof(string), typeof(FilePathPicker), new UIPropertyMetadata(null));

        Button _Button;
        
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _Button = Template.FindName("Button", this) as Button;
            _Button.Click += Button_Click;           
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new WF.OpenFileDialog();
            if (WF.DialogResult.OK == dialog.ShowDialog())
            {
                SelectedPath = dialog.FileName;
            }
        }
    }
}
