using Astrarium.Plugins.ObservationsLog.Types;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ObservationsLog.Database
{
    public static class Storage
    {
        private static string DATABASE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "ObservationsLog", "Observations.db");
        private static string DATABASE_DIR = Path.GetDirectoryName(DATABASE_PATH);

        static Storage() 
        {
            if (!Directory.Exists(DATABASE_DIR))
            {
                Directory.CreateDirectory(DATABASE_DIR);
            }

            var mapper = BsonMapper.Global;

            mapper.Entity<Session>()
                .Id(x => x.Id)
                .DbRef(x => x.Site)
                .DbRef(x => x.Observer)
                .DbRef(x => x.Observations);

            mapper.Entity<Observation>()
                .Id(x => x.Id)
                .DbRef(x => x.Target);

            mapper.Entity<Target>()
                .Id(x => x.Id);

            mapper.Entity<Site>()
                .Id(x => x.Id);

            mapper.Entity<Observer>()
                .Id(x => x.Id);
        }

        public class SessInfo
        {
            public string Id { get; set; }
            public DateTime Date { get; set; }
        }

        public static IEnumerable<Session> GetSessions(Expression<Func<Session, bool>> filter)
        {
            using (var db = GetDB())
            {
                var dbSessions = db.GetCollection<Session>();
                var dbTargets = db.GetCollection<Target>();
                return dbSessions
                    .Include(s => s.Observations)
                    .Include(BsonExpression.Create("Observations[*].Target"))
                    .Include(s => s.Observer)
                    .Include(s => s.Site)
                    .Query()
                    .ToEnumerable()
                    .AsQueryable()
                    .Where(filter)
                    .ToArray();                
            }
        }

        public static void SaveSession(Session session)
        {
            using (var db = GetDB())
            {
                db.GetCollection<Session>().Upsert(session);
            }
        }

        public static void SaveTarget(Target target)
        {
            using (var db = GetDB())
            {
                db.GetCollection<Target>().Upsert(target);
            }
        }

        public static IEnumerable<Site> GetSites()
        {
            using (var db = GetDB())
            {
                return db.GetCollection<Site>().FindAll();
            }
        }

        public static void AddSessions(ICollection<Session> sessions)
        {
            using (var db = GetDB())
            {
                db.GetCollection<Session>().InsertBulk(sessions);

                var observations = sessions.SelectMany(s => s.Observations).Distinct(new EntityIdEqualityComparer<Observation>()).ToArray();
                db.GetCollection<Observation>().InsertBulk(observations);

                var observers = sessions.Select(s => s.Observer).Distinct(new EntityIdEqualityComparer<Observer>()).ToArray();
                db.GetCollection<Observer>().InsertBulk(observers);

                var targets = sessions.SelectMany(s => s.Observations.Select(o => o.Target).Concat(s.Observations.Select(t => t.Target).OfType<MultipleStarTarget>().SelectMany(ms => ms.Components))).Distinct(new EntityIdEqualityComparer<Target>()).ToArray();
                db.GetCollection<Target>().InsertBulk(targets);

                var sites = sessions.Select(s => s.Site).Distinct(new EntityIdEqualityComparer<Site>()).ToArray();
                db.GetCollection<Site>().InsertBulk(sites);
            }
        }

        public static Session GetSession(string id)
        {
            using (var db = GetDB())
            {
                var dbSessions = db.GetCollection<Session>();
                return dbSessions.FindById(id);
            }
        }


        private static ILiteDatabase GetDB()
        {
            var db = new LiteDatabase(DATABASE_PATH);

            var dbSessions = db.GetCollection<Session>();
            var dbObservers = db.GetCollection<Observer>();
            var dbObservations = db.GetCollection<Observation>();
            var dbSites = db.GetCollection<Site>();
            var dbTargets = db.GetCollection<Target>();

            dbSessions.EnsureIndex(i => i.Id);
            dbObservers.EnsureIndex(i => i.Id);
            dbObservations.EnsureIndex(i => i.Id);
            dbSites.EnsureIndex(i => i.Id);
            dbTargets.EnsureIndex(i => i.Id);

            return db;
        }
    }
}
