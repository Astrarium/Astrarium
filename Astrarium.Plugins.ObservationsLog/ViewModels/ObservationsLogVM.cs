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
            SessionsList = new ObservableCollection<SessionsListItem>(Storage.GetSessions(s => true).Select(s => new SessionsListItem(s)));
            Session = SessionsList.FirstOrDefault();
        }
    }

    public class SessionsListItem
    {
        public string DateTime { get; set; }
        public string Title { get; set; }        
        public string Weather { get; set; }
        public string Comments { get; set; }
        public string Seeing { get; set; }

        public ObservableCollection<ObservationsListItem> Observations { get; set; } = new ObservableCollection<ObservationsListItem>();

        public SessionsListItem(Session session)
        {
            DateTime = session.Observations.First().Begin.ToString();
            Title = session.Observations.First().Begin.ToString();
            Weather = session.Weather;
            Comments = session.Comments;
            Seeing = session.Seeing;
            Observations = new ObservableCollection<ObservationsListItem>(session.Observations.Select(o => new ObservationsListItem(o)));        
        }
    }

    public class ObservationsListItem
    {
        public bool IsExpanded { get; set; } = true;
        public string Title { get; set; }
        public string Result { get; set; }
        public CelestialObject Body { get; set; }

        public ObservationsListItem(Observation observation)
        {
            Title = observation.Begin.TimeOfDay.ToString("hh\\:mm") + " - " + observation.Target.Name;
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
