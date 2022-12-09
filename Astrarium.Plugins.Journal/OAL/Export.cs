using Astrarium.Plugins.Journal.Database;
using Astrarium.Plugins.Journal.Database.Entities;
using Newtonsoft.Json;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Astrarium.Plugins.Journal.Types;

namespace Astrarium.Plugins.Journal.OAL
{
    public static class Export
    {
        public static void ExportToOAL(string file)
        {
            using (var db = new DatabaseContext())
            {
                var data = new observations();
                data.sites = db.Sites.ToArray().Select(x => x.ToSite()).ToArray();
                data.observers = db.Observers.ToArray().Select(x => x.ToObserver()).ToArray();
                data.scopes = db.Optics.ToArray().Select(x => x.ToOptics()).ToArray();
                data.eyepieces = db.Eyepieces.ToArray().Select(x => x.ToEyepiece()).ToArray();
                data.lenses = db.Lenses.ToArray().Select(x => x.ToLens()).ToArray();
                data.filters = db.Filters.ToArray().Select(x => x.ToFilter()).ToArray();
                data.imagers = db.Cameras.ToArray().Select(x => x.ToImager()).ToArray();

                ICollection<SessionDB> sessions = db.Sessions.Include(x => x.CoObservers).Include(x => x.Attachments).ToArray();
                ICollection<TargetDB> targets = db.Targets.ToArray();

                data.sessions = sessions.Select(x => x.ToSession()).ToArray();
                data.observation = db.Observations.Include(x => x.Target).Include(x => x.Attachments).ToArray().Select(x => x.ToObservation(sessions)).ToArray();
                data.targets = targets.Select(x => x.ToTarget()).ToArray();

                var serializer = new XmlSerializer(typeof(observations));
                using (var stream = new StreamWriter(file))
                {
                    serializer.Serialize(stream, data);
                }
            }
        }

        private static siteType ToSite(this SiteDB site)
        {
            return new siteType()
            {
                id = site.Id,
                name = site.Name,
                latitude = site.Latitude.ToAngle(),
                longitude = site.Longitude.ToAngle(),
                elevation = site.Elevation != null ? site.Elevation.Value : 0,
                elevationSpecified = site.Elevation != null,
                timezone = (site.Timezone * 60).ToString(CultureInfo.InvariantCulture),
                code = site.IAUCode
            };
        }

        private static observerType ToObserver(this ObserverDB observer)
        {
            return new observerType()
            {
                id = observer.Id,
                name = observer.FirstName,
                surname = observer.LastName,
                account = JsonConvert.DeserializeObject<Dictionary<string, string>>(observer.Accounts)
                    .Select(kv => new observerAccountType() { name = kv.Key, Value = kv.Value })
                    .ToArray(),
                contact = JsonConvert.DeserializeObject<string[]>(observer.Contacts),
                fstOffset = observer.FSTOffset != null ? observer.FSTOffset.Value : 0,
                fstOffsetSpecified = observer.FSTOffset != null
            };
        }

        private static opticsType ToOptics(this OpticsDB optics)
        {
            opticsType opt = null;

            if (optics.Type == "Telescope")
            {
                var details = JsonConvert.DeserializeObject<ScopeDetails>(optics.Details);
                opt = new scopeType()
                {
                    focalLength = details.FocalLength
                };
            }
            else if (optics.Type == "Fixed")
            {
                var details = JsonConvert.DeserializeObject<FixedOpticsDetails>(optics.Details);
                opt = new fixedMagnificationOpticsType()
                {
                    magnification = details.Magnification,
                    trueField = details.TrueField != null ? new nonNegativeAngleType() { unit = angleUnit.deg, Value = details.TrueField.Value } : null
                };
            }
            else
            {
                throw new Exception("Unknown optics type");
            }

            opt.id = optics.Id;
            opt.aperture = optics.Aperture;
            opt.type = optics.Scheme;
            opt.vendor = optics.Vendor;
            opt.model = optics.Model;
            opt.lightGraspSpecified = optics.LightGrasp != null;
            opt.lightGrasp = optics.LightGrasp ?? 0;
            opt.orientation = new opticsTypeOrientation()
            {
                erect = optics.OrientationErect ?? false,
                truesided = optics.OrientationTrueSided ?? false
            };

            return opt;
        }

