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

        private static OALSite ToSite(this SiteDB site)
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

        private static OALObserver ToObserver(this ObserverDB observer)
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

        private static OALOptics ToOptics(this OpticsDB optics)
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

        private static OALEyepiece ToEyepiece(this EyepieceDB eyepiece)
        {
            return new OALEyepiece()
            {
                id = eyepiece.Id,
                model = eyepiece.Model,
                vendor = eyepiece.Vendor,
                apparentFOV = eyepiece.ApparentFOV != null ? new OALNonNegativeAngle() { Unit = OALAngleUnit.Deg, Value = eyepiece.ApparentFOV.Value } : null,
                focalLength = eyepiece.FocalLength,
                maxFocalLength = eyepiece.FocalLengthMax ?? 0,
                maxFocalLengthSpecified = eyepiece.FocalLengthMax != null
            };
        }

        private static OALLens ToLens(this LensDB lens)
        {
            return new OALLens()
            {
                id = lens.Id,
                factor = lens.Factor,
                model = lens.Model,
                vendor = lens.Vendor
            };
        }

        private static OALFilter ToFilter(this FilterDB filter)
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

        private static OALImager ToImager(this CameraDB camera)
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

        private static OALSession ToSession(this SessionDB session)
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

        private static OALObservation ToObservation(this ObservationDB observation, ICollection<SessionDB> sessions)
        {
            var session = sessions.FirstOrDefault(x => x.Id == observation.SessionId);

            OALFindings findings = new OALFindings();

            if (observation.Target.Type == "VarStar" || observation.Target.Type == "Nova")
            {
                var details = JsonConvert.DeserializeObject<VariableStarObservationDetails>(observation.Details);
                findings = new OALFindingsVariableStar()
                {
                    VisMag = new OALVariableStarVisMag()
                    {
                        FainterThan = details.VisMagFainterThan ?? false,
                        FainterThanSpecified = details.VisMagFainterThan != null,
                        Uncertain = details.VisMagUncertain ?? false,
                        UncertainSpecified = details.VisMagUncertain != null,
                        Value = details.VisMag
                    },
                    BrightSky = details.BrightSky ?? false,
                    BrightSkySpecified = details.BrightSky != null,
                    ChartId = new OALVariableStarChartId()
                    {
                        NonAAVSOChart = details.NonAAVSOChart ?? false,
                        NonAAVSOChartSpecified = details.NonAAVSOChart != null,
                        Value = details.ChartDate
                    },
                    Clouds = details.Clouds ?? false,
                    CloudsSpecified = details.Clouds != null,
                    ComparismSequenceProblem = details.ComparismSequenceProblem ?? false,
                    ComparismSequenceProblemSpecified = details.ComparismSequenceProblem != null,
                    ComparisonStar = JsonConvert.DeserializeObject<string[]>(details.ComparisonStars ?? "[]"),
                    FaintStar = details.FaintStar ?? false,
                    FaintStarSpecified = details.FaintStar != null,
                    NearHorizion = details.NearHorizion ?? false,
                    NearHorizionSpecified = details.NearHorizion != null,
                    Outburst = details.Outburst ?? false,
                    OutburstSpecified = details.Outburst != null,
                    PoorSeeing = details.PoorSeeing ?? false,
                    PoorSeeingSpecified = details.PoorSeeing != null,
                    StarIdentificationUncertain = details.StarIdentificationUncertain ?? false,
                    StarIdentificationUncertainSpecified = details.StarIdentificationUncertain != null,
                    UnusualActivity = details.UnusualActivity ?? false,
                    UnusualActivitySpecified = details.UnusualActivity != null
                };
            }
            else if (observation.Target.Type.StartsWith("DeepSky."))
            {
                DeepSkyObservationDetails detailsDs = null;
                if (observation.Target.Type == "DeepSky.DoubleStar")
                {
                    var details = JsonConvert.DeserializeObject<DoubleStarObservationDetails>(observation.Details);
                    findings = new OALFindingsDeepSkyDS()
                    {
                        EqualBrightness = details.EqualBrightness ?? false,
                        EqualBrightnessSpecified = details.EqualBrightness != null,
                        ColorMain = GetValueFromXmlEnumAttribute<OALStarColor>(details.ColorMainComponent),
                        ColorMainSpecified = !string.IsNullOrEmpty(details.ColorMainComponent),
                        ColorCompanion = GetValueFromXmlEnumAttribute<OALStarColor>(details.ColorCompanionComponent),
                        ColorCompanionSpecified = !string.IsNullOrEmpty(details.ColorCompanionComponent),
                        NiceSurrounding = details.NiceSurrounding ?? false,
                        NiceSurroundingSpecified = details.NiceSurrounding != null
                    };
                    detailsDs = details;
                }
                if (observation.Target.Type == "DeepSky.OpenCluster")
                {
                    var details = JsonConvert.DeserializeObject<OpenClusterObservationDetails>(observation.Details);
                    findings = new OALFindingsDeepSkyOC()
                    {
                        Character = GetValueFromXmlEnumAttribute<OALClusterCharacter>(details.Character),
                        CharacterSpecified = !string.IsNullOrEmpty(details.Character),
                        UnusualShape = details.UnusualShape ?? false,
                        UnusualShapeSpecified = details.UnusualShape != null,
                        PartlyUnresolved = details.PartlyUnresolved ?? false,
                        PartlyUnresolvedSpecified = details.PartlyUnresolved != null,
                        ColorContrasts = details.ColorContrasts ?? false,
                        ColorContrastsSpecified = details.ColorContrasts != null
                    };
                    detailsDs = details;
                }


                var findingsDs = findings as OALFindingsDeepSky;

                if (findingsDs != null)
                {
                    findingsDs.Extended = detailsDs.Extended ?? false;
                    findingsDs.ExtendedSpecified = detailsDs != null;
                    findingsDs.LargeDiameter = detailsDs.LargeDiameter != null ? new OALNonNegativeAngle() { Unit = OALAngleUnit.ArcSec, Value = detailsDs.LargeDiameter.Value } : null;
                    findingsDs.SmallDiameter = detailsDs.SmallDiameter != null ? new OALNonNegativeAngle() { Unit = OALAngleUnit.ArcSec, Value = detailsDs.SmallDiameter.Value } : null;
                    findingsDs.Mottled = detailsDs.Mottled ?? false;
                    findingsDs.MottledSpecified = detailsDs.Mottled != null;
                    findingsDs.Rating = (OALFindingsDeepSkyRating)detailsDs.Rating;
                    findingsDs.Resolved = detailsDs.Resolved ?? false;
                    findingsDs.ResolvedSpecified = detailsDs.Resolved != null;
                    findingsDs.Stellar = detailsDs.Stellar ?? false;
                    findingsDs.StellarSpecified = detailsDs.Stellar != null;
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
                FaintestStarSpecified = false,
                Magnification = observation.Magnification ?? 0,
                MagnificationSpecified = observation.Magnification != null,
                Image = observation.Attachments?.Select(x => x.FilePath).ToArray(),                
                ObserverId = session?.ObserverId,
                FaintestStar = session?.FaintestStar ?? 0,
                Seeing = session?.Seeing?.ToString(),
                SiteId = session?.SiteId,
                SkyQuality = session?.SkyQuality != null ? new OALSurfaceBrightness() { unit = OALSurfaceBrightnessUnit.MagsPerSquareArcSec, Value = session.SkyQuality.Value } : null,
                Result = new OALFindings[] { findings }
            };
        }

        private static OALTarget ToTarget(this TargetDB target)
        {
            OALTarget tar = null;
            TargetDetails details = null;

            switch (target.Type)
            {
                case "Star":
                    details = JsonConvert.DeserializeObject<StarTargetDetails>(target.Details);
                    tar = new OALTargetStar();
                    break;
                case "Nova":
                case "VarStar":
                    details = JsonConvert.DeserializeObject<VariableStarTargetDetails>(target.Details);
                    tar = new OALTargetVariableStar();
                    break;
                case "Asterism":
                    details = JsonConvert.DeserializeObject<DeepSkyAsterismTargetDetails>(target.Details);
                    tar = new OALTargetDeepSkyAS();
                    break;
                case "DeepSky.GlobularCluster":
                    details = JsonConvert.DeserializeObject<DeepSkyGlobularClusterTargetDetails>(target.Details);
                    tar = new OALTargetDeepSkyGC();
                    break;
                case "DeepSky.GalaxyCluster":
                    tar = new OALTargetDeepSkyCG();
                    details = JsonConvert.DeserializeObject<DeepSkyClusterOfGalaxiesTargetDetails>(target.Details);
                    break;
                case "DeepSky.DarkNebula":
                    tar = new OALTargetDeepSkyDN();
                    details = JsonConvert.DeserializeObject<DeepSkyDarkNebulaTargetDetails>(target.Details);
                    break;
                case "DeepSky.DoubleStar":
                    tar = new OALTargetDeepSkyDS();
                    details = JsonConvert.DeserializeObject<DeepSkyDoubleStarTargetDetails>(target.Details);
                    break;
                case "DeepSky.Galaxy":
                    tar = new OALTargetDeepSkyGX();
                    details = JsonConvert.DeserializeObject<DeepSkyGalaxyTargetDetails>(target.Details);
                    break;
                case "DeepSky.GalacticNebula":
                    tar = new OALTargetDeepSkyGN();
                    details = JsonConvert.DeserializeObject<DeepSkyGalaxyNebulaTargetDetails>(target.Details);
                    break;
                case "DeepSky.OpenCluster":
                    tar = new OALTargetDeepSkyOC();
                    details = JsonConvert.DeserializeObject<DeepSkyOpenClusterTargetDetails>(target.Details);
                    break;
                case "DeepSky.PlanetaryNebula":
                    tar = new OALTargetDeepSkyPN();
                    details = JsonConvert.DeserializeObject<DeepSkyPlanetaryNebulaTargetDetails>(target.Details);
                    break;
                case "DeepSky.Quasar":
                    tar = new OALTargetDeepSkyQS();
                    details = JsonConvert.DeserializeObject<DeepSkyQuasarTargetDetails>(target.Details);
                    break;
                case "DeepSky.StarCloud":
                    tar = new OALTargetDeepSkySC();
                    details = JsonConvert.DeserializeObject<DeepSkyStarCloudTargetDetails>(target.Details);
                    break;
                case "DeepSky.Unspecified":
                    tar = new OALTargetDeepSkyNA();
                    details = JsonConvert.DeserializeObject<DeepSkyUnspecifiedTargetDetails>(target.Details);
                    break;
                case "Comet":
                    tar = new OALTargetComet();
                    break;
                case "Asteroid":
                    tar = new OALTargetMinorPlanet();
                    break;
                case "Moon":
                    tar = new OALTargetMoon();
                    break;
                case "Planet":
                    tar = new OALTargetPlanet();
                    break;
                case "Sun":
                    tar = new OALTargetSun();
                    break;
                default:
                    throw new Exception("Unknown target type");
            }
            if (tar is OALTargetStar star)
            {
                var d = details as StarTargetDetails;
                star.ApparentMag = d.Magnitude ?? 0;
                star.ApparentMagSpecified = d.Magnitude != null;
                star.Classification = d.Classification;
            }
            if (tar is OALTargetVariableStar varStar)
            {
                var d = details as VariableStarTargetDetails;
                varStar.MaxApparentMag = d.MaxMagnitude ?? 0;
                varStar.MaxApparentMagSpecified = d.MaxMagnitude != null;
                varStar.Type = d.VarStarType;
                varStar.Period = d.Period ?? 0;
                varStar.PeriodSpecified = d.Period != null;
            }
            if (tar is OALTargetDeepSky deepSky)
            {
                var d = details as DeepSkyTargetDetails;
                if (d.SmallDiameter != null)
                {
                    deepSky.SmallDiameter = new OALNonNegativeAngle() { Value = d.SmallDiameter.Value, Unit = OALAngleUnit.ArcSec };
                }
                if (d.LargeDiameter != null)
                {
                    deepSky.LargeDiameter = new OALNonNegativeAngle() { Value = d.LargeDiameter.Value, Unit = OALAngleUnit.ArcSec };
                }
                if (d.Brightness != null)
                {
                    deepSky.SurfBr = new OALSurfaceBrightness() { Value = d.Brightness.Value, unit = OALSurfaceBrightnessUnit.MagsPerSquareArcSec };
                }
                deepSky.VisMag = d.Magnitude ?? 0;
                deepSky.VisMagSpecified = d.Magnitude != null;
            }
            if (tar is OALTargetDeepSkyAS asterism)
            {
                var d = details as DeepSkyAsterismTargetDetails;
                asterism.PositionAngle = d.PositionAngle?.ToString();
            }
            if (tar is OALTargetDeepSkyGC gc)
            {
                var d = details as DeepSkyGlobularClusterTargetDetails;
                gc.MagStars = d.MagStars ?? 0;
                gc.MagStarsSpecified = d.MagStars != null;
                gc.Conc = d.Concentration;
            }
            if (tar is OALTargetDeepSkyCG cg)
            {
                var d = details as DeepSkyClusterOfGalaxiesTargetDetails;
                cg.Mag10 = d.Mag10 ?? 0;
                cg.Mag10Specified = d.Mag10 != null;
            }
            if (tar is OALTargetDeepSkyDN dn)
            {
                var d = details as DeepSkyDarkNebulaTargetDetails;
                dn.PositionAngle = d.PositionAngle?.ToString();
                dn.Opacity = d.Opacity?.ToString();
            }
            if (tar is OALTargetDeepSkyDS ds)
            {
                var d = details as DeepSkyDoubleStarTargetDetails;
                ds.PositionAngle = d.PositionAngle?.ToString();
                ds.MagComp = d.CompanionMagnitude ?? 0;
                ds.MagCompSpecified = d.CompanionMagnitude != null;
                if (d.Separation != null)
                {
                    ds.Separation = new OALNonNegativeAngle() { Value = d.Separation.Value, Unit = OALAngleUnit.ArcSec };
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

            tar.Id = target.Id;
            tar.Alias = JsonConvert.DeserializeObject<string[]>(target.Aliases);
            tar.Constellation = details.Constellation;
            tar.Name = target.Name;
            tar.Notes = target.Notes;
            tar.Item = target.Source;
            tar.ItemElementName = OALDataSource.DataSource;
            if (details.RA != null && details.Dec != null)
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

        private static OALAngle ToAngle(this double angle)
        {
            return new OALAngle() { Unit = OALAngleUnit.Deg, Value = angle };
        }

        private static OALNonNegativeAngle ToUnsignedAngle(this double angle)
        {
            return new OALNonNegativeAngle() { Unit = OALAngleUnit.Deg, Value = angle };
        }

        private static T GetValueFromXmlEnumAttribute<T>(string value) where T : Enum
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
