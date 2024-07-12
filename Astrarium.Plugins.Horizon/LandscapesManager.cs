using Astrarium.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Astrarium.Plugins.Horizon
{
    /// <summary>
    /// Defines landscape image used for rendering local horizon
    /// </summary>
    public class Landscape
    {
        /// <summary>
        /// Path to the image. Not serialized.
        /// </summary>
        [JsonIgnore]
        public string Path { get; set; }

        /// <summary>
        /// Flag indicating landscape is user-defined (i.e. can be edited and deleted). Not serialized.
        /// </summary>
        [JsonIgnore]
        public bool UserDefined { get; set; }

        /// <summary>
        /// Landscape shift relative to South direction, in degrees
        /// </summary>
        public double AzimuthShift { get; set; }

        /// <summary>
        /// Landscape title, should be unique.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Optional description, can include author name, copyrights, place of taken, external links and so on.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional array of landmarks.
        /// </summary>
        public Landmark[] Landmarks { get; set; }
    }

    /// <summary>
    /// Label to be rendered above landscape point.
    /// Used for mark
    /// </summary>
    public class Landmark
    {
        /// <summary>
        /// Landmark label
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Landmark altitude. For wide-length objects it's a starting point
        /// </summary>
        public double Altitude { get; set; }

        /// <summary>
        /// Landmark azimuth. For wide-length objects it's a starting point
        /// </summary>
        public double Azimuth { get; set; }

        /// <summary>
        /// Optional width of the landmark, in degrees. If set, the landmark object supposed to be a wide-length.
        /// </summary>
        public double? Width { get; set; }

        /// <summary>
        /// Optional color of the landmark label. Default is black.
        /// </summary>
        public Color? Color { get; set; }

        /// <summary>
        /// FOV to display the landmark. If not set, the landmark is displayed always.
        /// If set, the landmark will be displayed if current FOV is less or equal than specified value.
        /// </summary>
        public double? FOV { get; set; }
    }

    public interface ILandscapesManager
    {
        ICollection<Landscape> Landscapes { get; }
        Landscape CreateLandscape(string filePath);
        void SaveLandscapeMetadata(Landscape landscape);
    }

    [Singleton(typeof(ILandscapesManager))]
    public class LandscapesManager : ILandscapesManager
    {
        /// <summary>
        /// Base path to default landscapes directory deployed with the application
        /// </summary>
        private readonly string landscapesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Landscapes");

        /// <summary>
        /// Base path to user landscapes directory
        /// </summary>
        private readonly string userLandscapesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "Landscapes");

        public ICollection<Landscape> Landscapes { get; private set;  }

        public LandscapesManager()
        {
            Landscapes = new List<Landscape>(GetLandscapesFromDir(landscapesPath).Concat(GetLandscapesFromDir(userLandscapesPath, userDefined: true)));
        }

        public Landscape CreateLandscape(string imageFilePath)
        {
            Directory.CreateDirectory(userLandscapesPath);
            string targetFile = Path.Combine(userLandscapesPath, Path.GetFileName(imageFilePath));
            File.Copy(imageFilePath, targetFile, true);
            Landscape landscape = ReadLandscapeMetadata(targetFile);
            landscape.UserDefined = true;
            Landscapes.Add(landscape);
            return landscape;
        }

        public void SaveLandscapeMetadata(Landscape landscape)
        {
            string file = landscape.Path;
            string metadataFile = Path.Combine(Path.GetDirectoryName(file), $"{Path.GetFileNameWithoutExtension(file)}.json");
            Directory.CreateDirectory(userLandscapesPath);
            File.WriteAllText(metadataFile, JsonConvert.SerializeObject(landscape));
        }

        private ICollection<Landscape> GetLandscapesFromDir(string directory, bool userDefined = false)
        {
            List<Landscape> landscapes = new List<Landscape>();
            if (Directory.Exists(directory))
            {
                string[] files = Directory.GetFiles(directory, "*.png");
                foreach (string file in files)
                {
                    Landscape landscape = ReadLandscapeMetadata(file);
                    landscape.UserDefined = userDefined;
                    landscapes.Add(landscape);
                }
            }
            return landscapes;
        }

        private Landscape ReadLandscapeMetadata(string file)
        {
            string metadataFile = Path.Combine(Path.GetDirectoryName(file), $"{Path.GetFileNameWithoutExtension(file)}.json");
            Landscape landscape;
            if (File.Exists(metadataFile))
            {
                landscape = JsonConvert.DeserializeObject<Landscape>(File.ReadAllText(metadataFile));
            }
            else
            {
                landscape = new Landscape();
                landscape.Title = Path.GetFileNameWithoutExtension(file);
            }
            landscape.Path = file;
            return landscape;
        }
    }
}
