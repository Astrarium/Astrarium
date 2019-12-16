using ADK;
using Planetarium.Types;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Planetarium.Views
{
    /// <summary>
    /// Interaction logic for ObjectInfoView.xaml
    /// </summary>
    public partial class ObjectInfoView : UserControl
    {
        public static readonly DependencyProperty CellPaddingProperty =
            DependencyProperty.Register("CellPadding", typeof(Thickness), typeof(ObjectInfoView), new PropertyMetadata(new Thickness(4, 0, 8, 2)));

        public static readonly DependencyProperty HeaderPaddingProperty =
            DependencyProperty.Register("HeaderPadding", typeof(Thickness), typeof(ObjectInfoView), new PropertyMetadata(new Thickness(4, 16, 4, 4)));

        public static readonly DependencyProperty LinkCommandProperty =
                    DependencyProperty.Register("LinkCommand", typeof(ICommand), typeof(ObjectInfoView));

        public Thickness CellPadding
        {
            get { return (Thickness)GetValue(CellPaddingProperty); }
            set { SetValue(CellPaddingProperty, value); }
        }

        public Thickness HeaderPadding
        {
            get { return (Thickness)GetValue(HeaderPaddingProperty); }
            set { SetValue(HeaderPaddingProperty, value); }
        }

        public ICommand LinkCommand
        {
            get { return (ICommand)GetValue(LinkCommandProperty); }
            set { SetValue(LinkCommandProperty, value); }
        }

        public ObjectInfoView()
        {
            InitializeComponent();
            DataContextChanged += ObjectInfoView_DataContextChanged;
        }

        private void ObjectInfoView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ICollection<InfoElement>)
            {
                SetObjectInfo((ICollection<InfoElement>)e.NewValue);
            }
        }

        public void SetObjectInfo(ICollection<InfoElement> info)
        {
            var fontFamily = new FontFamily("Lucida Console");
            var fontSize = 12;

            tblInfo.RowDefinitions.Clear();
            tblInfo.Children.Clear();
            int r = 0;
            foreach (var item in info)
            {
                tblInfo.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                TableRow row = new TableRow();
                IEphemFormatter formatter;
                switch (item)
                {
                    case InfoElementHeader h:
                        {
                            var cell = new TextBlock() { Text = h.Text, Padding = HeaderPadding, FontWeight = FontWeights.Bold };
                            tblInfo.Children.Add(cell);
                            Grid.SetRow(cell, r);
                            Grid.SetColumn(cell, 0);
                            Grid.SetColumnSpan(cell, 2);
                        }
                        break;

                    case InfoElementPropertyLink p when !double.IsNaN(p.JulianDay):
                        {
                            formatter = p.Formatter ?? Formatters.GetDefault(p.Caption);

                            var cellCaption = new TextBlock() { Text = p.Caption, Padding = CellPadding };
                            tblInfo.Children.Add(cellCaption);
                            Grid.SetRow(cellCaption, r);
                            Grid.SetColumn(cellCaption, 0);

                            Hyperlink link = new Hyperlink() { FontFamily = fontFamily, FontSize = fontSize };
                            link.Inlines.Add(formatter.Format(p.Value));
                            link.Click += (s, e) => LinkClicked(p.JulianDay);
                            var cellValue = new TextBlock(link) { Padding = CellPadding, VerticalAlignment = VerticalAlignment.Center };
                            tblInfo.Children.Add(cellValue);
                            Grid.SetRow(cellValue, r);
                            Grid.SetColumn(cellValue, 1);
                        }
                        break;

                    case InfoElementProperty p:
                        {
                            formatter = p.Formatter ?? Formatters.GetDefault(p.Caption);

                            var cellCaption = new TextBlock() { Text = p.Caption, Padding = CellPadding };
                            tblInfo.Children.Add(cellCaption);
                            Grid.SetRow(cellCaption, r);
                            Grid.SetColumn(cellCaption, 0);

                            if (p.Value is Date date && date != null && !double.IsInfinity(date.Day) && !double.IsNaN(date.Day))
                            { 
                                Hyperlink link = new Hyperlink() { FontFamily = fontFamily, FontSize = fontSize };
                                link.Inlines.Add(formatter.Format(p.Value));
                                link.Click += (s, e) => LinkClicked(date.ToJulianEphemerisDay());
                                var cellValue = new TextBlock(link) { Padding = CellPadding, VerticalAlignment = VerticalAlignment.Center };
                                tblInfo.Children.Add(cellValue);
                                Grid.SetRow(cellValue, r);
                                Grid.SetColumn(cellValue, 1);
                            }
                            else
                            {
                                var cellValue = new TextBlock() { Text = formatter.Format(p.Value), Padding = CellPadding, FontFamily = fontFamily, FontSize = fontSize, VerticalAlignment = VerticalAlignment.Center };
                                tblInfo.Children.Add(cellValue);
                                Grid.SetRow(cellValue, r);
                                Grid.SetColumn(cellValue, 1);
                            }
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
            LinkCommand?.Execute(jd);
        }
    }
}
