using Astrarium.Types;
using System.Drawing;

namespace Astrarium.ViewModels
{
    public class ColorPickerVM : ViewModelBase
    {
        /// <summary>
        /// Called when user selects time span in the dialog.
        /// </summary>
        public Command SelectCommand { get; private set; }

        public Color SelectedColor
        {
            get => GetValue<Color>(nameof(SelectedColor));
            set => SetValue(nameof(SelectedColor), value);
        }

        public int Height
        {
            get => 270;
        }

        public string Title { get; set; }

        /// <summary>
        /// Command handler for <see cref="SelectCommand"/>
        /// </summary>
        private void Select()
        {
            Close(true);
        }

        public ColorPickerVM()
        {
            SelectCommand = new Command(Select);
        }

        public override bool Loggable => false;
    }
}
