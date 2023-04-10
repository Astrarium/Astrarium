using Astrarium.Plugins.Journal.Database;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Astrarium.Types;
using Astrarium.Plugins.Journal.Database.Entities;
using System.Collections;
using System.Reflection;

namespace Astrarium.Plugins.Journal.Types
{
    [Singleton(typeof(IDatabaseManager))]
    public class DatabaseManager : IDatabaseManager
    {
        private readonly string rootPath = JournalPlugin.PluginDataPath;

        public DatabaseManager()
        {
            Log.Debug("DB manager created");
        }

        public Task<List<Session>> GetSessions()
        {
            return Task.Run(() =>
            {
                var sessions = new List<Session>();

                using (var db = new DatabaseContext())
                {
                    var dbSessions = db.Sessions
                        .Include(x => x.Observations)
                        .Include(x => x.Observations.Select(o => o.Target))
                        .AsNoTracking()
                        .OrderByDescending(x => x.Begin)
                        .ToList();

                    foreach (var s in dbSessions)
                    {
                        var session = new Session(s.Id)
                        {
                            Begin = DateTimeOffset.Parse(s.Begin),
                            End = DateTimeOffset.Parse(s.End)
                        };

                        var observations = s.Observations.OrderByDescending(x => x.Begin);
                        foreach (var obs in observations)
                        {
                            session.Observations.Add(new Observation(obs.Id)
                            {
                                Session = session,
                                Begin = DateTimeOffset.Parse(obs.Begin),
                                End = DateTimeOffset.Parse(obs.End),
                                ObjectName = obs.Target.Name,
                                ObjectCommonName = obs.Target.CommonName,
                                ObjectType = obs.Target.Type,
                                ObjectNameAliases = DeserializeAliases(obs.Target.Aliases)
                            });
                        }

                        sessions.Add(session);
                    }
                }

                return sessions;
            });
        }

        public Task LoadSession(Session session)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var s = db.Sessions.Include(x => x.Attachments).FirstOrDefault(x => x.Id == session.Id);

                    session.SiteId = s.SiteId;
                    session.Weather = s.Weather;
                    session.Seeing = s.Seeing;
                    session.FaintestStar = s.FaintestStar != null ? (decimal)s.FaintestStar.Value : 6m;
                    session.FaintestStarSpecified = s.FaintestStar != null;
                    session.SkyQuality = s.SkyQuality != null ? (decimal)s.SkyQuality.Value : 19m;
                    session.SkyQualitySpecified = s.SkyQuality != null;

