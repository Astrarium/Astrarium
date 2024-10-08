﻿using Astrarium.Plugins.Journal.Database;
using Newtonsoft.Json;
using Astrarium.Plugins.Journal.Database.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Astrarium.Plugins.Journal.Types;
using System.Xml;
using System.Threading;
using System.Data.Common;
using System.Data.SQLite;
using Astrarium.Types;
using System.IO;
using System.IO.Compression;
using Astrarium.Algorithms;
using Astrarium.Types.Utils;

namespace Astrarium.Plugins.Journal.OAL
{
    [Singleton(typeof(IOALImporter))]
    public class OALImporter : IOALImporter
    {
        public event Action OnImportBegin;
        public event Action<bool> OnImportEnd;

        private enum ImageFileConflictBehaviour
        {
            /// <summary>
            /// Skip (existing file will remain)
            /// </summary>
            Skip = 0,
            /// <summary>
            /// Create a copy of importing file with another name
            /// </summary>
            CreateCopy = 1,
            /// <summary>
            /// Overwrites existing file with new one
            /// </summary>
            Overwrite = 2
        }

        private string[] ImportImages(string[] images, string rootSourcePath, string targetImagesDir, ImageFileConflictBehaviour imageFileConflictBehaviour)
        {
            for (int i = 0; i < images.Length; i++)
            {
                string relativeImagePath = images[i];
                string fullImagePath = Path.GetFullPath(Path.Combine(rootSourcePath, relativeImagePath));
                if (File.Exists(fullImagePath))
                {
                    string fileName = Path.GetFileName(fullImagePath);

                    string destinationFullPath = Path.Combine(targetImagesDir, fileName);

                    // file already exists, create another name
                    if (File.Exists(destinationFullPath) && imageFileConflictBehaviour == ImageFileConflictBehaviour.CreateCopy)
                    {
                        destinationFullPath = Utils.GenerateNewFileName(destinationFullPath);
                    }

                    if (!(File.Exists(destinationFullPath) && imageFileConflictBehaviour == ImageFileConflictBehaviour.Skip))
                    {
                        Utils.SafeFileCopy(fullImagePath, destinationFullPath);
                    }

                    images[i] = Path.Combine("images", fileName);
                }
                else
                {
                    images[i] = null;
                }
            }

            return images.Where(x => x != null).ToArray();
        }


