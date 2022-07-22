using Astrarium.Algorithms;
using Astrarium.Plugins.Journal.Controls;
using Astrarium.Plugins.Journal.Database;
using Astrarium.Plugins.Journal.Types;
using Astrarium.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace Astrarium.Plugins.Journal.ViewModels
{
    public class JournalVM : ViewModelBase
    {
        #region Private fields

        private DateTimeComparer dateComparer = new DateTimeComparer();

        private readonly string rootPath;
        private readonly string databasePath;
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

            Task.Run(Load);
        }

        public ObservableCollection<Session> AllSessions { get; private set; } = new ObservableCollection<Session>();

        public ICollection<DateTime> SessionDates => AllSessions.Select(x => x.SessionDate).Distinct(dateComparer).ToArray();

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
        public DBStoredEntity SelectedTreeViewItem
        {
            get => GetValue<DBStoredEntity>(nameof(SelectedTreeViewItem));
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
                SetValue(nameof(FilterString), value);
                FilteredSessions.Refresh();
                NotifyPropertyChanged(nameof(FilteredSessions));
            }
        }

        public ICollectionView FilteredSessions
        {
            get
            {
                var source = CollectionViewSource.GetDefaultView(AllSessions);

                if (string.IsNullOrWhiteSpace(FilterString))
                {
                    source.Filter = x => true;
                }
                else
                {
                    source.Filter = x => (x as Session).Observations.Any(obs => obs.ObjectName.ToLowerInvariant().Equals(FilterString.ToLowerInvariant()));
                }

                    
                return source;
            }
        }

        public async void Load()
        {
            var sessions = await DatabaseManager.GetSessions();
            AllSessions = new ObservableCollection<Session>(sessions);

            

            NotifyPropertyChanged(nameof(AllSessions), nameof(FilteredSessions), nameof(SessionDates));
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
            model.Begin = session.Begin;
            model.End = session.End;
            if (ViewManager.ShowDialog(model) ?? false)
            {
                var observation = await DatabaseManager.CreateObservation(session, model.CelestialBody, model.Begin, model.End);
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

        #endregion Command handlers
    }
}
