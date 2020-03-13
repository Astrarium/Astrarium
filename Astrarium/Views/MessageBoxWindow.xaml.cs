using System;
using System.Collections.Generic;
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
        }
        
        public MessageBoxButton Buttons
        {
            set
            {
                switch (value)
                {
                    case MessageBoxButton.OK:
                        AddButton("OK", MessageBoxResult.OK);
                        break;
                    case MessageBoxButton.OKCancel:
                        AddButton("OK", MessageBoxResult.OK);
                        AddButton("Cancel", MessageBoxResult.Cancel, isCancel: true);
                        break;
                    case MessageBoxButton.YesNo:
                        AddButton("Yes", MessageBoxResult.Yes);
                        AddButton("No", MessageBoxResult.No);
                        break;
                    case MessageBoxButton.YesNoCancel:
                        AddButton("Yes", MessageBoxResult.Yes);
                        AddButton("No", MessageBoxResult.No);
                        AddButton("Cancel", MessageBoxResult.Cancel, isCancel: true);
                        break;
                    default:
                        throw new ArgumentException("Unknown button value", "buttons");
                }
            }
        }

        private void AddButton(string text, MessageBoxResult result, bool isCancel = false)
        {
            var button = new Button() { Content = text, IsCancel = isCancel };
            button.Click += (o, args) => { Result = result; DialogResult = true; };
            ButtonContainer.Children.Add(button);
        }
    }
}
