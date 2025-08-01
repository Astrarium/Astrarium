using Astrarium.Projections;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.ViewModels
{
    public class SkyMapSettingsVM : ViewModelBase
    {
        /// <summary>
        /// Settings instance
        /// </summary>
        public ISettings Settings { get; private set; }

        private readonly ISkyMap map;

        public ProjectionVM[] Projections { get; private set; }

        public ProjectionVM Projection
        {
            get => GetValue<ProjectionVM>(nameof(Projection));
            set
            {
                SetValue(nameof(Projection), value);
                Settings.Set("Projection", value.Type.Name);
            }
        }

        public bool IsHorizontalMode
        {
            get => Settings.Get("ViewMode", ProjectionViewType.Horizontal) == ProjectionViewType.Horizontal;
            set
            {
                Settings.Set("ViewMode", value ? ProjectionViewType.Horizontal : ProjectionViewType.Equatorial);
                NotifyPropertyChanged(nameof(IsHorizontalMode), nameof(IsEquatorialMode));
            }
        }

        public bool IsEquatorialMode
        {
            get => Settings.Get("ViewMode", ProjectionViewType.Horizontal) == ProjectionViewType.Equatorial;
            set
            {
                Settings.Set("ViewMode", value ? ProjectionViewType.Equatorial : ProjectionViewType.Horizontal);
                NotifyPropertyChanged(nameof(IsHorizontalMode), nameof(IsEquatorialMode));
            }
        }

        public SkyMapSettingsVM(ISkyMap map, ISettings settings) 
        { 
            this.map = map;
            Settings = settings;

            Projections = System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Projection)) && !t.IsAbstract).Select(t => new ProjectionVM(t)).ToArray();

            string currentProjection = settings.Get("Projection", nameof(StereographicProjection));

            SetValue(nameof(Projection), Projections.FirstOrDefault(p => p.Type.Name == currentProjection));
        }
    }

    public class ProjectionVM
    {
        public string Name { get; private set; }
        public Type Type { get; private set; }

        public ProjectionVM(Type type)
        {
            Name = Text.Get($"Projection.{type.Name}");
            Type = type;
        }
    }
}
