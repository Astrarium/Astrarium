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

        public void SetAttachment(Attachment attachment)
        {
            this.attachment = attachment;
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
