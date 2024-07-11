using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Astrarium.Plugins.SolarSystem
{
    /// <summary>
    /// This class is responsible for downloading latest solar images from web URL. 
    /// </summary>
    internal class SolarTextureManager
    {
        /// <summary>
        /// Where to obtain solar images
        /// </summary>
        private static readonly string DownloadUrlTemplate = "https://soho.nascom.nasa.gov/data/REPROCESSING/Completed/{yyyy}/{res}/{yyyy}{MM}{dd}/";

        /// <summary>
        /// Temp path for downloaded images
        /// </summary>
        private static readonly string TempPath = Path.GetTempPath();

        /// <summary>
        /// Where to store final images (cache folder) 
        /// </summary>
        private static readonly string SunImagesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "SunImages");

        /// <summary>
        /// Reset event for processing requests
        /// </summary>
        private AutoResetEvent requestEvent = new AutoResetEvent(false);

        /// <summary>
        /// Requests queue
        /// </summary>
        private ConcurrentQueue<DateTime> requests = new ConcurrentQueue<DateTime>();

        /// <summary>
        /// Fired on request complete
        /// </summary>
        internal event Action OnRequestComplete;

        internal SolarTextureManager()
        {
            if (!Directory.Exists(SunImagesPath))
            {
                try
                {
                    Directory.CreateDirectory(SunImagesPath);
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to create directory for sun images: {SunImagesPath}, Details: {ex}");
                }
            }

            new Thread(RequestWorker) { IsBackground = true }.Start();
        }

        /// <summary>
        /// Processes requests
        /// </summary>
        private void RequestWorker()
        {
            while (true)
            {
                requestEvent.WaitOne();

                while (requests.Any())
                {
                    if (requests.TryPeek(out DateTime dt))
                    {
                        RequestSolarImage(dt);
                        requests.TryDequeue(out dt);
                    }
                }

                OnRequestComplete?.Invoke();
            }
        }

        /// <summary>
        /// Gets full path to the solar image in cache folder
        /// </summary>
        private string GetImagePath(DateTime dt)
        {
            return Path.Combine(SunImagesPath, $"{dt:yyyyMMdd}.png");
        }

        /// <summary>
        /// Gets solar texture for julian date
        /// </summary>
        /// <param name="jd">Julian date</param>
        /// <returns></returns>
        public int GetTexture(double jd)
        {
            Date date = new Date(jd);
            DateTime dt = new DateTime(date.Year, date.Month, (int)date.Day, 0, 0, 0, DateTimeKind.Utc);

            string imagePath = GetImagePath(dt);

            if (File.Exists(imagePath))
            {
                return GL.GetTexture(imagePath, readyCallback: () => OnRequestComplete?.Invoke());
            }
            else
            {
                if (!requests.Contains(dt) && IsDateValid(dt))
                {
                    requests.Enqueue(dt);
                    requestEvent.Set();
                }

                return 0;
            }
        }

        /// <summary>
        /// Checks date is valid
        /// </summary>
        private bool IsDateValid(DateTime date)
        {
            // obviously there is no image for the future dates
            if (date.Date > DateTime.UtcNow.Date)
            {
                return false;
            }

            // there are no images for dates prior 19th May 1996
            if (date.Date < new DateTime(1996, 5, 19, 0, 0, 0, DateTimeKind.Utc))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets solar image from SOHO
        /// </summary>
        private void RequestSolarImage(DateTime date)
        {
            if (!IsDateValid(date)) return;

            // path to cached destination image (PNG), cropped and with transparent background
            string pngImageFile = GetImagePath(date);

            if (!File.Exists(pngImageFile))
            {
                // only mdiigr images available prior 2011
                string res = date.Year < 2011 ? "mdiigr" : "hmiigr";

                string format = Regex.Replace(DownloadUrlTemplate.Replace("{res}", res), "{([^}]*)}", match => "{0:" + match.Groups[1].Value + "}");
                string url = string.Format(format, date);

                // listing url
                string lstUrl = url + ".full_512.lst";

                // regex pattern
                string regexPattern = $"{date:yyyyMMdd}_\\d{{4}}_{res}_512\\.\\w+";

                string srcImageFile = null;
                string srcFileName = null;

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls |
                    SecurityProtocolType.Tls11 |
                    SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Ssl3;

                try
                {
                    using (var client = new WebClient())
                    {
                        try
                        {
                            string listing = client.DownloadString(lstUrl);
                            srcFileName = listing.Split('\n').Select(s => s.Trim()).FirstOrDefault(s => Regex.IsMatch(s, regexPattern));
                        }
                        catch (Exception ex)
                        {
                            Log.Debug($"Unable to list solar images for the date {date}. Reason: {ex.Message}");
                            string listing = client.DownloadString(url);
                            var match = Regex.Match(listing, regexPattern);
                            if (match.Success)
                            {
                                srcFileName = match.Value;
                            }
                        }

                        if (srcFileName == null)
                        {
                            Log.Debug($"There are no solar image file for the date {date}");
                            return;
                        }

                        // path to the source image dowloaded from the web, located in the temp directory
                        srcImageFile = Path.Combine(TempPath, srcFileName);

                        // download latest Solar image from provided URL
                        client.DownloadFile(new Uri(url + srcFileName), srcImageFile);
                    }

                    // Prepare resulting circle image with transparent background
                    using (var image = (Bitmap)Image.FromFile(srcImageFile))
                    {
                        // default value of crop factor
                        float cropFactor = 0.93f;

                        // find first non-black pixel position
                        for (int y = 0; y < image.Height / 2; y++)
                        {
                            var color = image.GetPixel(image.Width / 2, y);
                            int grayscaled = (color.R + color.G + color.B) / 3;
                            if (grayscaled > 20)
                            {
                                cropFactor = 1 - ((float)(y + 1) / (image.Height / 2));
                                break;
                            }
                        }

                        float w = image.Width * cropFactor;
                        float h = image.Height * cropFactor;

                        using (Image textureBrush = new Bitmap((int)w, (int)h, PixelFormat.Format32bppArgb))
                        {
                            using (var g = Graphics.FromImage(textureBrush))
                            {
                                g.Clear(Color.Transparent);
                                g.SmoothingMode = SmoothingMode.AntiAlias;
                                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                g.TranslateTransform(w / 2, h / 2);

                                using (var crop = new GraphicsPath())
                                {
                                    crop.AddEllipse(-w / 2, -h / 2, w, h);
                                    g.SetClip(crop);
                                    g.DrawImage(image, -image.Width / 2, -image.Height / 2, image.Width, image.Height);
                                }
                            }

                            using (Image result = new Bitmap(textureBrush.Width, textureBrush.Height, PixelFormat.Format32bppArgb))
                            {
                                using (var g = Graphics.FromImage(result))
                                {
                                    g.Clear(Color.Transparent);
                                    g.SmoothingMode = SmoothingMode.AntiAlias;
                                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                                    using (var crop = new GraphicsPath())
                                    {
                                        crop.AddEllipse(0, 0, result.Width, result.Height);
                                        Brush brush = new TextureBrush(textureBrush, WrapMode.Clamp);
                                        g.FillPath(brush, crop);
                                    }
                                }

                                // save in cache folder
                                result.Save(pngImageFile, ImageFormat.Png);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to download file from {url}, exception: {ex}");
                }
                finally
                {
                    // cleanup: delete source image, if exists
                    if (srcImageFile != null && File.Exists(srcImageFile))
                    {
                        try
                        {
                            File.Delete(srcImageFile);
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Unable to delete file {srcImageFile}, exception: {ex}");
                        }
                    }
                }
            }
        }
    }
}
