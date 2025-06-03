using Astrarium.Types;
using Astrarium.Types.Themes;
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

        private readonly ISky sky;
        private readonly NotesManager notesManager;

        public Command AddNoteCommand { get; private set; }
        public Command<Note> ViewNoteCommand { get; private set; }
        public Command EditNoteCommand { get; private set; }
        public Command DeleteNoteCommand { get; private set; }

        public double UtcOffset => sky.Context.GeoLocation.UtcOffset;

        public ObservableCollection<Note> Notes 
        {
            get => GetValue<ObservableCollection<Note>>(nameof(Notes));
            private set => SetValue(nameof(Notes), value);
        }

        public Note SelectedNote
        {
            get => GetValue<Note>(nameof(SelectedNote));
            set => SetValue(nameof(SelectedNote), value);
        }

        public ObjectNotesViewModel(ISky sky, NotesManager notesManager) 
        {
            this.sky = sky;
            this.notesManager = notesManager;

            AddNoteCommand = new Command(AddNote);
            ViewNoteCommand = new Command<Note>(ViewNote);
            EditNoteCommand = new Command(EditNote);
            DeleteNoteCommand = new Command(DeleteNote);
        }

        public void SetObject(CelestialObject body)
        {
            this.body = body;
            Notes = new ObservableCollection<Note>(notesManager.GetNotesForObject(body));
        }

        private void AddNote()
        {
            var vm = ViewManager.CreateViewModel<NoteVM>().WithModel(new Note() { Date = sky.Context.JulianDay, BodyType = body.Type, BodyName = body.CommonName }, isEdit: true);
            if (ViewManager.ShowDialog(vm) == true)
            {
                notesManager.AddNote(vm.GetNote());
                Notes = new ObservableCollection<Note>(notesManager.GetNotesForObject(body));
            }
        }

        private void ViewNote(Note note)
        {
            if (note == null) return;
            OpenNote(note, isEdit: false);
        }

        private void EditNote()
        {
            if (SelectedNote == null) return;
            OpenNote(SelectedNote, isEdit: true);
        }

        private void OpenNote(Note note, bool isEdit)
        {
            var vm = ViewManager.CreateViewModel<NoteVM>().WithModel(note, isEdit);
            if (ViewManager.ShowDialog(vm) == true)
            {
                Notes.Remove(note);
                Notes.Add(vm.GetNote());

                
            }
        }

        private void DeleteNote()
        {
            if (SelectedNote == null) return;

            var note = SelectedNote;

            if (ViewManager.ShowMessageBox("$Warning", "Do you really want to delete the note?", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes) 
            {
                Notes.Remove(note);
                
            }
        }
    }
}
