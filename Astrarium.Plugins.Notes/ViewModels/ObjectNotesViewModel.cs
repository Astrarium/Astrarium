using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Notes.ViewModels
{
    public class ObjectNotesViewModel : ViewModelBase
    {
        private CelestialObject body;

        private readonly NotesManager notesManager;

        public Command AddNoteCommand { get; private set; }

        public bool IsEditMode
        {
            get => GetValue<bool>(nameof(IsEditMode));
            set => SetValue(nameof(IsEditMode), value);
        }

        public ObservableCollection<Note> Notes 
        {
            get => GetValue<ObservableCollection<Note>>(nameof(Notes));
            private set => SetValue(nameof(Notes), value);
        }

        public ObjectNotesViewModel(NotesManager notesManager) 
        {
            this.notesManager = notesManager;

            AddNoteCommand = new Command(AddNote);
        }

        public void SetObject(CelestialObject body)
        {
            this.body = body;
            Notes = new ObservableCollection<Note>(notesManager.GetNotesForObject(body));
        }

        private void AddNote()
        {
            IsEditMode = true;

            //var note = new Note() { BodyType = body.Type, BodyName = body.CommonName, Date = DateTime.Now, Description = "test " + DateTime.Now, Title = "test " + DateTime.Now };
            //Notes.Add(note);
            //notesManager.AddNote(note);
            //NotifyPropertyChanged(nameof(Notes));
        }
    }
}
