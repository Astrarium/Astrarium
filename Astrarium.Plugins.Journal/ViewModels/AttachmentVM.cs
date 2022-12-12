using Astrarium.Plugins.Journal.Types;
using Astrarium.Types;
using System.Windows.Input;

namespace Astrarium.Plugins.Journal.ViewModels
{
    public class AttachmentVM : ViewModelBase
    {
        private IDatabaseManager dbManager;
        private Attachment attachment;

        public ICommand ShowDetailsCommand { get; private set; }

        public AttachmentVM(IDatabaseManager dbManager)
        {
            this.dbManager = dbManager;
        }

        public void SetAttachment(Attachment attachment)
        {
            this.attachment = attachment;
            ShowDetailsCommand = new Command(ShowDetails);
        }

        public void ShowImage()
        {
            ActiveTabIndex = 0;
        }

        public void ShowDetails()
        {
            ActiveTabIndex = 1;
        }

        public int ActiveTabIndex
        {
            get => GetValue<int>(nameof(ActiveTabIndex));
            set => SetValue(nameof(ActiveTabIndex), value);
        }

        public string FilePath => attachment.FilePath;

        public string Title
        {
            get => attachment.Title;
            set
            {
                attachment.Title = value;
                SaveAndNotify(nameof(Title));
            }
        }

        public string Comments
        {
            get => attachment.Comments;
            set
            {
                attachment.Comments = value;
                SaveAndNotify(nameof(Comments));
            }
        }

        private async void SaveAndNotify(string propertyName)
        {
            await dbManager.SaveAttachment(attachment);
            NotifyPropertyChanged(propertyName);
        }
    }
}
