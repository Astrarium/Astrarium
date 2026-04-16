using Astrarium.Types;
using Astrarium.Types.Themes;
using System.Collections.Generic;

namespace Astrarium.Plugins.Notes.ViewModels
{
    internal class ObjectNotesVM : BaseNotesListVM
    {
        public override bool AllNotes => false;

        private CelestialObject body;

        public Command CloseCommand { get; private set; }

        public ObjectNotesVM(ISky sky, ISkyMap map, NotesManager notesManager) : base(sky, map, notesManager) 
        {
            CloseCommand = new Command(Close);
        }

        protected override List<Note> GetNotes()
        {
            return notesManager.GetNotesForObject(body);
        }

        protected override Note GetNewNote()
        {
            return new Note() { Date = sky.Context.JulianDay, Location = sky.Context.GeoLocation, Body = body, Markdown = true };
        }

        public ObjectNotesVM ForBody(CelestialObject body)
        {
            this.body = body;
            ReloadNotes();
            return this;
        }

        public override object Payload => new
        {
            Body = body.ToString(),
            NotesCount = GetNotes().Count
        };
    }
}
