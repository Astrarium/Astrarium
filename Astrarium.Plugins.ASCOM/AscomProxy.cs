using ASCOM.DriverAccess;
using ASCOM.DeviceInterface;
using Astrarium.Algorithms;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Astrarium.Types;

namespace Astrarium.Plugins.ASCOM
{
    public class AscomProxy : IAscomProxy, INotifyPropertyChanged
    {
        private Telescope telescope = null;
        private Thread watcherThread = null;
        private ManualResetEvent watcherResetEvent = new ManualResetEvent(false);
        private object locker = new object();
        private bool isDisposed = false;

        /// <inheritdoc/>
        public event Action<string> OnMessageShow;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc/>
        public CrdsEquatorial Position { get; } = new CrdsEquatorial();

        /// <summary>
        /// Initializes new instance of AscomProxy
        /// </summary>
        public AscomProxy()
        {
            watcherThread = new Thread(Polling)
            {
                IsBackground = true,
                Name = "ASCOM Watcher Thread"
            };

            watcherResetEvent.Reset();
            watcherThread.Start();
        }

        /// <summary>
        /// Free resources
        /// </summary>
        ~AscomProxy()
        {
            try
            {
                DoDisconnect();
                isDisposed = true;
                watcherResetEvent.Set();
            }
            catch { }
        }

        /// <inheritdoc/>
        public bool IsAscomPlatformInstalled => true;

        /// <inheritdoc/>
        public int PollingPeriod { get; set; }

        /// <inheritdoc/>
        public string Connect(string telescopeId)
        {
            try
            {
                lock (locker)
                {
                    string id = Telescope.Choose(telescopeId);
                    if (!string.IsNullOrEmpty(id))
                    {
                        DoDisconnect();
                        telescope = new Telescope(id);
                        telescope.Connected = true;
                        NotifyPropertyChanged(nameof(IsConnected));
                        RaiseOnMessageShow(Text.Get("Ascom.Messages.Connected", ("telescopeName", telescope.Name)));
                        watcherResetEvent.Set();
                    }
                    return id;
                }
            }
            catch (Exception ex)
            {
                RaiseOnMessageShow("$Ascom.Messages.UnableChoose");
                Log.Error($"Unable to choose telescope: {ex}");
                return null;
            }
        }

        /// <inheritdoc/>
        public void AbortSlewing()
        {
            try
            {
                lock (locker)
                {
                    if (telescope != null && telescope.Slewing && !telescope.AtPark)
                    {
                        telescope.AbortSlew();
                        RaiseOnMessageShow("$Ascom.Messages.SlewingAborted");
                        NotifyPropertyChanged(nameof(IsSlewing));
                    }
                }
            }
            catch (Exception ex)
            {               
                RaiseOnMessageShow("$Ascom.Messages.UnableAbortSlew");
                Log.Error($"Unable to abort slewing: {ex}");
            }
        }

        /// <inheritdoc/>
        public void FindHome() 
        {
            try
            {
                lock (locker)
                {
                    if (telescope != null && !telescope.AtHome && telescope.CanFindHome)
                    {
                        UnparkIfPossible();
                        Task.Run(() => telescope.FindHome());
                        NotifyPropertyChanged(nameof(IsSlewing));
                    }
                }
            }
            catch (Exception ex)
            {
                RaiseOnMessageShow("$Ascom.Messages.UnableFindHome");
                Log.Error($"Unable to find home: {ex}");
            }
        }

        /// <inheritdoc/>
        public void Park()
        {
            try
            {
                lock (locker)
                {
                    if (telescope != null && !telescope.AtPark && telescope.CanSetPark)
                    {
                        Task.Run(() => telescope.Park());
                        NotifyPropertyChanged(nameof(IsSlewing));
                    }                   
                }
            }
            catch (Exception ex)
            {
                RaiseOnMessageShow("$Ascom.Messages.UnablePark");
                Log.Error($"Unable to park telescope: {ex}");
            }
        }

        /// <inheritdoc/>
        public void Unpark()
        {
            try
            {
                lock (locker)
                {
                    UnparkIfPossible();
                    NotifyPropertyChanged(nameof(AtPark));
                }
            }
            catch (Exception ex)
            {
                RaiseOnMessageShow("$Ascom.Messages.UnableUnpark");
                Log.Error($"Unable to unpark telescope: {ex}");
            }
        }

