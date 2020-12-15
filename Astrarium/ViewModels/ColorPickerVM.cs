using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.ViewModels
{
    public class ColorPickerVM : ViewModelBase
    {
        private readonly ISettings settings;

        /// <summary>
        /// Called when user selects time span in the dialog.
        /// </summary>
        public Command SelectCommand { get; private set; }

        public Color SelectedColor
        {
            get => GetValue<Color>(nameof(SelectedColor));
            set => SetValue(nameof(SelectedColor), value);
        }

        public bool SliderOnly 
        {
            get
            {
                var schema = settings.Get<ColorSchema>("Schema");
                return schema != ColorSchema.Night && schema != ColorSchema.Day;
            }
        }

        public int Height
        {
            get => SliderOnly ? 50 : 270;
        }

        public string Title { get; set; }

        /// <summary>
        /// Command handler for <see cref="SelectCommand"/>
        /// </summary>
        private void Select()
        {
            Close(true);
        }

        public ColorPickerVM(ISettings settings)
        {
            this.settings = settings;
            SelectCommand = new Command(Select);
        }
    }
}
