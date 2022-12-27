using Astrarium.Plugins.Journal.Database;
using Astrarium.Plugins.Journal.Database.Entities;
using Newtonsoft.Json;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Astrarium.Plugins.Journal.Types;
using System.Reflection;
using Astrarium.Types;
using System.Threading;

namespace Astrarium.Plugins.Journal.OAL
{
    [Singleton(typeof(IOALExporter))]
    public class Export : IOALExporter
    {
        public void ExportToOAL(string file, CancellationToken? token = null)
        {
            bool isCompleted = false;

            try
            {
                using (var db = new DatabaseContext())
                {
                    if (token.HasValue)
                    {
                        // here is a hack to cancel reading from DB:
                        // run a timer that will check cancel token state
                        // and close the DB connection if cancellation is requested.
                        // That will cause and exception
                        new Thread(() =>
                        {
                            while (!isCompleted)
                            {
                                if (token.Value.IsCancellationRequested)
                                {
                                    db.Database.Connection.Close();
                                    break;
                                }
                                Thread.Sleep(10);
                            }
                        })
                        { IsBackground = true }.Start();
                    }

                    var data = new OALData();
                    data.Sites = db.Sites.ToArray().Select(x => x.ToSite()).ToArray();
                    data.Observers = db.Observers.ToArray().Select(x => x.ToObserver()).ToArray();
                    data.Optics = db.Optics.ToArray().Select(x => x.ToOptics()).ToArray();
                    data.Eyepieces = db.Eyepieces.ToArray().Select(x => x.ToEyepiece()).ToArray();
                    data.Lenses = db.Lenses.ToArray().Select(x => x.ToLens()).ToArray();
                    data.Filters = db.Filters.ToArray().Select(x => x.ToFilter()).ToArray();
                    data.Cameras = db.Cameras.ToArray().Select(x => x.ToImager()).ToArray();

                    ICollection<SessionDB> sessions = db.Sessions.Include(x => x.CoObservers).Include(x => x.Attachments).ToArray();
                    ICollection<TargetDB> targets = db.Targets.ToArray();

                    data.Sessions = sessions.Select(x => x.ToSession()).ToArray();

                    data.Observations = db.Observations.Include(x => x.Target).Include(x => x.Attachments).ToArray().Select(x => x.ToObservation(sessions)).ToArray();
                    data.Targets = targets.Select(x => x.ToTarget()).ToArray();

                    var serializer = new XmlSerializer(typeof(OALData));

                    using (var stream = new StreamWriter(file))
                    {
                        var serializerNamespaces = new XmlSerializerNamespaces();
                        serializerNamespaces.Add("oal", "http://groups.google.com/group/openastronomylog");
                        serializerNamespaces.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                        serializer.Serialize(stream, data, serializerNamespaces);
                    }
                }
            }
            catch
            {
                if (!(token.HasValue && token.Value.IsCancellationRequested))
                    throw;
            }
            finally
            {
                isCompleted = true;
            }
        }
    }

    public static class ExportExtensions
    {
        public static OALSite ToSite(this SiteDB site)
        {
            return new OALSite()
            {
                Id = site.Id,
                Name = site.Name,
                Latitude = site.Latitude.ToAngle(),
                Longitude = site.Longitude.ToAngle(),
                Elevation = site.Elevation != null ? site.Elevation.Value : 0,
                ElevationSpecified = site.Elevation != null,
                TimeZone = (site.Timezone * 60).ToString(CultureInfo.InvariantCulture),
                Code = site.IAUCode
            };
        }

        public static OALObserver ToObserver(this ObserverDB observer)
        {
            return new OALObserver()
            {
                Id = observer.Id,
                Name = observer.FirstName,
                Surname = observer.LastName,
                Account = JsonConvert.DeserializeObject<Dictionary<string, string>>(observer.Accounts)
                    .Select(kv => new OALObserverAccount() { Name = kv.Key, Value = kv.Value })
                    .ToArray(),
                Contact = JsonConvert.DeserializeObject<string[]>(observer.Contacts),
                FSTOffset = observer.FSTOffset != null ? observer.FSTOffset.Value : 0,
                FSTOffsetSpecified = observer.FSTOffset != null
            };
        }

        public static OALOptics ToOptics(this OpticsDB optics)
        {
            OALOptics opt = null;

            if (optics.Type == "Telescope")
            {
                var details = JsonConvert.DeserializeObject<ScopeDetails>(optics.Details);
                opt = new OALScope()
                {
                    FocalLength = details.FocalLength
                };
            }
            else if (optics.Type == "Fixed")
            {
                var details = JsonConvert.DeserializeObject<FixedOpticsDetails>(optics.Details);
                opt = new OALFixedMagnificationOptics()
                {
                    Magnification = details.Magnification,
                    TrueField = details.TrueField != null ? new OALNonNegativeAngle() { Unit = OALAngleUnit.Deg, Value = details.TrueField.Value } : null
                };
            }
            else
            {
                throw new Exception("Unknown optics type");
            }

            opt.Id = optics.Id;
            opt.Aperture = optics.Aperture;
            opt.Type = optics.Scheme;
            opt.Vendor = optics.Vendor;
            opt.Model = optics.Model;
            opt.LightGraspSpecified = optics.LightGrasp != null;
            opt.LightGrasp = optics.LightGrasp ?? 0;
            opt.Orientation = new OALOpticsOrientation()
            {
                Erect = optics.OrientationErect ?? false,
                TrueSided = optics.OrientationTrueSided ?? false
            };

            return opt;
        }

