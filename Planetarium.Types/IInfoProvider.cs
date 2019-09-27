using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

    public class CelestialObjectInfo
    { 
        public string Title { get; private set; }
        public string Subtitle { get; private set; }

        public IList<InfoElement> InfoElements { get; } = new List<InfoElement>();

        public CelestialObjectInfo()
        {
            
        }

        public CelestialObjectInfo SetTitle(string title)
        {
            Title = title;
            return this;
        }

        public CelestialObjectInfo SetSubtitle(string subtitle)
        {
            Subtitle = subtitle;
            return this;
        }

        public CelestialObjectInfo AddHeader(string text)
        {
            InfoElements.Add(new InfoElementHeader()
            {
                Text = text
            });
            return this;
        }

        public CelestialObjectInfo AddRow(string key, object value)
        {
            InfoElements.Add(new InfoElementProperty()
            {
                Caption = key,
                Value = value
            });
            return this;
        }

        public CelestialObjectInfo AddRow(string key, object value, IEphemFormatter formatter)
        {
            InfoElements.Add(new InfoElementProperty()
            {
                Caption = key,
                Value = value,
                Formatter = formatter
            });
            return this;
        }

        public CelestialObjectInfo AddRow(string key, object value, double jd, IEphemFormatter formatter)
        {
            InfoElements.Add(new InfoElementPropertyLink()
            {
                Caption = key,
                Value = value,
                JulianDay = jd,
                Formatter = formatter
            });
            return this;
        }

        public CelestialObjectInfo AddRow(string key, object value, double jd)
        {
            InfoElements.Add(new InfoElementPropertyLink()
            {
                Caption = key,
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