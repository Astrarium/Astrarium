using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
    public class SolarTextureDownloader
    {
        public Image Download(string url, float cropFactor)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), "Sun.jpg");
            try
            {
                // Download latest Solar image from provided URL
                using (var client = new WebClient())
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol =
                        SecurityProtocolType.Tls |
                        SecurityProtocolType.Tls11 |
                        SecurityProtocolType.Tls12 |
                        SecurityProtocolType.Ssl3;
                    client.DownloadFile(new Uri(url), tempFile);
                }

                // Prepare resulting circle image with transparent background
                using (var image = Image.FromFile(tempFile))
                {
                    Image result = new Bitmap(
                        (int)(image.Width * cropFactor),
                        (int)(image.Height * cropFactor),
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

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

                    return result;
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    try
                    {
                        File.Delete(tempFile);
                    }
                    catch { }
                }
            }
        }
    }
}
