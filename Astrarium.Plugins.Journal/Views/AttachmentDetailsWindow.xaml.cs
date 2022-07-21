using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Astrarium.Plugins.Journal.Views
{
    /// <summary>
    /// Interaction logic for AttachmentDetailsWindow.xaml
    /// </summary>
    public partial class AttachmentDetailsWindow : Window
    {
        public AttachmentDetailsWindow()
        {
            InitializeComponent();
        }

        private void ZoomInContextMenu(object sender, RoutedEventArgs e)
        {
            zoomBorder.ZoomIn();
        }

        private void ZoomOutContextMenu(object sender, RoutedEventArgs e)
        {
            zoomBorder.ZoomOut();
        }

        private void ZoomIn(object sender, RoutedEventArgs e)
        {
            zoomBorder.ZoomIn();
        }

        private void ZoomOut(object sender, RoutedEventArgs e)
        {
            zoomBorder.ZoomOut();
        }

        private void FitToWindow(object sender, RoutedEventArgs e)
        {
            zoomBorder.FitToWindow();
        }

        private void SetActualSize(object sender, RoutedEventArgs e)
        {
            zoomBorder.SetActualSize();
        }

        private void FlipHorizontal(object sender, RoutedEventArgs e)
        {
            btnFlipHorizontal.IsChecked = mnuFlipHorizontal.IsChecked = zoomBorder.FlipHorizontal();
        }

        private void FlipVertical(object sender, RoutedEventArgs e)
        {
            btnFlipVertical.IsChecked = mnuFlipVertical.IsChecked = zoomBorder.FlipVertical();
        }
    }
}
