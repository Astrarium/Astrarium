using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObservationPlannerDatabase.Database.Entities
{
    public class FilterDB : IEntity
    {
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
        public string Type { get; set; }
        public string Color { get; set; }

        /// <summary>
        /// Wratten number of color filter:
        /// <see href="https://en.wikipedia.org/wiki/Wratten_number"/>
        /// </summary>
        public string Wratten { get; set; }

        /// <summary>
        /// Schott filter code number
        /// </summary>
        public string Schott { get; set; }
    }
}
