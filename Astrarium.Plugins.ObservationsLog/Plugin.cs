using Astrarium.Plugins.ObservationsLog.OAL;
using Astrarium.Plugins.ObservationsLog.Types;
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

            MenuItems.Add(MenuItemPosition.MainMenuTop, topMenu);
        }

        private void Find()
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "ObservationsLog", "Observations.db");


            using (var db = new LiteDatabase(dbPath))
            {
                var dbSessions = db.GetCollection<Session>("sessions");
                //var dbObservations = db.GetCollection<Observation>("observations");

                var planetSessions = dbSessions.Query().ToEnumerable()
                        .Where(s => s.Observations.Any(o => o.Target is PlanetTarget)).ToArray();
            
            
            }
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

                    var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "ObservationsLog", "Observations.db");
                    var dbDir = Path.GetDirectoryName(dbPath);
                    if (!Directory.Exists(dbDir))
                    {
                        Directory.CreateDirectory(dbDir);
                    }

                    // TODO: for debug only!
                    if (File.Exists(dbPath))
                    {
                        File.Delete(dbPath);
                    }

                    using (var db = new LiteDatabase(dbPath))
                    {
                        var mapper = BsonMapper.Global;

                        /*
                        mapper.Entity<Session>()
                            .Id(x => x.Id)
                            .DbRef(x => x.Site)
                            .DbRef(x => x.Observer)
                            .DbRef(x => x.Observations);

                        mapper.Entity<Observation>()
                            .Id(x => x.Id)
                            .DbRef(x => x.Target);

                        mapper.Entity<MultipleStarTarget>()
                            .DbRef(x => x.Components, "targets");
                        */

                        var sessions = data.ImportAll();

                        var dbSessions = db.GetCollection<Session>("sessions");
                        /*
                        var dbObservers = db.GetCollection<Observer>("observers");
                        var dbObservations = db.GetCollection<Observation>("observations");
                        var dbSites = db.GetCollection<Site>("sites");
                        var dbTargets = db.GetCollection<Target>("targets");
                        */
                        dbSessions.EnsureIndex(i => i.Id);

                        /*
                        dbObservers.EnsureIndex(i => i.Id);
                        dbObservations.EnsureIndex(i => i.Id);
                        dbTargets.EnsureIndex(i => i.Id);
                        dbSites.EnsureIndex(i => i.Id);
                        */
                        // upload data
                        
                        dbSessions.InsertBulk(sessions);

                        /*
                        var observations = sessions.SelectMany(s => s.Observations).Distinct(new EntityIdEqualityComparer<Observation>()).ToArray();                        
                        dbObservations.InsertBulk(observations);

                        var observers = sessions.Select(s => s.Observer).Distinct(new EntityIdEqualityComparer<Observer>()).ToArray();
                        dbObservers.InsertBulk(observers);

                        var targets = sessions.SelectMany(s => s.Observations.Select(o => o.Target).Concat(s.Observations.Select(t => t.Target).OfType<MultipleStarTarget>().SelectMany(ms => ms.Components))).Distinct(new EntityIdEqualityComparer<Target>()).ToArray();
                        dbTargets.InsertBulk(targets);
                        
                        var sites = sessions.Select(s => s.Site).Distinct(new EntityIdEqualityComparer<Site>()).ToArray();
                        dbSites.InsertBulk(sites);
                        */
                        


                        // var planets = targets.Query().OfType<OAL.Types.PlanetTargetType, OAL.observationTargetType>().ToArray();
                        // var pl = targets.Query().OfType(typeof(OAL.deepSkyOC)).Where(t => ((OAL.deepSkyOC)t).@class.StartsWith("II.3.r")).ToArray();


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
