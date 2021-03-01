using Astrarium.Plugins.ObservationsLog.Database;
using Astrarium.Plugins.ObservationsLog.OAL;
using Astrarium.Plugins.ObservationsLog.Types;
using Astrarium.Plugins.ObservationsLog.ViewModels;
using Astrarium.Types;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Astrarium.Plugins.ObservationsLog
{
    public class Plugin : AbstractPlugin
    {
        public Plugin()
        {
            var topMenu = new MenuItem("Journal");
            topMenu.SubItems.Add(new MenuItem("Import...", new Command(ImportOAL)));
            topMenu.SubItems.Add(new MenuItem("Find", new Command(Find)));
            topMenu.SubItems.Add(new MenuItem("Show Log", new Command(Show)));

            MenuItems.Add(MenuItemPosition.MainMenuTop, topMenu);
        }

        private void Find()
        {
            //var sessions = Storage.GetSessions(s => s.Observations.Any(o => o.Target.Name == "mm"));

            //var target = sessions.ElementAt(0).Observations[0].Target;

            //target.Name = "mm";

            //Storage.SaveTarget(target);

        }

        private void Show()
        {
            var vm = new ObservationsLogVM();
            ViewManager.ShowWindow(vm);
        }

        private void ImportOAL()
        {
            var file = ViewManager.ShowOpenFileDialog("Import", "Open Astronomy Log files (*.xml)|*.xml|All files|*.*");
            if (file != null)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(observations));
                var stringData = File.ReadAllText(file);
                using (TextReader reader = new StringReader(stringData))
                {
                    var data = (observations)serializer.Deserialize(reader);
                    var sessions = data.ImportAll();
                    Storage.AddSessions(sessions);
                }
            }
        }
    }

    public static class LiteDBExtensions
    {
        public static ILiteQueryable<T> OfType<T>(this ILiteQueryable<T> queryable, Type type) 
        {
            var typeName = type.FullName + ", " + type.Assembly.GetName().Name;
            return queryable.Where(Query.EQ("_type", typeName));
        }
    }
}
