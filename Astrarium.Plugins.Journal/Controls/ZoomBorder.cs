using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Astrarium.Plugins.Journal.Controls
{
    public class ZoomBorder : Border
    {
        private MultiformatImage child = null;
        private Point origin;
        private Point start;
        private ScaleTransform scaleTransform = new ScaleTransform();
        private TranslateTransform translateTransform = new TranslateTransform();

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && value != child)
                    Initialize(value);
                base.Child = value;
            }
        }

        public void Initialize(UIElement element)
        {
            child = (MultiformatImage)element;
            if (child != null)
            {
                TransformGroup group = new TransformGroup();
                group.Children.Add(scaleTransform);
                group.Children.Add(translateTransform);
                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.5, 0.5);
                MouseWheel += child_MouseWheel;
                MouseLeftButtonDown += child_MouseLeftButtonDown;
                MouseLeftButtonUp += child_MouseLeftButtonUp;
                MouseMove += child_MouseMove;
            }
        }

        public void ZoomIn()
        {
            Zoom(0.2, new Point(0, 0));

            // reset pan
            translateTransform.X = 0;
            translateTransform.Y = 0;
        }

        public void ZoomOut()
        {
            Zoom(-0.2, new Point(0, 0));

            // reset pan
            translateTransform.X = 0;
            translateTransform.Y = 0;
        }

        public void FitToWindow()
        {
            ResetSize(true);
        }

        public void SetActualSize()
        {
            ResetSize(false);
        }

        private void ResetSize(bool fit)
        {
            if (child != null)
            {
                // fit
                if (fit)
                {
                    // reset zoom
                    scaleTransform.ScaleX = 1;
                    scaleTransform.ScaleY = 1;
                }
                else
                {
                    int w = (child.Source as BitmapSource).PixelWidth;
                    int h = (child.Source as BitmapSource).PixelHeight;

                    double scaleX = w / ActualWidth;
                    double scaleY = h / ActualHeight;

                    double scale = System.Math.Max(scaleX, scaleY);

                    scaleTransform.ScaleX = scale;
                    scaleTransform.ScaleY = scale;
                }

                // reset pan
                translateTransform.X = 0;
                translateTransform.Y = 0;
            }
        }

        #region Child Events

        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!(e.Delta > 0) && (scaleTransform.ScaleX < .5 || scaleTransform.ScaleY < .5))
                return;

            if (child != null)
            {
                Point relative = e.GetPosition(child);
                double zoom = e.Delta > 0 ? .2 : -.2;
                Zoom(zoom, relative);
            }
        }

        private void Zoom(double zoom, Point relative)
        {
            if (child != null)
            {
                double absoluteX;
                double absoluteY;

                int w = (child.Source as BitmapSource).PixelWidth;
                int h = (child.Source as BitmapSource).PixelHeight;

                relative.X -= ActualHeight / 2 * w / h;
                relative.Y -= ActualHeight / 2;

                absoluteX = relative.X * scaleTransform.ScaleX + translateTransform.X;
                absoluteY = relative.Y * scaleTransform.ScaleY + translateTransform.Y ;

                double zoomCorrected = zoom * scaleTransform.ScaleX;
                scaleTransform.ScaleX += zoomCorrected;
                scaleTransform.ScaleY += zoomCorrected;

                if (scaleTransform.ScaleX > 15)
                    scaleTransform.ScaleX = 15;
                if (scaleTransform.ScaleY > 15)
                    scaleTransform.ScaleY = 15;

                translateTransform.X = absoluteX - relative.X * scaleTransform.ScaleX;
                translateTransform.Y = absoluteY - relative.Y * scaleTransform.ScaleY;
            }
        }

        private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                start = e.GetPosition(this);
                origin = new Point(translateTransform.X, translateTransform.Y);
                Cursor = Cursors.Hand;
                child.CaptureMouse();
            }
        }

        private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                child.ReleaseMouseCapture();
                Cursor = Cursors.Arrow;
            }
        }

        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null && child.IsMouseCaptured)
            {
                Vector v = start - e.GetPosition(this);
                translateTransform.X = origin.X - v.X;
                translateTransform.Y = origin.Y - v.Y;
            }
        }

        #endregion
    }
}
