using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ADK.Demo
{
    public class AstroEvent
    {
        public string Text { get; private set; }
        public double JulianDay { get; private set; }

        public AstroEvent(double jd, string text)
        {
            JulianDay = jd;
            Text = text;
        }
    }

    public interface IInfoProvider<T> where T : CelestialObject
    {
        CelestialObjectInfo GetInfo(SkyContext context, T body);
    }

    public interface IAstroEventProvider
    {
        ICollection<AstroEvent> GetEvents(ICelestialObjectsProvider celestialObjectsProvider, double jdFrom, double jdTo);
    }

    public struct SearchResultItem 
    {
        public string Name { get; private set; }
        public CelestialObject Body { get; private set; }

        public SearchResultItem(CelestialObject body, string name)
        {
            Body = body;
            Name = name;
        }
    }

    public delegate ICollection<SearchResultItem> SearchDelegate(string searchString, int maxCount);

    public interface ISearchProvider
    {
        ICollection<SearchResultItem> Search(string searchString, int maxCount = 50);
    }

    public interface ISearchProvider<T> : ISearchProvider where T : CelestialObject { }

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
    }

    public class InfoElementPropertyLink : InfoElementProperty
    {
        public double JulianDay { get; set; }
    }
}