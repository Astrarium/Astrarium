using Astrarium.Algorithms;
using Astrarium.Types;
using System;

namespace Astrarium.Plugins.Notes
{
    public class Note : IComparable
    {
        public CelestialObject Body { get; set; }
        public CrdsGeographical Location { get; set; }
        public double Date { get; set; }
        public string Title { get; set; }
        public bool Markdown { get; set; }
        public string Description { get; set; }

        public int CompareTo(object other)
        {
            Note note = other as Note;

            if (note.Body.Type != Body.Type)
                return Body.Type.CompareTo(note.Body.Type);
            else
                return Body.Names[0].CompareTo(note.Body.Names[0]);
        }
    }
}
