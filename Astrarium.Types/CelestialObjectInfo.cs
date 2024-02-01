using System;
using System.Collections.Generic;
using System.Linq;

namespace Astrarium.Types
{
    public abstract class CelestialObjectInfo
    {
        public string Title { get; protected set; }
        public string Subtitle { get; protected set; }
        public IList<InfoElement> InfoElements { get; } = new List<InfoElement>();
    }

    public class CelestialObjectInfo<T> : CelestialObjectInfo where T : CelestialObject
    {
        /// <summary>
        /// Celestial body to get information about
        /// </summary>
        public T Body { get; private set; }

        /// <summary>
        /// Context instance
        /// </summary>
        public SkyContext Context { get; private set; }

        /// <summary>
        /// Collection of body ephemeris for given instant
        /// </summary>
        private IEnumerable<Ephemeris> Ephemeris { get; set; }

        public CelestialObjectInfo(SkyContext context, T body, IEnumerable<Ephemeris> ephemeris)
        {
            Context = context;
            Body = body;
            Ephemeris = ephemeris;
        }

        public CelestialObjectInfo<T> SetTitle(string title)
        {
            Title = title;
            return this;
        }

        public CelestialObjectInfo<T> SetSubtitle(string subtitle)
        {
            Subtitle = subtitle;
            return this;
        }

        public CelestialObjectInfo<T> AddHeader(string text)
        {
            InfoElements.Add(new InfoElementHeader()
            {
                Text = text
            });
            return this;
        }

        public CelestialObjectInfo<T> AddRow(string key, object value)
        {
            return AddRow(key, value, null);
        }

        /// <summary>
        /// Adds row about ephemeris value with specified key
        /// </summary>
        /// <param name="key">Unique ephemeris key</param>
        /// <returns>CelestialObjectInfo instance</returns>
        public CelestialObjectInfo<T> AddRow(string key)
        {
            var ep = Ephemeris.FirstOrDefault(e => e.Key == key);

            if (ep != null)
            {
                InfoElements.Add(new InfoElementProperty()
                {
                    Caption = Text.Get($"{Body.GetType().Name}.{key}"),
                    Value = ep.Value,
                    Formatter = ep.Formatter
                });
            }
            else
            {
                throw new Exception($"Key `{key}` not found.");
            }
            
            return this;
        }

        public CelestialObjectInfo<T> AddRow(string key, object value, IEphemFormatter formatter)
        {
            InfoElements.Add(new InfoElementProperty()
            {
                Caption = Text.Get($"{Body.GetType().Name}.{key}"),
                Value = value,
                Formatter = formatter ?? Formatters.GetDefault(key)
            });
            return this;
        }

        public CelestialObjectInfo<T> AddRow(string text, Uri uri, string uriText)
        {
            InfoElements.Add(new InfoElementLink()
            {
                Caption = text,
                Uri = uri,
                UriText = uriText
            });
            return this;
        }
    }

    public abstract class InfoElement { }

    public class InfoElementHeader : InfoElement
    {
        public string Text { get; set; }
    }

    public class InfoElementProperty : InfoElement
    {
        public IEphemFormatter Formatter { get; set; }
        public string Caption { get; set; }
        public object Value { get; set; }
        public string StringValue { get { return Formatter.Format(Value); } }
    }

    public class InfoElementLink : InfoElement
    {
        public string Caption { get; set; }
        public Uri Uri { get; set; }
        public string UriText { get; set; }
    }
}