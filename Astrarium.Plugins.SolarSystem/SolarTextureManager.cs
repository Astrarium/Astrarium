using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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
        /// Id of current solar texture. -1 means texture is not available or not downloaded yet.
        /// </summary>
        private int textureId = -1;

        /// <summary>
        /// Current date stamp (i.e. date of solar image used)
        /// </summary>
        private DateTime currentDate = DateTime.MinValue;

        /// <summary>
        /// Action to be called when texture is ready
        /// </summary>
        public Action FallbackAction { get; set; }

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
            
            if (!dt.Equals(currentDate) || textureId == -1)
            {
                Task.Run(() => RequestSolarImage(dt));
            }

            return textureId;
        }

        private void RequestSolarImage(DateTime dt)
        {
            Image image = GetSolarImage(dt);
            
            if (image != null)
            {
                using (Bitmap bmp = (Bitmap)image)
                {
                    BitmapData data = bmp.LockBits(new Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    Application.Current.Dispatcher.Invoke(() => BindTexture(data));
                    bmp.UnlockBits(data);
                }
                currentDate = dt;

            }
            else
            {
                Application.Current.Dispatcher.Invoke(UnbindTexture);
            }
        }

        private void BindTexture(BitmapData data)
        {
            if (textureId == -1)
            {
                // this texture id will be used all time
                textureId = GL.GenTexture();
            }

            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                data.Width, data.Height, 0, Astrarium.Types.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            if (FallbackAction != null)
            {
                FallbackAction.Invoke();
            }
        }

        private void UnbindTexture()
        {
            GL.DeleteTexture(textureId);
            textureId = -1;

            if (FallbackAction != null)
            {
                FallbackAction.Invoke();
            }
        }

        private Image GetSolarImage(DateTime date)
        {
            // obviously there is no image for the future dates
            if (date.Date > DateTime.UtcNow.Date)
            {
                return null;
            }
            
            // there are no images for dates prior 19th May 1996
            if (date.Date < new DateTime(1996, 5, 19, 0, 0, 0, DateTimeKind.Utc))
            {
                return null;
            }

            // path to cached destination image (PNG), cropped and with transparent background
            string pngImageFile = Path.Combine(SunImagesPath, $"{date:yyyyMMdd}.png");

            // if image is already in cache, return it
            if (File.Exists(pngImageFile))
            {
                try
                {
                    return Image.FromFile(pngImageFile);
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to read image from file {pngImageFile}, exception: {ex}");
                    return null;
                }
            }

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
                        return null;
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

                    // save in cache folder
                    result.Save(pngImageFile, ImageFormat.Png);

                    return result;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to download file from {url}, exception: {ex}");
                return null;
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
