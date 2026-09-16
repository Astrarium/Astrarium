using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Astrarium.Plugins.RestAPI
{
    [Singleton(typeof(IRestAPIController))]
    public class RestAPIHandler : IRestAPIController
    {
        private readonly ISky sky;
        private readonly ISkyMap map;

        public RestAPIHandler(ISky sky, ISkyMap map)
        {
            this.sky = sky;
            this.map = map;
        }

        Task<ICollection<CelestialObject>> IRestAPIController.SearchAsync(string searchString, int limit, IEnumerable<string> types)
        {
            return Task.Run<ICollection<CelestialObject>>(() =>
            {
                var bodies = sky.Search(searchString, (Types.CelestialObject body) => {
                    return types.Any() ? types.Contains(body.Type) || types.Any(x => body.Type.StartsWith(x + ".")) : true;
                }, limit);

                return bodies.Select(x => new CelestialObject()
                {
                    Type = x.Type,
                    Name = x.CommonName,
                    Names = x.Names.ToList(),
                    Coordinates = ToDto(x.Equatorial)
                }).ToArray();
            });
        }

        Task<Coordinates> IRestAPIController.CenterGETAsync()
        {
            return Task.FromResult(ToDto(map.Projection.CenterEquatorial));
        }

        Task IRestAPIController.CenterPOSTAsync(Coordinates eq)
        {
            if (eq.Alpha < 0 || eq.Alpha >= 360)
            {
                throw new ParameterValueIsOutOfRangeError("Right Ascension (alpha) should be in range from 0 to 360 degrees.");
            }

            if (eq.Delta < -90 || eq.Delta > 90)
            {
                throw new ParameterValueIsOutOfRangeError("Declination (delta) should be in range from -90 to +90 degrees.");
            }

            return Task.Run(() => map.GoToPoint(FromDto(eq), TimeSpan.Zero));
        }

        Task<GeoLocation> IRestAPIController.LocationGETAsync()
        {
            return Task.FromResult(ToDto(sky.Context.GeoLocation));
        }

        Task IRestAPIController.LocationPOSTAsync(GeoLocation geo)
        {
            if (geo.Longitude < -180 || geo.Longitude > 180)
            {
                throw new ParameterValueIsOutOfRangeError("Longitude range from -180 to +180 degrees.");
            }

            if (geo.Latitude < -90 || geo.Latitude > 90)
            {
                throw new ParameterValueIsOutOfRangeError("Latitude should be in range from -90 to +90 degrees.");
            }

            if (geo.Elevation < -100 || geo.Elevation > 9000)
            {
                throw new ParameterValueIsOutOfRangeError("Elevation should be in range from -100 to +9000 meters.");
            }

            if (geo.Offset < -12 || geo.Offset > 14)
            {
                throw new ParameterValueIsOutOfRangeError("Time zone offset should be in range from -12 to +14 hours.");
            }

            return Task.Run(() => sky.SetLocation(FromDto(geo)));
        }

        Task<double> IRestAPIController.DateGETAsync()
        {
            return Task.FromResult(sky.Context.JulianDay);
        }

        Task IRestAPIController.DatePOSTAsync(double jd)
        {
            CheckDate(jd, "Date");

            return Task.Run(() => sky.SetDate(jd));
        }

        Task<double> IRestAPIController.FovGETAsync()
        {
            return Task.FromResult(map.Projection.Fov);
        }

        Task IRestAPIController.FovPOSTAsync(double fov)
        {
            if (fov < map.Projection.MinFov)
            {
                throw new ParameterValueIsOutOfRangeError($"Specified FOV value is too small. Minimal alloved value is {map.Projection.MinFov}.");
            }
            if (fov > map.Projection.MaxFov)
            {
                throw new ParameterValueIsOutOfRangeError($"Specified FOV value is too large. Maximal alloved value is {map.Projection.MaxFov}.");
            }

            return Task.Run(() => 
            {
                map.Projection.Fov = fov;
                map.Invalidate();
            });
        }

        Task<CelestialObjectInfo> IRestAPIController.InfoAsync(string searchString, string type)
        {
            return Task.Run(() => 
            {
                var body = SearchCelestialObject(searchString, type);

                var categories = sky.GetEphemerisCategories(body);
                var ephem = sky.GetEphemerides(body, sky.Context, categories);

                return new CelestialObjectInfo()
                {
                    Name = body.CommonName,
                    Type = body.Type,
                    Names = ephem.CelestialObject.Names.ToList(),
                    Values = ephem.Select(x => new Ephemeris()
                    {
                        Key = x.Key,
                        Value = x.Value,
                    }).ToList()
                };
            });
        }

        Task<ICollection<Ephemerides>> IRestAPIController.EphemAsync(string searchString, double from, double to, string type, double? step = null, IEnumerable<string> categories = null)
        {
            return Task.Run<ICollection<Ephemerides>>(() =>
            {
                var body = SearchCelestialObject(searchString, type);

                if (categories == null || !categories.Any())
                {
                    categories = sky.GetEphemerisCategories(body);
                } 
                else
                {
                    var unknown = categories.Except(sky.GetEphemerisCategories(body));
                    if (unknown.Any())
                    {
                        throw new UnknownParameterValueError($"Unknown ephemeris categories: {string.Join(",", unknown)}.");
                    }
                }

                CheckDateRange(from, to);

                if (step < TimeSpan.FromSeconds(1).TotalDays)
                {
                    throw new ParameterValueIsOutOfRangeError("Step value is too small.");
                }

                if ((to - from) / step > 10000)
                {
                    throw new ParameterValueIsOutOfRangeError("Too many time instants to calculate.");
                }

                var ephem = sky.GetEphemerides(body, from, to, step ?? 1, categories);
                int index = 0;
                return ephem.Select(x => new Ephemerides()
                {
                    Date = from + (index++) * (step ?? 1),
                    Values = x.Select(e => new Ephemeris()
                    {
                        Key = e.Key,
                        Value = e.Value,
                    }).ToList(),
                }).ToList();
            });
        }

        Task<ICollection<AstronomicalEvent>> IRestAPIController.EventsAsync(double from, double to, IEnumerable<string> eventTypes = null)
        {
            return Task.Run<ICollection<AstronomicalEvent>>(() =>
            {
                if (eventTypes == null || !eventTypes.Any())
                {
                    eventTypes = sky.GetEventsCategories().Where(x => !x.StartsWith("Daily."));
                }
                else
                {
                    var unknown = eventTypes.Except(sky.GetEventsCategories());
                    if (unknown.Any())
                    {
                        throw new UnknownParameterValueError($"Unknown event types: {string.Join(",", unknown)}.");
                    }
                }

                CheckDateRange(from, to);

                var events = sky.GetEvents(from, to, eventTypes);
                return events.Select(x => new AstronomicalEvent()
                {
                    Date = x.JulianDay,
                    NoExactTime = x.NoExactTime,
                    Event = x.Text,
                }).ToList();
            });
        }

        #region Helpers

        private Coordinates ToDto(CrdsEquatorial eq)
        {
            return new Coordinates()
            {
                Alpha = eq.Alpha,
                Delta = eq.Delta,
            };
        }

        private CrdsEquatorial FromDto(Coordinates eq)
        {
            if (eq == null)
            {
                throw new UnknownParameterValueError("Failed to parse coordinates.");
            }
            return new CrdsEquatorial(eq.Alpha, eq.Delta);
        }

        private GeoLocation ToDto(CrdsGeographical geo)
        {
            return new GeoLocation()
            {
                Name = geo.Name,
                Elevation = geo.Elevation,
                Latitude = geo.Latitude,
                Longitude = -geo.Longitude,
                Offset = geo.UtcOffset
            };
        }

        private CrdsGeographical FromDto(GeoLocation geo)
        {
            if (geo == null)
            {
                throw new UnknownParameterValueError("Failed to parse location.");
            }

            return new CrdsGeographical()
            {
                Name = geo.Name,
                Elevation = geo.Elevation,
                Latitude = geo.Latitude,
                Longitude = -geo.Longitude,
                UtcOffset = geo.Offset
            };
        }

        private Types.CelestialObject SearchCelestialObject(string searchString, string type)
        {
            var body = sky.Search(searchString, (Types.CelestialObject b) => {
                return type != null ? type == b.Type : true;
            }).OrderBy(b => b.CommonName == searchString ? 0 : b.CommonName.Length - searchString.Length).FirstOrDefault();

            if (body == null)
                throw new ObjectNotFoundError(searchString, type);

            return body;
        }

        private void CheckDateRange(double from, double to)
        {
            CheckDate(from, "Start date");
            CheckDate(to, "End date");

            if (from >= to)
            {
                throw new ParameterValueIsOutOfRangeError("Invalid time range: start date is after or equal end date.");
            }
        }

        private void CheckDate(double jd, string name)
        {
            if (jd < Date.JulianDay(1, 1, 1))
            {
                throw new ParameterValueIsOutOfRangeError($"{name} is too far in past.");
            }

            if (jd > Date.JulianDay(9999, 12, 31))
            {
                throw new ParameterValueIsOutOfRangeError($"{name} is too far in future.");
            }
        }

        #endregion Helpers
    }
}
