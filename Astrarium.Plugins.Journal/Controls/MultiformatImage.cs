using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Astrarium.Plugins.Journal.Controls
{
    public class MultiformatImage : Image
    {
        private AutoResetEvent animationResetEvent = new AutoResetEvent(false);
        private string imageErrorText = null;

        public MultiformatImage()
        {
            this.Loaded += MultiformatImage_Loaded;
            this.Unloaded += MultiformatImage_Unloaded;
        }

        private void MultiformatImage_Loaded(object sender, RoutedEventArgs e)
        {
            animationResetEvent.Reset();
            ProcessImage();
        }

        private void MultiformatImage_Unloaded(object sender, RoutedEventArgs e)
        {
            animationResetEvent.Set();
        }

        ~MultiformatImage()
        {
            Dispatcher.Invoke(() => { Source = null; });
           
            animationResetEvent.Set();
        }

        private void ProcessImage()
        {
            try
            {
                if (Source != null)
                {
                    
                    Uri imageUri = new Uri(Source.ToString());

                    if (!File.Exists(imageUri.AbsolutePath))
                    {
                        imageErrorText = "Image not found";
                        InvalidateVisual();
                    }
                    else if (Path.GetExtension(imageUri.AbsolutePath).ToLower().Equals(".gif"))
                    {
                        ProcessGif(imageUri);
                    }
                    else
                    {
                        // TODO: process other types if required
                    }
                }
                else
                {
                    imageErrorText = "Image not found";
                    InvalidateVisual();
                }
            }
            catch
            {
                imageErrorText = "Bad image";
                InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext ctx)
        {
            if (imageErrorText != null)
            {
                DrawText(ctx, imageErrorText, new Point(ActualWidth  / 2, ActualHeight / 2), 14, Brushes.Red);
            }
            else if (Source != null)
            {
                base.OnRender(ctx);
            }
        }

        private void DrawText(DrawingContext ctx, string text, Point point, double size, Brush brush)
        {
            var typeface = new Typeface(new FontFamily("#Noto Sans"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            FormattedText formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, size, brush);
            ctx.DrawText(formattedText, new Point(point.X - formattedText.Width / 2, point.Y - formattedText.Height / 2));
        }

        private void ProcessGif(Uri imageUri)
        {
            var gifDecoder = new GifBitmapDecoder(imageUri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

            var frame = gifDecoder.Frames[0];
            int framesCount = gifDecoder.Frames.Count;
            int currentFrame = 0;

            var delay = (ushort)((BitmapMetadata)frame.Metadata).GetQuery("/grctlext/Delay") * 10;

            new Thread(() =>
            {
                do
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Source = gifDecoder.Frames[currentFrame];
                    });
                    currentFrame = (currentFrame + 1) % framesCount;
                }
                while (!animationResetEvent.WaitOne(delay));
            })
            { IsBackground = true }.Start();
        }
    }
}
