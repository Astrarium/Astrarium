using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows;

namespace Astrarium.Plugins.Tracks.ViewModels
{
    internal class TracksListVM : ViewModelBase
    {
        private readonly ISky sky;
        private readonly TrackCalc trackCalc;

        public Command OkCommand { get; private set; }
        public Command CancelCommand { get; private set; }
        public Command AddTrackCommand { get; private set; }
        public Command EditSelectedTrackCommand { get; private set; }
        public Command DeleteSelectedTrackCommand { get; private set; }
        public Command<TrackListItemVM> SelectTrackCommand { get; private set; }

        public ObservableCollection<TrackListItemVM> Tracks { get; private set; } = new ObservableCollection<TrackListItemVM>();
        public bool NoTracks => !Tracks.Any();

        private TrackListItemVM _SelectedTrack;
        public TrackListItemVM SelectedTrack
        {
            get
            {
                return _SelectedTrack;
            }
            set
            {
                _SelectedTrack = value;
                NotifyPropertyChanged(nameof(SelectedTrack));
            }
        }

        public TracksListVM(ISky sky, TrackCalc trackCalc)
        {
            this.sky = sky;            
            this.trackCalc = trackCalc;

            CancelCommand = new Command(Close);
            SelectTrackCommand = new Command<TrackListItemVM>(SelectTrack);
            EditSelectedTrackCommand = new Command(EditSelectedTrack);
            DeleteSelectedTrackCommand = new Command(DeleteSelectedTrack);
            AddTrackCommand = new Command(AddTrack);

            LoadList();
        }

        private void LoadList()
        {
            Guid? selectedTrackId = SelectedTrack?.Track.Id;

            var tracks = trackCalc.Tracks.Select(t => new TrackListItemVM()
            {
                Track = t,
                Body = t.Body.Names.First(),
                StartDate = JulianDayToString(t.From),
                EndDate = JulianDayToString(t.To),
                Color = t.Color                
            });

            Tracks.Clear();
            foreach (var track in tracks)
            {
                Tracks.Add(track);
            }

            if (selectedTrackId != null)
            {
                SelectedTrack = tracks.FirstOrDefault(t => t.Track.Id == selectedTrackId.Value);
            }

            NotifyPropertyChanged(nameof(NoTracks));
        }

        private void AddTrack()
        {
            EditTrack(new Track()
            {
                Id = Guid.NewGuid(),
                From = sky.Context.JulianDay,
                To = sky.Context.JulianDay + 30,
                LabelsStep = TimeSpan.FromDays(1),
                Color = Color.Gray
            });
        }

        private void SelectTrack(TrackListItemVM t)
        {
            EditTrack(t.Track);
        }

        private void EditSelectedTrack()
        {
            EditTrack(SelectedTrack.Track);
        }

        private void DeleteSelectedTrack()
        {
            if (ViewManager.ShowMessageBox("$TracksListWindow.WarningTitle", "$TracksListWindow.DeleteWarningText", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                trackCalc.Tracks.Remove(SelectedTrack.Track);
                sky.Calculate();
                LoadList();
            }
        }

        private void EditTrack(Track t)
        {
            var vm = ViewManager.CreateViewModel<MotionTrackVM>();
            vm.TrackId = t.Id;
            vm.LabelsStep = t.LabelsStep;
            vm.DrawLabels = t.DrawLabels;
            vm.SelectedBody = t.Body;
            vm.JulianDayFrom = t.From;
            vm.JulianDayTo = t.To;
            vm.UtcOffset = sky.Context.GeoLocation.UtcOffset;
            vm.TrackColor = t.Color;

            if (ViewManager.ShowDialog(vm) ?? false)
            {
                sky.Calculate();
                LoadList();
            }
        }

        private string JulianDayToString(double jd)
        {
            return Formatters.DateTime.Format(new Date(jd, sky.Context.GeoLocation.UtcOffset));
        }
    }

    internal class TrackListItemVM
    {
        public Track Track { get; set; }
        public string Body { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public Color Color { get; set; }
    }
}
