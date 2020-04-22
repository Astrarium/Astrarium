using System;
using System.Drawing.Printing;
using System.Threading;
using System.Windows;

namespace Astrarium.Types
{
    /// <summary>
    /// Defines methods to work with views (Windows and MessageBoxes) and ViewModels.
    /// </summary>
    public interface IViewManager
    {        
        /// <summary>
        /// Creates new ViewModel by its type.
        /// </summary>
        /// <typeparam name="TViewModel">Type of the ViewModel.</typeparam>
        /// <returns>Instance of ViewModel type <typeparamref name="TViewModel"/>.</returns>
        TViewModel CreateViewModel<TViewModel>() where TViewModel : ViewModelBase;

        /// <summary>
        /// Shows window by its ViewModel type. 
        /// Calling this method automatically creates instance of the ViewModel and attaches it to DataContext property. />
        /// </summary>
        void ShowWindow<TViewModel>() where TViewModel : ViewModelBase;

        /// <summary>
        /// Shows dialog window by its ViewModel type.
        /// Calling this method automatically creates instance of the ViewModel and attaches it to DataContext property. />
        /// </summary>
        /// <returns>Dialog result (true or false) or null, if dialog has been canceled</returns>
        bool? ShowDialog<TViewModel>() where TViewModel : ViewModelBase;

        /// <summary>
        /// Shows window by its ViewModel instance. 
        /// Calling this method automatically attaches passed ViewModel instance to DataContext property.
        /// </summary>
        void ShowWindow<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;

        /// <summary>
        /// Shows dialog window by its ViewModel instance. 
        /// Calling this method automatically attaches passed ViewModel instance to DataContext property.
        /// </summary>
        /// <returns>Dialog result (true or false) or null, if dialog has been canceled</returns>
        bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;

        /// <summary>
        /// Shows message box dialog
        /// </summary>
        /// <param name="caption">Window title</param>
        /// <param name="text">Message text</param>
        /// <param name="buttons">Buttons set</param>
        /// <returns>MessageBoxResult selected by user</returns>
        MessageBoxResult ShowMessageBox(string caption, string text, MessageBoxButton buttons);

        /// <summary>
        /// Shows window with progress bar
        /// </summary>
        /// <param name="caption">Window title</param>
        /// <param name="text">Text displayed above the progress bar</param>
        /// <param name="tokenSource">Cancellation token source instance. Use <see cref="CancellationTokenSource.Cancel()"/> to close the window.</param>
        /// <param name="progress">Progress instance. Can be null for indeterminate progress bar.</param>
        void ShowProgress(string caption, string text, CancellationTokenSource tokenSource, Progress<double> progress = null);

        /// <summary>
        /// Shows save file dialog
        /// </summary>
        /// <param name="caption">Dialog title</param>
        /// <param name="fileName">Default file name</param>
        /// <param name="extension">Default file extension</param>
        /// <param name="filter">Alowed files extensions filter</param>
        /// <returns>File name and path, if user pressed OK, null otherwise.</returns>
        string ShowSaveFileDialog(string caption, string fileName, string extension, string filter);

        /// <summary>
        /// Shows open file dialog
        /// </summary>
        /// <param name="caption">Dialog title</param>
        /// <param name="filter">Alowed files extensions filter</param>
        /// <returns>File name and path, if user pressed OK, null otherwise.</returns>
        string ShowOpenFileDialog(string caption, string filter);

        /// <summary>
        /// Shows folder picker dialog
        /// </summary>
        /// <param name="caption">Dialog title</param>
        /// <returns>Selected folder full path, if user pressed OK, null otherwise.</returns>
        string ShowSelectFolderDialog(string caption, string path);

        /// <summary>
        /// Shows print dialog
        /// </summary>
        /// <returns>Dialog result (true or false)</returns>
        bool ShowPrintDialog(PrintDocument document);

        /// <summary>
        /// Shows print preview dialog
        /// </summary>
        /// <returns>Dialog result (true or false)</returns>
        bool ShowPrintPreviewDialog(PrintDocument document);

        /// <summary>
        /// Shows date and time dialog
        /// </summary>
        /// <param name="jd">Julian day selected by default</param>
        /// <param name="utcOffset">UTC offset, in hours</param>
        /// <param name="displayMode">Dialog options</param>
        /// <returns>Julian day selected, or null</returns>
        double? ShowDateDialog(double jd, double utcOffset, DateOptions displayMode = DateOptions.DateTime);

        /// <summary>
        /// Shows search celestial object window
        /// </summary>
        /// <param name="filter">Predicate function to filder celestial objects</param>
        /// <returns>Celestial object picked by user, or null if no object picked</returns>
        CelestialObject ShowSearchDialog(Func<CelestialObject, bool> filter = null);

        /// <summary>
        /// Shows dialog to select a time span
        /// </summary>
        /// <param name="timeSpan">Time span selected by default</param>
        /// <returns></returns>
        TimeSpan? ShowTimeSpanDialog(TimeSpan timeSpan);
    }
}
