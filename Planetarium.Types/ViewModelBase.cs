using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Types
{
    /// <summary>
    /// Base class for all ViewModels.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Raises when the window or dialog associated with current ViewModel is going to be closed.
        /// </summary>
        public event Action<bool?> Closing;

        /// <summary>
        /// Raised when the ViewModel property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Closes the window associated with current ViewModel.
        /// </summary>
        public virtual void Close()
        {
            Close(null);
        }

        /// <summary>
        /// Sets the dialog result value and closes the dialog associated with the current ViewModel.
        /// </summary>
        /// <param name="dialogResult">Dialog result value to be set.</param>
        public void Close(bool? dialogResult)
        {
            Closing?.Invoke(dialogResult);
        }

        /// <summary>
        /// Notifies subscribers about changing property or properties. 
        /// </summary>
        /// <param name="propertyName">Cahnged property name(s).</param>
        protected void NotifyPropertyChanged(params string[] propertyName)
        {
            foreach (string pn in propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pn));
            }
        }
        
        /// <summary>
        /// Disposes allocated resources
        /// </summary>
        public virtual void Dispose()
        {

        }
    }
}
