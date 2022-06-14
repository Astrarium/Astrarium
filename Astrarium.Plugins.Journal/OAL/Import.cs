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
            var serializer = new XmlSerializer(typeof(observations));
            var stringData = File.ReadAllText(file);
            using (TextReader reader = new StringReader(stringData))
            {
                var data = (observations)serializer.Deserialize(reader);

                using (var db = new DatabaseContext())
                {
                    foreach (var site in data.sites)
                    {
                        SiteDB siteDB = site.ToSite();
                        db.Sites.Add(siteDB);
                    }

                    foreach (var observer in data.observers)
                    {
                        ObserverDB observerDB = observer.ToObserver();
                        db.Observers.Add(observerDB);
                    }

                    foreach (var optics in data.scopes)
                    {
                        OpticsDB opticsDB = optics.ToOptics();
                        db.Optics.Add(opticsDB);
                    }

                    foreach (var eyepiece in data.eyepieces)
                    {
                        EyepieceDB eyepieceDB = eyepiece.ToEyepiece();
                        db.Eyepieces.Add(eyepieceDB);
                    }

                    foreach (var lens in data.lenses)
                    {
                        LensDB lensDB = lens.ToLens();
                        db.Lenses.Add(lensDB);
                    }

                    foreach (var filter in data.filters)
                    {
                        FilterDB filterDB = filter.ToFilter();
                        db.Filters.Add(filterDB);
                    }

                    foreach (var imager in data.imagers)
                    {
                        ImagerDB imagerDB = imager.ToImager();
                        db.Imagers.Add(imagerDB);
                    }

                    db.SaveChanges();
                }

                foreach (var session in data.sessions)
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

                foreach (var obs in data.observation)
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

        private static LensDB ToLens(this lensType lens)
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

        private static string ToAccounts(this observerAccountType[] accounts)
        {
            Dictionary<string, string> accountsObject = new Dictionary<string, string>();
            if (accounts != null)
            {
                foreach (var a in accounts)
                {
                    accountsObject[a.name] = a.Value;
                }
            }
            return JsonConvert.SerializeObject(accountsObject, jsonSettings);
        }

        private static FilterDB ToFilter(this filterType filter)
        {
            return new FilterDB()
            {
                Id = filter.id,
                Model = filter.model,
                Vendor = filter.vendor,
                Type = filter.type.ToStringEnum(),
                Color = filter.colorSpecified ? filter.color.ToStringEnum() : null,
                Schott = filter.schott,
                Wratten = filter.wratten
            };
        }

        private static ImagerDB ToImager(this imagerType imager)
        {
            var imagerDb = new ImagerDB()
            {
                Id = imager.id,
                Model = imager.model,
                Vendor = imager.vendor,
                Remarks = imager.remarks
            };

            if (imager is ccdCameraType ccd)
            {
                CameraImagerDetails details = new CameraImagerDetails()
                {
                    PixelsX = int.Parse(ccd.pixelsX),
                    PixelsY = int.Parse(ccd.pixelsY),
                    PixelsXSize = ccd.pixelXSizeSpecified ? (double?)ccd.pixelXSize : (double?)null,
                    PixelsYSize = ccd.pixelYSizeSpecified ? (double?)ccd.pixelYSize : (double?)null,
                    Binning = int.Parse(ccd.binning)
                };
                imagerDb.Type = "Camera";
                imagerDb.Details = JsonConvert.SerializeObject(details, jsonSettings);
            }
            else
            {
                throw new NotImplementedException($"Unknown imager type: {imager.GetType()}");
            }

            return imagerDb;
        }

        private static EyepieceDB ToEyepiece(this eyepieceType eyepiece)
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

        private static OpticsDB ToOptics(this opticsType optics)
        {
            var scopeDb = new OpticsDB()
            {
                Id = optics.id,
                Aperture = optics.aperture,
                Type = optics.type,
                Vendor = optics.vendor,
                Model = optics.model,
                LightGrasp = optics.lightGraspSpecified ? optics.lightGrasp : (double?)null,
                OpticsType = optics is scopeType ? "Telescope" : "Fixed",
                OrientationErect = optics.orientation?.erect,
                OrientationTrueSided = optics.orientation?.truesided,
            };

            if (optics is scopeType scope)
            {
                scopeDb.OpticsType = "Telescope";
                scopeDb.Details = JsonConvert.SerializeObject(new ScopeDetails()
                {
                    FocalLength = scope.focalLength
                });
            }
            else if (optics is fixedMagnificationOpticsType fixedOptics)
            {
                scopeDb.OpticsType = "Fixed";
                scopeDb.Details = JsonConvert.SerializeObject(new FixedOpticsDetails()
                {
                    Magnification = fixedOptics.magnification,
                    TrueField = fixedOptics.trueField.ToAngle()
                });
            }
            else
            {
                throw new Exception($"Unknown optics type: {optics.GetType()}");
            }

            return scopeDb;
        }

        private static ObserverDB ToObserver(this observerType observer)
        {
            return new ObserverDB()
            {
                Id = observer.id,
                FirstName = observer.name,
                LastName = observer.surname,
                Accounts = observer.account.ToAccounts(),
                Contacts = observer.contact.ToListOfValues(),
                FSTOffset = observer.fstOffsetSpecified ? observer.fstOffset : (double?)null
            };
        }

        /// <summary>
        /// Creates new session object from observation.
        /// Used for observations without sessions.
        /// </summary>
        /// <param name="observation"></param>
        /// <returns></returns>
        private static SessionDB ToSession(this observationType observation)
        {
            return new SessionDB()
            {
                Id = Guid.NewGuid().ToString(),
                SiteId = observation.site,
                Begin = observation.begin,
                Seeing = observation.seeing.ToIntNullable(),
                SkyQuality = observation.skyquality?.ToBrightness(),
                FaintestStar = observation.faintestStarSpecified ? observation.faintestStar : (double?)null,
                // if observation end time not specified, set session end equal to begin time
                End = observation.endSpecified ? observation.end : observation.begin,
                CoObservers = new List<ObserverDB>(),
                Attachments = new List<AttachmentDB>(),
                ObserverId = observation.observer,
            };
        }

        private static ObservationDB ToObservation(this observationType observation, observations data)
        {
            // get target
            var target = data.targets.First(t => t.id == observation.target);

            // json-serialized finding details
            string jsonDetails = null;

            var finding = observation.result.FirstOrDefault();
            if (finding != null)
            {
                // Variable star
                if (finding is findingsVariableStarType vs)
                {
                    var details = new VariableStarObservationDetails();
                    details.ChartDate = vs.chartID?.Value;
                    details.NonAAVSOChart = vs.chartID?.nonAAVSOchartSpecified == true ? vs.chartID.nonAAVSOchart : (bool?)null;
                    details.ComparisonStars = vs.comparisonStar.ToListOfValues();
                    details.BrightSky = vs.brightSkySpecified ? vs.brightSky : (bool?)null;
                    details.Clouds = vs.cloudsSpecified ? vs.clouds : (bool?)null;
                    details.ComparismSequenceProblem = vs.comparismSequenceProblemSpecified ? vs.comparismSequenceProblem : (bool?)null;
                    details.FaintStar = vs.faintStarSpecified ? vs.faintStar : (bool?)null;
                    details.NearHorizion = vs.nearHorizionSpecified ? vs.nearHorizion : (bool?)null;
                    details.Outburst = vs.outburstSpecified ? vs.outburst : (bool?)null;
                    details.PoorSeeing = vs.poorSeeingSpecified ? vs.poorSeeing : (bool?)null;
                    details.StarIdentificationUncertain = vs.starIdentificationUncertainSpecified ? vs.starIdentificationUncertain : (bool?)null;
                    details.UnusualActivity = vs.unusualActivitySpecified ? vs.unusualActivity : (bool?)null;
                    jsonDetails = JsonConvert.SerializeObject(details, jsonSettings);
                }
                else if (finding is findingsDeepSkyType dst) 
                {
                    // Double star
                    if (finding is findingsDeepSkyDSType ds)
                    {
                        var details = BuildDeepSkyObservationDetails<DoubleStarObservationDetails>(ds);
                        details.ColorMainComponent = ds.colorMainSpecified ? ds.colorMain.ToStringEnum() : null;
                        details.ColorCompainionComponent = ds.colorCompanionSpecified ? ds.colorCompanion.ToStringEnum() : null;
                        details.EqualBrightness = ds.equalBrightnessSpecified ? ds.equalBrightness : (bool?)null;
                        details.NiceSurrounding = ds.niceSurroundingSpecified ? ds.niceSurrounding : (bool?)null;
                        jsonDetails = JsonConvert.SerializeObject(details, jsonSettings);
                    }

                    // Open cluster
                    else if (finding is findingsDeepSkyOCType oc)
                    {
                        var details = BuildDeepSkyObservationDetails<OpenClusterObservationDetails>(oc);
                        details.Character = oc.characterSpecified ? oc.character.ToStringEnum() : null;
                        details.PartlyUnresolved = oc.partlyUnresolvedSpecified ? oc.partlyUnresolved : (bool?)null;
                        details.UnusualShape = oc.unusualShapeSpecified ? oc.unusualShape : (bool?)null;
                        details.ColorContrasts = oc.colorContrastsSpecified ? oc.colorContrasts : (bool?)null;
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
                Id = observation.id,
                SessionId = observation.session,
                Begin = observation.begin,
                End = observation.endSpecified ? observation.end : observation.begin,

                Magnification = observation.magnificationSpecified ? observation.magnification : (double?)null,
                Accessories = observation.accessories,
                Target = target.ToTarget(),
                Result = finding?.description,
                Details = jsonDetails,
                ScopeId = !string.IsNullOrEmpty(observation.scope) ? observation.scope : null,
                EyepieceId = !string.IsNullOrEmpty(observation.eyepiece) ? observation.eyepiece : null,
                LensId = !string.IsNullOrEmpty(observation.lens) ? observation.lens : null,
                FilterId = !string.IsNullOrEmpty(observation.filter) ? observation.filter : null,
                ImagerId = !string.IsNullOrEmpty(observation.imager) ? observation.imager : null,
                Attachments = observation.image.ToAttachments()
            };

            obs.Target.Id = Guid.NewGuid().ToString();
            obs.TargetId = obs.Target.Id;

            return obs;
        }

        private static SessionDB ToSession(this sessionType session, observations data)
        {
            var observations = data.observation.Where(i => i.session == session.id);
            string observerId = observations.Select(o => o.observer).FirstOrDefault();
            string[] coObserverIds = session.coObserver ?? new string[0];
            string siteId = observations.Select(o => o.site).FirstOrDefault();

            // take worst sky conditions among observations related to session
            int? seeing = observations.Select(o => o.seeing.ToIntNullable()).Min();
            double? faintestStar = observations.Select(o => o.faintestStarSpecified ? o.faintestStar : (double?)null).Min();
            double? skyQuality = observations.Select(o => o.skyquality?.ToBrightness()).Min();

            DateTime begin = observations.Min(obs => obs.begin);
            if (session.begin < begin)
            {
                begin = session.begin;
            }

            DateTime end = observations.Max(obs => obs.endSpecified ? obs.end : obs.begin);
            if (session.end > end)
            {
                end = session.end;
            }

            return new SessionDB()
            {
                Id = session.id,
                SiteId = session.site,
                ObserverId = observerId,
                Begin = begin,
                End = end,
                Equipment = session.equipment,
                CoObservers = data.observers.Where(o => coObserverIds.Contains(o.id)).Select(x => x.ToObserver()).ToList(),
                Attachments = session.image.ToAttachments(),
                Comments = session.comments,
                Weather = session.weather,
                FaintestStar = faintestStar,
                Seeing = seeing,
                SkyQuality = skyQuality
            };
        }

        private static SiteDB ToSite(this siteType site)
        {
            return new SiteDB()
            {
                Id = site.id,
                Name = site.name,
                Latitude = site.latitude.ToAngle() ?? 0,
                Longitude = site.longitude.ToAngle() ?? 0,
                Elevation = site.elevationSpecified ? (double?)site.elevation : null,
                Timezone = site.timezone.ToDouble() / 60.0,
                IAUCode = site.code
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

        private static double? ToAngle(this angleType angle)
        {
            if (angle == null)
                return null;

            double value = angle.Value;
            switch (angle.unit)
            {
                case angleUnit.arcmin:
                    value = value / 60.0;
                    break;
                case angleUnit.arcsec:
                    value = value / 3600.0;
                    break;
                case angleUnit.deg:
                    break;
                case angleUnit.rad:
                    value = value * 180.0 / Math.PI;
                    break;
            }
            return value;
        }

        private static double? ToBrightness(this surfaceBrightnessType surfaceBrightness)
        {
            if (surfaceBrightness.unit == surfaceBrightnessUnit.magspersquarearcsec)
                return surfaceBrightness.Value;
            else
                return surfaceBrightness.Value / 3600;
        }

        private static T BuildDeepSkyTargetDetails<T>(deepSkyTargetType ds) where T : DeepSkyTargetDetails, new()
        {
            var details = new T();
            details.LargeDiameter = ds.largeDiameter.ToAngle();
            details.SmallDiameter = ds.smallDiameter.ToAngle();
            details.Brightness = ds.surfBr?.ToBrightness();
            details.Magnitude = ds.visMagSpecified ? ds.visMag : (double?)null;
            return details;
        }

        private static T BuildDeepSkyObservationDetails<T>(findingsDeepSkyType ds) where T : DeepSkyObservationDetails, new()
        {
            var details = new T();
            details.Rating = int.Parse(ds.rating.ToStringEnum());
            details.SmallDiameter = ds.smallDiameter.ToAngle();
            details.LargeDiameter = ds.largeDiameter.ToAngle();
            details.Resolved = ds.resolvedSpecified ? ds.resolved : (bool?)null;
            details.Stellar = ds.stellarSpecified ? ds.stellar : (bool?)null;
            details.Mottled = ds.mottledSpecified ? ds.mottled : (bool?)null;
            details.Extended = ds.extendedSpecified ? ds.extended : (bool?)null;
            return details;
        }

        private static TargetDB ToTarget(this observationTargetType target)
        {
            TargetDB result = new TargetDB();

            // Single star
            if (target is starTargetType st)
            {
                result.Type = "Star";
                result.Details = JsonConvert.SerializeObject(new StarTargetDetails()
                {
                    Magnitude = st.apparentMagSpecified ? st.apparentMag : (double?)null,
                    Classification = st.classification
                }, jsonSettings);
            }
            // Variable star
            else if (target is variableStarTargetType vs)
            {
                result.Type = "VarStar";
                result.Details = JsonConvert.SerializeObject(new VariableStarTargetDetails()
                {
                    Magnitude = vs.apparentMagSpecified ? vs.apparentMag : (double?)null,
                    MaxMagnitude = vs.maxApparentMagSpecified ? vs.maxApparentMag : (double?)null,
                    Period = vs.periodSpecified ? vs.period : (double?)null,
                    VarStarType = vs.type,
                    Classification = vs.classification
                }, jsonSettings);
            }
            // Multiple star (don't know why it's prefixed as "deepSky" in OAL)
            else if (target is deepSkyMS ms)
            {
                result.Type = "Star";
                result.Details = JsonConvert.SerializeObject(new StarTargetDetails(), jsonSettings);
            }
            // DeepSky object
            else if (target is deepSkyTargetType)
            {
                // Asterism 
                if (target is deepSkyAS a)
                {
                    result.Type = "Asterism";
                    var details = BuildDeepSkyTargetDetails<DeepSkyAsterismTargetDetails>(a);
                    details.PositionAngle = a.pa.ToIntNullable();
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Globular Cluster
                else if (target is deepSkyGC gc)
                {
                    result.Type = "DeepSky.GlobularCluster";
                    var details = BuildDeepSkyTargetDetails<DeepSkyGlobularClusterTargetDetails>(gc);
                    details.MagStars = gc.magStarsSpecified ? gc.magStars : (double?)null;
                    details.Concentration = gc.conc;
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Cluster of Galaxies 
                else if (target is deepSkyCG cg)
                {
                    result.Type = "DeepSky.GalaxyCluster";
                    var details = BuildDeepSkyTargetDetails<DeepSkyClusterOfGalaxiesTargetDetails>(cg);
                    details.Mag10 = cg.mag10Specified ? cg.mag10 : (double?)null;
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Dark nebula
                else if (target is deepSkyDN dn)
                {
                    result.Type = "DeepSky.DarkNebula";
                    var details = BuildDeepSkyTargetDetails<DeepSkyDarkNebulaTargetDetails>(dn);
                    details.PositionAngle = dn.pa.ToIntNullable();
                    details.Opacity = dn.opacity.ToIntNullable();
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Double star
                else if (target is deepSkyDS ds)
                {
                    result.Type = "DeepSky.DoubleStar";
                    var details = BuildDeepSkyTargetDetails<DeepSkyDoubleStarTargetDetails>(ds);
                    details.PositionAngle = ds.pa.ToIntNullable();
                    details.Separation = ds.separation.ToAngle();
                    details.CompanionMagnitude = ds.magCompSpecified ? ds.magComp : (double?)null;
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Galaxy
                else if (target is deepSkyGX gx)
                {
                    result.Type = "DeepSky.Galaxy";
                    var details = BuildDeepSkyTargetDetails<DeepSkyGalaxyTargetDetails>(gx);
                    details.PositionAngle = gx.pa.ToIntNullable();
                    details.HubbleType = gx.hubbleType;
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Galaxy nebula
                else if (target is deepSkyGN gn)
                {
                    result.Type = "DeepSky.GalacticNebula";
                    var details = BuildDeepSkyTargetDetails<DeepSkyGalaxyNebulaTargetDetails>(gn);
                    details.PositionAngle = gn.pa.ToIntNullable();
                    details.NebulaType = gn.nebulaType;
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Open Cluster
                else if (target is deepSkyOC oc)
                {
                    result.Type = "DeepSky.OpenCluster";
                    var details = BuildDeepSkyTargetDetails<DeepSkyOpenClusterTargetDetails>(oc);
                    details.BrightestStarMagnitude = oc.brightestStarSpecified ? oc.brightestStar : (double?)null;
                    details.StarsCount = oc.stars.ToIntNullable();
                    details.TrumplerClass = oc.@class;
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Planetary Nebula
                else if (target is deepSkyPN pn)
                {
                    result.Type = "DeepSky.PlanetaryNebula";
                    var details = BuildDeepSkyTargetDetails<DeepSkyPlanetaryNebulaTargetDetails>(pn);
                    details.CentralStarMagnitude = pn.magStarSpecified ? pn.magStar : (double?)null;
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Quasar
                else if (target is deepSkyQS qs)
                {
                    result.Type = "DeepSky.Quasar";
                    var details = BuildDeepSkyTargetDetails<DeepSkyQuasarTargetDetails>(qs);
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Star Cloud
                else if (target is deepSkySC sc)
                {
                    result.Type = "DeepSky.StarCloud";
                    var details = BuildDeepSkyTargetDetails<DeepSkyStarCloudTargetDetails>(sc);
                    details.PositionAngle = sc.pa.ToIntNullable();
                    result.Details = JsonConvert.SerializeObject(details, jsonSettings);
                }
                // Unspecified
                else if (target is deepSkyNA na)
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
            else if (target is SolarSystemTargetType)
            {
                if (target is CometTargetType)
                {
                    result.Type = "Comet";
                }
                else if (target is MinorPlanetTargetType)
                {
                    result.Type = "Asteroid";
                }
                else if (target is MoonTargetType)
                {
                    result.Type = "Moon";
                }
                else if (target is PlanetTargetType)
                {
                    result.Type = "Planet";
                }
                else if (target is SunTargetType)
                {
                    result.Type = "Sun";
                }
            }

            else
            {
                throw new NotImplementedException($"Type of target {target.GetType()} not supported.");
            }

            result.Id = target.id;
            result.Name = target.name;
            result.Aliases = target.alias.ToListOfValues();
            result.Source = target.Item;

            // TODO: convert to J2000
            result.RightAscension = target.position?.ra.ToAngle();

            // TODO: convert to J2000
            result.Declination = target.position?.dec.ToAngle();

            result.Constellation = target.constellation;
            result.Notes = target.notes;

            return result;
        }
    }
}
