using Astrarium.Types;
using Astrarium.Types.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Notes.ViewModels
{
    public class BaseNoteVM : ViewModelBase
    {
        protected ISky sky;
        protected ISkyMap map;

        internal BaseNoteVM(ISky sky, ISkyMap map) 
        { 
            this.sky = sky;
            this.map = map;
        }

        protected void SelectDate(Note note)
        {
            if (note.Body != null)
            {
                if (!sky.Context.GeoLocation.Equals(note.Location))
                {
                    if (ViewManager.ShowMessageBox("$Warning", "Change location?", System.Windows.MessageBoxButton.YesNo) != System.Windows.MessageBoxResult.Yes)
                    {
                        return;
                    }

                    sky.SetLocation(note.Location);
                }

                sky.SetDate(note.Date);
                var body = sky.Search(note.Body.Type, note.Body.CommonName);
                if (body != null)
                {
                    map.GoToObject(body, TimeSpan.FromSeconds(1));
                }
            }
        }
    }
}
