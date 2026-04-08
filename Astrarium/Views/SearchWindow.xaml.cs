using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Astrarium.Views
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        public SearchWindow()
        {
            InitializeComponent();

            lstResults.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
            txtSearchString.Focus();
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (lstResults.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                if (lstResults.HasItems)
                {
                    var item = lstResults.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;
                    item.IsSelected = true;
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Controls_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (lstResults.Items.Count > 0)
                {
                    DialogResult = true;
                    e.Handled = true;
                    Close();
                }
            }
            else if (e.Key == Key.Up || e.Key == Key.Down)
            {
                e.Handled = true;

                if (lstResults.Items.Count > 1)
                {
                    if (e.Key == Key.Up && lstResults.SelectedIndex > 0)
                    {
                        lstResults.SelectedIndex--;
                    }
                    else if (e.Key == Key.Down && lstResults.SelectedIndex < lstResults.Items.Count - 1)
                    {
                        lstResults.SelectedIndex++;
                    }

                    lstResults.ScrollIntoView(lstResults.SelectedItem);
                }
            }
        }
    }
}
