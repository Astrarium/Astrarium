using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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

namespace Astrarium.Plugins.Planner.Views
{
    /// <summary>
    /// Interaction logic for PlanningListWindow.xaml
    /// </summary>
    public partial class PlanningListWindow : Window
    {
        public PlanningListWindow()
        {
            InitializeComponent();
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(Date))
            {
                var column = new DataGridHyperlinkColumn();
                var dataTable = ((sender as DataGrid).ItemsSource as DataView).Table;
                var formatter = dataTable.Columns[e.PropertyName].ExtendedProperties["Formatter"];
                var converter = formatter != null ? new ToStringConverter((IEphemFormatter)formatter) : null;

                
                column.Binding = new Binding("[" + e.PropertyName + "]") { Converter = converter };
                column.SortMemberPath = e.PropertyName;
                column.Header = dataTable.Columns[e.PropertyName].Caption;
                e.Column = column;
            }
            else
            {
                var column = new DataGridTextColumn();
                var dataTable = ((sender as DataGrid).ItemsSource as DataView).Table;
                var formatter = dataTable.Columns[e.PropertyName].ExtendedProperties["Formatter"];
                var converter = formatter != null ? new ToStringConverter((IEphemFormatter)formatter) : null;
                column.Binding = new Binding("[" + e.PropertyName + "]") { Converter = converter };
                column.SortMemberPath = e.PropertyName;
                column.Header = dataTable.Columns[e.PropertyName].Caption;
                e.Column = column;
            }
        }

        class ToStringConverter : IValueConverter
        {
            private readonly IEphemFormatter formatter;

            public ToStringConverter(IEphemFormatter formatter)
            {
                this.formatter = formatter;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return formatter.Format(value);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
