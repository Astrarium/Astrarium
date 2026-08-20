using System.Collections.ObjectModel;

namespace Astrarium.Plugins.FOV
{
    public class Equipment
    {
        /// <summary>
        /// List of lens
        /// </summary>
        public ObservableCollection<Lens> Lens { get; set; } = new ObservableCollection<Lens>();

        /// <summary>
        /// List of binoculars
        /// </summary>
        public ObservableCollection<Binocular> Binoculars { get; set; } = new ObservableCollection<Binocular>();

        /// <summary>
        /// List of cameras
        /// </summary>
        public ObservableCollection<Camera> Cameras { get; set; } = new ObservableCollection<Camera>();

        /// <summary>
        /// List of telescopes
        /// </summary>
        public ObservableCollection<Telescope> Telescopes { get; set; } = new ObservableCollection<Telescope>();

        /// <summary>
        /// List of eyepieces
        /// </summary>
        public ObservableCollection<Eyepiece> Eyepieces { get; set; } = new ObservableCollection<Eyepiece>();
    }
}
