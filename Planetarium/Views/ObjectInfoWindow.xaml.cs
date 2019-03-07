using ADK.Demo;
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

namespace Planetarium.Views
{
    /// <summary>
    /// Interaction logic for ObjectInfoWindow.xaml
    /// </summary>
    public partial class ObjectInfoWindow : Window
    {
        public double JulianDay { get; private set; }

        public ObjectInfoWindow()
        {
            InitializeComponent();
        }

        public void SetObjectInfo(CelestialObjectInfo info)
        {
            lblSubtitle.Content = info.Subtitle;
            lblSubtitle.Visibility = string.IsNullOrWhiteSpace(info.Subtitle) ? Visibility.Collapsed : Visibility.Visible;
            lblTitle.Content = info.Title;

            var cellPadding = new Thickness(4, 0, 4, 2);
            var headerPadding = new Thickness(4, 16, 4, 4);

            tblInfo.RowDefinitions.Clear();
            tblInfo.Children.Clear();
            int r = 0;
            foreach (var item in info.InfoElements)
            {
                tblInfo.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                TableRow row = new TableRow();
                IEphemFormatter formatter;
                switch (item)
                {
                    case InfoElementHeader h:
                        {
                            var cell = new TextBlock() { Text = h.Text, Padding = headerPadding, FontWeight = FontWeights.Bold };
                            tblInfo.Children.Add(cell);
                            Grid.SetRow(cell, r);
                            Grid.SetColumn(cell, 0);
                            Grid.SetColumnSpan(cell, 2);
                        }
                        break;

                    case InfoElementPropertyLink p when !double.IsNaN(p.JulianDay):
                        {
                            formatter = p.Formatter ?? Formatters.GetDefault(p.Caption);

                            var cellCaption = new TextBlock() { Text = p.Caption, Padding = cellPadding };
                            tblInfo.Children.Add(cellCaption);
                            Grid.SetRow(cellCaption, r);
                            Grid.SetColumn(cellCaption, 0);

                            Hyperlink link = new Hyperlink();
                            link.Inlines.Add(formatter.Format(p.Value));
                            link.Click += (s, e) => LinkClicked(p.JulianDay);
                            var cellValue = new TextBlock(link) { Padding = cellPadding };
                            tblInfo.Children.Add(cellValue);
                            Grid.SetRow(cellValue, r);
                            Grid.SetColumn(cellValue, 1);
                        }
                        break;

                    case InfoElementProperty p:
                        {
                            formatter = p.Formatter ?? Formatters.GetDefault(p.Caption);

                            var cellCaption = new TextBlock() { Text = p.Caption, Padding = cellPadding };
                            tblInfo.Children.Add(cellCaption);
                            Grid.SetRow(cellCaption, r);
                            Grid.SetColumn(cellCaption, 0);

                            var cellValue = new TextBlock() { Text = formatter.Format(p.Value), Padding = cellPadding };
                            tblInfo.Children.Add(cellValue);
                            Grid.SetRow(cellValue, r);
                            Grid.SetColumn(cellValue, 1);
                        }
                        break;

                    default:
                        break;
                }

                r++;
            }

            tblInfo.InvalidateVisual();
        }

        private void LinkClicked(double jd)
        {
            JulianDay = jd;
            DialogResult = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = null;
            Close();
        }

    }
}
