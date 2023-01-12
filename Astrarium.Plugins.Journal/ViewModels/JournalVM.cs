using Astrarium.Algorithms;
using Astrarium.Plugins.Journal.Controls;
using Astrarium.Plugins.Journal.Database;
using Astrarium.Plugins.Journal.Types;
using Astrarium.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace Astrarium.Plugins.Journal.ViewModels
{
    public class JournalVM : ViewModelBase
    {
        #region Private fields

        private DateTimeComparer dateComparer = new DateTimeComparer();

        private ISky sky;
        private ISkyMap map;
        private IOALImporter importer;
        private ITargetDetailsFactory targetDetailsFactory;
        private IDatabaseManager dbManager;
        private readonly string rootPath;
        private readonly string imagesPath;

        #endregion Private fields

        #region Commands

        public ICommand CreateSessionCommand { get; private set; }
        public ICommand EditSessionCommand { get; private set; }
        public ICommand DeleteSessionCommand { get; private set; }
        public ICommand CreateObservationCommand { get; private set; }
        public ICommand EditObservationCommand { get; private set; }
        public ICommand DeleteObservationCommand { get; private set; }
        public ICommand ExpandCollapseCommand { get; private set; }
        public ICommand OpenImageCommand { get; private set; }
        public ICommand OpenAttachmentInSystemViewerCommand { get; private set; }
        public ICommand DeleteAttachmentCommand { get; private set; }
        public ICommand OpenAttachmentLocationCommand { get; private set; }
        public ICommand CreateAttachmentCommand { get; private set; }
        public ICommand ShowAttachmentDetailsCommand { get; private set; }
        public ICommand DropAttachmentsCommand { get; private set; }

        public ICommand EditOpticsCommand { get; private set; }
        public ICommand CreateOpticsCommand { get; private set; }
        public ICommand DeleteOpticsCommand { get; private set; }

        public ICommand EditEyepieceCommand { get; private set; }
        public ICommand CreateEyepieceCommand { get; private set; }
        public ICommand DeleteEyepieceCommand { get; private set; }

        public ICommand EditLensCommand { get; private set; }
        public ICommand CreateLensCommand { get; private set; }
        public ICommand DeleteLensCommand { get; private set; }

        public ICommand EditFilterCommand { get; private set; }
        public ICommand CreateFilterCommand { get; private set; }
        public ICommand DeleteFilterCommand { get; private set; }

        public ICommand EditCameraCommand { get; private set; }
        public ICommand CreateCameraCommand { get; private set; }
        public ICommand DeleteCameraCommand { get; private set; }

        public ICommand EditSiteCommand { get; private set; }

        public ICommand GoToCoordinatesCommand { get; private set; }

        #endregion Commands

        public JournalVM(ISky sky, ISkyMap map, ITargetDetailsFactory targetDetailsFactory, IOALImporter importer, IDatabaseManager dbManager)
        {
            this.sky = sky;
            this.map = map;
            this.importer = importer;
            this.targetDetailsFactory = targetDetailsFactory;
            this.dbManager = dbManager;

            this.importer.OnImportBegin += Importer_OnImportBegin;
            this.importer.OnImportEnd += Importer_OnImportCompleted;

            rootPath = JournalPlugin.PluginDataPath;
            imagesPath = JournalPlugin.ImagesDirectoryPath;

            ExpandCollapseCommand = new Command(ExpandCollapse);

            CreateSessionCommand = new Command(CreateSession);
            EditSessionCommand = new Command<Session>(EditSession);
            DeleteSessionCommand = new Command<Session>(DeleteSession);

            CreateObservationCommand = new Command<Session>(CreateObservation);
            EditObservationCommand = new Command<Observation>(EditObservation);
            DeleteObservationCommand = new Command<Observation>(DeleteObservation);

            OpenImageCommand = new Command<Attachment>(OpenImage);
            DeleteAttachmentCommand = new Command<Attachment>(DeleteAttachment);
            OpenAttachmentLocationCommand = new Command<Attachment>(OpenAttachmentLocation);
            OpenAttachmentInSystemViewerCommand = new Command<Attachment>(OpenAttachmentInSystemViewer);
            CreateAttachmentCommand = new Command(CreateAttachment);
            ShowAttachmentDetailsCommand = new Command<Attachment>(ShowAttachmentDetails);
            DropAttachmentsCommand = new Command<string[]>(DropAttachments);

            EditOpticsCommand = new Command<string>(EditOptics);
            CreateOpticsCommand = new Command(CreateOptics);
            DeleteOpticsCommand = new Command<string>(DeleteOptics);

            EditEyepieceCommand = new Command<string>(EditEyepiece);
            CreateEyepieceCommand = new Command(CreateEyepiece);
            DeleteEyepieceCommand = new Command<string>(DeleteEyepiece);

            EditLensCommand = new Command<string>(EditLens);
            CreateLensCommand = new Command(CreateLens);
            DeleteLensCommand = new Command<string>(DeleteLens);

            EditFilterCommand = new Command<string>(EditFilter);
            CreateFilterCommand = new Command(CreateFilter);
            DeleteFilterCommand = new Command<string>(DeleteFilter);

            EditCameraCommand = new Command<string>(EditCamera);
            CreateCameraCommand = new Command(CreateCamera);
            DeleteCameraCommand = new Command<string>(DeleteCamera);

            EditSiteCommand = new Command<string>(EditSite);

            GoToCoordinatesCommand = new Command(GoToCoordinates);

            Task.Run(Load);
        }

        private bool isDisposed = false;

        public override void Dispose()
        {
            base.Dispose();

            sessions = null;
            FilteredSessions = null;

            Sites = null; 
            Optics = null;
            Eyepieces = null;
            Lenses = null;
            Filters = null;
            Cameras = null;

            if (importer != null)
            {
                importer.OnImportBegin -= Importer_OnImportBegin;
                importer.OnImportEnd -= Importer_OnImportCompleted;
            }

            sky = null;
            importer = null;
            targetDetailsFactory = null;
            dbManager = null;

            isDisposed = true;

            GC.Collect();
        }

        private void Importer_OnImportBegin()
        {
            IsLoading = true;
        }

        private void Importer_OnImportCompleted(bool state)
        {
            if (state)
                Task.Run(Load);
            else
                IsLoading = false;
        }

        public bool IsLoading
        {
            get => GetValue<bool>(nameof(IsLoading));
            set => SetValue(nameof(IsLoading), value);
        }

        private ICollection<Session> sessions { get; set; } = new List<Session>();

        public int SessionsCount => sessions.Count;
        public int ObservationsCount => sessions.SelectMany(x => x.Observations).Count();

        public int FilteredSessionsCount => sessions.Where(x => x.IsEnabled).Count();
        public int FilteredObservationsCount => sessions.SelectMany(x => x.Observations).Where(x => x.IsEnabled).Count();

        public string LoggedTime => sessions.Sum(x => (x.End - x.Begin).TotalMinutes).ToString();

        public ICollection<DateTime> SessionDates => sessions.Where(x => x.IsEnabled).Select(x => x.SessionDate).Distinct(dateComparer).ToArray();

        /// <summary>
        /// Binds to date selected in the calendar view
        /// </summary>
        public DateTime CalendarDate
        {
            get => GetValue(nameof(CalendarDate), DateTime.Now);
            set
            {
                if (!dateComparer.Equals(value, CalendarDate))
                {
                    SetValue(nameof(CalendarDate), value);
                    if (SessionDates.Contains(value, dateComparer))
                    {
                        SelectedTreeViewItem = sessions.FirstOrDefault(x => dateComparer.Equals(x.Begin.DateTime, value));
                    }
                }
            }
        }

        /// <summary>
        /// Flag indicating selected journal item is Session
        /// </summary>
        public bool IsSessionSelected => SelectedTreeViewItem is Session;

        /// <summary>
        /// Binds to journal item currently displayed in the window
        /// </summary>
        public JournalEntity SelectedTreeViewItem
        {
            get => GetValue<JournalEntity>(nameof(SelectedTreeViewItem));
            set
            {
                if (SelectedTreeViewItem != value)
                {
                    // unsubscribe from changes
                    if (SelectedTreeViewItem != null)
                    {
                        SelectedTreeViewItem.DatabasePropertyChanged -= dbManager.SaveDatabaseEntityProperty;
                    }

                    // update backing field
                    SetValue(nameof(SelectedTreeViewItem), value);

                    NotifyPropertyChanged(nameof(IsSessionSelected));

                    // load session or observation details
                    LoadJournalItemDetails();

                    // subscribe for changes
                    if (value != null)
                    {
                        SetValue(nameof(CalendarDate), value.SessionDate);
                    }
                }
            }
        }

        public string FilterString
        {
            get => GetValue<string>(nameof(FilterString));
            set
            {
                if (value != FilterString)
                {
                    SetValue(nameof(FilterString), value);
                    FilteredSessions.Refresh();
                    
                    NotifyPropertyChanged(
                        nameof(FilteredSessions),
                        nameof(SessionDates),
                        nameof(SessionsCount),
                        nameof(ObservationsCount),
                        nameof(FilteredSessionsCount),
                        nameof(FilteredObservationsCount),
                        nameof(LoggedTime));
                }
            }
        }

        public ICollectionView FilteredSessions { get; private set; }

        private bool FilterSession(Session session)
        {
            string filterString = FilterString?.ToLowerInvariant().Trim();

            if (string.IsNullOrWhiteSpace(filterString) || session.DateString.ToLower().Replace(" ", "").Contains(filterString.Replace(" ", "")))
            {
                foreach (var obs in session.Observations)
                {
                    obs.IsEnabled = true;
                }
                return true;
            }

            bool fullMatch = false;

            // TODO: more complicated parsing cases

            // word in quotes, for example, "IC 27" should search by exact match
            var match = Regex.Match(filterString, "\"(.+)\"");
            if (match.Success)
            {
                filterString = match.Groups[1].Value.Trim();
                fullMatch = true;
            }

            foreach (var obs in session.Observations)
            {
                if (string.IsNullOrWhiteSpace(filterString))
                {
                    obs.IsEnabled = true;
                }
                else
                {
                    obs.IsEnabled = false;

                    if (fullMatch)
                    {
                        if (obs.ObjectName.ToLowerInvariant().Equals(filterString))
                            obs.IsEnabled = true;

                        if (obs.ObjectNameAliases != null && obs.ObjectNameAliases.Split(',').Any(x => x.Trim().ToLowerInvariant().Equals(filterString)))
                            obs.IsEnabled = true;
                    }
                    else
                    {
                        if (obs.ObjectName.ToLowerInvariant().Contains(filterString))
                            obs.IsEnabled = true;

                        if (obs.ObjectNameAliases != null && obs.ObjectNameAliases.Split(',').Any(x => x.Trim().ToLowerInvariant().Contains(filterString)))
                            obs.IsEnabled = true;
                    }

                    if (obs.ObjectType.Split('.').Any(x => x.Trim().ToLowerInvariant().Equals(filterString)))
                        obs.IsEnabled = true;
                }
            }
            return session.Observations.Any(x => x.IsEnabled);
        }

        public async void Load()
        {
            try
            {
                IsLoading = true;

                sessions = await dbManager?.GetSessions();

                FilteredSessions = CollectionViewSource.GetDefaultView(sessions);
                FilteredSessions.Filter = x => FilterSession(x as Session);

                Sites = await dbManager.GetSites();
                Optics = await dbManager.GetOptics();
                Eyepieces = await dbManager.GetEyepieces();
                Lenses = await dbManager.GetLenses();
                Filters = await dbManager.GetFilters();
                Cameras = await dbManager.GetCameras();

                IsLoading = false;

            }
            catch
            {
                if (isDisposed)
                {
                    Dispose();
                    return;
                }
            }

            NotifyPropertyChanged(
                nameof(sessions),
                nameof(FilteredSessions),
                nameof(SessionDates),
                nameof(SessionsCount),
                nameof(ObservationsCount),
                nameof(FilteredSessionsCount),
                nameof(FilteredObservationsCount),
                nameof(LoggedTime));

        }

        private async void LoadJournalItemDetails()
        {
            if (SelectedTreeViewItem is Session session)
            {
                await dbManager.LoadSession(session);
            }
            else if (SelectedTreeViewItem is Observation observation)
            {
                await dbManager.LoadObservation(observation);
            }
        }

        #region Command handlers

        private void ExpandCollapse()
        {
            if (sessions.Any(x => !x.IsExpanded))
            {
                foreach (var s in sessions)
                {
                    s.IsExpanded = true;
                }
            }
            else
            {
                foreach (var s in sessions)
                {
                    s.IsExpanded = false;
                }
            }
        }

        private async void DeleteObservation(Observation observation)
        {
            if (ViewManager.ShowMessageBox("$Warning", "Do you really want to delete the observation?\r\nThis action can not be undone.\r\nAll related data, including attached files, will be deleted.", MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                var files = await dbManager.GetObservationFiles(observation.Id);

                // delete from DB
                await dbManager.DeleteObservation(observation.Id);

                // delete attachments files
                foreach (var file in files)
                {
                    string fullPath = Path.GetFullPath(Path.Combine(JournalPlugin.PluginDataPath, file));
                    if (File.Exists(fullPath))
                    {
                        Utils.SafeFileDelete(fullPath);
                    }
                }

                observation.Session.Observations.Remove(observation);

                FilteredSessions.Refresh();

                SelectedTreeViewItem = observation.Session;
            }
        }

        private void CreateSession()
        {
            var date = CalendarDate;
            // TODO: not implemented
        }

        private void EditSession(Session session)
        {
            // TODO: not implemented
        }

        private async void DeleteSession(Session session)
        {
            if (ViewManager.ShowMessageBox("$Warning", "Do you really want to delete the session?\r\nThis action can not be undone.\r\nAll related observations, including attached files, will be deleted.", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var files = await dbManager.GetSessionFiles(session.Id);

                // delete from DB
                await dbManager.DeleteSession(session.Id);

                // delete attachments files
                foreach (var file in files)
                {
                    string fullPath = Path.GetFullPath(Path.Combine(JournalPlugin.PluginDataPath, file));
                    if (File.Exists(fullPath))
                    {
                        Utils.SafeFileDelete(fullPath);
                    }
                }

                sessions.Remove(session);
                FilteredSessions.Refresh();

                SelectedTreeViewItem = null;
            }
        }

        private async void CreateObservation(Session session)
        {
            var model = ViewManager.CreateViewModel<ObservationVM>();
            model.Date = session.Begin.Date;
            model.Begin = session.Begin.TimeOfDay;
            model.End = session.End.TimeOfDay;
            if (ViewManager.ShowDialog(model) ?? false)
            {
                double jd = new Date(model.Date.Year, model.Date.Month, model.Date.Day + model.Begin.TotalDays, sky.Context.GeoLocation.UtcOffset).ToJulianEphemerisDay();
                var targetDetails = CreateTargetDetails(jd, model.CelestialBody);
                var observation = await dbManager.CreateObservation(session, model.CelestialBody, targetDetails, model.Date.Date + model.Begin, model.Date.Date + model.End);
                session.Observations.Add(observation);
                SelectedTreeViewItem = observation;
            }
        }

        private async void EditObservation(Observation observation)
        {
            var model = ViewManager.CreateViewModel<ObservationVM>();
            model.Date = observation.Begin.Date;
            model.Begin = observation.Begin.TimeOfDay;
            model.End = observation.End.TimeOfDay;
            model.CelestialBody = await dbManager.GetTarget(observation.TargetId);
            
            if (ViewManager.ShowDialog(model) ?? false)
            {                
                await dbManager.EditObservation(observation, model.CelestialBody, model.Date.Date + model.Begin, model.Date.Date + model.End);
                LoadJournalItemDetails();
            }
        }

        private void OpenAttachmentLocation(Attachment attachment)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", $@"/e,/select,{attachment.FilePath}");
            }
            catch (Exception ex)
            {
                ViewManager.ShowMessageBox("$Error", ex.Message);
            }
        }

        private void CreateAttachment()
        {
            string[] fullPaths = ViewManager.ShowOpenFileDialog("Add attachment", Utils.GetOpenImageDialogFilterString(), multiSelect: true, out int filterIndex);
            if (fullPaths != null)
            {
                foreach (var file in fullPaths)
                {
                    CreateAttachment(file);
                }
            }
        }

        private void CreateAttachment(string fullPath)
        {
            // source file directory
            var directory = Path.GetDirectoryName(fullPath);

            string destinationPath = fullPath;
            
            // TODO: move to Utils

            // copying is needed
            if (!Utils.ArePathsEqual(directory, imagesPath))
            {
                destinationPath = Path.Combine(imagesPath, Path.GetFileName(fullPath));

                // new name is needed (already exists), just add a guid string
                if (File.Exists(destinationPath))
                {
                    destinationPath = Utils.GenerateNewFileName(destinationPath);
                }

                // copy
                File.Copy(fullPath, destinationPath);
            }

            using (var db = new DatabaseContext())
            {
                var attachment = new Database.Entities.AttachmentDB()
                {
                    Id = Guid.NewGuid().ToString(),
                    FilePath = Path.Combine("images", Path.GetFileName(destinationPath))
                };
                db.Attachments.Add(attachment);

                if (SelectedTreeViewItem is Session session)
                {
                    var existing = db.Sessions.Include(x => x.Attachments).First(x => x.Id == session.Id);
                    existing.Attachments.Add(attachment);
                }
                else if (SelectedTreeViewItem is Observation observation)
                {
                    var existing = db.Observations.Include(x => x.Attachments).First(x => x.Id == observation.Id);
                    existing.Attachments.Add(attachment);
                }

                db.SaveChanges();

                LoadJournalItemDetails();
            }
        }

        private void OpenAttachmentInSystemViewer(Attachment attachment)
        {
            try
            {
                System.Diagnostics.Process.Start(attachment.FilePath);
            }
            catch (Exception ex)
            {
                ViewManager.ShowMessageBox("$Error", ex.Message);
            }
        }

        private void OpenImage(Attachment attachment)
        {
            var model = ViewManager.CreateViewModel<AttachmentVM>();
            model.SetAttachment(attachment);
            model.ShowImage();
            ViewManager.ShowDialog(model);
        }

        private void ShowAttachmentDetails(Attachment attachment)
        {
            var model = ViewManager.CreateViewModel<AttachmentVM>();
            model.SetAttachment(attachment);
            model.ShowDetails();
            ViewManager.ShowDialog(model);
        }

        private void DropAttachments(string[] files)
        {
            foreach (var file in files)
            {
                CreateAttachment(file);
            }
        }

        private void DeleteAttachment(Attachment attachment)
        {
            if (ViewManager.ShowMessageBox("Warning", "Do you really want to delete the attachment? This action can not be undone.", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                // TODO: move to DB manager
                using (var db = new DatabaseContext())
                {
                    var existing = db.Attachments.FirstOrDefault(x => x.Id == attachment.Id);
                    db.Attachments.Remove(existing);
                    db.SaveChanges();

                    if (File.Exists(attachment.FilePath))
                    {
                        var filePaths = db.Attachments.Select(x => x.FilePath).ToArray();
                        if (!filePaths.Any(x => Utils.ArePathsEqual(existing.FilePath, x)))
                        {
                            // file is not used by another attachment
                            Utils.SafeFileDelete(attachment.FilePath);
                        }
                    }

                    LoadJournalItemDetails();
                }
            }
        }

        private async void EditOptics(string id)
        {
            var model = ViewManager.CreateViewModel<OpticsVM>();
            model.Optics = await dbManager.GetOptics(id);
            if (ViewManager.ShowDialog(model) ?? false)
            {
                Optics = await dbManager.GetOptics();
                (SelectedTreeViewItem as Observation).TelescopeId = model.Optics.Id;
            }
        }

        private async void CreateOptics()
        {
            var model = ViewManager.CreateViewModel<OpticsVM>();
            model.Optics = new Optics() { Id = Guid.NewGuid().ToString(), Type = "Telescope" };
            if (ViewManager.ShowDialog(model) ?? false)
            {
                Optics = await dbManager.GetOptics();
                (SelectedTreeViewItem as Observation).TelescopeId = model.Optics.Id;
            }
        }

        private async void DeleteOptics(string id)
        {
            if (ViewManager.ShowMessageBox("$Warning", "Do you really want to delete selected optics? This will be deleted from all observations. This action can not be undone.", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await dbManager.DeleteOptics(id);
                SelectedTreeViewItem.DatabasePropertyChanged -= dbManager.SaveDatabaseEntityProperty;
                Optics = await dbManager.GetOptics();
                LoadJournalItemDetails();
                (SelectedTreeViewItem as Observation).TelescopeId = null;
            }
        }

        private async void EditEyepiece(string id)
        {
            var model = ViewManager.CreateViewModel<EyepieceVM>();
            model.Eyepiece = await dbManager.GetEyepiece(id);
            if (ViewManager.ShowDialog(model) ?? false)
            {
                Eyepieces = await dbManager.GetEyepieces();
                (SelectedTreeViewItem as Observation).EyepieceId = model.Eyepiece.Id;
            }
        }

        private async void CreateEyepiece()
        {
            var model = ViewManager.CreateViewModel<EyepieceVM>();
            model.Eyepiece = new Eyepiece() { Id = Guid.NewGuid().ToString() };
            if (ViewManager.ShowDialog(model) ?? false)
            {
                Eyepieces = await dbManager.GetEyepieces();
                (SelectedTreeViewItem as Observation).EyepieceId = model.Eyepiece.Id;
            }
        }

        private async void DeleteEyepiece(string id)
        {
            if (ViewManager.ShowMessageBox("$Warning", "Do you really want to delete selected eyepiece? This will be deleted from all observations. This action can not be undone.", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await dbManager.DeleteEyepiece(id);
                SelectedTreeViewItem.DatabasePropertyChanged -= dbManager.SaveDatabaseEntityProperty;
                Eyepieces = await dbManager.GetEyepieces();
                LoadJournalItemDetails();
                (SelectedTreeViewItem as Observation).EyepieceId = null;
            }
        }

        private async void EditLens(string id)
        {
            var model = ViewManager.CreateViewModel<LensVM>();
            model.Lens = await dbManager.GetLens(id);
            if (ViewManager.ShowDialog(model) ?? false)
            {
                Lenses = await dbManager.GetLenses();
                (SelectedTreeViewItem as Observation).LensId = model.Lens.Id;
            }
        }

        private async void CreateLens()
        {
            var model = ViewManager.CreateViewModel<LensVM>();
            model.Lens = new Lens() { Id = Guid.NewGuid().ToString() };
            if (ViewManager.ShowDialog(model) ?? false)
            {
                Lenses = await dbManager.GetLenses();
                (SelectedTreeViewItem as Observation).LensId = model.Lens.Id;
            }
        }

        private async void DeleteLens(string id)
        {
            if (ViewManager.ShowMessageBox("$Warning", "Do you really want to delete selected lens? This will be deleted from all observations. This action can not be undone.", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await dbManager.DeleteLens(id);
                SelectedTreeViewItem.DatabasePropertyChanged -= dbManager.SaveDatabaseEntityProperty;
                Lenses = await dbManager.GetLenses();
                LoadJournalItemDetails();
                (SelectedTreeViewItem as Observation).LensId = null;
            }
        }

        private async void EditFilter(string id)
        {
            var model = ViewManager.CreateViewModel<FilterVM>();
            model.Filter = await dbManager.GetFilter(id);
            if (ViewManager.ShowDialog(model) ?? false)
            {
                Filters = await dbManager.GetFilters();
                (SelectedTreeViewItem as Observation).FilterId = model.Filter.Id;
            }
        }

        private async void CreateFilter()
        {
            var model = ViewManager.CreateViewModel<FilterVM>();
            model.Filter = new Filter() { Id = Guid.NewGuid().ToString() };
            if (ViewManager.ShowDialog(model) ?? false)
            {
                Filters = await dbManager.GetFilters();
                (SelectedTreeViewItem as Observation).FilterId = model.Filter.Id;
            }
        }

        private async void DeleteFilter(string id)
        {
            if (ViewManager.ShowMessageBox("$Warning", "Do you really want to delete selected filter? This will be deleted from all observations. This action can not be undone.", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await dbManager.DeleteFilter(id);
                SelectedTreeViewItem.DatabasePropertyChanged -= dbManager.SaveDatabaseEntityProperty;
                Filters = await dbManager.GetFilters();
                LoadJournalItemDetails();
                (SelectedTreeViewItem as Observation).FilterId = null;
            }
        }

        private async void EditCamera(string id)
        {
            var model = ViewManager.CreateViewModel<CameraVM>();
            model.Camera = await dbManager.GetCamera(id);
            if (ViewManager.ShowDialog(model) ?? false)
            {
                Cameras = await dbManager.GetCameras();
                (SelectedTreeViewItem as Observation).CameraId = model.Camera.Id;
            }
        }

        private async void CreateCamera()
        {
            var model = ViewManager.CreateViewModel<CameraVM>();
            model.Camera = new Camera() { Id = Guid.NewGuid().ToString() };
            if (ViewManager.ShowDialog(model) ?? false)
            {
                Cameras = await dbManager.GetCameras();
                (SelectedTreeViewItem as Observation).CameraId = model.Camera.Id;
            }
        }

        private async void DeleteCamera(string id)
        {
            if (ViewManager.ShowMessageBox("$Warning", "Do you really want to delete selected camera? This will be deleted from all observations. This action can not be undone.", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await dbManager.DeleteCamera(id);
                SelectedTreeViewItem.DatabasePropertyChanged -= dbManager.SaveDatabaseEntityProperty;
                Cameras = await dbManager.GetCameras();
                LoadJournalItemDetails();
                (SelectedTreeViewItem as Observation).CameraId = null;
            }
        }

        private async void EditSite(string id)
        {
            var site = await dbManager.GetSite(id);
            var location = new CrdsGeographical()
            {
                Elevation = site.Elevation,
                Latitude = site.Latitude,
                Longitude = -site.Longitude,
                LocationName = site.Name,
                UtcOffset = site.Timezone,
                TimeZoneId = ""
            };


            var model = ViewManager.ShowLocationDialog(location);
            if (model != null)
            {
                site.Elevation = location.Elevation;
                site.Latitude = location.Latitude;
                site.Longitude = location.Longitude;
                site.Name = location.LocationName;
                site.Timezone = location.UtcOffset;

                //await dbManager.sa(id)
            };

            //if (ViewManager.ShowDialog(model) ?? false)
            //{
            //    Cameras = await dbManager.GetCameras();
            //    (SelectedTreeViewItem as Observation).CameraId = model.Camera.Id;
            //}
        }

        private async void GoToCoordinates()
        {
            var obs = SelectedTreeViewItem as Observation;
            var details = obs.TargetDetails;

            if (details.RA != null && details.Dec != null)
            {
                double jd = Date.JulianEphemerisDay(new Date(obs.Begin.UtcDateTime));

                if (obs.Session.SiteId != null)
                {
                    var site = await dbManager.GetSite(obs.Session.SiteId);
                    var geo = new CrdsGeographical(-site.Longitude, site.Latitude, site.Timezone, site.Elevation, null, site.Name);

                    if (sky.Context.GeoLocation.DistanceTo(geo) >= 5)
                    {
                        if (ViewManager.ShowMessageBox("$Warning", "The observation's location place differs than selected one. Do you want to change the location and show the target as it seen from it?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            sky.Context.GeoLocation = geo;
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                sky.Context.JulianDay = jd;

                sky.Calculate();

                var body = sky.Search(obs.ObjectType, obs.ObjectCommonName);

                map.SelectedObject = body;

                // calculate horizontal coordinates
                var eq = new CrdsEquatorial(details.RA.Value, details.Dec.Value);

                var hor = eq.ToHorizontal(sky.Context.GeoLocation, sky.Context.SiderealTime);

                map.GoToPoint(hor, 1);
            }           
        }

        private TargetDetails CreateTargetDetails(double jd, CelestialObject body)
        {
            var context = new SkyContext(jd, sky.Context.GeoLocation);
            TargetDetails targetDetails = targetDetailsFactory.BuildTargetDetails(body, context);
            return targetDetails;
        }

        public ICollection Optics
        {
            get => GetValue<ICollection>(nameof(Optics));
            private set => SetValue(nameof(Optics), value);
        }

        public ICollection Eyepieces
        {
            get => GetValue<ICollection>(nameof(Eyepieces));
            private set => SetValue(nameof(Eyepieces), value);
        }

        public ICollection Lenses
        {
            get => GetValue<ICollection>(nameof(Lenses));
            private set => SetValue(nameof(Lenses), value);
        }

        public ICollection Filters
        {
            get => GetValue<ICollection>(nameof(Filters));
            private set => SetValue(nameof(Filters), value);
        }

        public ICollection Cameras
        {
            get => GetValue<ICollection>(nameof(Cameras));
            private set => SetValue(nameof(Cameras), value);
        }

        public ICollection Sites
        {
            get => GetValue<ICollection>(nameof(Sites));
            private set => SetValue(nameof(Sites), value);
        }

        #endregion Command handlers
    }
}
