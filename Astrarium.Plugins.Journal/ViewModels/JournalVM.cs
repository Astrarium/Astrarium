using Astrarium.Algorithms;
using Astrarium.Plugins.Journal.Controls;
using Astrarium.Plugins.Journal.Database;
using Astrarium.Plugins.Journal.Types;
using Astrarium.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            databasePath = Path.Combine(rootPath, "Observations.db");
            imagesPath = Path.Combine(rootPath, "images");

            ExpandCollapseCommand = new Command(ExpandCollapse);
            OpenImageCommand = new Command<Attachment>(OpenImage);
            DeleteAttachmentCommand = new Command<Attachment>(DeleteAttachment);
            OpenAttachmentLocationCommand = new Command<Attachment>(OpenAttachmentLocation);
            OpenAttachmentInSystemViewerCommand = new Command<Attachment>(OpenAttachmentInSystemViewer);
            CreateAttachmentCommand = new Command(CreateAttachment);
            ShowAttachmentDetailsCommand = new Command<Attachment>(ShowAttachmentDetails);
            DropAttachmentsCommand = new Command<string[]>(DropAttachments);
        }

        public ObservableCollection<TreeItemSession> AllSessions { get; private set; } = new ObservableCollection<TreeItemSession>();

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
        public bool IsSessionSelected => SelectedTreeViewItem is TreeItemSession;

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
                        SelectedTreeViewItem.DatabasePropertyChanged -= SaveDatabaseEntityProperty;
                    }

                    // update backing field
                    SetValue(nameof(SelectedTreeViewItem), value);

                    NotifyPropertyChanged(nameof(IsSessionSelected));

                    // load session or observation details
                    LoadJournalItemDetails();

                    // subscribe for changes
                    if (value != null)
                    {
                        value.DatabasePropertyChanged += SaveDatabaseEntityProperty;
                        SetValue(nameof(CalendarDate), value.SessionDate);
                    }
                }
            }
        }

        private void SaveDatabaseEntityProperty(object value, Type entityType, string column, object key)
        {
            using (var db = new DatabaseContext())
            {
                var entity = db.Set(entityType).Find(key);
                db.Entry(entity).Property(column).CurrentValue = value;
                db.SaveChanges();
            }
        }

        public async void Load()
        {
            await Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var sessions = db.Sessions
                        .Include(x => x.Observations)
                        .Include(x => x.Observations.Select(o => o.Target))
                        .OrderByDescending(x => x.Begin).ToArray();

                    List<TreeItemSession> allSessions = new List<TreeItemSession>();

                    foreach (var s in sessions)
                    {
                        var item = new TreeItemSession(s.Id)
                        {
                            Begin = s.Begin,
                            End = s.End
                        };

                        var observations = s.Observations.OrderByDescending(x => x.Begin);
                        foreach (var obs in observations)
                        {
                            item.Observations.Add(new TreeItemObservation(obs.Id)
                            {
                                Session = item,
                                Begin = obs.Begin,
                                End = obs.End,
                                ObjectName = obs.Target.Name,
                                ObjectType = obs.Target.Type,
                                ObjectNameAliases = DeserializeAliases(obs.Target.Aliases)
                            });
                        }

                        allSessions.Add(item);
                    }

                    AllSessions = new ObservableCollection<TreeItemSession>(allSessions);
                    NotifyPropertyChanged(nameof(AllSessions), nameof(SessionDates));
                }
            });
        }

        private void LoadJournalItemDetails()
        {
            
            string databaseFolder = Path.GetDirectoryName(databasePath);

            if (SelectedTreeViewItem is TreeItemSession session)
            {
                using (var db = new DatabaseContext())
                {
                    var s = db.Sessions.Include(x => x.Attachments).FirstOrDefault(x => x.Id == session.Id);
                    session.Weather = s.Weather;
                    session.Seeing = s.Seeing;
                    session.FaintestStar = s.FaintestStar != null ? (decimal)s.FaintestStar.Value : 6m;
                    session.FaintestStarSpecified = s.FaintestStar != null;
                    session.SkyQuality = s.SkyQuality != null ? (decimal)s.SkyQuality.Value : 19m;
                    session.SkyQualitySpecified = s.SkyQuality != null;

                    session.Equipment = s.Equipment;
                    session.Comments = s.Comments;
                    session.Attachments = s.Attachments.ToArray().Select(x => new Attachment()
                    {
                        Id = x.Id,
                        FilePath = Path.Combine(databaseFolder, x.FilePath),
                        Title = x.Title,
                        Comments = x.Comments
                    }).ToList();
                }
            }
            else if (SelectedTreeViewItem is TreeItemObservation observation)
            {
                using (var db = new DatabaseContext())
                {
                    var obs = db.Observations
                        .Include(x => x.Target)
                        .Include(x => x.Attachments)
                        .FirstOrDefault(x => x.Id == observation.Id);

                    observation.Findings = obs.Result;
                    observation.Details = DeserializeObservationDetails(obs.Target.Type, obs.Details);
                    observation.TargetDetails = DeserializeTargetDetails(obs.Target.Type, obs.Target.Details);

                    observation.TelescopeId = obs.ScopeId;
                    observation.EyepieceId = obs.EyepieceId;
                    observation.LensId = obs.LensId;
                    observation.FilterId = obs.FilterId;
                    observation.CameraId = obs.ImagerId;

                    observation.Constellation = obs.Target?.Constellation;
                    observation.EquatorialCoordinates = obs.Target?.RightAscension != null && obs.Target?.Declination != null ? new CrdsEquatorial((double)obs.Target?.RightAscension.Value, (double)obs.Target?.Declination.Value) : null;

                    observation.Attachments = obs.Attachments.ToArray().Select(x => new Attachment()
                    {
                        Id = x.Id,
                        FilePath = Path.Combine(databaseFolder, x.FilePath),
                        Title = x.Title,
                        Comments = x.Comments
                    }).ToList();
                }
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

                if (SelectedTreeViewItem is TreeItemSession session)
                {
                    var existing = db.Sessions.Include(x => x.Attachments).First(x => x.Id == session.Id);
                    existing.Attachments.Add(attachment);
                }
                else if (SelectedTreeViewItem is TreeItemObservation observation)
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
            var model = ViewManager.CreateViewModel<AttachmentDetailsVM>();
            model.SetAttachment(attachment);
            model.ShowImage();
            ViewManager.ShowDialog(model);
        }

        private void ShowAttachmentDetails(Attachment attachment)
        {
            var model = ViewManager.CreateViewModel<AttachmentDetailsVM>();
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

        private string DeserializeAliases(string aliases)
        {
            if (string.IsNullOrEmpty(aliases))
                return null;

            string value = string.Join(", ", JsonConvert.DeserializeObject<string[]>(aliases));
            if (string.IsNullOrEmpty(value))
                return null;
            else
                return value;
        }

        private PropertyChangedBase DeserializeObservationDetails(string targetType, string details)
        {
            if (details != null)
            {
                if (targetType == "VarStar" || targetType == "Nova")
                {
                    return JsonConvert.DeserializeObject<VariableStarObservationDetails>(details);
                }
                else if (targetType == "DeepSky.OpenCluster")
                {
                    return JsonConvert.DeserializeObject<OpenClusterObservationDetails>(details);
                }
                else if (targetType == "DeepSky.DoubleStar")
                {
                    return JsonConvert.DeserializeObject<DoubleStarObservationDetails>(details);
                }

                if (targetType.StartsWith("DeepSky"))
                {
                    return JsonConvert.DeserializeObject<DeepSkyObservationDetails>(details);
                }
            }
            return null;
        }

        private object DeserializeTargetDetails(string targetType, string details)
        {
            if (details != null)
            {
                if (targetType == "DeepSky.OpenCluster")
                {
                    return JsonConvert.DeserializeObject<DeepSkyOpenClusterTargetDetails>(details);
                }
                else if (targetType == "DeepSky.GalaxyCluster")
                {
                    return JsonConvert.DeserializeObject<DeepSkyClusterOfGalaxiesTargetDetails>(details);
                }
                else if (targetType == "Asterism")
                {
                    return JsonConvert.DeserializeObject<DeepSkyAsterismTargetDetails>(details);
                }
                //else if (targetType.StartsWith("DeepSky"))
                //{
                //    return JsonConvert.DeserializeObject<DeepSkyTargetDetails>(details);
                //}
            }
            return null;
        }
    }
}
