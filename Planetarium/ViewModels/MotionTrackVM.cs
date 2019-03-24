using ADK;
using Planetarium.Calculators;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.ViewModels
{
    public class MotionTrackVM : ViewModelBase
    {
        public Command OkCommand { get; private set; }
        public Command CancelCommand { get; private set; }

        public MotionTrackVM(IViewManager viewManager, ISearcher searcher, IEphemerisProvider ephemerisProvider, ITracksProvider tracksProvider)
        {
            ViewManager = viewManager;
            Searcher = searcher;
            EphemerisProvider = ephemerisProvider;
            TracksProvider = tracksProvider;

            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);
        }

        public IViewManager ViewManager { get; private set; }
        public ISearcher Searcher { get; private set; }
        public ITracksProvider TracksProvider { get; private set; }
        public IEphemerisProvider EphemerisProvider { get; private set; }

        private CelestialObject _SelectedBody;
        public CelestialObject SelectedBody
        {
            get
            {
                return _SelectedBody;
            }
            set
            {
                _SelectedBody = value;
                NotifyPropertyChanged(nameof(SelectedBody));
            }
        }

        public Func<CelestialObject, bool> Filter { get; } = body => body is IMovingObject;

        public double JulianDayFrom { get; set; }
        public double JulianDayTo { get; set; }
        public double UtcOffset { get; set; }

        public void Ok()
        {
            AddTrack(new Track()
            {
                Body = SelectedBody,
                From = JulianDayFrom,
                To = JulianDayTo,
                LabelsStep = TimeSpan.FromDays(1)
            });
            Close(true);
        }

        private void AddTrack(Track track)
        {
            if (!(track.Body is IMovingObject))
                throw new Exception($"The '{track.Body.GetType()}' class should implement '{nameof(IMovingObject)}' interface.");

            TracksProvider.Tracks.Add(track);

            var positions = EphemerisProvider.GetEphemerides(track.Body, track.From, track.To, track.Step, new[] { "Equatorial.Alpha", "Equatorial.Delta" });
            foreach (var eq in positions)
            {
                track.Points.Add(new CelestialPoint() { Equatorial0 = new CrdsEquatorial((double)eq[0].Value, (double)eq[1].Value) });
            }
        }
    }
}
