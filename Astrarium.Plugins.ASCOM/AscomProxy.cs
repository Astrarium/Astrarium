using ASCOM.DriverAccess;
using ASCOM.DeviceInterface;
using Astrarium.Algorithms;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Astrarium.Types;
using System.Collections.Concurrent;

namespace Astrarium.Plugins.ASCOM
{
    public class AscomProxy : IAscomProxy, INotifyPropertyChanged
    {
        private Telescope telescope = null;
        private Thread watcherThread = null;
        private AxisRates axisRates = null;
        private ConcurrentQueue<ButtonCommand> commands = new ConcurrentQueue<ButtonCommand>();
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
        public Task<string> Connect(string telescopeId)
        {
            return Task.Run(() =>
            {
                try
                {
                    bool needShowMessage = false;
                    string id = null;
                    lock (locker)
                    {
                        id = Telescope.Choose(telescopeId);
                        if (!string.IsNullOrEmpty(id))
                        {
                            DoDisconnect();
                            telescope = new Telescope(id);
                            telescope.Connected = true;
                            IsConnected = true;
                            needShowMessage = true;
                            watcherResetEvent.Set();
                        }
                    }
                    if (needShowMessage)
                    {
                        RaiseOnMessageShow(Text.Get("Ascom.Messages.Connected", ("telescopeName", telescope.Name)));
                    }
                    return id;
                }
                catch (Exception ex)
                {
                    RaiseOnMessageShow("$Ascom.Messages.UnableChoose");
                    Log.Error($"Unable to choose telescope: {ex}");
                    return null;
                }
            });
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
                        UnparkIfRequired();
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
                    UnparkIfRequired();
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
                        if (telescope.AtPark)
                        {
                            UnparkIfRequired();
                        }

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
                }
                RaiseOnMessageShow("$Ascom.Messages.Disconnected");
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

        private bool _IsConnected = false;

        /// <inheritdoc/>
        public bool IsConnected
        {
            get => _IsConnected;
            private set
            {
                if (_IsConnected != value)
                {
                    _IsConnected = value;
                    NotifyPropertyChanged(nameof(IsConnected));
                }
            }
        }

        private bool CanDoCommand => IsConnected && telescope != null;

        private bool _IsSlewing = false;

        /// <inheritdoc/>
        public bool IsSlewing
        {
            get => _IsSlewing;
            private set 
            {
                if (_IsSlewing != value)
                {
                    _IsSlewing = value;
                    NotifyPropertyChanged(nameof(IsSlewing));
                }
            }
        }

        private bool _AtHome = false;

        /// <inheritdoc/>
        public bool AtHome 
        { 
            get => _AtHome; 
            private set
            {
                if (_AtHome != value)
                {
                    _AtHome = value;
                    NotifyPropertyChanged(nameof(AtHome));
                }
            }
        }

        private bool _AtPark = false;

        /// <inheritdoc/>
        public bool AtPark
        {
            get => _AtPark;
            private set
            {
                if (_AtPark != value)
                {
                    _AtPark = value;
                    NotifyPropertyChanged(nameof(AtPark));
                }
            }
        }

        private bool _IsTracking = false;

        /// <inheritdoc/>
        public bool IsTracking 
        {
            get => _IsTracking;
            private set
            {
                if (_IsTracking != value)
                {
                    _IsTracking = value;
                    NotifyPropertyChanged(nameof(IsTracking));
                }
            }
        }

        /// <inheritdoc/>
        public string TelescopeName { get; private set; }

