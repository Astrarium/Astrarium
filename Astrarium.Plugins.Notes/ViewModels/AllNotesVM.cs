using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Notes.ViewModels
{
    [Singleton]
    internal class AllNotesVM : BaseNotesListVM
    {
        public override bool AllNotes => true;

        public Command CloseCommand { get; private set; }

        public AllNotesVM(ISky sky, ISkyMap map, NotesManager notesManager) : base(sky, map, notesManager) 
        {
            CloseCommand = new Command(Close);

            Task.Run(ReloadNotes);
        }

        protected override List<Note> GetNotes()
        {
            return notesManager.GetAllNotes();
        }

        protected override Note GetNewNote()
        {
            return new Note() { Date = sky.Context.JulianDay, Location = sky.Context.GeoLocation, Markdown = true };
        }
    }
}
