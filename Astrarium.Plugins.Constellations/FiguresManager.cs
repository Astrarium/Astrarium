using Astrarium.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Astrarium.Plugins.Constellations
{
    [Singleton]
    public class FiguresManager 
    {
        private object locker = new object();
        private readonly string RootFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "ConFigures");
        private readonly ISettings settings;

        private List<ConstellationFigure> figures = new List<ConstellationFigure>();
        public ICollection<ConstellationFigure> Figures
        {
            get
            {
                lock (locker)
                {
                    return figures;
                }
            }
        }

        public string Folder { get; private set; }
        public int DefaultBrightness { get; private set; }
        public int MaxBrightness { get; private set; }

        public FiguresManager(ISettings settings) 
        {
            this.settings = settings;
            this.settings.SettingValueChanged += Settings_SettingValueChanged;
        }

        private void Settings_SettingValueChanged(string name, object value)
        {
            if (name == "ConstFiguresType")
            {
                LoadFigures();
            }
            else if (name == "ConstFigures")
            {
                if (value is true)
                {
                    LoadFigures();
                }
                else if (value is false)
                {
                    UnloadFigures();
                }
            }
        }

        public void Initialize()
        {
            LoadFigures();
        }

        private void UnloadFigures()
        {
            lock (locker) 
            { 
                figures.ForEach(x => GL.RemoveTexture(x.File));
                figures.Clear();
            }
        }

        private void LoadFigures()
        {
            if (!settings.Get("ConstFigures")) return;

            var type = settings.Get<ConstellationsRenderer.FigureType>("ConstFiguresType");

            lock (locker)
            {
                Folder = Path.Combine(RootFolder, type.ToString());
                string path = Path.Combine(Folder, "figures.json");

                UnloadFigures();

                if (File.Exists(path))
                {
                    try
                    {
                        var jsonData = JsonConvert.DeserializeObject<JsonFigures>(File.ReadAllText(path));
                        DefaultBrightness = jsonData.DefaultBrightness;
                        MaxBrightness = jsonData.MaxBrightness;
                        figures = jsonData.Figures.Select(x => new ConstellationFigure(Folder, x)).ToList();
                    }
                    catch (Exception ex) 
                    {
                        Log.Error($"Error on loading constellation figures data from file: {path}. Error: {ex}");
                    }
                }
            }
        }
    }
}