        public void ImportFromOAL(string file, CancellationToken? token = null, IProgress<double> progress = null)
        {
            // this is required if importing ZIP archive
            string tempDirectory = null;

            try
            {
                if (Path.GetExtension(file).ToLower() == ".zip")
                {
                    string oalFile = ZipFile.OpenRead(file)
                        // order by nesting to pick a highest-level XML
                        .Entries.OrderBy(x => x.FullName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Length)
                        // take first XML one, expected this is an OAL file
                        .FirstOrDefault(x => x.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)).FullName;

                    if (string.IsNullOrEmpty(oalFile))
                    {
                        throw new Exception("The ZIP archive does not contain XML with OAL data.");
                    }

                    // create temp directory for extracting ZIP archive
                    tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                    // extract ZIP
                    ZipFile.ExtractToDirectory(file, tempDirectory);

                    // now this points to extracted XML file
                    file = Path.Combine(tempDirectory, oalFile);
                }

                using (XmlReader reader = new OALXmlReader(file))
                {
                    long current = 0;
                    long count = 0;

                    // parse OAL data
                    var data = (OALData)new XmlSerializer(typeof(OALData)).Deserialize(reader);

                    // root folder containing OAL file
                    string rootSourcePath = Path.GetDirectoryName(file);

                    // target folder for images
                    string targetImagesDir = JournalPlugin.ImagesDirectoryPath;
                    
                    // make sure images directory exists
                    Directory.CreateDirectory(targetImagesDir);

                    foreach (var observation in data.Observations)
                    {
                        if (observation.Images == null) continue;
                        observation.Images = ImportImages(observation.Images, rootSourcePath, targetImagesDir, ImageFileConflictBehaviour.CreateCopy);
                    }

                    foreach (var session in data.Sessions)
                    {
                        if (session.Images == null) continue;
                        session.Images = ImportImages(session.Images, rootSourcePath, targetImagesDir, ImageFileConflictBehaviour.CreateCopy);
                    }

                    var connectionFactory = new DatabaseConnectionFactory();

                    using (DbConnection conn = connectionFactory.CreateConnection())
                    {
                        conn.Open();
                        var trans = conn.BeginTransaction();

                        OnImportBegin?.Invoke();

                        try
                        {
                            using (var db = new DatabaseContext(conn, contextOwnsConnection: false))
                            {
                                db.Configuration.AutoDetectChangesEnabled = false;

                                foreach (var site in data.Sites)
                                {
                                    SiteDB siteDB = site.ToSite();
                                    db.Sites.Add(siteDB);
                                    token.GetValueOrDefault().ThrowIfCancellationRequested();
                                }

                                foreach (var observer in data.Observers)
                                {
                                    ObserverDB observerDB = observer.ToObserver();
                                    db.Observers.Add(observerDB);
                                    token.GetValueOrDefault().ThrowIfCancellationRequested();
                                }

                                foreach (var optics in data.Optics)
                                {
                                    OpticsDB opticsDB = optics.ToOptics();
                                    db.Optics.Add(opticsDB);
                                    token.GetValueOrDefault().ThrowIfCancellationRequested();
                                }

                                foreach (var eyepiece in data.Eyepieces)
                                {
                                    EyepieceDB eyepieceDB = eyepiece.ToEyepiece();
                                    db.Eyepieces.Add(eyepieceDB);
                                    token.GetValueOrDefault().ThrowIfCancellationRequested();
                                }

                                foreach (var lens in data.Lenses)
                                {
                                    LensDB lensDB = lens.ToLens();
                                    db.Lenses.Add(lensDB);
                                    token.GetValueOrDefault().ThrowIfCancellationRequested();
                                }

                                foreach (var filter in data.Filters)
                                {
                                    FilterDB filterDB = filter.ToFilter();
                                    db.Filters.Add(filterDB);
                                    token.GetValueOrDefault().ThrowIfCancellationRequested();
                                }

                                foreach (var imager in data.Cameras)
                                {
                                    CameraDB cameraDB = imager.ToCamera();
                                    db.Cameras.Add(cameraDB);
                                    token.GetValueOrDefault().ThrowIfCancellationRequested();
                                }

                                db.SaveChanges();
                            }

                            foreach (var session in data.Sessions)
                            {
                                using (var db = new DatabaseContext(conn, contextOwnsConnection: false))
                                {
                                    db.Configuration.AutoDetectChangesEnabled = false;

                                    SessionDB sessionDB = session.ToSession(data);
                                    db.Sessions.Add(sessionDB);

                                    // do not add co-observers repeatedly
                                    foreach (var coObserver in sessionDB.CoObservers)
                                    {
                                        db.Entry(coObserver).State = EntityState.Unchanged;
                                    }

                                    token.GetValueOrDefault().ThrowIfCancellationRequested();

                                    db.SaveChanges();
                                }
                            }

                            current = 0;
                            count = data.Observations.Count();

                            using (var db = new DatabaseContext(conn, contextOwnsConnection: false))
                            {
                                db.Configuration.AutoDetectChangesEnabled = false;

                                foreach (var obs in data.Observations)
                                {
                                    ObservationDB obsDB = obs.ToObservation(data);

                                    if (obsDB == null) continue;

                                    // get observation without sessions that have place at the same time
                                    var sameSessionObs = data.Observations.Where(x =>
                                        string.IsNullOrEmpty(x.SessionId) &&
                                        x.ObserverId == obs.ObserverId &&
                                        x.SiteId == obs.SiteId &&
                                        x.Begin.Equals(obs.Begin)).ToList(); // TODO: what if "Begin" differs not more, for example, than 3 hours?

                                    ++current;
                                    progress?.Report(current / (double)count * 100);

                                    // attach session to observations
                                    // because Astrarium OAL implementation of observation requires session
                                    if (sameSessionObs.Any())
                                    {
                                        SessionDB sessionDB = obs.ToSession();
                                        obsDB.SessionId = sessionDB.Id;
                                        sameSessionObs.ForEach(x => x.SessionId = sessionDB.Id);
                                        db.Sessions.Add(sessionDB);

                                        // do not add co-observers repeatedly
                                        foreach (var coObserver in sessionDB.CoObservers)
                                        {
                                            db.Entry(coObserver).State = EntityState.Unchanged;
                                        }
                                    }

                                    // create target copy for each observation
                                    db.Targets.Add(obsDB.Target);
                                    db.Observations.Add(obsDB);

                                    // save changes each 1000 records
                                    if (current % 1000 == 0)
                                    {
                                        db.SaveChanges();
                                    }

                                    token.GetValueOrDefault().ThrowIfCancellationRequested();
                                }
                                db.SaveChanges();
                            }

                            trans.Commit();

                            OnImportEnd?.Invoke(true);
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                trans.Rollback();
                            }
                            catch { }

                            OnImportEnd?.Invoke(false);

                            if (ex is OperationCanceledException)
                                return;

                            Exception ie = ex;
                            while (ie != null)
                            {
                                if (ie is SQLiteException sqlEx)
                                {
                                    break;
                                }
                                ie = ie.InnerException;
                            }

                            if (ie != null)
                                throw ie;
                            else
                                throw ex;
                        }
                        finally
                        {
                            trans?.Dispose();
                        }
                    }
                }
            }
            finally
            {
                // delete temp directory, if required
                if (!string.IsNullOrEmpty(tempDirectory))
                {
                    FileSystem.DeleteDirectory(tempDirectory);
                }
            }
        }
    }

    public static class ImportExtensions
    {
        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        private static string ToStringEnum<TEnum>(this TEnum value) where TEnum : struct, IConvertible
        {
            Type enumType = typeof(TEnum);
            if (!enumType.IsEnum)
                return null;

            MemberInfo member = enumType.GetMember(value.ToString()).FirstOrDefault();
            if (member == null)
                return null;

            XmlEnumAttribute attribute = member.GetCustomAttributes(false).OfType<XmlEnumAttribute>().FirstOrDefault();
            if (attribute == null)
                return member.Name;

            return attribute.Name;
        }

        public static LensDB ToLens(this OALLens lens)
        {
            return new LensDB()
            {
                Id = lens.Id,
                Model = lens.Model,
                Vendor = lens.Vendor,
                Factor = lens.Factor
            };
        }

        private static string ToListOfValues(this object[] values)
        {
            if (values == null)
                return "[]";
            else
                return JsonConvert.SerializeObject(values, jsonSettings);
        }

        private static ICollection<AttachmentDB> ToAttachments(this string[] files)
        {
            if (files == null)
                return new List<AttachmentDB>();
            else
                return files.Select(i => new AttachmentDB() { Id = Guid.NewGuid().ToString(), FilePath = i }).ToList();
        }

        private static string ToAccounts(this OALObserverAccount[] accounts)
        {
            Dictionary<string, string> accountsObject = new Dictionary<string, string>();
            if (accounts != null)
            {
                foreach (var a in accounts)
                {
                    accountsObject[a.Name] = a.Value;
                }
            }
            return JsonConvert.SerializeObject(accountsObject, jsonSettings);
        }

        public static FilterDB ToFilter(this OALFilter filter)
        {
            return new FilterDB()
            {
                Id = filter.Id,
                Model = filter.Model,
                Vendor = filter.Vendor,
                Type = filter.Type.ToStringEnum(),
                Color = filter.ColorSpecified ? filter.Color.ToStringEnum() : null,
                Wratten = filter.Wratten
            };
        }

        public static CameraDB ToCamera(this OALImager imager)
        {
            var cameraDb = new CameraDB()
            {
                Id = imager.Id,
                Model = imager.Model,
                Vendor = imager.Vendor,
                Remarks = imager.Remarks
            };

            if (imager is OALCamera ccd)
            {
                cameraDb.PixelsX = int.Parse(ccd.PixelsX);
                cameraDb.PixelsY = int.Parse(ccd.PixelsY);
                cameraDb.PixelXSize = ccd.PixelXSizeSpecified ? (double?)ccd.PixelXSize : (double?)null;
                cameraDb.PixelYSize = ccd.PixelYSizeSpecified ? (double?)ccd.PixelYSize : (double?)null;
                cameraDb.Binning = int.Parse(ccd.Binning);
            }
            else
            {
                throw new NotImplementedException($"Unknown imager type: {imager.GetType()}");
            }

            return cameraDb;
        }

        public static EyepieceDB ToEyepiece(this OALEyepiece eyepiece)
        {
            return new EyepieceDB()
            {
                Id = eyepiece.Id,
                Model = eyepiece.Model,
                Vendor = eyepiece.Vendor,
                FocalLength = eyepiece.FocalLength,
                FocalLengthMax = eyepiece.MaxFocalLengthSpecified ? eyepiece.MaxFocalLength : (double?)null,
                ApparentFOV = eyepiece.ApparentFOV.ToAngle()
            };
        }

        public static OpticsDB ToOptics(this OALOptics optics)
        {
            var scopeDb = new OpticsDB()
            {
                Id = optics.Id,
                Aperture = optics.Aperture,
                Scheme = optics.Type,
                Vendor = optics.Vendor,
                Model = optics.Model,
                LightGrasp = optics.LightGraspSpecified ? optics.LightGrasp : (double?)null,
                Type = optics is OALScope ? "Telescope" : "Fixed",
                OrientationErect = optics.Orientation?.Erect,
                OrientationTrueSided = optics.Orientation?.TrueSided,
            };

            if (optics is OALScope scope)
            {
                scopeDb.Type = "Telescope";
                scopeDb.Details = JsonConvert.SerializeObject(new ScopeDetails()
                {
                    FocalLength = scope.FocalLength
                });
            }
            else if (optics is OALFixedMagnificationOptics fixedOptics)
            {
                scopeDb.Type = "Fixed";
                scopeDb.Details = JsonConvert.SerializeObject(new FixedOpticsDetails()
                {
                    Magnification = fixedOptics.Magnification,
                    TrueField = fixedOptics.TrueField.ToAngle()
                });
            }
            else
            {
                throw new Exception($"Unknown optics type: {optics.GetType()}");
            }

            return scopeDb;
        }

        public static ObserverDB ToObserver(this OALObserver observer)
        {
            return new ObserverDB()
            {
                Id = observer.Id,
                FirstName = observer.Name,
                LastName = observer.Surname,
                Accounts = observer.Account.ToAccounts(),
                Contacts = observer.Contact.ToListOfValues(),
                FSTOffset = observer.FSTOffsetSpecified ? observer.FSTOffset : (double?)null
            };
        }

        /// <summary>
        /// Creates new session object from observation.
        /// Used for observations without sessions.
        /// </summary>
        /// <param name="observation"></param>
        /// <returns></returns>
        public static SessionDB ToSession(this OALObservation observation)
        {
            return new SessionDB()
            {
                Id = Guid.NewGuid().ToString(),
                SiteId = observation.SiteId,
                Begin = observation.Begin,
                Seeing = observation.Seeing.ToIntNullable(),
                SkyQuality = observation.SkyQuality?.ToBrightness(),
                FaintestStar = observation.FaintestStarSpecified ? observation.FaintestStar : (double?)null,
                // if observation end time not specified, set session end equal to begin time
                End = observation.EndSpecified ? observation.End : observation.Begin,
                CoObservers = new List<ObserverDB>(),
                Attachments = new List<AttachmentDB>(),
                ObserverId = observation.ObserverId,
            };
        }

        public static ObservationDB ToObservation(this OALObservation observation, OALData data)
        {
            // get target
            OALTarget target = data.Targets.FirstOrDefault(t => t.Id == observation.TargetId);

            if (target == null)
                return null;

            Type oalTargetType = target.GetType();

            // json-serialized finding details
            string jsonDetails = null;

            if (observation.Result.Length > 1)
            {
                // TODO: what to to in this case?
            }

            OALFindings finding = observation.Result.FirstOrDefault();

            if (finding != null)
            {
                Type findingsType = finding.GetType();

                string[] bodyTypes = oalTargetType.GetCustomAttributes<CelestialObjectTypeAttribute>(inherit: false).Select(a => a.CelestialObjectType).ToArray();

                string bodyType = bodyTypes[0];

                if (bodyTypes.Length > 1)
                {
                    Type discriminatorType = oalTargetType.GetCustomAttribute<CelestialObjectTypeDiscriminatorAttribute>(inherit: false)?.Discriminator;
                    if (discriminatorType != null)
                    {
                        var discriminator = (ICelestialObjectTypeDiscriminator)Activator.CreateInstance(discriminatorType);
                        bodyType = discriminator.Discriminate(target);
                    }
                }

                // ObservationDetails class related to that celestial object type
                Type observationDetailsType = Assembly.GetAssembly(typeof(OALImporter))
                    .GetTypes().FirstOrDefault(x => typeof(ObservationDetails).IsAssignableFrom(x) && x.GetCustomAttributes<CelestialObjectTypeAttribute>(inherit: false).Any(a => a.CelestialObjectType == bodyType)) ?? typeof(ObservationDetails);

                // Create empty TargetDetails
                ObservationDetails details = (ObservationDetails)Activator.CreateInstance(observationDetailsType);

                // Get names of properties
                var properties = findingsType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public).Where(x => x.GetCustomAttribute<OALConverterAttribute>(true) != null);

                // Fill target details
                foreach (var prop in properties)
                {
                    var attr = prop.GetCustomAttribute<OALConverterAttribute>();
                    var converter = (IOALConverter)Activator.CreateInstance(attr.ImportConverter);

                    object value = prop.GetValue(finding);

                    var specifiedProperty = findingsType.GetProperty($"{prop.Name}Specified");
                    if (specifiedProperty == null || (bool)specifiedProperty.GetValue(finding))
                    {
                        object convertedValue = converter.Convert(value);

                        if (attr.Property != null)
                        {
                            observationDetailsType.GetProperty(attr.Property).SetValue(details, convertedValue);
                        }
                        else
                        {
                            var dict = convertedValue as Dictionary<string, object>;
                            foreach (var kv in dict)
                            {
                                observationDetailsType.GetProperty(kv.Key).SetValue(details, kv.Value);
                            }
                        }
                    }
                }

                jsonDetails = JsonConvert.SerializeObject(details, jsonSettings);
            }

            // basic data
            var obs = new ObservationDB()
            {
                Id = observation.Id,
                SessionId = observation.SessionId,
                Begin = observation.Begin,
                End = observation.EndSpecified ? observation.End : observation.Begin,
                Magnification = observation.MagnificationSpecified ? observation.Magnification : (double?)null,
                Accessories = observation.Accessories,
                Target = target.ToTarget(XmlConvert.ToDateTimeOffset(observation.Begin), data),
                Result = finding?.Description,
                Lang = finding?.Lang,
                Details = jsonDetails,
                ScopeId = !string.IsNullOrEmpty(observation.ScopeId) ? observation.ScopeId : null,
                EyepieceId = !string.IsNullOrEmpty(observation.EyepieceId) ? observation.EyepieceId : null,
                LensId = !string.IsNullOrEmpty(observation.LensId) ? observation.LensId : null,
                FilterId = !string.IsNullOrEmpty(observation.FilterId) ? observation.FilterId : null,
                CameraId = !string.IsNullOrEmpty(observation.CameraId) ? observation.CameraId : null,
                Attachments = observation.Images.ToAttachments()
            };

            obs.Target.Id = Guid.NewGuid().ToString();
            obs.TargetId = obs.Target.Id;

            return obs;
        }

        public static SessionDB ToSession(this OALSession session, OALData data)
        {
            var observations = data.Observations.Where(i => i.SessionId == session.Id);
            string observerId = observations.Select(o => o.ObserverId).FirstOrDefault();
            string[] coObserverIds = session.CoObservers ?? new string[0];
            string siteId = observations.Select(o => o.SiteId).FirstOrDefault();

            // take worst sky conditions among observations related to session
            int? seeing = observations.Select(o => o.Seeing.ToIntNullable()).Min();
            double? faintestStar = observations.Select(o => o.FaintestStarSpecified ? o.FaintestStar : (double?)null).Min();
            double? skyQuality = observations.Select(o => o.SkyQuality?.ToBrightness()).Min();

            DateTimeOffset begin = observations.Min(obs => XmlConvert.ToDateTimeOffset(obs.Begin));
            if (XmlConvert.ToDateTimeOffset(session.Begin) < begin)
            {
                begin = XmlConvert.ToDateTimeOffset(session.Begin);
            }

            DateTimeOffset end = observations.Max(obs => obs.EndSpecified ? XmlConvert.ToDateTimeOffset(obs.End) : XmlConvert.ToDateTimeOffset(obs.Begin));
            if (XmlConvert.ToDateTimeOffset(session.End) > end)
            {
                end = XmlConvert.ToDateTimeOffset(session.End);
            }

            return new SessionDB()
            {
                Id = session.Id,
                SiteId = session.SiteId,
                ObserverId = observerId,
                Begin = begin.ToString("yyyy-MM-ddTHH:mm:sszzzzzzz"),
                End = end.ToString("yyyy-MM-ddTHH:mm:sszzzzzzz"),
                Equipment = session.Equipment,
                CoObservers = data.Observers.Where(o => coObserverIds.Contains(o.Id)).Select(x => x.ToObserver()).ToList(),
                Attachments = session.Images.ToAttachments(),
                Comments = session.Comments,
                Weather = session.Weather,
                FaintestStar = faintestStar,
                Seeing = seeing,
                SkyQuality = skyQuality
            };
        }

        public static SiteDB ToSite(this OALSite site)
        {
            return new SiteDB()
            {
                Id = site.Id,
                Name = site.Name,
                Latitude = site.Latitude.ToAngle() ?? 0,
                Longitude = site.Longitude.ToAngle() ?? 0,
                Elevation = site.ElevationSpecified ? (double?)site.Elevation : null,
                Timezone = site.TimeZone.ToDouble() / 60.0,
                IAUCode = site.Code
            };
        }

        private static double ToDouble(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;
            else
            {
                if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out double result))
                    return result;
                else
                    return 0;
            }
        }

        private static int? ToIntNullable(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            else
            {
                if (int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out int result))
                    return result;
                else
                    return null;
            }
        }

        private static double? ToAngle(this OALAngle angle, OALAngleUnit unit = OALAngleUnit.Deg)
        {
            if (angle == null)
                return null;

            double value = angle.Value;
            switch (angle.Unit)
            {
                case OALAngleUnit.ArcMin:
                    value = value / 60.0;
                    break;
                case OALAngleUnit.ArcSec:
                    value = value / 3600.0;
                    break;
                case OALAngleUnit.Deg:
                    break;
                case OALAngleUnit.Rad:
                    value = value * 180.0 / Math.PI;
                    break;
            }

            if (unit == OALAngleUnit.ArcSec)
                value *= 3600;

            return value;
        }

        private static CrdsEquatorial ToEquatorial(this OALEquPosType pos, DateTimeOffset dateTime)
        {
            if (pos == null)
                return null;

            double ra = (double)pos.RA.ToAngle();
            double dec = (double)pos.Dec.ToAngle();

            var eq = new CrdsEquatorial(ra, dec);

            if (pos.Frame?.Equinox == OALReferenceFrameEquinox.EqOfDate)
            {
                return eq;
            }

            double jd = Date.JulianEphemerisDay(new Date(dateTime.UtcDateTime));
            
            if (pos.Frame == null || pos.Frame.Equinox == OALReferenceFrameEquinox.J2000)
            {
                PrecessionalElements pe = Precession.ElementsFK5(Date.EPOCH_J2000, jd);
                return Precession.GetEquatorialCoordinates(eq, pe);
            }
            else if (pos.Frame?.Equinox == OALReferenceFrameEquinox.B1950)
            {
                PrecessionalElements pe = Precession.ElementsFK5(Date.EPOCH_B1950, jd);
                return Precession.GetEquatorialCoordinates(eq, pe);
            }

            return null;
        }

        private static double? ToBrightness(this OALSurfaceBrightness surfaceBrightness)
        {
            if (surfaceBrightness.Unit == OALSurfaceBrightnessUnit.MagsPerSquareArcSec)
                return surfaceBrightness.Value;
            else
                return surfaceBrightness.Value / 3600;
        }

        private static TargetDB ToTarget(this OALTarget target, DateTimeOffset dateTime, OALData data)
        {
            // Type of OALTarget
            Type oalTargetType = target.GetType();

            // Get celestial object type name(s)
            string[] bodyTypes = oalTargetType.GetCustomAttributes<CelestialObjectTypeAttribute>(inherit: false).Select(x => x.CelestialObjectType).ToArray();

            string bodyType = bodyTypes[0];

            if (bodyTypes.Length > 1)
            {
                Type discriminatorType = oalTargetType.GetCustomAttribute<CelestialObjectTypeDiscriminatorAttribute>(inherit: false)?.Discriminator;
                if (discriminatorType != null)
                {
                    var discriminator = (ICelestialObjectTypeDiscriminator)Activator.CreateInstance(discriminatorType);
                    bodyType = discriminator.Discriminate(target);
                }
            }

            // TargetDetails class related to that celestial object type
            Type targetDetailsType = Assembly.GetAssembly(typeof(OALImporter))
                .GetTypes().FirstOrDefault(x => typeof(TargetDetails).IsAssignableFrom(x) && x.GetCustomAttributes<CelestialObjectTypeAttribute>(inherit: false).Any(a => a.CelestialObjectType == bodyType)) ?? typeof(TargetDetails);

            // Create empty TargetDetails
            TargetDetails details = (TargetDetails)Activator.CreateInstance(targetDetailsType);

            // Get names of properties
            var properties = oalTargetType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public).Where(x => x.GetCustomAttribute<OALConverterAttribute>(true) != null);

            // Fill target details
            foreach (var prop in properties)
            {
                var attr = prop.GetCustomAttribute<OALConverterAttribute>();
                var converter = (IOALConverter)Activator.CreateInstance(attr.ImportConverter);

                object value = prop.GetValue(target);

                var specifiedProperty = oalTargetType.GetProperty($"{prop.Name}Specified");
                if (specifiedProperty == null || (bool)specifiedProperty.GetValue(target))
                {
                    object convertedValue = converter.Convert(value);
                    targetDetailsType.GetProperty(attr.Property).SetValue(details, convertedValue);
                }
            }

            TargetDB result = new TargetDB();

            var eq = target.Position?.ToEquatorial(dateTime);

            details.RA = eq?.Alpha;
            details.Dec = eq?.Delta;
            details.Constellation = target.Constellation;

            result.Id = target.Id;
            result.Type = bodyType;
            result.Name = target.Name;
            result.CommonName = target.Name;
            result.Aliases = target.Alias.ToListOfValues();
            result.Source = target.Item;
            result.Details = JsonConvert.SerializeObject(details);
            result.Notes = target.Notes;
            if (target.ItemElementName == OALDataSource.Observer)
            {
                var observer = data.Observers.FirstOrDefault(x => x.Id == target.Item);
                if (observer != null)
                {
                    result.Source = $"Observer: {observer.Name} {observer.Surname}".Trim();
                }
            }

            return result;
        }
    }
}