        public static OALEyepiece ToEyepiece(this EyepieceDB eyepiece)
        {
            return new OALEyepiece()
            {
                Id = eyepiece.Id,
                Model = eyepiece.Model,
                Vendor = eyepiece.Vendor,
                ApparentFOV = eyepiece.ApparentFOV != null ? new OALNonNegativeAngle() { Unit = OALAngleUnit.Deg, Value = eyepiece.ApparentFOV.Value } : null,
                FocalLength = eyepiece.FocalLength,
                MaxFocalLength = eyepiece.FocalLengthMax ?? 0,
                MaxFocalLengthSpecified = eyepiece.FocalLengthMax != null
            };
        }

        public static OALLens ToLens(this LensDB lens)
        {
            return new OALLens()
            {
                Id = lens.Id,
                Factor = lens.Factor,
                Model = lens.Model,
                Vendor = lens.Vendor
            };
        }

        public static OALFilter ToFilter(this FilterDB filter)
        {
            return new OALFilter()
            {
                Id = filter.Id,
                Vendor = filter.Vendor,
                Model = filter.Model,
                Type = GetValueFromXmlEnumAttribute<OALFilterKind>(filter.Type),
                Color = filter.Color != null ? GetValueFromXmlEnumAttribute<OALFilterColor>(filter.Color) : OALFilterColor.LightRed,
                ColorSpecified = filter.Color != null,
                Wratten = filter.Wratten
            };
        }

        public static OALImager ToImager(this CameraDB camera)
        {
            return new OALCamera()
            {
                Id = camera.Id,
                Vendor = camera.Vendor,
                Model = camera.Model,
                Binning = camera.Binning.ToString(),
                PixelsX = camera.PixelsX.ToString(),
                PixelsY = camera.PixelsY.ToString(),
                PixelXSize = (decimal)(camera.PixelXSize ?? 0),
                PixelXSizeSpecified = camera.PixelXSize != null,
                PixelYSize = (decimal)(camera.PixelYSize ?? 0),
                PixelYSizeSpecified = camera.PixelYSize != null,
                Remarks = camera.Remarks
            };
        }

        public static OALSession ToSession(this SessionDB session)
        {
            return new OALSession()
            {
                Id = session.Id,
                Begin = session.Begin,
                End = session.End,
                SiteId = session.SiteId,
                Equipment = session.Equipment,
                Comments = session.Comments,
                Weather = session.Weather,
                Images = session.Attachments?.Select(x => x.FilePath).ToArray(),
                CoObservers = session.CoObservers?.Select(x => x.Id).ToArray(),
            };
        }

        public static OALObservation ToObservation(this ObservationDB observation, ICollection<SessionDB> sessions)
        {
            var session = sessions.FirstOrDefault(x => x.Id == observation.SessionId);

            // Celestial object type
            string bodyType = observation.Target.Type;

            // OALFindings class related to that celestial object type
            Type oalFindingsType = Assembly.GetAssembly(typeof(Export))
                .GetTypes().FirstOrDefault(x => typeof(OALFindings).IsAssignableFrom(x) && x.GetCustomAttributes<CelestialObjectTypeAttribute>(inherit: false).Any(a => a.CelestialObjectType == bodyType)) ?? typeof(OALFindings);

            // ObservationDetails class related to that celestial object type
            Type observationDetailsType = Assembly.GetAssembly(typeof(Export))
                .GetTypes().FirstOrDefault(x => typeof(ObservationDetails).IsAssignableFrom(x) && x.GetCustomAttributes<CelestialObjectTypeAttribute>(inherit: false).Any(a => a.CelestialObjectType == bodyType)) ?? typeof(ObservationDetails);

            // Create empty OALFindings
            OALFindings findings = (OALFindings)Activator.CreateInstance(oalFindingsType);

            // Get names of properties
            var properties = oalFindingsType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public).Where(x => x.GetCustomAttribute<OALConverterAttribute>(true) != null);

            ObservationDetails details = (ObservationDetails)JsonConvert.DeserializeObject(observation.Details ?? "{}", observationDetailsType);

            // Fill OALFindings
            foreach (var prop in properties)
            {
                var attr = prop.GetCustomAttribute<OALConverterAttribute>();
                var converter = (IOALConverter)Activator.CreateInstance(attr.ExportConverter);

                object value = attr.Property != null ?
                    observationDetailsType.GetProperty(attr.Property).GetValue(details) : details;

                object convertedValue = converter.Convert(value);
                prop.SetValue(findings, convertedValue);

                var specifiedProperty = oalFindingsType.GetProperty($"{prop.Name}Specified");
                if (specifiedProperty != null && value != null)
                {
                    // set Specified property to "true"
                    specifiedProperty.SetValue(findings, true);
                }
            }

