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
                if (value != null && value != this.Child)
                    this.Initialize(value);
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
                child.RenderTransformOrigin = new Point(0, 0);
                MouseWheel += child_MouseWheel;
                MouseLeftButtonDown += child_MouseLeftButtonDown;
                MouseLeftButtonUp += child_MouseLeftButtonUp;
                MouseMove += child_MouseMove;
                PreviewMouseRightButtonDown += new MouseButtonEventHandler(child_PreviewMouseRightButtonDown);
            }
        }


        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (child.Source != null)
            {
                int w = (child.Source as BitmapSource).PixelWidth;
                int h = (child.Source as BitmapSource).PixelHeight;
                translateTransform.X = (sizeInfo.NewSize.Width - w) / 2;
                translateTransform.Y = (sizeInfo.NewSize.Height - h) / 2;
            }
        }

        public void Reset()
        {
            if (child != null)
            {
                int w = (child.Source as BitmapSource).PixelWidth;
                int h = (child.Source as BitmapSource).PixelHeight;

                double scaleX = w / this.ActualWidth;
                double scaleY = h / this.ActualHeight;


                double scale = System.Math.Max(scaleX, scaleY);

                // reset zoom
                scaleTransform.ScaleX = scale;
                scaleTransform.ScaleY = scale;

                // reset pan
                translateTransform.X = (ActualWidth - w) / 2;
                translateTransform.Y = (ActualHeight - h) / 2;
            }
        }

        #region Child Events

        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (child != null)
            {
                double zoom = e.Delta > 0 ? .2 : -.2;
                if (!(e.Delta > 0) && (scaleTransform.ScaleX < .5 || scaleTransform.ScaleY < .5))
                    return;

                Point relative = e.GetPosition(child);
                double absoluteX;
                double absoluteY;

                absoluteX = relative.X * scaleTransform.ScaleX + translateTransform.X;
                absoluteY = relative.Y * scaleTransform.ScaleY + translateTransform.Y;

                double zoomCorrected = zoom * scaleTransform.ScaleX;
                scaleTransform.ScaleX += zoomCorrected;
                scaleTransform.ScaleY += zoomCorrected;

                if (scaleTransform.ScaleX > 15) scaleTransform.ScaleX = 15;
                if (scaleTransform.ScaleY > 15) scaleTransform.ScaleY = 15;

                translateTransform.X = absoluteX - relative.X * scaleTransform.ScaleX;
                translateTransform.Y = absoluteY - relative.Y * scaleTransform.ScaleY;

                //if (tt.X > 0) tt.X = 0;
                //if (tt.Y > 0) tt.Y = 0;
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

        void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Reset();
        }

        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    Vector v = start - e.GetPosition(this);
                    translateTransform.X = origin.X - v.X;
                    translateTransform.Y = origin.Y - v.Y;

                    //if (tt.X > 0) tt.X = 0;
                    //if (tt.Y > 0) tt.Y = 0;
                }
            }
        }

        #endregion
    }
}
