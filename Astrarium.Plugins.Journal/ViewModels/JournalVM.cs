using Astrarium.Algorithms;
using Astrarium.Plugins.Journal.Controls;
using Astrarium.Plugins.Journal.Database;
using Astrarium.Plugins.Journal.Types;
using Astrarium.Types;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Astrarium.Plugins.Journal.ViewModels
{
    public class JournalVM : ViewModelBase
    {
        #region Private fields

        private DateTimeComparer dateComparer = new DateTimeComparer();

        private readonly string rootPath;
        private readonly string imagesPath;

        #endregion Private fields

        #region Commands

        public ICommand CreateObservationCommand { get; set; }
        public ICommand DeleteObservationCommand { get; set; }

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

        #endregion Commands

        public JournalVM()
        {
            rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "Observations");
            imagesPath = Path.Combine(rootPath, "images");

            ExpandCollapseCommand = new Command(ExpandCollapse);

            CreateObservationCommand = new Command<Session>(CreateObservation);
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

            Task.Run(Load);
        }

        public ObservableCollection<Session> AllSessions { get; private set; } = new ObservableCollection<Session>();

        public int SessionsCount => AllSessions.Count;
        public int ObservationsCount => AllSessions.SelectMany(x => x.Observations).Count();

        public int FilteredSessionsCount => AllSessions.Where(x => x.IsEnabled).Count();
        public int FilteredObservationsCount => AllSessions.SelectMany(x => x.Observations).Where(x => x.IsEnabled).Count();

        public string LoggedTime => AllSessions.Sum(x => (x.End - x.Begin).TotalMinutes).ToString();

        public ICollection<DateTime> SessionDates => AllSessions.Where(x => x.IsEnabled).Select(x => x.SessionDate).Distinct(dateComparer).ToArray();

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
                        SelectedTreeViewItem = AllSessions.FirstOrDefault(x => dateComparer.Equals(x.Begin, value));
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
                        SelectedTreeViewItem.DatabasePropertyChanged -= DatabaseManager.SaveDatabaseEntityProperty;
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
                        nameof(AllSessions),
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
            foreach (var obs in session.Observations)
            {
                if (string.IsNullOrWhiteSpace(FilterString))
                {
                    obs.IsEnabled = true;
                }
                else
                {
                    string filterString = FilterString.ToLowerInvariant().Trim();
                    obs.IsEnabled = false;

                    if (obs.ObjectName.ToLowerInvariant().Contains(filterString))
                        obs.IsEnabled = true;

                    if (obs.ObjectNameAliases != null && obs.ObjectNameAliases.Split(',').Any(x => x.Trim().ToLowerInvariant().Contains(filterString)))
                        obs.IsEnabled = true;

                    if (obs.ObjectType.Split('.').Any(x => x.Trim().ToLowerInvariant().Equals(filterString)))
                        obs.IsEnabled = true;

                    if (obs.Begin.Year.ToString().Equals(filterString))
                        obs.IsEnabled = true;

                    if ($"{obs.Begin.Month:00}.{obs.Begin.Year}".Equals(filterString))
                        obs.IsEnabled = true;

                    if ($"{obs.Begin.Year}".Equals(filterString))
                        obs.IsEnabled = true;
                }
            }
            return session.Observations.Any(x => x.IsEnabled);
        }

        public async void Load()
        {
            var sessions = await DatabaseManager.GetSessions();
            AllSessions = new ObservableCollection<Session>(sessions);

            FilteredSessions = CollectionViewSource.GetDefaultView(AllSessions);
            FilteredSessions.Filter = x => FilterSession(x as Session);

            Sites = await DatabaseManager.GetSites();
            Optics = await DatabaseManager.GetOptics();
            Eyepieces = await DatabaseManager.GetEyepieces();

            NotifyPropertyChanged(
                nameof(AllSessions), 
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
                await DatabaseManager.LoadSession(session);
            }
            else if (SelectedTreeViewItem is Observation observation)
            {
                await DatabaseManager.LoadObservation(observation);
            }
        }

        #region Command handlers

        private void ExpandCollapse()
        {
            if (AllSessions.Any(x => !x.IsExpanded))
            {
                foreach (var s in AllSessions)
                {
                    s.IsExpanded = true;
                }
            }
            else
            {
                foreach (var s in AllSessions)
                {
                    s.IsExpanded = false;
                }
            }
        }

        private async void DeleteObservation(Observation observation)
        {
            if (ViewManager.ShowMessageBox("$Warning", "Do you really want to delete the observation?", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                observation.Session.Observations.Remove(observation);
                await DatabaseManager.DeleteObservation(observation.Id);
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
                var observation = await DatabaseManager.CreateObservation(session, model.CelestialBody, model.Date.Date + model.Begin, model.Date.Date + model.End);
                session.Observations.Add(observation);
                SelectedTreeViewItem = observation;
            }
        }

        private void OpenAttachmentLocation(Attachment attachment)
        {
            string path = Path.Combine(rootPath, attachment.FilePath);
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", $@"/e,/select,{path}");
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
            
            // copying is needed
            if (!Utils.ArePathsEqual(directory, imagesPath))
            {
                destinationPath = Path.Combine(imagesPath, Path.GetFileName(fullPath));

                // new name is needed (already exists), just add a guid string
                if (File.Exists(destinationPath))
                {
                    destinationPath = Path.Combine(Path.GetDirectoryName(destinationPath), $"{Path.GetFileNameWithoutExtension(destinationPath)}-{Guid.NewGuid()}{Path.GetExtension(destinationPath)}");
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
                            try
                            {
                                File.Delete(attachment.FilePath);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        else
                        {
                            // file is used
                        }
                    }

                    LoadJournalItemDetails();
                }
            }
        }

        private async void EditOptics(string id)
        {
            var model = ViewManager.CreateViewModel<OpticsVM>();
            model.Optics = await DatabaseManager.GetOptics(id);
            if (ViewManager.ShowDialog(model) ?? false)
            {
                Optics = await DatabaseManager.GetOptics();
                (SelectedTreeViewItem as Observation).TelescopeId = model.Optics.Id;
            }
        }

        private async void CreateOptics()
        {
            var model = ViewManager.CreateViewModel<OpticsVM>();
            model.Optics = new Optics() { Id = Guid.NewGuid().ToString(), Type = "Telescope" };
            if (ViewManager.ShowDialog(model) ?? false)
            {
                Optics = await DatabaseManager.GetOptics();
                (SelectedTreeViewItem as Observation).TelescopeId = model.Optics.Id;
            }
        }

        private async void DeleteOptics(string id)
        {
            if (ViewManager.ShowMessageBox("$Warning", "Do you really want to delete selected optics? This will be deleted from all observations. This action can not be undone.", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await DatabaseManager.DeleteOptics(id);
                SelectedTreeViewItem.DatabasePropertyChanged -= DatabaseManager.SaveDatabaseEntityProperty;
                Optics = await DatabaseManager.GetOptics();
                LoadJournalItemDetails();
                (SelectedTreeViewItem as Observation).TelescopeId = null;
            }
        }

        private async void EditEyepiece(string id)
        {
            var model = ViewManager.CreateViewModel<EyepieceVM>();
            model.Eyepiece = await DatabaseManager.GetEyepiece(id);
            if (ViewManager.ShowDialog(model) ?? false)
            {
                Eyepieces = await DatabaseManager.GetEyepieces();
                (SelectedTreeViewItem as Observation).EyepieceId = model.Eyepiece.Id;
            }
        }

        private async void CreateEyepiece()
        {
            var model = ViewManager.CreateViewModel<EyepieceVM>();
            model.Eyepiece = new Eyepiece() { Id = Guid.NewGuid().ToString() };
            if (ViewManager.ShowDialog(model) ?? false)
            {
                Eyepieces = await DatabaseManager.GetEyepieces();
                (SelectedTreeViewItem as Observation).EyepieceId = model.Eyepiece.Id;
            }
        }

        private async void DeleteEyepiece(string id)
        {
            if (ViewManager.ShowMessageBox("$Warning", "Do you really want to delete selected eyepiece? This will be deleted from all observations. This action can not be undone.", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await DatabaseManager.DeleteEyepiece(id);
                SelectedTreeViewItem.DatabasePropertyChanged -= DatabaseManager.SaveDatabaseEntityProperty;
                Eyepieces = await DatabaseManager.GetEyepieces();
                LoadJournalItemDetails();
                (SelectedTreeViewItem as Observation).EyepieceId = null;
            }
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

        public ICollection Sites
        {
            get => GetValue<ICollection>(nameof(Sites));
            private set => SetValue(nameof(Sites), value);
        }

        #endregion Command handlers
    }
}