        private static eyepieceType ToEyepiece(this EyepieceDB eyepiece)
        {
            return new eyepieceType()
            {
                id = eyepiece.Id,
                model = eyepiece.Model,
                vendor = eyepiece.Vendor,
                apparentFOV = eyepiece.ApparentFOV != null ? new nonNegativeAngleType() { unit = angleUnit.deg, Value = eyepiece.ApparentFOV.Value } : null,
                focalLength = eyepiece.FocalLength,
                maxFocalLength = eyepiece.FocalLengthMax ?? 0,
                maxFocalLengthSpecified = eyepiece.FocalLengthMax != null
            };
        }

        private static lensType ToLens(this LensDB lens)
        {
            return new lensType()
            {
                id = lens.Id,
                factor = lens.Factor,
                model = lens.Model,
                vendor = lens.Vendor
            };
        }

        private static filterType ToFilter(this FilterDB filter)
        {
            return new filterType()
            {
                id = filter.Id,
                vendor = filter.Vendor,
                model = filter.Model,
                type = GetValueFromXmlEnumAttribute<filterKind>(filter.Type),
                color = filter.Color != null ? GetValueFromXmlEnumAttribute<filterColorType>(filter.Color) : filterColorType.lightred,
                colorSpecified = filter.Color != null,
                wratten = filter.Wratten
            };
        }

        private static imagerType ToImager(this CameraDB camera)
        {
            return new ccdCameraType()
            {
                id = camera.Id,
                vendor = camera.Vendor,
                model = camera.Model,
                binning = camera.Binning.ToString(),
                pixelsX = camera.PixelsX.ToString(),
                pixelsY = camera.PixelsY.ToString(),
                pixelXSize = (decimal)(camera.PixelXSize ?? 0),
                pixelXSizeSpecified = camera.PixelXSize != null,
                pixelYSize = (decimal)(camera.PixelYSize ?? 0),
                pixelYSizeSpecified = camera.PixelYSize != null,
                remarks = camera.Remarks
            };
        }

        private static sessionType ToSession(this SessionDB session)
        {
            return new sessionType()
            {
                id = session.Id,
                begin = session.Begin,
                end = session.End,
                site = session.SiteId,
                equipment = session.Equipment,
                comments = session.Comments,
                weather = session.Weather,
                image = session.Attachments?.Select(x => x.FilePath).ToArray(),
                coObserver = session.CoObservers?.Select(x => x.Id).ToArray(),
            };
        }

        private static observationType ToObservation(this ObservationDB observation, ICollection<SessionDB> sessions)
        {
            var session = sessions.FirstOrDefault(x => x.Id == observation.SessionId);

            var findings = new findingsType();

            if (observation.Target.Type == "VarStar" || observation.Target.Type == "Nova")
            {
                var details = JsonConvert.DeserializeObject<VariableStarObservationDetails>(observation.Details);
                findings = new findingsVariableStarType()
                {
                    visMag = new variableStarVisMagType()
                    {
                        fainterThan = details.VisMagFainterThan ?? false,
                        fainterThanSpecified = details.VisMagFainterThan != null,
                        uncertain = details.VisMagUncertain ?? false,
                        uncertainSpecified = details.VisMagUncertain != null,
                        Value = details.VisMag
                    },
                    brightSky = details.BrightSky ?? false,
                    brightSkySpecified = details.BrightSky != null,
                    chartID = new variableStarChartIDType()
                    {
                        nonAAVSOchart = details.NonAAVSOChart ?? false,
                        nonAAVSOchartSpecified = details.NonAAVSOChart != null,
                        Value = details.ChartDate
                    },
                    clouds = details.Clouds ?? false,
                    cloudsSpecified = details.Clouds != null,
                    comparismSequenceProblem = details.ComparismSequenceProblem ?? false,
                    comparismSequenceProblemSpecified = details.ComparismSequenceProblem != null,
                    comparisonStar = JsonConvert.DeserializeObject<string[]>(details.ComparisonStars ?? "[]"),
                    faintStar = details.FaintStar ?? false,
                    faintStarSpecified = details.FaintStar != null,
                    nearHorizion = details.NearHorizion ?? false,
                    nearHorizionSpecified = details.NearHorizion != null,
                    outburst = details.Outburst ?? false,
                    outburstSpecified = details.Outburst != null,
                    poorSeeing = details.PoorSeeing ?? false,
                    poorSeeingSpecified = details.PoorSeeing != null,
                    starIdentificationUncertain = details.StarIdentificationUncertain ?? false,
                    starIdentificationUncertainSpecified = details.StarIdentificationUncertain != null,
                    unusualActivity = details.UnusualActivity ?? false,
                    unusualActivitySpecified = details.UnusualActivity != null
                };
            }

            // TODO: other types of ObservationDetails

            findings.description = observation.Result;

            return new observationType()
            {
                id = observation.Id,
                begin = observation.Begin,
                end = observation.End,
                endSpecified = true,
                accessories = observation.Accessories,
                eyepiece = observation.EyepieceId,
                filter = observation.FilterId,
                imager = observation.CameraId,
                lens = observation.LensId,
                scope = observation.ScopeId,
                session = observation.SessionId,
                target = observation.TargetId,
                faintestStarSpecified = false,
                magnification = observation.Magnification ?? 0,
                magnificationSpecified = observation.Magnification != null,
                image = observation.Attachments?.Select(x => x.FilePath).ToArray(),                
                observer = session?.ObserverId,
                faintestStar = session?.FaintestStar ?? 0,
                seeing = session?.Seeing?.ToString(),
                site = session?.SiteId,
                skyquality = session?.SkyQuality != null ? new surfaceBrightnessType() { unit = surfaceBrightnessUnit.magspersquarearcsec, Value = session.SkyQuality.Value } : null,
                result = new findingsType[] { findings }
            };
        }

