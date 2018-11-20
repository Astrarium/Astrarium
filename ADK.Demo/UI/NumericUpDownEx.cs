using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Windows.Forms
{
    [DesignerCategory("code")]
    public class NumericUpDownEx : NumericUpDown
    {
        public NumericUpDownEx() : base() { }

        private void TryUpdateText(string format)
        {
            this.Text = (_Sign && Value >= 0 ? "+" : "") + (String.Format("{0:" + format + "}", (int)Value));
        }

        protected override void UpdateEditText()
        {
            TryUpdateText(_Format);
        }
        private string _Format = "D2";
        public string Format
        {
            get
            {
                return _Format;
            }
            set
            {
                TryUpdateText(value);
                _Format = value;
            }
        }

        private bool _Sign = false;
        public bool Sign
        {
            get
            {
                return _Sign;
            }
            set
            {
                _Sign = value;
                UpdateEditText();
            }
        }
    }
}
