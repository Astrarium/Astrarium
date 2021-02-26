using Astrarium.Algorithms;
using Astrarium.Plugins.ObservationsLog.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ObservationsLog.OAL
{
    public static class Mappings
    {
        public static ICollection<Session> ImportAll(this observations data)
        {
            var observationsWithoutSession = data.observation.Where(i => string.IsNullOrEmpty(i.session)).ToList();

            var newSessions = observationsWithoutSession.Select(o => 
                new Session()
                {
                    Id = Guid.NewGuid().ToString(),
                    Site = data.sites.FirstOrDefault(i => i.id == o.site).ToSite(),
                    Seeing = o.seeing,
                    Observer = data.observers.FirstOrDefault(i => i.id == o.observer).ToObserver(),
                    FaintestStar = o.faintestStarSpecified ? (double?)o.faintestStar : null,
                    SkyBrightness = o.skyquality?.ToBrightness(),  
                    Observations = new List<Observation>() { o.ToObservation(data) }
                }
            ).ToArray();

            return data.sessions.Select(s => s.ToSession(data)).Concat(newSessions).ToArray();
        }

        private static Observer ToObserver(this observerType observer)
        {
            return new Observer()
            {
                Id = observer.id,
                Name = observer.name,
                Surname = observer.surname
            };
        }

        private static Observation ToObservation(this observationType observation, observations data)
        {
            return new Observation()
            {
                Id = observation.id,
                Begin = observation.begin,
                End = observation.endSpecified ? (DateTime?)observation.end : null,
                Result = string.Join("\n\r", observation.result.Select(r => r.description)),                
                Target = data.targets.FirstOrDefault(t => t.id == observation.target).ToTarget(data, observation.begin),                
            };
        }

        public static Session ToSession(this sessionType session, observations data)
        {
            var observations = data.observation.Where(i => i.session == session.id);
            var seeing = observations.Select(o => o.seeing).FirstOrDefault(s => !string.IsNullOrEmpty(s));
            var faintestStars = observations.Where(o => o.faintestStarSpecified).Select(o => o.faintestStar);
            var brightnesses = observations.Select(o => o.skyquality).Where(q => q != null);
            double? skyBrightness = brightnesses.Any() ? (double?)brightnesses.Select(q => (double)q.ToBrightness()).Min() : null;   
            double? faintestStar = faintestStars.Any() ? (double?)faintestStars.Min() : null;
            string observerId = observations.Select(o => o.observer).FirstOrDefault();
            var observer = data.observers.FirstOrDefault(i => i.id == observerId).ToObserver();            
            string siteId = observations.Select(o => o.site).FirstOrDefault();
            var site = data.sites.FirstOrDefault(i => i.id == siteId).ToSite();

            return new Session()
            {
                Id = session.id,
                Site = site,
                Observations = observations.Select(i => i.ToObservation(data)).ToList(),
                Seeing = seeing,
                FaintestStar = faintestStar,
                SkyBrightness = skyBrightness,
                Comments = session.comments,
                Weather = session.weather,
                Observer = observer,
            };
        }

        private static Site ToSite(this siteType site)
        {
            return new Site()
            {
                Id = site.id,
                Name = site.name,
                Latitude = site.latitude.ToAngle(),
                Longitude = site.longitude.ToAngle(),
                Elevation = site.elevationSpecified ? (double?)site.elevation : null,
                TimeZone = site.timezone,
                IAUCode = site.code
            };
        }

        private static CrdsEquatorial ToEquatorialCoordinates(this equPosType pos, DateTime dateTime)
        {
            double ra = pos.ra.ToAngle();
            double dec = pos.dec.ToAngle();

            double jd = new Date(dateTime).ToJulianDay();
            var eq0 = new CrdsEquatorial(ra, dec);
            
            switch (pos.frame?.equinox ?? referenceFrameTypeEquinox.EqOfDate)
            {
                case referenceFrameTypeEquinox.B1950:                    
                    eq0 = Precession.GetEquatorialCoordinates(eq0, Precession.ElementsFK5(Date.EPOCH_B1950, jd));
                    break;
                case referenceFrameTypeEquinox.J2000:
                    eq0 = Precession.GetEquatorialCoordinates(eq0, Precession.ElementsFK5(Date.EPOCH_J2000, jd));
                    break;
                default:
                    break;
            }

            // TODO: geocentric positions should be converted to topocentric

            return eq0;
        }

        private static double ToAngle(this angleType angle)
        {
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
                    value = Angle.ToDegrees(value);
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

        private static Target ToTarget(this observationTargetType target, observations data, DateTime dateTime)
        {
            Target result = null;

            // Single star
            if (target is starTargetType st)
            {
                var starTarget = new StarTarget();
                starTarget.ApparentMag = st.apparentMagSpecified ? (double?)st.apparentMag : null;
                starTarget.Classification = st.classification;
                result = starTarget;
            }
            // Multiple star (don't know why it's prefixed as "deepSky" in OAL)
            else if (target is deepSkyMS ms)
            {
                var msTarget = new MultipleStarTarget();
                msTarget.Components = ms.component.Select(id => data.targets.FirstOrDefault(t => t.id == id).ToTarget(data, dateTime)).Where(t => t != null).OfType<StarTarget>().ToList();
                result = msTarget;
            }
            // DeepSky object
            else if (target is deepSkyTargetType ds)
            {
                DeepSkyTarget deepSkyTarget = null;

                
                // Astarism 
                if (target is deepSkyAS a)
                {
                    var aTarget = new AsterismTarget();
                    aTarget.PosAngle = a.pa;
                    result = deepSkyTarget = aTarget;
                }
                // Cluster of Galaxies 
                else if (target is deepSkyCG cg)
                {
                    var cgTarget = new ClusterOfGalaxiesTarget();
                    cgTarget.Mag10 = cg.mag10Specified ? (double?)cg.mag10 : null;
                    result = deepSkyTarget = cgTarget;
                }
                // Dark nebula
                else if (target is deepSkyDN dn)
                {
                    var dnTarget = new DarkNebulaTarget();
                    dnTarget.PosAngle = dn.pa;
                    dnTarget.Opacity = dnTarget.Opacity;
                    result = deepSkyTarget = dnTarget;
                }

                // Double star
                else if (target is deepSkyDS dsd)
                {
                    var dsdTarget = new DoubleStarTarget();
                    dsdTarget.PosAngle = dsd.pa;
                    dsdTarget.Separation = dsd.separation?.ToAngle();
                    result = deepSkyTarget = dsdTarget;
                }

                // Galaxy
                else if (target is deepSkyGX gx)
                {
                    var galaxyTarget = new GalaxyTarget();
                    galaxyTarget.PosAngle = gx.pa;
                    galaxyTarget.HubbleType = gx.hubbleType;
                    result = deepSkyTarget = galaxyTarget;
                }
                // Galaxy nebula
                else if (target is deepSkyGN gn)
                {
                    var nebulaTarget = new GalaxyNebulaTarget();
                    nebulaTarget.PosAngle = gn.pa;
                    nebulaTarget.NebulaType = gn.nebulaType;
                    result = deepSkyTarget = nebulaTarget;
                }
                else
                {
                    result = new Target();
                }

                // DeepSkyTarget properties
                if (deepSkyTarget != null) 
                {
                    deepSkyTarget.SurfaceBrightness = ds.surfBr?.ToBrightness();
                    deepSkyTarget.VisualMag = ds.visMag;
                    deepSkyTarget.LargeDiameter = ds.largeDiameter?.ToAngle();
                    deepSkyTarget.SmallDiameter = ds.smallDiameter?.ToAngle();
                }
            }
            else if (target is CometTargetType)
            {
                result = new CometTarget();
            }
            else if (target is MinorPlanetTargetType)
            {
                result = new MinorPlanetTarget();
            }
            else if (target is MoonTargetType)
            {
                result = new MoonTarget();
            }
            else if (target is PlanetTargetType)
            {
                result = new PlanetTarget();
            }
            else if (target is SunTargetType)
            {
                result = new SunTarget();
            }
            else
            {
                result = new Target();
            }

            result.Id = target.id;
            result.Name = target.name;
            result.OtherNames = target.alias;
            result.Constellation = target.constellation;
            result.Position = target.position?.ToEquatorialCoordinates(dateTime);

            return result;
        }
    }
}
