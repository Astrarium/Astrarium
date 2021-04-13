using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Threading;
using System.Windows;

namespace Astrarium.Types
{
    public static class ViewManager
    {
        private static IViewManager viewManager;

        public static void SetImplementation(IViewManager viewManager)
        {
            ViewManager.viewManager = viewManager;
        }

        /// <summary>
        /// Creates new ViewModel by its type.
        /// </summary>
        /// <typeparam name="TViewModel">Type of the ViewModel.</typeparam>
        /// <returns>Instance of ViewModel type <typeparamref name="TViewModel"/>.</returns>
        public static TViewModel CreateViewModel<TViewModel>() where TViewModel : ViewModelBase
        {
            return viewManager.CreateViewModel<TViewModel>();
        }

        /// <summary>
        /// Shows window by its ViewModel type. 
        /// Calling this method automatically creates instance of the ViewModel and attaches it to DataContext property. />
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        public static void ShowWindow<TViewModel>(bool isSingleInstance = false) where TViewModel : ViewModelBase
        {
            viewManager.ShowWindow<TViewModel>(isSingleInstance);
        }

        public static bool? ShowDialog<TViewModel>() where TViewModel : ViewModelBase
        {
            return viewManager.ShowDialog<TViewModel>();
        }

        public static void ShowWindow<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
        {
            viewManager.ShowWindow(viewModel);
        }

        public static bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
        {
            return viewManager.ShowDialog(viewModel);
        }

        public static MessageBoxResult ShowMessageBox(string caption, string text, MessageBoxButton buttons)
        {
            return viewManager.ShowMessageBox(caption, text, buttons);
        }

        public static MessageBoxResult ShowMessageBox(string caption, string text)
        {
            return viewManager.ShowMessageBox(caption, text, MessageBoxButton.OK);
        }

        /// <summary>
        /// Shows window with progress bar
        /// </summary>
        /// <param name="caption">Window title</param>
        /// <param name="text">Text displayed above the progress bar</param>
        /// <param name="tokenSource">Cancellation token source instance. Use <see cref="CancellationTokenSource.Cancel()"/> to close the window.</param>
        /// <param name="progress">Progress instance. Can be null for indeterminate progress bar.</param>
        public static void ShowProgress(string caption, string text, CancellationTokenSource tokenSource, Progress<double> progress = null)
        {
            viewManager.ShowProgress(caption, text, tokenSource, progress);
        }

        /// <summary>
        /// Shows file save dialog.
        /// </summary>
        /// <param name="caption">Dialog title</param>
        /// <param name="fileName">Default file name</param>
        /// <param name="extension">Default file extension</param>
        /// <param name="filter">Alowed files extensions filter</param>
        /// <param name="selectedFilterIndex">Selected filter index, as specified in the <paramref name="filter"/> string</param>
        /// <returns>File name and path, if user pressed OK, null otherwise.</returns>
        public static string ShowSaveFileDialog(string caption, string fileName, string extension, string filter, out int selectedFilterIndex)
        {
            return viewManager.ShowSaveFileDialog(caption, fileName, extension, filter, out selectedFilterIndex);
        }

        /// <summary>
        /// Shows open file dialog
        /// </summary>
        /// <param name="caption">Dialog title</param>
        /// <param name="filter">Alowed files extensions filter</param>
        /// <returns>File name and path, if user pressed OK, null otherwise.</returns>
        public static string ShowOpenFileDialog(string caption, string filter)
        {
            return viewManager.ShowOpenFileDialog(caption, filter);
        }

        public static Color? ShowColorDialog(string caption, Color color)
        {
            return viewManager.ShowColorDialog(caption, color);
        }

        public static string ShowSelectFolderDialog(string caption, string path)
        {
            return viewManager.ShowSelectFolderDialog(caption, path);
        }

        public static bool ShowPrintDialog(PrintDocument document)
        {
            return viewManager.ShowPrintDialog(document);
        }

        public static bool ShowPrintPreviewDialog(PrintDocument document)
        {
            return viewManager.ShowPrintPreviewDialog(document);
        }

        /// <summary>
        /// Shows date and time dialog
        /// </summary>
        /// <param name="jd">Julian day selected by default</param>
        /// <param name="utcOffset">UTC offset, in hours</param>
        /// <param name="displayMode">Dialog options</param>
        /// <returns>Julian day selected, or null</returns>
        public static double? ShowDateDialog(double jd, double utcOffset, DateOptions displayMode = DateOptions.DateTime)
        {
            return viewManager.ShowDateDialog(jd, utcOffset, displayMode);
        }

        public static CelestialObject ShowSearchDialog(Func<CelestialObject, bool> filter = null)
        {
            return viewManager.ShowSearchDialog(filter);
        }

        public static TimeSpan? ShowTimeSpanDialog(TimeSpan timeSpan)
        {
            return viewManager.ShowTimeSpanDialog(timeSpan);
        }

        public static void ShowPopupMessage(string message)
        {
            viewManager.ShowPopupMessage(message);
        }
    }
}
