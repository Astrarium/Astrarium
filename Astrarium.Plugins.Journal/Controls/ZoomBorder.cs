using Astrarium.Algorithms;
using System;
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

        public bool FlipHorizontal()
        {
            if (child != null)
            {
                scaleTransform.ScaleX *= -1;
                return scaleTransform.ScaleX < 0;
            }
            return false;
        }

        public bool FlipVertical()
        {
            if (child != null)
            {
                scaleTransform.ScaleY *= -1;
                return scaleTransform.ScaleY < 0;
            }
            return false;
        }

        private void ResetSize(bool fit)
        {
            if (child != null)
            {
                // fit
                if (fit)
                {
                    // reset zoom
                    scaleTransform.ScaleX = 1 * Math.Sign(scaleTransform.ScaleX);
                    scaleTransform.ScaleY = 1 * Math.Sign(scaleTransform.ScaleY);
                }
                else
                {
                    int w = (child.Source as BitmapSource).PixelWidth;
                    int h = (child.Source as BitmapSource).PixelHeight;

                    double scaleX = w / ActualWidth;
                    double scaleY = h / ActualHeight;

                    double scale = Math.Max(scaleX, scaleY);

                    scaleTransform.ScaleX = Math.Sign(scaleTransform.ScaleX) * scale;
                    scaleTransform.ScaleY = Math.Sign(scaleTransform.ScaleY) * scale;
                }

                // reset pan
                translateTransform.X = 0;
                translateTransform.Y = 0;
            }
        }

        #region Child Events

        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!(e.Delta > 0) && (Math.Abs(scaleTransform.ScaleX) < .5 || Math.Abs(scaleTransform.ScaleY) < .5))
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
            if (child != null && child.Source != null)
            {
                double absoluteX;
                double absoluteY;

                int w = (child.Source as BitmapSource).PixelWidth;
                int h = (child.Source as BitmapSource).PixelHeight;

                relative.X -= ActualHeight / 2 * w / h;
                relative.Y -= ActualHeight / 2;

                absoluteX = relative.X * scaleTransform.ScaleX + translateTransform.X;
                absoluteY = relative.Y * scaleTransform.ScaleY + translateTransform.Y;

                double zoomCorrected = zoom * Math.Abs(scaleTransform.ScaleX);
                scaleTransform.ScaleX += Math.Sign(scaleTransform.ScaleX) * zoomCorrected;
                scaleTransform.ScaleY += Math.Sign(scaleTransform.ScaleY) * zoomCorrected;

                if (Math.Abs(scaleTransform.ScaleX) > 15)
                    scaleTransform.ScaleX = 15 * Math.Sign(scaleTransform.ScaleX);
                if (Math.Abs(scaleTransform.ScaleY) > 15)
                    scaleTransform.ScaleY = 15 * Math.Sign(scaleTransform.ScaleY);

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

                //v.X = v.X * Math.Cos(Angle.ToRadians(rotateTransform.Angle));
                //v.Y = v.Y * Math.Sin(Angle.ToRadians(rotateTransform.Angle));

                translateTransform.X = origin.X - v.X;
                translateTransform.Y = origin.Y - v.Y;
            }
        }

        #endregion
    }
}
