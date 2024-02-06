using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Astrarium.Views
{
    /// <summary>
    /// Interaction logic for ObjectInfoView.xaml
    /// </summary>
    public partial class ObjectInfoView : UserControl
    {
        public static readonly DependencyProperty LinkCommandProperty =
            DependencyProperty.Register("LinkCommand", typeof(ICommand), typeof(ObjectInfoView));

        public static readonly DependencyProperty UriCommandProperty =
            DependencyProperty.Register("UriCommand", typeof(ICommand), typeof(ObjectInfoView));

        public static readonly DependencyProperty PropertyValueClickedCommandProperty =
            DependencyProperty.Register("PropertyValueClickedCommand", typeof(ICommand), typeof(ObjectInfoView));

        public ICommand LinkCommand
        {
            get { return (ICommand)GetValue(LinkCommandProperty); }
            set { SetValue(LinkCommandProperty, value); }
        }

        public ICommand UriCommand
        {
            get { return (ICommand)GetValue(UriCommandProperty); }
            set { SetValue(UriCommandProperty, value); }
        }

        public ICommand PropertyValueClickedCommand
        {
            get { return (ICommand)GetValue(PropertyValueClickedCommandProperty); }
            set { SetValue(PropertyValueClickedCommandProperty, value); }
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

            var hoverStyle = this.FindResource("HoverPropertyStyle") as Style;
            var cellPadding = new Thickness(4, 0, 8, 2);
            var headerMargin = new Thickness(4, 16, 4, 4);

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
                            var cell = new TextBlock() { Text = h.Text, Margin = headerMargin, FontWeight = FontWeights.Bold, Background = Brushes.Transparent };
                            tblInfo.Children.Add(cell);
                            Grid.SetRow(cell, r);
                            Grid.SetColumn(cell, 0);
                            Grid.SetColumnSpan(cell, 2);
                        }
                        break;

                    case InfoElementProperty p:
                        {
                            formatter = p.Formatter ?? Formatters.GetDefault(p.Caption);

                            var cellCaption = new TextBlock() { Text = p.Caption, Padding = cellPadding };
                            tblInfo.Children.Add(cellCaption);
                            Grid.SetRow(cellCaption, r);
                            Grid.SetColumn(cellCaption, 0);

                            if (p.Value is Date date && date != null && !double.IsInfinity(date.Day) && !double.IsNaN(date.Day))
                            {
                                Hyperlink link = new Hyperlink() { FontFamily = fontFamily, FontSize = fontSize };
                                link.Inlines.Add(formatter.Format(p.Value));
                                link.Click += (s, e) => LinkClicked(date.ToJulianEphemerisDay());
                                var cellValue = new TextBlock(link) { Padding = cellPadding, VerticalAlignment = VerticalAlignment.Center };
                                tblInfo.Children.Add(cellValue);
                                Grid.SetRow(cellValue, r);
                                Grid.SetColumn(cellValue, 1);
                            }
                            else
                            {
                                var cellValue = new TextBlock() { ToolTip = Text.Get("ObjectInfoWindow.CopyValueHint"), Text = formatter.Format(p.Value), FontFamily = fontFamily, FontSize = fontSize, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left, Style = hoverStyle };
                                cellValue.MouseLeftButtonDown += (s, o) => PropertyClicked(p.Value);
                                tblInfo.Children.Add(cellValue);
                                Grid.SetRow(cellValue, r);
                                Grid.SetColumn(cellValue, 1);
                            }
                        }
                        break;

                    case InfoElementLink L:
                        {
                            var cellCaption = new TextBlock() { Text = L.Caption, Padding = cellPadding };
                            tblInfo.Children.Add(cellCaption);
                            Grid.SetRow(cellCaption, r);
                            Grid.SetColumn(cellCaption, 0);
                            Hyperlink link = new Hyperlink() { FontFamily = fontFamily, FontSize = fontSize };
                            link.Inlines.Add(L.UriText);
                            link.Click += (s, e) => UriClicked(L.Uri);
                            link.ToolTip = L.Uri;
                            var cellValue = new TextBlock(link) { Padding = cellPadding, VerticalAlignment = VerticalAlignment.Center };
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
            LinkCommand?.Execute(jd);
        }

        private void UriClicked(Uri uri)
        {
            UriCommand?.Execute(uri);
        }

        private void PropertyClicked(object value)
        {
            PropertyValueClickedCommand?.Execute(value);
        }
    }
}
