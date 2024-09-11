using Astrarium.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Notes
{
    [Singleton]
    public class NotesManager
    {
        private static readonly string NotesDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "Notes");

        private bool isLoaded = false;

        private Lazy<List<Note>> notes;

        public NotesManager() 
        {
            notes = new Lazy<List<Note>>(LoadNotes, isThreadSafe: true);
        }

        private List<Note> LoadNotes()
        {
            string file = Path.Combine(NotesDir, "Notes.json");
            if (File.Exists(file))
            {
                string json = File.ReadAllText(file);
                return JsonConvert.DeserializeObject<List<Note>>(json);
            }
            else
            {
                var notes = new List<Note>()
                {
                    new Note() { BodyName = "Sun", BodyType = "Sun", Date = new DateTime(2001, 1, 1), Title = "Sun note", Description = "Description"}
                };

                SaveNotes(notes);
                return notes;
            }
        }

        private void SaveNotes(ICollection<Note> notes)
        {
            string file = Path.Combine(NotesDir, "Notes.json");
            Directory.CreateDirectory(NotesDir);
            string json = JsonConvert.SerializeObject(notes, Formatting.Indented);
            File.WriteAllText(file, json);
        }

        public List<Note> GetNotesForObject(CelestialObject body)
        {
            return notes.Value.Where(n => n.BodyType == body.Type && n.BodyName == body.CommonName).ToList();
        }

        public void AddNote(Note note)
        {
            notes.Value.Add(note);
            SaveNotes(notes.Value);
        }
    }
}
