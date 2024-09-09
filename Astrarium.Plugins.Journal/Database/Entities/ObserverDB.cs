using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Database.Entities
{
    public class ObserverDB : IEntity
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }

        /// <summary>
        /// JSON-serialized observer contacts
        /// </summary>
        public string Contacts { get; set; }

        /// <summary>
        /// Personal offset to the "reference" correlation between the sky quality 
        /// as it can be measured with an SQM and the estimated naked eye limiting magnitude(fst) 
        /// The individual observer's offset depends mainly on the visual acuity of the observer. 
        /// If the fstOffset is known, the sky quality may be derived from faintestStar estimates
        /// by this observer. 
        /// The "reference" correlation used to convert between sky quality and fst was given by 
        /// Bradley Schaefer: fst = 5 * (1.586 - log(10 ^ ((21.568 - BSB) / 5) + 1)) where BSB is the sky quality
        /// (or background surface brightness) given in magnitudes per square arcsecond 
        /// </summary>
        public double? FSTOffset { get; set; }

        /// <summary>
        /// JSON-serialized observer accounts
        /// </summary>
        public string Accounts { get; set; }
    }
}
