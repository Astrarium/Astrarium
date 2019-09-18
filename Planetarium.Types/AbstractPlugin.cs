using Planetarium.Config;
using Planetarium.Renderers;
using Planetarium.Types.Config.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Planetarium.Types
{
    public abstract class AbstractPlugin
    {
        public ICollection<SettingItem> SettingItems { get; } = new List<SettingItem>();
        public ICollection<ToolbarButton> ToolbarItems { get; } = new List<ToolbarButton>();
        public ICollection<Type> Renderers { get; } = new List<Type>();
        public ICollection<Type> Calculators { get; } = new List<Type>();
        public ICollection<Type> AstroEventProviders { get; } = new List<Type>();

        protected void AddSetting(SettingItem setting)
        {
            SettingItems.Add(setting);
        }

        protected void AddToolbarItem(ToolbarButton button)
        {
            ToolbarItems.Add(button);
        }

        protected void ExportResourceDictionaries(params string[] names)
        {
            string assemblyName = Assembly.GetAssembly(GetType()).FullName;
            foreach (string name in names)
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri($"/{assemblyName};component/{name}", UriKind.Relative) });
            }
        }

        protected void ExportRenderers(params Type[] renderers)
        {
            foreach (Type renderer in renderers)
            {
                if (!typeof(BaseRenderer).IsAssignableFrom(renderer))
                    throw new ArgumentException($"Type {renderer} must descend from {typeof(BaseRenderer)} class.", nameof(renderers));

                if (renderer.IsAbstract)
                    throw new ArgumentException($"Type {renderer} should not be an abstract class.", nameof(renderers));

                Renderers.Add(renderer);
            }
        }

        protected void ExportCalculators(params Type[] calculators)
        {
            foreach (Type calculator in calculators)
            {
                if (!typeof(BaseCalc).IsAssignableFrom(calculator))
                    throw new ArgumentException($"Type {calculator} must descend from {typeof(BaseCalc)} class.", nameof(calculators));

                if (calculator.IsAbstract)
                    throw new ArgumentException($"Type {calculator} should not be an abstract class.", nameof(calculators));

                Calculators.Add(calculator);
            }
        }

        protected void ExportAstroEventProviders(params Type[] astroEventProviders)
        {
            foreach (Type provider in astroEventProviders)
            {
                if (!typeof(BaseAstroEventsProvider).IsAssignableFrom(provider))
                    throw new ArgumentException($"Type {provider} must descend from {typeof(BaseAstroEventsProvider)} class.", nameof(astroEventProviders));

                if (provider.IsAbstract)
                    throw new ArgumentException($"Type {provider} should not be an abstract class.", nameof(astroEventProviders));

                AstroEventProviders.Add(provider);
            }
        }
    }
}
