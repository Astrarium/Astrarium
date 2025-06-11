using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Notes.ViewModels
{
    [Singleton]
    public class AllNotesVM : NotesVM
    {
        public override bool AllNotes => true;

        public AllNotesVM(ISky sky, ISkyMap map, NotesManager notesManager) : base(sky, map, notesManager) 
        {
            
        }

        public override void OnActivated()
        {
            Task.Run(ReloadNotes);
        }

        protected override List<Note> GetNotes()
        {
            return notesManager.GetAllNotes();
        }

        protected override Note GetNewNote()
        {
            return new Note() { Date = sky.Context.JulianDay, Markdown = true };
        }
    }
}
