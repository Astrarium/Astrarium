using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Astrarium.Plugins.ObservationsLog.Controls
{
    [TemplatePart(Name = PartInput, Type = typeof(TextBox))]
    [TemplatePart(Name = PartPlaceholder, Type = typeof(TextBlock))]
    public class TimeEdit : Control
    {
        public const string PartInput = "PART_Input";
        public const string PartPlaceholder = "PART_Placeholder";

        private TextBox _input;
        private TextBlock _placeholder;

        public TimeSpan? Time
        {
            get { return (TimeSpan?)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        //public uint Hours
        //{
        //    get { return (uint)GetValue(HoursProperty); }
        //    set { SetValue(HoursProperty, value); }
        //}

        //public uint Minutes
        //{
        //    get { return (uint)GetValue(MinutesProperty); }
        //    set { SetValue(MinutesProperty, value); }
        //}

        public readonly static DependencyProperty TimeProperty = DependencyProperty.Register(
            nameof(Time),
            typeof(TimeSpan?),
            typeof(TimeEdit),
            new FrameworkPropertyMetadata(null, (o, e) =>
            {
               // e.NewValue
            })
            {
                BindsTwoWayByDefault = true,
                AffectsRender = true
            });

        private MaskedTextProvider Provider;


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _input = Template.FindName(PartInput, this) as TextBox;
            _input.Text = "00:00";
            _input.Loaded += AssociatedObjectLoaded;
            _input.PreviewTextInput += AssociatedObjectPreviewTextInput;
            _input.PreviewKeyDown += AssociatedObjectPreviewKeyDown;
            _input.LostFocus += _input_LostFocus;
            //_input.PreviewLostKeyboardFocus += _input_PreviewLostKeyboardFocus;
            DataObject.AddPastingHandler(_input, Pasting);

            _placeholder = Template.FindName(PartPlaceholder, this) as TextBlock;  
        }


        private void _input_LostFocus(object sender, RoutedEventArgs e)
        {
            
        }

        /*
                Mask Character  Accepts  Required?  
                0  Digit (0-9)  Required  
                9  Digit (0-9) or space  Optional  
                #  Digit (0-9) or space  Required  
                L  Letter (a-z, A-Z)  Required  
                ?  Letter (a-z, A-Z)  Optional  
                &  Any character  Required  
                C  Any character  Optional  
                A  Alphanumeric (0-9, a-z, A-Z)  Required  
                a  Alphanumeric (0-9, a-z, A-Z)  Optional  
                   Space separator  Required 
                .  Decimal separator  Required  
                ,  Group (thousands) separator  Required  
                :  Time separator  Required  
                /  Date separator  Required  
                $  Currency symbol  Required  

                In addition, the following characters have special meaning:

                Mask Character  Meaning  
                <  All subsequent characters are converted to lower case  
                >  All subsequent characters are converted to upper case  
                |  Terminates a previous < or >  
                \  Escape: treat the next character in the mask as literal text rather than a mask symbol  

                */

        private string InputMask = "00:00";
        private char PromptChar = '0';

        void AssociatedObjectLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Provider = new MaskedTextProvider(InputMask, CultureInfo.CurrentCulture);
            this.Provider.Set(_input.Text);
            this.Provider.PromptChar = this.PromptChar;
            _input.Text = Time?.ToString(@"hh\:mm") ?? this.Provider.ToDisplayString();
            _placeholder.Visibility = Time == null ? Visibility.Visible : Visibility.Collapsed;
            _input.Visibility = Time == null ? Visibility.Collapsed : Visibility.Visible;
            //seems the only way that the text is formatted correct, when source is updated
            var textProp = DependencyPropertyDescriptor.FromProperty(TextBox.TextProperty, typeof(TextBox));
            if (textProp != null)
            {
                textProp.AddValueChanged(_input, (s, args) => this.UpdateText());
            }
        }

        void AssociatedObjectPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            this.TreatSelectedText();

            var position = this.GetNextCharacterPosition(_input.SelectionStart);

            if (this.Provider.Replace(e.Text, position))
                position++;

            position = this.GetNextCharacterPosition(position);

            this.RefreshText(position);

            e.Handled = true;
        }

        void AssociatedObjectPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Keyboard.ClearFocus();
            }


            if (e.Key == Key.Space)//handle the space
            {
                this.TreatSelectedText();

                var position = this.GetNextCharacterPosition(_input.SelectionStart);

                if (this.Provider.InsertAt(" ", position))
                    this.RefreshText(position);

                e.Handled = true;
            }

            if (e.Key == Key.Back)//handle the back space
            {
                if (_input.SelectionStart != 0)
                {
                    var position = this.GetNextCharacterPosition(_input.SelectionStart - 1);

                    if (this.Provider.Replace("0", position))
                        position++;

                        this.RefreshText(_input.SelectionStart - 1);
                }
                e.Handled = true;
            }

            if (e.Key == Key.Delete)//handle the delete key
            {
                //treat selected text
                if (this.TreatSelectedText())
                {
                    this.RefreshText(_input.SelectionStart);
                }
                else
                {

                    if (this.Provider.RemoveAt(_input.SelectionStart))
                        this.RefreshText(_input.SelectionStart);
                }

                e.Handled = true;
            }

        }

        /// <summary>
        /// Pasting prüft ob korrekte Daten reingepastet werden
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var pastedText = (string)e.DataObject.GetData(typeof(string));

                this.TreatSelectedText();

                var position = GetNextCharacterPosition(_input.SelectionStart);

                if (this.Provider.InsertAt(pastedText, position))
                {
                    this.RefreshText(position);
                }
            }

            e.CancelCommand();
        }

        private void UpdateText()
        {
            //check Provider.Text + TextBox.Text
            if (this.Provider.ToDisplayString().Equals(_input.Text))
                return;

            //use provider to format
            var success = this.Provider.Set(_input.Text);

            //ui and mvvm/codebehind should be in sync
            this.SetText(success ? this.Provider.ToDisplayString() : _input.Text);
        }

        /// <summary>
        /// Falls eine Textauswahl vorliegt wird diese entsprechend behandelt.
        /// </summary>
        /// <returns>true Textauswahl behandelt wurde, ansonsten falls </returns>
        private bool TreatSelectedText()
        {
            if (_input.SelectionLength > 0)
            {
                return this.Provider.RemoveAt(_input.SelectionStart,
                                              _input.SelectionStart + _input.SelectionLength - 1);
            }
            return false;
        }

        private void RefreshText(int position)
        {
            if (SetText(this.Provider.ToDisplayString()))
            {
                _input.SelectionStart = position;
            }
        }

        private bool SetText(string text)
        {
            this.Provider.Set(_input.Text);
            if (DateTime.TryParseExact(text, "HH:mm", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out DateTime dt))
            {
                _input.Text = String.IsNullOrWhiteSpace(text) ? String.Empty : text;
                this.Provider.Set(_input.Text);
                this.Time = dt.TimeOfDay;
                return true;
            }
            else
            {
                return false;
            }
        }

        private int GetNextCharacterPosition(int startPosition)
        {
            var position = this.Provider.FindEditPositionFrom(startPosition, true);
            if (position == -1)
                return startPosition;
            else
                return position;
        }
    }
}