        private static observationTargetType ToTarget(this TargetDB target)
        {
            observationTargetType tar = null;
            object details = null;

            switch (target.Type)
            {
                case "Star":
                    details = JsonConvert.DeserializeObject<StarTargetDetails>(target.Details);
                    tar = new starTargetType();
                    break;
                case "Nova":
                case "VarStar":
                    details = JsonConvert.DeserializeObject<VariableStarTargetDetails>(target.Details);
                    tar = new variableStarTargetType();
                    break;
                case "Asterism":
                    details = JsonConvert.DeserializeObject<DeepSkyAsterismTargetDetails>(target.Details);
                    tar = new deepSkyAS();
                    break;
                case "DeepSky.GlobularCluster":
                    details = JsonConvert.DeserializeObject<DeepSkyGlobularClusterTargetDetails>(target.Details);
                    tar = new deepSkyGC();
                    break;
                case "DeepSky.GalaxyCluster":
                    tar = new deepSkyCG();
                    details = JsonConvert.DeserializeObject<DeepSkyClusterOfGalaxiesTargetDetails>(target.Details);
                    break;
                case "DeepSky.DarkNebula":
                    tar = new deepSkyDN();
                    details = JsonConvert.DeserializeObject<DeepSkyDarkNebulaTargetDetails>(target.Details);
                    break;
                case "DeepSky.DoubleStar":
                    tar = new deepSkyDS();
                    details = JsonConvert.DeserializeObject<DeepSkyDoubleStarTargetDetails>(target.Details);
                    break;
                case "DeepSky.Galaxy":
                    tar = new deepSkyGX();
                    details = JsonConvert.DeserializeObject<DeepSkyGalaxyTargetDetails>(target.Details);
                    break;
                case "DeepSky.GalacticNebula":
                    tar = new deepSkyGN();
                    details = JsonConvert.DeserializeObject<DeepSkyGalaxyNebulaTargetDetails>(target.Details);
                    break;
                case "DeepSky.OpenCluster":
                    tar = new deepSkyOC();
                    details = JsonConvert.DeserializeObject<DeepSkyOpenClusterTargetDetails>(target.Details);
                    break;
                case "DeepSky.PlanetaryNebula":
                    tar = new deepSkyPN();
                    details = JsonConvert.DeserializeObject<DeepSkyPlanetaryNebulaTargetDetails>(target.Details);
                    break;
                case "DeepSky.Quasar":
                    tar = new deepSkyQS();
                    details = JsonConvert.DeserializeObject<DeepSkyQuasarTargetDetails>(target.Details);
                    break;
                case "DeepSky.StarCloud":
                    tar = new deepSkySC();
                    details = JsonConvert.DeserializeObject<DeepSkyStarCloudTargetDetails>(target.Details);
                    break;
                case "DeepSky.Unspecified":
                    tar = new deepSkyNA();
                    details = JsonConvert.DeserializeObject<DeepSkyUnspecifiedTargetDetails>(target.Details);
                    break;
                case "Comet":
                    tar = new CometTargetType();
                    details = JsonConvert.DeserializeObject<DeepSkyPlanetaryNebulaTargetDetails>(target.Details);
                    break;
                case "Asteroid":
                    tar = new MinorPlanetTargetType();
                    break;
                case "Moon":
                    tar = new MoonTargetType();
                    break;
                case "Planet":
                    tar = new PlanetTargetType();
                    break;
                case "Sun":
                    tar = new SunTargetType();
                    break;
                default:
                    throw new Exception("Unknown target type");
            }
            if (tar is starTargetType star)
            {
                var d = details as StarTargetDetails;
                star.apparentMag = d.Magnitude ?? 0;
                star.apparentMagSpecified = d.Magnitude != null;
                star.classification = d.Classification;
            }
            if (tar is variableStarTargetType varStar)
            {
                var d = details as VariableStarTargetDetails;
                varStar.maxApparentMag = d.MaxMagnitude ?? 0;
                varStar.maxApparentMagSpecified = d.MaxMagnitude != null;
                varStar.type = d.VarStarType;
                varStar.period = d.Period ?? 0;
                varStar.periodSpecified = d.Period != null;
            }
            if (tar is deepSkyTargetType deepSky)
            {
                var d = details as DeepSkyTargetDetails;
                if (d.SmallDiameter != null)
                {
                    deepSky.smallDiameter = new nonNegativeAngleType() { Value = d.SmallDiameter.Value, unit = angleUnit.arcsec };
                }
                if (d.LargeDiameter != null)
                {
                    deepSky.largeDiameter = new nonNegativeAngleType() { Value = d.LargeDiameter.Value, unit = angleUnit.arcsec };
                }
                if (d.Brightness != null)
                {
                    deepSky.surfBr = new surfaceBrightnessType() { Value = d.Brightness.Value, unit = surfaceBrightnessUnit.magspersquarearcsec };
                }
                deepSky.visMag = d.Magnitude ?? 0;
                deepSky.visMagSpecified = d.Magnitude != null;
            }
            if (tar is deepSkyAS asterism)
            {
                var d = details as DeepSkyAsterismTargetDetails;
                asterism.pa = d.PositionAngle?.ToString();
            }
            if (tar is deepSkyGC gc)
            {
                var d = details as DeepSkyGlobularClusterTargetDetails;
                gc.magStars = d.MagStars ?? 0;
                gc.magStarsSpecified = d.MagStars != null;
                gc.conc = d.Concentration;
            }
            if (tar is deepSkyCG cg)
            {
                var d = details as DeepSkyClusterOfGalaxiesTargetDetails;
                cg.mag10 = d.Mag10 ?? 0;
                cg.mag10Specified = d.Mag10 != null;
            }
            if (tar is deepSkyDN dn)
            {
                var d = details as DeepSkyDarkNebulaTargetDetails;
                dn.pa = d.PositionAngle?.ToString();
                dn.opacity = d.Opacity?.ToString();
            }
            if (tar is deepSkyDS ds)
            {
                var d = details as DeepSkyDoubleStarTargetDetails;
                ds.pa = d.PositionAngle?.ToString();
                ds.magComp = d.CompanionMagnitude ?? 0;
                ds.magCompSpecified = d.CompanionMagnitude != null;
                if (d.Separation != null)
                {
                    ds.separation = new nonNegativeAngleType() { Value = d.Separation.Value, unit = angleUnit.arcsec };
                }
            }

            // TODO: handle other types
            //deepSkyGX
            //deepSkyGN
            //deepSkyOC
            //deepSkyPN
            //deepSkyQS
            //deepSkySC
            //deepSkyNA

            tar.id = target.Id;
            tar.alias = JsonConvert.DeserializeObject<string[]>(target.Aliases);
            tar.constellation = target.Constellation;
            tar.name = target.Name;
            tar.notes = target.Notes;
            tar.Item = target.Source;
            tar.ItemElementName = ItemChoiceType.datasource;
            if (target.RightAscension != null && target.Declination != null)
            {
                tar.position = new equPosType()
                {
                    ra = ToUnsignedAngle(target.RightAscension.Value),
                    dec = ToAngle(target.Declination.Value),
                    frame = new referenceFrameType()
                    {
                        equinox = referenceFrameTypeEquinox.EqOfDate,

                        // TODO: is it correct?
                        origin = referenceFrameTypeOrigin.topo
                    }
                };
            }
            return tar;
        }

        private static angleType ToAngle(this double angle)
        {
            return new angleType() { unit = angleUnit.deg, Value = angle };
        }

        private static nonNegativeAngleType ToUnsignedAngle(this double angle)
        {
            return new nonNegativeAngleType() { unit = angleUnit.deg, Value = angle };
        }

        private static T GetValueFromXmlEnumAttribute<T>(string value) where T : Enum
        {
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
