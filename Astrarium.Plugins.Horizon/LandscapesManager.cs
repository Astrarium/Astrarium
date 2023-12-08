using Astrarium.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Astrarium.Plugins.Horizon
{
    public class Landscape
    {
        [JsonIgnore]
        public string Path { get; set; }

        [JsonIgnore]
        public bool UserDefined { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Copyright { get; set; }
        public string Author { get; set; }
        public string URL { get; set; }
    }

    public interface ILandscapesManager
    {
        ICollection<Landscape> Landscapes { get; }

        Landscape CreateLandscape(string filePath);
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
            string targetFile = Path.Combine(userLandscapesPath, Path.GetFileName(imageFilePath));
            File.Copy(imageFilePath, targetFile, true);
            Landscape landscape = ReadLandscapeMetadata(targetFile);
            landscape.UserDefined = true;
            Landscapes.Add(landscape);
            return landscape;
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