        /// <inheritdoc/>
        public void Slew(CrdsEquatorial eq)
        {
            try
            {
                lock (locker)
                {
                    if (telescope != null && telescope.Connected)
                    {
                        UnparkIfRequired();
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
        public void ProcessCommand(ButtonCommand command)
        {
            commands.Enqueue(command);
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
                        Task.Run(() => telescope.SetupDialog());
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
                        UnparkIfRequired();
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
                IsConnected = false;
                IsSlewing = false;
                AtHome = false;
                AtPark = false;
                IsTracking = false;
            }
        }

        /// <summary>
        /// Unparks telescope if it's required
        /// </summary>
        private void UnparkIfRequired()
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

                    lock (locker)
                    {
                        if (telescope != null && telescope.Connected)
                        {
                            eq = ConvertCoordinatesFromTelescopeEpoch();
                            IsConnected = true;
                        }
                    }

                    if (IsConnected)
                    {
                        if (axisRates == null)
                        {
                            var primary = telescope.AxisRates(TelescopeAxes.axisPrimary)[1];
                            var secondary = telescope.AxisRates(TelescopeAxes.axisSecondary)[1];
                            axisRates = new AxisRates(primary.Minimum, primary.Maximum, secondary.Minimum, secondary.Maximum);
                        }

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

                        IsSlewing = telescope.Slewing;
                        AtHome = telescope.AtHome;
                        AtPark = telescope.AtPark;
                        IsTracking = telescope.Tracking;
                        TelescopeName = telescope.Name;

                        while (commands.TryDequeue(out ButtonCommand command))
                        {
                            HandleCommand(command);
                        }
                    }
                    else
                    {
                        IsSlewing = false;
                        AtHome = false;
                        AtPark = false;
                        IsTracking = false;

                        while (commands.TryDequeue(out ButtonCommand command));
                    }
                }
                catch 
                {
                    IsConnected = false; 
                    IsSlewing = false;
                    AtHome = false;
                    AtPark = false;
                    IsTracking = false;
                }
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

        /// <summary>
        /// Handles command from joystick device
        /// </summary>
        /// <param name="command"></param>
        private void HandleCommand(ButtonCommand command)
        {
            if (command.IsPressed)
            {
                switch (command.Action)
                {
                    case ButtonAction.RotatePrimary: RotatePrimary(); break;
                    case ButtonAction.RotateSecondary: RotateSecondary(); break;
                    case ButtonAction.RotatePrimaryReverse: RotatePrimaryReverse(); break;
                    case ButtonAction.RotateSecondaryReverse: RotateSecondaryReverse(); break;
                    case ButtonAction.SetMaxRotationSpeed: SetMaxRotationSpeed(); break;
                    case ButtonAction.SetMinRotationSpeed: SetMinRotationSpeed(); break;
                    case ButtonAction.IncreaseRotationSpeed: IncreaseRotationSpeed(); break;
                    case ButtonAction.DecreaseRotationSpeed: DecreaseRotationSpeed(); break;
                    case ButtonAction.SwitchTracking: SwitchTracking(); break;
                    case ButtonAction.AbortSlewing: AbortSlewing(); break;
                    default: break;
                }
            }
            else
            {
                switch (command.Action)
                {
                    case ButtonAction.RotatePrimary:
                    case ButtonAction.RotatePrimaryReverse:
                        StopRotatePrimaryAxis();
                        break;
                    case ButtonAction.RotateSecondary:
                    case ButtonAction.RotateSecondaryReverse:
                        StopRotateSecondaryAxis();
                        break;
                    default:
                        break;
                }
            }
        }

        private void RotatePrimary()
        {
            if (CanDoCommand)
            {
                UnparkIfRequired();
                telescope.MoveAxis(TelescopeAxes.axisPrimary, axisRates.Primary);
            }
        }

        private void RotateSecondary()
        {
            if (CanDoCommand)
            {
                UnparkIfRequired();
                telescope.MoveAxis(TelescopeAxes.axisSecondary, axisRates.Secondary);
            }
        }

        private void RotatePrimaryReverse()
        {
            if (CanDoCommand)
            {
                UnparkIfRequired();
                telescope.MoveAxis(TelescopeAxes.axisPrimary, -axisRates.Primary);
            }
        }

        private void RotateSecondaryReverse()
        {
            if (CanDoCommand)
            {
                UnparkIfRequired();
                telescope.MoveAxis(TelescopeAxes.axisSecondary, -axisRates.Secondary);
            }
        }

        private void SetMaxRotationSpeed()
        {
            if (CanDoCommand)
            {
                axisRates.SetMax();
            }
        }

        private void SetMinRotationSpeed()
        {
            if (CanDoCommand)
            {
                axisRates.SetMin();
            }
        }

        private void IncreaseRotationSpeed()
        {
            if (CanDoCommand)
            {
                axisRates.Increase();
            }
        }

        private void DecreaseRotationSpeed()
        {
            if (CanDoCommand)
            {
                axisRates.Decrease();
            }
        }

        private void StopRotatePrimaryAxis()
        {
            if (CanDoCommand && !telescope.AtPark)
            {
                telescope.MoveAxis(TelescopeAxes.axisPrimary, 0);
            }
        }

        private void StopRotateSecondaryAxis()
        {
            if (CanDoCommand && !telescope.AtPark)
            {
                telescope.MoveAxis(TelescopeAxes.axisSecondary, 0);
            }
        }
    }
}