using Astrarium.Plugins.Journal.Database;
using Newtonsoft.Json;
using Astrarium.Plugins.Journal.Database.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Astrarium.Plugins.Journal.Types;

namespace Astrarium.Plugins.Journal.OAL
{
    public static class Import
    {
        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public static void ImportFromOAL(string file)
        {
            var serializer = new XmlSerializer(typeof(OALData));

            var stringData = File.ReadAllText(file);
            using (TextReader reader = new StringReader(stringData))
            {
                var data = (OALData)serializer.Deserialize(reader);

                using (var db = new DatabaseContext())
                {
                    foreach (var site in data.Sites)
                    {
                        SiteDB siteDB = site.ToSite();
                        db.Sites.Add(siteDB);
                    }

                    foreach (var observer in data.Observers)
                    {
                        ObserverDB observerDB = observer.ToObserver();
                        db.Observers.Add(observerDB);
                    }

                    foreach (var optics in data.Optics)
                    {
                        OpticsDB opticsDB = optics.ToOptics();
                        db.Optics.Add(opticsDB);
                    }

                    foreach (var eyepiece in data.Eyepieces)
                    {
                        EyepieceDB eyepieceDB = eyepiece.ToEyepiece();
                        db.Eyepieces.Add(eyepieceDB);
                    }

                    foreach (var lens in data.Lenses)
                    {
                        LensDB lensDB = lens.ToLens();
                        db.Lenses.Add(lensDB);
                    }

                    foreach (var filter in data.Filters)
                    {
                        FilterDB filterDB = filter.ToFilter();
                        db.Filters.Add(filterDB);
                    }

                    foreach (var imager in data.Cameras)
                    {
                        CameraDB cameraDB = imager.ToCamera();
                        db.Cameras.Add(cameraDB);
                    }

                    db.SaveChanges();
                }

                foreach (var session in data.Sessions)
                {
                    using (var db = new DatabaseContext())
                    {
                        SessionDB sessionDB = session.ToSession(data);
                        db.Sessions.Add(sessionDB);

                        // do not add co-observers repeatedly
                        foreach (var coObserver in sessionDB.CoObservers)
                        {
                            db.Entry(coObserver).State = EntityState.Unchanged;
                        }

                        db.SaveChanges();
                    }
                }

                foreach (var obs in data.Observations)
                {
                    ObservationDB obsDB = obs.ToObservation(data);

                    using (var db = new DatabaseContext())
                    {
                        // observation has no session, create new one
                        // because Astrarium OAL implementation of observation requires session
                        if (string.IsNullOrEmpty(obsDB.SessionId))
                        {
                            SessionDB sessionDB = obs.ToSession();

                            // attach session to observation
                            obsDB.SessionId = sessionDB.Id;
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

                        db.SaveChanges();
                    }
                }
            }
        }

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

        private static LensDB ToLens(this OALLens lens)
        {
            return new LensDB()
            {
                Id = lens.id,
                Model = lens.model,
                Vendor = lens.vendor,
                Factor = lens.factor
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

        private static FilterDB ToFilter(this OALFilter filter)
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

        private static CameraDB ToCamera(this OALImager imager)
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

        private static EyepieceDB ToEyepiece(this OALEyepiece eyepiece)
        {
            return new EyepieceDB()
            {
                Id = eyepiece.id,
                Model = eyepiece.model,
                Vendor = eyepiece.vendor,
                FocalLength = eyepiece.focalLength,
                FocalLengthMax = eyepiece.maxFocalLengthSpecified ? eyepiece.maxFocalLength : (double?)null,
                ApparentFOV = eyepiece.apparentFOV.ToAngle()
            };
        }

        private static OpticsDB ToOptics(this OALOptics optics)
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

        private static ObserverDB ToObserver(this OALObserver observer)
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
        private static SessionDB ToSession(this OALObservation observation)
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

        private static ObservationDB ToObservation(this OALObservation observation, OALData data)
        {
            // get target
            var target = data.Targets.First(t => t.Id == observation.TargetId);

            // json-serialized finding details
            string jsonDetails = null;

            if (observation.Result.Length > 1)
            {
                // TODO: what to to in this case?
            }

            var finding = observation.Result.FirstOrDefault();
            if (finding != null)
            {
                // Variable star
                if (finding is OALFindingsVariableStar vs)
                {
                    var details = new VariableStarObservationDetails();
                    details.VisMag = vs.VisMag.Value;
                    details.VisMagUncertain = vs.VisMag.UncertainSpecified ? vs.VisMag.Uncertain : (bool?)null;
                    details.VisMagFainterThan = vs.VisMag.FainterThanSpecified ? vs.VisMag.FainterThan : (bool?)null;

                    details.ComparisonStars = vs.ComparisonStar.ToListOfValues();

                    // AAVSO chart date identifier
                    details.ChartDate = vs.ChartId?.Value;
                    details.NonAAVSOChart = vs.ChartId?.NonAAVSOChartSpecified == true ? vs.ChartId.NonAAVSOChart : (bool?)null;

                    details.BrightSky = vs.BrightSkySpecified ? vs.BrightSky : (bool?)null;
                    details.Clouds = vs.CloudsSpecified ? vs.Clouds : (bool?)null;
                    details.ComparismSequenceProblem = vs.ComparismSequenceProblemSpecified ? vs.ComparismSequenceProblem : (bool?)null;
                    details.FaintStar = vs.FaintStarSpecified ? vs.FaintStar : (bool?)null;
                    details.NearHorizion = vs.NearHorizionSpecified ? vs.NearHorizion : (bool?)null;
                    details.Outburst = vs.OutburstSpecified ? vs.Outburst : (bool?)null;
                    details.PoorSeeing = vs.PoorSeeingSpecified ? vs.PoorSeeing : (bool?)null;
                    details.StarIdentificationUncertain = vs.StarIdentificationUncertainSpecified ? vs.StarIdentificationUncertain : (bool?)null;
                    details.UnusualActivity = vs.UnusualActivitySpecified ? vs.UnusualActivity : (bool?)null;
                    jsonDetails = JsonConvert.SerializeObject(details, jsonSettings);
                }
                else if (finding is OALFindingsDeepSky dst)
                {
                    // Double star
                    if (finding is OALFindingsDeepSkyDS ds)
                    {
                        var details = BuildDeepSkyObservationDetails<DoubleStarObservationDetails>(ds);
                        details.ColorMainComponent = ds.ColorMainSpecified ? ds.ColorMain.ToStringEnum() : null;
                        details.ColorCompanionComponent = ds.ColorCompanionSpecified ? ds.ColorCompanion.ToStringEnum() : null;
                        details.EqualBrightness = ds.EqualBrightnessSpecified ? ds.EqualBrightness : (bool?)null;
                        details.NiceSurrounding = ds.NiceSurroundingSpecified ? ds.NiceSurrounding : (bool?)null;
                        jsonDetails = JsonConvert.SerializeObject(details, jsonSettings);
                    }

                    // Open cluster
                    else if (finding is OALFindingsDeepSkyOC oc)
                    {
                        var details = BuildDeepSkyObservationDetails<OpenClusterObservationDetails>(oc);
                        details.Character = oc.CharacterSpecified ? oc.Character.ToStringEnum() : null;
                        details.PartlyUnresolved = oc.PartlyUnresolvedSpecified ? oc.PartlyUnresolved : (bool?)null;
                        details.UnusualShape = oc.UnusualShapeSpecified ? oc.UnusualShape : (bool?)null;
                        details.ColorContrasts = oc.ColorContrastsSpecified ? oc.ColorContrasts : (bool?)null;
                        jsonDetails = JsonConvert.SerializeObject(details, jsonSettings);
                    }

                    // Other deep sky objects
                    else 
                    {
                        var details = BuildDeepSkyObservationDetails<DeepSkyObservationDetails>(dst);
                        jsonDetails = JsonConvert.SerializeObject(details, jsonSettings);
                    }
                }
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
                Target = target.ToTarget(data),
                Result = finding?.Description,
                Lang = finding?.Lang,
                Details = jsonDetails,
                ScopeId = !string.IsNullOrEmpty(observation.ScopeId) ? observation.ScopeId : null,
                EyepieceId = !string.IsNullOrEmpty(observation.EyepieceId) ? observation.EyepieceId : null,
                LensId = !string.IsNullOrEmpty(observation.LensId) ? observation.LensId : null,
                FilterId = !string.IsNullOrEmpty(observation.FilterId) ? observation.FilterId : null,
                CameraId = !string.IsNullOrEmpty(observation.CameraId) ? observation.CameraId : null,
                Attachments = observation.Image.ToAttachments()
            };

            obs.Target.Id = Guid.NewGuid().ToString();
            obs.TargetId = obs.Target.Id;

            return obs;
        }

        private static SessionDB ToSession(this OALSession session, OALData data)
        {
            var observations = data.Observations.Where(i => i.SessionId == session.Id);
            string observerId = observations.Select(o => o.ObserverId).FirstOrDefault();
            string[] coObserverIds = session.CoObservers ?? new string[0];
            string siteId = observations.Select(o => o.SiteId).FirstOrDefault();

            // take worst sky conditions among observations related to session
            int? seeing = observations.Select(o => o.Seeing.ToIntNullable()).Min();
            double? faintestStar = observations.Select(o => o.FaintestStarSpecified ? o.FaintestStar : (double?)null).Min();
            double? skyQuality = observations.Select(o => o.SkyQuality?.ToBrightness()).Min();

            DateTime begin = observations.Min(obs => obs.Begin);
            if (session.Begin < begin)
            {
                begin = session.Begin;
            }

            DateTime end = observations.Max(obs => obs.EndSpecified ? obs.End : obs.Begin);
            if (session.End > end)
            {
                end = session.End;
            }

            return new SessionDB()
            {
                Id = session.Id,
                SiteId = session.SiteId,
                ObserverId = observerId,
                Begin = begin,
                End = end,
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

        private static SiteDB ToSite(this OALSite site)
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

        //private static CrdsEquatorial ToEquatorialCoordinates(this equPosType pos, DateTime dateTime)
        //{
        //    double ra = pos.ra.ToAngle();
        //    double dec = pos.dec.ToAngle();

        //    double jd = new Date(dateTime).ToJulianDay();
        //    var eq0 = new CrdsEquatorial(ra, dec);

        //    switch (pos.frame?.equinox ?? referenceFrameTypeEquinox.EqOfDate)
        //    {
        //        case referenceFrameTypeEquinox.B1950:                    
        //            eq0 = Precession.GetEquatorialCoordinates(eq0, Precession.ElementsFK5(Date.EPOCH_B1950, jd));
        //            break;
        //        case referenceFrameTypeEquinox.J2000:
        //            eq0 = Precession.GetEquatorialCoordinates(eq0, Precession.ElementsFK5(Date.EPOCH_J2000, jd));
        //            break;
        //        default:
        //            break;
        //    }

        //    // TODO: geocentric positions should be converted to topocentric

        //    return eq0;
        //}

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

        private static double? ToBrightness(this OALSurfaceBrightness surfaceBrightness)
        {
            if (surfaceBrightness.unit == OALSurfaceBrightnessUnit.MagsPerSquareArcSec)
                return surfaceBrightness.Value;
            else
                return surfaceBrightness.Value / 3600;
        }

        private static T BuildDeepSkyTargetDetails<T>(OALTargetDeepSky ds) where T : DeepSkyTargetDetails, new()
        {
            var details = new T();
            details.LargeDiameter = ds.LargeDiameter.ToAngle(unit: OALAngleUnit.ArcSec);
            details.SmallDiameter = ds.SmallDiameter.ToAngle(unit: OALAngleUnit.ArcSec);
            details.Brightness = ds.SurfBr?.ToBrightness();
            details.Magnitude = ds.VisMagSpecified ? ds.VisMag : (double?)null;
            return details;
        }

        private static T BuildDeepSkyObservationDetails<T>(OALFindingsDeepSky ds) where T : DeepSkyObservationDetails, new()
        {
            var details = new T();
            details.Rating = int.Parse(ds.Rating.ToStringEnum());
            details.SmallDiameter = ds.SmallDiameter.ToAngle(unit: OALAngleUnit.ArcSec);
            details.LargeDiameter = ds.LargeDiameter.ToAngle(unit: OALAngleUnit.ArcSec);
            details.Resolved = ds.ResolvedSpecified ? ds.Resolved : (bool?)null;
            details.Stellar = ds.StellarSpecified ? ds.Stellar : (bool?)null;
            details.Mottled = ds.MottledSpecified ? ds.Mottled : (bool?)null;
            details.Extended = ds.ExtendedSpecified ? ds.Extended : (bool?)null;
            return details;
        }

        private static TargetDB ToTarget(this OALTarget target, OALData data)
        {
            TargetDB result = new TargetDB();

            // Single star
            if (target is OALTargetStar st)
            {
                result.Type = "Star";
                result.Details = JsonConvert.SerializeObject(new StarTargetDetails()
                {
                    Magnitude = st.ApparentMagSpecified ? st.ApparentMag : (double?)null,
                    Classification = st.Classification
                }, jsonSettings);

                // Variable star
                if (target is OALTargetVariableStar vs)
                {
                    string[] novae = new string[] { "Nova", "Novae", "NA", "NB", "NC", "NR", "RN" };

                    result.Type = vs.Type != null && novae.Any(x => vs.Type.Equals(x, StringComparison.OrdinalIgnoreCase)) ? "Nova" : "VarStar";

                    result.Details = JsonConvert.SerializeObject(new VariableStarTargetDetails()
                    {
                        Magnitude = vs.ApparentMagSpecified ? vs.ApparentMag : (double?)null,
                        MaxMagnitude = vs.MaxApparentMagSpecified ? vs.MaxApparentMag : (double?)null,
                        Period = vs.PeriodSpecified ? vs.Period : (double?)null,
                        VarStarType = vs.Type,
                        Classification = vs.Classification
                    }, jsonSettings);
                }
            }
            // Multiple star (don't know why it's prefixed as "deepSky" in OAL)
            else if (target is OALTargetDeepSkyMS ms)
            {
                result.Type = "Star";
                result.Details = JsonConvert.SerializeObject(new StarTargetDetails(), jsonSettings);
            }
            // DeepSky object
            else if (target is OALTargetDeepSky)
            {
                // Asterism 
                if (target is OALTargetDeepSkyAS a)
                {
                    result.Type = "Asterism";
                    var details = BuildDeepSkyTargetDetails<DeepSkyAsterismTargetDetails>(a);
                    details.PositionAngle = a.PositionAngle.ToIntNullable();
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Globular Cluster
                else if (target is OALTargetDeepSkyGC gc)
                {
                    result.Type = "DeepSky.GlobularCluster";
                    var details = BuildDeepSkyTargetDetails<DeepSkyGlobularClusterTargetDetails>(gc);
                    details.MagStars = gc.MagStarsSpecified ? gc.MagStars : (double?)null;
                    details.Concentration = gc.Conc;
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Cluster of Galaxies 
                else if (target is OALTargetDeepSkyCG cg)
                {
                    result.Type = "DeepSky.GalaxyCluster";
                    var details = BuildDeepSkyTargetDetails<DeepSkyClusterOfGalaxiesTargetDetails>(cg);
                    details.Mag10 = cg.Mag10Specified ? cg.Mag10 : (double?)null;
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Dark nebula
                else if (target is OALTargetDeepSkyDN dn)
                {
                    result.Type = "DeepSky.DarkNebula";
                    var details = BuildDeepSkyTargetDetails<DeepSkyDarkNebulaTargetDetails>(dn);
                    details.PositionAngle = dn.PositionAngle.ToIntNullable();
                    details.Opacity = dn.Opacity.ToIntNullable();
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Double star
                else if (target is OALTargetDeepSkyDS ds)
                {
                    result.Type = "DeepSky.DoubleStar";
                    var details = BuildDeepSkyTargetDetails<DeepSkyDoubleStarTargetDetails>(ds);
                    details.PositionAngle = ds.PositionAngle.ToIntNullable();
                    details.Separation = ds.Separation.ToAngle(unit: OALAngleUnit.ArcSec);
                    details.CompanionMagnitude = ds.MagCompSpecified ? ds.MagComp : (double?)null;
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Galaxy
                else if (target is OALTargetDeepSkyGX gx)
                {
                    result.Type = "DeepSky.Galaxy";
                    var details = BuildDeepSkyTargetDetails<DeepSkyGalaxyTargetDetails>(gx);
                    details.PositionAngle = gx.PositionAngle.ToIntNullable();
                    details.HubbleType = gx.HubbleType;
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Galaxy nebula
                else if (target is OALTargetDeepSkyGN gn)
                {
                    result.Type = "DeepSky.GalacticNebula";
                    var details = BuildDeepSkyTargetDetails<DeepSkyGalaxyNebulaTargetDetails>(gn);
                    details.PositionAngle = gn.PositionAngle.ToIntNullable();
                    details.NebulaType = gn.NebulaType;
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Open Cluster
                else if (target is OALTargetDeepSkyOC oc)
                {
                    result.Type = "DeepSky.OpenCluster";
                    var details = BuildDeepSkyTargetDetails<DeepSkyOpenClusterTargetDetails>(oc);
                    details.BrightestStarMagnitude = oc.BrightestStarSpecified ? oc.BrightestStar : (double?)null;
                    details.StarsCount = oc.Stars.ToIntNullable();
                    details.TrumplerClass = oc.TrumplerClass;
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Planetary Nebula
                else if (target is OALTargetDeepSkyPN pn)
                {
                    result.Type = "DeepSky.PlanetaryNebula";
                    var details = BuildDeepSkyTargetDetails<DeepSkyPlanetaryNebulaTargetDetails>(pn);
                    details.CentralStarMagnitude = pn.MagStarSpecified ? pn.MagStar : (double?)null;
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Quasar
                else if (target is OALTargetDeepSkyQS qs)
                {
                    result.Type = "DeepSky.Quasar";
                    var details = BuildDeepSkyTargetDetails<DeepSkyQuasarTargetDetails>(qs);
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Star Cloud
                else if (target is OALTargetDeepSkySC sc)
                {
                    result.Type = "DeepSky.StarCloud";
                    var details = BuildDeepSkyTargetDetails<DeepSkyStarCloudTargetDetails>(sc);
                    details.PositionAngle = sc.PositionAngle.ToIntNullable();
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Unspecified
                else if (target is OALTargetDeepSkyNA na)
                {
                    result.Type = "DeepSky.Unspecified";
                    var details = BuildDeepSkyTargetDetails<DeepSkyUnspecifiedTargetDetails>(na);
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Other?
                else
                {
                    throw new NotImplementedException("This type is not supported.");
                }
            }
            else if (target is OALTargetSolarSystem)
            {
                if (target is OALTargetComet)
                {
                    result.Type = "Comet";
                }
                else if (target is OALTargetMinorPlanet)
                {
                    result.Type = "Asteroid";
                }
                else if (target is OALTargetMoon)
                {
                    result.Type = "Moon";
                }
                else if (target is OALTargetPlanet)
                {
                    result.Type = "Planet";
                }
                else if (target is OALTargetSun)
                {
                    result.Type = "Sun";
                }
            }

            else
            {
                throw new NotImplementedException($"Type of target {target.GetType()} not supported.");
            }

            result.Id = target.Id;
            result.Name = target.Name;
            result.CommonName = target.Name;
            result.Aliases = target.Alias.ToListOfValues();
            result.Source = target.Item;
            if (target.ItemElementName == OALDataSource.Observer)
            {
                var observer = data.Observers.FirstOrDefault(x => x.Id == target.Item);
                if (observer != null)
                {
                    result.Source = $"Observer: {observer.Name} {observer.Surname}".Trim();
                }
            }

            // TODO: convert to equinox of date 
            result.RightAscension = target.Position?.RA.ToAngle();

            // TODO: convert to equinox of date
            result.Declination = target.Position?.Dec.ToAngle();

            result.Constellation = target.Constellation;
            result.Notes = target.Notes;

            return result;
        }
    }
}
