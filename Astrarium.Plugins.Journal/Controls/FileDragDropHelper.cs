using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Astrarium.Plugins.Journal.Controls
{
    /// <summary>
    /// FileDragDropHelper
    /// </summary>
    public class FileDragDropHelper
    {
        public static bool GetDropCommand(DependencyObject obj)
        {
            return (bool)obj.GetValue(DropCommandProperty);
        }

        public static void SetDropCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(DropCommandProperty, value);
        }

        private static void PropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as Control;
            if (control != null)
            {
                control.Drop -= OnDrop;
                control.Drop += OnDrop;
            }
        }

        public static readonly DependencyProperty DropCommandProperty =
                DependencyProperty.RegisterAttached("DropCommand", typeof(ICommand), typeof(FileDragDropHelper), new PropertyMetadata() { PropertyChangedCallback = PropChanged });

        private static void OnDrop(object sender, DragEventArgs dragEventArgs)
        {
            DependencyObject d = sender as DependencyObject;
            if (d == null) return;
            ICommand target = (ICommand)d.GetValue(DropCommandProperty);
            if (dragEventArgs.Data.GetDataPresent(DataFormats.FileDrop))
            {
                target.Execute((string[])dragEventArgs.Data.GetData(DataFormats.FileDrop));
            }
        }
    }
}
