using ADK.Demo.Calculators;
using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADK.Demo.UI
{
    public partial class FormTrackSettings : Form
    {
        private Sky sky;
        private ITracksProvider tracksProvider;

        private Track _Track;
        public Track Track
        {
            get { return _Track;  }
            set
            {
                _Track = value;

                dtFrom.JulianDay = _Track.From;
                dtFrom.UtcOffset = sky.Context.GeoLocation.UtcOffset;
                dtTo.JulianDay = _Track.To;
                dtTo.UtcOffset = sky.Context.GeoLocation.UtcOffset;
                selTimeInterval.TimeInterval = _Track.LabelsStep;
                selCelestialBody.Searcher = sky;
                selCelestialBody.Filter = (b) => b is IMovingObject;
                selCelestialBody.SelectedObject = _Track.Body;
            }
        }

        public FormTrackSettings(Sky sky, ITracksProvider tracksProvider)
        {
            InitializeComponent();

            this.sky = sky;
            this.tracksProvider = tracksProvider;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            AddTrack(new Track()
            {
                Body = selCelestialBody.SelectedObject,
                From = dtFrom.JulianDay,
                To = dtTo.JulianDay,
                LabelsStep = selTimeInterval.TimeInterval
            });


            DialogResult = DialogResult.OK;
        }

        private void AddTrack(Track track)
        {
            if (!(track.Body is IMovingObject))
                throw new Exception($"The '{track.Body.GetType()}' class should implement '{nameof(IMovingObject)}' interface.");

            tracksProvider.Tracks.Add(track);

            var positions = sky.GetEphemerides(track.Body, track.From, track.To, track.Step, new[] { "Equatorial.Alpha", "Equatorial.Delta" });
            foreach (var eq in positions)
            {
                track.Points.Add(new CelestialPoint() { Equatorial0 = new CrdsEquatorial((double)eq[0].Value, (double)eq[1].Value) });
            }

            sky.Calculate();
        }

        public CelestialObject SelectedObject
        {
            get
            {
                return selCelestialBody.SelectedObject;
            }
        }

        /// <summary>
        /// Gets starting Julian Day for creating track
        /// </summary>
        public double JulianDayFrom
        {
            get
            {
                return dtFrom.JulianDay;
            }
        }

        /// <summary>
        /// Gets finishing Julian Day for creating track
        /// </summary>
        public double JulianDayTo
        {
            get
            {
                return dtTo.JulianDay;
            }
        }

        /// <summary>
        /// Gets step, in days, for creating track
        /// </summary>
        public double Step
        {
            get
            {
                return selTimeInterval.TimeInterval.TotalDays;
            }
        }

    }
}
