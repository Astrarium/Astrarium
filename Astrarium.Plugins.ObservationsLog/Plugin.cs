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

            MenuItems.Add(MenuItemPosition.MainMenuTop, topMenu);
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

                    using (var db = new LiteDatabase(dbPath))
                    {
                        var mapper = BsonMapper.Global;

                        mapper.Entity<Session>()
                            .Id(x => x.Id)
                            .DbRef(x => x.Observations);

                        mapper.Entity<Observation>()
                            .Id(x => x.Id)
                            .DbRef(x => x.Observer)
                            .DbRef(x => x.Target);
                            
                        var sessions = data.Import();

                        var dbObservers = db.GetCollection<Observer>("observers");
                        var dbObservations = db.GetCollection<Observation>("observations");
                        var dbSessions = db.GetCollection<Session>("sessions");
                        var dbTargets = db.GetCollection<Target>("targets");

                        dbObservers.EnsureIndex(i => i.Id);
                        dbObservations.EnsureIndex(i => i.Id);
                        dbSessions.EnsureIndex(i => i.Id);
                        dbTargets.EnsureIndex(i => i.Id);

                        // upload data
                        
                        dbSessions.InsertBulk(sessions);

                        var observations = sessions.SelectMany(s => s.Observations).Distinct(new EntityIdEqualityComparer<Observation>()).ToArray();                        
                        dbObservations.InsertBulk(observations);

                        var observers = sessions.SelectMany(s => s.Observations.Select(o => o.Observer)).Distinct(new EntityIdEqualityComparer<Observer>()).ToArray();
                        dbObservers.Insert(observers);

                        var targets = sessions.SelectMany(s => s.Observations.Select(o => o.Target)).Distinct(new EntityIdEqualityComparer<Target>()).ToArray();
                        dbTargets.Insert(targets);
                        
                        // var planets = targets.Query().OfType<OAL.Types.PlanetTargetType, OAL.observationTargetType>().ToArray();
                        // var pl = targets.Query().OfType(typeof(OAL.deepSkyOC)).Where(t => ((OAL.deepSkyOC)t).@class.StartsWith("II.3.r")).ToArray();


                        var obs = dbObservations.Query()
                            .Include(o => o.Target)
                            .Include(o => o.Observer)
                            .ToArray();
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
