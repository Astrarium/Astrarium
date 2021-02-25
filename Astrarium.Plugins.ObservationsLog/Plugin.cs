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

            MenuItems.Add(MenuItemPosition.MainMenuTop, topMenu);
        }

        private void ImportOAL()
        {
            var file = ViewManager.ShowOpenFileDialog("Import", "Open Astronomy Log files (*.xml)|*.xml|All files|*.*");
            if (file != null)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(OAL.observations));
                var stringData = File.ReadAllText(file);
                using (TextReader reader = new StringReader(stringData))
                {
                    var data = (OAL.observations)serializer.Deserialize(reader);

                    var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "ObservationsLog", "Observations.db");
                    var dbDir = Path.GetDirectoryName(dbPath);
                    if (Directory.Exists(dbDir))
                    {
                        Directory.CreateDirectory(dbDir);
                    }

                    using (var db = new LiteDatabase(dbPath))
                    {
                        var observers = db.GetCollection<OAL.observerType>("observers");
                        var sites = db.GetCollection<OAL.siteType>("sites");
                        var sessions = db.GetCollection<OAL.sessionType>("sessions");
                        var targets = db.GetCollection<OAL.observationTargetType>("targets");
                        var scopes = db.GetCollection<OAL.opticsType>("scopes");
                        var eyepieces = db.GetCollection<OAL.eyepieceType>("eyepieces");
                        var lenses = db.GetCollection<OAL.lensType>("lenses");
                        var filters = db.GetCollection<OAL.filterType>("filters");
                        var imagers = db.GetCollection<OAL.imagerType>("imagers");
                        var observations = db.GetCollection<OAL.observationType>("observations");

                        /*
                        observers.EnsureIndex(i => i.id);
                        observers.InsertBulk(data.observers);

                        sites.EnsureIndex(i => i.id);
                        sites.InsertBulk(data.sites);

                        sessions.EnsureIndex(i => i.id);
                        sessions.InsertBulk(data.sessions);

                        targets.EnsureIndex(i => i.id);
                        targets.InsertBulk(data.targets);

                        scopes.EnsureIndex(i => i.id);
                        scopes.InsertBulk(data.scopes);

                        eyepieces.EnsureIndex(i => i.id);
                        eyepieces.InsertBulk(data.eyepieces);

                        lenses.EnsureIndex(i => i.id);
                        lenses.InsertBulk(data.lenses);

                        filters.EnsureIndex(i => i.id);
                        filters.InsertBulk(data.filters);

                        imagers.EnsureIndex(i => i.id);
                        imagers.InsertBulk(data.imagers);

                        observations.EnsureIndex(i => i.id);
                        observations.InsertBulk(data.observation);
                        */
                        // var planets = targets.Query().OfType<OAL.Types.PlanetTargetType, OAL.observationTargetType>().ToArray();
                        var pl = targets.Query().OfType(typeof(OAL.deepSkyOC)).Where(t => ((OAL.deepSkyOC)t).@class.StartsWith("II.3.r")).ToArray();


                        var Observers = data.observers.Select(i => OAL.Mappings.FromOAL(i)).ToArray();
                    }
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
