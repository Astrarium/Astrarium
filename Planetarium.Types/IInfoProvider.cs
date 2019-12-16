using Planetarium.Objects;
using Planetarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Planetarium.Types
{
    public class AstroEvent
    {
        public string Text { get; private set; }
        public double JulianDay { get; private set; }
        public bool NoExactTime { get; private set; }

        public AstroEvent(double jd, string text, bool noExactTime = false)
        {
            JulianDay = jd;
            Text = text;
            NoExactTime = noExactTime;
        }

        public override string ToString()
        {
            return $"JD={JulianDay}: {Text}";
        }
    }

    /*
    public class InfoBuilder<T> where T : CelestialObject
    {
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public IList<InfoElement> InfoElements { get; } = new List<InfoElement>();

        public CelestialObject CelestialBody { get; private set; }
        public SkyContext Context { get; private set; }

        public InfoBuilder(SkyContext context, CelestialObject body)
        {
            Context = context;
            CelestialBody = body;
        }

        public InfoBuilder<T> AddRow(string key)
        {
            InfoElements.Add(new InfoElementProperty()
            {
                Caption = key,
                NeedCalculate = true
            });
            return this;
        }
    }
    */

    public abstract class CelestialObjectInfo
    {
        public string Title { get; protected set; }
        public string Subtitle { get; protected set; }
        public IList<InfoElement> InfoElements { get; } = new List<InfoElement>();
    }

    public class CelestialObjectInfo<T> : CelestialObjectInfo where T : CelestialObject
    {
        public T Body { get; private set; }
        public SkyContext Context { get; private set; }

        private List<Ephemeris> Ephemeris { get; set; }

        public CelestialObjectInfo(SkyContext context, T body, List<Ephemeris> ephemeris)
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

        public CelestialObjectInfo<T> AddRow(string text, object value, IEphemFormatter formatter)
        {
            InfoElements.Add(new InfoElementProperty()
            {
                Caption = text,
                Value = value,
                Formatter = formatter
            });
            return this;
        }
     
        public CelestialObjectInfo<T> AddRow(string text, object value, double jd)
        {
            InfoElements.Add(new InfoElementPropertyLink()
            {
                Caption = text,
                Value = value,
                JulianDay = jd
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

    public class InfoElementPropertyLink : InfoElementProperty
    {
        public double JulianDay { get; set; }
    }
}