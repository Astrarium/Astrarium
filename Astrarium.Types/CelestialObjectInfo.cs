using Astrarium.Algorithms;
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
        public abstract string ObjectType { get; }
        public abstract string ObjectCommonName { get; }
        public abstract CelestialObject GetBody();
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

        public override CelestialObject GetBody() => Body;

        /// <summary>
        /// Gets object type
        /// </summary>
        public override string ObjectType => Body.Type;

        /// <summary>
        /// Gets object common name
        /// </summary>
        public override string ObjectCommonName => Body.CommonName;

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

        public void Clear()
        {
            InfoElements.Clear();
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
                var value = ep.Value;
                AddRow(key, value, ep.Formatter);
            }
            else
            {
                throw new Exception($"Key `{key}` not found.");
            }

            return this;
        }

        public CelestialObjectInfo<T> AddRow(string key, object value, IEphemFormatter formatter)
        {
            formatter = formatter ?? Formatters.GetDefault(key);
            InfoElementPropertyBase ie;

            if (formatter is ITimeInstantFormatter f && f.HasTimeInstant(value))
            {
                ie = new InfoElementDateProperty();
            }
            else 
            {
                ie = new InfoElementProperty();
            }

            ie.Caption = Text.Get($"{Body.GetType().Name}.{key}");
            ie.Value = value;
            ie.Formatter = formatter;

            InfoElements.Add(ie);

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

        public CelestialObjectInfo<T> AddRow(string text, Action command, string commandText)
        {
            InfoElements.Add(new InfoElementCommand()
            {
                Caption = text,
                Command = command,
                Text = commandText
            });
            return this;
        }
    }

    public abstract class InfoElement { }

    public abstract class InfoElementPropertyBase : InfoElement
    {
        public IEphemFormatter Formatter { get; set; }
        public string Caption { get; set; }
        public object Value { get; set; }
        public string StringValue { get { return Formatter.Format(Value); } }
    }

    public class InfoElementHeader : InfoElement
    {
        public string Text { get; set; }
    }

    public class InfoElementProperty : InfoElementPropertyBase { }
    public class InfoElementDateProperty : InfoElementPropertyBase { }

    public class InfoElementLink : InfoElement
    {
        public string Caption { get; set; }
        public Uri Uri { get; set; }
        public string UriText { get; set; }
    }

    public class InfoElementCommand : InfoElement
    {
        public string Caption { get; set; }
        public Action Command { get; set; }
        public string Text { get; set; }
    }
}