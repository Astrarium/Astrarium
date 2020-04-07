using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Types
{
    public abstract class PropertyChangedBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Raised when the ViewModel property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies subscribers about changing property or properties. 
        /// </summary>
        /// <param name="propertyName">Changed property name(s).</param>
        protected void NotifyPropertyChanged(params string[] propertyName)
        {
            foreach (string pn in propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pn));
            }
        }
    }
}
