using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Database.Entities
{
    /// <summary>
    /// Defines a file attached to an observation or a session
    /// </summary>
    public class AttachmentDB : IEntity
    {
        /// <inheritdoc />
        public string Id { get; set; }

        /// <summary>
        /// Relative path to the file (from the database full path)
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// User-friendly title of the file, for example, "Image of the Moon at 2022-06-10 19:00 UTC"
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Text comments for the image. Here can be a desciption of the image, or camera settings: any information you may need.
        /// </summary>
        public string Comments { get; set; }
    }
}
