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
        public static ICollection<Session> Import(this observations data)
        {
            var observationsWithoutSession = data.observation.Where(i => string.IsNullOrEmpty(i.session)).ToList();

            var newSessions = observationsWithoutSession.Select(o => new Session() {
                Id = Guid.NewGuid().ToString(),
                Observations = new List<Observation>() { o.Import(data) },
            }).ToArray();

            return data.sessions.Select(s => s.Import(data)).Concat(newSessions).ToArray();
        }

        public static Observer Import(this observerType observer)
        {
            return new Observer()
            {
                Id = observer.id,
                Name = observer.name,
                Surname = observer.surname
            };
        }


        public static Observation Import(this observationType observation, observations data)
        {
            return new Observation()
            {
                Id = observation.id,
                Begin = observation.begin,
                End = observation.endSpecified ? (DateTime?)observation.end : null,
                Result = string.Join("\n\r", observation.result.Select(r => r.description)),
                Observer = data.observers.FirstOrDefault(i => i.id == observation.observer).Import(),
                Target = data.targets.FirstOrDefault(t => t.id == observation.target).Import(observation.begin),                
            };
        }

        public static Session Import(this sessionType session, observations data)
        {
            return new Session()
            {
                Id = session.id,
                Observations = data.observation.Where(i => i.session == session.id).Select(i => i.Import(data)).ToList()
                //Observer = data.observers.FirstOrDefault(i => i.id == observation.id).Import()
            };
        }

        public static CrdsEquatorial Import(this equPosType pos, DateTime dateTime)
        {
            double ra = ToAngle(pos.ra.Value, pos.ra.unit);
            double dec = ToAngle(pos.dec.Value, pos.dec.unit);

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

        private static double ToAngle(double value, angleUnit unit)
        {
            switch (unit)
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

        public static Target Import(this observationTargetType target, DateTime dateTime)
        {
            Target result;


            if (target is deepSkyGX ds)
            {
                var r = new DeepSkyGalaxyTarget();
                r.LargeDiameter = (float)ds.largeDiameter?.Value;
                result = r;
            }
            else if (target is PlanetTargetType p)
            {
                var r = new PlanetTarget();
                result = r;
            }
            else
            {
                result = new Target();
            }


            result.Id = target.id;
            result.Name = target.name;
            result.OtherNames = target.alias;
            result.Constellation = target.constellation;
            result.Position = target.position?.Import(dateTime);

            return result;
        }
    }
}
