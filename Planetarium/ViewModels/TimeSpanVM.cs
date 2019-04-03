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
        /// Gets array of units
        /// </summary>
        public string[] Units { get; private set; } = new string[]
        {
            "Second",
            "Minute",
            "Hour",
            "Day"
        };

        public int Unit { get; set; }

        /// <summary>
        /// Gets Interval
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// Command handler for <see cref="SelectCommand"/>
        /// </summary>
        private void Select()
        {
            Close(true);
        }

        public TimeSpan TimeInterval
        {
            get
            {
                var value = (double)Interval;
                switch (Unit)
                {
                    case 0:
                        return TimeSpan.FromSeconds(value);
                    case 1:
                        return TimeSpan.FromMinutes(value);
                    case 2:
                        return TimeSpan.FromHours(value);
                    default:
                        return TimeSpan.FromDays(value);
                }
            }
            set
            {
                if (value.TotalDays >= 1)
                {
                    Unit = 3;
                    Interval = (int)value.TotalDays;
                }
                else if (value.TotalHours >= 1)
                {
                    Unit = 2;
                    Interval = (int)value.TotalHours;
                }
                else if (value.TotalMinutes >= 1)
                {
                    Unit = 1;
                    Interval = (int)value.TotalMinutes;
                }
                else if (value.TotalSeconds >= 1)
                {
                    Unit = 0;
                    Interval = (int)value.TotalSeconds;
                }
            }
        }

        public TimeSpanVM()
        {
            SelectCommand = new Command(Select);
        }
    }
}
