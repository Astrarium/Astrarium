using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace Astrarium.Plugins.Notes.ViewModels
{
    public abstract class NotesVM : ViewModelBase
    {
        protected readonly ISky sky;
        protected readonly ISkyMap map;
        protected readonly NotesManager notesManager;

        public Command AddNoteCommand { get; private set; }
        public Command<Note> ViewNoteCommand { get; private set; }
        public Command EditNoteCommand { get; private set; }
        public Command DeleteNoteCommand { get; private set; }
        public Command<Note> SelectDateCommand { get; private set; }

        public double UtcOffset => sky.Context.GeoLocation.UtcOffset;

        private List<Note> notes = new List<Note>();

        public ICollectionView Notes 
        {
            get => GetValue<ICollectionView>(nameof(Notes));
            private set => SetValue(nameof(Notes), value);
        }

        public Note SelectedNote
        {
            get => GetValue<Note>(nameof(SelectedNote));
            set => SetValue(nameof(SelectedNote), value);
        }

        public string FilterString
        {
            get => GetValue<string>(nameof(FilterString), "");
            set 
            { 
                SetValue(nameof(FilterString), value);
                Notes.Refresh();
                
            }
        }

        public abstract bool AllNotes { get; }

        public bool IsEmpty => !notes.Any();

        public NotesVM(ISky sky, ISkyMap map, NotesManager notesManager) 
        {
            this.sky = sky;
            this.map = map;
            this.notesManager = notesManager;

            AddNoteCommand = new Command(AddNote);
            ViewNoteCommand = new Command<Note>(ViewNote);
            EditNoteCommand = new Command(EditNote);
            DeleteNoteCommand = new Command(DeleteNote);
            SelectDateCommand = new Command<Note>(SelectDate);
        }

        protected abstract List<Note> GetNotes();

        protected abstract Note GetNewNote();

        protected void ReloadNotes()
        {
            notes = GetNotes();
            Notes = CollectionViewSource.GetDefaultView(notes);
            Notes.Filter = x => FilterNotes(x as Note);
            NotifyPropertyChanged(nameof(IsEmpty));
        }

        private bool FilterNotes(Note note)
        {
            return                
                (note.Title != null && note.Title.IndexOf(FilterString, StringComparison.CurrentCultureIgnoreCase) != -1) ||
                (note.Description != null && note.Description.IndexOf(FilterString, StringComparison.CurrentCultureIgnoreCase) != -1);
        }

        private void AddNote()
        {
            var vm = ViewManager.CreateViewModel<NoteVM>().WithModel(GetNewNote(), isEdit: true);
            ViewManager.ShowDialog(vm);

            if (vm.HasChanges)
            {
                notesManager.AddNote(vm.GetNote());
                ReloadNotes();
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
            ViewManager.ShowDialog(vm);
            if (vm.HasChanges) 
            {
                notesManager.ChangeNote(note, vm.GetNote());
                ReloadNotes();
            }
        }

        private void DeleteNote()
        {
            if (SelectedNote == null) return;
            var note = SelectedNote;
            if (ViewManager.ShowMessageBox("$Warning", "Do you really want to delete the note?", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes) 
            {
                notesManager.RemoveNote(note);
                ReloadNotes();
            }
        }

        private void SelectDate(Note note)
        {
            if (note.Body != null)
            {
                sky.SetDate(note.Date);

                var body = sky.Search(note.Body.Type, note.Body.CommonName);

                if (body != null)
                {
                    map.GoToObject(body, TimeSpan.FromMilliseconds(1));
                }
            }
        }
    }
}
