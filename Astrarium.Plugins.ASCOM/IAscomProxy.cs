using Astrarium.Algorithms;
using System;
using System.ComponentModel;

namespace Astrarium.Plugins.ASCOM
{
    /// <summary>
    /// Defines proxy interface with methods to work with ASCOM platform.
    /// The implementation can be real ASCOM bridge to work with, in case if ASCOM platform is installer,
    /// or it can be a dummy class that does nothing, just mock in case if no ASCOM assemblies are loaded.
    /// </summary>
    public interface IAscomProxy : INotifyPropertyChanged
    {
        /// <summary>
        /// Flag indicating the ASCOM platform is installed
        /// </summary>
        bool IsAscomPlatformInstalled { get; }

        /// <summary>
        /// Polling period to update telescope state
        /// </summary>
        int PollingPeriod { get; set; }

        /// <summary>
        /// Returns true if telescope is connected and able to be operated
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Returns true if telescope is slewing, false otherwise
        /// </summary>
        bool IsSlewing { get; }

        /// <summary>
        /// Returns true if telescope is in at home position
        /// </summary>
        bool AtHome { get; }

        /// <summary>
        /// Returns true if telescope is parked
        /// </summary>
        bool AtPark { get; }

        /// <summary>
        /// Returns true if tracking is on
        /// </summary>
        bool IsTracking { get; }

        /// <summary>
        /// Gets current telescope position
        /// </summary>
        CrdsEquatorial Position { get; }

        /// <summary>
        /// Gets short telescope name
        /// </summary>
        string TelescopeName { get; }

        /// <summary>
        /// Aborts telescope slewing
        /// </summary>
        void AbortSlewing();

        /// <summary>
        /// Sets telescope at home position
        /// </summary>
        void FindHome();

        /// <summary>
        /// Parks the telescope
        /// </summary>
        void Park();

        /// <summary>
        /// Unparks the telescope
        /// </summary>
        void Unpark();

        /// <summary>
        /// Sets tracking
        /// </summary>
        void SwitchTracking();

        /// <summary>
        /// Syncs the telescope (sets specified coordinates to appropriate target values without slewing)
        /// </summary>
        /// <param name="eq">Target equatorial coordinates</param>
        void Sync(CrdsEquatorial eq);

        /// <summary>
        /// Shows setup dialog for the telescope.
        /// </summary>
        void ShowSetupDialog();

        /// <summary>
        /// Shows dialog to choose telescope and, if choosen, connects to the selected telescope.
        /// </summary>
        string Connect(string telescopeId);

        /// <summary>
        /// Disconnects the selected telescope.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Sets observers location for the telescope.
        /// </summary>
        /// <param name="geo">Geographical coordinates to be set as telescope location.</param>
        void SetLocation(CrdsGeographical geo);

        /// <summary>
        /// Sets UTC date and time for the telescope.
        /// </summary>
        /// <param name="utc">Date & Time in UTC</param>
        void SetDateTime(DateTime utc);

        /// <summary>
        /// Sends "Slew" command to the telescope.
        /// </summary>
        /// <param name="eq">Target telescope position, in equatorial coordinates.</param>
        void Slew(CrdsEquatorial eq);

        /// <summary>
        /// Gets info about telescope and installed ASCOM platform
        /// </summary>
        AscomInfo Info { get; }

        /// <summary>
        /// Raised when a message should be shown for the user
        /// </summary>
        event Action<string> OnMessageShow;
    }

    /// <summary>
    /// Defines set of properties representing info about telescope and installed ASCOM platform
    /// </summary>
    public struct AscomInfo
    {
        /// <summary>
        /// Name of the telescope
        /// </summary>
        public string TelescopeName { get; internal set; }

        /// <summary>
        /// Short description of the telescope
        /// </summary>
        public string TelescopeDescription { get; internal set; }
        
        /// <summary>
        /// ASCOM driver description
        /// </summary>
        public string DriverDescription { get; internal set; }

        /// <summary>
        /// ASCOM driver version info
        /// </summary>
        public string DriverVersion { get; internal set; }

        /// <summary>
        /// ASCOM interface version info
        /// </summary>
        public string InterfaceVersion { get; internal set; }

        /// <summary>
        /// Flag indicating the telescope can find home position
        /// </summary>
        public bool CanFindHome { get; internal set; }

        /// <summary>
        /// Flag indicating the telescope can slew
        /// </summary>
        public bool CanSlew { get; internal set; }

        /// <summary>
        /// Flag indicating the telescope can set tracking
        /// </summary>
        public bool CanSetTracking { get; internal set; }

        /// <summary>
        /// Flag indicating the telescope can sync
        /// </summary>
        public bool CanSync { get; internal set; }

        /// <summary>
        /// Flag indicating the telescope can park
        /// </summary>
        public bool CanPark { get; internal set; }

        /// <summary>
        /// Flag indicating the telescope can unpark
        /// </summary>
        public bool CanUnpark { get; internal set; }
    }
}