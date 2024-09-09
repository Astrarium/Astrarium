using System.Windows;

namespace Astrarium.Plugins.Journal.Views
{
    /// <summary>
    /// Interaction logic for AttachmentWindow.xaml
    /// </summary>
    public partial class AttachmentWindow : Window
    {
        public AttachmentWindow()
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
