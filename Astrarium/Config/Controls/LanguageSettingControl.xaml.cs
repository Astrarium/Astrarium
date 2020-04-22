using Astrarium.Types;
using System.Windows.Controls;

namespace Astrarium.Config.Controls
{
    /// <summary>
    /// Interaction logic for LanguageSettingControl.xaml
    /// </summary>
    public partial class LanguageSettingControl : UserControl
    {
        public LanguageSettingControl()
        {
            InitializeComponent();
            cmbCultures.ItemsSource = Text.GetLocales();
        }
    }
}
