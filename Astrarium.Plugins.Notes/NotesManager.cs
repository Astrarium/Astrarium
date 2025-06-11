using Astrarium.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Notes
{
    public class NoteConverter : JsonConverter<Note>
    {
        private readonly ISky sky;

        public override bool CanRead => true;

        public override bool CanWrite => true;

        public NoteConverter(ISky sky)
        {
            this.sky = sky;
        }

        public override void WriteJson(JsonWriter writer, Note value, JsonSerializer serializer)
        {
            var obj = new JObject
            {
                ["BodyType"] = value.Body?.Type,
                ["BodyName"] = value.Body?.CommonName,
                ["Date"] = value.Date,
                ["Title"] = value.Title,
                ["Markdown"] = value.Markdown,
                ["Description"] = value.Description
            };

            obj.WriteTo(writer);
        }

        public override Note ReadJson(JsonReader reader, Type objectType, Note existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            string type = (string)jsonObject["BodyType"];
            string name = (string)jsonObject["BodyName"];

            var body = sky.Search(type, name);

            var note = new Note
            {
                Body = body,
                Date = (double)jsonObject["Date"],
                Title = (string)jsonObject["Title"],
                Markdown = (bool)jsonObject["Markdown"],
                Description = (string)jsonObject["Description"]
            };

            return note;
        }
    }


    [Singleton]
    public class NotesManager
    {
        private static readonly string NotesDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "Notes");

        private bool isLoaded = false;

        private Lazy<List<Note>> notes;

        private readonly JsonSerializerSettings serializerSettings;

        public NotesManager(ISky sky) 
        {
            this.serializerSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new NoteConverter(sky) }
            };

            this.notes = new Lazy<List<Note>>(LoadNotes, isThreadSafe: true);
        }

        private List<Note> LoadNotes()
        {
            string file = Path.Combine(NotesDir, "Notes.json");
            if (File.Exists(file))
            {
                string json = File.ReadAllText(file);
                try
                {
                    

                    return JsonConvert.DeserializeObject<List<Note>>(json, serializerSettings);
                }
                catch (Exception ex)
                {
                    return new List<Note>();
                }
            }
            else
            {
                return new List<Note>();
            }
        }

        private void SaveNotes(ICollection<Note> notes)
        {
            string file = Path.Combine(NotesDir, "Notes.json");
            Directory.CreateDirectory(NotesDir);
            string json = JsonConvert.SerializeObject(notes, Formatting.Indented, serializerSettings);
            File.WriteAllText(file, json);
        }

        public List<Note> GetNotesForObject(CelestialObject body)
        {

            return notes.Value
                .Where(n => body.Equals(n.Body))
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
