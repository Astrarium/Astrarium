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

        public CelestialObjectInfo AddRow(string caption, object value)
        {
            InfoElements.Add(new InfoElementProperty()
            {
                Caption = caption,
                Value = value.ToString()
            });
            return this;
        }

        public CelestialObjectInfo AddRow(string caption, object value, IEphemFormatter formatter = null)
        {
            InfoElements.Add(new InfoElementProperty()
            {
                Caption = caption,
                Value = formatter.Format(value)
            });
            return this;
        }

        public CelestialObjectInfo AddRow(string caption, object value, IEphemFormatter formatter, double jd)
        {
            InfoElements.Add(new InfoElementPropertyLink()
            {
                Caption = caption,
                Value = formatter.Format(value),
                JulianDay = jd
            });
            return this;
        }

        public CelestialObjectInfo AddRow(string caption, object value, double jd)
        {
            InfoElements.Add(new InfoElementPropertyLink()
            {
                Caption = caption,
                Value = value.ToString(),
                JulianDay = jd
            });
            return this;
        }
    }

    public abstract class InfoElement
    {

    }

    public class InfoElementHeader : InfoElement
    {
        public string Text { get; set; }
    }

    public class InfoElementProperty : InfoElement
    {
        public string Caption { get; set; }
        public string Value { get; set; }
    }

    public class InfoElementPropertyLink : InfoElement
    {
        public string Caption { get; set; }
        public string Value { get; set; }
        public double JulianDay { get; set; }
    }
}