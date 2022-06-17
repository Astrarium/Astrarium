using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            animationResetEvent.Set();
        }

        private void ProcessImage()
        {
            try
            {
                Uri imageUri = new Uri(Source.ToString());
                if (Path.GetExtension(imageUri.AbsolutePath).ToLower().Equals(".gif"))
                {
                    ProcessGif(imageUri);
                }
                // TODO: process other types if required
            }
            catch
            {
                
            }
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
