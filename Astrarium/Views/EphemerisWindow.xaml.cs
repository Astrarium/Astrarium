using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for EphemerisWindow.xaml
    /// </summary>
    public partial class EphemerisWindow : Window
    {
        public EphemerisWindow()
        {
            InitializeComponent();
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var column = e.Column as DataGridTextColumn;
            var dataTable = ((sender as DataGrid).ItemsSource as DataView).Table;
            column.Binding = new Binding("[" + e.PropertyName + "]");            
            column.Header = dataTable.Columns[e.PropertyName].Caption;
        }
    }
}
