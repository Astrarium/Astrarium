using Astrarium.Plugins.Journal.Database;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Astrarium.Types;
using Astrarium.Algorithms;
using Astrarium.Plugins.Journal.Database.Entities;
using System.Windows;

namespace Astrarium.Plugins.Journal.Types
{
    public static class DatabaseManager
    {
        private static readonly string rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "Observations");

        public static Task<List<Session>> GetSessions()
        {
            return Task.Run(() =>
            {
                var sessions = new List<Session>();

                using (var db = new DatabaseContext())
                {
                    var dbSessions = db.Sessions
                        .Include(x => x.Observations)
                        .Include(x => x.Observations.Select(o => o.Target))
                        .OrderByDescending(x => x.Begin).ToArray();

                    foreach (var s in dbSessions)
                    {
                        var session = new Session(s.Id)
                        {
                            Begin = s.Begin,
                            End = s.End
                        };

                        var observations = s.Observations.OrderByDescending(x => x.Begin);
                        foreach (var obs in observations)
                        {
                            session.Observations.Add(new Observation(obs.Id)
                            {
                                Session = session,
                                Begin = obs.Begin,
                                End = obs.End,
                                ObjectName = obs.Target.Name,
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

        public static Task LoadSession(Session session)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var s = db.Sessions.Include(x => x.Attachments).FirstOrDefault(x => x.Id == session.Id);
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
                        FilePath = Path.Combine(rootPath, x.FilePath),
                        Title = x.Title,
                        Comments = x.Comments
                    }).ToList();
                }
            });
        }

        public static Task LoadObservation(Observation observation)
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
                    observation.Details = DeserializeObservationDetails(obs.Target.Type, obs.Details);
                    observation.TargetDetails = DeserializeTargetDetails(obs.Target.Type, obs.Target.Details);

                    observation.TelescopeId = obs.ScopeId;
                    observation.EyepieceId = obs.EyepieceId;
                    observation.LensId = obs.LensId;
                    observation.FilterId = obs.FilterId;
                    observation.CameraId = obs.ImagerId;

                    observation.Constellation = obs.Target?.Constellation;
                    observation.EquatorialCoordinates = obs.Target?.RightAscension != null && obs.Target?.Declination != null ? new CrdsEquatorial((double)obs.Target?.RightAscension.Value, (double)obs.Target?.Declination.Value) : null;

                    observation.Attachments = obs.Attachments.ToArray().Select(x => new Attachment()
                    {
                        Id = x.Id,
                        FilePath = Path.Combine(rootPath, x.FilePath),
                        Title = x.Title,
                        Comments = x.Comments
                    }).ToList();
                }
            });
        }

        public static async void SaveDatabaseEntityProperty(object value, Type entityType, string column, object key)
        {
            await Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var entity = db.Set(entityType).Find(key);
                    db.Entry(entity).Property(column).CurrentValue = value;
                    db.SaveChanges();
                }
            });
        }

        public static Task SaveAttachment(Attachment attachment)
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

        public static Task<Observation> CreateObservation(Session session, CelestialObject body, DateTime begin, DateTime end)
        {
            return Task.Run(() =>
            {
                var target = new TargetDB()
                {
                    Id = Guid.NewGuid().ToString(),
                    Aliases = JsonConvert.SerializeObject(body.Names),
                    Type = body.Type,
                    Name = body.Names.First(),
                    Source = "Astrarium"
                };

                var observation = new ObservationDB()
                {
                    Id = Guid.NewGuid().ToString(),
                    Begin = begin,
                    End = end,
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
                    Begin = observation.Begin,
                    End = observation.End,
                    ObjectName = observation.Target.Name,
                    ObjectType = observation.Target.Type,
                    ObjectNameAliases = DeserializeAliases(observation.Target.Aliases)
                };

                return obs;
            });
        }

        public static Task DeleteObservation(string id)
        {
            return Task.Run(() => {
                using (var ctx = new DatabaseContext())
                {
                    var existing = ctx.Observations.FirstOrDefault(x => x.Id == id);
                    if (existing != null)
                    {
                        ctx.Observations.Remove(existing);
                        ctx.SaveChanges();
                    }
                }
            });
        }

        private static string DeserializeAliases(string aliases)
        {
            if (string.IsNullOrEmpty(aliases))
                return null;

            string value = string.Join(", ", JsonConvert.DeserializeObject<string[]>(aliases));
            if (string.IsNullOrEmpty(value))
                return null;
            else
                return value;
        }

        private static PropertyChangedBase CreateObservationDetails(string targetType)
        {
            if (targetType == "VarStar" || targetType == "Nova")
            {
                return new VariableStarObservationDetails();
            }
            else if (targetType == "DeepSky.OpenCluster")
            {
                return new OpenClusterObservationDetails();
            }
            else if (targetType == "DeepSky.DoubleStar")
            {
                return new DoubleStarObservationDetails();
            }
            if (targetType.StartsWith("DeepSky"))
            {
                return new DeepSkyObservationDetails();
            }
            return null;
        }

        private static PropertyChangedBase DeserializeObservationDetails(string targetType, string details)
        {
            if (details != null)
            {
                if (targetType == "VarStar" || targetType == "Nova")
                {
                    return JsonConvert.DeserializeObject<VariableStarObservationDetails>(details);
                }
                else if (targetType == "DeepSky.OpenCluster")
                {
                    return JsonConvert.DeserializeObject<OpenClusterObservationDetails>(details);
                }
                else if (targetType == "DeepSky.DoubleStar")
                {
                    return JsonConvert.DeserializeObject<DoubleStarObservationDetails>(details);
                }

                if (targetType.StartsWith("DeepSky"))
                {
                    return JsonConvert.DeserializeObject<DeepSkyObservationDetails>(details);
                }
            }
            return null;
        }

        private static object DeserializeTargetDetails(string targetType, string details)
        {
            if (details != null)
            {
                if (targetType == "DeepSky.OpenCluster")
                {
                    return JsonConvert.DeserializeObject<DeepSkyOpenClusterTargetDetails>(details);
                }
                else if (targetType == "DeepSky.GalaxyCluster")
                {
                    return JsonConvert.DeserializeObject<DeepSkyClusterOfGalaxiesTargetDetails>(details);
                }
                else if (targetType == "Asterism")
                {
                    return JsonConvert.DeserializeObject<DeepSkyAsterismTargetDetails>(details);
                }
                //else if (targetType.StartsWith("DeepSky"))
                //{
                //    return JsonConvert.DeserializeObject<DeepSkyTargetDetails>(details);
                //}
            }
            return null;
        }
    }
}
