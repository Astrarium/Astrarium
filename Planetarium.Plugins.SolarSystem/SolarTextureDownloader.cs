using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Renderers
{
    /// <summary>
    /// This class is responsible for downloading latest solar images from web URL. 
    /// </summary>
    internal class SolarTextureDownloader
    {
        private static readonly string TempPath = Path.GetTempPath();
        private static readonly string SunImagesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ADK", "SunImages");

        internal SolarTextureDownloader()
        {
            if (!Directory.Exists(SunImagesPath))
            {
                try
                {
                    Directory.CreateDirectory(SunImagesPath);
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Unable to create directory for sun images: {SunImagesPath}, Details: {ex}");
                }
            }
        }

        public Image Download(string url)
        {
            // path to the source image (BMP) dowloaded from the web, located in the temp directory
            string bmpImageFile = Path.Combine(TempPath, Path.GetFileName(url));

            // path to cached destination image (PNG), cropped and with transparent background
            string pngImageFile = Path.Combine(SunImagesPath, Path.GetFileNameWithoutExtension(url) + ".png");

            try
            {
                // if image is already in cache, return it
                if (File.Exists(pngImageFile))
                {
                    return Image.FromFile(pngImageFile);
                }

                // download latest Solar image from provided URL
                using (var client = new WebClient())
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol =
                        SecurityProtocolType.Tls |
                        SecurityProtocolType.Tls11 |
                        SecurityProtocolType.Tls12 |
                        SecurityProtocolType.Ssl3;

                    client.DownloadFile(new Uri(url), bmpImageFile);
                }
                
                // Prepare resulting circle image with transparent background
                using (var image = (Bitmap)Image.FromFile(bmpImageFile))
                {
                    // default value of crop factor
                    float cropFactor = 0.93f;

                    // find first non-black pixel position
                    for (int x = 0; x < image.Width / 2; x++)
                    {
                        var color = image.GetPixel(x, image.Height / 2);
                        if (color.A > 20)
                        {
                            int grayscaled = (color.R + color.G + color.B) / 3;
                            if (grayscaled > 20)
                            {
                                cropFactor = 1 - 2 * (float)(x + 2) / image.Width;
                                break;
                            }
                        }
                    }

                    Image result = new Bitmap(
                        (int)(image.Width * cropFactor),
                        (int)(image.Height * cropFactor),
                        PixelFormat.Format32bppArgb);

                    using (var g = Graphics.FromImage(result))
                    {
                        g.Clear(Color.Transparent);
                        g.SmoothingMode = SmoothingMode.AntiAlias;

                        using (var crop = new GraphicsPath())
                        {
                            g.TranslateTransform(
                                image.Width * cropFactor / 2,
                                image.Height * cropFactor / 2);

                            float cropMargin = 1e-3f;

                            crop.AddEllipse(
                                -image.Width * cropFactor / 2 * (1 - cropMargin),
                                -image.Height * cropFactor / 2 * (1 - cropMargin),
                                image.Width * cropFactor * (1 - cropMargin),
                                image.Height * cropFactor * (1 - cropMargin));

                            g.SetClip(crop);

                            g.DrawImage(image, -image.Width / 2, -image.Height / 2, image.Width, image.Height);
                        }
                    }

                    // save in cache folder
                    result.Save(pngImageFile, ImageFormat.Png);

                    return result;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Unable to download file from {url}, exception: {ex}");
                return null;
            }
            finally
            {
                // cleanup: delete source image, if exists
                if (File.Exists(bmpImageFile))
                {
                    try
                    {
                        File.Delete(bmpImageFile);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"Unable to delete file {bmpImageFile}, exception: {ex}");
                    }
                }
            }
        }
    }
}
