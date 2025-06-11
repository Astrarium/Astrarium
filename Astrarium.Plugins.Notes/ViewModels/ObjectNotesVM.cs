using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace Astrarium.Plugins.Notes.ViewModels
{
    public class ObjectNotesVM : NotesVM
    {
        private CelestialObject body;

        public ObjectNotesVM(ISky sky, ISkyMap map, NotesManager notesManager) : base(sky, map, notesManager) { }

        public override bool AllNotes => false;

        protected override List<Note> GetNotes()
        {
            return notesManager.GetNotesForObject(body);
        }

        protected override Note GetNewNote()
        {
            return new Note() { Date = sky.Context.JulianDay, Body = body, Markdown = true };
        }

        public ObjectNotesVM ForBody(CelestialObject body)
        {
            this.body = body;
            ReloadNotes();
            return this;
        }

        
    }
}