        /// <inheritdoc/>
        public void SwitchTracking()
        {
            try
            {
                lock (locker)
                {
                    if (telescope.CanSetTracking)
                    {
                        telescope.Tracking = !telescope.Tracking;
                    }
                    if (!telescope.CanSetTracking)
                    {
                        RaiseOnMessageShow("$Ascom.Messages.UnableSwitchTracking");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to switch tracking: {ex}");
            }
        }

        /// <inheritdoc/>
        public void Disconnect()
        {
            try
            {
                lock (locker)
                {
                    DoDisconnect();
                    RaiseOnMessageShow("$Ascom.Messages.Disconnected");
                }
            }
            catch (Exception ex)
            {
                RaiseOnMessageShow("$Ascom.Messages.UnableDisconnect");
                Log.Error($"Unable to disconnect: {ex}");
            }
        }

        /// <inheritdoc/>
        public void SetDateTime(DateTime utc)
        {
            try
            {
                lock (locker)
                {
                    if (telescope != null)
                    {
                        telescope.UTCDate = utc;
                    }
                }
            }
            catch (Exception ex)
            {
                RaiseOnMessageShow("$Ascom.Messages.UnableSetDateTime");
                Log.Error($"Unable to set date and time: {ex}");
            }
        }

        /// <inheritdoc/>
        public void SetLocation(CrdsGeographical geo)
        {
            try
            {
                lock (locker)
                {
                    if (telescope != null)
                    {
                        telescope.SiteLongitude = geo.Longitude;
                        telescope.SiteLatitude = geo.Latitude;
                        telescope.SiteElevation = geo.Elevation;                        
                    }
                }
            }
            catch (Exception ex)
            {
                RaiseOnMessageShow("$Ascom.Messages.UnableSetLocation");
                Log.Error($"Unable to set telescope observation place: {ex}");
            }
        }

        /// <inheritdoc/>
        public bool IsConnected
        {
            get
            {
                try
                {
                    lock (locker)
                    {
                        return (telescope != null && telescope.Connected);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to get telescope connection state: {ex}");
                    return false;
                }
            }
        }

        /// <inheritdoc/>
        public bool IsSlewing
        {
            get
            {
                try
                {
                    lock (locker)
                    {
                        return (telescope != null && telescope.Connected && telescope.Slewing);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to get slewing state of telescope: {ex}");
                    return false;
                }
            }
        }

        /// <inheritdoc/>
        public bool AtHome
        {
            get
            {
                lock (locker)
                {
                    return (telescope != null && telescope.Connected && telescope.AtHome);
                }
            }
        }

        /// <inheritdoc/>
        public bool AtPark
        {
            get
            {
                lock (locker)
                {
                    return (telescope != null && telescope.Connected && telescope.AtPark);
                }
            }
        }

        /// <inheritdoc/>
        public bool IsTracking
        {
            get
            {
                lock (locker)
                {
                    return (telescope != null && telescope.Connected && telescope.Tracking);
                }
            }
        }

        /// <inheritdoc/>
        public string TelescopeName
        {
            get
            {
                try
                {
                    lock (locker)
                    { 
                        return telescope?.Name;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to get telescope name: {ex}");
                    return null;
                }
            }
        }

        /// <inheritdoc/>
        public string TelescopeDescription
        {
            get
            {
                try
                {
                    lock (locker)
                    {
                        return telescope?.Description;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Unable to get telescope description: {ex}");
                    return null;
                }
            }
        }

        /// <inheritdoc/>
        public void Slew(CrdsEquatorial eq)
        {
            try
            {
                lock (locker)
                {
                    if (telescope != null && telescope.Connected)
                    {
                        UnparkIfPossible();
                        EnableTrackingIfPossible();

                        var eqT = ConvertCoordinatesToTelescopeEpoch(eq);

                        // Slew
                        if (telescope.CanSlewAsync)
                        {
                            telescope.SlewToCoordinatesAsync(eqT.Alpha / 15, eqT.Delta);
                        }
                        else
                        {
                            Task.Run(() => telescope.SlewToCoordinates(eqT.Alpha / 15, eqT.Delta));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to slew telescope: {ex}");
                RaiseOnMessageShow("$Ascom.Messages.UnableSlew");
            }
        }

        /// <inheritdoc/>
        public void ShowSetupDialog()
        {
            try
            {
                lock (locker)
                {
                    if (telescope != null && telescope.Connected)
                    {
                        telescope.SetupDialog();
                    }
                    else
                    {
                        Log.Error("Unable to show setup dialog (telescope disconnected).");
                    }
                }
            }
            catch (Exception ex)
            {
                RaiseOnMessageShow("$Ascom.Messages.UnableShowSetup");
                Log.Error($"Unable to show setup dialog {ex}");
            }
        }

        /// <inheritdoc/>
        public void Sync(CrdsEquatorial eq)
        {
            try
            {
                lock (locker)
                {
                    if (telescope != null && telescope.Connected)
                    {
                        UnparkIfPossible();
                        EnableTrackingIfPossible();

                        if (telescope.CanSync)
                        {
                            var eqT = ConvertCoordinatesToTelescopeEpoch(eq);
                            telescope.SyncToCoordinates(eqT.Alpha / 15, eqT.Delta);
                        }
                        else
                        {
                            RaiseOnMessageShow("$Ascom.Messages.SyncNotSupported");
                        }
                    }
                    else
                    {
                        Log.Error("Unable to sync (telescope disconnected).");
                    }
                }
            }
            catch (Exception ex)
            {
                RaiseOnMessageShow("$Ascom.Messages.UnableSync");
                Log.Error($"Unable to sync {ex}");
            }
        }

        /// <inheritdoc/>
        public AscomInfo Info
        {
            get
            {
                lock (locker)
                {
                    if (telescope != null)
                    {
                        return new AscomInfo()
                        {
                            DriverVersion = telescope.DriverVersion,
                            DriverDescription = telescope.DriverInfo,
                            InterfaceVersion = telescope.InterfaceVersion.ToString(),
                            TelescopeDescription = telescope.Description,
                            TelescopeName = telescope.Name,
                            CanFindHome = telescope.CanFindHome,
                            CanSlew = telescope.CanSlew,
                            CanSync = telescope.CanSync,
                            CanSetTracking = telescope.CanSetTracking,
                            CanPark = telescope.CanPark,
                            CanUnpark = telescope.CanUnpark
                        };
                    }
                    else
                    {
                        return new AscomInfo();
                    }
                }
            }
        }

        /// <summary>
        /// Does disconnecting logic 
        /// </summary>
        private void DoDisconnect()
        {
            if (telescope != null)
            {
                telescope.Connected = false;

                if (!telescope.Connected)
                {
                    watcherResetEvent.Reset();
                }
                else
                {
                    Log.Error($"Unable to disconnect.");
                }

                try
                {
                    telescope.Dispose();
                }
                catch { }

                telescope = null;
                Position.Alpha = 0;
                Position.Delta = 0;
                NotifyPropertyChanged(nameof(IsConnected));
                NotifyPropertyChanged(nameof(IsSlewing));
                NotifyPropertyChanged(nameof(AtHome));
                NotifyPropertyChanged(nameof(AtPark));
                NotifyPropertyChanged(nameof(IsTracking));
            }
        }

        /// <summary>
        /// Unparks telescope if it's possible
        /// </summary>
        private void UnparkIfPossible()
        {
            try
            {
                if (telescope.CanSetPark && telescope.AtPark)
                {
                    telescope.Unpark();
                }
                if (!telescope.CanSetPark && telescope.AtPark)
                {
                    RaiseOnMessageShow("$Ascom.Messages.UnparkManully");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to unpark telesope: {ex}");
            }
        }

        /// <summary>
        /// Set tracking ON if it's possible
        /// </summary>
        private void EnableTrackingIfPossible()
        {
            try
            {
                if (telescope.CanSetTracking)
                {
                    telescope.Tracking = true;
                }
                if (!telescope.CanSetTracking)
                {
                    RaiseOnMessageShow("$Ascom.Messages.SwitchTrackingManully");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to set tracking: {ex}");
            }
        }


        /// <summary>
        /// Thread loop to update telescope state
        /// </summary>
        private void Polling()
        {
            while (!isDisposed)
            {
                Thread.Sleep(PollingPeriod);
                watcherResetEvent.WaitOne();

                try
                {
                    CrdsEquatorial eq = new CrdsEquatorial();

                    bool isConnected = false;

                    lock (locker)
                    {
                        if (telescope != null && telescope.Connected)
                        {
                            eq = ConvertCoordinatesFromTelescopeEpoch();
                            isConnected = true;
                        }
                    }

                    if (isConnected)
                    {
                        bool isMoving = false;

                        if (Math.Abs(eq.Alpha - Position.Alpha) > 1e-6 || Math.Abs(eq.Delta - Position.Delta) > 1e-6)
                        {
                            isMoving = true;
                        }

                        Position.Alpha = eq.Alpha;
                        Position.Delta = eq.Delta;

                        if (isMoving)
                        {
                            NotifyPropertyChanged(nameof(Position));
                        }

                        NotifyPropertyChanged(nameof(IsSlewing));
                        NotifyPropertyChanged(nameof(AtHome));
                        NotifyPropertyChanged(nameof(AtPark));
                        NotifyPropertyChanged(nameof(IsTracking));
                    }
                }
                catch { }
            }            
        }

        /// <summary>
        /// Raises OnMessageShow event
        /// </summary>
        /// <param name="message">Message text</param>
        private void RaiseOnMessageShow(string message)
        {
            if (message.StartsWith("$"))
                OnMessageShow?.Invoke(Text.Get(message.Substring(1)));
            else
                OnMessageShow?.Invoke(message);
        }

        /// <summary>
        /// Converts coordinates in current epoch to an epoch used by telescope hardware.
        /// </summary>
        /// <param name="eq">Equatorial coordinates in current epoch (used by sky map)</param>
        /// <returns>Coordinates in epoch used by telescope hardware</returns>
        private CrdsEquatorial ConvertCoordinatesToTelescopeEpoch(CrdsEquatorial eq)
        {
            CrdsEquatorial eqT = new CrdsEquatorial(eq);

            double jd0 = new Date(DateTime.UtcNow).ToJulianEphemerisDay();

            switch (telescope.EquatorialSystem)
            {
                default:
                case EquatorialCoordinateType.equOther:
                case EquatorialCoordinateType.equTopocentric:
                    break;

                case EquatorialCoordinateType.equB1950:
                    eqT = Precession.GetEquatorialCoordinates(eq, Precession.ElementsFK5(jd0, Date.EPOCH_B1950));
                    break;

                case EquatorialCoordinateType.equJ2000:
                    eqT = Precession.GetEquatorialCoordinates(eq, Precession.ElementsFK5(jd0, Date.EPOCH_J2000));
                    break;

                case EquatorialCoordinateType.equJ2050:
                    eqT = Precession.GetEquatorialCoordinates(eq, Precession.ElementsFK5(jd0, Date.EPOCH_J2050));
                    break;
            }

            return eqT;
        }

        /// <summary>
        /// Converts coordinates of telescope from epoch used by hardware to current epoch (used in sky map)
        /// </summary>
        /// <returns>Equatorial coordinates in current epoch</returns>
        private CrdsEquatorial ConvertCoordinatesFromTelescopeEpoch()
        {
            double jd = new Date(DateTime.UtcNow).ToJulianEphemerisDay();

            CrdsEquatorial eq = new CrdsEquatorial(telescope.RightAscension * 15, telescope.Declination);

            switch (telescope.EquatorialSystem)
            {
                default:
                case EquatorialCoordinateType.equOther:
                case EquatorialCoordinateType.equTopocentric:
                    break;

                case EquatorialCoordinateType.equB1950:
                    eq = Precession.GetEquatorialCoordinates(eq, Precession.ElementsFK5(Date.EPOCH_B1950, jd));
                    break;

                case EquatorialCoordinateType.equJ2000:
                    eq = Precession.GetEquatorialCoordinates(eq, Precession.ElementsFK5(Date.EPOCH_J2000, jd));
                    break;

                case EquatorialCoordinateType.equJ2050:
                    eq = Precession.GetEquatorialCoordinates(eq, Precession.ElementsFK5(Date.EPOCH_J2050, jd));
                    break;
            }

            return eq;
        }

        /// <summary>
        /// Helper method to simplify notification logic
        /// </summary>
        /// <param name="property">Name of the property</param>
        private void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}