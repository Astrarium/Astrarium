using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Database.Entities
{
    public class LensDB : IEntity
    {
        /// <summary>
        /// Empty element (equals to "Not selected")
        /// </summary>
        public static LensDB Empty = new LensDB() { Id = null };

        public string Id { get; set; }
        public string Model { get; set; }
        public string Vendor { get; set; }
        public double Factor { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Vendor} {Model}".Trim();
        }
    }
}
