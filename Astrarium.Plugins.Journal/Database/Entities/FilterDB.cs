using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Database.Entities
{
    public class FilterDB : IEntity
    {
        /// <summary>
        /// Empty element (equals to "Not selected")
        /// </summary>
        public static FilterDB Empty = new FilterDB() { Id = null };

        /// <inheritdoc />
        public string Id { get; set; }

        /// <summary>
        /// Filter model
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Filter vendor
        /// </summary>
        public string Vendor { get; set; }

        /// <summary>
        /// Filter type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Filter color
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Wratten number of color filter:
        /// <see href="https://en.wikipedia.org/wiki/Wratten_number"/>
        /// </summary>
        public string Wratten { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Vendor} {Model}".Trim();
        }
    }
}
