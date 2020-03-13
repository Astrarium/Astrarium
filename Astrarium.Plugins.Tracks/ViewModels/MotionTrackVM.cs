using Astrarium.Algorithms;
using Astrarium.Objects;
using Astrarium.Plugins.Tracks;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Tracks.ViewModels
{
    public class MotionTrackVM : ViewModelBase
    {
        public Command OkCommand { get; private set; }
        public Command CancelCommand { get; private set; }

        public MotionTrackVM(ISky sky, ITracksProvider tracksProvider)
        {
            this.sky = sky;
            this.tracksProvider = tracksProvider;

            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);
        }

        private readonly ISky sky;
        private readonly ITracksProvider tracksProvider;

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

        public Guid TrackId { get; set; }
        public double JulianDayFrom { get; set; }
        public double JulianDayTo { get; set; }
        public double UtcOffset { get; set; }
        public Color TrackColor { get; set; } = Color.DimGray;
        public bool DrawLabels { get; set; }
        public TimeSpan LabelsStep { get; set; } = TimeSpan.FromDays(1);

        public void Ok()
        {
            var track = new Track()
            {
                Id = TrackId,
                Body = SelectedBody,
                From = JulianDayFrom,
                To = JulianDayTo,
                LabelsStep = LabelsStep,
                Color = TrackColor,
                DrawLabels = DrawLabels
            };

            if (track.Body == null)
            {
                ViewManager.ShowMessageBox("Warning", "Please specify a celestial body.", System.Windows.MessageBoxButton.OK);
                return;
            }

            if (JulianDayFrom > JulianDayTo)
            {
                ViewManager.ShowMessageBox("Warning", "Wrong date range:\nend date should be greater than start date.", System.Windows.MessageBoxButton.OK);
                return;
            }

            if (LabelsStep.TotalDays < track.SmallestLabelsStep())
            {
                ViewManager.ShowMessageBox("Warning", "Wrong labels step value:\nit's too small to calculate the track.", System.Windows.MessageBoxButton.OK);
                return;
            }

            if ((JulianDayTo - JulianDayFrom) / track.Step > 10000)
            {
                ViewManager.ShowMessageBox("Warning", "Step value and date range mismatch:\nresulting track data is too large. Please increase the step or reduce the date range.", System.Windows.MessageBoxButton.OK);
                return;
            }

            AddOrEditTrack(track);
            Close(true);
        }

        private void AddOrEditTrack(Track track)
        {
            var categories = sky.GetEphemerisCategories(track.Body);
            if (!(categories.Contains("Equatorial.Alpha") && categories.Contains("Equatorial.Delta")))
            {
                throw new Exception($"Ephemeris provider for type {track.Body.GetType().Name} does not provide \"Equatorial.Alpha\" and \"Equatorial.Delta\" ephemeris.");
            }

            var positions = sky.GetEphemerides(track.Body, track.From, track.To, track.Step, new[] { "Equatorial.Alpha", "Equatorial.Delta" });
            foreach (var eq in positions)
            {
                track.Points.Add(new CelestialPoint() { Equatorial0 = new CrdsEquatorial((double)eq[0].Value, (double)eq[1].Value) });
            }

            int index = tracksProvider.Tracks.FindIndex(t => t.Id == track.Id);
            if (index > -1)
            {
                tracksProvider.Tracks[index] = track;
            }
            else
            {
                tracksProvider.Tracks.Add(track);
            }            
        }
    }
}
