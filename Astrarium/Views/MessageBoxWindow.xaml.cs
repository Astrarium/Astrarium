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
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBoxWindow : Window
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

        public MessageBoxWindow()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(NavigationCommands.GoToPage, (s, e) => Navigate((string)e.Parameter)));
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            var button = ButtonContainer.Children.OfType<Button>().FirstOrDefault(b => b.IsDefault);
            button?.Focus();
        }

        public MessageBoxButton Buttons
        {
            set
            {
                switch (value)
                {
                    case MessageBoxButton.OK:
                        AddButton(Text.Get("OK"), MessageBoxResult.OK, isDefault: true);
                        break;
                    case MessageBoxButton.OKCancel:
                        AddButton(Text.Get("OK"), MessageBoxResult.OK);
                        AddButton(Text.Get("Cancel"), MessageBoxResult.Cancel, isDefault: true);
                        break;
                    case MessageBoxButton.YesNo:
                        AddButton(Text.Get("Yes"), MessageBoxResult.Yes);
                        AddButton(Text.Get("No"), MessageBoxResult.No, isDefault: true);
                        break;
                    case MessageBoxButton.YesNoCancel:
                        AddButton(Text.Get("Yes"), MessageBoxResult.Yes);
                        AddButton(Text.Get("No"), MessageBoxResult.No);
                        AddButton(Text.Get("Cancel"), MessageBoxResult.Cancel, isDefault: true);
                        break;
                    default:
                        throw new ArgumentException("Unknown button value", "buttons");
                }
            }
        }

        private void AddButton(string text, MessageBoxResult result, bool isDefault = false)
        {
            var button = new Button() { Content = text, IsCancel = result == MessageBoxResult.Cancel, IsDefault = isDefault };
            button.Click += (o, args) => { Result = result; DialogResult = true; };
            ButtonContainer.Children.Add(button);
        }

        private void Navigate(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url));
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to open browser: " + ex);
            }
        }
    }
}