            findings.Description = observation.Result;
            findings.Lang = observation.Lang;

            return new OALObservation()
            {
                Id = observation.Id,
                Begin = observation.Begin,
                End = observation.End,
                EndSpecified = true,
                Accessories = observation.Accessories,
                EyepieceId = observation.EyepieceId,
                FilterId = observation.FilterId,
                CameraId = observation.CameraId,
                LensId = observation.LensId,
                ScopeId = observation.ScopeId,
                SessionId = observation.SessionId,
                TargetId = observation.TargetId,
                FaintestStar = session?.FaintestStar ?? 0,
                FaintestStarSpecified = session?.FaintestStar != null,
                Magnification = observation.Magnification ?? 0,
                MagnificationSpecified = observation.Magnification != null,
                Image = observation.Attachments?.Select(x => x.FilePath).ToArray(),
                ObserverId = session?.ObserverId,
                Seeing = session?.Seeing?.ToString(),
                SiteId = session?.SiteId,
                SkyQuality = session?.SkyQuality != null ? new OALSurfaceBrightness() { Unit = OALSurfaceBrightnessUnit.MagsPerSquareArcSec, Value = session.SkyQuality.Value } : null,
                Result = new OALFindings[] { findings }
            };
        }

        public static OALTarget ToTarget(this TargetDB target)
        {
            // Celestial object type
            string bodyType = target.Type;

            // OALTarget class related to that celestial object type
            Type oalTargetType = Assembly.GetAssembly(typeof(Export))
                .GetTypes().FirstOrDefault(x => typeof(OALTarget).IsAssignableFrom(x) && x.GetCustomAttributes<CelestialObjectTypeAttribute>().Any(a => a.CelestialObjectType == bodyType)) ?? typeof(OALTarget);

            // TargetDetails class related to that celestial object type
            Type targetDetailsType = Assembly.GetAssembly(typeof(Export))
                .GetTypes().FirstOrDefault(x => typeof(TargetDetails).IsAssignableFrom(x) && x.GetCustomAttributes<CelestialObjectTypeAttribute>().Any(a => a.CelestialObjectType == bodyType)) ?? typeof(TargetDetails);

            // Create empty OALTarget
            OALTarget tar = (OALTarget)Activator.CreateInstance(oalTargetType);

            // Get names of properties
            var properties = oalTargetType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public).Where(x => x.GetCustomAttribute<OALConverterAttribute>(true) != null);

            TargetDetails details = (TargetDetails)JsonConvert.DeserializeObject(target.Details, targetDetailsType);

            // Fill OALTarget
            foreach (var prop in properties)
            {
                var attr = prop.GetCustomAttribute<OALConverterAttribute>();
                var converter = (IOALConverter)Activator.CreateInstance(attr.ExportConverter);

                object value = attr.Property != null ?
                    targetDetailsType.GetProperty(attr.Property).GetValue(details) : details;

                object convertedValue = converter.Convert(value);
                prop.SetValue(tar, convertedValue);

                var specifiedProperty = oalTargetType.GetProperty($"{prop.Name}Specified");
                if (specifiedProperty != null && value != null)
                {
                    // set Specified property to "true"
                    specifiedProperty.SetValue(tar, true);
                }
            }

            tar.Id = target.Id;
            tar.Alias = JsonConvert.DeserializeObject<string[]>(target.Aliases);
            tar.Constellation = details?.Constellation;
            tar.Name = target.Name;
            tar.Notes = target.Notes;
            tar.Item = target.Source;
            tar.ItemElementName = OALDataSource.DataSource;
            if (details != null && details.RA != null && details.Dec != null)
            {
                tar.Position = new OALEquPosType()
                {
                    RA = ToUnsignedAngle(details.RA.Value),
                    Dec = ToAngle(details.Dec.Value),
                    Frame = new OALReferenceFrame()
                    {
                        Equinox = OALReferenceFrameEquinox.EqOfDate,
                        Origin = OALReferenceFrameOrigin.Topo
                    }
                };
            }

            return tar;
        }

        public static OALAngle ToAngle(this double angle)
        {
            return new OALAngle() { Unit = OALAngleUnit.Deg, Value = angle };
        }

        public static OALNonNegativeAngle ToUnsignedAngle(this double angle)
        {
            return new OALNonNegativeAngle() { Unit = OALAngleUnit.Deg, Value = angle };
        }

        public static T GetValueFromXmlEnumAttribute<T>(string value) where T : Enum
        {
            if (value == null) return default(T);

            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                typeof(XmlEnumAttribute)) is XmlEnumAttribute attribute)
                {
                    if (attribute.Name == value)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == value)
                        return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException("Could not parse XmlEnumAttribute. Not found.", nameof(value));
        }
    }
}
