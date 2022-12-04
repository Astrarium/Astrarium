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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections;

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
                session.DatabasePropertyChanged -= SaveDatabaseEntityProperty;

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
                }

                session.DatabasePropertyChanged += SaveDatabaseEntityProperty;
            });
        }

        public static Task LoadObservation(Observation observation)
        {
            return Task.Run(() =>
            {
                observation.DatabasePropertyChanged -= SaveDatabaseEntityProperty;

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

                    observation.Constellation = obs.Target?.Constellation;
                    observation.EquatorialCoordinates = obs.Target?.RightAscension != null && obs.Target?.Declination != null ? new CrdsEquatorial((double)obs.Target?.RightAscension.Value, (double)obs.Target?.Declination.Value) : null;

                    observation.Attachments = obs.Attachments.ToArray().Select(x => new Attachment()
                    {
                        Id = x.Id,
                        FilePath = Path.GetFullPath(Path.Combine(rootPath, x.FilePath)),
                        Title = x.Title,
                        Comments = x.Comments
                    }).ToList();
                }

                observation.DatabasePropertyChanged += SaveDatabaseEntityProperty;
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
                    CommonName = body.CommonName,
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
                    ObjectNameAliases = DeserializeAliases(observation.Target.Aliases),
                    // TODO: save coordinates of the body
                };

                return obs;
            });
        }

        public static Task EditObservation(Observation observation, CelestialObject body, DateTime begin, DateTime end)
        {
            return Task.Run(() =>
            {
                using (var ctx = new DatabaseContext())
                {
                    var observationDb = ctx.Observations.FirstOrDefault(x => x.Id == observation.Id);
                    if (observationDb != null)
                    {
                        observationDb.Begin = begin;
                        observationDb.End = end;
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

        public static Task<ICollection> GetSites()
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

        public static Task<ICollection> GetOptics()
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

        public static Task<Optics> GetOptics(string id)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var opticsDb = db.Optics.FirstOrDefault(x => x.Id == id);
                    if (opticsDb != null)
                    {
                        var optics = new Optics();

                        optics.Id = opticsDb.Id;
                        optics.Vendor = opticsDb.Vendor;
                        optics.Model = opticsDb.Model;
                        optics.Scheme = opticsDb.Scheme;
                        optics.Type = opticsDb.Type;
                        optics.Aperture = opticsDb.Aperture;
                        optics.OrientationErect = opticsDb.OrientationErect;
                        optics.OrientationTrueSided = opticsDb.OrientationTrueSided;

                        if (optics.Type == "Telescope")
                        {
                            var details = JsonConvert.DeserializeObject<ScopeDetails>(opticsDb.Details);
                            optics.FocalLength = details.FocalLength;
                        }
                        else if (optics.Type == "Fixed")
                        {
                            var details = JsonConvert.DeserializeObject<FixedOpticsDetails>(opticsDb.Details);
                            optics.Magnification = details.Magnification;
                            optics.TrueField = details.TrueField ?? 0;
                            optics.TrueFieldSpecified = details.TrueField != null;
                        }

                        return optics;
                    }

                    return null;
                }
            });
        }

        public static Task SaveOptics(Optics optics)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var opticsDb = db.Optics.FirstOrDefault(x => x.Id == optics.Id);
                    if (opticsDb == null)
                    {
                        opticsDb = new OpticsDB() { Id = optics.Id };
                        db.Optics.Add(opticsDb);
                    }

                    opticsDb.Vendor = optics.Vendor;
                    opticsDb.Model = optics.Model;
                    opticsDb.Scheme = optics.Scheme;
                    opticsDb.Type = optics.Type;
                    opticsDb.Aperture = optics.Aperture;
                    opticsDb.OrientationErect = optics.OrientationErect;
                    opticsDb.OrientationTrueSided = optics.OrientationTrueSided;

                    if (optics.Type == "Telescope")
                    {
                        opticsDb.Details = JsonConvert.SerializeObject(new ScopeDetails() { FocalLength = optics.FocalLength });
                    }
                    else if (optics.Type == "Fixed")
                    {
                        opticsDb.Details = JsonConvert.SerializeObject(new FixedOpticsDetails() { Magnification = optics.Magnification, TrueField = optics.TrueFieldSpecified ? optics.TrueField : (double?)null });
                    }

                    db.SaveChanges();
                }
            });
        }

        public static Task DeleteOptics(string id)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var existing = db.Optics.FirstOrDefault(x => x.Id == id);
                    if (existing != null)
                    {
                        db.Optics.Remove(existing);
                        db.Database.ExecuteSqlCommand($"UPDATE [Observations] SET [ScopeId] = NULL WHERE [ScopeId] = '{id}'");
                        db.SaveChanges();
                    }
                }
            });
        }

        public static Task<ICollection> GetEyepieces()
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

        public static Task<CelestialObject> GetTarget(string id)
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

        public static Task<ICollection> GetLenses()
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

        public static Task<Lens> GetLens(string id)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var lensDb = db.Lenses.FirstOrDefault(x => x.Id == id);
                    if (lensDb != null)
                    {
                        var lens = new Lens();
                        lens.Id = lensDb.Id;
                        lens.Vendor = lensDb.Vendor;
                        lens.Model = lensDb.Model;
                        lens.Factor = lensDb.Factor;
                        return lens;
                    }

                    return null;
                }
            });
        }

        public static Task SaveLens(Lens lens)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var lensDb = db.Lenses.FirstOrDefault(x => x.Id == lens.Id);
                    if (lensDb == null)
                    {
                        lensDb = new LensDB() { Id = lens.Id };
                        db.Lenses.Add(lensDb);
                    }

                    lensDb.Vendor = lens.Vendor;
                    lensDb.Model = lens.Model;
                    lensDb.Factor = lens.Factor;
                    
                    db.SaveChanges();
                }
            });
        }

        public static Task DeleteLens(string id)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var existing = db.Lenses.FirstOrDefault(x => x.Id == id);
                    if (existing != null)
                    {
                        db.Lenses.Remove(existing);
                        db.Database.ExecuteSqlCommand($"UPDATE [Observations] SET [LensId] = NULL WHERE [LensId] = '{id}'");
                        db.SaveChanges();
                    }
                }
            });
        }

        public static Task SaveFilter(Filter filter)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var filterDb = db.Filters.FirstOrDefault(x => x.Id == filter.Id);
                    if (filterDb == null)
                    {
                        filterDb = new FilterDB() { Id = filter.Id };
                        db.Filters.Add(filterDb);
                    }

                    filterDb.Vendor = filter.Vendor;
                    filterDb.Model = filter.Model;
                    filterDb.Type = filter.Type;
                    if (filter.Type == "color")
                    {
                        filterDb.Color = filter.Color;
                        filterDb.Wratten = filter.Wratten;
                    }
                    else
                    {
                        filterDb.Color = null;
                        filterDb.Wratten = null;
                    }

                    db.SaveChanges();
                }
            });
        }

        public static Task<ICollection> GetFilters()
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

        public static Task<ICollection> GetCameras()
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

        public static Task<Filter> GetFilter(string id)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var filterDb = db.Filters.FirstOrDefault(x => x.Id == id);
                    if (filterDb != null)
                    {
                        var filter = new Filter();

                        filter.Id = filterDb.Id;
                        filter.Vendor = filterDb.Vendor;
                        filter.Model = filterDb.Model;
                        filter.Type = filterDb.Type;
                        if (filter.Type == "color")
                        {
                            filter.Color = filterDb.Color;
                            filter.Wratten = filterDb.Wratten;
                        }

                        return filter;
                    }

                    return null;
                }
            });
        }

        public static Task DeleteFilter(string id)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var existing = db.Filters.FirstOrDefault(x => x.Id == id);
                    if (existing != null)
                    {
                        db.Filters.Remove(existing);
                        db.Database.ExecuteSqlCommand($"UPDATE [Observations] SET [FilterId] = NULL WHERE [FilterId] = '{id}'");
                        db.SaveChanges();
                    }
                }
            });
        }

        public static Task<Eyepiece> GetEyepiece(string id)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var eyepieceDb = db.Eyepieces.FirstOrDefault(x => x.Id == id);
                    if (eyepieceDb != null)
                    {
                        var eyepiece = new Eyepiece();

                        eyepiece.Id = eyepieceDb.Id;
                        eyepiece.Vendor = eyepieceDb.Vendor;
                        eyepiece.Model = eyepieceDb.Model;
                        eyepiece.FocalLength = eyepieceDb.FocalLength;
                        eyepiece.MaxFocalLength = eyepieceDb.FocalLengthMax.HasValue ? eyepieceDb.FocalLengthMax.Value : 10;
                        eyepiece.IsZoomEyepiece = eyepieceDb.FocalLengthMax.HasValue;
                        eyepiece.ApparentFOV = eyepieceDb.ApparentFOV.HasValue ? eyepieceDb.ApparentFOV.Value : 50;
                        eyepiece.ApparentFOVSpecified = eyepieceDb.ApparentFOV.HasValue;
                        return eyepiece;
                    }

                    return null;
                }
            });
        }

        public static Task SaveEyepiece(Eyepiece eyepiece)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var eyepieceDb = db.Eyepieces.FirstOrDefault(x => x.Id == eyepiece.Id);
                    if (eyepieceDb == null)
                    {
                        eyepieceDb = new EyepieceDB() { Id = eyepiece.Id };
                        db.Eyepieces.Add(eyepieceDb);
                    }

                    eyepieceDb.Vendor = eyepiece.Vendor;
                    eyepieceDb.Model = eyepiece.Model;
                    eyepieceDb.FocalLength = eyepiece.FocalLength;
                    eyepieceDb.FocalLengthMax = eyepiece.IsZoomEyepiece ? eyepiece.MaxFocalLength : (double?)null;
                    eyepieceDb.ApparentFOV = eyepiece.ApparentFOVSpecified ? eyepiece.ApparentFOV : (double?)null;

                    db.SaveChanges();
                }
            });
        }

        public static Task DeleteEyepiece(string id)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var existing = db.Eyepieces.FirstOrDefault(x => x.Id == id);
                    if (existing != null)
                    {
                        db.Eyepieces.Remove(existing);
                        db.Database.ExecuteSqlCommand($"UPDATE [Observations] SET [EyepieceId] = NULL WHERE [EyepieceId] = '{id}'");
                        db.SaveChanges();
                    }
                }
            });
        }

        public static Task<Camera> GetCamera(string id)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var cameraDb = db.Cameras.FirstOrDefault(x => x.Id == id);
                    if (cameraDb != null)
                    {
                        var camera = new Camera();

                        camera.Id = cameraDb.Id;
                        camera.Vendor = cameraDb.Vendor;
                        camera.Model = cameraDb.Model;
                        camera.PixelsX = cameraDb.PixelsX;
                        camera.PixelsY = cameraDb.PixelsY;
                        camera.PixelXSize = cameraDb.PixelXSize;
                        camera.PixelYSize = cameraDb.PixelYSize;
                        camera.Binning = cameraDb.Binning;
                        camera.Remarks = cameraDb.Remarks;

                        return camera;
                    }

                    return null;
                }
            });
        }

        public static Task SaveCamera(Camera camera)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var cameraDb = db.Cameras.FirstOrDefault(x => x.Id == camera.Id);
                    if (cameraDb == null)
                    {
                        cameraDb = new CameraDB() { Id = camera.Id };
                        db.Cameras.Add(cameraDb);
                    }

                    cameraDb.Vendor = camera.Vendor;
                    cameraDb.Model = camera.Model;
                    cameraDb.PixelsX = camera.PixelsX;
                    cameraDb.PixelsY = camera.PixelsY;
                    cameraDb.PixelXSize = camera.PixelXSize;
                    cameraDb.PixelYSize = camera.PixelYSize;
                    cameraDb.Binning = camera.Binning;
                    cameraDb.Remarks = camera.Remarks;

                    db.SaveChanges();
                }
            });
        }

        public static Task DeleteCamera(string id)
        {
            return Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var existing = db.Cameras.FirstOrDefault(x => x.Id == id);
                    if (existing != null)
                    {
                        db.Cameras.Remove(existing);
                        db.Database.ExecuteSqlCommand($"UPDATE [Observations] SET [CameraId] = NULL WHERE [CameraId] = '{id}'");
                        db.SaveChanges();
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