                    session.Equipment = s.Equipment;
                    session.Comments = s.Comments;
                    session.Attachments = s.Attachments.ToArray().Select(x => new Attachment()
                    {
                        Id = x.Id,
                        FilePath = Path.GetFullPath(Path.Combine(rootPath, x.FilePath)),
                        Title = x.Title,
                        Comments = x.Comments
                    }).ToList();
                };
            });
        }

        public Task<ICollection<string>> GetSessionFiles(string sessionId)
        {
            return Task.Run(() =>
            {
                using (var ctx = new DatabaseContext())
                {
                    var files = new List<string>();
                    files.AddRange(ctx.Database.SqlQuery<string>($"SELECT \"FilePath\" FROM \"Attachments\" WHERE \"Id\" IN (SELECT \"AttachmentId\" FROM \"ObservationAttachments\" WHERE \"ObservationId\" IN (SELECT \"Id\" FROM \"Observations\" WHERE \"SessionId\" = @p0))", sessionId));
                    files.AddRange(ctx.Database.SqlQuery<string>($"SELECT \"FilePath\" FROM \"Attachments\" WHERE \"Id\" IN (SELECT \"AttachmentId\" FROM \"SessionAttachments\" WHERE \"SessionId\" = @p0)", sessionId));
                    return (ICollection<string>)files;
                }
            });
        }

        public Task<ICollection<string>> GetObservationFiles(string observationId)
        {
            return Task.Run(() =>
            {
                using (var ctx = new DatabaseContext())
                {
                    return (ICollection<string>)ctx.Database.SqlQuery<string>($"SELECT \"FilePath\" FROM \"Attachments\" WHERE \"Id\" IN (SELECT \"AttachmentId\" FROM \"ObservationAttachments\" WHERE \"ObservationId\" = @p0)", observationId).ToArray();
                }
            });
        }

        public Task LoadObservation(Observation observation)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var obs = db.Observations
                        .Include(x => x.Target)
                        .Include(x => x.Attachments)
                        .FirstOrDefault(x => x.Id == observation.Id);

                    observation.Findings = obs.Result;
                    observation.Accessories = obs.Accessories;
                    observation.TargetId = obs.TargetId;
                    observation.TargetNotes = obs.Target.Notes;
                    observation.Details = DeserializeObservationDetails(obs.Target.Type, obs.Details);
                    observation.TargetDetails = DeserializeTargetDetails(obs.Target.Type, obs.Target.Details);

                    observation.TelescopeId = obs.ScopeId;
                    observation.EyepieceId = obs.EyepieceId;
                    observation.LensId = obs.LensId;
                    observation.FilterId = obs.FilterId;
                    observation.CameraId = obs.CameraId;

                    observation.Attachments = obs.Attachments.ToArray().Select(x => new Attachment()
                    {
                        Id = x.Id,
                        FilePath = Path.GetFullPath(Path.Combine(rootPath, x.FilePath)),
                        Title = x.Title,
                        Comments = x.Comments
                    }).ToList();
                }
            });
        }

        public Task SaveDatabaseEntityProperty(object value, Type entityType, string column, object key)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var entity = db.Set(entityType).Find(key);
                    if (entity != null)
                    {
                        db.Entry(entity).Property(column).CurrentValue = value;
                        db.SaveChanges();
                    }
                }
            });
        }

        public Task SaveAttachment(Attachment attachment)
        {
            return Task.Run(() =>
            {
                using (var ctx = new DatabaseContext())
                {
                    var a = ctx.Attachments.FirstOrDefault(x => x.Id == attachment.Id);
                    a.Title = attachment.Title;
                    a.Comments = attachment.Comments;
                    ctx.SaveChanges();
                }
            });
        }

        public Task<Observation> CreateObservation(Session session, CelestialObject body, TargetDetails targetDetails, DateTime begin, DateTime end)
        {
            return Task.Run(() =>
            {
                var target = new TargetDB()
                {
                    Id = Guid.NewGuid().ToString(),
                    Aliases = JsonConvert.SerializeObject(body.Names),
                    Type = body.Type,
                    Name = body.Names.First(),
                    CommonName = body.CommonName,
                    Source = "Astrarium",
                    Details = JsonConvert.SerializeObject(targetDetails)
                };

                var observation = new ObservationDB()
                {
                    Id = Guid.NewGuid().ToString(),
                    Begin = begin.ToString(),
                    End = end.ToString(),
                    TargetId = target.Id,
                    SessionId = session.Id,
                    Details = JsonConvert.SerializeObject(CreateObservationDetails(body.Type))
                };

                using (var ctx = new DatabaseContext())
                {
                    ctx.Targets.Add(target);
                    ctx.Observations.Add(observation);
                    ctx.SaveChanges();
                }

                var obs = new Observation(observation.Id)
                {
                    Session = session,
                    Begin = DateTimeOffset.Parse(observation.Begin),
                    End = DateTimeOffset.Parse(observation.End),
                    ObjectName = observation.Target.Name,
                    ObjectType = observation.Target.Type,
                    ObjectNameAliases = DeserializeAliases(observation.Target.Aliases),
                    // TODO: save coordinates of the body
                    //TargetDetails = 
                };

                return obs;
            });
        }

        public Task EditObservation(Observation observation, CelestialObject body, DateTime begin, DateTime end)
        {
            return Task.Run(() =>
            {
                using (var ctx = new DatabaseContext())
                {
                    var observationDb = ctx.Observations.FirstOrDefault(x => x.Id == observation.Id);
                    if (observationDb != null)
                    {
                        observationDb.Begin = begin.ToString();
                        observationDb.End = end.ToString();
                        var targetDb = ctx.Targets.FirstOrDefault(x => x.Id == observationDb.TargetId);
                        if (targetDb != null)
                        {
                            // target has been changed
                            if (body.Type != targetDb.Type || body.CommonName != targetDb.CommonName)
                            {
                                targetDb.Aliases = JsonConvert.SerializeObject(body.Names);
                                targetDb.Type = body.Type;
                                targetDb.Name = body.Names.First();
                                targetDb.CommonName = body.CommonName;

                                // TODO: save coordinates of the body

                                targetDb.Source = "Astrarium";
                                // if target has been changed, we have to change observation details too
                                observationDb.Details = JsonConvert.SerializeObject(CreateObservationDetails(body.Type));
                            }
                        }

                        ctx.SaveChanges();

                        observation.Begin = begin;
                        observation.End = end;
                        observation.ObjectName = body.Names.First();
                        observation.ObjectType = body.Type;
                        observation.ObjectNameAliases = string.Join(", ", body.Names);
                    }
                }
            });
        }

        public Task DeleteObservation(string id)
        {
            return Task.Run(() =>
            {
                using (var ctx = new DatabaseContext())
                {
                    DbContextTransaction trans = null;

                    try
                    {
                        trans = ctx.Database.BeginTransaction();

                        // targets related to the observation
                        ctx.Database.ExecuteSqlCommand($"DELETE FROM \"Targets\" WHERE \"Id\" IN (SELECT \"TargetId\" FROM \"Observations\" WHERE \"Id\" = @p0)", id);

                        // attachments related to the observation
                        ctx.Database.ExecuteSqlCommand($"DELETE FROM \"Attachments\" WHERE \"Id\" IN (SELECT \"AttachmentId\" FROM \"ObservationAttachments\" WHERE \"ObservationId\" = @p0)", id);
                        ctx.Database.ExecuteSqlCommand($"DELETE FROM \"ObservationAttachments\" WHERE \"ObservationId\" = @p0", id);

                        // session itself
                        ctx.Database.ExecuteSqlCommand("DELETE FROM \"Observations\" WHERE \"Id\" = @p0", id);

                        trans.Commit();
                    }
                    catch
                    {
                        trans?.Rollback();
                    }
                }
            });
        }

        public Task DeleteSession(string id)
        {
            return Task.Run(() =>
            {
                using (var ctx = new DatabaseContext())
                {
                    DbContextTransaction trans = null;

                    try
                    {
                        trans = ctx.Database.BeginTransaction();

                        // targets related to the session's observations
                        ctx.Database.ExecuteSqlCommand($"DELETE FROM \"Targets\" WHERE \"Id\" IN (SELECT \"TargetId\" FROM \"Observations\" WHERE \"SessionId\" = @p0)", id);

                        // attachments related to the session's observations
                        ctx.Database.ExecuteSqlCommand($"DELETE FROM \"Attachments\" WHERE \"Id\" IN (SELECT \"AttachmentId\" FROM \"ObservationAttachments\" WHERE \"ObservationId\" IN (SELECT \"Id\" FROM \"Observations\" WHERE \"SessionId\" = @p0))", id);
                        ctx.Database.ExecuteSqlCommand($"DELETE FROM \"ObservationAttachments\" WHERE \"ObservationId\" IN (SELECT \"Id\" FROM \"Observations\" WHERE \"SessionId\" = @p0)", id);

                        // attachments related to the session
                        ctx.Database.ExecuteSqlCommand($"DELETE FROM \"Attachments\" WHERE \"Id\" IN (SELECT \"AttachmentId\" FROM \"SessionAttachments\" WHERE \"SessionId\" = @p0)", id);
                        ctx.Database.ExecuteSqlCommand($"DELETE FROM \"SessionAttachments\" WHERE \"SessionId\" = @p0", id);

                        // observations related to the session
                        ctx.Database.ExecuteSqlCommand("DELETE FROM \"Observations\" WHERE \"SessionId\" = @p0", id);

                        // session itself
                        ctx.Database.ExecuteSqlCommand("DELETE FROM \"Sessions\" WHERE \"Id\" = @p0", id);

                        trans.Commit();
                    }
                    catch
                    {
                        trans?.Rollback();
                    }
                }
            });
        }

        public Task<Site> GetSite(string id)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    return db.Sites.FirstOrDefault(x => x.Id == id)?.FromDBO();
                }
            });
        }

        public Task<ICollection> GetSites()
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var list = db.Sites.ToList();
                    list.Insert(0, SiteDB.Empty);
                    return (ICollection)list;
                }
            });
        }

        public Task<ICollection> GetOptics()
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var list = db.Optics.ToList();
                    list.Insert(0, OpticsDB.Empty);
                    return (ICollection)list;
                }
            });
        }

        public Task<Optics> GetOptics(string id)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    return db.Optics.FirstOrDefault(x => x.Id == id)?.FromDBO();
                }
            });
        }

        public Task SaveOptics(Optics optics)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var opticsDb = GetOrCreate<OpticsDB>(db, optics.Id);
                    optics.ToDBO(opticsDb);
                    db.SaveChanges();
                }
            });
        }

        public Task DeleteOptics(string id)
        {
            return Task.Run(() =>
            {
                using (var ctx = new DatabaseContext())
                {
                    DbContextTransaction trans = null;
                    try
                    {
                        trans = ctx.Database.BeginTransaction();
                        ctx.Database.ExecuteSqlCommand($"DELETE FROM [Optics] WHERE [Id] = @p0", id);
                        ctx.Database.ExecuteSqlCommand($"UPDATE [Observations] SET [ScopeId] = NULL WHERE [ScopeId] = @p0", id);
                        trans.Commit();
                    }
                    catch
                    {
                        trans?.Rollback();
                    }
                }
            });
        }

        public Task<ICollection> GetEyepieces()
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var list = db.Eyepieces.ToList();
                    list.Insert(0, EyepieceDB.Empty);
                    return (ICollection)list;
                }
            });
        }

        public Task SaveSite(Site site)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var siteDb = GetOrCreate<SiteDB>(db, site.Id);
                    site.ToDBO(siteDb);
                    db.SaveChanges();
                }
            });
        }

        /// <summary>
        /// Holder class to adapt observation target entry to CelestialObjectPicker control
        /// </summary>
        private class DummyCelestialObject : CelestialObject
        {
            public string TypeHolder { get; set; }
            public string CommonNameHolder { get; set; }
            public string NameHolder { get; set; }

            public override string[] Names => new string[] { NameHolder };
            public override string[] DisplaySettingNames => new string[0];
            public override string Type => TypeHolder;
            public override string CommonName => CommonNameHolder;
        }

        public Task<CelestialObject> GetTarget(string id)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var targetDb = db.Targets.FirstOrDefault(x => x.Id == id);
                    if (targetDb != null)
                    {
                        var target = new DummyCelestialObject()
                        {
                            NameHolder = targetDb.Name,
                            CommonNameHolder = targetDb.CommonName,
                            TypeHolder = targetDb.Type
                        };
                        return (CelestialObject)target;
                    }

                    return null;
                }
            });
        }

        public Task<ICollection> GetLenses()
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var list = db.Lenses.ToList();
                    list.Insert(0, LensDB.Empty);
                    return (ICollection)list;
                }
            });
        }

        public Task<Lens> GetLens(string id)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    return db.Lenses.FirstOrDefault(x => x.Id == id)?.FromDBO();
                }
            });
        }

        public Task SaveLens(Lens lens)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var lensDb = GetOrCreate<LensDB>(db, lens.Id);
                    lens.ToDBO(lensDb);
                    db.SaveChanges();
                }
            });
        }

        public Task DeleteLens(string id)
        {
            return Task.Run(() =>
            {
                using (var ctx = new DatabaseContext())
                {
                    DbContextTransaction trans = null;
                    try
                    {
                        trans = ctx.Database.BeginTransaction();
                        ctx.Database.ExecuteSqlCommand($"DELETE FROM [Lenses] WHERE [Id] = @p0", id);
                        ctx.Database.ExecuteSqlCommand($"UPDATE [Observations] SET [LensId] = NULL WHERE [LensId] = @p0", id);
                        trans.Commit();
                    }
                    catch
                    {
                        trans?.Rollback();
                    }
                }
            });
        }

        public Task SaveFilter(Filter filter)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var filterDb = GetOrCreate<FilterDB>(db, filter.Id);
                    filter.ToDBO(filterDb);
                    db.SaveChanges();
                }
            });
        }

        public Task<ICollection> GetFilters()
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var list = db.Filters.ToList();
                    list.Insert(0, FilterDB.Empty);
                    return (ICollection)list;
                }
            });
        }

        public Task<ICollection> GetCameras()
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var list = db.Cameras.ToList();
                    list.Insert(0, CameraDB.Empty);
                    return (ICollection)list;
                }
            });
        }

        public Task<Filter> GetFilter(string id)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    return db.Filters.FirstOrDefault(x => x.Id == id)?.FromDBO();
                }
            });
        }

        public Task DeleteFilter(string id)
        {
            return Task.Run(() =>
            {
                using (var ctx = new DatabaseContext())
                {
                    DbContextTransaction trans = null;
                    try
                    {
                        trans = ctx.Database.BeginTransaction();
                        ctx.Database.ExecuteSqlCommand($"DELETE FROM [Filters] WHERE [Id] = @p0", id);
                        ctx.Database.ExecuteSqlCommand($"UPDATE [Observations] SET [FilterId] = NULL WHERE [FilterId] = @p0", id);
                        trans.Commit();
                    }
                    catch
                    {
                        trans?.Rollback();
                    }
                }
            });
        }

        public Task<Eyepiece> GetEyepiece(string id)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    return db.Eyepieces.FirstOrDefault(x => x.Id == id)?.FromDBO();
                }
            });
        }

        public Task SaveEyepiece(Eyepiece eyepiece)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var eyepieceDb = GetOrCreate<EyepieceDB>(db, eyepiece.Id);
                    eyepiece.ToDBO(eyepieceDb);
                    db.SaveChanges();
                }
            });
        }

        public Task DeleteEyepiece(string id)
        {
            return Task.Run(() =>
            {
                using (var ctx = new DatabaseContext())
                {
                    DbContextTransaction trans = null;
                    try
                    {
                        trans = ctx.Database.BeginTransaction();
                        ctx.Database.ExecuteSqlCommand($"DELETE FROM [Eyepieces] WHERE [Id] = @p0", id);
                        ctx.Database.ExecuteSqlCommand($"UPDATE [Observations] SET [EyepieceId] = NULL WHERE [EyepieceId] = @p0", id);
                        trans.Commit();
                    }
                    catch
                    {
                        trans?.Rollback();
                    }
                }
            });
        }

        public Task<Camera> GetCamera(string id)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    return db.Cameras.FirstOrDefault(x => x.Id == id)?.FromDBO();
                }
            });
        }

        public Task SaveCamera(Camera camera)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var cameraDb = GetOrCreate<CameraDB>(db, camera.Id);
                    camera.ToDBO(cameraDb);
                    db.SaveChanges();
                }
            });
        }

        public Task DeleteCamera(string id)
        {
            return Task.Run(() =>
            {
                using (var ctx = new DatabaseContext())
                {
                    DbContextTransaction trans = null;
                    try
                    {
                        trans = ctx.Database.BeginTransaction();
                        ctx.Database.ExecuteSqlCommand($"DELETE FROM [Cameras] WHERE [Id] = @p0", id);
                        ctx.Database.ExecuteSqlCommand($"UPDATE [Observations] SET [CameraId] = NULL WHERE [CameraId] = @p0", id);
                        trans.Commit();
                    }
                    catch
                    {
                        trans?.Rollback();
                    }
                }
            });
        }

        private string DeserializeAliases(string aliases)
        {
            if (string.IsNullOrEmpty(aliases))
                return null;

            string value = string.Join(", ", JsonConvert.DeserializeObject<string[]>(aliases));
            if (string.IsNullOrEmpty(value))
                return null;
            else
                return value;
        }

        private ObservationDetails CreateObservationDetails(string targetType)
        {
            Type observationDetailsType = Assembly.GetAssembly(GetType()).GetTypes()
                .Where(x => typeof(ObservationDetails).IsAssignableFrom(x) && x.GetCustomAttributes<CelestialObjectTypeAttribute>()
                .Any(a => a.CelestialObjectType == targetType)).FirstOrDefault();

            if (observationDetailsType != null)
            {
                return (ObservationDetails)Activator.CreateInstance(observationDetailsType);
            }
            return null;
        }

        private ObservationDetails DeserializeObservationDetails(string targetType, string details)
        {
            if (details != null)
            {
                Type observationDetailsType = Assembly.GetAssembly(GetType()).GetTypes()
                    .Where(x => typeof(ObservationDetails).IsAssignableFrom(x) && x.GetCustomAttributes<CelestialObjectTypeAttribute>()
                    .Any(a => a.CelestialObjectType == targetType)).FirstOrDefault();

                if (observationDetailsType != null)
                {
                    return (ObservationDetails)JsonConvert.DeserializeObject(details, observationDetailsType);
                }
            }
            return null;
        }

        private TargetDetails DeserializeTargetDetails(string targetType, string details)
        {
            if (details != null)
            {
                Type targetDetailsType = Assembly.GetAssembly(GetType()).GetTypes()
                    .Where(x => typeof(TargetDetails).IsAssignableFrom(x) && x.GetCustomAttributes<CelestialObjectTypeAttribute>()
                    .Any(a => a.CelestialObjectType == targetType)).FirstOrDefault() ?? typeof(TargetDetails);

                if (targetDetailsType != null)
                {
                    return (TargetDetails)JsonConvert.DeserializeObject(details, targetDetailsType);
                }
            }
            return null;
        }

        private TEntity GetOrCreate<TEntity>(DatabaseContext ctx, string id) where TEntity : class, IEntity, new()
        {
            var entity = ctx.Set<TEntity>().FirstOrDefault(x => x.Id == id);
            if (entity == null)
            {
                entity = new TEntity() { Id = id };
                ctx.Set<TEntity>().Add(entity);
            }
            return entity;
        }
    }
}
