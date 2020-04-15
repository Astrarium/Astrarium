using Astrarium.Types;
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

namespace Astrarium.Types.Controls
{
    public class FilePathPicker : Control
    {
        public FilePathPicker()
        {

        }

        public enum PickerMode
        {
            File = 0,
            Directory = 1
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

        public PickerMode Mode
        {
            get { return (PickerMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        public readonly static DependencyProperty ModeProperty = DependencyProperty.Register(nameof(Mode), typeof(PickerMode), typeof(FilePathPicker), new UIPropertyMetadata(null));

        Button _Button;
        
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _Button = Template.FindName("Button", this) as Button;
            _Button.Click += Button_Click;           
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Mode == PickerMode.File)
            {
                // TODO: take caption and filter from control properties
                string path = ViewManager.ShowOpenFileDialog("Open file", "*.*");
                if (!string.IsNullOrEmpty(path))
                { 
                    SelectedPath = path;
                }
            }
            else if (Mode == PickerMode.Directory)
            {
                // TODO: take caption from control properties
                string path = ViewManager.ShowSelectFolderDialog("Choose folder", SelectedPath);
                if (!string.IsNullOrEmpty(path))
                {
                    SelectedPath = path;
                }
            }
        }
    }
}
