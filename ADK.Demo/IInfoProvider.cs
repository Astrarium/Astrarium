using ADK.Demo.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ADK.Demo
{
    public interface IInfoProvider<T> where T : CelestialObject
    {
        CelestialObjectInfo GetInfo(SkyContext context, T body);
    }

    public class CelestialObjectInfo
    { 
        public string Title { get; private set; }

        public IList<InfoElement> InfoElements { get; } = new List<InfoElement>();

        public CelestialObjectInfo()
        {
            
        }

        public CelestialObjectInfo SetTitle(string title)
        {
            Title = title;
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

        public CelestialObjectInfo AddRow(string key, object value, IEphemFormatter formmater)
        {
            InfoElements.Add(new InfoElementProperty()
            {
                Caption = key,
                Value = value,
                Formatter = formmater
            });
            return this;
        }

        public CelestialObjectInfo AddRow(string key, object value, IEphemFormatter formatter, double jd)
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
    }

    public class InfoElementPropertyLink : InfoElementProperty
    {
        public double JulianDay { get; set; }
    }
}