using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
