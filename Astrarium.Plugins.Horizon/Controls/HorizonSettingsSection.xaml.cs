using Astrarium.Types.Controls;
using System.Windows.Input;

namespace Astrarium.Plugins.Horizon.Controls
{
    /// <summary>
    /// Interaction logic for HorizonSettingsSection.xaml
    /// </summary>
    public partial class HorizonSettingsSection : SettingsSection
    {
        public HorizonSettingsSection()
        {
            InitializeComponent();
        }

        private void ComboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }
    }
}
