using Astrarium.Algorithms;
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
                ["Body"] = new JObject
                {
                    ["Type"] = value.Body.Type,
                    ["Name"] = value.Body.CommonName
                },
                ["Location"] = new JObject
                {
                    ["Name"] = value.Location.Name,
                    ["Latitude"] = value.Location.Latitude,
                    ["Longitude"] = value.Location.Longitude,
                    ["UtcOffset"] = value.Location.UtcOffset,
                    ["Elevation"] = value.Location.Elevation
                },
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

            string type = (string)jsonObject["Body"]["Type"];
            string name = (string)jsonObject["Body"]["Name"];

            var body = sky.Search(type, name);

            JObject jsonLocation = jsonObject["Location"] as JObject;
            CrdsGeographical location = new CrdsGeographical()
            {
                Name = (string)jsonLocation["Name"],
                Latitude = (double)jsonLocation["Latitude"],
                Longitude = (double)jsonLocation["Longitude"],
                UtcOffset = (double)jsonLocation["UtcOffset"],
                Elevation = (double)jsonLocation["Elevation"]
            };

            var note = new Note
            {
                Body = body,
                Location = location,
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
                    Log.Error($"Unable to read notes: {ex}");
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
            Task.Run(() => SaveNotes(notes.Value));
        }

        public void RemoveNote(Note note)
        {
            notes.Value.Remove(note);
            Task.Run(() => SaveNotes(notes.Value));
        }

        public void ChangeNote(Note old, Note @new)
        {
            notes.Value.Remove(old);
            notes.Value.Add(@new);
            Task.Run(() => SaveNotes(notes.Value));
        }
    }
}
