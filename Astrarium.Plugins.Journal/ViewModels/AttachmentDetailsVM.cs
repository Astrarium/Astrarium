using Astrarium.Plugins.Journal.Database;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.Journal.ViewModels
{
    public class AttachmentDetailsVM : ViewModelBase
    {
        private Attachment attachment;

        public ICommand ShowDetailsCommand { get; private set; }

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
                Save();
                NotifyPropertyChanged(nameof(Title));
            }
        }

        public string Comments
        {
            get => attachment.Comments;
            set
            {
                attachment.Comments = value;
                Save();
                NotifyPropertyChanged(nameof(Comments));
            }
        }

        private void Save()
        {
            using (var ctx = new DatabaseContext())
            {
                var a = ctx.Attachments.FirstOrDefault(x => x.Id == attachment.Id);
                a.Title = Title;
                a.Comments = Comments;
                ctx.SaveChanges();
            }
        }
    }
}
