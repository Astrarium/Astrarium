using ADK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.ViewModels
{
    /// <summary>
    /// Defines ViewModel for the <see cref="Views.TimeSpanWindow"/> View. 
    /// </summary>
    public class TimeSpanVM : ViewModelBase
    {
        /// <summary>
        /// Called when user selects time span in the dialog.
        /// </summary>
        public Command SelectCommand { get; private set; }

        /// <summary>
        /// Gets or sets days part of TimeSpan
        /// </summary>
        public int Days { get; set; }

        /// <summary>
        /// Gets or sets hours part of TimeSpan
        /// </summary>
        public int Hours { get; set; }

        /// <summary>
        /// Gets or sets minutes part of TimeSpan
        /// </summary>
        public int Minutes { get; set; }

        /// <summary>
        /// Gets or sets seconds part of TimeSpan
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// Gets or sets TimeSpan value
        /// </summary>
        public TimeSpan TimeSpan
        {
            get
            {
                return new TimeSpan(Days, Hours, Minutes, Seconds);
            }
            set
            {
                Days = value.Days;
                Hours = value.Hours;
                Minutes = value.Minutes;
                Seconds = value.Seconds;
            }
        }

        /// <summary>
        /// Command handler for <see cref="SelectCommand"/>
        /// </summary>
        private void Select()
        {
            Close(true);
        }

        public TimeSpanVM()
        {
            SelectCommand = new Command(Select);
        }
    }
}
