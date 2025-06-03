using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Notes.ViewModels
{
    public class NoteVM : ViewModelBase
    {
        private readonly ISky sky;
        private Note note = null;
        private bool isEdit = false;

        public Command EditCommand { get; private set; }
        public Command CancelCommand { get; private set; }
        public Command CloseCommand { get; private set; }
        public Command OkCommand { get; private set; }

        public NoteVM(ISky sky) 
        {
            this.sky = sky;
            EditCommand = new Command(Edit);
            CancelCommand = new Command(Cancel);
            CloseCommand = new Command(Close);
            OkCommand = new Command(OK);
        }

        public NoteVM WithModel(Note note, bool isEdit = false)
        {
            this.isEdit = isEdit;
            IsEditMode = isEdit;
            this.note = note;

            Body = sky.Search(note.BodyType, note.BodyName);

            
            SetValues();
            return this;
        }

        public Note GetNote()
        {
            return new Note() { Date = Date, BodyName = Body.CommonName, BodyType = Body.Type, Description = Description, Title = Title };
        }

        public double UtcOffset => sky.Context.GeoLocation.UtcOffset;

        public CelestialObject Body
        {
            get => GetValue<CelestialObject>(nameof(Body));
            set => SetValue(nameof(Body), value);
        }

        public bool IsEditMode
        {
            get => GetValue<bool>(nameof(IsEditMode));
            set => SetValue(nameof(IsEditMode), value);
        }

        public double Date
        {
            get => GetValue<double>(nameof(Date));
            set => SetValue(nameof(Date), value);
        }

        public string Title
        {
            get => GetValue<string>(nameof(Title));
            set => SetValue(nameof(Title), value);
        }

        public string Description
        {
            get => GetValue<string>(nameof(Description));
            set => SetValue(nameof(Description), value);
        }

        private void Edit()
        {
            IsEditMode = true;
        }

        private void Cancel()
        {
            IsEditMode = false;
            SetValues();
            if (isEdit)
            {
                Close(false);
            }
        }

        private void OK()
        {
            IsEditMode = false;
            note = GetNote();
            SetValues();
            Close(true);
        }

        private void SetValues()
        {
            Date = note.Date;
            Description = note.Description;
            Title = note.Title;
        }
    }
}
