using Astrarium.Plugins.ObservationsLog.Database;
using Astrarium.Plugins.ObservationsLog.Types;
using Astrarium.Types;
using Astrarium.Types.Themes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ObservationsLog.ViewModels
{
    public class ObservationsLogVM : ViewModelBase
    {
        public ObservableCollection<SessionsListItem> SessionsList
        {
            get { return GetValue(nameof(SessionsList), new ObservableCollection<SessionsListItem>()); }
            set { SetValue(nameof(SessionsList), value); }
        }

        public SessionsListItem Session
        {
            get { return GetValue<SessionsListItem>(nameof(Session)); }
            set { SetValue(nameof(Session), value); }
        }

        public ObservationsLogVM()
        {
            SessionsList = new ObservableCollection<SessionsListItem>(Storage.GetSessions(s => true).Select(s => new SessionsListItem(s)).OrderByDescending(s => s.Begin));
            Session = SessionsList.FirstOrDefault();
        }
    }

    public class SessionsListItem
    {
        public DateTime Begin { get; set; }
        public string Title { get; set; }        
        public string Weather { get; set; }
        public string Comments { get; set; }
        public string Seeing { get; set; }
        public int ObservationsCount { get => Observations.Count; }

        public ObservableCollection<ObservationsListItem> Observations { get; set; } = new ObservableCollection<ObservationsListItem>();

        public SessionsListItem(Session session)
        {
            var firstObservation = session.Observations.OrderBy(o => o.Begin).First();
            var utcBegin = firstObservation.Begin.ToUniversalTime();
            
            Begin = utcBegin;
            Title = utcBegin.AddHours(session.Site.TimeZone).ToString();
            Weather = session.Weather;
            Comments = session.Comments;
            Seeing = session.Seeing;
            Observations = new ObservableCollection<ObservationsListItem>(session.Observations.Select(o => new ObservationsListItem(session, o))); 
        }
    }

    public class ObservationsListItem
    {
        public bool IsExpanded { get; set; } = true;
        public string Title { get; set; }
        public string Result { get; set; }
        public CelestialObject Body { get; set; }

        public TimeSpan? Begin { get; set; }
        public TimeSpan? End { get; set; }

        public ObservationsListItem(Session session, Observation observation)
        {
            var utcBegin = observation.Begin.ToUniversalTime();
            var utcEnd = observation.End?.ToUniversalTime();

            var timeBegin = utcBegin.AddHours(session.Site.TimeZone).TimeOfDay;
            var timeEnd = utcEnd?.AddHours(session.Site.TimeZone).TimeOfDay;

            Begin = timeBegin;
            End = timeEnd;
            Title = timeBegin.ToString("hh\\:mm") + " - " + observation.Target.Name;
            Result = observation.Result;
            Body = new TargetBody(new string[] { observation.Target.Name }); 
        }
    }

    public class TargetBody : CelestialObject
    {
        public TargetBody(string[] names)
        {
            this.names = names;
        }

        private string[] names;
        public override string[] Names => names;
        public override string[] DisplaySettingNames => new string[] { };
    }
}
