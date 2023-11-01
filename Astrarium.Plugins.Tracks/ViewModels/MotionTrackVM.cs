using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Tracks.ViewModels
{
    public class MotionTrackVM : ViewModelBase
    {
        private readonly ISky sky;
        private readonly TrackCalc trackCalc;

        public Command OkCommand { get; private set; }
        public Command CancelCommand { get; private set; }
        public ColorSchema ColorSchema { get; private set; }

        public MotionTrackVM(ISky sky, ISettings settings, TrackCalc trackCalc)
        {
            this.sky = sky;            
            this.trackCalc = trackCalc;
            
            ColorSchema = settings.Get<ColorSchema>("Schema");
            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);
        }

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
        public SkyColor TrackColor { get; set; } = new SkyColor(Color.DimGray);
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

            if (JulianDayFrom > JulianDayTo)
            {
                ViewManager.ShowMessageBox("$MotionTrackWindow.WarningTitle", "$MotionTrackWindow.DateWarningText", System.Windows.MessageBoxButton.OK);
                return;
            }

            if ((JulianDayTo - JulianDayFrom) / track.Step > 10000)
            {
                ViewManager.ShowMessageBox("$MotionTrackWindow.WarningTitle", "$MotionTrackWindow.StepDateRangeMismatchText", System.Windows.MessageBoxButton.OK);
                return;
            }

            AddOrEditTrack(track);
        }

        private async void AddOrEditTrack(Track track)
        {
            var categories = sky.GetEphemerisCategories(track.Body);
            if (!(categories.Contains("Equatorial.Alpha") && categories.Contains("Equatorial.Delta")))
            {
                throw new Exception($"Ephemeris provider for type {track.Body.GetType().Name} does not provide \"Equatorial.Alpha\" and \"Equatorial.Delta\" ephemeris.");
            }

            var tokenSource = new CancellationTokenSource();

            ViewManager.ShowProgress("$MotionTrackWindow.WaitTitle", "$MotionTrackWindow.WaitText", tokenSource);

            double trackStep = Math.Min(track.Step, track.To - track.From);

            var positions = await Task.Run(() => sky.GetEphemerides(track.Body, track.From, track.To + 1e-6, trackStep, new[] { "Equatorial.Alpha", "Equatorial.Delta" }, tokenSource.Token));

            if (!tokenSource.IsCancellationRequested)
            {
                tokenSource.Cancel();

                foreach (var eq in positions)
                {
                    track.Points.Add(new CrdsEquatorial(eq.GetValue<double>("Equatorial.Alpha"), eq.GetValue<double>("Equatorial.Delta")));
                }

                Track existing = trackCalc.Tracks.FirstOrDefault(t => t.Id == track.Id);
                if (existing != null)
                {
                    int index = trackCalc.Tracks.IndexOf(existing);
                    trackCalc.Tracks[index] = track;
                }
                else
                {
                    trackCalc.Tracks.Add(track);
                }
            }

            Close(true);
        }
    }
}
