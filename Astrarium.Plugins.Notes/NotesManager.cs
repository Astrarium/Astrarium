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

        private readonly ISky sky;

        public NotesManager(ISky sky) 
        {
            this.sky = sky;
            notes = new Lazy<List<Note>>(LoadNotes, isThreadSafe: true);
        }

        private List<Note> LoadNotes()
        {
            string file = Path.Combine(NotesDir, "Notes.json");
            if (File.Exists(file))
            {
                string json = File.ReadAllText(file);
                try
                {
                    return JsonConvert.DeserializeObject<List<Note>>(json);
                    
                }
                catch (Exception ex)
                {
                    return new List<Note>();
                }
                finally
                {
                    Task.Run(SearchBodies);
                }
            }
            else
            {
                return new List<Note>();
            }
        }

        private void SearchBodies()
        {
            notes.Value.ForEach(n => n.Body = sky.Search(n.BodyType, n.BodyName));
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
            return notes.Value
                .Where(n => n.BodyType == body.Type && n.BodyName == body.CommonName)
                .OrderByDescending(n => n.Date).ToList();
        }

        public List<Note> GetAllNotes()
        {
            return notes.Value
                .OrderByDescending(n => n.Date).ToList();
        }

        public void AddNote(Note note)
        {
            notes.Value.Add(note);
            SaveNotes(notes.Value);
        }

        public void RemoveNote(Note note)
        {
            notes.Value.Remove(note);
            SaveNotes(notes.Value);
        }

        public void ChangeNote(Note old, Note @new)
        {
            notes.Value.Remove(old);
            notes.Value.Add(@new);
            SaveNotes(notes.Value);
        }
    }
}
