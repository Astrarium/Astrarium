using Astrarium.Types;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Astrarium.Views
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(NavigationCommands.GoToPage, (s, e) => Navigate((string)e.Parameter)));
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Navigate(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
        private void Navigate(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url));                
            }
            catch (Exception ex)
            {
                Log.Error("Unable to open browser: " + ex);
            }
        }
    }
}
